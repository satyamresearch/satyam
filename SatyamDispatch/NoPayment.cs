using System;
using AmazonMechanicalTurkAPI;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace SatyamDispatch
{
    public static class NoPayment
    {
        [FunctionName("NoPayment")]
        public static void Run([QueueTrigger("nopayment")]string myQueueItem, TraceWriter log)
        {
            string[] fields = myQueueItem.Split('_');
            string AmazonAccessKeyID = fields[0];
            string AmazonSecretAccessKeyID = fields[1];
            string assignmentID = fields[2];
            //log.Info($"No Payment: {myQueueItem}");

            AmazonMTurkHIT hit = new AmazonMTurkHIT();
            hit.setAccount(AmazonAccessKeyID, AmazonSecretAccessKeyID, false);

            /// Reject
            hit.RejectAssignment(assignmentID, "Sorry! Your work was not within acceptable parameters!");
        }
    }
}
