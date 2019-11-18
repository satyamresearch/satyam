using Constants;
using JobTemplateClasses;
using SatyamAnalysis;
using SatyamResultAggregators;
using SatyamResultsSaving;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SatyamResultValidation
{
    public class JHMDBVideoClassifciationValidation
    {
        public static List<string> GroundTruth = new List<string>()
        {
            "clap",
            "jump",
            "pick",
            "push",
            "run",
            "sit",
            "stand",
            "throw",
            "walk",
            "wave",
        };

        public static string getVideoCategoryFromFileName(string filename)
        {
            string[] fields = filename.Split('_');
            string VideoCategory = "";
            foreach (string field in fields)
            {
                if (GroundTruth.Contains(field))
                {
                    VideoCategory = field;break;
                }
            }
            return VideoCategory;
        }

        public static bool SingleObjectLabelingResultEqualsGroundTruth(string satyamUri, string resultString)
        {
            string fileName = URIUtilities.filenameFromURINoExtension(satyamUri);
            
            string VideoCategory = getVideoCategoryFromFileName(fileName);
            if (VideoCategory == "")
            {
                Console.WriteLine("this video doesn't have valid category in filename");
            }

            SingleObjectLabelingResult result = JSonUtils.ConvertJSonToObject<SingleObjectLabelingResult>(resultString);

            return (result.Category.Equals(VideoCategory, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool AggregatedResultEqualsGroundTruth(string satyamUri, string resultString)
        {
            string fileName = URIUtilities.filenameFromURINoExtension(satyamUri);

            string VideoCategory = getVideoCategoryFromFileName(fileName);
            if (VideoCategory == "")
            {
                Console.WriteLine("this video doesn't have valid category in filename");
            }

            SingleObjectLabelingAggregatedResult result = JSonUtils.ConvertJSonToObject<SingleObjectLabelingAggregatedResult>(resultString);

            return (result.Category.Equals(VideoCategory, StringComparison.InvariantCultureIgnoreCase));
        }


        public static void EvaluateAndPrintConfusionMatrixOfAggregatedResultEntries(List<SatyamAggregatedResultsTableEntry> aggResultEntries, 
            string outputFile,
            out int noCorrect,
            //out SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth,
            //out SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res,
            bool prepareDataForTraining = false, string outputDirectory = null)
        {

            noCorrect = 0;
            //SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth = new SortedDictionary<string, Dictionary<string, int>>();
            //SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res = new SortedDictionary<string, Dictionary<string, int>>();

            List<KeyValuePair<string,string>> detections_gts = new List<KeyValuePair<string, string>>();
            

            WebClient wc = new WebClient();
            foreach (SatyamAggregatedResultsTableEntry aggResultEntry in aggResultEntries)
            {
                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(aggResultEntry);
                SingleObjectLabelingAggregatedResult aggResult = JSonUtils.ConvertJSonToObject<SingleObjectLabelingAggregatedResult>(data.AggregatedResultString);
                string fileName = URIUtilities.filenameFromURI(data.SatyamURI);
                string VideoCategoryGroundTruth = getVideoCategoryFromFileName(fileName);

                detections_gts.Add( new KeyValuePair<string, string>(aggResult.Category, VideoCategoryGroundTruth));

                //if (!confusionMatrix_res_groundtruth.ContainsKey(aggResult.Category))
                //{
                //    confusionMatrix_res_groundtruth.Add(aggResult.Category, new Dictionary<string, int>());
                //}
                //if (!confusionMatrix_res_groundtruth[aggResult.Category].ContainsKey(VideoCategoryGroundTruth))
                //{
                //    confusionMatrix_res_groundtruth[aggResult.Category].Add(VideoCategoryGroundTruth, 0);
                //}
                //if (!confusionMatrix_groundtruth_res.ContainsKey(VideoCategoryGroundTruth))
                //{
                //    confusionMatrix_groundtruth_res.Add(VideoCategoryGroundTruth, new Dictionary<string, int>());
                //}
                //if (!confusionMatrix_groundtruth_res[VideoCategoryGroundTruth].ContainsKey(aggResult.Category))
                //{
                //    confusionMatrix_groundtruth_res[VideoCategoryGroundTruth].Add(aggResult.Category, 0);
                //}
                //confusionMatrix_res_groundtruth[aggResult.Category][VideoCategoryGroundTruth]++;
                //confusionMatrix_groundtruth_res[VideoCategoryGroundTruth][aggResult.Category]++;

                if (aggResult.Category.Equals(VideoCategoryGroundTruth, StringComparison.InvariantCultureIgnoreCase))
                {
                    noCorrect++;
                }
                else
                {
                    Console.WriteLine("{0}, Groundtruth: {1}, Aggregated: {2}, Votes: {3}",
                        fileName, VideoCategoryGroundTruth, aggResult.Category,
                        JSonUtils.ConvertObjectToJSon(aggResult.metaData));
                }
                
                
                


                // prepare training dataset
                if (prepareDataForTraining)
                {
                    if (!Directory.Exists(outputDirectory + "\\" + aggResult.Category))
                    {
                        Directory.CreateDirectory(outputDirectory + "\\" + aggResult.Category);
                    }
                    wc.DownloadFile(data.SatyamURI, outputDirectory + "\\" + aggResult.Category + "\\" + fileName);
                }
            }


            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth;
            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res;
            SatyamResultValidationToolKit.getConfusionMat(detections_gts, out confusionMatrix_res_groundtruth, out confusionMatrix_groundtruth_res);
            SatyamResultValidationToolKit.printConfusionMatrix(confusionMatrix_groundtruth_res, confusionMatrix_res_groundtruth, outputFile);
        }

        public static void ValidateSatyamJHMDBVideoClassificationAggregationResultByGUID(string guid, string outputFile, 
            //string confusingImageListFilePath = null,
            bool prepareDataForTraining = false, string outputDirectory = null)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();

            int noTotal = results.Count;
            int noCorrect = 0;

            EvaluateAndPrintConfusionMatrixOfAggregatedResultEntries(results, outputFile, out noCorrect, prepareDataForTraining, outputDirectory);

            Console.WriteLine("Result: {0}/{1}, precision: {2}", noCorrect, noTotal, (double)noCorrect / noTotal);
        }


        public static void AggregateWithParameterAndValidateSatyamVideoClassificationResultByGUID(string guid, 
            int MinResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE, 
            int MaxResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE, 
            double MajorityThreshold = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAJORITY_THRESHOLD, 
            bool prepareDataForTraining = false, string outputDirectory = null,
            string confusingVideoListFilePath = null)
        {
            string configString = "Min_" + MinResults + "_Max_" + MaxResults + "_Thresh_" + MajorityThreshold;
            Console.WriteLine("Aggregating for param set " + configString);
            int noTerminatedTasks = 0;
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUIDOrderByID(guid);
            resultsDB.close();


            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime = SatyamResultValidationToolKit.SortResultsBySubmitTime(entries);

            int noTotalConverged = 0;
            int noCorrect = 0;

            Dictionary<int, List<SatyamResultsTableEntry>> ResultsPerTask = new Dictionary<int, List<SatyamResultsTableEntry>>();
            List<int> aggregatedTasks = new List<int>();

            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth = new SortedDictionary<string, Dictionary<string, int>>();
            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res = new SortedDictionary<string, Dictionary<string, int>>();

            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();

            Dictionary<int, int> noResultsNeededForAggregation = SatyamResultsAnalysis.getNoResultsNeededForAggregationFromLog(configString, guid);
            Dictionary<int, int> noResultsNeededForAggregation_new = new Dictionary<int, int>();

            foreach (DateTime t in entriesBySubmitTime.Keys)
            {
                //Console.WriteLine("Processing Results of time: {0}", t);
                foreach (SatyamResultsTableEntry entry in entriesBySubmitTime[t])
                {
                    SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                    SatyamJob job = task.jobEntry;
                    string fileName = URIUtilities.filenameFromURI(task.SatyamURI);
                    string VideoCategoryGroundTruth = getVideoCategoryFromFileName(fileName);

                    int taskEntryID = entry.SatyamTaskTableEntryID;
                    if (aggregatedTasks.Contains(taskEntryID))
                    {
                        continue;
                    }

                    if (!ResultsPerTask.ContainsKey(taskEntryID))
                    {
                        ResultsPerTask.Add(taskEntryID, new List<SatyamResultsTableEntry>());
                    }

                    ResultsPerTask[taskEntryID].Add(entry);

                    // check log if enough results are collected
                    if (noResultsNeededForAggregation != null && noResultsNeededForAggregation.ContainsKey(taskEntryID)
                        && ResultsPerTask[taskEntryID].Count < noResultsNeededForAggregation[taskEntryID])
                    {
                        continue;
                    }

                    //SingleObjectLabelingAggregatedResult aggResult = SingleObjectLabelingAggregator.getAggregatedResult(ResultsPerTask[taskEntryID], MinResults, MaxResult, MajorityThreshold);
                    string aggResultString = SingleObjectLabelingAggregator.GetAggregatedResultString(ResultsPerTask[taskEntryID], MinResults, MaxResults, MajorityThreshold);

                    if (aggResultString == null)
                    {
                        continue;
                    }

                    SatyamAggregatedResultsTableEntry aggEntry = new SatyamAggregatedResultsTableEntry();
                    aggEntry.JobGUID = ResultsPerTask[taskEntryID][0].JobGUID;
                    aggEntry.JobTemplateType = ResultsPerTask[taskEntryID][0].JobTemplateType;
                    aggEntry.SatyamTaskTableEntryID = taskEntryID;
                    aggEntry.UserID = ResultsPerTask[taskEntryID][0].UserID;
                    aggEntry.ResultString = aggResultString;

                    /// aggregation happen
                    // record logs
                    if (noResultsNeededForAggregation == null || !noResultsNeededForAggregation.ContainsKey(taskEntryID))
                    {
                        noResultsNeededForAggregation_new.Add(taskEntryID, ResultsPerTask[taskEntryID].Count);
                    }

                    aggEntries.Add(aggEntry);
                    noTotalConverged++;
                    if (ResultsPerTask[taskEntryID].Count >= MaxResults)
                    {
                        noTerminatedTasks++;
                    }
                    aggregatedTasks.Add(taskEntryID);
                }
            }

            SatyamResultsAnalysis.RecordAggregationLog(noResultsNeededForAggregation_new, configString, guid);


            string outputmatFile = DirectoryConstants.defaultTempDirectory + guid + "\\" + configString + "_mat.txt";
            EvaluateAndPrintConfusionMatrixOfAggregatedResultEntries(aggEntries, outputmatFile, out noCorrect, prepareDataForTraining, outputDirectory);


            Console.WriteLine("noTotalConverged {0}", noTotalConverged);
            Console.WriteLine("noTerminatedTasks {0}", noTerminatedTasks);
            Console.WriteLine("Result: {0}/{1}, precision: {2}", noCorrect, noTotalConverged, (double)noCorrect / noTotalConverged);
            /// local file 
            string outputString = String.Format("{0} {1} {2} {3} {4} {5}\n", configString, noCorrect, noTotalConverged, (double)noCorrect / noTotalConverged, noTerminatedTasks, ResultsPerTask.Count - noTotalConverged);
            string outputfile = DirectoryConstants.defaultTempDirectory + guid + "\\resultSummary.txt";
            File.AppendAllText(outputfile, outputString);



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




    }
}
