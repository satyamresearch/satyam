using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SQLTableManagement;
using SQLTables;

namespace SatyamDispatch
{
    public static class PurgeAggregatedTasks
    {
        [FunctionName("PurgeAggregatedTasks")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {

            DateTime start = DateTime.Now;
            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
            List<int> IDList = taskDB.getAllIDs();
            taskDB.close();

            SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
            List<int> AggIDList = aggDB.getAllTaskIDs();
            aggDB.close();

            foreach (int id in IDList)
            {
                if (AggIDList.Contains(id))
                {
                    taskDB = new SatyamTaskTableAccess();
                    taskDB.DeleteEntry(id);
                    taskDB.close();
                    if ((DateTime.Now - start).TotalSeconds > 280) break;
                }
            }
            
            
        }
    }
}
