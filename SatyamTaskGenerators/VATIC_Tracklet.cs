using AzureBlobStorage;
using Constants;
using JobTemplateClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SatyamTaskGenerators
{
    

    public class VATIC_Tracklet
    {
        public int label;
        public List<List<object>> boxes;
        public List<List<string>> attributes;

        public VATIC_Tracklet()
        {
            boxes = new List<List<object>>();
            attributes = new List<List<string>>();
        }

        public static Dictionary<string, VATIC_Tracklet> ReadTrackletsFromVIRAT(List<string> traces)
        {
            //List<string> traces = System.IO.File.ReadLines(filepath).ToList();
            Dictionary<string, VATIC_Tracklet> tracklets = new Dictionary<string, VATIC_Tracklet>();

            foreach(string line in traces)
            {
                string[] elem = line.Split(' ');
                string objId = elem[0];
                int objType = Convert.ToInt32(elem[7]);
                int frameId = Convert.ToInt32(elem[2]);
                int xtl = Convert.ToInt32(elem[3]);
                int ytl = Convert.ToInt32(elem[4]);
                int width = Convert.ToInt32(elem[5]);
                int height = Convert.ToInt32(elem[6]);

                int xbr = xtl + width;
                int ybr = ytl + height;

                List<object> formatedFrame = new List<object>();

                formatedFrame.Add(xtl);
                formatedFrame.Add(ytl);
                formatedFrame.Add(xbr);
                formatedFrame.Add(ybr);
                formatedFrame.Add(frameId);
                formatedFrame.Add(false);// out of view
                formatedFrame.Add(false);// occlusion


                if (!tracklets.ContainsKey(objId))
                {
                    tracklets.Add(objId, new VATIC_Tracklet());
                    tracklets[objId].label = objType;
                }
                tracklets[objId].boxes.Add(formatedFrame);
                //tracklets[objId].attributes.Add();

            }
            return tracklets;
        }


        public static void writeTrackletsIntoVIRAT(Dictionary<string, VATIC_Tracklet> tracklets, string outputFilePath)
        {
            List<string> output = new List<string>();
            foreach (string key in tracklets.Keys)
            {
                VATIC_Tracklet tck = tracklets[key];

                for (int i = 0; i < tck.boxes.Count; i++)
                {
                    string objId = key;
                    int objType = tck.label;
                    int frameId = Convert.ToInt32(tck.boxes[i][4]);
                    int xtl = Convert.ToInt32(tck.boxes[i][0]);
                    int ytl = Convert.ToInt32(tck.boxes[i][1]);
                    int xbr = Convert.ToInt32(tck.boxes[i][2]);
                    int ybr = Convert.ToInt32(tck.boxes[i][3]);
                    int width = xbr-xtl;
                    int height = ybr-ytl;

                    List<string> line = new List<string>();
                    line.Add(objId);
                    line.Add("0");
                    line.Add(frameId.ToString());
                    line.Add(xtl.ToString());
                    line.Add(ytl.ToString());
                    line.Add(width.ToString());
                    line.Add(height.ToString());
                    line.Add(objType.ToString());

                    string ln = ObjectsToStrings.ListString(line,' ');
                    output.Add(ln);
                }

                File.WriteAllLines(outputFilePath, output);
                
            }
        }

        

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="GUID"></param>
        ///// <param name="chunkLength"></param>
        ///// <param name="noFramePerChunk"></param> fps must be the same as the annotation file.
        //public static List<string> getVideoChunksWithAnnotationsPerObject(string videoName, string currDir, int chunkLength, int noFramePerChunk)
        //{
            

        //    //string tmpLocalPath = @"C:\research\dataset\ComplexActivity\VATIC_virat_files\";
        //    //string tmpTestVideo = "VIRAT_S_000002.mp4";
        //    //string tmpAnnoFile = "VIRAT_S_000002.viratdata.objects.txt";



        //    ////var client = new WebClient();

        //    ////int noFrameOverlap = (int)(chunkOverlap * outputFPS);
        //    ////List<string> videoUrls = bcm.getURLList(job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName);
        //    ////foreach (string video in videoUrls)
        //    ////{

        //    ////string videoName = URIUtilities.filenameFromURINoExtension(video);
        //    ////string videoName = "VIRAT_S_000002";
        //    ////string videonameExtension = URIUtilities.filenameFromURI(video);
        //    //string outputDirectory = currDir + "\\" + videoName;

        //    //// must be a new clean directory
        //    //if (Directory.Exists(outputDirectory))
        //    //{
        //    //    Directory.Delete(outputDirectory, true);
        //    //}

        //    //Directory.CreateDirectory(outputDirectory);
        //    //client.DownloadFile(video, outputDirectory + "\\" + videonameExtension);
        //    //FFMpegWrappers.ExtractFrames(DirectoryConstants.ffmpeg, outputDirectory + "\\" + videonameExtension, outputDirectory, videoName, DateTime.Now, outputFPS);
        //    //FFMpegWrappers.ExtractFrames(DirectoryConstants.ffmpeg, tmpLocalPath + "\\" + tmpTestVideo, outputDirectory, videoName, DateTime.Now, outputFPS);
        //    //File.Delete(outputDirectory + "\\" + videonameExtension);

        //    // Group
        //    List<string> chunkFolders = TrackingDataPreprocessor.GroupFramesIntoChunks(outputDirectory, noFramePerChunk);

            

        //    // Chunk Annotations Per Object
        //    List<string> traces = File.ReadLines(tmpLocalPath + tmpAnnoFile).ToList();
        //    Dictionary<int, Dictionary<string, List<string>>> annotationsPerObjectPerChunk = new Dictionary<int, Dictionary<string, List<string>>>();
        //    foreach (string line in traces)
        //    {
        //        string[] elem = line.Split(' ');
        //        string objId = elem[0];
        //        int frameId = Convert.ToInt32(elem[2]);
        //        int noChunk = frameId / noFramePerChunk;
        //        if (!annotationsPerObjectPerChunk.ContainsKey(noChunk))
        //        {
        //            annotationsPerObjectPerChunk.Add(noChunk, new Dictionary<string, List<string>>());
        //        }
        //        if (!annotationsPerObjectPerChunk[noChunk].ContainsKey(objId))
        //        {
        //            annotationsPerObjectPerChunk[noChunk].Add(objId, new List<string>());
        //        }
        //        annotationsPerObjectPerChunk[noChunk][objId].Add(line);

        //    }

        //    // output annotation file per object per chunk
        //    foreach (int chunkID in annotationsPerObjectPerChunk.Keys)
        //    {
        //        foreach (string obj in annotationsPerObjectPerChunk[chunkID].Keys)
        //        {
        //            string filePath = chunkFolders[chunkID] + "\\" + obj + ".txt";
        //            File.WriteAllLines(filePath, annotationsPerObjectPerChunk[chunkID][obj]);
        //        }
        //    }

        //    return chunkFolders;

        //}


        /// <summary>
        /// Assume input FPS = 30, downsample and chunk the annotations according to the outputFPS number.
        /// </summary>
        /// <param name="chunkFolders"></param>
        /// <param name="annotationFile"></param>
        /// <param name="noFramePerChunk"></param>
        /// <param name="FPS"></param>
        public static void parseAnnotationFileIntoChunkFolders(List<string> chunkFolders, string annotationFile, int noFramePerChunk, int outputFPS, int inputFPS = -1)
        {
            List<int> frameCount = new List<int>();

            foreach (string folder in chunkFolders)
            {
                int count = Directory.GetFiles(folder).Length;
                frameCount.Add(count);
            }

            if (inputFPS == -1) inputFPS = outputFPS;

            int sampleRate = inputFPS / outputFPS; // regularize to the floor rate.
            if (sampleRate == 0)
            {
                Console.WriteLine("Error: sample rate is 0. Invalid outputFPS vs inputFPS.");
                return;
            }else if (inputFPS % outputFPS != 0)
            {
                Console.WriteLine("Error: Invalid outputFPS vs inputFPS. Not dividable.");
                return;
            }

            // Chunk Annotations Per Object
            List<string> traces = File.ReadLines(annotationFile).ToList();
            Dictionary<int, Dictionary<string, List<string>>> annotationsPerObjectPerChunk = new Dictionary<int, Dictionary<string, List<string>>>();
            foreach (string line in traces)
            {
                string[] elem = line.Split(' ');
                string objId = elem[0];
                int frameId = Convert.ToInt32(elem[2]);
                int objType = Convert.ToInt32(elem[7]);

                // obj type filtering
                List<int> ObjTypeFilter = new List<int>() { 1 };
                if (!ObjTypeFilter.Contains(objType)) continue;

                if (frameId % sampleRate != 0) continue; // filter by fps

                int SampledFrameID = frameId / sampleRate;

                int noChunk = SampledFrameID / noFramePerChunk;
                if (frameCount.Count <= noChunk || noChunk <0) continue; // the video is less than the annotation file.

                if (!annotationsPerObjectPerChunk.ContainsKey(noChunk))
                {
                    annotationsPerObjectPerChunk.Add(noChunk, new Dictionary<string, List<string>>());
                }
                if (!annotationsPerObjectPerChunk[noChunk].ContainsKey(objId))
                {
                    annotationsPerObjectPerChunk[noChunk].Add(objId, new List<string>());
                }

                // de-indent the frameId  
                string newline = "";
                int deindentedFrameID = -1;
                for (int i= 0; i < elem.Length; i++ )
                {
                    
                    if (i == 2)
                    {
                        deindentedFrameID = SampledFrameID - noChunk * noFramePerChunk;
                        newline += deindentedFrameID.ToString() + " ";
                    }
                    else
                    {
                        newline += elem[i];
                        if (i == elem.Length - 1) break;
                        newline += " ";
                    }
                }
                if (deindentedFrameID ==-1 || deindentedFrameID >= frameCount[noChunk])
                {
                    continue;
                }
                annotationsPerObjectPerChunk[noChunk][objId].Add(newline);

            }

            // output annotation file per object per chunk
            foreach (int chunkID in annotationsPerObjectPerChunk.Keys)
            {
                if (chunkID >= chunkFolders.Count) continue;
                foreach (string obj in annotationsPerObjectPerChunk[chunkID].Keys)
                {
                    string filePath = chunkFolders[chunkID] + "\\" + obj + ".txt";
                    File.WriteAllLines(filePath, annotationsPerObjectPerChunk[chunkID][obj]);
                }
            }
        }

        

        /// <summary>
        /// Input should be a folder of videos and corresponding annotation file with the same name
        /// </summary>
        /// <param name="jobEntry"></param>
        public static bool ProcessAndUploadToAzureBlob(SatyamJobSubmissionsTableAccessEntry jobEntry)
        {
            // if the input is a folder of folders of frames, then copy directly
            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            string satyamContainerName = SatyamTaskGenerator.JobTemplateToSatyamContainerNameMap[jobEntry.JobTemplateType];
            string GUID = jobEntry.JobGUID;
            string satyamDirectoryName = GUID;
            SatyamJob job = JSonUtils.ConvertJSonToObject<SatyamJob>(jobEntry.JobParametersString);
            MultiObjectTrackingSubmittedJob jobParams = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(job.JobParameters);

            BlobContainerManager bcm = new BlobContainerManager();
            string status = bcm.Connect(job.azureInformation.AzureBlobStorageConnectionString);
            List<string> FileTypes = SatyamTaskGenerator.ValidFileTypesByTemplate[job.JobTemplateType];
            if (status != "SUCCESS")
            {
                return false;
            }

            string guidFolder = DirectoryConstants.defaultTempDirectory + "\\" + GUID;
            Directory.CreateDirectory(guidFolder);
            int chunkLength = jobParams.ChunkDuration; // sec
            int outputFPS = jobParams.FrameRate;
            double chunkOverlap = jobParams.ChunkOverlap; // sec

            var client = new WebClient();

            // sample to frames
            int noFramePerChunk = (int)(chunkLength * outputFPS);
            int noFrameOverlap = (int)(chunkOverlap * outputFPS);
            List<string> videoUrls = bcm.getURLList(job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName);


            Dictionary<string, List<string>> videos = new Dictionary<string, List<string>>();
            foreach (string videolink in videoUrls)
            {
                string videoName = URIUtilities.filenameFromURINoExtension(videolink);
                
                if (!videos.ContainsKey(videoName))
                {
                    videos.Add(videoName, new List<string>());
                }
                videos[videoName].Add(videolink);
            }

            foreach (string videoName in videos.Keys)
            {
                // filter out those that doesn't provide a annotation file with it....
                if (videos[videoName].Count != 2)
                {
                    Console.WriteLine("Warning: Not 2 files provided for {0}.", videoName);
                    //Directory.Delete(guidFolder, true);
                    //return false;
                    continue;
                }

                string videoURL = "";
                string annotationURL ="";
                foreach (string fileLink in videos[videoName])
                {
                    string extension = URIUtilities.fileExtensionFromURI(fileLink);
                    if (extension != "txt")
                    {
                        videoURL = fileLink;
                    }
                    else
                    {
                        annotationURL = fileLink;
                    }
                }

                string outputDirectory = guidFolder + "\\" + videoName;
                Directory.CreateDirectory(outputDirectory);

                string videoNameWithExtension = URIUtilities.filenameFromURI(videoURL);
                client.DownloadFile(videoURL, outputDirectory + "\\" + videoNameWithExtension);
                FFMpegWrappers.ExtractFrames(DirectoryConstants.ffmpeg, outputDirectory + "\\" + videoNameWithExtension, outputDirectory, videoName, DateTime.Now, outputFPS);

                File.Delete(outputDirectory + "\\" + videoNameWithExtension);


                List<string> chunkFolders = TrackingDataPreprocessor.GroupFramesIntoChunks(outputDirectory, noFramePerChunk);

                //parse VIRAT annotation file
                string annotationNameWithExtension = URIUtilities.filenameFromURI(annotationURL);
                client.DownloadFile(annotationURL, outputDirectory + "\\" + annotationNameWithExtension);

                parseAnnotationFileIntoChunkFolders(chunkFolders, 
                    outputDirectory + "\\" + annotationNameWithExtension, 
                    noFramePerChunk, outputFPS);

                //upload
                for (int i = 0; i < chunkFolders.Count; i++)
                {
                    string subDir = chunkFolders[i];
                    satyamStorage.uploadALocalFolder(subDir, satyamContainerName, satyamDirectoryName + "/Video_" + videoName + "_startingFrame_" + noFramePerChunk * i);
                }

                Directory.Delete(outputDirectory, true);

            }
            return true;
        }

    }
}
