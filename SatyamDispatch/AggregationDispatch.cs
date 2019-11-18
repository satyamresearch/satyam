using System;
using System.Collections.Generic;
using System.Linq;
using AzureBlobStorage;
using Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SatyamResultAggregators;
using SQLTableManagement;
using SQLTables;

namespace SatyamDispatch
{
    public static class AggregationDispatch
    {
        [FunctionName("AggregationDispatch")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            DateTime start = DateTime.Now;
            bool logging = false;
            if (logging) log.Info($"Aggregation Dispatch executed at: {DateTime.Now}");

            
            //first get all the results that are not aggregated
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> results = resultsDB.getEntriesByStatus(ResultStatus.inconclusive);
            resultsDB.close();

            if (results.Count == 0) //there is nothing to do
            {   
                return;
            }

            //first group all the results by the guids, and then by tasks since wach task may have multiple entries
            // organize by guid, so when polling,  each guid will get a chance.
            SortedDictionary<string, SortedDictionary<int, List<SatyamResultsTableEntry>>> ResultsByGUID = new SortedDictionary<string, SortedDictionary<int, List<SatyamResultsTableEntry>>>();
            List<string> pendingGUID = new List<string>();
            foreach (SatyamResultsTableEntry entry in results)
            {
                string guid = entry.JobGUID;
                if (!ResultsByGUID.ContainsKey(guid))
                {
                    ResultsByGUID.Add(guid, new SortedDictionary<int, List<SatyamResultsTableEntry>>());
                }

                
                int taskID = entry.SatyamTaskTableEntryID;
                if (!ResultsByGUID[guid].ContainsKey(taskID))
                {
                    ResultsByGUID[guid].Add(taskID, new List<SatyamResultsTableEntry>());
                }
                ResultsByGUID[guid][taskID].Add(entry);

                if (!pendingGUID.Contains(entry.JobGUID))
                {
                    pendingGUID.Add(entry.JobGUID);
                }
            }
            List<string> guidList = ResultsByGUID.Keys.ToList();


            if (logging) log.Info($"Aggregation Dispatch: Results Collected at: {DateTime.Now}");

            //check if guid is still pending
            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            List<string> completedGUIDs = jobDB.getAllJobGUIDSByStatus(JobStatus.completed);
            jobDB.close();

            //check if this taskID has already been aggregated and exists int he aggregationDB
            Dictionary<string, Dictionary<int, SatyamAggregatedResultsTableEntry>> ExistingAggEntriesPerTaskPerGUID = SatyamAggregatedResultManagement.getAggregatedEntriesPerTaskByGuidList(pendingGUID);

            //Console.WriteLine("Aggregating {0} Tasks", taskIDList.Count);
            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();


            SatyamDispatchStorageAccountAccess satyamQueue = new SatyamDispatchStorageAccountAccess();
            int i = -1;
            int count = 0;
            while (true)
            {
                i++;
                bool Done = true;
                for (int j = 0; j < guidList.Count; j++)
                {
                    string guid = guidList[j];
                    List<int> taskIDList = ResultsByGUID[guid].Keys.ToList();
                    if (taskIDList.Count <= i)
                    {
                        continue;
                    }
                    Done = false;
                    int taskId = taskIDList[i];
                    if (completedGUIDs.Contains(guid))
                    {
                        // Hit Completed, mark results outdated.
                        resultsDB = new SatyamResultsTableAccess();
                        resultsDB.UpdateStatusByTaskID(taskId, ResultStatus.outdated);
                        resultsDB.close();
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
                        // already aggregated to MinResult request, but leftover results will be judged. So do nothing
                    }
                    
                    //if it does not exist only then aggregate
                    if (logging) log.Info($"{(DateTime.Now - start).TotalSeconds} Dispatching aggregation for guid {guid} task {taskId}");
                    string queueName = "aggregation";
                    string m = guid + "_" + taskId;
                    satyamQueue.Enqueue(queueName, m);
                    count++;
                    
                }
                if (Done) break;
                // emergency break
                if ((DateTime.Now - start).TotalSeconds > 280) break;
            }

            if (logging) log.Info($"Aggregation Dispatch finished at: {DateTime.Now}, dispatched {count} aggregations");
        }
    }
}
