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
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SatyamResultValidation
{
    public class ObjectCountingValidation
    {


        public static void ValidateSatyamKITTIObjectCountingAggregationResultByGUID(string guid,
            bool saveImage = false,
            int MinHeight = TaskConstants.OBJECT_COUNTING_VALIDATION_MIN_HEIGHT,
            int MaxOcclusion = TaskConstants.OBJECT_COUNTING_VALIDATION_MAX_OCCLUSION,
            double Max_Truncation = TaskConstants.OBJECT_COUNTING_VALIDATION_MIN_TRUNCATION)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();
            double totalError, totalGT;
            ValidateSatyamKITTIObjectCountingAggregationResult(results,out totalError,out totalGT, MinHeight, MaxOcclusion, Max_Truncation);
        }




        

        public static void AggregateWithParameterAndValidateObjectCountingResultByGUID(string guid,
            //bool saveImage = false,
            bool prepareTrainingSet = false, string outputDirectory = null,
            int MinHeight = TaskConstants.OBJECT_COUNTING_VALIDATION_MIN_HEIGHT,
            int MaxOcclusion = TaskConstants.OBJECT_COUNTING_VALIDATION_MAX_OCCLUSION,
            double Max_Truncation = TaskConstants.OBJECT_COUNTING_VALIDATION_MIN_TRUNCATION,
            int MinResults = TaskConstants.OBJECT_COUNTING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.OBJECT_COUNTING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND = TaskConstants.OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND,
            double MAX_DEVIATION_FRACTION = TaskConstants.OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION,
            double SUPER_MAJORITY_VALUE = TaskConstants.OBJECT_COUNTING_MTURK_SUPER_MAJORITY_VALUE
            )
        {
            if (!Directory.Exists(DirectoryConstants.defaultTempDirectory + guid))
            {
                Directory.CreateDirectory(DirectoryConstants.defaultTempDirectory + guid);
            }

            string configString = "Min_" + MinResults + "_Max_" + MaxResults + "_Dev_" + MAX_DEVIATION_FRACTION + "_Majority_" + SUPER_MAJORITY_VALUE;
            Console.WriteLine("Aggregating for param set " + configString);
            int noTerminatedTasks = 0;
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUIDOrderByID(guid);
            resultsDB.close();

            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime = SatyamResultValidationToolKit.SortResultsBySubmitTime(entries);

            int noTotalConverged = 0;
            //int noCorrect = 0;

            Dictionary<int, List<SatyamResultsTableEntry>> ResultsPerTask = new Dictionary<int, List<SatyamResultsTableEntry>>();
            List<int> aggregatedTasks = new List<int>();

            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();
            Dictionary<int, int> noResultsNeededForAggregation = SatyamResultsAnalysis.getNoResultsNeededForAggregationFromLog(configString, guid);
            Dictionary<int, int> noResultsNeededForAggregation_new = new Dictionary<int, int>();
            // play back by time
            foreach (DateTime t in entriesBySubmitTime.Keys)
            {
                //Console.WriteLine("Processing Results of time: {0}", t);
                foreach (SatyamResultsTableEntry entry in entriesBySubmitTime[t])
                {
                    SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                    SatyamJob job = task.jobEntry;
                    string fileName = URIUtilities.filenameFromURI(task.SatyamURI);


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

                    string aggResultString = ObjectCountingAggregator.GetAggregatedResultString(ResultsPerTask[taskEntryID], MinResults, MaxResults, MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND, MAX_DEVIATION_FRACTION, SUPER_MAJORITY_VALUE);

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
                    /// // record logs
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

            double totalError, totalGroundtruth;
            //ValidateSatyamKITTIObjectCountingAggregationResult(aggEntries, out totalError, out totalGroundtruth, 
            //    MinHeight, MaxOcclusion, Max_Truncation);
            ValidateSatyamCARPKObjectCountingAggregationResult(aggEntries, out totalError, out totalGroundtruth);

            Console.WriteLine("noTotalConverged {0}", noTotalConverged);
            Console.WriteLine("noTerminatedTasks {0}", noTerminatedTasks);
            /// local file 
            string outputString = String.Format("{0} {1} {2} {3} {4} {5} {6}\n", configString, totalError, totalGroundtruth, totalError/ totalGroundtruth, noTotalConverged, noTerminatedTasks, ResultsPerTask.Count - noTotalConverged);
            string outputfile = DirectoryConstants.defaultTempDirectory + guid + "\\resultSummary.txt";
            File.AppendAllText(outputfile, outputString);

            string approvalString = configString + "_PayDev_" + TaskConstants.OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_FOR_PAYMENT +
                "_PayFrac_" + TaskConstants.OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION_FOR_PAYMENT;

            //for (double prob = 0; prob < 1;prob +=0.2)
            //{
            //    SatyamResultsAnalysis.AnalyzeApprovalRate(aggEntries, entries, guid, approvalString, anotherChanceProbablity: prob);
            //}
            //for (double ratio = 0; ratio < 1; ratio += 0.2)
            //{
            //    SatyamResultsAnalysis.AnalyzeApprovalRate(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guid, configString, approvalRatioThreshold: ratio);
            //}
            SatyamResultsAnalysis.AggregationAnalysis(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guid, configString);


        }

        private static void ValidateSatyamKITTIObjectCountingAggregationResult(List<SatyamAggregatedResultsTableEntry> aggResultEntries,
            //bool saveImage = false,
            out double totalError,
            out double totalGroundTruth,
            int MinHeight = TaskConstants.OBJECT_COUNTING_VALIDATION_MIN_HEIGHT,
            int MaxOcclusion = TaskConstants.OBJECT_COUNTING_VALIDATION_MAX_OCCLUSION,
            double Max_Truncation = TaskConstants.OBJECT_COUNTING_VALIDATION_MIN_TRUNCATION)
        {
            List<double> aggResultCounts = new List<double>();
            List<int> GroundTruthCounts = new List<int>();
            List<double> errors = new List<double>();
            foreach (SatyamAggregatedResultsTableEntry aggResultEntry in aggResultEntries)
            {
                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(aggResultEntry);
                string fileName = URIUtilities.filenameFromURINoExtension(data.SatyamURI);
                KITTIDetectionGroundTruth GroundTruthObjects = new KITTIDetectionGroundTruth(KITTIDetectionResultValidation.GroundTruthLabelDirectory + fileName + ".txt", MinHeight, MaxOcclusion, Max_Truncation);

                string jobGUID = aggResultEntry.JobGUID;
                int taskID = aggResultEntry.SatyamTaskTableEntryID;
                String resultString = data.AggregatedResultString;
                ObjectCountingAggregatedResult result = JSonUtils.ConvertJSonToObject<ObjectCountingAggregatedResult>(resultString);
                int GroundtruthCount = 0;
                for (int i = 0; i < GroundTruthObjects.objects.Count; i++)
                {
                    MultiObjectLocalizationAndLabelingResultSingleEntry obj = GroundTruthObjects.objects[i];
                    //if (obj.Category == "Car" || obj.Category == "Van" || obj.Category == "DontCare")
                    if (obj.Category == "Car" || obj.Category == "Van")
                    {
                        //if (!GroundTruthObjects.BlackListed[i])
                        //{
                        GroundtruthCount++;
                        //}
                    }
                }
                aggResultCounts.Add(result.Count);
                GroundTruthCounts.Add(GroundtruthCount);
                errors.Add(Math.Abs(GroundtruthCount - result.Count));
            }
            totalError = errors.Sum();
            totalGroundTruth = GroundTruthCounts.Sum();
            double totalErrorRatio = totalError / totalGroundTruth;

            Console.WriteLine("Error: {0} / {1} = {2}", totalError, totalGroundTruth, totalErrorRatio);
            Console.WriteLine("Total Aggregated {0}", aggResultEntries.Count);
        }

        private static void ValidateSatyamCARPKObjectCountingAggregationResult(List<SatyamAggregatedResultsTableEntry> aggResultEntries,
            out double totalError, out double totalGroundTruth)
        {
            List<double> aggResultCounts = new List<double>();
            List<int> GroundTruthCounts = new List<int>();
            List<double> abs_errors = new List<double>();
            List<double> errors = new List<double>();
            foreach (SatyamAggregatedResultsTableEntry aggResultEntry in aggResultEntries)
            {
                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(aggResultEntry);
                string fileName = URIUtilities.filenameFromURINoExtension(data.SatyamURI);
                string labelFilePath = DirectoryConstants.CARPKCountingLabels + fileName + ".txt";

                string[] labels = File.ReadAllLines(labelFilePath);
                int GroundtruthCount = labels.Length;
                
                string jobGUID = aggResultEntry.JobGUID;
                int taskID = aggResultEntry.SatyamTaskTableEntryID;
                String resultString = data.AggregatedResultString;
                ObjectCountingAggregatedResult result = JSonUtils.ConvertJSonToObject<ObjectCountingAggregatedResult>(resultString);

                
                aggResultCounts.Add(result.Count);
                GroundTruthCounts.Add(GroundtruthCount);
                abs_errors.Add(Math.Abs(GroundtruthCount - result.Count));
                errors.Add(GroundtruthCount - result.Count);
            }
            totalError = abs_errors.Sum();
            totalGroundTruth = GroundTruthCounts.Sum();
            double totalErrorRatio = totalError / totalGroundTruth;

            Console.WriteLine("Error: {0} / {1} = {2}", totalError, totalGroundTruth, totalErrorRatio);
            Console.WriteLine("Total Aggregated {0}", aggResultEntries.Count);
        }
    }
}