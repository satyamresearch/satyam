using System;
using AmazonMechanicalTurkAPI;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace SatyamDispatch
{
    public static class Payment
    {
        [FunctionName("Payment")]
        public static void Run([QueueTrigger("payment")]string myQueueItem, TraceWriter log)
        {
            string[] fields = myQueueItem.Split('_');
            string AmazonAccessKeyID = fields[0];
            string AmazonSecretAccessKeyID = fields[1];
            string assignmentID = fields[2];
            //log.Info($"Payment: {myQueueItem}");

            AmazonMTurkHIT hit = new AmazonMTurkHIT();
            hit.setAccount(AmazonAccessKeyID, AmazonSecretAccessKeyID, false);

            /// approve
            hit.ApproveAssignment(assignmentID, "Great Job! Your work was within acceptable parameters!");            
        }
    }
}
