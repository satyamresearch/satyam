using AzureBlobStorage;
using Constants;
using JobTemplateClasses;
using SatyamResultAggregators;
using SatyamResultClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static SatyamResultAggregators.TrackletLabelingAggregator;

namespace SatyamAnalysis
{
    public class TrackletLabelingAnalyzer
    {
        public static void SaveResultVideosLocally(string guid)
        {
            string outputDirectory = DirectoryConstants.defaultTempDirectory + guid;
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();
            SaveResultVideosLocally(entries, outputDirectory);
        }


        public static void SaveAggregatedResultVideosLocally(string guid)
        {
            string outputDirectory = DirectoryConstants.defaultTempDirectory + guid;
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> entries = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();
            //SaveAggregatedResultVideosLocally(entries, outputDirectory);
            GroupEntriesByVideoNameAndStitchAndSaveAggregatedResultVideosLocally(entries, outputDirectory);
        }


        public static void SaveResultVideosLocally(List<SatyamResultsTableEntry> entries, string directoryName, int fps = 10)
        {
            directoryName = directoryName + "\\Raw";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();

            


            for (int i = 0; i < entries.Count; i++)
            {
                SatyamResultsTableEntry entry = entries[i];
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(task.jobEntry.JobParameters);

                string blobDir = URIUtilities.localDirectoryFullPathFromURI(task.SatyamURI);

                //VATIC_DVA_CrowdsourcedResult taskr = new VATIC_DVA_CrowdsourcedResult(satyamResult.TaskResult, task.SatyamURI, start, end, task.SatyamJobSubmissionsTableEntryID.ToString());
                VATIC_DVA_CrowdsourcedResult taskr = MultiObjectTrackingAggregator.createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(satyamResult.TaskResult, blobDir, entry.ID.ToString(), job.FrameRate);
                MultiObjectTrackingResult res = taskr.getCompressedTracksInTimeSegment();

                List<string> ImageURLs = satyamStorage.getURLListOfSpecificExtensionUnderSubDirectoryByURI(blobDir, new List<string>() { "jpg", "png" });
                string videoName = URIUtilities.localDirectoryNameFromURI(blobDir) + "_" + URIUtilities.filenameFromURINoExtension(task.SatyamURI) + "_" + entry.ID;

                MultiObjectTrackingAnalyzer.generateVideoForEvaluation(ImageURLs, res, directoryName, videoName, job.FrameRate);
            }
        }

        public static void SaveAggregatedResultVideosLocally(List<SatyamAggregatedResultsTableEntry> entries, string directoryName)
        {
            directoryName = directoryName + "\\Aggregated";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();

            for (int i = 0; i < entries.Count; i++)
            {
                SatyamAggregatedResultsTableEntry entry = entries[i];
                SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
                SatyamTask aggTask = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamAggResult.TaskParameters);
                MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(aggTask.jobEntry.JobParameters);
                TrackletLabelingAggregatedResult aggresult = JSonUtils.ConvertJSonToObject<TrackletLabelingAggregatedResult>(satyamAggResult.AggregatedResultString);

                WebClient wb = new WebClient();
                Stream aggTrackStream = wb.OpenRead(aggresult.AggregatedTrackletsString_URL);
                StreamReader reader = new StreamReader(aggTrackStream);
                String aggTrackString = reader.ReadToEnd();

                MultiObjectTrackingResult aggTracks = JSonUtils.ConvertJSonToObject<MultiObjectTrackingResult>(aggTrackString);

                string blobDir = URIUtilities.localDirectoryFullPathFromURI(aggTask.SatyamURI);
                List<string> ImageURLs = satyamStorage.getURLListOfSpecificExtensionUnderSubDirectoryByURI(blobDir, new List<string>() { "jpg", "png" });


                string videoName = URIUtilities.localDirectoryNameFromURI(blobDir) + "_" + URIUtilities.filenameFromURINoExtension(aggTask.SatyamURI) + "_" + entry.ID;

                MultiObjectTrackingAnalyzer.generateVideoForEvaluation(ImageURLs, aggTracks, directoryName, videoName, job.FrameRate);
            }
        }

        public static void GroupEntriesByVideoNameAndStitchAndSaveAggregatedResultVideosLocally(List<SatyamAggregatedResultsTableEntry> entries, string directoryName)
        {
            if (entries.Count == 0) return;
            directoryName = directoryName + "\\AggregatedStitched";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }


            SortedDictionary<string, List<SatyamAggregatedResultsTableEntry>> entriesPerVideo = MultiObjectTrackingAnalyzer.GroupChunksByVideoName(entries);

            foreach (string videoname in entriesPerVideo.Keys)
            {
                List<string> ImageURLs;
                int fps;
                MultiObjectTrackingResult stitched = stitchAllChunksAllObjectsOfOneVideo(entriesPerVideo[videoname], out ImageURLs, out fps);
                if (stitched != null)
                {
                    // incase video is too large
                    MultiObjectTrackingAnalyzer.generateVideoFramesForEvaluation(ImageURLs, stitched, directoryName, videoname, fps);

                    //MultiObjectTrackingAnalyzer.generateVideoForEvaluation(ImageURLs, stitched, directoryName, videoname, fps);
                }

            }
        }


        public static MultiObjectTrackingResult stitchAllChunksAllObjectsOfOneVideo(List<SatyamAggregatedResultsTableEntry> entries, out List<string> ImageURLs, out int fps)
        {
            ImageURLs = new List<string>();
            fps = 0;
            if (entries.Count == 0) return null;

            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            MultiObjectTrackingResult stitched = new MultiObjectTrackingResult();

            int totalFrameCounts = 0;

            // ensure the order is correct
            SortedDictionary<int, List<SatyamAggregatedResultsTableEntry>> sortedEntries = new SortedDictionary<int, List<SatyamAggregatedResultsTableEntry>>();
            List<int> idx = new List<int>();
            for (int i = 0; i < entries.Count; i++)
            {
                SatyamAggregatedResultsTableEntry entry = entries[i];
                SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
                SatyamTask aggTask = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamAggResult.TaskParameters);
                string video = URIUtilities.localDirectoryNameFromURI(aggTask.SatyamURI);
                string[] fields = video.Split('_');

                int startingFrame = Convert.ToInt32(fields[fields.Length - 1]);
                if (!sortedEntries.ContainsKey(startingFrame))
                {
                    sortedEntries.Add(startingFrame, new List<SatyamAggregatedResultsTableEntry>());
                    idx.Add(startingFrame);
                }
                sortedEntries[startingFrame].Add(entries[i]);
            }


            idx.Sort();


            List<string> AggObjIds = new List<string>();
            for (int i = 0; i < idx.Count; i++)
            {
                int noFramesOverlap = 0;
                string blobDir = "";
                // grouping all objects that belong to the same chunk
                MultiObjectTrackingResult aggTracksOfAllObjectsPerChunk = new MultiObjectTrackingResult();

                List<string> objIds = new List<string>();
                for (int j = 0; j < sortedEntries[idx[i]].Count; j++)
                {
                    SatyamAggregatedResultsTableEntry entry = sortedEntries[idx[i]][j];
                    SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
                    SatyamTask aggTask = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamAggResult.TaskParameters);
                    MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(aggTask.jobEntry.JobParameters);
                    if (job.ChunkOverlap != 0.0)
                    {
                        noFramesOverlap = (int)(job.ChunkOverlap * job.FrameRate);
                    }
                    fps = job.FrameRate;
                    blobDir = URIUtilities.localDirectoryFullPathFromURI(aggTask.SatyamURI);

                    string objId = URIUtilities.filenameFromURINoExtension(aggTask.SatyamURI);
                    objIds.Add(objId);

                    TrackletLabelingAggregatedResult aggresult = JSonUtils.ConvertJSonToObject<TrackletLabelingAggregatedResult>(satyamAggResult.AggregatedResultString);

                    WebClient wb = new WebClient();
                    Stream aggTrackStream = wb.OpenRead(aggresult.AggregatedTrackletsString_URL);
                    StreamReader reader = new StreamReader(aggTrackStream);
                    String aggTrackString = reader.ReadToEnd();

                    MultiObjectTrackingResult aggTracks = JSonUtils.ConvertJSonToObject<MultiObjectTrackingResult>(aggTrackString);
                    if (aggTracksOfAllObjectsPerChunk.tracks.Count == 0)
                    {
                        aggTracksOfAllObjectsPerChunk = aggTracks;
                    }
                    else
                    {
                        for(int k = 0; k < aggTracks.tracks.Count; k++)
                        {
                            aggTracksOfAllObjectsPerChunk.tracks.Add(aggTracks.tracks[k]);
                        }
                    }
                }

                List<string> TraceURLs = satyamStorage.getURLListOfSpecificExtensionUnderSubDirectoryByURI(blobDir, new List<string>() { "jpg", "png" });

                if (i == 0)
                {
                    ImageURLs.AddRange(TraceURLs);
                    
                    stitched = aggTracksOfAllObjectsPerChunk;
                    totalFrameCounts += TraceURLs.Count;
                    AggObjIds = objIds;
                }
                else
                {
                    int noNewFrames = 0;
                    for (int j = noFramesOverlap; j < TraceURLs.Count; j++)
                    {
                        ImageURLs.Add(TraceURLs[j]);
                        noNewFrames++;
                    }

                    //stitched = MultiObjectTrackingAnalyzer.stitchTwoTracesByTubeletsOfOverlappingVideoChunk(stitched,
                    //    aggTracksOfAllObjectsPerChunk, totalFrameCounts, totalFrameCounts + noNewFrames, noFramesOverlap, fps);

                    // postpone the agg trace
                    double frameTimeInMiliSeconds = (double)1000 / (double)fps;
                    double timeToPostponeInMilliSeconds = (double)(totalFrameCounts - noFramesOverlap) * frameTimeInMiliSeconds;
                    //TimeSpan timeSpanToPostponeInSeconds = new TimeSpan(0,0,0,0, (int)timeToPostponeInMilliSeconds);
                    TimeSpan timeSpanToPostponeInSeconds = DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)timeToPostponeInMilliSeconds);
                    aggTracksOfAllObjectsPerChunk.postpone(timeSpanToPostponeInSeconds);

                    // overlap must be 0
                    for (int k = 0; k < aggTracksOfAllObjectsPerChunk.tracks.Count; k++)
                    {
                        if (!AggObjIds.Contains(objIds[k]))
                        {
                            stitched.tracks.Add(aggTracksOfAllObjectsPerChunk.tracks[k]);
                            AggObjIds.Add(objIds[k]);
                        }
                        else
                        {
                            // stitch the track for the same id
                            int tckIdx = AggObjIds.IndexOf(objIds[k]);
                            stitched.tracks[tckIdx] = CompressedTrack.stitchTwoAdjacentTrack(stitched.tracks[tckIdx], aggTracksOfAllObjectsPerChunk.tracks[k]);
                        }
                    }
                    


                    totalFrameCounts += noNewFrames;
                }

                //debug
                //generateVideoForEvaluation(ImageURLs, stitched, directoryName + "_" + i, videoName, fps);
            }
            return stitched;
        }
    }
}
