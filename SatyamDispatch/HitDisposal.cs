using System;
using AmazonMechanicalTurkAPI;
using Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SQLTableManagement;
using SQLTables;
using Utilities;

namespace SatyamDispatch
{
    public static class HitDisposal
    {
        [FunctionName("HitDisposal")]
        public static void Run([QueueTrigger("hit-disposal")]string myQueueItem, TraceWriter log)
        {
            bool logging = false;
            if (logging) log.Info($"C# Queue trigger function processed: {myQueueItem}");

            SatyamAmazonHITTableAccessEntry hitEntryToBeRemoved = JSonUtils.ConvertJSonToObject<SatyamAmazonHITTableAccessEntry>(myQueueItem);
            //check if it's still in the DB
            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();
            bool exist = hitDB.EntryExistByID(hitEntryToBeRemoved.ID.ToString());
            hitDB.close();
            if (!exist)
            {   
                return;
            }

            //AmazonHITManagement.SafeDeleteHit(hitEntryToBeRemoved);

            AmazonMTurkHIT hit = new AmazonMTurkHIT();
            hit.setAccount(hitEntryToBeRemoved.AmazonAccessKeyID, hitEntryToBeRemoved.AmazonSecretAccessKeyID, false);
            if (hit.DeleteHIT(hitEntryToBeRemoved.HITID))
            {
                hitDB = new SatyamAmazonHITTableAccess();
                hitDB.DeleteEntry(hitEntryToBeRemoved.ID);
                hitDB.close();
            }
            else
            {
                try
                {
                    hit.expireHIT(hitEntryToBeRemoved.HITID);
                }
                catch
                {
                    Console.WriteLine("HIT already expired?");
                }
                hitDB = new SatyamAmazonHITTableAccess();
                hitDB.UpdateStatus(hitEntryToBeRemoved.ID, HitStatus.expired);
                hitDB.close();
            }
        }
    }
}
