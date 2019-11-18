using System;
using System.Collections.Generic;
using System.Linq;
using AmazonMechanicalTurkAPI;
using AzureBlobStorage;
using Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SQLTables;
using Utilities;

namespace SatyamDispatch
{
    public static class HitDisposalDispatch
    {
        [FunctionName("HitDisposalDispatch")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            DateTime start = DateTime.Now;
            bool logging = false;
            if (logging) log.Info($"DisposalDispatch executed at: {DateTime.Now}");


            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();
            // first try and remove all the expired entries
            List<SatyamAmazonHITTableAccessEntry> expiredHitEntries = hitDB.getAllEntriesByStatus(HitStatus.expired);
            hitDB.close();
            /// dispatch into the queue
            SatyamDispatchStorageAccountAccess satyamQueue = new SatyamDispatchStorageAccountAccess();

            foreach (SatyamAmazonHITTableAccessEntry expiredEntry in expiredHitEntries)
            {
                if (logging) log.Info($"Dispatching Disposal for {expiredEntry.HITID}");
                string queueName = "hit-disposal";
                string m = JSonUtils.ConvertObjectToJSon(expiredEntry);
                satyamQueue.Enqueue(queueName, m);

                if ((DateTime.Now - start).TotalSeconds > 280) return;
            }

            hitDB = new SatyamAmazonHITTableAccess();
            List<SatyamAmazonHITTableAccessEntry> hitEntriesPending = hitDB.getAllEntriesByStatus(HitStatus.pending);
            List<SatyamAmazonHITTableAccessEntry> hitEntriesSubmitted = hitDB.getAllEntriesByStatus(HitStatus.submitted);
            List<SatyamAmazonHITTableAccessEntry> hitEntriesAccepted = hitDB.getAllEntriesByStatus(HitStatus.accepted);
            List<SatyamAmazonHITTableAccessEntry> hitEntriesRejected = hitDB.getAllEntriesByStatus(HitStatus.rejected);
            List<SatyamAmazonHITTableAccessEntry> hitEntriesTaken = hitDB.getAllEntriesByStatus(HitStatus.taken);
            //hitDB.close();

            List<SatyamAmazonHITTableAccessEntry> hitEntries = new List<SatyamAmazonHITTableAccessEntry>();
            hitEntries.AddRange(hitEntriesPending);
            hitEntries.AddRange(hitEntriesSubmitted);
            hitEntries.AddRange(hitEntriesAccepted);
            hitEntries.AddRange(hitEntriesRejected);
            hitEntries.AddRange(hitEntriesTaken);


            if (hitEntries.Count == 0)
            {
                
                return;
            }

            //Group them by jobguids since each job can have many tasks
            Dictionary<string, List<SatyamAmazonHITTableAccessEntry>> HITSByGUID = new Dictionary<string, List<SatyamAmazonHITTableAccessEntry>>();
            foreach (SatyamAmazonHITTableAccessEntry hitEntry in hitEntries)
            {
                if (hitEntry.EntryExpired())
                {
                    //hitDB = new SatyamAmazonHITTableAccess();
                    hitDB.UpdateStatus(hitEntry.ID, HitStatus.expired);
                    //hitDB.close();
                }

                string guid = hitEntry.JobGUID;
                if (!HITSByGUID.ContainsKey(hitEntry.JobGUID))
                {
                    HITSByGUID.Add(hitEntry.JobGUID, new List<SatyamAmazonHITTableAccessEntry>());
                }
                HITSByGUID[guid].Add(hitEntry);

                if ((DateTime.Now - start).TotalSeconds > 280) break;
            }
            hitDB.close();

            //now check if this guid has any entries in the task table
            List<string> guids = HITSByGUID.Keys.ToList();

            

            foreach (string guid in guids)
            {
                SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
                int noTasks = taskDB.getTasksCountByGUID(guid);
                taskDB.close();
                if (noTasks == 0)//remove these additional HITS from the system
                {
                    List<SatyamAmazonHITTableAccessEntry> hitEntriesToBeRemoved = HITSByGUID[guid];
                    foreach (SatyamAmazonHITTableAccessEntry hitEntryToBeRemoved in hitEntriesToBeRemoved)
                    {
                        if (logging) log.Info($"Dispatching Disposal for {hitEntryToBeRemoved.HITID}");
                        string queueName = "hit-disposal";
                        string m = JSonUtils.ConvertObjectToJSon(hitEntryToBeRemoved);
                        satyamQueue.Enqueue(queueName, m);
                        if ((DateTime.Now - start).TotalSeconds > 280) break;
                    }
                }
                if ((DateTime.Now - start).TotalSeconds > 280) break;

            }
            
            

            if (logging) log.Info($"DisposalDispatch finished at: {DateTime.Now}");
        }
    }
}
