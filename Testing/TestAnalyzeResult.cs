using Constants;
using JobTemplateClasses;
using SatyamAnalysis;
using SatyamResultValidation;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTableManagement;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Testing
{
    class TestAnalyzeResult
    {
        public static void TestSaveDetectionResult()
        {
            /// MultiObject Lab N Loc
            ////SatyamResultsAnalysis.AnalyzeFromResultsTable("4edc8ef3-704a-45a4-8e0d-40b225c2e0e7");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally("4edc8ef3-704a-45a4-8e0d-40b225c2e0e7", "4edc8ef3-704a-45a4-8e0d-40b225c2e0e7");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("4edc8ef3-704a-45a4-8e0d-40b225c2e0e7", "4edc8ef3-704a-45a4-8e0d-40b225c2e0e7");
            ////SatyamResultsAnalysis.AnalyzeFromResultsTable("afb41f8e-efe5-4d94-a9d0-3480704ea473");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally("afb41f8e-efe5-4d94-a9d0-3480704ea473", "afb41f8e-efe5-4d94-a9d0-3480704ea473");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("afb41f8e-efe5-4d94-a9d0-3480704ea473", "afb41f8e-efe5-4d94-a9d0-3480704ea473");
            ////SatyamResultPerJobDataAnalysis ana = SatyamResultsAnalysis.AnalyzeFromResultsTable("382dce96-21d5-4798-be50-7056917aea23");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally("382dce96-21d5-4798-be50-7056917aea23", "382dce96-21d5-4798-be50-7056917aea23");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("382dce96-21d5-4798-be50-7056917aea23", "382dce96-21d5-4798-be50-7056917aea23");

            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("f8c81c2e-301a-44ea-8a61-9856f38a72f5", 
            //    DirectoryConstants.defaultTempDirectory + "\\f8c81c2e-301a-44ea-8a61-9856f38a72f5");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("9c2c9681-ebf0-4304-afd3-795c030a60a5",
            //    DirectoryConstants.defaultTempDirectory + "\\9c2c9681-ebf0-4304-afd3-795c030a60a5");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("dca00b33-9336-4146-be3a-cc0e20fb3e8d",
            //    DirectoryConstants.defaultTempDirectory + "\\dca00b33-9336-4146-be3a-cc0e20fb3e8d");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("b4ec157d-f8ac-4c46-9b81-f99869643e66",
            //    DirectoryConstants.defaultTempDirectory + "\\b4ec157d-f8ac-4c46-9b81-f99869643e66");

            ///KITTI
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("d6f1c3ee-cd59-4197-b40a-0f45512596eb",
            //    DirectoryConstants.defaultTempDirectory + "\\KITTIAggregated");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("8fbc6ca1-e6c0-4720-93a3-1d49aa873a65",
            //    DirectoryConstants.defaultTempDirectory + "\\KITTIAggregated");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("5f14ecaa-1c4c-41d6-ad05-6ee0929e494e",
            //    DirectoryConstants.defaultTempDirectory + "\\KITTIAggregated");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally("3609af9c-e734-41dc-997c-94c060a1f63d",
            //    DirectoryConstants.defaultTempDirectory + "\\KITTIAggregated");

            //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally("d6f1c3ee-cd59-4197-b40a-0f45512596eb",
            //    DirectoryConstants.defaultTempDirectory + "\\KITTIResults");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally("8fbc6ca1-e6c0-4720-93a3-1d49aa873a65",
            //    DirectoryConstants.defaultTempDirectory + "\\KITTIResults");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally("5f14ecaa-1c4c-41d6-ad05-6ee0929e494e",
            //    DirectoryConstants.defaultTempDirectory + "\\KITTIResults");
            //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally("3609af9c-e734-41dc-997c-94c060a1f63d",
            //    DirectoryConstants.defaultTempDirectory + "\\KITTIResults");


            //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally("48c7ebc9-0b3d-4993-b404-bd4c83c3a625",
            //    DirectoryConstants.defaultTempDirectory + "48c7ebc9-0b3d-4993-b404-bd4c83c3a625");

            /// Revision Task
            //string guid = "91b49464-5161-4a2d-be26-1509ceda8897";
            //string guid = "a7e3fe3a-5097-4149-99bc-bae06da18aba";

            //string guid = "aefd12fc-42ed-4ec5-8c9a-6c41703a843f"; // revision kitti 10
            //string guid = "2c239102-e38f-4f0e-b639-6fae05250475"; // without revision kitti 10

            //string guid = "a328f600-3b1f-4845-87a0-402648ad51b1"; // revision Seattle 10, TF Serving backend
            //string guid = "5c403d7f-6c48-4638-b545-472b9f190606"; // no revision Seattle 10

            //string guid = "40eee69b-c260-4760-940f-92f30cdfbb7f"; // RevisionTF Seattle9000 TestStewart 10
            string guid = "a55c9c21-16c0-432a-930e-de428d8213a5"; // RevisionTF Fastrcnn_KITTI on TestStewart 10


            MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally(guid, DirectoryConstants.defaultTempDirectory + guid);
            MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally(guid, DirectoryConstants.defaultTempDirectory + guid);

        }

        public static void TestVisualizeKITTITrackingResult()
        {
            List<string> guids = new List<string>()
            {
                //"2b9522d3-79f5-410e-a22a-655ddcf50733", // 0000-0001 2video frame 10 sec chunk
                //"776dfb2c-341b-471e-a3a9-80747b103e36", //// 0000 5 sec chunk
                //"d2b845de-6efe-4ba8-b874-8738776f0ae1", // 0000 3 sec chunk
                //"4d39fe29-afb0-4b10-a4b7-aac64cc70d47", // 0017, $0.15 per chunk 3 sec chunk
                //"f88dd90c-cc2e-464a-bb6b-00f9d6e27c21", // 0000, 3 sec chunk full instructions
                //"206d083d-680c-4e4f-891e-740aaa760f5f", // 0017 incomplete chunk 20 frames
                //"1be478ff-d390-47d7-9535-01b5dd744797", // 0017, $0.5 per chunk, 3 sec chunk, 0.5 overlap
                //"2bb180e1-8bb7-410f-b49e-8f3ed8cd06f5", // 0000, $0.5 per chunk, 3 sec chunk, 0.5 overlap
                //"1e43a983-548d-4a2e-8161-5537eb985902", // KITTI all, $0.5 per chunk, 3 sec chunk, 0.5 overlap
                //"77c4f6af-f669-42fb-925a-4ff670cf9ae2", // the missing 0012
                //"62e64a6f-0982-4471-bba0-4c0803d932d0", // 0011 $1 per chunk, 3 sec 0.5 overlap
                "d9c96b28-6778-428d-b059-01a5f9cdad39", // 0011 $1 chunk, masters
            };

            foreach(string guid in guids)
            {
                MultiObjectTrackingAnalyzer.SaveKITTIAggregatedResultVideosLocally(guid, DirectoryConstants.defaultTempDirectory + guid);
                MultiObjectTrackingAnalyzer.SaveKITTIResultVideosLocally(guid, DirectoryConstants.defaultTempDirectory + guid);
            }
        }

        public static void TestVisualizeTrackingResult()
        {
            List<string> guids = new List<string>()
            {
               //"fa5a1e19-24f8-4d31-8162-0a6511c3e28c", // Seattle Test 1
               "7ff1bdb4-d0e5-4d3d-aed1-92a184ba688b", // 1 year Seattle
            };

            foreach (string guid in guids)
            {
                MultiObjectTrackingAnalyzer.SaveAggregatedResultVideosLocally(guid, DirectoryConstants.defaultTempDirectory + guid);
                //MultiObjectTrackingAnalyzer.SaveResultVideosLocally(guid, DirectoryConstants.defaultTempDirectory + guid);
            }
        }


        public static void TestGetGlobalStatistics()
        {
            Dictionary<string, int> workerSubmissions = new Dictionary<string, int>();
            Dictionary<string, int> HitSubmissions = new Dictionary<string, int>();
            Dictionary<int, int> TaskSubmissions = new Dictionary<int, int>();

            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = new List<SatyamResultsTableEntry>();

            entries = (resultsDB.getAllEntries());
            for (int i = 0; i < entries.Count; i++)
            {
                SatyamResultsTableEntry entry = entries[i];
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                SatyamJob job = task.jobEntry;

                string worker = satyamResult.amazonInfo.WorkerID;
                string hitID = satyamResult.amazonInfo.HITID;
                int taskID = entry.SatyamTaskTableEntryID;

                if (!workerSubmissions.ContainsKey(worker))
                {
                    workerSubmissions.Add(worker, 0);
                }
                workerSubmissions[worker]++;
                if (!HitSubmissions.ContainsKey(hitID))
                {
                    HitSubmissions.Add(hitID, 0);
                }
                HitSubmissions[hitID]++;
                if (!TaskSubmissions.ContainsKey(taskID))
                {
                    TaskSubmissions.Add(taskID, 0);
                }
                TaskSubmissions[taskID]++;
            }
            Console.WriteLine(workerSubmissions.Count);
            Console.WriteLine(HitSubmissions.Count);
            Console.WriteLine(TaskSubmissions.Count);
        }


        public static void TestSatyamResultAnalysis()
        {
            List<List<string>> jobs = new List<List<string>>()
            {
                //// Video Classification
                //new List<string>(){ "ca2a97e3-3c32-48cc-b7d9-fdd945b50f23" }, //10 class human gesture in JHMDB

                //// Image Classification
                //new List<string>(){ "9c6283dd-f218-4588-a0d9-8c0afe841503" }, // all img 10class 
                //new List<string>(){ "03977c5f-0e87-49ab-9c49-f20bfb47726a" }, // 2img 10 class
                
                //// Image Counting
                //new List<string>(){
                //    "94979929-4315-4c81-9782-55c8955222a8",// CARPK NTU 20161029
                //    "58a85bc8-4fb9-4672-8719-38cf62352fbc", // KITTI all
                //}, 
                
                //// Image Detection
                new List<string>(){
                    //"3609af9c-e734-41dc-997c-94c060a1f63d",
                    //"5f14ecaa-1c4c-41d6-ad05-6ee0929e494e",
                    //"8fbc6ca1-e6c0-4720-93a3-1d49aa873a65",
                    //"d6f1c3ee-cd59-4197-b40a-0f45512596eb",  // KITTI Detection all

                    //"a7e3fe3a-5097-4149-99bc-bae06da18aba", // rivision tasks satyam
                    //"aefd12fc-42ed-4ec5-8c9a-6c41703a843f", // revision task kitti
                    
                    //"2c239102-e38f-4f0e-b639-6fae05250475", // without revision task kitti
                    //"2b1a86ee-7d39-49db-90bc-fb8d2ff63019", // revision kitti 10 internal test with recordings

                    //"a328f600-3b1f-4845-87a0-402648ad51b1", // revision Seattle 10, TF Serving backend
                    //"5c403d7f-6c48-4638-b545-472b9f190606", // no revision Seattle 10

                    //"40eee69b-c260-4760-940f-92f30cdfbb7f", // RevisionTF Seattle9000 on TestStewart 10
                    //"a55c9c21-16c0-432a-930e-de428d8213a5", // RevisionTF Fastrcnn_KITTI on TestStewart 10
                    //"9e3096f6-e898-45eb-8b15-e2854485a59c", // RevisionTF Fastrcnn_KITTI on TestStewart 30, but aggregation is not functioning for most of the time
                    "3e835157-d058-46ee-a13b-9c1566cba505", //RevisionTF Seattle9000 on TestStewart 30

                },
                
                ////// Video Tracking
                //new List<string>(){
                //    "1e43a983-548d-4a2e-8161-5537eb985902",
                //    "77c4f6af-f669-42fb-925a-4ff670cf9ae2"}, // KITTI Tracking all

                ////// Caprk Counting Adaptive Pricing Test
                //new List<string>(){
                //    //"f7c498d8-c06e-4b46-9169-71048c0e4eac",// carpk 10
                //    "e5fdff10-f018-4779-9d18-70c25fa3259f",}, // carpk all
                //new List<string>(){
                //    "a388df30-8742-4b6e-911d-32093502c2e8"}, // KITTI all

                //////// Image Segmentation
                //new List<string>()
                //{
                //    "86cbfe43-175a-4a5a-ba89-073f3004e7df", // pascal VOC
                //},

                ////// Cam Pose Estimation
                //new List<string>()
                //{
                //    "dff5415b-b278-43db-8e32-4f654ca4b3a1", //3000 results
                //}

            };


            foreach( List<string> guids in jobs)
            {
                SatyamResultsAnalysis.AnalyzeFromResultsTable(guids);
            }
        }


        public static void TestSaveImageSegmentationResult()
        {
            //string guid = "64089d4c-fb67-4ba4-bf71-53c1fb9cfbf6";//test
            //string guid = "91a14bb7-3c13-4d6f-9044-438beae9e559";//testpascalVOC
            //string guid = "a2c3fe42-98b0-4abc-8b01-f59c0edbca2f";// testpascalperson
            //string guid = "5139bc96-4dd8-4898-9532-bcaff09ec065";//test holes
            //string guid = "6c3d5101-b660-4bce-9cc6-7c7f8f7b3724";//test complex segments

            //string guid = "5dcc5de5-3c62-4f4e-856a-24b689c8dc67";//test complex segments
            //string guid = "7e3ea843-f743-4407-9c45-5265ccfad474";// test MTurk
            //string guid = "e14ec717-8689-43c9-b087-221c3b5201f2";// test more 
            //string guid = "71186069-808d-4550-9e5c-c611e497e77b";//20 class mturk
            //string guid = "86cbfe43-175a-4a5a-ba89-073f3004e7df";//20 class mturk all pascal
            //string guid = "61bf62b5-d796-4b3c-9de9-9c67f04cf26f";// changed taskpage

            string guid = "13969e67-4fb7-4dfe-ace8-e2c5d7106cd4";

            //ImageSegmentationResultAnalysis.SaveResultImagesLocally(guid, DirectoryConstants.defaultTempDirectory + "\\" + guid + "\\");
            ImageSegmentationResultAnalysis.SaveAggregatedResultImagesLocally(guid, DirectoryConstants.defaultTempDirectory +"\\" + guid + "\\");
        }


        public static void TestSaveTrackletLabelingResult()
        {
            //string guid = "1c231e7b-17bd-4dc6-aace-66f0bf67cfb9"; //test mturk
            //string guid = "c121fb39-4f96-468c-b536-f4322c2baafb"; //test mturk 2
            //string guid = "0efdc91e-62e2-4a70-b5fd-18720c170e03"; // test3
            //string guid = "a07c2b9b-c295-4bd0-8203-25ab96e5b45d"; // test3 with overlap
            //string guid = "c627ab13-f1c4-47d3-83e0-6df1cd3ff5e7"; // test 3 decouple attributes
            //string guid = "29b74b77-a8bf-4382-b998-190cb3491922"; // test 4 long video
            //string guid = "9a46e24d-42e2-4b4e-a7df-dffc307fc4d6"; // test 5, all false invalid
            //string guid = "b883f268-aa34-4450-84dc-aea257949c04"; // gold set part 1
            string guid = "24d6e8ac-c559-466e-b801-abab6ce17235"; // gold set part 2

            //TrackletLabelingAnalyzer.SaveResultVideosLocally(guid);
            TrackletLabelingAnalyzer.SaveAggregatedResultVideosLocally(guid);

        }


        public static void TestAggregationAnalysis()
        {
            string guid = "86cbfe43-175a-4a5a-ba89-073f3004e7df";//20 class mturk all pascal
            //string guid = "61bf62b5-d796-4b3c-9de9-9c67f04cf26f";// changed taskpage

            string configString = "Min_" + 10 + "_Max_" + 10 + "_Majority_" + 0.5 + "_Ratio_" + 0.5;

            SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> aggEntries = aggDB.getEntriesByGUID(guid);
            aggDB.close();

            SatyamResultsTableAccess resDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resDB.getEntriesByGUID(guid);
            resDB.close();

            List<SatyamResultsTableEntry> filteredResults = new List<SatyamResultsTableEntry>();
            //foreach (SatyamResultsTableEntry entry in entries)
            //{
            //    SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);


            //    string workerID = res.amazonInfo.WorkerID;
            //    string assignmentID = res.amazonInfo.AssignmentID;
            //    if (assignmentID == "" || assignmentID == "ASSIGNMENT_ID_NOT_AVAILABLE") continue;
            //    if (workerID == "") continue;

            //    /// im seg only
            //    ImageSegmentationResult res_imseg = JSonUtils.ConvertJSonToObject<ImageSegmentationResult>(res.TaskResult);
            //    if (res_imseg == null) continue;

            //    filteredResults.Add(entry);
            //}

            filteredResults = entries;

            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime =
                SatyamResultValidationToolKit.SortResultsBySubmitTime_OneResultPerTurkerPerTask(filteredResults);
            SatyamResultsAnalysis.AggregationAnalysis(aggEntries,entriesBySubmitTime,null,null,guid, configString, 
                usePreApprovalResult:false,
                approvalRatioThreshold:0.5);


        }

        public static void TestSaveCamPoseResult()
        {
            //string guid = "64241744-27fe-456c-9165-ffc918571c29";
            //string guid = "719f7af6-602c-489f-b5f6-751821c93e33";
            //string guid = "a3810f29-bfa4-4a56-af78-857bc5e61804";
            //string guid = "dff5415b-b278-43db-8e32-4f654ca4b3a1";
            string guid = "715dbdb6-9b1b-4afc-a8d6-45d01ec12a08";
            CameraPoseAnnotationAnalyzer.SaveResultImagesLocally(guid);
        }

        public static void TestVisualizeCameraPoseAnnotationResult()
        {
            
            string result = "{\"objects\":{\"caxis\":{\"vertices\":[[454,218],[496,201],[450,184],[409,208]]},\"xvppoints\":{\"vertices\":[[566,357],[655,236],[454,116],[526,105],[289,219],[368,189]]},\"yvppoints\":{\"vertices\":[[386,317],[480,339],[182,65],[101,61],[399,116],[452,119]]},\"zvppoints\":{\"vertices\":[[182,65],[182,126],[281,33],[278,115],[686,74],[679,228]]}},\"displayScaleReductionX\":1.0906666666666667,\"displayScaleReductionY\":1.091858037578288,\"imageWidth\":818,\"imageHeight\":523}";
            string ImPath = @"C:\Users\Hang Qiu\Downloads\test-image.jpg";
            string resultDir = @"C:\Users\Hang Qiu\Downloads\";
            Image im = Image.FromFile(ImPath);

            Image ResultImage = CameraPoseAnnotationAnalyzer.DrawResultStringOnImage(result, im);

            ImageUtilities.saveImage(ResultImage, resultDir, "test-result");

        }

        public static void TestSaveOpenQuestionairResult()
        {
            string guid = "ddba5900-525b-4945-b3cb-337a77e46654";
            MiscTaskAnalyzer.SaveWholeSatyamResultText(guid);
            WorkerInfoManagement.SaveAllWorkerInfo(DirectoryConstants.defaultTempDirectory + guid + "\\workerinfo.txt");
        }

        public static void TestSaveWorkerInfo()
        {
            string directory = @"C:\research\MSRDVA\temp\WorkerInfo";
            WorkerInfoManagement.SaveAllWorkerInfo(directory);
        }
    }
}
