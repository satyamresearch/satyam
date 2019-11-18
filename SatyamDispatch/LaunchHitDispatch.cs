using System;
using System.Collections.Generic;
using AmazonMechanicalTurkAPI;
using Constants;
using JobTemplateClasses;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SatyamTaskGenerators;
using SQLTableManagement;
using SQLTables;
using Utilities;

namespace SatyamDispatch
{
    public static class LaunchHitDispatch
    {
        [FunctionName("LaunchHitDispatch")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            //log.Info($"Launch Hit Dispatch executed at: {DateTime.Now}");
            int maxHitsPerCycle = 100; // can launch a max of 300 within 5 min.
            AmazonHITManagement.LaunchAmazonHITsFromTaskTable(maxHitsPerCycle);
        }
    }
}
