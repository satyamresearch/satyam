using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using SQLTables;

namespace SQLTableManagement
{
    public static class SatyamTaskTableManagement
    {
        //remove all tasks that have already been aggregated from the task table
        public static void PurgeAllAggregatedTasks()
        {
            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
            //List<int> IDList = taskDB.getAllIDs();
            List<SatyamTaskTableEntry> taskList = taskDB.getAllEntries();

            SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
            List<int> AggIDList = aggDB.getAllTaskIDs();
            //List<SatyamAggregatedResultsTableEntry> aggEntreis = aggDB.getAllEntries();

            foreach(SatyamTaskTableEntry t in taskList)
            {
                int id = t.ID;
                if (!AggIDList.Contains(id)) continue;

                //int LatestNumberAggregated = aggDB.getLatestNoResultsAggregatedByTaskID(id);
                //int MinResults = TaskConstants.getMinResultsByTemplate(t.JobTemplateType);
                
                //if(LatestNumberAggregated >= MinResults)
                //{
                    taskDB.DeleteEntry(id);
                //}
            }
            aggDB.close();
            taskDB.close();
        }


        //public static void PurgeAllAggregatedTasks()
        //{
        //    SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
        //    List<int> IDList = taskDB.getAllIDs();

        //    SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
        //    List<int> AggIDList = aggDB.getAllTaskIDs();

        //    foreach (int id in IDList)
        //    {
        //        if (AggIDList.Contains(id))
        //        {
        //            taskDB.DeleteEntry(id);
        //        }
        //    }
        //    aggDB.close();
        //    taskDB.close();
        //}
        public static void UpdateResultNumber()
        {
            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
            List<int> IDList = taskDB.getAllIDs();
            taskDB.close();

            foreach (int id in IDList)
            {
                UpdateResultNumber(id);
            }            
        }

        public static void UpdateResultNumber(int taskID)
        {
            SatyamResultsTableAccess resultdb = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> res = resultdb.getEntriesByTaskID(taskID);
            resultdb.close();
            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
            taskDB.UpdateResultNumber(taskID, res.Count);
            taskDB.close();
        }


        
        
    }
}
