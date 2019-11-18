using AzureBlobStorage;
using Constants;
using HelperClasses;
using JobTemplateClasses;
using SatyamAnalysis;
using SatyamResultAggregators;
using SatyamResultClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Utilities;

namespace SatyamResultValidation
{


    
    public class KITTITrackingGroundTruth
    {
        public List<MultiObjectTrackingResult> groundtruthPerChunk = new List<MultiObjectTrackingResult>();
        public List<int> totalFramesPerChunk = new List<int>();

        public void LoadGroundTruthFile(string filepath, int fps = 10)
        {
            groundtruthPerChunk.Clear();
            totalFramesPerChunk.Clear();

            List<string> ObjClassOfInterest = new List<string>() { "car", "pedestrian" };
            SortedDictionary<int, string> annotationString = new SortedDictionary<int, string>();
            string[] lines = System.IO.File.ReadAllLines(filepath);
            int MaxFrameId = 0;
            /////////////////////debug
            //string DirectoryURL = "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0000_153frame/";
            //SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            //List<string> ImageURLs = satyamStorage.getURLListOfSubDirectoryByURL(DirectoryURL);
            //List<Image> imageList = new List<Image>();
            //List<Image> imagesWithTracks = new List<Image>();
            //var wc = new WebClient();
            //foreach (string uri in ImageURLs)
            //{
            //    Image x = Image.FromStream(wc.OpenRead(uri));
            //    imageList.Add(x);
            //}
            //int currentFrameID = 0;
            //List<BoundingBox> locations = new List<BoundingBox>();
            //List<string> labels = new List<string>();
            //Dictionary<string, List<bool>> attributes = new Dictionary<string, List<bool>>();
            //attributes.Add("occlusion", new List<bool>());
            //List<int> idx = new List<int>();
            //Directory.CreateDirectory(DirectoryConstants.DataStoragePath + "\\KITTITrackingOriginal");
            //////////////////////finish debug
            foreach (string line in lines)
            {
                string[] fields = line.Split(' ');
                int frameId = Convert.ToInt32((fields[0]));       // frame
                int track_id = Convert.ToInt32(fields[1]);         //id
                string obj_type = fields[2].ToLower();          // object type [car, pedestrian, cyclist, ...]
                int truncation = Convert.ToInt32((fields[3]));  // truncation [-1,0,1,2]
                int occlusion = Convert.ToInt32((fields[4]));   // occlusion  [-1,0,1,2]
                double obs_angle = Convert.ToDouble(fields[5]); // observation angle [rad]
                double x1 = Convert.ToDouble(fields[6]);        // left   [px]
                double y1 = Convert.ToDouble(fields[7]);        // top    [px]
                double x2 = Convert.ToDouble(fields[8]);        // right  [px]
                double y2 = Convert.ToDouble(fields[9]);        // bottom [px]
                double h = Convert.ToDouble(fields[10]);        // height [m]
                double w = Convert.ToDouble(fields[11]);        // width  [m]
                double l = Convert.ToDouble(fields[12]);        // length [m]
                double X = Convert.ToDouble(fields[13]);        // X [m]
                double Y = Convert.ToDouble(fields[14]);        // Y [m]
                double Z = Convert.ToDouble(fields[15]);        // Z [m]
                double yaw = Convert.ToDouble(fields[16]);      // yaw angle [rad]

                if (track_id == -1) continue;

                if (obj_type == "van") obj_type = "car";
                if (obj_type == "person_sitting") obj_type = "pedestrian";
                //if (!ObjClassOfInterest.Contains(obj_type)) continue;

                if (frameId > MaxFrameId) MaxFrameId = frameId;

                if (!annotationString.ContainsKey(track_id)){
                    string FirstAnnoString = obj_type + "_Location" + getAnnotationStringPerFrame(frameId, x1,y1,x2,y2,occlusion,truncation, fps);
                    annotationString.Add(track_id, FirstAnnoString);
                }
                else
                {
                    annotationString[track_id]+= getAnnotationStringPerFrame(frameId, x1, y1, x2, y2, occlusion, truncation);
                }

                ////////////////////////debug
                //locations.Add(new BoundingBox((int)x1, (int)y1, (int)x2, (int)y2));
                //labels.Add(obj_type);
                //attributes["occlusion"].Add(occlusion > TaskConstants.MULTI_OBJ_TRACKING_VALIDATION_MAX_OCCLUSION);
                //idx.Add(track_id);

                //if (frameId != currentFrameID)
                //{
                //    Image new_image = MultiObjectTrackingAnalyzer.generateTrackImage(imageList[currentFrameID], labels, locations, attributes, idx);
                //    imagesWithTracks.Add(new_image);
                //    if (!Directory.Exists(DirectoryConstants.DataStoragePath + "\\KITTITrackingOriginal" + "\\img" + currentFrameID.ToString("0000") + ".jpg"))
                //    {
                //        new_image.Save(DirectoryConstants.DataStoragePath + "\\KITTITrackingOriginal" + "\\img" + currentFrameID.ToString("0000") + ".jpg");
                //    }                    

                //    currentFrameID = frameId;
                //    locations.Clear();
                //    labels.Clear();
                //    attributes["occlusion"].Clear();
                //    idx.Clear();
                //}
                ///////////////////////finish debug
            }

            
            // connect objects into one string
            string totalAnnotationString = ObjectsToStrings.ListString(annotationString.Select(item=> item.Value).ToList(), '|');
            //DateTime start = DateTime.MinValue;
            //double MilSecondsPerFrame = (double)1000 / (double)fps;
            //DateTime end = start.AddMilliseconds(MilSecondsPerFrame * MaxFrameId);
            string videoName = URIUtilities.filenameFromDirectoryNoExtension(filepath);
            VATIC_DVA_CrowdsourcedResult raw = new VATIC_DVA_CrowdsourcedResult(totalAnnotationString, videoName, videoName, MaxFrameId+1, fps);
            groundtruthPerChunk.Add(raw.getCompressedTracksInTimeSegment());
            totalFramesPerChunk.Add(MaxFrameId+1);
        }


        public void LoadGroundTruthFileAsMultipleChunks(string filepath, int chunkDurationInSeconds, int fps = 10)
        {
            groundtruthPerChunk.Clear();
            totalFramesPerChunk.Clear();

            List<string> ObjClassOfInterest = new List<string>() { "car", "pedestrian" };
            SortedDictionary<int, string> annotationString = new SortedDictionary<int, string>();
            string[] lines = System.IO.File.ReadAllLines(filepath);
            
            int totalFrame = chunkDurationInSeconds * fps;
            int CurrentStartingFrameId = 0;
            int LineIndex = 0;
            while (true)
            {
                int MaxFrameId = 0;
                annotationString.Clear();
                while(LineIndex < lines.Count())
                {
                    
                    string line = lines[LineIndex];
                    string[] fields = line.Split(' ');
                    int frameId = Convert.ToInt32((fields[0]));       // frame
                    int track_id = Convert.ToInt32(fields[1]);         //id
                    string obj_type = fields[2].ToLower();          // object type [car, pedestrian, cyclist, ...]
                    int truncation = Convert.ToInt32((fields[3]));  // truncation [-1,0,1,2]
                    int occlusion = Convert.ToInt32((fields[4]));   // occlusion  [-1,0,1,2]
                    double obs_angle = Convert.ToDouble(fields[5]); // observation angle [rad]
                    double x1 = Convert.ToDouble(fields[6]);        // left   [px]
                    double y1 = Convert.ToDouble(fields[7]);        // top    [px]
                    double x2 = Convert.ToDouble(fields[8]);        // right  [px]
                    double y2 = Convert.ToDouble(fields[9]);        // bottom [px]
                    double h = Convert.ToDouble(fields[10]);        // height [m]
                    double w = Convert.ToDouble(fields[11]);        // width  [m]
                    double l = Convert.ToDouble(fields[12]);        // length [m]
                    double X = Convert.ToDouble(fields[13]);        // X [m]
                    double Y = Convert.ToDouble(fields[14]);        // Y [m]
                    double Z = Convert.ToDouble(fields[15]);        // Z [m]
                    double yaw = Convert.ToDouble(fields[16]);      // yaw angle [rad]

                    if (frameId >= CurrentStartingFrameId + totalFrame) break;

                    if (track_id == -1)
                    {
                        LineIndex++;
                        continue;
                    }

                    if (obj_type == "van") obj_type = "car";
                    if (obj_type == "person_sitting") obj_type = "pedestrian";
                    //if (!ObjClassOfInterest.Contains(obj_type))
                    //{
                    //    LineIndex++;
                    //    continue;
                    //}

                    if (frameId > MaxFrameId) MaxFrameId = frameId;

                    if (!annotationString.ContainsKey(track_id))
                    {
                        string FirstAnnoString = obj_type + "_Location" + getAnnotationStringPerFrame(frameId, x1, y1, x2, y2, occlusion, truncation, fps);
                        annotationString.Add(track_id, FirstAnnoString);
                    }
                    else
                    {
                        annotationString[track_id] += getAnnotationStringPerFrame(frameId, x1, y1, x2, y2, occlusion, truncation);
                    }
                    LineIndex++;
                }

                // connect objects into one string
                string totalAnnotationString = ObjectsToStrings.ListString(annotationString.Select(item => item.Value).ToList(), '|');
                //DateTime start = DateTime.MinValue;
                //double MilSecondsPerFrame = (double)1000 / (double)fps;
                //DateTime end = start.AddMilliseconds(MilSecondsPerFrame * MaxFrameId);
                string videoName = URIUtilities.filenameFromDirectoryNoExtension(filepath);
                VATIC_DVA_CrowdsourcedResult raw = new VATIC_DVA_CrowdsourcedResult(totalAnnotationString, videoName, videoName, MaxFrameId+1, fps);
                groundtruthPerChunk.Add(raw.getCompressedTracksInTimeSegment());
                this.totalFramesPerChunk.Add(MaxFrameId + 1);

                CurrentStartingFrameId += totalFrame;

                if (LineIndex >= lines.Count()) break;
            }            
        }

        public string getAnnotationStringPerFrame(int frameId, double x1, double y1, double x2, double y2, int occlusion, int trucation, int fps = 10) // KITTI default fps is 10
        {
            DateTime start = DateTime.MinValue;
            double MilSecondsPerFrame = (double)1000 / (double)fps;
            return String.Format(":{0},{1},{2},{3},{4},{5},{6}", 
                DateTimeUtilities.convertDateTimeToString(start.AddMilliseconds(MilSecondsPerFrame * frameId)),
                (int)x1, (int)y1, (int)x2, (int)y2,
                occlusion>TaskConstants.MULTI_OBJ_TRACKING_VALIDATION_MAX_OCCLUSION,
                trucation>TaskConstants.MULTI_OBJ_TRACKING_VALIDATION_MIN_TRUNCATION); 
        }

        public void SaveGroundTruthImagesUsingAzureBlobImages(string groundtruthFilepath, string DirectoryURL, string outputDirectory, int fps =10)
        {
            LoadGroundTruthFile(groundtruthFilepath, fps);
            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            List<string> ImageURLs = satyamStorage.getURLListOfSubDirectoryByURL(DirectoryURL);
            string videoName = URIUtilities.filenameFromURI(DirectoryURL);
            MultiObjectTrackingAnalyzer.generateVideoFramesForEvaluation(ImageURLs, groundtruthPerChunk[0], outputDirectory, videoName, fps);
        }

        public void SaveGroundTruthImagesUsingLocalImages(string groundtruthFilepath, string localDirectory, string outputDirectory, int fps = 10)
        {
            LoadGroundTruthFile(groundtruthFilepath, fps);
            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            List<string> ImageURLs = Directory.GetFiles(localDirectory).ToList();
            string videoName = URIUtilities.localDirectoryNameFromDirectory(localDirectory);
            MultiObjectTrackingAnalyzer.generateVideoFramesForEvaluation(ImageURLs, groundtruthPerChunk[0], outputDirectory, videoName, fps);
        }
    }

    

    public class KITTITrackingResultValidation
    {
        public static Dictionary<string, List<int>> MissingFrameIdsPerVideo = new Dictionary<string, List<int>>()
        {
            { "0013", new List<int>(){ 220} },
            { "0016", new List<int>(){ 78,143} },
            { "0017", new List<int>(){ 26,63} },
            { "0018", new List<int>(){ 46,68,107} },
        };


        public enum KITTITrackingValidationResultType
        {
            None = 0,
            TP = 1,
            //FP = 1,
            FP_WrongCategoryRightBox = 2, FP_IOU_TOOSMALL = 3, FP_NoGroundTruth = 4,
            FN_DeviatedBox = 5, FN_NoDetection = 6,
        };

        public List<int> MIN_HEIGHT = new List<int>() { 40, 25, 25 };
        public List<int> MAX_OCCLUSION = new List<int>() { 0, 1, 2 };
        public List<double> MAX_TRUNCATION = new List<double>() { 0.15, 0.3, 0.5 };

        public static List<string> CLASS_NAMES = new List<string>() { "Car", "Pedestrian", "Cyclist", "Van", "Person_sitting" };

        public const string GroundTruthLabelDirectory = @"C:\research\dataset\KITTI\Tracking\training\label_02\";
        public const string GroundTruthImageDirectory = @"C:\research\dataset\KITTI\Tracking\training\image_02\";



        public SortedDictionary<int, List<string>> CleanGroundTruthAndGetStatistics(int ChunkDurationinSeconds, int noObjectFramesStep = 50, string LabelPath = GroundTruthLabelDirectory, int fps = 10)
        {

            SortedDictionary<int, List<string>> noObjectsPerChunkHistogram = new SortedDictionary<int, List<string>>();
            SortedDictionary<int, List<string>> noTotalObjectFramesPerChunkHistogram = new SortedDictionary<int, List<string>>();

            int totalFramesPerChunk = ChunkDurationinSeconds * fps;
            string[] files = System.IO.Directory.GetFiles(LabelPath);

            int noObjectStep = 5;
            
            foreach (string file in files)
            {
                
                string videoName = URIUtilities.filenameFromDirectoryNoExtension(file);
                Console.WriteLine("Processing Video: {0}", videoName);
                KITTITrackingGroundTruth gt = new KITTITrackingGroundTruth();
                gt.LoadGroundTruthFileAsMultipleChunks(file, ChunkDurationinSeconds, fps);
                for (int i = 0; i < gt.groundtruthPerChunk.Count; i++)
                {
                    int noObjectsPerChunk = gt.groundtruthPerChunk[i].tracks.Count / noObjectStep * noObjectStep;
                    if (!noObjectsPerChunkHistogram.ContainsKey(noObjectsPerChunk))
                    {
                        noObjectsPerChunkHistogram.Add(noObjectsPerChunk, new List<string>());
                    }
                    noObjectsPerChunkHistogram[noObjectsPerChunk].Add(videoName + '_'+i);

                    int noObjectFramesPerChunk = 0;
                    foreach(CompressedTrack track in gt.groundtruthPerChunk[i].tracks)
                    {
                        noObjectFramesPerChunk += track.spaceTimeTrack.space_time_track.Count;
                    }
                    noObjectFramesPerChunk = noObjectFramesPerChunk/noObjectFramesStep* noObjectFramesStep;
                    if (!noTotalObjectFramesPerChunkHistogram.ContainsKey(noObjectFramesPerChunk))
                    {
                        noTotalObjectFramesPerChunkHistogram.Add(noObjectFramesPerChunk, new List<string>());
                    }
                    noTotalObjectFramesPerChunkHistogram[noObjectFramesPerChunk].Add(videoName + '_' + i);
                }
            }

            ///summary
            foreach (int num in noObjectsPerChunkHistogram.Keys)
            {
                Console.WriteLine("{0}, {1}", num, noObjectsPerChunkHistogram[num].Count);
            }

            foreach (int num in noTotalObjectFramesPerChunkHistogram.Keys)
            {
                Console.WriteLine("{0}, {1}", num, noTotalObjectFramesPerChunkHistogram[num].Count);
            }

            foreach (int num in noTotalObjectFramesPerChunkHistogram.Keys)
            {                
                Console.WriteLine(ObjectsToStrings.ListString(noTotalObjectFramesPerChunkHistogram[num],','));   
            }
            return noTotalObjectFramesPerChunkHistogram;
        }


        


        public static void SavingSatyamTrackingAggregationResultByGUID_KITTIFormat(string guid, string outputDirectory)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();
            SavingSatyamTrackingAggregationResultByGUID_KITTIFormat(results, guid, outputDirectory);
        }

        public static void SavingSatyamKITTITrackingAggregationResultByGUID_KITTIFormat(string guid, string outputDirectory)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();
            ValidateSatyamKITTITrackingAggregationResults(results, guid, outputDirectory);
        }

        public static bool SavingSatyamTrackingAggregationResultByGUID_KITTIFormat(List<SatyamAggregatedResultsTableEntry> results, string guid, string outputDirectory, string configString=null)
        {
            bool success = true;
            if (configString == null)
            {
                configString = "Min_" + TaskConstants.MULTI_OBJ_TRACKING_MTURK_MIN_RESULTS_TO_AGGREGATE +
                        "_Max_" + TaskConstants.MULTI_OBJ_TRACKING_MTURK_MAX_RESULTS_TO_AGGREGATE +
                        "_IoU_" + TaskConstants.MULTI_OBJ_TRACKING_MIN_TUBELET_SIMILARITY_THRESHOLD +
                        "_Ratio_" + TaskConstants.MULTI_OBJ_TRACKING_APPROVALRATIO_PER_VIDEO;
            }

            SortedDictionary<string, List<SatyamAggregatedResultsTableEntry>> entriesPerVideo = MultiObjectTrackingAnalyzer.GroupChunksByVideoName(results);
            foreach (string videoname in entriesPerVideo.Keys)
            {
                SatyamAggregatedResultsTableEntry entry = entriesPerVideo[videoname][0];
                SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
                SatyamTask aggTask = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamAggResult.TaskParameters);
                MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(aggTask.jobEntry.JobParameters);


                ///////////////////////////// KITTI Frame no check //////////////////////////////
                //string videoSequenceNo = videoname.Split('_')[2];
                //if (guid == "1e43a983-548d-4a2e-8161-5537eb985902")
                //{
                //    videoSequenceNo = MultiObjectTrackingAnalyzer.getCorrectSequenceNo(videoSequenceNo);
                //}

                //if (!MultiObjectTrackingAnalyzer.totalFrames.ContainsKey(videoSequenceNo))
                //{
                //    continue;
                //}
                //int totalChunks = (int)Math.Ceiling((double)(MultiObjectTrackingAnalyzer.totalFrames[videoSequenceNo]) / (double)(job.FrameRate * job.ChunkDuration));
                //if (entriesPerVideo[videoname].Count < totalChunks)
                //{
                //    // not enough chunks are aggregated
                //    Console.WriteLine("Not enough chunks are aggregated for video: {0}", videoname);
                //    success = false;
                //    List<int> ids = new List<int>();
                //    foreach(SatyamAggregatedResultsTableEntry agg in entriesPerVideo[videoname])
                //    {
                //        ids.Add(agg.SatyamTaskTableEntryID);
                //    }
                //    ids.Sort();
                //    for (int i = 0; i < totalChunks; i++)
                //    {
                //        if (ids.Contains(ids[0] + i)) continue;
                //        Console.WriteLine("Missing Chunk #{0}, taskID {1}", i, ids[0] + i);
                //    }
                    
                //    continue;
                //}

                List<string> ImageURLs;
                int fps;
                MultiObjectTrackingResult stitched = MultiObjectTrackingAnalyzer.stitchAllChunksOfOneVideo(entriesPerVideo[videoname], out ImageURLs, out fps);
                if (stitched != null)
                {
                    
                    //string filepath = writeStitchedResultToFile_KITTIFormat_Tracking(stitched, videoname, fps, outputDirectory + "\\AggregatedStitched_KITTIFormat\\" + configString);
                    string filedir = writeStitchedResultToFile_KITTIFormat_Detection(stitched, videoname, fps, outputDirectory + "\\AggregatedStitched_KITTIFormat\\" + configString);
                }
                else
                {
                    // write empty files
                    string fileDir = outputDirectory + "\\AggregatedStitched_KITTIFormat\\" + configString + "\\data-frame\\";
                    List<string> outputs = new List<string>();
                    for (int i = 0; i < 30; i++)
                    {
                        string filePath = fileDir + "\\" + videoname + "_frame_" + i.ToString("000000") + ".txt";
                        File.WriteAllLines(filePath, outputs.ToArray());
                    }                    
                }
            }

            //string command = "cd " + DirectoryConstants.KITTITrackingDevKitPython;
            //Console.WriteLine(command);
            ////SystemCommands.ExecuteCommandSync(command);
            //command = DirectoryConstants.python + " " + DirectoryConstants.KITTITrackingDevKitPython + "evaluate_tracking.py " + guid + " " + DirectoryConstants.KITTITrackingLabels;
            //Console.WriteLine(command);
            //SystemCommands.ExecuteCommandSync(command);
            return success;
        }


    public static bool ValidateSatyamKITTITrackingAggregationResults(List<SatyamAggregatedResultsTableEntry> results, string guid, string outputDirectory, string configString = null)
    {
        bool success = true;
        if (configString == null)
        {
            configString = "Min_" + TaskConstants.MULTI_OBJ_TRACKING_MTURK_MIN_RESULTS_TO_AGGREGATE +
                    "_Max_" + TaskConstants.MULTI_OBJ_TRACKING_MTURK_MAX_RESULTS_TO_AGGREGATE +
                    "_IoU_" + TaskConstants.MULTI_OBJ_TRACKING_MIN_TUBELET_SIMILARITY_THRESHOLD +
                    "_Ratio_" + TaskConstants.MULTI_OBJ_TRACKING_APPROVALRATIO_PER_VIDEO;
        }

        SortedDictionary<string, List<SatyamAggregatedResultsTableEntry>> entriesPerVideo = MultiObjectTrackingAnalyzer.GroupChunksByVideoName(results);
        foreach (string videoname in entriesPerVideo.Keys)
        {
            SatyamAggregatedResultsTableEntry entry = entriesPerVideo[videoname][0];
            SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
            SatyamTask aggTask = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamAggResult.TaskParameters);
            MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(aggTask.jobEntry.JobParameters);


            ///////////////////////////// KITTI Frame no check //////////////////////////////
            string videoSequenceNo = videoname.Split('_')[2];
            if (guid == "1e43a983-548d-4a2e-8161-5537eb985902")
            {
                videoSequenceNo = MultiObjectTrackingAnalyzer.getCorrectSequenceNo(videoSequenceNo);
            }

            if (!MultiObjectTrackingAnalyzer.totalFrames.ContainsKey(videoSequenceNo))
            {
                continue;
            }
            int totalChunks = (int)Math.Ceiling((double)(MultiObjectTrackingAnalyzer.totalFrames[videoSequenceNo]) / (double)(job.FrameRate * job.ChunkDuration));
            if (entriesPerVideo[videoname].Count < totalChunks)
            {
                // not enough chunks are aggregated
                Console.WriteLine("Not enough chunks are aggregated for video: {0}", videoname);
                success = false;
                List<int> ids = new List<int>();
                foreach (SatyamAggregatedResultsTableEntry agg in entriesPerVideo[videoname])
                {
                    ids.Add(agg.SatyamTaskTableEntryID);
                }
                ids.Sort();
                for (int i = 0; i < totalChunks; i++)
                {
                    if (ids.Contains(ids[0] + i)) continue;
                    Console.WriteLine("Missing Chunk #{0}, taskID {1}", i, ids[0] + i);
                }

                continue;
            }

        List<string> ImageURLs;
        int fps;
        MultiObjectTrackingResult stitched = MultiObjectTrackingAnalyzer.stitchAllChunksOfOneVideo(entriesPerVideo[videoname], out ImageURLs, out fps);
        if (stitched != null)
        {
            if (guid == "1e43a983-548d-4a2e-8161-5537eb985902")
            {
                stitched = DebugMissingFrame(stitched, videoSequenceNo);
            }

            string filepath = writeStitchedResultToFile_KITTIFormat_Tracking(stitched, videoSequenceNo, fps, outputDirectory + "\\AggregatedStitched_KITTIFormat\\" + configString);

                ////////////////////////////// visualization of kitti dataset using local images ///////////////////
                //string videoSequenceNo = URIUtilities.filenameFromDirectoryNoExtension(filepath);
                string videoFrameDir = DirectoryConstants.KITTITrackingImages + videoSequenceNo + "\\";
                // a copy of how the kitti format file looks like
                KITTITrackingGroundTruth gt = new KITTITrackingGroundTruth();
                gt.SaveGroundTruthImagesUsingLocalImages(filepath, videoFrameDir,
                    outputDirectory + "\\AggregatedParam\\");

                // a copy of aggregatedstitched
                List<string> files = Directory.GetFiles(videoFrameDir).ToList();
                List<string> LocalImageURLs = new List<string>();
                for (int j = 0; j < files.Count; j++)
                {
                    LocalImageURLs.Add(files[j]);
                }

                MultiObjectTrackingAnalyzer.generateVideoFramesForEvaluation(LocalImageURLs, stitched, outputDirectory + "\\AggregatedStitched\\", videoname, fps);
            }
    }

            //string command = "cd " + DirectoryConstants.KITTITrackingDevKitPython;
            //Console.WriteLine(command);
            ////SystemCommands.ExecuteCommandSync(command);
            //command = DirectoryConstants.python + " " + DirectoryConstants.KITTITrackingDevKitPython + "evaluate_tracking.py " + guid + " " + DirectoryConstants.KITTITrackingLabels;
            //Console.WriteLine(command);
            //SystemCommands.ExecuteCommandSync(command);
            return success;
        }

public static bool AggregateWithParameterAndValidateSatyamKITTITrackingAggregationResultByGUID(string guid, string outputDirectory,
            double boxToleranceThreshold = TaskConstants.MULTI_OBJ_TRACKING_BOX_DEVIATION_THRESHOLD,
            double ObjectCoverageApprovalThresholdPerVideo = TaskConstants.MULTI_OBJ_TRACKING_APPROVALRATIO_PER_VIDEO,
            double BoxCoverageApprovalThresholdPerTrack = TaskConstants.MULTI_OBJ_TRACKING_APPROVALRATIO_PER_TRACK,
            //int approvalDifferenceLimit = TaskConstants.MULTI_OBJ_TRACKING_MAX_APPROVAL_DIFFERENCE,
            int consensusNumber = TaskConstants.MULTI_OBJ_TRACKING_MIN_RESULTS_FOR_CONSENSUS,
            double minTubeletIoUSimilarityThreshold = TaskConstants.MULTI_OBJ_TRACKING_MIN_TUBELET_SIMILARITY_THRESHOLD,
            int MinResults = TaskConstants.MULTI_OBJ_TRACKING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.MULTI_OBJ_TRACKING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            bool overwrite = false)
        {

            string configString = "Min_" + MinResults + "_Max_" + MaxResults + "_IoU_" + minTubeletIoUSimilarityThreshold + "_Ratio_" + ObjectCoverageApprovalThresholdPerVideo;
            Console.WriteLine("Aggregating for param set " + configString);
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();            
            //List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUIDOrderByID(guid);
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();
            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime = SatyamResultValidationToolKit.SortResultsBySubmitTime_OneResultPerTurkerPerTask(entries);

            Dictionary<int, List<MultiObjectTrackingResult>> ResultsPerTask = new Dictionary<int, List<MultiObjectTrackingResult>>();
            List<int> aggregatedTasks = new List<int>();

            int noTotalConverged = 0;
            //int noCorrect = 0;
            int noTerminatedTasks = 0;

            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();

            Dictionary<int, int> noResultsNeededForAggregation = SatyamResultsAnalysis.getNoResultsNeededForAggregationFromLog(configString, guid);
            Dictionary<int, int> noResultsNeededForAggregation_new = new Dictionary<int, int>();
            // play back by time
            foreach (DateTime t in entriesBySubmitTime.Keys)
            {
                //Console.WriteLine("Processing Results of time: {0}", t);
                List<SatyamResultsTableEntry> ResultEntries = entriesBySubmitTime[t];
                foreach (SatyamResultsTableEntry entry in ResultEntries)
                {
                    SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                    MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(task.jobEntry.JobParameters);
                    string fileName = URIUtilities.filenameFromURINoExtension(task.SatyamURI);
                    int taskEntryID = entry.SatyamTaskTableEntryID;
                    if (aggregatedTasks.Contains(taskEntryID))
                    {
                        continue;
                    }
                    if (!ResultsPerTask.ContainsKey(taskEntryID))
                    {
                        ResultsPerTask.Add(taskEntryID, new List<MultiObjectTrackingResult>());
                    }

                    

                    string videoName = URIUtilities.localDirectoryNameFromURI(task.SatyamURI);
                    string[] fields = videoName.Split('_');
                    string videoSequence = fields[fields.Length - 4];
                    int startingFrame = Convert.ToInt32(fields[fields.Length - 1]);
                    int maxChunkEndFrame = startingFrame + job.ChunkDuration * job.FrameRate;
                    int noFrameOverlap = (int)(job.ChunkOverlap * job.FrameRate);
                    if (startingFrame != 0)
                    {
                        startingFrame -= noFrameOverlap;
                    }
                    if (guid == "1e43a983-548d-4a2e-8161-5537eb985902")
                    {
                        videoSequence = MultiObjectTrackingAnalyzer.getCorrectSequenceNo(videoSequence);
                    }
                    
                    // check if this video has already been done
                    if (!overwrite && File.Exists(outputDirectory+ "\\AggregatedStitched_KITTIFormat\\" + configString+"\\data\\" + videoSequence + ".txt"))
                    {
                        //Console.WriteLine("This video aggregation has been done before");
                        continue;
                    }
                    
                   


                    string videoFrameDir = DirectoryConstants.KITTITrackingImages + videoSequence;
                    List<string> files = Directory.GetFiles(videoFrameDir).ToList();
                    List<string> ImageURLs = new List<string>();
                    for (int j = startingFrame; j < files.Count && j < maxChunkEndFrame; j++)
                    {
                        ImageURLs.Add(files[j]);
                    }

                    if (ImageURLs.Count == 0)
                    {
                        continue;
                    }
                    VATIC_DVA_CrowdsourcedResult taskr = new VATIC_DVA_CrowdsourcedResult(satyamResult.TaskResult, videoName, entry.ID.ToString(), ImageURLs.Count, job.FrameRate);

                    //VATIC_DVA_CrowdsourcedResult taskr = VATIC_DVA_CrowdsourcedResult.createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(satyamResult.TaskResult, task.SatyamURI, task.SatyamJobSubmissionsTableEntryID.ToString(), job.FrameRate);
                    MultiObjectTrackingResult res = taskr.getCompressedTracksInTimeSegment();
                    ResultsPerTask[taskEntryID].Add(res);

                    // check log if enough results are collected
                    
                    if (noResultsNeededForAggregation != null && noResultsNeededForAggregation.ContainsKey(taskEntryID) 
                        && ResultsPerTask[taskEntryID].Count < noResultsNeededForAggregation[taskEntryID])
                    {
                        continue;
                    }


                    // hack for masters 
                    int tempMin = MinResults;
                    if (TaskConstants.masterGUIDs.Contains(entry.JobGUID))
                    {
                        tempMin = 1;
                    }

                    MultiObjectTrackingAggregatedResult aggResult = MultiObjectTrackingAggregator.getAggregatedResult(ResultsPerTask[taskEntryID], job.FrameRate,job.BoundaryLines,
                        tempMin, boxToleranceThreshold, ObjectCoverageApprovalThresholdPerVideo, BoxCoverageApprovalThresholdPerTrack, consensusNumber, minTubeletIoUSimilarityThreshold, MaxResults);
                    if (aggResult == null)
                    {
                        continue;
                    }

                    //////////////// aggregation happen
                    // record logs
                    if (noResultsNeededForAggregation == null || !noResultsNeededForAggregation.ContainsKey(taskEntryID))
                    {
                        noResultsNeededForAggregation_new.Add(taskEntryID, ResultsPerTask[taskEntryID].Count);
                    }



                    aggregatedTasks.Add(taskEntryID);
                    noTotalConverged++;
                    if (ResultsPerTask[taskEntryID].Count >= MaxResults)
                    {
                        noTerminatedTasks++;
                    }
                    SatyamAggregatedResult SatyamAggResult = new SatyamAggregatedResult();
                    SatyamAggResult.SatyamTaskTableEntryID = taskEntryID;
                    SatyamAggResult.AggregatedResultString = JSonUtils.ConvertObjectToJSon<MultiObjectTrackingAggregatedResult>(aggResult);
                    SatyamAggResult.TaskParameters = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString).TaskParametersString;

                    SatyamAggregatedResultsTableEntry aggEntry = new SatyamAggregatedResultsTableEntry();
                    aggEntry.SatyamTaskTableEntryID = taskEntryID;
                    aggEntry.JobGUID = entry.JobGUID;
                    aggEntry.ResultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(SatyamAggResult);


                    aggEntries.Add(aggEntry);
                    List<SatyamAggregatedResultsTableEntry> tmpEntries = new List<SatyamAggregatedResultsTableEntry>();
                    tmpEntries.Add(aggEntry);

                    string videoSequenceNo = videoName.Split('_')[2];

                    //Console.WriteLine("Aggregated: {0}", videoName);                    
                    MultiObjectTrackingAnalyzer.SaveKITTIAggregatedResultVideosLocally(tmpEntries, DirectoryConstants.defaultTempDirectory + guid);
                }
            }


            Console.WriteLine("Total_Aggregated_Tasks: {0}", noTotalConverged);
            Console.WriteLine("Total_Terminated_Tasks: {0}", noTerminatedTasks);

            SatyamResultsAnalysis.RecordAggregationLog(noResultsNeededForAggregation_new, configString, guid);

            bool success = ValidateSatyamKITTITrackingAggregationResults(aggEntries, guid, outputDirectory, configString);

            string approvalString = configString + "_PayCover_" + TaskConstants.MULTI_OBJ_TRACKING_APPROVALRATIO_PER_VIDEO_FOR_PAYMENT +
                "_PayIoU_" + TaskConstants.MULTI_OBJ_TRACKING_MIN_TUBELET_SIMILARITY_THRESHOLD_FOR_PAYMENT;

            //for (double prob = 0; prob < 1;prob +=0.2)
            //{
            //    SatyamResultsAnalysis.AnalyzeApprovalRate(aggEntries, entries, guid, approvalString, anotherChanceProbablity: prob);
            //}
            //for (double ratio = 0; ratio < 1; ratio += 0.2)
            //{
            //    SatyamResultsAnalysis.AnalyzeApprovalRate(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guid, configString, approvalRatioThreshold: ratio);
            //}
            SatyamResultsAnalysis.AggregationAnalysis(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guid, configString);

            return success;
        }


        public static MultiObjectTrackingResult DebugMissingFrame(MultiObjectTrackingResult stitched, string videoSequenceNo)
        {
            
            if (MissingFrameIdsPerVideo.ContainsKey(videoSequenceNo))
            {
                foreach(int frameId in MissingFrameIdsPerVideo[videoSequenceNo])
                {
                    DateTime timeOfMissingFrame = stitched.VideoSegmentStartTime + DateTimeUtilities.getTimeSpanFromTotalMilliSeconds(100 * frameId); // 10 fps 100 ms per frame
                    for (int i = 0; i < stitched.tracks.Count; i++)
                    {
                        int noFrames = stitched.tracks[i].spaceTimeTrack.space_time_track.Count;
                        for (int j = 0; j < noFrames; j++)
                        {
                            if (stitched.tracks[i].spaceTimeTrack.space_time_track[j].time >= timeOfMissingFrame)
                            {
                                stitched.tracks[i].spaceTimeTrack.space_time_track[j].time += DateTimeUtilities.getTimeSpanFromTotalMilliSeconds(100);
                            }
                        }

                        foreach (string key in stitched.tracks[i].booleanAttributeTracks.Keys)
                        {
                            int frames = stitched.tracks[i].booleanAttributeTracks[key].attribute_track.Count;
                            for (int j=0;j< frames; j++)
                            {
                                if (stitched.tracks[i].booleanAttributeTracks[key].attribute_track[j].time >= timeOfMissingFrame)
                                {
                                    stitched.tracks[i].booleanAttributeTracks[key].attribute_track[j].time += DateTimeUtilities.getTimeSpanFromTotalMilliSeconds(100);
                                }
                            }
                        }

                    }
                }
               
            }
            return stitched;
            
        }

        public static string writeStitchedResultToFile_KITTIFormat_Tracking(MultiObjectTrackingResult stitched, string videoName, int fps, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory +"\\data\\"))
            {
                Directory.CreateDirectory(outputDirectory + "\\data\\");
            }

            string filePath = outputDirectory + "\\data\\" + videoName + ".txt";
            if (Directory.Exists(filePath))
            {
                return null;
            }

            double frameTimeMilliSeconds = (double)1000 / (double)fps;
            int frameID = 0;
            DateTime start = stitched.VideoSegmentStartTime;
            List<string> lines = new List<string>();
            DateTime end = start;
            foreach(CompressedTrack entity in stitched.tracks)
            {
                if (entity.endTime > end)
                {
                    end = entity.endTime;
                }
            }
            DateTime frameTime = start;
            while (frameTime<= end)
            {
                // for each frame
                
                int noObject = 0;
                for (int i=0;i<stitched.tracks.Count;i++)
                {
                    // for each entity
                    CompressedTrack entity = stitched.tracks[i];
                    SpaceTime st = entity.getSpaceTimeAt(frameTime);
                    BooleanAttribute outofview_attr = entity.getAttributeAt("outofview", frameTime);
                    BooleanAttribute occlusion_attr = entity.getAttributeAt("occlusion", frameTime);
                    if (st != null && outofview_attr != null && !outofview_attr.value)
                    {
                        noObject++;
                        BoundingBox box = st.region;
                        ///////////////////// KITTI only:  remember to change, hacky way for now
                        bool success = box.regularizeByImageSize(1242, 375);
                        if (!success) continue;
                        ////////////////////////////
                        string gt = frameID.ToString() + " " +                    // frame
                            i.ToString() + " " +                          //id
                            entity.label + " " +               // object type [car+ " " + pedestrian+ " " + cyclist+ " " + ...]
                            "0" + " " +                          // truncation [-1+ " " +0+ " " +1+ " " +2]
                            //(occlusion_attr.value ? 1 : 0).ToString() + " " +   // occlusion  [-1+ " " +0+ " " +1+ " " +2]
                            "0" + " " +
                            "-10" + " " +                          // observation angle [rad]
                            box.tlx.ToString() + " " +                    // left   [px]
                            box.tly.ToString() + " " +                    // top    [px]
                            box.brx.ToString() + " " +                    // right  [px]
                            box.bry.ToString() + " " +                    // bottom [px]
                            "-1000" + " " +                      // height [m]
                            "-1000" + " " +                      // width  [m]
                            "-1000" + " " +                      // length [m]
                            "-10" + " " +                        // X [m]
                            "-1" + " " +                         // Y [m]
                            "-1" + " " +                         // Z [m]
                            "-1" + " " +                         // yaw angle [rad]
                            "1";                           // confidence
                            
                        lines.Add(gt);
                    }
                }
                frameID++;
                frameTime = start + DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)frameTimeMilliSeconds * frameID);
            }

            Console.WriteLine("Writing to {0}", filePath);
            //string filePath = outputDirectory + "\\" + videoName.Split('_')[2] + ".txt";
            File.WriteAllLines(filePath, lines.ToArray());
            return filePath;
        }

        public static string writeStitchedResultToFile_KITTIFormat_Detection(MultiObjectTrackingResult stitched, string videoName, int fps, string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory + "\\data-frame\\"))
            {
                Directory.CreateDirectory(outputDirectory + "\\data-frame\\");
            }

            string fileDir = outputDirectory + "\\data-frame\\";

            double frameTimeMilliSeconds = (double)1000 / (double)fps;
            int frameID = 0;
            DateTime start = stitched.VideoSegmentStartTime;
            List<string> lines = new List<string>();
            DateTime end = start;
            foreach (CompressedTrack entity in stitched.tracks)
            {
                if (entity.endTime > end)
                {
                    end = entity.endTime;
                }
            }
            DateTime frameTime = start;
            while (frameTime <= end)
            {
                // for each frame
                List<string> outputs = new List<string>();
                int noObject = 0;
                for (int i = 0; i < stitched.tracks.Count; i++)
                {
                    // for each entity
                    CompressedTrack entity = stitched.tracks[i];
                    SpaceTime st = entity.getSpaceTimeAt(frameTime);
                    BooleanAttribute outofview_attr = entity.getAttributeAt("outofview", frameTime);
                    BooleanAttribute occlusion_attr = entity.getAttributeAt("occlusion", frameTime);
                    if (st != null && outofview_attr != null && !outofview_attr.value)
                    {
                        noObject++;
                        BoundingBox box = st.region;
                        ///////////////////// KITTI only:  remember to change, hacky way for now
                        bool success = box.regularizeByImageSize(1242, 375);
                        if (!success) continue;
                        ////////////////////////////
                        string s = entity.label + " 0 0 0 " +
                            box.tlx.ToString() + " " +
                            box.tly.ToString() + " " +
                            box.brx.ToString() + " " +
                            box.bry.ToString() + " 0 0 0 0 0 0 0";
                        outputs.Add(s);
                    }
                }

                Console.WriteLine("Writing to {0} on frame {1}", fileDir, frameID);
                string filePath = fileDir + "\\" + videoName + "_frame_" + frameID.ToString("000000") + ".txt";
                
                File.WriteAllLines(filePath, outputs.ToArray());
                

                frameID++;
                frameTime = start + DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)frameTimeMilliSeconds * frameID);
            }

            // hack for empty ground truth generation
            while (frameID < 30)
            {
                
                List<string> outputs = new List<string>();
                string filePath = fileDir + "\\" + videoName + "_frame_" + frameID.ToString("000000") + ".txt";

                File.WriteAllLines(filePath, outputs.ToArray());
                frameID++;
            }

            return fileDir;
        }


    }
}
