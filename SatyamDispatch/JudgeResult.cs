using System;
using System.Collections.Generic;
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
    public static class JudgeResult
    {
        [FunctionName("JudgeResult")]
        public static void Run([QueueTrigger("judge-result")]string myQueueItem, TraceWriter log)
        {
            bool logging = false;
            string[] fields = myQueueItem.Split('_');
            string guid = fields[0];
            int taskID = Convert.ToInt32(fields[1]);
            string resultID = fields[2];
            if (logging) log.Info($"Judge Result: {myQueueItem}");

            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> results = resultsDB.getEntriesID(resultID);
            resultsDB.close();
            if (results.Count == 0) return;

            SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
            SatyamAggregatedResultsTableEntry  aggResult = aggDB.getEntryByTaskID(taskID);
            aggDB.close();
            if (aggResult == null) return;

            SatyamResultsTableEntry result = results[0];
            SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(result.ResultString);
            string taskTemplateType = result.JobTemplateType;
            string workerID = res.amazonInfo.WorkerID;
            DateTime doneTime = res.TaskEndTime;
            if (AcceptanceCriterionChecker.IsAcceptable(aggResult, result))
            {
                resultsDB = new SatyamResultsTableAccess();
                resultsDB.UpdateStatusByID(result.ID, ResultStatus.accepted);
                resultsDB.close();
                WorkerStatisticsManagement.UpdateWorkerStatistics(workerID, taskTemplateType, true, doneTime);
                if (logging) log.Info($"Accepted");
            }
            else
            {
                resultsDB = new SatyamResultsTableAccess();
                resultsDB.UpdateStatusByID(result.ID, ResultStatus.rejected);
                resultsDB.close();
                WorkerStatisticsManagement.UpdateWorkerStatistics(workerID, taskTemplateType, false, doneTime);
                if (logging) log.Info($"Rejected");
            }
        }
    }
}
