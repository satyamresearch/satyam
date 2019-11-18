using AzureBlobStorage;
using Constants;
using HelperClasses;
using JobTemplateClasses;
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
using System.Threading.Tasks;
using UsefulAlgorithms;
using Utilities;

namespace SatyamAnalysis
{
    public class MultiObjectTrackingAnalyzer
    {

        // this is to compensate for wrong folder organizations on azure
        public static Dictionary<string, string> VideoSequenceMap = new Dictionary<string, string>()
        {
            { "0012","0013" },
            { "0013","0014" },
            { "0014","0015" },
            { "0015","0016" },
            { "0016","0017" },
            { "0017","0018" },
            { "0018","0019" },
            { "0019","0020" },
            { "0020","00xx" },
        };

        public static Dictionary<string, int> totalFrames = new Dictionary<string, int>()
        {
            { "0000", 154},
            { "0001", 447},
            { "0002", 233},
            { "0003", 144},
            { "0004", 314},
            { "0005", 297},
            { "0006", 270},
            { "0007", 800},
            { "0008", 390},
            { "0009", 803},
            { "0010", 294},
            { "0011", 373},
            { "0012", 78},
            { "0013", 340},
            { "0014", 106},
            { "0015", 376},
            { "0016", 209},
            { "0017", 145},
            { "0018", 339},
            { "0019", 1059},
            { "0020", 837},

        };

        public static string getCorrectSequenceNo(string videoSequenceNo)
        {
            if (MultiObjectTrackingAnalyzer.VideoSequenceMap.ContainsKey(videoSequenceNo))
            {
                return MultiObjectTrackingAnalyzer.VideoSequenceMap[videoSequenceNo];
            }
            return videoSequenceNo;
        }

        public static void SaveKITTIResultVideosLocally(string jobGUID, string directoryName)
        {
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            SaveKITTIResultVideosLocally(entries, directoryName);
            resultsDB.close();
        }

        public static void SaveKITTIAggregatedResultVideosLocally(string jobGUID, string directoryName)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            resultsDB.close();
            SaveKITTIAggregatedResultVideosLocally(entries, directoryName);
            //GroupEntriesByVideoNameAndStitchAndSaveAggregatedResultVideosLocally(entries, directoryName);
        }

        public static void SaveKITTIResultVideosLocally(List<SatyamResultsTableEntry> entries, string directoryName, int fps = 10)
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

                
                //VATIC_DVA_CrowdsourcedResult taskr = VATIC_DVA_CrowdsourcedResult.createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(satyamResult.TaskResult, task.SatyamURI, task.SatyamJobSubmissionsTableEntryID.ToString(), job.FrameRate);

                //MultiObjectTrackingResult res = taskr.getCompressedTracksInTimeSegment();

                string[] names = task.SatyamURI.Split('/');
                string videoName = names[names.Length - 2] + "_" + entry.ID;
                string[] fields = videoName.Split('_');
                string videoSequence = fields[fields.Length - 5];
                int startingFrame = Convert.ToInt32(fields[fields.Length - 2]);
                int maxChunkEndFrame = startingFrame + job.ChunkDuration * job.FrameRate;
                int noFrameOverlap = (int)(job.ChunkOverlap * job.FrameRate);
                if (startingFrame != 0)
                {
                    startingFrame -= noFrameOverlap;
                }

                if (entry.JobGUID == "1e43a983-548d-4a2e-8161-5537eb985902")
                {
                    videoSequence = getCorrectSequenceNo(videoSequence);
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
                MultiObjectTrackingResult res = taskr.getCompressedTracksInTimeSegment();

                generateVideoForEvaluation(ImageURLs, res, directoryName, videoName, job.FrameRate);
            }
        }

        public static void SaveKITTIAggregatedResultVideosLocally(List<SatyamAggregatedResultsTableEntry> entries, string directoryName)
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
                MultiObjectTrackingAggregatedResult aggresult = JSonUtils.ConvertJSonToObject<MultiObjectTrackingAggregatedResult>(satyamAggResult.AggregatedResultString);
                string[] names = aggTask.SatyamURI.Split('/');
                string videoName = names[names.Length - 2] + "_" + entry.ID;
                string[] fields = videoName.Split('_');
                string videoSequence = fields[fields.Length - 5];
                int startingFrame = Convert.ToInt32(fields[fields.Length - 2]);
                int maxChunkEndFrame = startingFrame + job.ChunkDuration * job.FrameRate;
                int noFrameOverlap = (int)(job.ChunkOverlap * job.FrameRate);
                if (startingFrame != 0)
                {
                    startingFrame -= noFrameOverlap;
                }

                if (entry.JobGUID == "1e43a983-548d-4a2e-8161-5537eb985902")
                {
                    videoSequence = getCorrectSequenceNo(videoSequence);
                }

                string videoFrameDir = DirectoryConstants.KITTITrackingImages + videoSequence;
                List<string> files = Directory.GetFiles(videoFrameDir).ToList();
                List<string> ImageURLs = new List<string>();
                for (int j = startingFrame; j < files.Count && j < maxChunkEndFrame; j++)
                {
                    ImageURLs.Add(files[j]);
                }

                generateVideoForEvaluation(ImageURLs, aggresult.tracklets, directoryName, videoName, job.FrameRate);
            }
        }



        public static void SaveResultVideosLocally(string jobGUID, string directoryName)
        {
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            SaveResultVideosLocally(entries, directoryName);
            resultsDB.close();
        }

        public static void SaveAggregatedResultVideosLocally(string jobGUID, string directoryName)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            resultsDB.close();
            //SaveAggregatedResultVideosLocally(entries, directoryName);
            GroupEntriesByVideoNameAndStitchAndSaveAggregatedResultVideosLocally(entries, directoryName);
        }

        public static void SaveResultVideosLocally(List<SatyamResultsTableEntry> entries, string directoryName, int fps =10)
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

                //VATIC_DVA_CrowdsourcedResult taskr = new VATIC_DVA_CrowdsourcedResult(satyamResult.TaskResult, task.SatyamURI, start, end, task.SatyamJobSubmissionsTableEntryID.ToString());
                VATIC_DVA_CrowdsourcedResult taskr = MultiObjectTrackingAggregator.createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(satyamResult.TaskResult, task.SatyamURI, entry.ID.ToString(), job.FrameRate);

                MultiObjectTrackingResult res = taskr.getCompressedTracksInTimeSegment();

                List<string> ImageURLs = satyamStorage.getURLListOfSubDirectoryByURL(task.SatyamURI);
                string[] names = task.SatyamURI.Split('/');
                string videoName = names[names.Length - 2] + "_" + entry.ID;
                generateVideoForEvaluation(ImageURLs, res, directoryName, videoName, job.FrameRate);
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
                MultiObjectTrackingAggregatedResult aggresult = JSonUtils.ConvertJSonToObject<MultiObjectTrackingAggregatedResult>(satyamAggResult.AggregatedResultString);

                List<string> ImageURLs = satyamStorage.getURLListOfSubDirectoryByURL(aggTask.SatyamURI);

                
                string videoName = URIUtilities.localDirectoryNameFromURI(aggTask.SatyamURI) + "_" + entry.ID;

                generateVideoForEvaluation(ImageURLs, aggresult.tracklets, directoryName, videoName, job.FrameRate);
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


            SortedDictionary<string, List<SatyamAggregatedResultsTableEntry>> entriesPerVideo = GroupChunksByVideoName(entries);

            foreach (string videoname in entriesPerVideo.Keys)
            {
                List<string> ImageURLs;
                int fps;
                MultiObjectTrackingResult stitched = stitchAllChunksOfOneVideo(entriesPerVideo[videoname], out ImageURLs, out fps);
                if (stitched != null)
                {
                    //generateVideoForEvaluation(ImageURLs, stitched, directoryName, videoName, fps);
                    generateVideoFramesForEvaluation(ImageURLs, stitched, directoryName, videoname, fps);
                }
                
            }
        }


        public static SortedDictionary<string, List<SatyamAggregatedResultsTableEntry>> GroupChunksByVideoName(List<SatyamAggregatedResultsTableEntry> entries)
        {
            SortedDictionary<string, List<SatyamAggregatedResultsTableEntry>> entriesPerVideo = new SortedDictionary<string, List<SatyamAggregatedResultsTableEntry>>();
            int fps = 0;
            for (int i = 0; i < entries.Count; i++)
            {
                SatyamAggregatedResultsTableEntry entry = entries[i];
                SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
                SatyamTask aggTask = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamAggResult.TaskParameters);
                string video = URIUtilities.localDirectoryNameFromURI(aggTask.SatyamURI);
                string[] fields = video.Split('_');
                string videoName = "";
                for (int j = 0; j < fields.Length - 2; j++)
                {
                    // remove the starting frame

                    videoName += fields[j];
                    if (j == fields.Length - 3) break;
                    videoName += '_';
                    MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(aggTask.jobEntry.JobParameters);
                    fps = job.FrameRate;
                }

                if (!entriesPerVideo.ContainsKey(videoName))
                {
                    entriesPerVideo.Add(videoName, new List<SatyamAggregatedResultsTableEntry>());
                }
                entriesPerVideo[videoName].Add(entries[i]);
            }
            return entriesPerVideo;
        }

        public static MultiObjectTrackingResult stitchAllChunksOfOneVideo(List<SatyamAggregatedResultsTableEntry> entries, out List<string> ImageURLs, out int fps)
        {
            ImageURLs = new List<string>();
            fps = 0;
            if (entries.Count == 0) return null;

            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            MultiObjectTrackingResult stitched = new MultiObjectTrackingResult();
            
            int totalFrameCounts = 0;

            // ensure the order is correct
            SortedDictionary<int, SatyamAggregatedResultsTableEntry> sortedEntries = new SortedDictionary<int, SatyamAggregatedResultsTableEntry>();
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
                    sortedEntries.Add(startingFrame, entries[i]);
                    idx.Add(startingFrame);
                }
            }


            idx.Sort();

            for (int i = 0; i < idx.Count; i++)
            {
                //SatyamAggregatedResultsTableEntry entry = entries[i];
                SatyamAggregatedResultsTableEntry entry = sortedEntries[idx[i]];
                SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
                SatyamTask aggTask = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamAggResult.TaskParameters);
                MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(aggTask.jobEntry.JobParameters);
                MultiObjectTrackingAggregatedResult aggresult = JSonUtils.ConvertJSonToObject<MultiObjectTrackingAggregatedResult>(satyamAggResult.AggregatedResultString);
                int noFramesOverlap = 0;
                if (job.ChunkOverlap != 0.0)
                {
                    noFramesOverlap = (int)(job.ChunkOverlap * job.FrameRate);
                }

                List<string> TraceURLs = satyamStorage.getURLListOfSubDirectoryByURL(aggTask.SatyamURI);
                if (i == 0)
                {
                    ImageURLs.AddRange(TraceURLs);

                    string[] names = aggTask.SatyamURI.Split('/');
                    fps = job.FrameRate;
                    stitched = aggresult.tracklets;
                    totalFrameCounts += TraceURLs.Count;
                }
                else
                {
                    int noNewFrames = 0;
                    for (int j = noFramesOverlap; j < TraceURLs.Count; j++)
                    {
                        ImageURLs.Add(TraceURLs[j]);
                        noNewFrames++;
                    }

                    //stitched = stitchTwoTracesByOneFrameBoundingBoxes(stitched, aggresult.tracklets, totalFrameCounts, totalFrameCounts+ noNewFrames, noFramesOverlap,fps);
                    stitched = stitchTwoTracesByTubeletsOfOverlappingVideoChunk(stitched, aggresult.tracklets, totalFrameCounts, totalFrameCounts + noNewFrames, noFramesOverlap, fps);

                    totalFrameCounts += noNewFrames;
                }

                //debug
                //generateVideoForEvaluation(ImageURLs, stitched, directoryName + "_" + i, videoName, fps);
            }
            return stitched;
        }

        public static MultiObjectTrackingResult stitchTwoTracesByOneFrameBoundingBoxes(MultiObjectTrackingResult currentTrace, MultiObjectTrackingResult nextTrace, 
            int totalFrameCountsBeforeStitching, int totalFrameCountsAfterStitching, int noFramesOverlap = 0, int fps = 10)
        {
            List<List<BoundingBox>> boundingboxes = new List<List<BoundingBox>>();
            List<List<string>> CrossChunkEntitiesAnnotationStrings = new List<List<string>>();
            List<List<string>> SingleChunkEntitiesAnnotationStrings = new List<List<string>>();
            for (int i = 0; i < 2; i++)
            {
                boundingboxes.Add(new List<BoundingBox>());
                CrossChunkEntitiesAnnotationStrings.Add(new List<string>());
                SingleChunkEntitiesAnnotationStrings.Add(new List<string>());
            }
            double frameTimeInMiliSeconds = (double)1000 / (double)fps;
            double timeToPostponeInMilliSeconds = (double)(totalFrameCountsBeforeStitching-noFramesOverlap) * frameTimeInMiliSeconds;
            //TimeSpan timeSpanToPostponeInSeconds = new TimeSpan(0,0,0,0, (int)timeToPostponeInMilliSeconds);
            TimeSpan timeSpanToPostponeInSeconds = DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)timeToPostponeInMilliSeconds);

            // get the end of the first trace and the begining + overlap of the second trace
            DateTime CurrentTraceSampleFrameTime = currentTrace.VideoSegmentStartTime.AddMilliseconds(frameTimeInMiliSeconds * (totalFrameCountsBeforeStitching - 1));
            DateTime NextTraceSampleFrameTime = nextTrace.VideoSegmentStartTime.AddMilliseconds(frameTimeInMiliSeconds * (Math.Max(noFramesOverlap-1,0)));
            // current trace
            foreach (CompressedTrack entity in currentTrace.tracks)
            {
                
                SpaceTime st = entity.getSpaceTimeAt(CurrentTraceSampleFrameTime);
                BooleanAttribute outofview_attr = entity.getAttributeAt("outofview", CurrentTraceSampleFrameTime);
                if (st != null && outofview_attr != null && !outofview_attr.value)
                {

                    BoundingBox box = st.region;
                    boundingboxes[0].Add(box);
                    CrossChunkEntitiesAnnotationStrings[0].Add(entity.unCompressToTrackAnnotationString());
                }
                else
                {
                    SingleChunkEntitiesAnnotationStrings[0].Add(entity.unCompressToTrackAnnotationString());
                }
                
            }

            // next trace
            nextTrace.VideoSegmentStartTime+= timeSpanToPostponeInSeconds;
            nextTrace.VideoSegmentEndTime += timeSpanToPostponeInSeconds;

            foreach (CompressedTrack entity in nextTrace.tracks)
            {
                // get the frame first before changing the time stmaps.
                SpaceTime st = entity.getSpaceTimeAt(NextTraceSampleFrameTime);
                BooleanAttribute outofview_attr = entity.getAttributeAt("outofview", entity.endTime);
                

                /// postpone the timestamp of the second trace
                entity.startTime += timeSpanToPostponeInSeconds;
                entity.endTime += timeSpanToPostponeInSeconds;
                entity.spaceTimeTrack.startTime+= timeSpanToPostponeInSeconds;
                entity.spaceTimeTrack.endTime+= timeSpanToPostponeInSeconds;

                //foreach (SpaceTime st in entity.spaceTimeTrack.space_time_track)
                for (int i=0;i< entity.spaceTimeTrack.space_time_track.Count;i++)
                {
                    entity.spaceTimeTrack.space_time_track[i].time+= timeSpanToPostponeInSeconds;
                }
                //foreach (CompressedBooleanAttributeTrack booleanAttributeTrack in entity.booleanAttributeTracks.Values)
                foreach ( string key in entity.booleanAttributeTracks.Keys)
                {
                    entity.booleanAttributeTracks[key].startTime+= timeSpanToPostponeInSeconds;
                    entity.booleanAttributeTracks[key].endTime+= timeSpanToPostponeInSeconds;

                    //foreach (BooleanAttribute ba in entity.booleanAttributeTracks[key].attribute_track)
                    for (int i=0;i< entity.booleanAttributeTracks[key].attribute_track.Count;i++)
                    {
                        entity.booleanAttributeTracks[key].attribute_track[i].time+= timeSpanToPostponeInSeconds;
                    }
                }

                if (st != null && outofview_attr != null && !outofview_attr.value)
                {

                    BoundingBox box = st.region;
                    boundingboxes[1].Add(box);
                    CrossChunkEntitiesAnnotationStrings[1].Add(entity.unCompressToTrackAnnotationString());
                }
                else
                {
                    SingleChunkEntitiesAnnotationStrings[1].Add(entity.unCompressToTrackAnnotationString());
                }
                
            }
            List<MultipartiteWeightedMatch> association = BoundingBoxAssociation.computeBoundingBoxAssociations(boundingboxes);

            DateTime NewFrameStartTime = currentTrace.VideoSegmentStartTime + DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)(totalFrameCountsBeforeStitching * frameTimeInMiliSeconds));

            string totalStitchedAnnotationString = stitchAnnotationStringByAssociation(CrossChunkEntitiesAnnotationStrings, SingleChunkEntitiesAnnotationStrings,
                association, NewFrameStartTime);

            MultiObjectTrackingResult ret = MultiObjectTrackingResult.ConvertAnnotationStringToMultiObjectTrackingResult(currentTrace.VideoSegmentStartTime, totalStitchedAnnotationString, currentTrace.cameraId, currentTrace.UID, totalFrameCountsAfterStitching,fps);
            return ret;
        }


        public static MultiObjectTrackingResult stitchTwoTracesByTubeletsOfOverlappingVideoChunk(MultiObjectTrackingResult currentTrace, MultiObjectTrackingResult nextTrace,
            int totalFrameCountsBeforeStitching, int totalFrameCountsAfterStitching, int noFramesOverlap,int fps = 10)
        {
            //if (noFramesOverlap == 0) return null;
            double frameTimeInMiliSeconds = (double)1000 / (double)fps;

            List<MultiObjectTrackingResult> compressedOverlappingTracks = new List<MultiObjectTrackingResult>();

            DateTime overlapStart = currentTrace.VideoSegmentStartTime.AddMilliseconds(frameTimeInMiliSeconds * (totalFrameCountsBeforeStitching - noFramesOverlap));
            DateTime overlapEnd = currentTrace.VideoSegmentStartTime.AddMilliseconds(frameTimeInMiliSeconds * (totalFrameCountsBeforeStitching - 1));
            //DateTime nextTrace_overlapStart = nextTrace.VideoSegmentStartTime;
            //DateTime nextTrace_overlapEnd = nextTrace.VideoSegmentStartTime.AddMilliseconds(frameTimeInMiliSeconds * (noFramesOverlap-1));
            //MultiObjectTrackingResult nextTrace_Overlap = nextTrace.getSubTimeSegment(nextTrace_overlapStart, nextTrace_overlapEnd, fps);
            //compressedOverlappingTracks.Add(nextTrace_Overlap);

            List<List<string>> CrossChunkEntitiesAnnotationStrings = new List<List<string>>();
            List<List<string>> SingleChunkEntitiesAnnotationStrings = new List<List<string>>();
            for (int i = 0; i < 2; i++)
            {
                //boundingboxes.Add(new List<BoundingBox>());
                CrossChunkEntitiesAnnotationStrings.Add(new List<string>());
                SingleChunkEntitiesAnnotationStrings.Add(new List<string>());
            }



            double timeToPostponeInMilliSeconds = (double)(totalFrameCountsBeforeStitching - noFramesOverlap) * frameTimeInMiliSeconds;
            //TimeSpan timeSpanToPostponeInSeconds = new TimeSpan(0,0,0,0, (int)timeToPostponeInMilliSeconds);
            TimeSpan timeSpanToPostponeInSeconds = DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)timeToPostponeInMilliSeconds);

            //// get the end of the first trace and the begining + overlap of the second trace
            //DateTime CurrentTraceSampleFrameTime = currentTrace.VideoSegmentStartTime.AddMilliseconds(frameTimeInMiliSeconds * (totalFrameCountsBeforeStitching - 1));
            //DateTime NextTraceSampleFrameTime = nextTrace.VideoSegmentStartTime.AddMilliseconds(frameTimeInMiliSeconds * (Math.Max(noFramesOverlap - 1, 0)));

            // current trace
            List<string> currentTrace_filteredAnnotationStrings = new List<string>();
            foreach (CompressedTrack entity in currentTrace.tracks)
            {
                // get a snapshot frame at start and end of current trace
                VATIC_DVA_Frame currentTraceOverlapStartFrame = entity.getFrameAt(overlapStart);

                string annotationString = entity.unCompressToTrackAnnotationString();
                string[] fields = annotationString.Split(':');
                List<string> TimeFilteredSegmentStringList = new List<string>();
                TimeFilteredSegmentStringList.Add(fields[0]); // adds the label back
                if (currentTraceOverlapStartFrame != null)
                {
                    TimeFilteredSegmentStringList.Add(currentTraceOverlapStartFrame.ToAnnotationString()); // adds a starting frame
                }
                
                // remove the label of the second string
                int count = 0;
                for (int j = 1; j < fields.Length; j++)
                {
                    // remove the overlapping part.
                    DateTime time = DateTimeUtilities.getDateTimeFromString(fields[j].Split(',')[0]);
                    if (time <= overlapStart || time > overlapEnd) continue;

                    TimeFilteredSegmentStringList.Add(fields[j]);
                    count++;
                }
                if (count == 0)
                {
                    // not a cross chunk one
                    SingleChunkEntitiesAnnotationStrings[0].Add(annotationString);
                    continue;
                }
                else
                {
                    //a cross chunk one
                    CrossChunkEntitiesAnnotationStrings[0].Add(annotationString);
                    // construct for association
                    string filteredAnnotationString = ObjectsToStrings.ListString(TimeFilteredSegmentStringList, ':');
                    currentTrace_filteredAnnotationStrings.Add(filteredAnnotationString);
                }                
            }

            string currentOverlap_totalFilteredAnnotationString = ObjectsToStrings.ListString(currentTrace_filteredAnnotationStrings, '|');

            MultiObjectTrackingResult currentTrace_Overlap = MultiObjectTrackingResult.ConvertAnnotationStringToMultiObjectTrackingResult(overlapStart, currentOverlap_totalFilteredAnnotationString, "current", "0", noFramesOverlap, fps);
            compressedOverlappingTracks.Add(currentTrace_Overlap);

            // next trace
            /// postpone the timestamp of the second trace
            nextTrace.postpone(timeSpanToPostponeInSeconds);
            List<string> nextTrace_filteredAnnotationStrings = new List<string>();
            foreach (CompressedTrack entity in nextTrace.tracks)
            {
                VATIC_DVA_Frame nextTraceOverlapStartFrame = entity.getFrameAt(overlapStart);
                VATIC_DVA_Frame nextTraceOverlapEndFrame = entity.getFrameAt(overlapEnd);

                string annotationString = entity.unCompressToTrackAnnotationString();
                string[] fields = annotationString.Split(':');
                List<string> TimeFilteredSegmentStringList = new List<string>();
                TimeFilteredSegmentStringList.Add(fields[0]); // adds the label back

                if (nextTraceOverlapStartFrame != null)
                {
                    TimeFilteredSegmentStringList.Add(nextTraceOverlapStartFrame.ToAnnotationString()); // adds a starting frame
                }
                // remove the label of the second string
                int count = 0;
                for (int j = 1; j < fields.Length; j++)
                {
                    // remove the overlapping part.
                    DateTime time = DateTimeUtilities.getDateTimeFromString(fields[j].Split(',')[0]);
                    if (time <= overlapStart || time >= overlapEnd) continue;

                    TimeFilteredSegmentStringList.Add(fields[j]);
                    count++;
                }
                if (count == 0)
                {
                    // not a cross chunk one
                    SingleChunkEntitiesAnnotationStrings[1].Add(annotationString);
                    continue;
                }
                else
                {
                    //a cross chunk one
                    CrossChunkEntitiesAnnotationStrings[1].Add(annotationString);
                    // construct for association
                    if (nextTraceOverlapEndFrame != null)
                    {
                        TimeFilteredSegmentStringList.Add(nextTraceOverlapEndFrame.ToAnnotationString()); // adds a ending frame
                    }
                    string filteredAnnotationString = ObjectsToStrings.ListString(TimeFilteredSegmentStringList, ':');
                    nextTrace_filteredAnnotationStrings.Add(filteredAnnotationString);
                }
            }
            string nextOverlap_totalFilteredAnnotationString = ObjectsToStrings.ListString(nextTrace_filteredAnnotationStrings, '|');

            MultiObjectTrackingResult nextTrace_Overlap = MultiObjectTrackingResult.ConvertAnnotationStringToMultiObjectTrackingResult(overlapStart, nextOverlap_totalFilteredAnnotationString, "next", "1", noFramesOverlap, fps);
            compressedOverlappingTracks.Add(nextTrace_Overlap);

            List<MultipartiteWeightedMatch> association = TrackletsAssociation.AssociateTracklets(compressedOverlappingTracks);

            
            DateTime NewFrameStartTime = currentTrace.VideoSegmentStartTime + DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)(totalFrameCountsBeforeStitching * frameTimeInMiliSeconds));
            string totalStitchedAnnotationString = stitchAnnotationStringByAssociation(CrossChunkEntitiesAnnotationStrings, SingleChunkEntitiesAnnotationStrings,
                association, NewFrameStartTime);

            MultiObjectTrackingResult ret = MultiObjectTrackingResult.ConvertAnnotationStringToMultiObjectTrackingResult(currentTrace.VideoSegmentStartTime, totalStitchedAnnotationString, currentTrace.cameraId, currentTrace.UID, totalFrameCountsAfterStitching, fps);
            return ret;
        }

        public static string stitchAnnotationStringByAssociation(List<List<string>> CrossChunkEntitiesAnnotationStrings, List<List<string>> SingleChunkEntitiesAnnotationStrings,
            List<MultipartiteWeightedMatch> association, DateTime NewFrameStartTime)
        {
            List<string> stitchedAnnotationStrings = new List<string>();
            // traverse the current trace
            for (int i = 0; i < CrossChunkEntitiesAnnotationStrings[0].Count; i++)
            {
                string stitchedAnnotationString = "";
                foreach (MultipartiteWeightedMatch match in association)
                {
                    if (match.elementList.Count == 2 && match.elementList[0] == i)
                    {
                        // there is an association from the next trace

                        string[] currentTraceFields = CrossChunkEntitiesAnnotationStrings[0][match.elementList[0]].Split(':');
                        string[] nextTraceFields = CrossChunkEntitiesAnnotationStrings[1][match.elementList[1]].Split(':');
                        //List<string> LabelRemovedStringList = new List<string>();
                        List<string> MergedStringList= new List<string>();

                        // majority label, if same segment use second one.
                        string curLabel = currentTraceFields[0].Split('_')[0];
                        string nextLabel = nextTraceFields[0].Split('_')[0];
                        //if (curLabel != nextLabel)
                        //{
                        //    MergedStringList.Add(nextTraceFields[0]);
                        //}
                        //else
                        //{
                        //    MergedStringList.Add(currentTraceFields[0]);
                        //}
                        MergedStringList.Add(currentTraceFields[0]);
                        int a = 1;
                        int b = 1;
                        while (true)
                        {
                            if (a == currentTraceFields.Length && b == nextTraceFields.Length) break;
                            if (a == currentTraceFields.Length)
                            {
                                MergedStringList.Add(nextTraceFields[b]); b++;continue;
                            }
                            if (b == nextTraceFields.Length)
                            {
                                MergedStringList.Add(currentTraceFields[a]); a++;continue;
                            }

                            DateTime curTime = DateTimeUtilities.getDateTimeFromString(currentTraceFields[a].Split(',')[0]);
                            DateTime nextTime = DateTimeUtilities.getDateTimeFromString(nextTraceFields[b].Split(',')[0]);
                            if (curTime<=nextTime)
                            {
                                MergedStringList.Add(currentTraceFields[a]);
                                a++;
                                if (curTime == nextTime) b++; // skip the same frame from next trace
                            }
                            else
                            {
                                MergedStringList.Add(nextTraceFields[b]);
                                b++;
                            }
                        }

                        stitchedAnnotationString = ObjectsToStrings.ListString(MergedStringList, ':');


                        //// remove the label of the second string
                        //for (int j = 1; j < nextTraceFields.Length; j++)
                        //{
                        //    // remove the overlapping part.
                        //    DateTime time = DateTimeUtilities.getDateTimeFromString(nextTraceFields[j].Split(',')[0]);
                        //    if (time < NewFrameStartTime) continue;

                        //    LabelRemovedStringList.Add(nextTraceFields[j]);
                        //}
                        //if (LabelRemovedStringList.Count > 0)
                        //{
                        //    string LabelRemovedString = ObjectsToStrings.ListString(LabelRemovedStringList, ':');
                        //    stitchedAnnotationString = CrossChunkEntitiesAnnotationStrings[0][match.elementList[0]] + ":" + LabelRemovedString;
                        //}
                        //else
                        //{
                        //    stitchedAnnotationString = CrossChunkEntitiesAnnotationStrings[0][match.elementList[0]];
                        //}
                        
                    }
                }
                if (stitchedAnnotationString == "")
                {
                    // there is no match from next 
                    stitchedAnnotationString = CrossChunkEntitiesAnnotationStrings[0][i];
                }
                stitchedAnnotationStrings.Add(stitchedAnnotationString);
            }

            // traverse the next trace
            for (int i = 0; i < CrossChunkEntitiesAnnotationStrings[1].Count; i++)
            {
                bool added = false;
                foreach (MultipartiteWeightedMatch match in association)
                {
                    if (match.elementList.Count == 2 && match.elementList[1] == i)
                    {
                        //this track has been associated and added.
                        added = true;
                    }
                }
                if (!added)
                {
                    stitchedAnnotationStrings.Add(CrossChunkEntitiesAnnotationStrings[1][i]);
                }
            }
            //add the irrelavant single chunk entities from both chunk
            for (int i = 0; i < 2; i++)
            {
                foreach (string anno in SingleChunkEntitiesAnnotationStrings[i])
                {
                    stitchedAnnotationStrings.Add(anno);
                }
            }
            string totalStitchedAnnotationString = ObjectsToStrings.ListString(stitchedAnnotationStrings, '|');
            return totalStitchedAnnotationString;
        }

        // if the stitched video is too long
        public static void generateVideoFramesForEvaluation(List<string> ImageURLs, MultiObjectTrackingResult ctts, String directory, String videoName, int fps)
        {
            if (Directory.Exists(directory + "\\" + videoName))
            {
                return;
            }

            Directory.CreateDirectory(directory + "\\" + videoName);
            Console.WriteLine("Saving " + directory + "\\" + videoName);

            //List<Image> imageList = new List<Image>();
            var wc = new WebClient();
            

            //List<DateTime> frameTimes = new List<DateTime>();
            double frameTimeSpanInMiliseconds = (double)1000 / (double)fps;
            //double frameTimeSpanInMiliseconds = (double)(ChunkDuration) / (double)(ImageURLs.Count) * 1000;
            DateTime start = ctts.VideoSegmentStartTime;
            for (int i = 0; i < ImageURLs.Count; i++)
            {
                DateTime t;
                t = start.AddMilliseconds(frameTimeSpanInMiliseconds * i);
                //frameTimes.Add(t);

                Image x = Image.FromStream(wc.OpenRead(ImageURLs[i]));
                //imageList.Add(x);

                List<BoundingBox> locations = new List<BoundingBox>();
                List<string> labels = new List<string>();
                Dictionary<string, List<bool>> attributes = new Dictionary<string, List<bool>>();
                attributes.Add("occlusion", new List<bool>());

                List<int> idx = new List<int>();

                if (ctts.tracks.Count != 0)
                {
                    foreach (string key in ctts.tracks[0].booleanAttributeTracks.Keys)
                    {
                        if (!attributes.ContainsKey(key))
                        {
                            attributes.Add(key, new List<bool>());
                        }

                    }
                }

                for (int j = 0; j < ctts.tracks.Count; j++)
                {

                    CompressedTrack ct = ctts.tracks[j];
                    SpaceTime st = ct.getSpaceTimeAt(t);

                    BooleanAttribute outofview_attr = ct.getAttributeAt("outofview", t);
                    if (st != null && outofview_attr != null && !outofview_attr.value)
                    {
                        BoundingBox l = st.region;
                        locations.Add(l);
                        labels.Add(ctts.tracks[j].label);
                        //attributes["occlusion"].Add(ct.getAttributeAt("occlusion", t).value);
                        foreach (string key in attributes.Keys)
                        {
                            attributes[key].Add(ct.getAttributeAt(key, t).value);
                        }
                        idx.Add(j);
                    }
                }
                Image new_image = generateTrackImage(x, labels, locations, attributes, idx);
                new_image.Save(directory + "\\" + videoName + "\\img" + i.ToString("000") + ".jpg");
            }

            FFMpegWrappers.generateVideoFromFolderofFrames(videoName, directory + "\\" + videoName + "\\");
            Console.WriteLine("done");
        }

        public static void generateVideoForEvaluation(List<string> ImageURLs, MultiObjectTrackingResult ctts, String directory, String videoName, int fps)
        {
            if (Directory.Exists(directory + "\\" + videoName))
            {
                return;
            }

            if (File.Exists(directory + "\\" + videoName + ".mp4"))
            {
                return;
            }

            List<Image> imageList = new List<Image>();
            var wc = new WebClient();
            foreach (string uri in ImageURLs)
            {
                Image x = Image.FromStream(wc.OpenRead(uri));
                imageList.Add(x);
            }

            List<DateTime> frameTimes = new List<DateTime>();
            double frameTimeSpanInMiliseconds = (double)1000 / (double)fps;
            //double frameTimeSpanInMiliseconds = (double)(ChunkDuration) / (double)(ImageURLs.Count) * 1000;
            DateTime start = ctts.VideoSegmentStartTime;
            for (int i = 0; i < ImageURLs.Count; i++)
            {
                DateTime t;
                t = start.AddMilliseconds(frameTimeSpanInMiliseconds * i);
                frameTimes.Add(t);
            }
            generateVideoForEvaluation(imageList, frameTimes, ctts, videoName, directory);
        }

        public static void generateVideoForEvaluation(List<Image> Images, List<DateTime> dateTimeList, MultiObjectTrackingResult ctts, String videoName, String directory)
        {
            List<Image> imagesWithTracks = new List<Image>();

            for (int i = 0; i < Images.Count; i++)
            {
                List<BoundingBox> locations = new List<BoundingBox>();
                List<string> labels = new List<string>();
                Dictionary<string, List<bool>> attributes = new Dictionary<string, List<bool>>();
                attributes.Add("occlusion", new List<bool>());

                if (ctts.tracks.Count != 0)
                {
                    foreach (string key in ctts.tracks[0].booleanAttributeTracks.Keys)
                    {
                        if (!attributes.ContainsKey(key))
                        {
                            attributes.Add(key, new List<bool>());
                        }
                        
                    }
                }                

                List<int> idx = new List<int>();

                for (int j = 0; j < ctts.tracks.Count; j++)
                {

                    CompressedTrack ct = ctts.tracks[j];
                    SpaceTime st = ct.getSpaceTimeAt(dateTimeList[i]);
                    
                    BooleanAttribute outofview_attr = ct.getAttributeAt("outofview", dateTimeList[i]);
                    if (st != null && outofview_attr != null && !outofview_attr.value)
                    {
                        BoundingBox l = st.region;
                        locations.Add(l);
                        labels.Add(ctts.tracks[j].label);
                        foreach(string key in attributes.Keys)
                        {
                            attributes[key].Add(ct.getAttributeAt(key, dateTimeList[i]).value);
                        }
                        
                        idx.Add(j);
                    }
                }
                Image new_image = generateTrackImage(Images[i], labels, locations, attributes, idx);
                imagesWithTracks.Add(new_image);
            }

            Console.WriteLine("Saving " + directory + "\\" + videoName);
            FFMpegWrappers.generateVideoFromFrames(imagesWithTracks, videoName, directory);

            //Directory.CreateDirectory(directory + "\\" + videoName);
            //Console.WriteLine("Saving " + directory + "\\" + videoName);
            //for (int i = 0; i < imagesWithTracks.Count; i++)
            //{

            //    imagesWithTracks[i].Save(directory + "\\" + videoName + "\\img" + i.ToString("000") + ".jpg");
            //}
            //Console.WriteLine("done");
        }

        public static Image generateTrackImage(Image image, List<string> Labels, List<BoundingBox> locations, Dictionary<string, List<bool>> attributes, List<int> idx)
        {
            int thickness = 5;
            float[] dashValues = { 2, 2, 2, 2 };
            Image outputImage = (Image)image.Clone();

            using (Graphics g = Graphics.FromImage(outputImage))
            {
                for (int i = 0; i < locations.Count; i++)
                {
                    if (locations[i] == null)
                    {
                        continue;
                    }
                    Color c = ColorSet.getColorByObjectType(Labels[i]);
                    Pen pen = new Pen(c, thickness);
                    if (attributes["occlusion"][i] == true)
                    {
                        pen.DashPattern = dashValues;
                    }
                    int x = locations[i].tlx;
                    int y = locations[i].tly;
                    int width = locations[i].brx - x;
                    int height = locations[i].bry - y;
                    Rectangle rect = new Rectangle(x, y, width, height);
                    g.DrawRectangle(pen, rect);

                    // draw the idx number for debugging
                    PointF loc = new PointF(x, y);
                    string idxString = idx[i].ToString();
                    foreach (string key in attributes.Keys)
                    {
                        if (key == "occlusion" || key == "outofview") continue;
                        if (attributes[key][i])
                        {
                            idxString += "\n" + key;
                        }
                        
                    }
                    g.DrawString(idxString, new Font("Arial", 14), Brushes.Yellow, loc);
                }
            }
            return outputImage;
        }
    }
}
