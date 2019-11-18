using AzureBlobStorage;
using Constants;
using HelperClasses;
using JobTemplateClasses;
using SatyamResultClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Linq;
using UsefulAlgorithms;
using Utilities;

namespace SatyamResultAggregators
{
    //public class MultiObjectTrackingAggregatedResult
    //{
    //    public List<CompressedTrack> tracklets;
    //    public MultiObjectTrackingAggregatedResult()
    //    {
    //        tracklets = new List<CompressedTrack>();
    //    }
    //    public override string ToString()
    //    {
    //        string trackAnnotationString = "";
    //        for (int i = 0; i < tracklets.Count; i++)
    //        {
    //            trackAnnotationString += tracklets[i].unCompressToTrackAnnotationString();
    //            if (i == tracklets.Count - 1) break;
    //            trackAnnotationString += "|";
    //        }
    //        return trackAnnotationString;
    //    }
    //}

    public class MultiObjectTrackingAggregatedResultMetaData
    {
        public int TotalCount;
    }

    public class MultiObjectTrackingAggregatedResult
    {
        public MultiObjectTrackingResult tracklets=new MultiObjectTrackingResult();
        //public MultiObjectTrackingAggregatedResultMetaData metaData;
    }


    public class MultiObjectTrackingAggregator
    {
        public static Dictionary<string, int> ImageCountCache = new Dictionary<string, int>();

        public static VATIC_DVA_CrowdsourcedResult createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(string TrackAnnotationString, string DirectoryURL, string l_UID, int fps = 10)
        {
            //if (!ImageCountCache.ContainsKey(DirectoryURL))
            //{
                SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
                List<string> ImageURLs = satyamStorage.getURLListOfSubDirectoryByURL(DirectoryURL);
            int totalFrameCount = ImageURLs.Count;
            //    ImageCountCache.Add(DirectoryURL, ImageURLs.Count);
            //}

            //int totalFrameCount = ImageCountCache[DirectoryURL];
            string videoName = URIUtilities.filenameFromURI(DirectoryURL);
            return new VATIC_DVA_CrowdsourcedResult(TrackAnnotationString, videoName, l_UID, totalFrameCount, fps);
        }

        public static string GetAggregatedResultString(List<SatyamResultsTableEntry> results)
        {
            
            if (results.Count == 0) return null;

            string resultString = null;
            SatyamResult res0 = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(res0.TaskParametersString);
            MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(task.jobEntry.JobParameters);

            List<MultiObjectTrackingResult> resultList = new List<MultiObjectTrackingResult>();

            bool masters = false;
            List<string> WorkersPerTask = new List<string>();
            foreach (SatyamResultsTableEntry entry in results)
            {    
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);

                // remove duplicate workers result
                string workerID = res.amazonInfo.WorkerID;
                if (workerID!="" && WorkersPerTask.Contains(workerID))
                {
                    continue;
                }
                //enclose only non-duplicate results, one per each worker.
                WorkersPerTask.Add(workerID);

                //if (res.TaskResult == "") continue;
                //VATIC_DVA_CrowdsourcedResult taskr = JSonUtils.ConvertJSonToObject<VATIC_DVA_CrowdsourcedResult>(res.TaskResult);
                VATIC_DVA_CrowdsourcedResult taskr = createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(res.TaskResult, task.SatyamURI, entry.ID.ToString(), job.FrameRate);
                resultList.Add(taskr.getCompressedTracksInTimeSegment());
                 
                // temproray hack for masters
                if (TaskConstants.masterGUIDs.Contains(entry.JobGUID))
                {
                    masters = true;
                }
            }
            int MinResults = TaskConstants.MULTI_OBJ_TRACKING_MTURK_MIN_RESULTS_TO_AGGREGATE;
            if (masters)
            {
                MinResults = 1;
            }
                
            MultiObjectTrackingAggregatedResult r = getAggregatedResult(resultList, job.FrameRate, job.BoundaryLines, MinResults);
                
            if (r != null)
            {
                string rString = JSonUtils.ConvertObjectToJSon(r);
                SatyamAggregatedResult aggResult = new SatyamAggregatedResult();
                aggResult.SatyamTaskTableEntryID = results[0].SatyamTaskTableEntryID;
                aggResult.AggregatedResultString = rString;
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
                aggResult.TaskParameters = res.TaskParametersString;
                resultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(aggResult);
            }
            
            
            return resultString;
        }

        public static MultiObjectTrackingAggregatedResult getAggregatedResult(List<MultiObjectTrackingResult> compressedResults,
            int fps,
            List<LineSegment> boundaryLines,
            int MinResults = TaskConstants.MULTI_OBJ_TRACKING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            double boxToleranceThreshold = TaskConstants.MULTI_OBJ_TRACKING_BOX_DEVIATION_THRESHOLD, 
            double ObjectCoverageApprovalThresholdPerVideo = TaskConstants.MULTI_OBJ_TRACKING_APPROVALRATIO_PER_VIDEO, 
            double BoxCoverageApprovalThresholdPerTrack = TaskConstants.MULTI_OBJ_TRACKING_APPROVALRATIO_PER_TRACK, 
            //int approvalDifferenceLimit = TaskConstants.MULTI_OBJ_TRACKING_MAX_APPROVAL_DIFFERENCE, 
            int consensusNumber = TaskConstants.MULTI_OBJ_TRACKING_MIN_RESULTS_FOR_CONSENSUS,
            double minTubeletIoUSimilarityThreshold = TaskConstants.MULTI_OBJ_TRACKING_MIN_TUBELET_SIMILARITY_THRESHOLD,
            int MaxResults = TaskConstants.MULTI_OBJ_TRACKING_MTURK_MAX_RESULTS_TO_AGGREGATE)
        {

            if (compressedResults.Count < MinResults) //need at least three results!
            {
                return null;
            }

            //statistics of each turker result: CompressedTracksGroupInATimeSegment
            MultiObjectTrackingAggregatedResult aggResult = new MultiObjectTrackingAggregatedResult();
            //List<CompressedTrack> doneTracks = new List<CompressedTrack>();

            List<int> totalTracksPerResults = new List<int>();
            List<int> approvedCounts = new List<int>();
            List<int> rejectedCounts = new List<int>();
            List<int> inconclusiveCounts = new List<int>();

            List<string> approveTracks = new List<string>();
            List<string> rejectedTracks = new List<string>();
            List<string> inconclusiveTracks = new List<string>();

            List<List<CompressedTrack>> missedTracks = new List<List<CompressedTrack>>();
            List<List<CompressedTrack>> rejectTracks = new List<List<CompressedTrack>>();
            List<List<CompressedTrack>> inconcludedTracks = new List<List<CompressedTrack>>();


            List<MultipartiteWeightedMatch> association = TrackletsAssociation.AssociateTracklets(compressedResults);

            int noAcceptedTracks = 0;
            int noAssociatedTracks = 0;

            SortedDictionary<int, int> noAssociatedTracksPerResult = new SortedDictionary<int, int>();
            for (int i = 0; i < compressedResults.Count; i++)
            {
                noAssociatedTracksPerResult.Add(i, 0);
                approvedCounts.Add(0);
                rejectedCounts.Add(0);
                inconclusiveCounts.Add(0);
                missedTracks.Add(new List<CompressedTrack>());
                rejectTracks.Add(new List<CompressedTrack>());
                inconcludedTracks.Add(new List<CompressedTrack>());
                totalTracksPerResults.Add(compressedResults[i].tracks.Count);
            }

            foreach (MultipartiteWeightedMatch matchingTracks in association)
            {
                if (matchingTracks.elementList.Count > 1)
                {
                    noAssociatedTracks++;
                    foreach (KeyValuePair<int, int> entry in matchingTracks.elementList)
                    {
                        noAssociatedTracksPerResult[entry.Key]++;
                    }
                }
            }

            //count how many people have a high association ratio
            // find the highest winner and biase towards him
            int noTracksWithHighAssociationRatio = 0;
            double highestAssociationRatio = 0;
            int winnerTrackIdx = -1;
            for (int i = 0; i < compressedResults.Count; i++)
            {
                if (totalTracksPerResults[i] == 0) continue;
                double ratio = ((double)noAssociatedTracksPerResult[i]) / (double)totalTracksPerResults[i];
                if (ratio > ObjectCoverageApprovalThresholdPerVideo)
                {
                    noTracksWithHighAssociationRatio++;
                    if (ratio > highestAssociationRatio)
                    {
                        highestAssociationRatio = ratio;
                        winnerTrackIdx = i;
                    }
                }
            }
            if (noTracksWithHighAssociationRatio < MinResults && compressedResults.Count < MaxResults) //at least three people should have most of their boxes highly corroborated by one other person
            {
                return null;
            }

            SortedDictionary<int, List<int>> noHighQualityAssociation = new SortedDictionary<int, List<int>>();

            //for each association, majority vote for boxes for each frame
            foreach (MultipartiteWeightedMatch matchingTracks in association)
            {
                
                Dictionary<int, int> majorityTrackGroup = getMajorityGroupTracks(matchingTracks.elementList, compressedResults, fps, minTubeletIoUSimilarityThreshold);
                //Dictionary<int, int> majorityTrackGroup = matchingTracks.elementList;

                if (majorityTrackGroup.Count< consensusNumber)
                {
                    continue;
                }

                List<CompressedTrack> trackList = new List<CompressedTrack>();
                List<string> identifiers = new List<string>();
                CompressedTrack consensusTrackPerEntity = new CompressedTrack();
                //bool winnerInGroup = false;
                int winnerEntityIdx = 0;
                //bool inconclusive = false;
                foreach (KeyValuePair<int, int> aTrack in majorityTrackGroup)
                {
                    if (aTrack.Key == winnerTrackIdx)
                    {
                        //winnerInGroup = true;
                        winnerEntityIdx = aTrack.Value;
                    }
                    string matchString = aTrack.Key + "_" + aTrack.Value;
                    trackList.Add(compressedResults[aTrack.Key].tracks[aTrack.Value]);
                    identifiers.Add(matchString);
                }
                
                //// bias to winner
                //if (winnerInGroup)
                //{
                //    consensusTrackPerEntity = compressedResults[winnerTrackIdx].tracks[winnerEntityIdx];
                //    inconclusive = false;
                //}
                //else
                //{
                    //consensusTrackPerEntity = getConsensusTrack(trackList, identifiers, boxToleranceThreshold, fps, out concludedBoxCount, out approvedBoxCountsPerTrack, out rejectedBoxCountsPerTrack, out inconclusiveBoxCountsPerTrack, out missedBoxesPerTrack, out rejectBoxesPerTrack);
                // check the quality of the consensus
                //inconclusive = AcceptAndRejectTracks(consensusTrackPerEntity, BoxCoverageApprovalThresholdPerTrack, approvalDifferenceLimit, consensusNumber, approvedBoxCountsPerTrack, rejectedBoxCountsPerTrack, inconclusiveBoxCountsPerTrack, concludedBoxCount, missedBoxesPerTrack, rejectBoxesPerTrack, boundaryLines, out inConclusivePartition, out approvedPartition, out rejectedPartition);
                //}

                consensusTrackPerEntity = getConsensusTrack(trackList, boxToleranceThreshold, fps);
                if (consensusTrackPerEntity == null)
                {
                    //too small overlapping time
                    continue;
                }
                if (consensusTrackPerEntity.spaceTimeTrack.space_time_track.Count == 0)
                {
                    //the whole track is inconclusive   
                    continue;
                }
                //else
                //{
                // majority votes reached a consensus    
                //}



                //if (inconclusive)
                //{
                //    for (int i = 0; i < compressedResults.Count; i++)
                //    {
                //        inconclusiveCounts[i]++;
                //    }
                //}
                //else
                //{
                    // conclusive consensus
                    foreach (int k in majorityTrackGroup.Keys)
                    {
                        if (!noHighQualityAssociation.ContainsKey(k))
                        {
                            noHighQualityAssociation.Add(k, new List<int>());
                        }
                        noHighQualityAssociation[k].Add(majorityTrackGroup[k]);
                    }


                    noAcceptedTracks++;
                    aggResult.tracklets.tracks.Add(consensusTrackPerEntity);

                    //for (int i = 0; i < approvedPartition.Count; i++)
                    //{
                    //    approvedCounts[approvedPartition[i]]++;
                    //}
                    //for (int i = 0; i < rejectedPartition.Count; i++)
                    //{
                    //    rejectedCounts[rejectedPartition[i]]++;
                    //}
                //}
            }

            //count how many people have a high "quality" association ratio
            int noResultsWithHighQualityObjectCoverage = 0;
            for (int i = 0; i < compressedResults.Count; i++)
            {
                if (totalTracksPerResults[i] == 0) continue;
                if (!noHighQualityAssociation.ContainsKey(i)) continue;
                double ratio = ((double)noHighQualityAssociation[i].Count) / (double)totalTracksPerResults[i];
                if (ratio > ObjectCoverageApprovalThresholdPerVideo)
                {
                    noResultsWithHighQualityObjectCoverage++;
                }
            }
            if (noResultsWithHighQualityObjectCoverage < MinResults && compressedResults.Count < MaxResults) //at least three people should have most of their boxes highly corroborated by one other person
            {
                return null;
            }



            //int MaxInconclusiveTrackToTerminate = (int)Math.Ceiling((double)noAssociatedTracks * (1 - ObjectCoverageApprovalThresholdPerVideo));
            int MaxInconclusiveTrackToTerminate = (int)((double)noAssociatedTracks * (1 - ObjectCoverageApprovalThresholdPerVideo));
            //if (MaxInconclusiveTrackToTerminate < approvalDifferenceLimit)
            //{
            //    MaxInconclusiveTrackToTerminate = approvalDifferenceLimit;
            //}

            if ((noAssociatedTracks - noAcceptedTracks) > MaxInconclusiveTrackToTerminate && compressedResults.Count < MaxResults)
            {
                return null;
            }

            

            return aggResult;
        }


        
        public static bool IsAcceptable(SatyamAggregatedResultsTableEntry aggResultEntry, SatyamResultsTableEntry resultEntry,
            int approvalDifferenceLimit = TaskConstants.MULTI_OBJ_TRACKING_MAX_APPROVAL_DIFFERENCE,
            double minSimilarityThreshold = TaskConstants.MULTI_OBJ_TRACKING_MIN_TUBELET_SIMILARITY_THRESHOLD_FOR_PAYMENT,
            double approvalThresholdPerVideoChunk = TaskConstants.MULTI_OBJ_TRACKING_APPROVALRATIO_PER_VIDEO_FOR_PAYMENT)
        {
            //now we need to decide which jobs to accept and which to reject and which are inconclusive
            //1. a job has acceptance potential if it has atleast doneBoxes-2 or doneBoxes*(0.9)
            //2. If there are two or more jobs that have acceptance potential they are both approved or its inconclusive 
            //3. If two or more jobs are accepted , others maybe rejected
            SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(aggResultEntry.ResultString);
            SatyamTask aggTask = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamAggResult.TaskParameters);
            MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(aggTask.jobEntry.JobParameters);
            MultiObjectTrackingAggregatedResult aggresult = JSonUtils.ConvertJSonToObject<MultiObjectTrackingAggregatedResult>(satyamAggResult.AggregatedResultString);

            SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(resultEntry.ResultString);
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);

            VATIC_DVA_CrowdsourcedResult vatic_tmp = createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(satyamResult.TaskResult, task.SatyamURI, aggResultEntry.SatyamTaskTableEntryID.ToString(), job.FrameRate);
            MultiObjectTrackingResult result_tmp = vatic_tmp.getCompressedTracksInTimeSegment();

            // fix for datetime serialization bug
            string serialized = JSonUtils.ConvertObjectToJSon(result_tmp);
            MultiObjectTrackingResult result = JSonUtils.ConvertJSonToObject<MultiObjectTrackingResult>(serialized);

            List<int> acceptancePotential = new List<int>();
            List<int> rejectPotential = new List<int>();

            int MaxErrorTrackToPay = (int)Math.Ceiling((double)aggresult.tracklets.tracks.Count * (1 - approvalThresholdPerVideoChunk));
            if (MaxErrorTrackToPay < approvalDifferenceLimit)
            {
                MaxErrorTrackToPay = approvalDifferenceLimit;
            }

            // compare it against the aggregated 
            List<MultiObjectTrackingResult> compressedResults = new List<MultiObjectTrackingResult>();
            compressedResults.Add(result);
            compressedResults.Add(aggresult.tracklets);

            List<MultipartiteWeightedMatch> association = TrackletsAssociation.AssociateTracklets(compressedResults);

            int noCorrectTrack = 0;
            //for each association, majority vote for boxes for each frame
            foreach (MultipartiteWeightedMatch matchingTracks in association)
            {
                if (matchingTracks.elementList.ContainsKey(1)) // this contains an aggregated track
                {
                    if (matchingTracks.elementList.ContainsKey(0)) // a result track has been associated
                    {
                        double TrackIoU = matchingTracks.getWeight(0, 1);
                        if (TrackIoU >= minSimilarityThreshold)
                        {
                            noCorrectTrack++;
                        }
                        else
                        {

                        }
                    }
                }
                
            }
            if (aggresult.tracklets.tracks.Count - noCorrectTrack <= MaxErrorTrackToPay)
            {
                return true;
            }

            return false;
        }



        // get consensus track for one entity from multiple turker results
        public static CompressedTrack getConsensusTrack(List<CompressedTrack> trackList, double boxToleranceThreshold, int fps)
        {

            if (trackList.Count == 0) return null;

            TimeSpan dt = new TimeSpan(0, 0, 0, 0, (int)Math.Floor(1000.0 / fps));
            // get the timespan superset
            DateTime earliestStartTime = trackList[0].startTime;
            DateTime latestEndTime = trackList[0].endTime;
            foreach (CompressedTrack tck in trackList)
            {
                if (tck.startTime < earliestStartTime)
                {
                    earliestStartTime = tck.startTime;
                }
                if (tck.endTime > latestEndTime)
                {
                    latestEndTime = tck.endTime;
                }
            }
            if (latestEndTime - earliestStartTime <= dt)
            {
                return null;
            }

            // get majority label
            Dictionary<string, int> labelVotes = new Dictionary<string, int>();
            int maxCount = 0;
            string majorityLabel = null;
            foreach (CompressedTrack tck in trackList)
            {
                if (!labelVotes.ContainsKey(tck.label))
                {
                    labelVotes.Add(tck.label, 0);
                }
                labelVotes[tck.label]++;
                if (labelVotes[tck.label] > maxCount)
                {
                    maxCount = labelVotes[tck.label];
                    majorityLabel = tck.label;
                }
            }

            CompressedTrack ret = new CompressedTrack();
            ret.cameraId = trackList[0].cameraId;
            ret.taskName = trackList[0].taskName;
            ret.label = majorityLabel;




            //form boundingbox groups for each frame
            List<SpaceTime> OverlappingPart = new List<SpaceTime>();
            for (DateTime t = earliestStartTime; t <= latestEndTime; t += dt)
            {
                //each paritition has one box per frame only, the second list will always have <=1 member
                List<List<BoundingBox>> boundingBoxes = new List<List<BoundingBox>>();
                for (int i = 0; i < trackList.Count; i++)
                {
                    CompressedTrack tck = trackList[i];
                    boundingBoxes.Add(new List<BoundingBox>());
                    //if (t <= tck.endTime && t >= tck.startTime)
                    //{
                    //if (t >= tck.startTime)
                    //{
                    // the track has a frame
                    SpaceTime st = tck.getSpaceTimeAt(t);
                    BoundingBox tb;
                    if (st == null)
                    {
                        //tb = new BoundingBox(0, 0, 0, 0);
                    }
                    else
                    {
                        tb = st.region;
                        boundingBoxes[i].Add(tb);
                    }
                }

                List<MultipartiteWeightedMatch> association = BoundingBoxAssociation.computeBoundingBoxAssociations(boundingBoxes);

                //List<BoundingBox> doneBoxes; List<int> approvedBoxCounts; List<int> rejectedBoxCounts; List<int> inconclusiveBoxCounts; List<string> approvedBoxesIdentifier; List<string> rejectedBoxesIdentifier; List<string> inconclusiveBoxesIdentifier; List<List<BoundingBox>> missedBoxes; List<List<BoundingBox>> rejectBoxes; List<List<BoundingBox>> inconcludedBoxes;

                //MajorityVoteBoundingBoxes(association, boundingBoxes, boxToleranceThreshold, out doneBoxes, out approvedBoxCounts, out rejectedBoxCounts, out inconclusiveBoxCounts, out approvedBoxesIdentifier, out rejectedBoxesIdentifier, out inconclusiveBoxesIdentifier, out missedBoxes, out rejectBoxes, out inconcludedBoxes);

                double deviation_threshold_x = boxToleranceThreshold;
                double deviation_threshold_y = boxToleranceThreshold;
                List<List<BoundingBoxGroup>> allBoxGroups = new List<List<BoundingBoxGroup>>();
                List<BoundingBox> doneBoxes = new List<BoundingBox>(); //stores the aggregated boxes

                // find the associaiton with max num of partitions
                int maxAssociationCount = 0;
                int maxAssociationIndex = -1;
                for(int k= 0; k < association.Count;k++)
                {
                    if (association[k].elementList.Count > maxAssociationCount)
                    {
                        maxAssociationCount = association[k].elementList.Count;
                        maxAssociationIndex = k;
                    }
                }

                MultipartiteWeightedMatch match = association[maxAssociationIndex];
                List<BoundingBox> boxList = new List<BoundingBox>();
                List<string> identifiers = new List<string>();
                foreach (KeyValuePair<int, int> entry in match.elementList)
                {
                    boxList.Add(boundingBoxes[entry.Key][entry.Value]);
                    identifiers.Add(entry.Key + "_" + entry.Value);
                }
                List<BoundingBoxGroup> boxGroups = MergeAndGroupBoundingBoxes.GreedyMeanHierarchicalMergeByPixelDeviation(boxList, identifiers, deviation_threshold_x, deviation_threshold_y);

                allBoxGroups.Add(boxGroups);

                //find the boxgroup with the maxCount
                maxCount = 0;
                int index = -1;
                for (int i = 0; i < boxGroups.Count; i++)
                {
                    if (boxGroups[i].boundingBoxList.Count > maxCount) // there are two boxes within acceptance range
                    {
                        maxCount = boxGroups[i].boundingBoxList.Count;
                        index = i;
                    }
                }
                if (maxCount > 1)
                {
                    doneBoxes.Add(boxGroups[index].mergedBoundingBox);
                    OverlappingPart.Add(new SpaceTime(doneBoxes[0], t));
                }
            }

            ret.startTime = DateTime.MaxValue;
            ret.endTime = DateTime.MinValue;

            foreach (SpaceTime st in OverlappingPart)
            {
                DateTime t = st.time;
                if (ret.startTime > t)
                {
                    ret.startTime = t;
                }
                if (ret.endTime < t)
                {
                    ret.endTime = t;
                }
            }

            foreach (SpaceTime st in OverlappingPart)
            {
                DateTime t = st.time;
                ret.spaceTimeTrack.space_time_track.Add(st);

                //add attribute only if there is a concluded box, the index must match
                //majority vote on booleanAttibutes
                Dictionary<string, double> booleanAggregate = new Dictionary<string, double>();
                int no = 0;
                foreach (CompressedTrack tck in trackList)
                {
                    bool silence = false;
                    foreach (KeyValuePair<string, CompressedBooleanAttributeTrack> bt in tck.booleanAttributeTracks)
                    {
                        if (!booleanAggregate.ContainsKey(bt.Key))
                        {
                            booleanAggregate.Add(bt.Key, 0);
                        }

                        BooleanAttribute attr = bt.Value.getAttributeAt(t);
                        if (attr == null)
                        {
                            //if (bt.Key == "outofview") booleanAggregate[bt.Key]++;
                            silence = true;
                        }
                        else
                        {
                            if (attr.value)
                            {
                                booleanAggregate[bt.Key]++;
                            }
                        }
                    }
                    if (!silence)
                    {
                        no++;
                    }
                }
                foreach (KeyValuePair<string, double> ba in booleanAggregate)
                {
                    if (!ret.booleanAttributeTracks.ContainsKey(ba.Key))
                    {
                        ret.booleanAttributeTracks.Add(ba.Key, new CompressedBooleanAttributeTrack());
                        ret.booleanAttributeTracks[ba.Key].attribute_name = ba.Key;
                        ret.booleanAttributeTracks[ba.Key].startTime = ret.startTime;
                        ret.booleanAttributeTracks[ba.Key].endTime = ret.endTime;
                    }
                    if (ba.Value / (double)no >= 0.5)
                    {
                        ret.booleanAttributeTracks[ba.Key].attribute_track.Add(new BooleanAttribute(t, true));
                    }
                    else
                    {
                        ret.booleanAttributeTracks[ba.Key].attribute_track.Add(new BooleanAttribute(t, false));
                    }
                }
            }
            

            ret.spaceTimeTrack.startTime = ret.startTime;
            ret.spaceTimeTrack.endTime = ret.endTime;
            return ret;
        }


        public static Dictionary<int, int> getMajorityGroupTracks(Dictionary<int, int> associations, List<MultiObjectTrackingResult> mTurkEntries, int fps, double minSimilarityThreshold)
        {
            Dictionary<int, int> majorityGroup = new Dictionary<int, int>();
            if (associations.Count == 0) return majorityGroup;
            List<CompressedTrack> trackList = new List<CompressedTrack>();
            List<string> identifiers = new List<string>();
            foreach (KeyValuePair<int, int> amatch in associations)
            {
                string matchString = amatch.Key + "_" + amatch.Value;
                trackList.Add(mTurkEntries[amatch.Key].tracks[amatch.Value]);
                identifiers.Add(matchString);
            }
            List<CompressedTrackGroup> tGroup = GreedyMeanHierarchicalMerge(trackList, identifiers, fps, minSimilarityThreshold);

            int partNo = -1;
            if (tGroup.Count == 1) // only one group
            {
                partNo = 0;
            }
            else
            {
                //first find the one with the largest number of 
                int maxCount = 0;
                for (int i = 0; i < tGroup.Count; i++)
                {
                    CompressedTrackGroup gr = tGroup[i];
                    int cnt = gr.CompressedTrackList.Count;
                    if (cnt > maxCount)
                    {
                        maxCount = cnt;
                        partNo = i;
                    }
                }
            }
            foreach (string identifier in tGroup[partNo].identifiers)
            {
                string[] fields = identifier.Split('_');
                majorityGroup.Add(Convert.ToInt32(fields[0]), Convert.ToInt32(fields[1]));
            }
            return majorityGroup;
        }
        public static bool AcceptAndRejectTracks(CompressedTrack consensusTrack, 
            double approvalThreshold, 
            int approvalDifferenceLimit, 
            int consensusNumber, 
            List<int> approvedBoxCountsPerTrack, 
            List<int> rejectedBoxCountsPerTrack, 
            List<int> inconclusiveBoxCountsPerTrack, 
            int concludedBoxCount, 
            List<List<BoundingBox>> missedBoxes, 
            List<List<BoundingBox>> rejectBoxes, 
            List<LineSegment> lines, 
            out List<int> inConclusivePartition, out List<int> acceptedPartition, out List<int> rejectedPartition)
        {

            inConclusivePartition = new List<int>();
            acceptedPartition = new List<int>();
            rejectedPartition = new List<int>();

            List<int> acceptancePotential = new List<int>();
            List<int> rejectPotential = new List<int>();

            int acceptanceThresold = (int)Math.Ceiling((double)concludedBoxCount * (1 - approvalThreshold));
            if (acceptanceThresold < approvalDifferenceLimit)
            {
                acceptanceThresold = approvalDifferenceLimit;
            }
            for (int i = 0; i < approvedBoxCountsPerTrack.Count; i++)
            {
                if (concludedBoxCount - approvedBoxCountsPerTrack[i] <= acceptanceThresold)
                {
                    acceptancePotential.Add(i);
                }
                else
                {
                    rejectPotential.Add(i);
                }
            }

            //now for the guys who have reject potential run another check
            //to see if the boxes they have missed are at boundary of the scene
            if (lines != null)
            {
                //List<int[,]> lines = mTurkEntries[0].getBoundaryLines();
                List<double[,]> dlines = new List<double[,]>();
                for (int j = 0; j < lines.Count; j++)
                {
                    double[,] dline = new double[2, 2];
                    dline[0, 0] = (double)lines[j].x1;
                    dline[0, 1] = (double)lines[j].y1;
                    dline[1, 0] = (double)lines[j].x2;
                    dline[1, 1] = (double)lines[j].y1;
                    dlines.Add(dline);
                }

                List<int> pardonList = new List<int>();
                for (int i = 0; i < rejectPotential.Count; i++)
                {
                    int entryID = rejectPotential[i];
                    int noPardons = 0;
                    for (int j = 0; j < missedBoxes[entryID].Count; j++)
                    {
                        bool intersect = missedBoxes[entryID][j].intersectsLineSegments(dlines);
                        if (intersect)
                        {
                            noPardons++; //boxes at the endes are pardoned
                        }
                    }
                    int finalMissed = missedBoxes[entryID].Count - noPardons;
                    noPardons = 0;
                    for (int j = 0; j < rejectBoxes[entryID].Count; j++)
                    {
                        bool intersect = rejectBoxes[entryID][j].intersectsLineSegments(dlines);
                        if (intersect)
                        {
                            noPardons++; //boxes at the endes are pardoned
                        }
                    }
                    int finalRejects = rejectBoxes[entryID].Count - noPardons;
                    int totalMistakes = finalRejects + finalMissed;
                    if (totalMistakes <= acceptanceThresold)
                    {
                        pardonList.Add(entryID);
                    }
                }

                for (int i = 0; i < pardonList.Count; i++)
                {
                    rejectPotential.Remove(pardonList[i]);
                    acceptancePotential.Add(pardonList[i]);
                }
            }

            


            bool inconclusive = true;
            if (acceptancePotential.Count >= consensusNumber)
            {
                inconclusive = false;
            }

            if (inconclusive)
            {

            }
            else
            {
                for (int i = 0; i < acceptancePotential.Count; i++)
                {
                    acceptedPartition.Add(acceptancePotential[i]);
                }
                for (int i = 0; i < rejectPotential.Count; i++)
                {
                    rejectedPartition.Add(rejectPotential[i]);
                }
            }
            return inconclusive;
        }



        public static CompressedTrack computeMeanCompressedTrackFromTwoGroups(List<CompressedTrack> tgroup1, List<CompressedTrack> tgroup2, int fps)
        {

            // Assumption: same label type
            // find out the maximum time span
            TimeSpan dt = new TimeSpan(0, 0, 0, 0, (int)Math.Floor(1000.0 / fps));

            DateTime commonStartTime = tgroup1[0].startTime;
            DateTime commonEndTime = tgroup1[0].endTime;

            List<CompressedTrack> aggrGroup = new List<CompressedTrack>();
            foreach (CompressedTrack tck in tgroup1)
            {
                aggrGroup.Add(tck);
            }
            foreach (CompressedTrack tck in tgroup2)
            {
                aggrGroup.Add(tck);
            }

            foreach (CompressedTrack tck in aggrGroup)
            {
                if (tck.startTime < commonStartTime)
                {
                    commonStartTime = tck.startTime;
                }
                if (tck.endTime > commonEndTime)
                {
                    commonEndTime = tck.endTime;
                }
            }
            // calcualte the mean tracking, averaging all results that has the same frame, majority voting for boolean attributes
            CompressedTrack meanT = new CompressedTrack();
            meanT.startTime = commonStartTime; meanT.endTime = commonEndTime;
            meanT.cameraId = tgroup1[0].cameraId;
            meanT.taskName = tgroup1[0].taskName;
            meanT.label = tgroup1[0].label;
            meanT.spaceTimeTrack.startTime = commonStartTime; meanT.spaceTimeTrack.endTime = commonEndTime;


            for (DateTime t = commonStartTime; t <= commonEndTime; t += dt)
            {
                int no = 0;
                Dictionary<string, double> booleanAggregate = new Dictionary<string, double>();
                BoundingBox b = new BoundingBox(0, 0, 0, 0);
                foreach (CompressedTrack tck in aggrGroup)
                {
                    if (t <= tck.endTime && t >= tck.startTime)
                    {
                        BoundingBox tb = tck.getSpaceTimeAt(t).region;
                        b.tlx += tb.tlx;
                        b.tly += tb.tly;
                        b.brx += tb.brx;
                        b.bry += tb.bry;
                        foreach (KeyValuePair<string, CompressedBooleanAttributeTrack> bt in tck.booleanAttributeTracks)
                        {
                            if (!booleanAggregate.ContainsKey(bt.Key))
                            {
                                booleanAggregate.Add(bt.Key, 0);
                            }

                            BooleanAttribute attr = bt.Value.getAttributeAt(t);
                            if (attr == null)
                            {
                                //if (bt.Key == "outofview") booleanAggregate[bt.Key]++;
                            }
                            else
                            {
                                if (attr.value)
                                {
                                    booleanAggregate[bt.Key]++;
                                }
                            }
                        }
                        no++;
                    }
                }
                if (no == 0) continue; // nothing in a particular frame
                // get the mean box
                b.tlx /= no;
                b.tly /= no;
                b.brx /= no;
                b.bry /= no;
                meanT.spaceTimeTrack.space_time_track.Add(new SpaceTime(b, t));
                // get the majority attribute
                foreach (KeyValuePair<string, double> ba in booleanAggregate)
                {
                    if (!meanT.booleanAttributeTracks.ContainsKey(ba.Key))
                    {
                        meanT.booleanAttributeTracks.Add(ba.Key, new CompressedBooleanAttributeTrack());
                        meanT.booleanAttributeTracks[ba.Key].attribute_name = ba.Key;
                        meanT.booleanAttributeTracks[ba.Key].startTime = commonStartTime;
                        meanT.booleanAttributeTracks[ba.Key].endTime = commonEndTime;
                    }
                    if (ba.Value / no > 0.5)
                    {
                        meanT.booleanAttributeTracks[ba.Key].attribute_track.Add(new BooleanAttribute(t, true));
                    }
                    else
                    {
                        meanT.booleanAttributeTracks[ba.Key].attribute_track.Add(new BooleanAttribute(t, false));
                    }
                }
            }

            return meanT;
        }


        public static List<CompressedTrackGroup> GreedyMeanHierarchicalMerge(List<CompressedTrack> originalTracks, List<string> originalIdentifiers, int fps, double minOverlapThreshold)
        {

            List<CompressedTrackGroup> ret = new List<CompressedTrackGroup>();

            //initialize by creating one group per track
            for (int i = 0; i < originalTracks.Count; i++)
            {
                CompressedTrack b = originalTracks[i];
                CompressedTrackGroup tgroup = new CompressedTrackGroup();
                tgroup.mergedCompressedTrack = b;
                tgroup.CompressedTrackList.Add(b);
                tgroup.identifiers.Add(originalIdentifiers[i]);
                ret.Add(tgroup);
            }

            bool changed = false;

            do
            {
                changed = false;
                //find the closest mergable tracks group pair and merge by averaging
                int index1 = -1;
                int index2 = -1;

                double bestSimilarity = 0;

                CompressedTrack bestMergedTrack = null;
                for (int i = 0; i < ret.Count - 1; i++)
                {
                    for (int j = i + 1; j < ret.Count; j++)
                    {
                        CompressedTrackSimilarityMetric.ICompressedTrackSimilarityMetric metric = new CompressedTrackSimilarityMetric.TubeletIoU();

                        //compute the merged bouding box
                        //CompressedTrack meanT = computeMeanCompressedTrackFromTwoGroups(ret[i].CompressedTrackList, ret[j].CompressedTrackList, fps);

                        //double sim1 = metric.getMetric(meanT, ret[i].mergedCompressedTrack);
                        //double sim2 = metric.getMetric(meanT, ret[j].mergedCompressedTrack);
                        //double minSim = Math.Min(sim1, sim2);

                        double minSim = metric.getMetric(ret[i].mergedCompressedTrack, ret[j].mergedCompressedTrack);
                        
                        if (minSim >= minOverlapThreshold)
                        {
                            if (minSim > bestSimilarity)
                            {
                                bestSimilarity = minSim;
                                index1 = i;
                                index2 = j;
                                //bestMergedTrack = meanT;
                                bestMergedTrack = computeMeanCompressedTrackFromTwoGroups(ret[i].CompressedTrackList, ret[j].CompressedTrackList, fps);
                                changed = true;
                            }
                        }
                    }
                }

                if (changed) //there is something worth merging
                {
                    //merge 2 into 1
                    for (int i = 0; i < ret[index2].CompressedTrackList.Count; i++)
                    {
                        ret[index1].CompressedTrackList.Add(ret[index2].CompressedTrackList[i]);
                        ret[index1].identifiers.Add(ret[index2].identifiers[i]);
                    }
                    ret[index1].mergedCompressedTrack = bestMergedTrack;

                    //remove 2
                    ret.RemoveAt(index2);
                }
                if (ret.Count == 1)
                {
                    break;
                }

            } while (changed);

            return ret;
        }

        public static void MajorityVoteBoundingBoxes(List<MultipartiteWeightedMatch> association, List<List<BoundingBox>> boundingBoxes, double boxToleranceThreshold,
            out List<BoundingBox> doneBoxes,
            out List<int> approvedCounts, out List<int> rejectedCounts, out List<int> inconclusiveCounts,
            out List<string> approvedBoxesIdentifier, out List<string> rejectedBoxesIdentifier, out List<string> inconclusiveBoxesIdentifier,
            out List<List<BoundingBox>> missedBoxes, out List<List<BoundingBox>> rejectBoxes, out List<List<BoundingBox>> inconcludedBoxes)
        {

            //for each association group similar boxes 
            List<List<BoundingBoxGroup>> associationGrouping = new List<List<BoundingBoxGroup>>();
            foreach (MultipartiteWeightedMatch matching in association)
            {
                List<BoundingBox> boundingBoxList = new List<BoundingBox>();
                List<string> identifiers = new List<string>();
                /*for(int i=0;i<matching.elementList.Count;i++)
                {
                    string match = i + "_" + matching.elementList[i];
                    boundingBoxList.Add(boundingBoxes[i][matching.elementList[i]]);
                    identifiers.Add(match);
                }*/
                foreach (KeyValuePair<int, int> amatch in matching.elementList)
                {
                    string matchString = amatch.Key + "_" + amatch.Value;
                    boundingBoxList.Add(boundingBoxes[amatch.Key][amatch.Value]);
                    identifiers.Add(matchString);
                }
                List<BoundingBoxGroup> groups = MergeAndGroupBoundingBoxes.GreedyMeanHierarchicalMergeByPixelDeviation(boundingBoxList, identifiers, boxToleranceThreshold);
                associationGrouping.Add(groups);
            }

            doneBoxes = new List<BoundingBox>();

            approvedCounts = new List<int>();
            rejectedCounts = new List<int>();
            inconclusiveCounts = new List<int>();

            approvedBoxesIdentifier = new List<string>();
            rejectedBoxesIdentifier = new List<string>();
            inconclusiveBoxesIdentifier = new List<string>();

            missedBoxes = new List<List<BoundingBox>>();
            rejectBoxes = new List<List<BoundingBox>>();
            inconcludedBoxes = new List<List<BoundingBox>>();

            //statistical attributes for each entry
            foreach (List<BoundingBox> boxes in boundingBoxes)
            {
                //boxCounts.Add(boxes.Count);
                approvedCounts.Add(0);
                rejectedCounts.Add(0);
                inconclusiveCounts.Add(0);
                missedBoxes.Add(new List<BoundingBox>());
                rejectBoxes.Add(new List<BoundingBox>());
                inconcludedBoxes.Add(new List<BoundingBox>());
            }

            foreach (List<BoundingBoxGroup> boxGroup in associationGrouping)
            {
                if (boxGroup.Count == 0)
                {
                    break;
                }
                if (boxGroup.Count == 1)
                {
                    if (boxGroup[0].boundingBoxList.Count > 1) //two boxes coincide!
                    {
                        doneBoxes.Add(boxGroup[0].mergedBoundingBox);
                        List<int> doneParitions = new List<int>();
                        foreach (string identifier in boxGroup[0].identifiers)
                        {
                            string[] fields = identifier.Split('_');
                            int partitionID = Convert.ToInt32(fields[0]);
                            approvedCounts[partitionID]++;
                            approvedBoxesIdentifier.Add(identifier);
                            doneParitions.Add(partitionID);
                        }
                        for (int i = 0; i < boundingBoxes.Count; i++)
                        {
                            if (!doneParitions.Contains(i))
                            {
                                missedBoxes[i].Add(boxGroup[0].mergedBoundingBox);
                            }
                        }
                    }
                    else
                    {
                        inconclusiveBoxesIdentifier.Add(boxGroup[0].identifiers[0]);
                        string[] fields = boxGroup[0].identifiers[0].Split('_');
                        int partitionID = Convert.ToInt32(fields[0]);
                        inconclusiveCounts[partitionID]++;
                        inconcludedBoxes[partitionID].Add(boxGroup[0].mergedBoundingBox);
                    }
                }
                else
                {
                    //first find the one with the largest number of 
                    int maxCount = 0;
                    int partNo = -1;
                    for (int i = 0; i < boxGroup.Count; i++)
                    {
                        BoundingBoxGroup gr = boxGroup[i];
                        int cnt = gr.boundingBoxList.Count;
                        if (cnt > maxCount)
                        {
                            maxCount = cnt;
                            partNo = i;
                        }
                    }
                    if (boxGroup[partNo].boundingBoxList.Count > 1) //two boxes coincide!
                    {
                        List<int> doneParitions = new List<int>();
                        doneBoxes.Add(boxGroup[partNo].mergedBoundingBox);
                        for (int i = 0; i < boxGroup.Count; i++)
                        {
                            if (i == partNo)
                            {
                                foreach (string identifier in boxGroup[partNo].identifiers)
                                {
                                    string[] fields = identifier.Split('_');
                                    int partitionID = Convert.ToInt32(fields[0]);
                                    approvedCounts[partitionID]++;
                                    approvedBoxesIdentifier.Add(identifier);
                                    doneParitions.Add(partitionID);

                                }
                            }
                            else
                            {
                                foreach (string identifier in boxGroup[i].identifiers)
                                {
                                    string[] fields = identifier.Split('_');
                                    int partitionID = Convert.ToInt32(fields[0]);
                                    rejectedCounts[partitionID]++;
                                    rejectedBoxesIdentifier.Add(identifier);
                                    rejectBoxes[partitionID].Add(boxGroup[partNo].mergedBoundingBox);
                                    doneParitions.Add(partitionID);
                                }
                            }
                        }
                        for (int i = 0; i < boundingBoxes.Count; i++)
                        {
                            if (!doneParitions.Contains(i))
                            {
                                missedBoxes[i].Add(boxGroup[partNo].mergedBoundingBox);
                            }
                        }
                    }
                }
            }
        }



        
    }


}
