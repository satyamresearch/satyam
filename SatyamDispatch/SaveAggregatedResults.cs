using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SQLTableManagement;

namespace SatyamDispatch
{
    public static class SaveAggregatedResults
    {
        [FunctionName("SaveAggregatedResults")]
        public static void Run([TimerTrigger("0 0 0 */7 * *")]TimerInfo myTimer, TraceWriter log)
        {
            //log.Info($"SaveAggregatedResults executed at: {DateTime.Now}");
            SatyamJobSubmissionsTableManagement.processLaunchedJobs();
        }
    }
}
