using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLTables;
using Constants;
using SatyamTaskGenerators;
using SatyamResultAggregators;
using Utilities;
using SatyamTaskResultClasses;

namespace SQLTableManagement
{
    public static class SatyamResultsTableManagement
    {

        //go through all the inconclusive results and aggregate them if they are not aggregated already
        public static void AggregateResults()
        {
            //first get all the results that are not aggregated
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> results = resultsDB.getEntriesByStatus(ResultStatus.inconclusive);

            if(results.Count == 0) //there is nothing to do
            {
                resultsDB.close();
                return;
            }

            //first group all the results by the tasks since wach task may have multiple entries
            Dictionary<int, List<SatyamResultsTableEntry>> collectedResults = new Dictionary<int, List<SatyamResultsTableEntry>>();
            List<string> pendingGUID = new List<string>();
            foreach (SatyamResultsTableEntry entry in results)
            {
                int taskID = entry.SatyamTaskTableEntryID;
                if (!collectedResults.ContainsKey(taskID))
                {
                    collectedResults.Add(taskID, new List<SatyamResultsTableEntry>());
                }
                collectedResults[taskID].Add(entry);

                if (!pendingGUID.Contains(entry.JobGUID))
                {
                    pendingGUID.Add(entry.JobGUID);
                }
            }
            List<int> taskIDList = collectedResults.Keys.ToList();
            taskIDList.Sort();
            //check if guid is still pending
            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            List<string> completedGUIDs = jobDB.getAllJobGUIDSByStatus(JobStatus.completed);
            jobDB.close();
            
            //check if this taskID has already been aggregated and exists int he aggregationDB
            Dictionary<string, Dictionary<int, SatyamAggregatedResultsTableEntry>> ExistingAggEntriesPerTaskPerGUID = SatyamAggregatedResultManagement.getAggregatedEntriesPerTaskByGuidList(pendingGUID);

            Console.WriteLine("Aggregating {0} Tasks", taskIDList.Count);
            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();
            //foreach (int taskId in taskIDList)
            for (int i=0;i<taskIDList.Count;i++)
            {
                //int taskId = taskIDList[taskIDList.Count - i - 1];
                int taskId = taskIDList[i];
                string guid = collectedResults[taskId][0].JobGUID;
                if (completedGUIDs.Contains(guid))
                {
                    // Hit Completed, mark results outdated.
                    resultsDB.UpdateStatusByTaskID(taskId, ResultStatus.outdated);
                    continue;
                }

                //SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
                //int LatestResultsAggregated = aggDB.getLatestNoResultsAggregatedByTaskID(taskId);
                //aggDB.close();

                if (ExistingAggEntriesPerTaskPerGUID.ContainsKey(guid) && ExistingAggEntriesPerTaskPerGUID[guid].ContainsKey(taskId))
                {
                    continue;
                    //int MinResults = TaskConstants.getMinResultsByTemplate(ExistingAggEntriesPerTaskPerGUID[guid][taskId].JobTemplateType);
                    //if (LatestResultsAggregated >= MinResults)
                    //{
                    //    continue;
                    //}
                    // already aggregated to MinResult Request, but leftover results will be judged. So do nothing
                }
                
                //if it does not exist only then aggregate
                Console.WriteLine("Task Type: {2}({3}), Task ID: {0}, # of Results: {1}", taskId, collectedResults[taskId].Count, collectedResults[taskId][0].JobTemplateType, collectedResults[taskId][0].JobGUID);


                //if (collectedResults[taskId].Count != LatestResultsAggregated)
                //{
                    SatyamAggregatedResultsTableEntry aggEntry = ResultsTableAggregator.GetAggregatedResultString(taskId, collectedResults[taskId]);
                    if (aggEntry != null)
                    {
                        //aggEntries.Add(aggEntry);
                        SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
                        aggDB.AddEntry(aggEntry, collectedResults[taskId].Count);
                        aggDB.close();
                    }
                //}
                //else
                //{
                //    Console.WriteLine("No New Results");
                //}
            }
            

            resultsDB.close();
            
        }

        //go through all the inconclusive results and check with aggregated table, if aggreagted, compare and see if it is acceptable and then 
        //accept/reject
        public static void AcceptRejectResults()
        {

            //get all inconclusive results and group them by taskIDs
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();

            List<SatyamResultsTableEntry> results = resultsDB.getAllEntriesByStatus(ResultStatus.inconclusive);

            if(results.Count == 0)
            {
                resultsDB.close();
                return;
            }

            Dictionary<int, List<SatyamResultsTableEntry>> resultsByTaskID = new Dictionary<int, List<SatyamResultsTableEntry>>();
            List<string> pendingGUID = new List<string>();
            foreach(SatyamResultsTableEntry result in results)
            {
                if(!resultsByTaskID.ContainsKey(result.SatyamTaskTableEntryID))
                {
                    resultsByTaskID.Add(result.SatyamTaskTableEntryID,new List<SatyamResultsTableEntry>());
                }
                resultsByTaskID[result.SatyamTaskTableEntryID].Add(result);

                if (!pendingGUID.Contains(result.JobGUID))
                {
                    pendingGUID.Add(result.JobGUID);
                }
            }

            //now check against the aggregated result to see if the result is aceptable

            Dictionary<string, Dictionary<int, SatyamAggregatedResultsTableEntry>> aggEntriesPerTaskPerGUID = SatyamAggregatedResultManagement.getAggregatedEntriesPerTaskByGuidList(pendingGUID);

            foreach(KeyValuePair<int, List<SatyamResultsTableEntry>> entry in resultsByTaskID)
            {
                if (entry.Value.Count == 0) continue;
                int taskEntryID = entry.Key;
                string taskGUID = entry.Value[0].JobGUID;
                if (aggEntriesPerTaskPerGUID.ContainsKey(taskGUID) && aggEntriesPerTaskPerGUID[taskGUID].ContainsKey(taskEntryID))
                {
                    // this task has been aggregated
                    foreach(SatyamResultsTableEntry result in resultsByTaskID[taskEntryID]) //now got through each task to see if they satify pass criterion
                    {
                        SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(result.ResultString);
                        string taskTemplateType = result.JobTemplateType;
                        string workerID = res.amazonInfo.WorkerID;
                        DateTime doneTime = res.TaskEndTime;

                        if (AcceptanceCriterionChecker.IsAcceptable(aggEntriesPerTaskPerGUID[taskGUID][taskEntryID], result))
                        {
                            resultsDB.UpdateStatusByID(result.ID,ResultStatus.accepted);
                            WorkerStatisticsManagement.UpdateWorkerStatistics(workerID, taskTemplateType, true, doneTime);
                        }
                        else
                        {
                            resultsDB.UpdateStatusByID(result.ID, ResultStatus.rejected);
                            WorkerStatisticsManagement.UpdateWorkerStatistics(workerID, taskTemplateType, false, doneTime);
                        }
                    }
                }
            }
            resultsDB.close();
        }


        
    }
}
