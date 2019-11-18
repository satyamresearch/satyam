using System;
using System.Collections.Generic;
using AzureBlobStorage;
using Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SatyamResultAggregators;
using SatyamTaskResultClasses;
using SQLTableManagement;
using SQLTables;
using Utilities;

namespace SatyamDispatch
{
    public static class JudgeResultDispatch
    {
        [FunctionName("JudgeResultDispatch")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            DateTime start = DateTime.Now;
            bool logging = false;
            if (logging) log.Info($"Judge Result Dispatch executed at: {DateTime.Now}");
            //get all inconclusive results and group them by taskIDs
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> results = resultsDB.getAllEntriesByStatus(ResultStatus.inconclusive);
            resultsDB.close();

            if (results.Count == 0)
            {                
                return;
            }

            Dictionary<int, List<SatyamResultsTableEntry>> resultsByTaskID = new Dictionary<int, List<SatyamResultsTableEntry>>();
            List<string> pendingGUID = new List<string>();
            foreach (SatyamResultsTableEntry result in results)
            {
                if (!resultsByTaskID.ContainsKey(result.SatyamTaskTableEntryID))
                {
                    resultsByTaskID.Add(result.SatyamTaskTableEntryID, new List<SatyamResultsTableEntry>());
                }
                resultsByTaskID[result.SatyamTaskTableEntryID].Add(result);

                if (!pendingGUID.Contains(result.JobGUID))
                {
                    pendingGUID.Add(result.JobGUID);
                }
            }

            //now check against the aggregated result to see if the result is aceptable
            SatyamDispatchStorageAccountAccess satyamQueue = new SatyamDispatchStorageAccountAccess();

            Dictionary<string, Dictionary<int, SatyamAggregatedResultsTableEntry>> aggEntriesPerTaskPerGUID = SatyamAggregatedResultManagement.getAggregatedEntriesPerTaskByGuidList(pendingGUID);

            foreach (KeyValuePair<int, List<SatyamResultsTableEntry>> entry in resultsByTaskID)
            {
                if (entry.Value.Count == 0) continue;
                int taskEntryID = entry.Key;
                string taskGUID = entry.Value[0].JobGUID;
                if (aggEntriesPerTaskPerGUID.ContainsKey(taskGUID) && aggEntriesPerTaskPerGUID[taskGUID].ContainsKey(taskEntryID))
                {
                    // this task has been aggregated
                    foreach (SatyamResultsTableEntry result in resultsByTaskID[taskEntryID]) //now got through each task to see if they satify pass criterion
                    {
                        if (logging) log.Info($"Dispatching Judgement for {taskEntryID}");
                        string queueName = "judge-result";
                        string m = taskGUID + "_" + taskEntryID + "_" + result.ID;
                        satyamQueue.Enqueue(queueName, m);
                        if ((DateTime.Now - start).TotalSeconds > 280) break;
                    }
                }
                if ((DateTime.Now - start).TotalSeconds > 280) break;
            }

            if (logging) log.Info($"Judge Result Dispatch finished at: {DateTime.Now}");
        }
    }
}
