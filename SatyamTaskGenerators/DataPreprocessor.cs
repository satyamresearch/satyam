using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AzureBlobStorage;
using Constants;
using JobTemplateClasses;
using SQLTables;
using Utilities;

namespace SatyamTaskGenerators
{
    public static class DefaultDataPreprocessor
    {
        public static bool copyDataFromUserBlobToSatyamBlob(SatyamJobSubmissionsTableAccessEntry jobEntry)
        {
            
            string satyamContainerName = SatyamTaskGenerator.JobTemplateToSatyamContainerNameMap[jobEntry.JobTemplateType];
            string GUID = jobEntry.JobGUID;
            string satyamDirectoryName = GUID;
            SatyamJob job = JSonUtils.ConvertJSonToObject<SatyamJob>(jobEntry.JobParametersString);
            BlobContainerManager bcm = new BlobContainerManager();
            string status = bcm.Connect(job.azureInformation.AzureBlobStorageConnectionString);
            List<string> FileTypes = SatyamTaskGenerator.ValidFileTypesByTemplate[job.JobTemplateType];
            if (status != "SUCCESS")
            {
                return false;
            }
            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            satyamStorage.copyFilesFromAnotherAzureBlob(bcm, job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName, satyamContainerName, satyamDirectoryName, FileTypes);
            return true;
        }
    }

    public static class VideoClassficationPreprocessor
    {
        public static bool ProcessAndUploadToAzureBlob(SatyamJobSubmissionsTableAccessEntry jobEntry)
        {
            // Assumpltion:  the input is a folder videos, convert to mp4, and upload to satyam blob
            
            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            string satyamContainerName = SatyamTaskGenerator.JobTemplateToSatyamContainerNameMap[jobEntry.JobTemplateType];
            string GUID = jobEntry.JobGUID;
            string satyamDirectoryName = GUID;
            SatyamJob job = JSonUtils.ConvertJSonToObject<SatyamJob>(jobEntry.JobParametersString);
            SingleObjectLabelingSubmittedJob jobParams = JSonUtils.ConvertJSonToObject<SingleObjectLabelingSubmittedJob>(job.JobParameters);

            BlobContainerManager bcm = new BlobContainerManager();
            string status = bcm.Connect(job.azureInformation.AzureBlobStorageConnectionString);
            List<string> FileTypes = SatyamTaskGenerator.ValidFileTypesByTemplate[job.JobTemplateType];
            if (status != "SUCCESS")
            {
                return false;
            }

            string guidFolder = DirectoryConstants.defaultTempDirectory + "\\" + GUID;
            Directory.CreateDirectory(guidFolder);

            //var client = new WebClient();
            List<string> videoUrls = bcm.getURLList(job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName);
            foreach (string video in videoUrls)
            {
                FFMpegWrappers.ConvertVideoURLToMP4(DirectoryConstants.ffmpeg, video, guidFolder);
            }

            satyamStorage.uploadALocalFolder(guidFolder, satyamContainerName, satyamDirectoryName);

            return true;
        }
    }

    public static class TrackingDataPreprocessor
    {
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

            if (jobParams.DataSrcFormat == DataFormat.Video)
            {
                // sample to frames
                int noFramePerChunk = (int)(chunkLength * outputFPS);
                int noFrameOverlap = (int)(chunkOverlap * outputFPS);
                List<string> videoUrls = bcm.getURLList(job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName);
                foreach(string video in videoUrls)
                {
                    string videoName = URIUtilities.filenameFromURINoExtension(video);
                    string videonameExtension = URIUtilities.filenameFromURI(video);
                    string outputDirectory = guidFolder + "\\" + videoName;
                    Directory.CreateDirectory(outputDirectory);
                    client.DownloadFile(video, outputDirectory + "\\" + videonameExtension);
                    FFMpegWrappers.ExtractFrames( DirectoryConstants.ffmpeg, outputDirectory + "\\" + videonameExtension, outputDirectory, videoName, DateTime.Now, outputFPS);
                    Console.WriteLine("deleting downloaded file..."); 
                    File.Delete(outputDirectory + "\\" + videonameExtension);
                    GroupFramesIntoChunksAndUploadChunks(videoName, outputDirectory, noFramePerChunk, noFrameOverlap, satyamContainerName, satyamDirectoryName);
                    Directory.Delete(outputDirectory, true);

                }
            }

            if (jobParams.DataSrcFormat == DataFormat.VideoFrame)
            {
                int noFramePerChunk = (int)(chunkLength * outputFPS);//just use one fps for now, assume input should've already downsampled
                int noFrameOverlap = (int)(chunkOverlap * outputFPS);
                // chunk according to parameters
                List<string> frameUrls = bcm.getURLList(job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName);
                Dictionary<string, List<string>> framesPerVideo = new Dictionary<string, List<string>>();
                foreach(string url in frameUrls)
                {
                    // assumed hierarchy: blob/directory.../videoname/frameid.jpg 
                    //string frameName = URIUtilities.filenameFromURINoExtension(url);
                    string[] urlparts = url.Split('/');                    
                    string videoName = urlparts[urlparts.Length - 2];
                    if (!framesPerVideo.ContainsKey(videoName))
                    {
                        framesPerVideo.Add(videoName, new List<string>());
                    }
                    framesPerVideo[videoName].Add(url);
                }
                
                foreach (string video in framesPerVideo.Keys)
                {
                    string outputDirectory = guidFolder + "\\" + video;
                    Directory.CreateDirectory(outputDirectory);
                    foreach(string frameURL in framesPerVideo[video])
                    {
                        string frameName = URIUtilities.filenameFromURI(frameURL);
                        client.DownloadFile(frameURL, outputDirectory + "\\" + frameName);
                    }
                    GroupFramesIntoChunksAndUploadChunks(video, outputDirectory, noFramePerChunk, noFrameOverlap, satyamContainerName, satyamDirectoryName);
                    Directory.Delete(outputDirectory, true);
                }
            }
            return true;
        }

        //public static void GroupFramesIntoChunksAndUploadChunks(string videoName, string CurrentDir, int noFramePerChunk, int noFramesOverlap, string satyamContainerName, string satyamDirectoryName)
        //{
        //    SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
        //    List<string> files = Directory.GetFiles(CurrentDir).ToList();
        //    int chunkSequence = 0;
        //    List<List<string>> allFilesPerChunk = new List<List<string>>();
        //    for(int i=0;i<files.Count;i++)
        //    {
                
        //        int index = (int)Math.Ceiling((double)(i + 1) / (double)noFramePerChunk);
        //        if ( index > chunkSequence)
        //        {
                    
        //            allFilesPerChunk.Add(new List<string>());
        //            chunkSequence++;
        //            if (chunkSequence > 1)
        //            {
        //                //starting the 2nd chunk, add frames before 
        //                for (int j = 0; j < noFramesOverlap; j++)
        //                {
        //                    allFilesPerChunk[chunkSequence-1].Add(files[i - noFramesOverlap + j]);
        //                }
        //            }                   
        //        }
        //        allFilesPerChunk[index - 1].Add(files[i]);
        //    }
        //    for (int i = 0; i < allFilesPerChunk.Count; i++)
        //    {
        //        string subDir = CurrentDir + "\\" + i;
        //        Directory.CreateDirectory(subDir);
        //        foreach(string file in allFilesPerChunk[i])
        //        {
        //            string fileName = URIUtilities.filenameFromDirectory(file);
        //            File.Copy(file, subDir + "\\" + fileName);
        //        }
        //        satyamStorage.uploadALocalFolder(subDir, satyamContainerName, satyamDirectoryName +"/Video_" + videoName + "_startingFrame_" + noFramePerChunk * i);
        //    }
        //}

        public static List<string> GroupFramesIntoChunks(string CurrentDir, int noFramePerChunk, int noFramesOverlap=0)
        {
            List<string> files = Directory.GetFiles(CurrentDir).ToList();
            int chunkSequence = 0;
            List<List<string>> allFilesPerChunk = new List<List<string>>();
            for (int i = 0; i < files.Count; i++)
            {

                int index = (int)Math.Ceiling((double)(i + 1) / (double)noFramePerChunk);
                if (index > chunkSequence)
                {

                    allFilesPerChunk.Add(new List<string>());
                    chunkSequence++;
                    if (chunkSequence > 1)
                    {
                        //starting the 2nd chunk, add frames before 
                        for (int j = 0; j < noFramesOverlap; j++)
                        {
                            allFilesPerChunk[chunkSequence - 1].Add(files[i - noFramesOverlap + j]);
                        }
                    }
                }
                allFilesPerChunk[index - 1].Add(files[i]);
            }

            List<string> chunkFolders = new List<string>();
            for (int i = 0; i < allFilesPerChunk.Count; i++)
            {
                string subDir = CurrentDir + "\\" + i;
                Directory.CreateDirectory(subDir);
                chunkFolders.Add(subDir);
                foreach (string file in allFilesPerChunk[i])
                {
                    string fileName = URIUtilities.filenameFromDirectory(file);
                    File.Copy(file, subDir + "\\" + fileName);
                }
            }

            return chunkFolders;
        }


        public static void GroupFramesIntoChunksAndUploadChunks(string videoName, string CurrentDir, int noFramePerChunk, int noFramesOverlap, string satyamContainerName, string satyamDirectoryName)
        {
            List<string> chunkFolders = GroupFramesIntoChunks(CurrentDir, noFramePerChunk, noFramesOverlap);

            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            

            for (int i = 0; i < chunkFolders.Count; i++)
            {
                string subDir = chunkFolders[i];
                satyamStorage.uploadALocalFolder(subDir, satyamContainerName, satyamDirectoryName + "/Video_" + videoName + "_startingFrame_" + noFramePerChunk * i);
            }
        }
    }

    
}
