using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SQLTableManagement
{
    public static class PeriodicManagement
    {

        public static void Run()
        {
            ///check result
            SatyamResultsTableManagement.AggregateResults(); //process all the new results in the results table and aggregate them
            SatyamResultsTableManagement.AcceptRejectResults(); //accept and reject results based on the aggregated result
            AmazonMTurkPayments.MakePayments(); //make all the required payments to Amazon Turkers
            SatyamTaskTableManagement.PurgeAllAggregatedTasks(); // remove all tasks that are finished
            SatyamJobSubmissionsTableManagement.processLaunchedJobs(); //save results of all finished jobs

            ///adjust parameters
            //AmazonHITManagement.AdaptTaskParameters();

            ///launch another round
            SatyamJobSubmissionsTableManagement.preprocessSubmittedData(); // process the data acoordint to task template requirement
            SatyamJobSubmissionsTableManagement.processPreprocessedJobs(); //process the newly submitted jobs
            AmazonHITManagement.LaunchAmazonHITsFromTaskTable(); //Launch new HITS whereever applicable
            AmazonHITManagement.ClearHITSFromAmazon(); //clear HITS whereever applicable
        }

        public static void RunLoop()
        {
            do
            {
                Console.WriteLine("Starting Management at " + DateTime.Now);
                PeriodicManagement.Run();
                Console.WriteLine("Finished Management at " + DateTime.Now);
                //Thread.Sleep(300000); //every 10 minutes
            } while (true);
        }


        
    }
}
