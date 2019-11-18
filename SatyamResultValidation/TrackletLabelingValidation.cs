using AzureBlobStorage;
using Constants;
using JobTemplateClasses;
using SatyamAnalysis;
using SatyamResultAggregators;
using SatyamResultClasses;
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
using static SatyamResultAggregators.TrackletLabelingAggregator;

namespace SatyamResultValidation
{
    public class TrackletLabelingValidation
    {


        public static void AggregateWithParameter(string guid,
            int MinResults = TaskConstants.TRACKLET_LABELING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.TRACKLET_LABELING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double boxToleranceThreshold = TaskConstants.TRACKLET_LABELING_BOX_DEVIATION_THRESHOLD,
            double ObjectCoverageApprovalThresholdPerVideo = TaskConstants.TRACKLET_LABELING_APPROVALRATIO_PER_VIDEO,
            double BoxCoverageApprovalThresholdPerTrack = TaskConstants.TRACKLET_LABELING_APPROVALRATIO_PER_TRACK,
            int consensusNumber = TaskConstants.TRACKLET_LABELING_MIN_RESULTS_FOR_CONSENSUS,
            double minTubeletIoUSimilarityThreshold = TaskConstants.TRACKLET_LABELING_MIN_TUBELET_SIMILARITY_THRESHOLD,
            double attributeMajority = TaskConstants.TRACKLET_LABELING_MTURK_ATTRIBUTE_MAJORITY_THRESHOLD,
            bool allFalseAttributeInvalid = false
            )
        {

            string configString = "Min_" + MinResults + "_Max_" + MaxResults + "_IoU_" + minTubeletIoUSimilarityThreshold + "_Ratio_" + ObjectCoverageApprovalThresholdPerVideo;
            Console.WriteLine("Aggregating for " + guid + " with param set " + configString);
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();

            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime = SatyamResultValidationToolKit.SortResultsBySubmitTime_OneResultPerTurkerPerTask(entries);

            Dictionary<int, List<MultiObjectTrackingResult>> ResultsPerTask = new Dictionary<int, List<MultiObjectTrackingResult>>();
            List<int> aggregatedTasks = new List<int>();

            int noTotalConverged = 0;
            //int noCorrect = 0;
            int noTerminatedTasks = 0;

            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();

            Dictionary<int, int> noResultsNeededForAggregation = SatyamResultsAnalysis.getNoResultsNeededForAggregationFromLog(configString, guid);
            Dictionary<int, int> noResultsNeededForAggregation_new = new Dictionary<int, int>();
            // play back by time
            Dictionary<int, List<string>> WorkersPerTask = new Dictionary<int, List<string>>();
            foreach (DateTime t in entriesBySubmitTime.Keys)
            {
                //Console.WriteLine("Processing Results of time: {0}", t);
                List<SatyamResultsTableEntry> ResultEntries = entriesBySubmitTime[t];
                foreach (SatyamResultsTableEntry entry in ResultEntries)
                {
                    SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                    MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(task.jobEntry.JobParameters);
                    string fileName = URIUtilities.filenameFromURINoExtension(task.SatyamURI);
                    int taskEntryID = entry.SatyamTaskTableEntryID;
                    if (aggregatedTasks.Contains(taskEntryID))
                    {
                        continue;
                    }
                    if (!ResultsPerTask.ContainsKey(taskEntryID))
                    {
                        ResultsPerTask.Add(taskEntryID, new List<MultiObjectTrackingResult>());
                        WorkersPerTask.Add(taskEntryID, new List<string>());
                    }

                    // remove duplicate workers result
                    string workerID = satyamResult.amazonInfo.WorkerID;
                    if (WorkersPerTask[taskEntryID].Contains(workerID))
                    {
                        continue;
                    }
                    //enclose only non-duplicate results, one per each worker.
                    WorkersPerTask[taskEntryID].Add(workerID);



                    string videoName = URIUtilities.localDirectoryNameFromURI(task.SatyamURI);
                    string[] fields = videoName.Split('_');
                    int startingFrame = Convert.ToInt32(fields[fields.Length - 1]);
                    int maxChunkEndFrame = startingFrame + job.ChunkDuration * job.FrameRate;
                    int noFrameOverlap = (int)(job.ChunkOverlap * job.FrameRate);
                    if (startingFrame != 0)
                    {
                        startingFrame -= noFrameOverlap;
                    }




                    string blobDir = URIUtilities.localDirectoryFullPathFromURI(task.SatyamURI);
                    VATIC_DVA_CrowdsourcedResult taskr = TrackletLabelingAggregator.createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(satyamResult.TaskResult, blobDir, entry.SatyamTaskTableEntryID.ToString(), job.FrameRate);
                    //MultiObjectTrackingResult result_tmp = vatic_tmp.getCompressedTracksInTimeSegment();

                    //VATIC_DVA_CrowdsourcedResult taskr = new VATIC_DVA_CrowdsourcedResult(satyamResult.TaskResult, videoName, entry.ID.ToString(), ImageURLs.Count, job.FrameRate);                    
                    MultiObjectTrackingResult res = taskr.getCompressedTracksInTimeSegment();

                    if (allFalseAttributeInvalid)
                    {
                        if (TrackletLabelingAggregator.AllAttributeAllFalse(res)) continue;
                    }

                    ResultsPerTask[taskEntryID].Add(res);

                    // check log if enough results are collected

                    if (noResultsNeededForAggregation != null && noResultsNeededForAggregation.ContainsKey(taskEntryID)
                        && ResultsPerTask[taskEntryID].Count < noResultsNeededForAggregation[taskEntryID])
                    {
                        continue;
                    }

                    


                    // hack for masters 
                    int tempMin = MinResults;
                    if (TaskConstants.masterGUIDs.Contains(entry.JobGUID))
                    {
                        tempMin = 1;
                    }

                    TrackletLabelingAggregatedResult aggResult = TrackletLabelingAggregator.getAggregatedResult(ResultsPerTask[taskEntryID],
                        guid, task.SatyamURI,
                        job.FrameRate, job.BoundaryLines, 
                        MinResults, MaxResults, boxToleranceThreshold,
                        ObjectCoverageApprovalThresholdPerVideo,
                        BoxCoverageApprovalThresholdPerTrack,
                        consensusNumber,minTubeletIoUSimilarityThreshold,
                        attributeMajority);

                    if (aggResult == null)
                    {
                        continue;
                    }

                    //////////////// aggregation happen
                    // record logs
                    if (noResultsNeededForAggregation == null || !noResultsNeededForAggregation.ContainsKey(taskEntryID))
                    {
                        noResultsNeededForAggregation_new.Add(taskEntryID, ResultsPerTask[taskEntryID].Count);
                    }



                    aggregatedTasks.Add(taskEntryID);
                    noTotalConverged++;
                    if (ResultsPerTask[taskEntryID].Count >= MaxResults)
                    {
                        noTerminatedTasks++;
                    }
                    SatyamAggregatedResult SatyamAggResult = new SatyamAggregatedResult();
                    SatyamAggResult.SatyamTaskTableEntryID = taskEntryID;
                    SatyamAggResult.AggregatedResultString = JSonUtils.ConvertObjectToJSon<TrackletLabelingAggregatedResult>(aggResult);
                    SatyamAggResult.TaskParameters = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString).TaskParametersString;

                    SatyamAggregatedResultsTableEntry aggEntry = new SatyamAggregatedResultsTableEntry();
                    aggEntry.SatyamTaskTableEntryID = taskEntryID;
                    aggEntry.JobGUID = entry.JobGUID;
                    aggEntry.ResultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(SatyamAggResult);


                    aggEntries.Add(aggEntry);                   
                }
            }


            Console.WriteLine("Total_Aggregated_Tasks: {0}", noTotalConverged);
            Console.WriteLine("Total_Terminated_Tasks: {0}", noTerminatedTasks);

            SatyamResultsAnalysis.RecordAggregationLog(noResultsNeededForAggregation_new, configString, guid);

            TrackletLabelingAnalyzer.GroupEntriesByVideoNameAndStitchAndSaveAggregatedResultVideosLocally(aggEntries, DirectoryConstants.defaultTempDirectory + guid);

        }
    }
}
