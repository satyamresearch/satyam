using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonMechanicalTurkAPI
{
    public class AmazonMTurkHITBatchLauncher
    {
        //public string url;
        //string taskName;

        AmazonMTurkHIT hit;

        public AmazonMTurkHITBatchLauncher(string accessKeyId, string secretAccessKey)
        {
            hit = new AmazonMTurkHIT();
            hit.setAccount(accessKeyId, secretAccessKey, false);
        }

        public string LaunchBatches(string url, Dictionary<string, object> HitParams, int noTasks)
        {
            return LaunchBatches(1,1,url,HitParams,noTasks);
        }

        public string LaunchBatches(string url, Dictionary<string, object> HitParams, int noTasks, out List<string> hitIDs)
        {
            return LaunchBatches(1, 1, url, HitParams, noTasks,out hitIDs);
        }

        
        //launches n batches such that each batch hs a batch size
        public string LaunchBatches(int numHITSPerTask, int batchSize, string url, Dictionary<string, object> HitParams, int noTasks)
        {
            //first check if there is sufficient money
            double moneyInAccount = hit.getAccountBalance();
            double moneyRequired = (double)HitParams["Reward"] * noTasks;
            if(moneyRequired > moneyInAccount)
            {
                return "INSUFFICIENT_FUNDS_TO_LAUNCH_TASK";
            }


            int noRounds = (int)Math.Floor((double)(noTasks * numHITSPerTask) / (double)batchSize);
            int reminder = noTasks * numHITSPerTask - noRounds * batchSize;

            HitParams["MaxHITs"] = batchSize;

            //ActiveMTurkHitsListAccess HITList = new ActiveMTurkHitsListAccess();

            for (int i = 0; i < noRounds; i++)
            {
                //HIT h1 = hit.CreateExternalHIT(url, HitParams);
            }

            if (reminder > 0)
            {
                HitParams["MaxHITs"] = reminder;
                //HIT h = hit.CreateExternalHIT(url, HitParams);
            }

            return "SUCCESS";
        }

        public string LaunchBatches(int numHITSPerTask, int batchSize, string url, Dictionary<string, object> HitParams, int noTasks, out List<string> hitIDs)
        {

            hitIDs = new List<string>();
            //first check if there is sufficient money
            double moneyInAccount = hit.getAccountBalance();
            double moneyRequired = (double)HitParams["Reward"] * noTasks;
            if (moneyRequired > moneyInAccount)
            {
                return "INSUFFICIENT_FUNDS_TO_LAUNCH_TASK";
            }


            int noRounds = (int)Math.Floor((double)(noTasks * numHITSPerTask) / (double)batchSize);
            int reminder = noTasks * numHITSPerTask - noRounds * batchSize;

            HitParams["MaxHITs"] = batchSize;

            //ActiveMTurkHitsListAccess HITList = new ActiveMTurkHitsListAccess();

            for (int i = 0; i < noRounds; i++)
            {
                HIT h1 = hit.CreateExternalHIT(url, HitParams);
                hitIDs.Add(h1.HITId);
            }

            if (reminder > 0)
            {
                HitParams["MaxHITs"] = reminder;
                HIT h = hit.CreateExternalHIT(url, HitParams);
                hitIDs.Add(h.HITId);
            }

            return "SUCCESS";
        }

        

    }
}
