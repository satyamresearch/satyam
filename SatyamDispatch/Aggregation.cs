using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SatyamResultAggregators;
using SQLTables;

namespace SatyamDispatch
{
    public static class Aggregation
    {
        [FunctionName("Aggregation")]
        public static void Run([QueueTrigger("aggregation")]string guidAndTaskID, TraceWriter log)
        {
            bool logging = false;
            string[] fields = guidAndTaskID.Split('_');
            string guid = fields[0];
            int taskID = Convert.ToInt32(fields[1]);

            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> collectedResults = resultsDB.getEntriesByGUIDAndTaskID(guid, taskID);
            resultsDB.close();

            if (collectedResults.Count == 0) return;

            if (logging) log.Info($"Task Type: {collectedResults[0].JobTemplateType}({guid}), Task ID: {taskID}, # of Results: {collectedResults.Count}");

            //SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
            //int LatestResultsAggregated = aggDB.getLatestNoResultsAggregatedByTaskID(taskID);
            //aggDB.close();

            //if (collectedResults.Count != LatestResultsAggregated)
            //{
                SatyamAggregatedResultsTableEntry aggEntry = ResultsTableAggregator.GetAggregatedResultString(taskID, collectedResults);
                if (aggEntry != null)
                {
                //aggEntries.Add(aggEntry);
                    SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
                    aggDB.AddEntry(aggEntry, collectedResults.Count);
                    aggDB.close();
                    if (logging) log.Info($"Aggregated");
                }
            //}

        }
    }
}
