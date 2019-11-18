using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SatyamResultsSaving;
using SQLTables;
using SatyamTaskGenerators;
using Constants;
using SatyamTaskResultClasses;
using Utilities;
using JobTemplateClasses;

namespace SQLTableManagement
{
    public static class SatyamJobSubmissionsTableManagement
    {
        //goes through the jobDB and finds submitted jobs
        //uses the TaskGenerator to populate the taskTable


        //public static string processSubmittedJobsDebug()
        //{
        //    string debugString = "Entered the Function\n";
            
        //    SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
        //    List<SatyamJobSubmissionsTableAccessEntry> jobEntries = jobDB.getAllEntriesByStatus(JobStatus.submitted);

        //    debugString += "got the job entries, there are " + jobEntries.Count + "\n";
            

        //    if (jobEntries.Count == 0)
        //    {
        //        debugString += "No new Entries in the DB\n";
        //        return debugString;
        //    }

        //    int noEntries = 0;

        //    foreach (SatyamJobSubmissionsTableAccessEntry entry in jobEntries)
        //    {
        //        debugString += "About To Populate Entry " + noEntries + "\n";
                
        //        //SatyamTaskGenerator.PopulateTasksIntoTaskTable(entry);
        //        debugString += SatyamTaskGenerator.PopulateTasksIntoTaskTableDebug(entry);

        //        return debugString;

        //        if (TaskConstants.MTurkTaskTemplates.Contains(entry.JobTemplateType))
        //        {
        //            jobDB.UpdateEntryStatus(entry.JobGUID, JobStatus.ready);
        //        }
        //        else
        //        {
        //            jobDB.UpdateEntryStatus(entry.JobGUID, JobStatus.launched);
        //        }
        //        noEntries++;
        //    }
        //    return debugString;
        //}


        public static void preprocessSubmittedData()
        {
            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            List<SatyamJobSubmissionsTableAccessEntry> jobEntries = jobDB.getAllEntriesByStatus(JobStatus.submitted);
            

            foreach (SatyamJobSubmissionsTableAccessEntry entry in jobEntries)
            {
                if (SatyamTaskGenerator.PreprocessSubmittedData(entry))
                {
                    jobDB.UpdateEntryStatus(entry.JobGUID, JobStatus.preprocessed);
                }
            }
            jobDB.close();
        }

        public static void processPreprocessedJobs()
        {
            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            List<SatyamJobSubmissionsTableAccessEntry> jobEntries = jobDB.getAllEntriesByStatus(JobStatus.preprocessed);

           

            foreach (SatyamJobSubmissionsTableAccessEntry entry in jobEntries)
            {
                SatyamTaskGenerator.PopulateTasksIntoTaskTable(entry);
                if (TaskConstants.MTurkTaskTemplates.Contains(entry.JobTemplateType))
                {
                    jobDB.UpdateEntryStatus(entry.JobGUID, JobStatus.ready);
                }
                else
                {
                    jobDB.UpdateEntryStatus(entry.JobGUID, JobStatus.launched);
                }
            }
            jobDB.close();
        }

        //ready to Launched status change happens by the AmazonHITTableManager


        //Completed tasks are removed from the TaskTable
        //Thus, we check if tasks are still pernding
        //if not tasks for a GUID are pending its deemed as complete
        //then save the results and chnage status to completed
        public static void processLaunchedJobs()
        {
            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            List<string> guidList = jobDB.getAllJobGUIDSByStatus(JobStatus.launched);
            List<string> readyList = jobDB.getAllJobGUIDSByStatus(JobStatus.ready);
            guidList.AddRange(readyList);
            

            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
            foreach(String GUID in guidList)
            {
                if(GUID == "SINGLE_OBJECT_LABLING_DEMO")
                {
                    continue;
                }
                List<int> IDList = taskDB.getAllIDsByGUID(GUID);
                if(IDList.Count == 0)
                {
                    SatyamSaveResults.SaveByGUIDRequester(GUID);
                    SatyamSaveResults.SaveByGUIDSatyam(GUID);
                    SatyamSaveAggregatedResult.SaveByGUIDRequester(GUID);
                    SatyamSaveAggregatedResult.SaveByGUIDSatyam(GUID);
                    jobDB.UpdateEntryStatus(GUID, JobStatus.completed);
                }
            }

            jobDB.close();
            taskDB.close();
        }


        // If the results were not good enough, or aggregation method changed, retargeted, users might want to reopen the job
        // All we need to do: (IN STRICT ORDER)

        //      clear all results back to inconclusive,
        //      remove all aggregated results
        //      restore the task table as it was for the guid,
        //      change the job status back to launched, and 
        // WARNING:  
        //      for safety, this has to be run atomically synchronously, without any parallel process. for now.
        public static void reopenJobForMoreResults(string guid)
        {
            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            SatyamJobSubmissionsTableAccessEntry jobEntry = jobDB.getEntryByJobGIUD(guid);
            jobDB.close();

            if (jobEntry.JobStatus != JobStatus.completed)
            {
                Console.WriteLine("Job not completed yet!");
                return;
            }

            SatyamAggregatedResultsTableAccess aggResultDB = new SatyamAggregatedResultsTableAccess();
            bool delSuccess = aggResultDB.DeleteEntriesByGUID(guid);
            aggResultDB.close();
            if (!delSuccess)
            {
                Console.WriteLine("Delete Agg Result DB Failed");
                return;
            }

            SatyamResultsTableAccess resultDB = new SatyamResultsTableAccess();
            
            if (!resultDB.UpdateStatusByGUID(guid, ResultStatus.inconclusive))
            {
                Console.WriteLine("Update Result DB Failed");
                //resultDB.close();
                //return;
            }

            
            List<SatyamResultsTableEntry> results = resultDB.getEntriesByGUID(guid);
            resultDB.close();
            Dictionary<int, SatyamTask> taskParamsByTaskID = new Dictionary<int, SatyamTask>();
            foreach(SatyamResultsTableEntry  result in results)
            {
                if (taskParamsByTaskID.ContainsKey(result.SatyamTaskTableEntryID)) continue;

                SatyamResult satyamRes = JSonUtils.ConvertJSonToObject<SatyamResult>(result.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamRes.TaskParametersString);
                taskParamsByTaskID.Add(result.SatyamTaskTableEntryID, task);   
            }

            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();

            foreach (int taskID in taskParamsByTaskID.Keys)
            {
                SatyamTask task = taskParamsByTaskID[taskID];
                SatyamJob job = task.jobEntry;
                bool suc = taskDB.AddEntryWithSpecificID(taskID, job.JobTemplateType, job.UserID, job.JobGUIDString, JSonUtils.ConvertObjectToJSon(task), 
                    job.JobSubmitTime, job.amazonHITInformation.Price);
                if (!suc)
                {
                    Console.WriteLine("Update Task Table Failed");                    
                    taskDB.close();
                    return;
                }
            }
            taskDB.close();

            jobDB = new SatyamJobSubmissionsTableAccess();
            bool success = jobDB.UpdateEntryStatus(guid, JobStatus.launched);
            jobDB.close();
            if (!success)
            {
                Console.WriteLine("Update Job Entry Failed");
                return;
            }
        }
       
    }
}
