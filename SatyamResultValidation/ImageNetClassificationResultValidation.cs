using System;
using System.Collections.Generic;
using SQLTables;
using SatyamResultsSaving;
using Utilities;
using SatyamResultAggregators;
using System.Text;
using AzureBlobStorage;
using SatyamTaskResultClasses;
using SatyamTaskGenerators;
using JobTemplateClasses;
using System.Linq;
using Constants;
using System.Net;
using System.Drawing;
using System.IO;
using SatyamAnalysis;

namespace SatyamResultValidation
{
    


    public class ImageNetClassificationResultValidation
    {
        public enum ConfusingReason
        {
            MultiObjMultiClass = 0,
            SingleObjConfusingClass = 1,
            SingleObjectSingleClass = 2,
            NotAvailable = 3
        };
        public static int typeOfReasons = 4;

        public static Dictionary<string, string> GroundTruth = new Dictionary<string, string>()
        {
            { "n02084071", "dog" },
            { "n02122298", "cat" },
            { "n02701002", "ambulance" },
            { "n02834778", "bicycle" },
            { "n03690473", "LorryTruck" },
            { "n03790512", "motorcycle" },
            //{ "n03930630", "pickup" },
            { "n04285965", "SUV" },
            { "n04520170", "van" },
            { "n09619168", "FemalePerson" },
            { "n09624168", "MalePerson" },
        };

        //public static List<string> GroundTruth_AlphabeticalOrder = new List<string>()
        //{
        //    "bicycle",
        //    "cat",
        //    "dog",
        //    "FemalePerson",
        //    "LorryTruck",
        //    "MalePerson",
        //    "motorcycle",
        //    "SUV",
        //    "van"
        //};

        public static List<string> GroundTruth_AlphabeticalOrder = new List<string>()
        {
            "Van",
            "Pedestrian",
        };



        public static Dictionary<string, ConfusingReason> getConfusingImageList(string filePath)
        {
            Dictionary<string, ConfusingReason> imageList = new Dictionary<string, ConfusingReason>();
            string[] lines = System.IO.File.ReadAllLines(filePath);
            Dictionary<ConfusingReason, int> noImagesPerConfusingReason = new Dictionary<ConfusingReason, int>();
            foreach (string line in lines)
            {
                string[] words = line.Split('\t');
                
                int count = -1;
                foreach (string word in words)
                {
                    if (word == "1")
                    {
                        if (!imageList.ContainsKey(words[0]))
                        {
                            imageList.Add(words[0], (ConfusingReason)count);
                        }
                        
                        if (!noImagesPerConfusingReason.ContainsKey((ConfusingReason)count))
                        {
                            noImagesPerConfusingReason.Add((ConfusingReason)count, 0);
                        }
                        noImagesPerConfusingReason[(ConfusingReason)count]++;
                    }
                    count++;
                }
            }

            for (int i = 0; i < typeOfReasons; i++)
            {
                Console.WriteLine("{0}: {1}", (ConfusingReason)i, noImagesPerConfusingReason[(ConfusingReason)i]);
            }

            return imageList;
        }

        public static bool IsBlackListed(string satyamURI, Dictionary<string, ConfusingReason> imageBlackListReason)
        {
            string fileName = URIUtilities.filenameFromURI(satyamURI);
            string imageCategoryName = fileName.Split('_')[0];
            if (GroundTruth[imageCategoryName] == "ambulance" || imageCategoryName == "ambulance")
            {
                return true;
            }
            if (imageBlackListReason.ContainsKey(fileName))
            {
                if (imageBlackListReason[fileName] == ConfusingReason.NotAvailable)
                {
                    return true;
                }
                if (imageBlackListReason[fileName] == ConfusingReason.MultiObjMultiClass)
                {
                    return true;
                }

                //if (imageBlackListReason[fileName] == ConfusingReason.SingleObjConfusingClass)
                //{
                //    return true;
                //}
            }
            return false;
        }

        public static bool SingleObjectLabelingResultEqualsGroundTruth(string satyamUri, string resultString)
        {
            //string[] uri_parts = satyamUri.Split('/');
            //string[] name_parts = uri_parts[uri_parts.Length - 1].Split('_');
            //string imageName = name_parts[0];
            string fileName = URIUtilities.filenameFromURI(satyamUri);
            string imageCategoryName = fileName.Split('_')[0];
            SingleObjectLabelingResult result = JSonUtils.ConvertJSonToObject<SingleObjectLabelingResult>(resultString);

            return (result.Category.Equals(GroundTruth[imageCategoryName], StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool AggregatedResultEqualsGroundTruth(string satyamUri, string resultString)
        {
            //string[] uri_parts = satyamUri.Split('/');
            //string[] name_parts = uri_parts[uri_parts.Length - 1].Split('_');
            //string imageName = name_parts[0];
            string fileName = URIUtilities.filenameFromURI(satyamUri);
            string imageCategoryName = fileName.Split('_')[0];
            SingleObjectLabelingAggregatedResult result = JSonUtils.ConvertJSonToObject<SingleObjectLabelingAggregatedResult>(resultString);

            return (result.Category.Equals(GroundTruth[imageCategoryName], StringComparison.InvariantCultureIgnoreCase));            
        }

        public static void ValidateSatyamImageNetClassificationAggregationResultByGUID(string guid, string confusingImageListFilePath=null,
            bool prepareDataForTraining = false, string outputDirectory=null)
        {
            Dictionary<string, ConfusingReason> imageBlackListReason = new Dictionary<string, ConfusingReason>();
            if (confusingImageListFilePath!=null)
            {
                imageBlackListReason = getConfusingImageList(confusingImageListFilePath);
            }
             

            //get all aggregated results
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();

            int noTotal = 0;
            int noCorrect = 0;

            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth = new SortedDictionary<string, Dictionary<string, int>>();
            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res = new SortedDictionary<string, Dictionary<string, int>>();

            StringBuilder s = new StringBuilder();
            s.Append("<!DOCTYPE html>\n");
            s.Append("<html>\n");
            s.Append("<body>\n");
            String title = String.Format("<h1>Job GUID {0} Incorrect Result Summary</h1>\n", guid);
            s.Append(title);

            WebClient wc = new WebClient();

            for (int i = 0; i < results.Count; i++)
            {
                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(results[i]);
                //String uri = data.SatyamURI;
                //string[] uri_parts= uri.Split('/');
                //string[] name_parts = uri_parts[uri_parts.Length - 1].Split('_');
                string fileName = URIUtilities.filenameFromURI(data.SatyamURI);
                string imageCategoryName = fileName.Split('_')[0];
                

                String resultString = data.AggregatedResultString;
                SingleObjectLabelingAggregatedResult result = JSonUtils.ConvertJSonToObject<SingleObjectLabelingAggregatedResult>(resultString);

                // skip all ambulances and black listed
                if (IsBlackListed(data.SatyamURI, imageBlackListReason))
                {
                    continue;
                }

                if (!confusionMatrix_res_groundtruth.ContainsKey(result.Category))
                {
                    confusionMatrix_res_groundtruth.Add(result.Category, new Dictionary<string, int>() { { GroundTruth[imageCategoryName] , 0} });
                }
                else
                {
                    if (!confusionMatrix_res_groundtruth[result.Category].ContainsKey(GroundTruth[imageCategoryName]))
                    {
                        confusionMatrix_res_groundtruth[result.Category].Add(GroundTruth[imageCategoryName], 0);
                    }
                }

                if (!confusionMatrix_groundtruth_res.ContainsKey(GroundTruth[imageCategoryName]))
                {
                    confusionMatrix_groundtruth_res.Add(GroundTruth[imageCategoryName], new Dictionary<string, int>() { { result.Category, 0 } });
                }
                else
                {
                    if (!confusionMatrix_groundtruth_res[GroundTruth[imageCategoryName]].ContainsKey(result.Category))
                    {
                        confusionMatrix_groundtruth_res[GroundTruth[imageCategoryName]].Add(result.Category, 0);
                    }
                }


                if (result.Category.Equals(GroundTruth[imageCategoryName], StringComparison.InvariantCultureIgnoreCase))
                {
                    noCorrect++;
                }
                else
                {
                    //Console.WriteLine("{0}, Groundtruth: {1}, Aggregated: {2}, Votes: {3}",
                    //    fileName, GroundTruth[imageCategoryName], result.Category, 
                    //    JSonUtils.ConvertObjectToJSon(result.metaData));

                    String record = String.Format("<p>{0}, Groundtruth: {1}, Aggregated: {2}, Votes: {3}</p>\n",
                        fileName, GroundTruth[imageCategoryName], result.Category,
                        JSonUtils.ConvertObjectToJSon(result.metaData));
                    String img = String.Format("<img src=\"{0}\" >\n", data.SatyamURI);
                    s.Append(record);
                    s.Append(img);
                }

                // prepare training dataset
                if (prepareDataForTraining)
                {
                    if (GroundTruth[imageCategoryName] != "ambulance" && result.Category != "ambulance")
                    {
                        Image im = Image.FromStream(wc.OpenRead(data.SatyamURI));
                        if (!Directory.Exists(outputDirectory + result.Category))
                        {
                            Directory.CreateDirectory(outputDirectory + result.Category);
                        }
                        im.Save(outputDirectory + "\\" + result.Category + "\\" + fileName);
                    }                    
                }

                noTotal++;
                confusionMatrix_res_groundtruth[result.Category][GroundTruth[imageCategoryName]]++;
                confusionMatrix_groundtruth_res[GroundTruth[imageCategoryName]][result.Category]++;
            }
            Console.WriteLine("Result: {0}/{1}, precision: {2}", noCorrect, noTotal, (double)noCorrect / noTotal);

            // write the confusion matrix
            s.Append("<p>");
            String row = "\t\t";
            foreach (string resultCategory in confusionMatrix_res_groundtruth.Keys)
            {
                row += resultCategory + "\t";
            }
            row += "<br>\n";
            s.Append(row);
            Console.WriteLine(row);
            

            foreach(string groundTruthCategory in confusionMatrix_groundtruth_res.Keys)
            {
                row = groundTruthCategory +"\t";
                foreach (string resultCategory in confusionMatrix_res_groundtruth.Keys)
                {
                    if (confusionMatrix_groundtruth_res[groundTruthCategory].ContainsKey(resultCategory))
                    {
                        row += confusionMatrix_groundtruth_res[groundTruthCategory][resultCategory].ToString();
                    }
                    else
                    {
                        row += "0";
                    }
                    row += "\t";
                }
                row += "<br>\n";
                s.Append(row);
                Console.WriteLine(row);
            }

            s.Append("</p>\n");

            s.Append("</body>\n");
            s.Append("</html>\n");
            string dataToBeSaved = s.ToString();

            SatyamJobStorageAccountAccess storage = new SatyamJobStorageAccountAccess();
            string FileName = "AggregatedIncorrectResults-" + guid + ".html";
            
            storage.SaveATextFile("singleobjectlabeling", guid, FileName, dataToBeSaved);



        }




        public static void AggregateWithParameterAndValidateSatyamImageNetClassificationResultByGUID(string guid, 
            int MinResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE, 
            int MaxResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE, 
            double MajorityThreshold = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAJORITY_THRESHOLD, 
            string confusingImageListFilePath = null)
        {
            string configString = "Min_" + MinResults + "_Max_" + MaxResults + "_Thresh_" + MajorityThreshold;
            Console.WriteLine("Aggregating for param set " + configString);
            int noTerminatedTasks = 0;

            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUIDOrderByID(guid);
            resultsDB.close();

            //SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime =
            //    SatyamResultValidation.SortResultsBySubmitTime_OneResultPerTurkerPerTask(entries);
            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime =
                SatyamResultValidationToolKit.SortResultsBySubmitTime(entries);

            Dictionary<string, ConfusingReason> imageBlackListReason = new Dictionary<string, ConfusingReason>();
            if (confusingImageListFilePath != null)
            {
                imageBlackListReason = getConfusingImageList(confusingImageListFilePath);
            }

            //Dictionary<int, List<SingleObjectLabelingResult>> ResultsPerTask = new Dictionary<int, List<SingleObjectLabelingResult>>();
            Dictionary<int, List<SatyamResultsTableEntry>> ResultsPerTask = new Dictionary<int, List<SatyamResultsTableEntry>>();
            
            List<int> aggregatedTasks = new List<int>();

            int noTotalConverged = 0;
            int noCorrect = 0;

            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth = new SortedDictionary<string, Dictionary<string, int>>();
            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res = new SortedDictionary<string, Dictionary<string, int>>();

            StringBuilder s = new StringBuilder();
            s.Append("<!DOCTYPE html>\n");
            s.Append("<html>\n");
            s.Append("<body>\n");
            String title = String.Format("<h1>Job GUID {0} Incorrect Result Summary</h1>\n", guid);
            s.Append(title);

            Dictionary<int, int> noResultsNeededForAggregation = SatyamResultsAnalysis.getNoResultsNeededForAggregationFromLog(configString, guid);
            Dictionary<int, int> noResultsNeededForAggregation_new = new Dictionary<int, int>();

            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();
            foreach (DateTime t in entriesBySubmitTime.Keys)
            {
                foreach (SatyamResultsTableEntry entry in entriesBySubmitTime[t])
                {
                    //SatyamResultsTableEntry entry = entries[i];
                    SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                    SatyamJob job = task.jobEntry;
                    string fileName = URIUtilities.filenameFromURI(task.SatyamURI);
                    string imageCategoryName = fileName.Split('_')[0];

                    if (IsBlackListed(task.SatyamURI, imageBlackListReason))
                    {
                        continue;
                    }
                    int taskEntryID = entry.SatyamTaskTableEntryID;
                    if (aggregatedTasks.Contains(taskEntryID))
                    {
                        continue;
                    }

                    if (!ResultsPerTask.ContainsKey(taskEntryID))
                    {
                        ResultsPerTask.Add(taskEntryID, new List<SatyamResultsTableEntry>());
                    }
                    //ResultEntriesPerTask[taskEntryID].Add(JSonUtils.ConvertJSonToObject<SingleObjectLabelingResult>(satyamResult.TaskResult));
                    ResultsPerTask[taskEntryID].Add(entry);

                    // check log if enough results are collected
                    if (noResultsNeededForAggregation != null && noResultsNeededForAggregation.ContainsKey(taskEntryID)
                        && ResultsPerTask[taskEntryID].Count < noResultsNeededForAggregation[taskEntryID])
                    {
                        continue;
                    }


                    string aggResultString = SingleObjectLabelingAggregator.GetAggregatedResultString(ResultsPerTask[taskEntryID], MinResults, MaxResults, MajorityThreshold);
                    //SingleObjectLabelingAggregatedResult aggResult = SingleObjectLabelingAggregator.getAggregatedResult(ResultEntriesPerTask[taskEntryID], MinResults, MaxResults, MajorityThreshold);


                    if (aggResultString == null)
                    {
                        continue;
                    }



                    SatyamAggregatedResultsTableEntry aggEntry = new SatyamAggregatedResultsTableEntry();
                    aggEntry.JobGUID = ResultsPerTask[taskEntryID][0].JobGUID;
                    aggEntry.JobTemplateType = ResultsPerTask[taskEntryID][0].JobTemplateType;
                    aggEntry.SatyamTaskTableEntryID = ResultsPerTask[taskEntryID][0].SatyamTaskTableEntryID;
                    aggEntry.UserID = ResultsPerTask[taskEntryID][0].UserID;
                    aggEntry.ResultString = aggResultString;

                    /// aggregation happen
                    // record logs
                    if(noResultsNeededForAggregation == null || !noResultsNeededForAggregation.ContainsKey(taskEntryID))
                    {
                        noResultsNeededForAggregation_new.Add(taskEntryID, ResultsPerTask[taskEntryID].Count);
                    }
                    /// 
                    //if (aggResult.Category == "None of the Above")
                    //{
                    //    continue;
                    //}

                    aggEntries.Add(aggEntry);

                    SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(aggEntry);
                    String resultString = data.AggregatedResultString;
                    SingleObjectLabelingAggregatedResult aggResult = JSonUtils.ConvertJSonToObject<SingleObjectLabelingAggregatedResult>(resultString);

                    if (!confusionMatrix_res_groundtruth.ContainsKey(aggResult.Category))
                    {
                        confusionMatrix_res_groundtruth.Add(aggResult.Category, new Dictionary<string, int>() { { GroundTruth[imageCategoryName], 0 } });
                    }
                    else
                    {
                        if (!confusionMatrix_res_groundtruth[aggResult.Category].ContainsKey(GroundTruth[imageCategoryName]))
                        {
                            confusionMatrix_res_groundtruth[aggResult.Category].Add(GroundTruth[imageCategoryName], 0);
                        }
                    }

                    if (!confusionMatrix_groundtruth_res.ContainsKey(GroundTruth[imageCategoryName]))
                    {
                        confusionMatrix_groundtruth_res.Add(GroundTruth[imageCategoryName], new Dictionary<string, int>() { { aggResult.Category, 0 } });
                    }
                    else
                    {
                        if (!confusionMatrix_groundtruth_res[GroundTruth[imageCategoryName]].ContainsKey(aggResult.Category))
                        {
                            confusionMatrix_groundtruth_res[GroundTruth[imageCategoryName]].Add(aggResult.Category, 0);
                        }
                    }
                    if (aggResult.Category.Equals(GroundTruth[imageCategoryName], StringComparison.InvariantCultureIgnoreCase))
                    {
                        noCorrect++;
                    }
                    else
                    {
                        //Console.WriteLine("{0}, Groundtruth: {1}, Aggregated: {2}, Votes: {3}",
                        //    fileName, GroundTruth[imageCategoryName], aggResult.Category,
                        //    JSonUtils.ConvertObjectToJSon(aggResult.metaData));

                        String record = String.Format("<p>{0}, Groundtruth: {1}, Aggregated: {2}, Votes: {3}</p>\n",
                            fileName, GroundTruth[imageCategoryName], aggResult.Category,
                            JSonUtils.ConvertObjectToJSon(aggResult.metaData));
                        String img = String.Format("<img src=\"{0}\" >\n", task.SatyamURI);
                        s.Append(record);
                        s.Append(img);


                    }
                    noTotalConverged++;
                    if (ResultsPerTask[taskEntryID].Count >= MaxResults)
                    {
                        noTerminatedTasks++;
                    }
                    confusionMatrix_res_groundtruth[aggResult.Category][GroundTruth[imageCategoryName]]++;
                    confusionMatrix_groundtruth_res[GroundTruth[imageCategoryName]][aggResult.Category]++;
                    aggregatedTasks.Add(taskEntryID);
                }
            }

            SatyamResultsAnalysis.RecordAggregationLog(noResultsNeededForAggregation_new, configString, guid);

            s.Append("</body>\n");
            s.Append("</html>\n");
            string dataToBeSaved = s.ToString();

            SatyamJobStorageAccountAccess storage = new SatyamJobStorageAccountAccess();
            string FileName = String.Format("AggregatedIncorrectResults-{0}_Min{1}Max{2}Thresh{3}.html", guid, MinResults, MaxResults, MajorityThreshold);
            storage.SaveATextFile("singleobjectlabeling", guid, FileName, dataToBeSaved);

            s.Clear();
            s.Append("<!DOCTYPE html>\n");
            s.Append("<html>\n");
            s.Append("<body>\n");
            s.Append(title);

            string resultSummary = String.Format("<p>Result: {0}/{1}, precision: {2}</p>\n", noCorrect, noTotalConverged, (double)noCorrect / noTotalConverged);
            resultSummary += String.Format("<p>Terminated: {0}, Not Enough Results: {1}</p>\n", noTerminatedTasks, ResultsPerTask.Count - noTotalConverged);
            Console.WriteLine(resultSummary);
            s.Append(resultSummary);

            // write the confusion matrix
            s.Append("<p>");
            String row = "\t\t";
            foreach (string resultCategory in confusionMatrix_res_groundtruth.Keys)
            {
                row += resultCategory + "\t";
            }
            row += "<br>\n";
            s.Append(row);
            Console.WriteLine(row);

            string matString = "";
            foreach (string groundTruthCategory in confusionMatrix_groundtruth_res.Keys)
            {
                row = groundTruthCategory + "\t";
                foreach (string resultCategory in confusionMatrix_res_groundtruth.Keys)
                {
                    if (confusionMatrix_groundtruth_res[groundTruthCategory].ContainsKey(resultCategory))
                    {
                        row += confusionMatrix_groundtruth_res[groundTruthCategory][resultCategory].ToString();
                    }
                    else
                    {
                        row += "0";
                    }
                    row += "\t";
                }
                row += "<br>\n";
                s.Append(row);
                Console.WriteLine(row);
                matString += row;
            }

            s.Append("</p>\n");

            s.Append("</body>\n");
            s.Append("</html>\n");

            string summaryToBeSaved = s.ToString();

            FileName = String.Format("Aggregated_Summary-{0}_Min{1}Max{2}Thresh{3}.html", guid, MinResults, MaxResults, MajorityThreshold);
            storage.SaveATextFile("singleobjectlabeling", guid, FileName, summaryToBeSaved);

            /// local file 
            string outputString = String.Format("{0} {1} {2} {3} {4} {5}\n", configString, noCorrect, noTotalConverged, (double)noCorrect / noTotalConverged, noTerminatedTasks, ResultsPerTask.Count - noTotalConverged);
            string outputfile = DirectoryConstants.defaultTempDirectory + guid + "\\resultSummary.txt";
            File.AppendAllText(outputfile, outputString);
            string outputmatFile = DirectoryConstants.defaultTempDirectory + guid + "\\" + configString + "_mat.txt";
            File.WriteAllText(outputmatFile, matString);




            //for (double prob = 0; prob < 1;prob +=0.2)
            //{
            //    SatyamResultsAnalysis.AnalyzeApprovalRate(aggEntries, entries, guid, configString, anotherChanceProbablity: prob);
            //}
            //for (double ratio = 0; ratio < 1; ratio += 0.2)
            //{
            //    SatyamResultsAnalysis.AnalyzeApprovalRate(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guid, configString, approvalRatioThreshold: ratio);
            //}
            SatyamResultsAnalysis.AggregationAnalysis(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guid, configString);

        }


        public static void ValidateSameTaskSameWorkerMultipleResultsEffect(string jobGUID, string confusingImageListFilePath = null)
        {
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUIDOrderByID(jobGUID);
            resultsDB.close();
            Dictionary<int, int> noResultsPerTask = new Dictionary<int, int>();
            Dictionary<int, int> noAcceptedResultsPerTask = new Dictionary<int, int>();
            Dictionary<int, int> noPaidResultsPerTask = new Dictionary<int, int>();
            Dictionary<int, double> moneyPaidPerTask = new Dictionary<int, double>();
            SortedDictionary<int, Dictionary<string, List<string>>> ResultsPerWorkerPerTask = new SortedDictionary<int, Dictionary<string, List<string>>>();
            SortedDictionary<int, Dictionary<string, int>> noDifferentDuplicateResultsPerWorkerPerTask = new SortedDictionary<int, Dictionary<string, int>>();
            Dictionary<int, string> taskSatyamUri = new Dictionary<int, string>();
            Dictionary<int, double> timeTakenTillAggregationPerTask = new Dictionary<int, double>();
            Dictionary<int, DateTime> finalTaskEndTime = new Dictionary<int, DateTime>();
            Dictionary<string, double> finalHITEndTime = new Dictionary<string, double>();
            int totalResults = 0;
            int noTasksWithDuplicateResultsFromSameWorker = 0;
            int noTasksWithMixedResultsFromSameWorker = 0;
            int noTaskWithDuplicateResultsAsMajority = 0;
            int noTaskWhoseDuplicateResultsChangedAggregation = 0;
            int noTaskWhoseDuplicateResultsChangedAggregationIncorrectly = 0;
            int noCorrectDecisionsAmongDuplicates = 0;

            //noCorrectDuplicateResultsOfSameWorkerSameTask = 0;
            int noTasksWithCorrectDuplicateResultsOfSameWorkerSameTask = 0;
            int noWorkerSwitchedToCorrect = 0;
            int noWorkerSwitchedToCorrectAndMaintained = 0;
            int noTasksWithWorkerSwitchedToCorrect = 0;
            int noTasksWithWorkerSwitchedToCorrectAndMaintained = 0;

            List<double> timeTakenPerResult = new List<double>();
            List<double> acceptedTimeTakenPerResult = new List<double>();
            List<int> SequenceNumberOfResultPerTask = new List<int>();
            List<int> SequenceNumberOfResultPerWorker = new List<int>();

            string JobGUID = entries[0].JobGUID;


            Dictionary<string, int> noJobsPerWorker = new Dictionary<string, int>();
            Dictionary<string, int> noAcceptedJobsPerWorker = new Dictionary<string, int>();
            Dictionary<string, int> noPaidJobsPerWorker = new Dictionary<string, int>();


            List<double> resultArrivalTimes = new List<double>();

            totalResults = entries.Count;

            ///imagenet only
            Dictionary<string, ConfusingReason> imageBlackListReason = new Dictionary<string, ConfusingReason>();
            if (confusingImageListFilePath != null)
            {
                imageBlackListReason = getConfusingImageList(confusingImageListFilePath);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                SatyamResultsTableEntry entry = entries[i];
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                SatyamJob job = task.jobEntry;

                if (IsBlackListed(task.SatyamURI, imageBlackListReason))
                {
                    continue;
                }
                int taskEntryID = entry.SatyamTaskTableEntryID;
                if (!noResultsPerTask.ContainsKey(taskEntryID))
                {
                    noResultsPerTask.Add(taskEntryID, 0);
                    noAcceptedResultsPerTask.Add(taskEntryID, 0);
                    noPaidResultsPerTask.Add(taskEntryID, 0);
                    moneyPaidPerTask.Add(taskEntryID, 0);
                    finalTaskEndTime.Add(taskEntryID, entry.SubmitTime);

                    ResultsPerWorkerPerTask.Add(taskEntryID, new Dictionary<string, List<string>>());
                    noDifferentDuplicateResultsPerWorkerPerTask.Add(taskEntryID, new Dictionary<string, int>());
                    taskSatyamUri.Add(taskEntryID, task.SatyamURI);
                }

                if (!ResultsPerWorkerPerTask[taskEntryID].ContainsKey(satyamResult.amazonInfo.WorkerID))
                {
                    ResultsPerWorkerPerTask[taskEntryID].Add(satyamResult.amazonInfo.WorkerID, new List<string>());
                    noDifferentDuplicateResultsPerWorkerPerTask[taskEntryID].Add(satyamResult.amazonInfo.WorkerID, 0);
                }
                if (!ResultsPerWorkerPerTask[taskEntryID][satyamResult.amazonInfo.WorkerID].Contains(satyamResult.TaskResult))
                {
                    noDifferentDuplicateResultsPerWorkerPerTask[taskEntryID][satyamResult.amazonInfo.WorkerID]++;
                }
                ResultsPerWorkerPerTask[taskEntryID][satyamResult.amazonInfo.WorkerID].Add(satyamResult.TaskResult);

            }

            ///////////////////////////////////// Per Task Analysis //////////////////////////////////////
            SortedDictionary<int, int> resultsPerTaskHistogram = new SortedDictionary<int, int>();
            SortedDictionary<int, int> resultsAcceptedPerTaskHistogram = new SortedDictionary<int, int>();
            SortedDictionary<int, int> resultsPaidPerTaskHistogram = new SortedDictionary<int, int>();
            SortedDictionary<int, int> moneyPaidPerTaskHistogram = new SortedDictionary<int, int>(); //cents
            SortedDictionary<int, int> ResultsPerWorkerPerTaskHistogram = new SortedDictionary<int, int>();
            List<int> taskIDs = noResultsPerTask.Keys.ToList();
            foreach (int taskID in taskIDs)
            {
                if (ResultsPerWorkerPerTask[taskID].Count != noResultsPerTask[taskID])
                {
                    //has multiple results from same turker
                    noTasksWithDuplicateResultsFromSameWorker++;
                    // the aggregation result
                    List<SingleObjectLabelingResult> allResultsPerTask = new List<SingleObjectLabelingResult>();
                    SingleObjectLabelingAggregatedResult aggregatedResultPerTask = new SingleObjectLabelingAggregatedResult();
                    // the aggregation if without duplicate results
                    List<SingleObjectLabelingResult> OnlyFirstResultOfEachTurkerPerTask = new List<SingleObjectLabelingResult>();
                    SingleObjectLabelingAggregatedResult aggregatedOnlyFirstResultOfTurkerPerTask = new SingleObjectLabelingAggregatedResult();
                    foreach (List<string> ResultsStringsPerWorkerPerTask in ResultsPerWorkerPerTask[taskID].Values)
                    {
                        foreach (string resultString in ResultsStringsPerWorkerPerTask)
                        {
                            allResultsPerTask.Add(JSonUtils.ConvertJSonToObject<SingleObjectLabelingResult>(resultString));
                        }
                        OnlyFirstResultOfEachTurkerPerTask.Add(JSonUtils.ConvertJSonToObject<SingleObjectLabelingResult>(ResultsStringsPerWorkerPerTask[0]));
                    }
                    aggregatedResultPerTask = SingleObjectLabelingAggregator.getAggregatedResult(allResultsPerTask);
                    aggregatedOnlyFirstResultOfTurkerPerTask = SingleObjectLabelingAggregator.getAggregatedResult(OnlyFirstResultOfEachTurkerPerTask);
                    if (aggregatedResultPerTask != null)
                    {
                        if (aggregatedOnlyFirstResultOfTurkerPerTask == null ||
                        aggregatedResultPerTask.Category != aggregatedOnlyFirstResultOfTurkerPerTask.Category)
                        {
                            noTaskWhoseDuplicateResultsChangedAggregation++;
                            if (AggregatedResultEqualsGroundTruth(taskSatyamUri[taskID], JSonUtils.ConvertObjectToJSon(aggregatedResultPerTask)))
                            {
                                noTaskWhoseDuplicateResultsChangedAggregationIncorrectly++;
                            }
                        }
                    }


                    bool hasMixedResults = false;
                    bool hasCorrectResult = false;
                    bool atLeastOneShiftedFromIncorrectToCorrect = false;
                    bool atLeastOneShiftedFromIncorrectToCorrectAndMaintained = false;
                    foreach (List<string> ResultsStringsPerWorkerPerTask in ResultsPerWorkerPerTask[taskID].Values)
                    {
                        if (!ResultsPerWorkerPerTaskHistogram.ContainsKey(ResultsStringsPerWorkerPerTask.Count))
                        {
                            ResultsPerWorkerPerTaskHistogram.Add(ResultsStringsPerWorkerPerTask.Count, 0);
                        }
                        ResultsPerWorkerPerTaskHistogram[ResultsStringsPerWorkerPerTask.Count]++;

                        double superMajority = 0.6; // tunable
                        if (((double)ResultsStringsPerWorkerPerTask.Count + 1) / ((double)noResultsPerTask[taskID] + 2) > superMajority)
                        {
                            noTaskWithDuplicateResultsAsMajority++;
                        }
                        ///////////// imagenet only //////////////////

                        if (ResultsStringsPerWorkerPerTask.Distinct().Count() > 1)
                        {
                            // multiple choices given
                            hasMixedResults = true;
                            bool incorrect = false;
                            bool switchToCorrect = false;
                            foreach (string resultString in ResultsStringsPerWorkerPerTask)
                            {
                                if (SingleObjectLabelingResultEqualsGroundTruth(taskSatyamUri[taskID], resultString))
                                {
                                    hasCorrectResult = true;
                                    noCorrectDecisionsAmongDuplicates++;

                                    if (incorrect)
                                    {
                                        switchToCorrect = true;
                                    }
                                    incorrect = false;
                                }
                                else
                                {
                                    incorrect = true;
                                }
                            }
                            if (switchToCorrect)
                            {
                                // been incorrect and swithed to correct
                                noWorkerSwitchedToCorrect++;
                                atLeastOneShiftedFromIncorrectToCorrect = true;
                                if (!incorrect)
                                {
                                    //switch to correct and maintain till the end
                                    noWorkerSwitchedToCorrectAndMaintained++;
                                    atLeastOneShiftedFromIncorrectToCorrectAndMaintained = true;
                                }

                            }
                        }
                        ////////////////////////////////////////////////
                    }

                    if (hasMixedResults)
                    {
                        noTasksWithMixedResultsFromSameWorker++;
                    }
                    if (hasCorrectResult)
                    {
                        noTasksWithCorrectDuplicateResultsOfSameWorkerSameTask++;
                    }
                    if (atLeastOneShiftedFromIncorrectToCorrect)
                    {
                        noTasksWithWorkerSwitchedToCorrect++;
                    }
                    if (atLeastOneShiftedFromIncorrectToCorrectAndMaintained)
                    {
                        noTasksWithWorkerSwitchedToCorrectAndMaintained++;
                    }
                }
            }

            Console.WriteLine("DuplicateResultsHistogram");
            foreach (int no in ResultsPerWorkerPerTaskHistogram.Keys)
            {
                Console.WriteLine("{0}, {1}", no, ResultsPerWorkerPerTaskHistogram[no]);
            }

            Console.WriteLine(
                //"{0} images (in total {1}({2} of which are correct)) has duplicate results from same turker, \n" +
                //"\t{3} images have >=1 workers making more than majority number of results.\n" +
                //"\t{11} images aggregation is changed ({12} of which incorrectly) by duplicate results from same turker.\n" +
                //"\t{4} images(In total {5} times) with duplicate results include mixed(>= 2) decisions from same worker.\n" +
                "\t\t{6} images has correct decision among duplicate decisions.\n" +
                "\t\t{7} images({8} times) a worker has switched from incorrect to correct choice\n" +
                "\t\t\t{9} images({10} times) of which maintained the correct choice till the last time of their job on that image.",
                //noTasksWithDuplicateResultsFromSameWorker, noDuplicateResultsFromSameWorker, noCorrectDecisionsAmongDuplicates,
                //noTaskWithDuplicateResultsAsMajority,
                //noTasksWithMixedResultsFromSameWorker, noDecisionChangeAmongDuplicateResultsOfSameWorkerSameTask,
                //noTasksWithCorrectDuplicateResultsOfSameWorkerSameTask,
                noTasksWithWorkerSwitchedToCorrect, noWorkerSwitchedToCorrect,
                noTasksWithWorkerSwitchedToCorrectAndMaintained, noWorkerSwitchedToCorrectAndMaintained,
                noTaskWhoseDuplicateResultsChangedAggregation, noTaskWhoseDuplicateResultsChangedAggregationIncorrectly
                );
            
            
        }


        public static void validateImageNet1000ClassDetectionResult()
        {
            string baseDir = @"C:\research\dataset\inception1000\";
            //string classNameFile = baseDir + "1000class.txt";
            string resultFile = baseDir + "results_inception_Seattle.txt";
            string is_a_File = baseDir + "wordnet.is_a.txt";
            string wnid_words_File = baseDir + "words.txt";
            string synsetFile = baseDir + "synset.txt";

            //List<string> classNameContent = File.ReadAllLines(classNameFile).ToList();

            //Dictionary<int, string> classNames = new Dictionary<int, string>();
            //for (int i=0;i<classNameContent.Count;i++)
            //{
            //    string[] fields = classNameContent[i].Split(':');
            //    string[] names = fields[1].Split('\'');
            //    string name = "";
            //    for(int j = 1; j < names.Length - 1; j++)
            //    {
            //        name += names[j];
            //        if (j != names.Length - 2)
            //        {
            //            name += '\'';
            //        }
            //    }
            //    classNames.Add(i, name);
            //}

            List<string> synsetContent = File.ReadAllLines(synsetFile).ToList();

            Dictionary<int, string> synsetNames = new Dictionary<int, string>();
            for (int i = 0; i < synsetContent.Count; i++)
            {
                string[] fields = synsetContent[i].Split(' ');
                synsetNames.Add(i, fields[0]);
            }


            List<string> is_a_Content = File.ReadLines(is_a_File).ToList();
            Dictionary<string, List<string>> is_a = new Dictionary<string, List<string>>();
            foreach(string isA in is_a_Content)
            {
                string[] fields = isA.Split(' ');
                if (!is_a.ContainsKey(fields[1]))
                {
                    is_a.Add(fields[1], new List<string>());
                }
                is_a[fields[1]].Add(fields[0]);
            }

            List<string> wordsContent = File.ReadAllLines(wnid_words_File).ToList();
            Dictionary<string, List<string>> word_wnid = new Dictionary<string, List<string>>();
            foreach (string word in wordsContent)
            {
                string[] fields = word.Split('\t');
                //word_wnid.Add(fields[1], fields[0]);
                if (!word_wnid.ContainsKey(fields[1]))
                {
                    word_wnid.Add(fields[1], new List<string>());
                }
                word_wnid[fields[1]].Add(fields[0]);
            }

            List<string> resContent = File.ReadAllLines(resultFile).ToList();
            List<KeyValuePair<double, int>> results = new List<KeyValuePair<double, int>>();
            List<int> gt = new List<int>();
            foreach (string res in resContent)
            {
                string[] fields = res.Split('[');
                double confidence = Convert.ToDouble(fields[1].Split(']')[0]);
                int classIdx = Convert.ToInt32(fields[2].Split(']')[0]);
                results.Add(new KeyValuePair<double,int>(confidence,classIdx));
                gt.Add(Convert.ToInt32(fields[2].Split(' ')[1]));
            }

            List<string> result_wnid = new List<string>();
            List<string> result_category = new List<string>();


            List<string> dets_gts_oneString = new List<string>();

            List<string> categoriesFound = new List<string>();
            for (int i = 0; i < results.Count; i++)
            {
                
                KeyValuePair<double, int> res = results[i];

                if (res.Value == 0)
                {
                    result_wnid.Add("");
                    result_category.Add("");
                    continue;
                }
                string synsetName = synsetNames[res.Value-1];
                

                //string classwnid = word_wnid[classname];
                List<string> is_wnid = new List<string>();
                is_wnid.Add(synsetName);
                //foreach(string classwnid in word_wnid[synsetName])
                //{
                //    is_wnid.Add(classwnid);
                //}

                for (int j = 0; j < is_wnid.Count; j++)
                {
                    if (!is_a.ContainsKey(is_wnid[j])) continue;
                    foreach (string wnid in is_a[is_wnid[j]])
                    {
                        is_wnid.Add(wnid);
                    }
                }

                bool found = false;
                foreach(string wnid in is_wnid)
                {
                    if (GroundTruth.ContainsKey(wnid))
                    {
                        result_wnid.Add(wnid);
                        result_category.Add(GroundTruth[wnid]);
                        found = true;
                        if (!categoriesFound.Contains(GroundTruth[wnid]))
                        {
                            categoriesFound.Add(GroundTruth[wnid]);
                        }
                        break;
                    }
                }
                if (!found)
                {
                    result_wnid.Add("");
                    result_category.Add("");
                }

                string gtString = GroundTruth_AlphabeticalOrder[gt[i] - 1];
                dets_gts_oneString.Add(synsetName + " " + gtString);
            }

            int noCorrect = 0;
            int noTotal = results.Count;
            int noTotalPredictions = 0;

            List<KeyValuePair<string, string>> dets_gts = new List<KeyValuePair<string, string>>();
            

            for (int i = 0; i < results.Count; i++)
            {
                string detection = result_category[i];
                string gtString = GroundTruth_AlphabeticalOrder[gt[i] - 1];
                dets_gts.Add(new KeyValuePair<string, string>(detection, gtString));
                
                if (result_category[i] != "" && result_category[i] != "amulance")
                {
                    noTotalPredictions++;
                    
                    if (detection.Equals(gtString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        noCorrect++;
                    }
                    else
                    {
                        Console.WriteLine(detection + " " + gtString);
                    }
                }
            }

            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth;
            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res;
            SatyamResultValidationToolKit.getConfusionMat(dets_gts, out confusionMatrix_res_groundtruth, out confusionMatrix_groundtruth_res);
            string outputFile = @"C:\research\dataset\inception1000\confusionMat.txt";
            SatyamResultValidationToolKit.printConfusionMatrix(confusionMatrix_groundtruth_res, confusionMatrix_res_groundtruth, outputFile);

            outputFile = @"C:\research\dataset\inception1000\det_gts.txt";
            File.WriteAllLines(outputFile, dets_gts_oneString.ToArray());
        }
    }
}
