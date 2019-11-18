using SQLTableManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public class TestJobManagement
    {
        public static void reopenGUID()
        {
            //TrackingGuids
            //List<string> guids = new List<string>
            //{
            //    //"1be478ff-d390-47d7-9535-01b5dd744797",
            //    "1e43a983-548d-4a2e-8161-5537eb985902", // KITTI all
            //    "77c4f6af-f669-42fb-925a-4ff670cf9ae2", // the missing 0012
            //    //"62e64a6f-0982-4471-bba0-4c0803d932d0", // 0011 $1 per chunk
            //};

            //////DetectionGuids
            //List<string> guids = new List<string>
            //{
            //    ////KITTI set
            //    "3609af9c-e734-41dc-997c-94c060a1f63d",
            //    "5f14ecaa-1c4c-41d6-ad05-6ee0929e494e",
            //    "8fbc6ca1-e6c0-4720-93a3-1d49aa873a65",
            //    "d6f1c3ee-cd59-4197-b40a-0f45512596eb",
            //};

            //// image Classification Task
            //List<string> guids = new List<string>
            //{
            //    //"03977c5f-0e87-49ab-9c49-f20bfb47726a",//the 2img10class set 
            //    "9c6283dd-f218-4588-a0d9-8c0afe841503",//the allimg10class set 
            //};

            // image counting Task
            //List<string> guids = new List<string>
            //{
            //    //"94979929-4315-4c81-9782-55c8955222a8",//the CARPK set 
            //    //"f7c498d8-c06e-4b46-9169-71048c0e4eac", // carpk test 12
            //    "e5fdff10-f018-4779-9d18-70c25fa3259f", // carpk 164 for pricing adjustment
            //    "a388df30-8742-4b6e-911d-32093502c2e8", // KITTI 100
            //};

            //// video classification task
            //List<string> guids = new List<string>
            //{
            //    "ca2a97e3-3c32-48cc-b7d9-fdd945b50f23",//the JHMDB set 
            //};

            //// image segmentation Task
            //List<string> guids = new List<string>
            //{
            //    //"64089d4c-fb67-4ba4-bf71-53c1fb9cfbf6",//test
            //    //"91a14bb7-3c13-4d6f-9044-438beae9e559",//testpascal
            //    //"6c3d5101-b660-4bce-9cc6-7c7f8f7b3724", // test complex segments
            //    //"e14ec717-8689-43c9-b087-221c3b5201f2", // test MTurk on complex segments
            //    "86cbfe43-175a-4a5a-ba89-073f3004e7df", // pascal voc all
            //};


            // tracklet labeling
            List<string> guids = new List<string>()
            {
                //"1c231e7b-17bd-4dc6-aace-66f0bf67cfb9",//test
                //"c121fb39-4f96-468c-b536-f4322c2baafb", // test 2
                //"0efdc91e-62e2-4a70-b5fd-18720c170e03",// test 3
                //"29b74b77-a8bf-4382-b998-190cb3491922", // test 4
                //"9a46e24d-42e2-4b4e-a7df-dffc307fc4d6", // test 5, all false invalid
                "0058c6c8-c875-4e51-bcfc-7734ab2351fa", // 
        };


            foreach (string guid in guids)
            {
                SatyamJobSubmissionsTableManagement.reopenJobForMoreResults(guid);
            }
        }

        public static void TestChangeGUIDPrice()
        {
            //string guid = "86cbfe43-175a-4a5a-ba89-073f3004e7df";
            //string guid = "7ff1bdb4-d0e5-4d3d-aed1-92a184ba688b";
            //string guid = "b883f268-aa34-4450-84dc-aea257949c04";

            string guid = "dff5415b-b278-43db-8e32-4f654ca4b3a1";

            double TargetPricePerTask = 0.05;
            AmazonHITManagement.ExpireHitByGUID(guid);
            AmazonHITManagement.AdjustTasksByGUID(guid, TargetPricePerTask);
        }
    }
}
