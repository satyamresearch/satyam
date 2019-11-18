using AmazonMechanicalTurkAPI;
using Constants;
using JobTemplateClasses;
using SatyamAnalysis;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace SQLTableManagement
{
    public static class AmazonHITManagement
    {
        //public static Dictionary<string, int> getRemainingHITsFinishedTasksFromResultDB(string guid, int taskPerHIT)
        //{
        //    SatyamResultsTableAccess resultDB = new SatyamResultsTableAccess();
        //    List<SatyamResultsTableEntry>  entries = resultDB.getEntriesByGUID(guid);
        //    Dictionary<string, int> noResultPerHITID = new Dictionary<string, int>();
        //    //int noFinishedHIT = 0;
        //    foreach (SatyamResultsTableEntry entry in entries)
        //    {
        //        string HITId = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString).amazonInfo.HITID;
        //        if (!noResultPerHITID.ContainsKey(HITId))
        //        {
        //            noResultPerHITID.Add(HITId,1);
        //        }
        //        else
        //        {
        //            noResultPerHITID[HITId]++;
        //            //if (noResultPerHITID[HITId] == taskPerHIT)
        //            //{
        //            //    noFinishedHIT++;
        //            //}
        //        }
        //    }
        //    return noResultPerHITID;
        //}
                
        //makes sure that there are enough HITS in Amazon to ensure that jobs will be done
        public static void LaunchAmazonHITsFromTaskTable(int max=int.MaxValue)
        {
            //Group them by jobguids since each job can have many tasks
            Dictionary<string, List<SatyamTaskTableEntry>> TasksByGUID = OrganizeMTurkTasksByGUID();


            if (TasksByGUID == null) //there is nothing to do!!
            {
                return;
            }

            


            //after running the aggregator, the number of tasks  in each guid <= no of HITS pending
            //so if there are less HITS pending we launch more and if there are more we leave them alone until the job is done 


            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();

            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();

            //now check how manyy hits are pending and launch to ensure that there are enough hits in amazon
            foreach (KeyValuePair<string, List<SatyamTaskTableEntry>> entry in TasksByGUID)
            {                
                String jobGUID = entry.Key;
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(entry.Value[0].TaskParametersString);
                SatyamJob job = task.jobEntry;
                
                //// filter task templates -- temporary filter
                //if (job.JobTemplateType == TaskConstants.Segmentation_Image_MTurk) continue;

                string AmazonAccessKeyID = job.amazonHITInformation.AmazonAccessKeyID;                 
                string AmazonSecretAccessKeyID = job.amazonHITInformation.AmazonSecretAccessKeyID;
                string JobTemplateType = entry.Value[0].JobTemplateType;
                Dictionary<string, Object> hitParams = new Dictionary<string, object>();
                hitParams.Add("Title", job.amazonHITInformation.AmazonMTurkTaskTitle);
                hitParams.Add("TaskDescription", job.amazonHITInformation.AmazonMTurkTaskDescription);
                hitParams.Add("KeyWords", job.amazonHITInformation.AmazonMTurkTaskKeywords);
                hitParams.Add("Reward",job.amazonHITInformation.Price); //convert cents to dollars

                if (TaskConstants.masterGUIDs.Contains(job.JobGUIDString))
                {
                    hitParams.Add("Masters", true);
                }


                int tasksPerHIT = job.TasksPerJob;
                List<SatyamAmazonHITTableAccessEntry> hitEntries = hitDB.getAllEntriesByJobGUIDAndStatus(jobGUID,HitStatus.pending);

                // the task will be marked as submitted when the webpage exit
                int noHITsRemaining = hitEntries.Count;

                ////////////////////////// check and deal with pending stragglers, the actual hit might have expired.
                //int noStragglers = 0;
                //foreach (SatyamAmazonHITTableAccessEntry pendingEntry in hitEntries)
                //{
                //    if ((DateTime.Now - pendingEntry.CreationTime).TotalHours > 1)
                //    {
                //        // the hit will be finished in 1hr if taken by a user, 
                //        // there is a higher chance that this hit have already been expired
                //        // launch more for stragglers
                //        noStragglers++;
                //    }
                //}
                //noHITsRemaining -= noStragglers;
                ///////////////////////////////

                //////////////////////////////////// the query amazon way
                //AmazonMTurkHIT hit = new AmazonMTurkHIT();
                //hit.setAccount(AmazonAccessKeyID, AmazonSecretAccessKeyID, false);

                ////an alternative without querying amazon
                ////Dictionary<string, int> noHITsFinishedTasks = getRemainingHITsFinishedTasksFromResultDB(jobGUID, tasksPerHIT);

                ////int noHITsRemaining = 0;
                //noHITsRemaining = 0;
                //int noPendingHITDone = 0;
                //foreach (SatyamAmazonHITTableAccessEntry hitEntry in hitEntries)
                //{
                //    int noTasksPending = hit.getNumTasksRemaining(hitEntry.HITID);
                //    noHITsRemaining += noTasksPending;
                //    //an alternative without querying amazon
                //    //if (noHITsFinishedTasks.ContainsKey(hitEntry.HITID) && noHITsFinishedTasks[hitEntry.HITID] == tasksPerHIT)
                //    //{
                //    //    //HIT done
                //    //    noPendingHITDone++;
                //    //}
                //}
                ////int noHITsRemainingFromResultDB = hitEntries.Count - noPendingHITDone;

                ////Console.WriteLine("Job {0}, HITS Remaining/fromDB/Pending/noTasks: {1}/{2}/{3}", 
                ////    jobGUID, noHITsRemaining, noHITsRemainingFromResultDB, hitEntries.Count, TasksByGUID[jobGUID].Count);

                ////if (noHITsRemainingFromResultDB!= noHITsRemaining)
                ////{
                ////    // not gonna work, what is the hit has less tasks than tasksPerHIT??
                ////    Console.WriteLine("Database method is wrong");
                ////}
                /////////////////////////////////////////////////////////


                int NoParallelTurker = 3; // too large will make the task stale on AMT, too small will hurt latency
                int difference = (int)Math.Ceiling((double)(TasksByGUID[jobGUID].Count * NoParallelTurker - noHITsRemaining * tasksPerHIT) / (double)tasksPerHIT);

                //int difference = 50;
                //difference *= 2; // boost for parallelism
                //difference = 10;

                //difference = Math.Min(difference, max) * NoParallelTurker;

                if (difference > 0)
                {
                    AmazonMTurkHITBatchLauncher hitLauncher = new AmazonMTurkHITBatchLauncher(AmazonAccessKeyID, AmazonSecretAccessKeyID);
                    string url = TaskConstants.JobTemplateTypeToTaskURI[JobTemplateType];
                    double reward = job.amazonHITInformation.Price;
                    url = url + "?reward=" + reward;
                    List<string> hitIDs = null;
                    string hitParametersString = JSonUtils.ConvertObjectToJSon<Dictionary<string, object>>(hitParams);
                    hitLauncher.LaunchBatches(url, hitParams, difference, out hitIDs);
                    if(hitIDs != null)
                    {
                        foreach(string hitID in hitIDs)
                        {
                            hitDB.AddEntry(JobTemplateType, job.UserID, hitID, job.JobGUIDString, DateTime.Now, hitParametersString, AmazonAccessKeyID, AmazonSecretAccessKeyID);
                        }
                        jobDB.UpdateEntryStatus(entry.Key, JobStatus.launched);
                    }
                }                
            }
            jobDB.close();
            hitDB.close();
        }

        public static List<SatyamResultsTableEntry> getAllEntriesByGUIDFilteredByHITID(string guid, string hitid)
        {
            List<SatyamResultsTableEntry> ret = new List<SatyamResultsTableEntry>();
            SatyamResultsTableAccess resDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> res = resDB.getEntriesByGUID(guid);
            resDB.close();
            foreach (SatyamResultsTableEntry r in res)
            {
                SatyamResult rr = JSonUtils.ConvertJSonToObject<SatyamResult>(r.ResultString);
                if (rr.amazonInfo.HITID == hitid)
                {
                    ret.Add(r);
                }
            }
            return ret;
        }

        /// <summary>
        /// safely delete a hit, following the hit life cycle
        /// </summary>
        public static Dictionary<string, List<SatyamResultsTableEntry>> resultCache = new Dictionary<string, List<SatyamResultsTableEntry>>();
        public static bool SafeDeleteHit(SatyamAmazonHITTableAccessEntry hitEntryToBeRemoved)
        {
            // first expire
            AmazonMTurkHIT hit = new AmazonMTurkHIT();
            hit.setAccount(hitEntryToBeRemoved.AmazonAccessKeyID, hitEntryToBeRemoved.AmazonSecretAccessKeyID, false);
            hit.expireHIT(hitEntryToBeRemoved.HITID);
            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();
            hitDB.UpdateStatus(hitEntryToBeRemoved.ID, HitStatus.expired);
            hitDB.close();

            // then try dispose
            if (!hit.DeleteHIT(hitEntryToBeRemoved.HITID))
            {
                // can't dispose due to invalid status: reviewable but payment not cleared, or disposed.
                // because payment not cleared
                // find all results of this hitID and mark for rejection
                if (!resultCache.ContainsKey(hitEntryToBeRemoved.JobGUID))
                {
                    List<SatyamResultsTableEntry> ret = new List<SatyamResultsTableEntry>();
                    SatyamResultsTableAccess resDB = new SatyamResultsTableAccess();
                    List<SatyamResultsTableEntry> tempres = resDB.getEntriesByGUID(hitEntryToBeRemoved.JobGUID);
                    resDB.close();
                    resultCache.Add(hitEntryToBeRemoved.JobGUID, tempres);
                }
                List<SatyamResultsTableEntry> results = resultCache[hitEntryToBeRemoved.JobGUID];
                List<SatyamResultsTableEntry> res = new List<SatyamResultsTableEntry>();
                foreach (SatyamResultsTableEntry r in results)
                {
                    SatyamResult rr = JSonUtils.ConvertJSonToObject<SatyamResult>(r.ResultString);
                    if (rr.amazonInfo.HITID == hitEntryToBeRemoved.HITID)
                    {
                        res.Add(r);
                    }
                }               

                if (res.Count == 0)
                {
                    // the hit can't be disposed because there are pending assignments, yet we don't have any on records, 
                    // so delete it. AWS will automatically handle it after 120 days of inactivity.
                    hitDB = new SatyamAmazonHITTableAccess();
                    hitDB.DeleteEntry(hitEntryToBeRemoved.ID);
                    hitDB.close();
                    Console.WriteLine("Delete Success\n");
                    return true;
                }

                //bool HITDone = true;
                foreach (SatyamResultsTableEntry r in res)
                {
                    //SatyamResult rr = JSonUtils.ConvertJSonToObject<SatyamResult>(r.ResultString);
                    //string assignmentID = rr.amazonInfo.AssignmentID;
                    //hit.RejectAssignment(assignmentID, "Sorry! Your work was not within acceptable parameters!");
                    if (r.Status == ResultStatus.inconclusive || r.Status==ResultStatus.outdated)
                    {
                        // reject all for now... TODO: check if there is an aggregated results.
                        //Console.WriteLine("Rejecting result ID {0}", r.ID);
                        //SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
                        //resultsDB.UpdateStatusByID(r.ID, ResultStatus.rejected);
                        //resultsDB.close();
                        //HITDone = false;
                    }
                    else
                    {
                        Console.WriteLine("Result ID {0} is of status {1}", r.ID, r.Status);

                        if (r.Status==ResultStatus.accepted || r.Status == ResultStatus.rejected)
                        {
                            // need to be handled by payment function
                            //HITDone = false;
                        }

                        // just in case last payment was not made successfully, 
                        // mark it to be paid again.... 
                        
                        SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
                        if (ResultStatus.acceptedStatusList.Contains(r.Status))
                        {
                            Console.WriteLine("Marking as Accepted so Next Cycle will Pay");
                            resultsDB.UpdateStatusByID(r.ID, ResultStatus.accepted);
                        }
                        if (ResultStatus.rejectedStatusList.Contains(r.Status))
                        {
                            Console.WriteLine("Marking as Rejected so Next Cycle will Reject");
                            resultsDB.UpdateStatusByID(r.ID, ResultStatus.rejected);
                        }
                        resultsDB.close();
                    }


                }
                
                // Commented out cuz the case where the inconsistency status happen is rare, 
                // so don't delete the hit too aggressively
                //if (HITDone)
                //{
                //    hitDB = new SatyamAmazonHITTableAccess();
                //    hitDB.DeleteEntry(hitEntryToBeRemoved.ID);
                //    hitDB.close();
                //    Console.WriteLine("Delete Success\n");
                //    return true;
                //}


                Console.WriteLine("Delete Failure\n");
                return false;
            }
            else
            {
                hitDB = new SatyamAmazonHITTableAccess();
                hitDB.DeleteEntry(hitEntryToBeRemoved.ID);
                hitDB.close();
                Console.WriteLine("Delete Success\n");
                return true;
            }
            
        }

        //makes sure that unneccesary HITS- those jobs that are completed are removed
        //go to HIT Table check if there any pending HITS for which the jobGUID does exist in the taskTable 
        public static void ClearHITSFromAmazon()
        {
            int succCount = 0;
            int failCount = 0;
            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();
            //first try and remove all the expired entries
            List<SatyamAmazonHITTableAccessEntry> expiredHitEntries = hitDB.getAllEntriesByStatus(HitStatus.expired);
            AmazonMTurkHIT hit = new AmazonMTurkHIT();

            foreach (SatyamAmazonHITTableAccessEntry expiredEntry in expiredHitEntries)
            {
                //hit.setAccount(expiredEntry.AmazonAccessKeyID, expiredEntry.AmazonSecretAccessKeyID, false);
                //if (hit.DeleteHIT(expiredEntry.HITID))
                //{
                //    hitDB.DeleteEntry(expiredEntry.ID);
                //}
                bool succ = SafeDeleteHit(expiredEntry);
                if (succ)
                {
                    succCount++;
                }
                else
                {
                    failCount++;
                }
                Console.WriteLine("Succ: {0}, Fail: {1}", succCount, failCount);
            }


            List<SatyamAmazonHITTableAccessEntry> hitEntriesPending = hitDB.getAllEntriesByStatus(HitStatus.pending);
            List<SatyamAmazonHITTableAccessEntry> hitEntriesSubmitted = hitDB.getAllEntriesByStatus(HitStatus.submitted);
            List<SatyamAmazonHITTableAccessEntry> hitEntriesAccepted = hitDB.getAllEntriesByStatus(HitStatus.accepted);
            List<SatyamAmazonHITTableAccessEntry> hitEntriesRejected = hitDB.getAllEntriesByStatus(HitStatus.rejected);
            List<SatyamAmazonHITTableAccessEntry> hitEntriesTaken = hitDB.getAllEntriesByStatus(HitStatus.taken);

            List<SatyamAmazonHITTableAccessEntry> hitEntries = new List<SatyamAmazonHITTableAccessEntry>();
            hitEntries.AddRange(hitEntriesPending);
            hitEntries.AddRange(hitEntriesSubmitted);
            hitEntries.AddRange(hitEntriesAccepted);
            hitEntries.AddRange(hitEntriesRejected);
            hitEntries.AddRange(hitEntriesTaken);

            hitDB.close();

            if (hitEntries.Count == 0)
            {
                
                return;
            }

            //Group them by jobguids since each job can have many tasks
            Dictionary<string, List<SatyamAmazonHITTableAccessEntry>> HITSByGUID = new Dictionary<string, List<SatyamAmazonHITTableAccessEntry>>();
            foreach(SatyamAmazonHITTableAccessEntry hitEntry in hitEntries)
            {
                // if launched for too long, auto expire
                if (hitEntry.EntryExpired())
                {
                    hitDB = new SatyamAmazonHITTableAccess();
                    hitDB.UpdateStatus(hitEntry.ID, HitStatus.expired);
                    hitDB.close();
                }

                // categorize by guid
                string guid = hitEntry.JobGUID;
                if (!HITSByGUID.ContainsKey(hitEntry.JobGUID))
                {
                    HITSByGUID.Add(hitEntry.JobGUID, new List<SatyamAmazonHITTableAccessEntry>());
                }
                HITSByGUID[guid].Add(hitEntry);
            }

            //now check if this guid has any entries in the task table
            List<string> guids = HITSByGUID.Keys.ToList();

            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();

            foreach(string guid in guids)
            {
                int noTasks = taskDB.getTasksCountByGUID(guid);
                if(noTasks == 0)//remove these additional HITS from the system
                {
                    List<SatyamAmazonHITTableAccessEntry> hitEntriesToBeRemoved = HITSByGUID[guid];
                    hit = new AmazonMTurkHIT();
                    hit.setAccount(hitEntriesToBeRemoved[0].AmazonAccessKeyID, hitEntriesToBeRemoved[0].AmazonSecretAccessKeyID, false);
                    foreach(SatyamAmazonHITTableAccessEntry hitEntryToBeRemoved in hitEntriesToBeRemoved)
                    {
                        //if (!hit.DeleteHIT(hitEntryToBeRemoved.HITID))
                        //{
                        //    try
                        //    {
                        //        hit.expireHIT(hitEntryToBeRemoved.HITID);
                        //    }
                        //    catch
                        //    {
                        //        Console.WriteLine("HIT already expired?");
                        //    }
                        //    hitDB = new SatyamAmazonHITTableAccess();
                        //    hitDB.UpdateStatus(hitEntryToBeRemoved.ID, HitStatus.expired);
                        //    hitDB.close();
                        //}
                        //else
                        //{
                        //    hitDB = new SatyamAmazonHITTableAccess();
                        //    hitDB.DeleteEntry(hitEntryToBeRemoved.ID);
                        //    hitDB.close();
                        //}
                        bool succ = SafeDeleteHit(hitEntryToBeRemoved);
                        if (succ)
                        {
                            succCount++;
                        }
                        else
                        {
                            failCount++;
                            //TODO: auto decide pay or reject
                        }
                        Console.WriteLine("Succ: {0}, Fail: {1}", succCount, failCount);
                    }
                }
            }
            taskDB.close();
        }

        public static Dictionary<string, List<SatyamTaskTableEntry>> OrganizeMTurkTasksByGUID()
        {
            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();

            //get all the MTURK related entries in the task Table
            List<SatyamTaskTableEntry> mturkEntries = new List<SatyamTaskTableEntry>();
            foreach (string taskTemplate in TaskConstants.MTurkTaskTemplates)
            {
                List<SatyamTaskTableEntry> entries = taskDB.getAllEntriesByJobtemplateType(taskTemplate);
                mturkEntries.AddRange(entries);
            }
            taskDB.close();

            if (mturkEntries.Count == 0) //there is nothing to do!!
            {
                return null;
            }

            //Group them by jobguids since each job can have many tasks
            Dictionary<string, List<SatyamTaskTableEntry>> TasksByGUID = new Dictionary<string, List<SatyamTaskTableEntry>>();
            foreach (SatyamTaskTableEntry entry in mturkEntries)
            {
                string guid = entry.JobGUID;
                if (!TasksByGUID.ContainsKey(entry.JobGUID))
                {
                    TasksByGUID.Add(entry.JobGUID, new List<SatyamTaskTableEntry>());
                }
                TasksByGUID[guid].Add(entry);
            }
            return TasksByGUID;
        }

        public static void AdaptTaskParameters()
        {
            double TargetPricePerHour = -1;
            double Budget = -1;
            //double TargetPricePerHour = 13.56; 
            //double MinPricePerTask = 0.01;
            //double MaxPricePerTask = 0.5;
            double MinPricePerTaskDifferenceToAdjust = 0.01;//USD
            int MinNoResultsToAdjust = 10;

            Dictionary<string, List<SatyamTaskTableEntry>> MTurkTasksByGUID = OrganizeMTurkTasksByGUID();
            if (MTurkTasksByGUID == null) return;

            // get statistics
            foreach (string guid in MTurkTasksByGUID.Keys)
            {

                // testing for counting only
                List<string> ACGuids = new List<string>()
                {
                    "e5fdff10-f018-4779-9d18-70c25fa3259f",//carpk 164
                    "a388df30-8742-4b6e-911d-32093502c2e8",//kitti 100
                };
                if (!ACGuids.Contains(guid)) continue;

                SatyamTaskTableEntry task0 = MTurkTasksByGUID[guid][0];
                double CurrentPricePerTask = task0.PricePerTask;

                SatyamTask t = JSonUtils.ConvertJSonToObject<SatyamTask>(task0.TaskParametersString);
                TargetPricePerHour = t.jobEntry.TargetPricePerTask;
                //TargetPricePerHour = 13.56; //temporary
                Budget = t.jobEntry.TotalBudget;

                if (TargetPricePerHour <= 0) continue;//invalid

                SatyamResultPerJobDataAnalysis ana = SatyamResultsAnalysis.AnalyzeFromResultsTable(new List<string>() { guid });

                if (ana == null) continue;

                int noAcceptedResults = ana.totalAcceptedResults;
                if (noAcceptedResults < MinNoResultsToAdjust) continue;


                double medianTimePerTaskInSeconds = ana.medianAcceptedTimeTakenPerResultInSeconds;
                double TargetPricePerTask = TargetPricePerHour / 3600 * medianTimePerTaskInSeconds;
                //TargetPricePerTask = Math.Max(TargetPricePerTask, MinPricePerTask);
                //TargetPricePerTask = Math.Min(TargetPricePerTask, MaxPricePerTask);
                TargetPricePerTask = Math.Round(TargetPricePerTask, 2);// round to cents.

                // is the price within the budget?
                double MinBudgetNeeded = MTurkTasksByGUID[guid].Count * TargetPricePerTask * 3;// at least 3 results per task.
                if (Budget < MinBudgetNeeded)
                {
                    Console.WriteLine("Budget too small to adapt price");
                    continue;
                }


                if (Math.Abs(CurrentPricePerTask-TargetPricePerTask) > MinPricePerTaskDifferenceToAdjust)
                {
                    // need to adjust
                    Console.WriteLine("Adapting the price to {0}", TargetPricePerTask);
                    ExpireHitByGUID(guid);
                    AdjustTasksByGUID(guid, TargetPricePerTask);
                }

            }
            
        }

        public static void AdjustTasksByGUID(string guid, double price)
        {
            SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
            List<SatyamTaskTableEntry> entries = taskDB.getAllEntriesByGUID(guid);
            taskDB.ClearByJobGUID(guid);

            /// update the price and the taskparam...
            foreach (SatyamTaskTableEntry entry in entries)
            {
                int taskID = entry.ID;
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(entry.TaskParametersString);
                task.jobEntry.amazonHITInformation.Price = price;
                string newTaskParams = JSonUtils.ConvertObjectToJSon(task);

                taskDB.AddEntryWithSpecificID(taskID, entry.JobTemplateType, entry.UserID, entry.JobGUID, newTaskParams, entry.JobSubmitTime, price);
            }
            taskDB.close();
        }


        public static void ExpireHitByGUID(string guid)
        {
            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();
            List<SatyamAmazonHITTableAccessEntry> expiredHitEntries = hitDB.getAllEntriesByJobGUID(guid);
            hitDB.close();

            //AmazonMTurkHIT hit = new AmazonMTurkHIT();

            foreach (SatyamAmazonHITTableAccessEntry expiredEntry in expiredHitEntries)
            {
                //hit.setAccount(expiredEntry.AmazonAccessKeyID, expiredEntry.AmazonSecretAccessKeyID, false);
                //if (!hit.DeleteHIT(expiredEntry.HITID))
                //{
                //    try
                //    {
                //        hit.expireHIT(expiredEntry.HITID);
                //    }
                //    catch
                //    {
                //        Console.WriteLine("HIT already expired?");
                //    }
                //    hitDB.UpdateStatus(expiredEntry.ID, HitStatus.expired);
                //}
                //else
                //{
                //    hitDB.DeleteEntry(expiredEntry.ID);
                //}
                SafeDeleteHit(expiredEntry);
            }
            //hitDB.close();

        }
    }
}
