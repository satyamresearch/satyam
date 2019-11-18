using AzureBlobStorage;
using HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SatyamResultClasses
{
    public class VATIC_DVA_Frame
    {
        private DateTime t;
        public DateTime time
        {
            get { return t; }
            set
            {
                t = DateTime.SpecifyKind(value, DateTimeKind.Local);
            }
        }
        public int tlx;
        public int tly;
        public int brx;
        public int bry;
        public Boolean occluded;
        public Boolean outofview;
        public VATIC_DVA_Frame(DateTime l_time, int l_tlx, int l_tly, int l_brx, int l_bry, bool l_occluded, bool l_outofview = false)
        {
            tlx = l_tlx;
            tly = l_tly;
            brx = l_brx;
            bry = l_bry;
            time = l_time;
            occluded = l_occluded;
            outofview = l_outofview;
        }
        public VATIC_DVA_Frame(string locString)
        {
            string[] fields = locString.Split(',');
            time = Utilities.DateTimeUtilities.getDateTimeFromString(fields[0]);
            tlx = Convert.ToInt32(fields[1]);
            tly = Convert.ToInt32(fields[2]);
            brx = Convert.ToInt32(fields[3]);
            bry = Convert.ToInt32(fields[4]);
            occluded = Convert.ToBoolean(fields[5]);
            outofview = Convert.ToBoolean(fields[6]);
        }

        public VATIC_DVA_Frame(VATIC_DVA_Frame loc, DateTime t)
        {
            tlx = loc.tlx;
            tly = loc.tly;
            brx = loc.brx;
            bry = loc.bry;
            occluded = loc.occluded;
            outofview = loc.outofview;
            time = new DateTime(t.Ticks);
        }

        public VATIC_DVA_Frame(VATIC_DVA_Frame loc)
        {
            tlx = loc.tlx;
            tly = loc.tly;
            brx = loc.brx;
            bry = loc.bry;
            occluded = loc.occluded;
            outofview = loc.outofview;
            time = new DateTime(loc.time.Ticks);
        }

        public override string ToString()
        {
            return (DateTimeUtilities.convertDateTimeToString(time) + "," + tlx + "," + tly + "," + brx + "," + bry);
        }

        public string ToAnnotationString()
        {
            return (DateTimeUtilities.convertDateTimeToString(time) + "," + tlx + "," + tly + "," + brx + "," + bry + "," + occluded + "," + outofview);
        }

        public bool IsSameExceptTime(VATIC_DVA_Frame secondLastFrame)
        {
            if (this.tlx == secondLastFrame.tlx && this.tly == secondLastFrame.tly && this.brx == secondLastFrame.brx && this.bry == secondLastFrame.bry 
                && this.occluded == secondLastFrame.occluded && this.outofview == secondLastFrame.outofview)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class VATIC_DVA_FrameSequence
    {
        public List<VATIC_DVA_Frame> frames = new List<VATIC_DVA_Frame>();
        private DateTime start;
        private DateTime end;
        public DateTime startTime
        {
            get { return start; }
            set { start = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public DateTime endTime
        {
            get { return end; }
            set { end = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }

        // commented out on 04/30/2018: this constructor has in mind the surveillance scenario, switched to the new one for KITTI groundtruth visualization id match, pls double check on other effects
        //public VATIC_DVA_FrameSequence(string locString, DateTime videoEndTime)
        //{
        //    bool started = false;
        //    bool brokeInMiddle = false;
        //    string[] fields = locString.Split(':');
        //    for (int i = 1; i < fields.Length; i++)
        //    {
        //        VATIC_DVA_Frame l = new VATIC_DVA_Frame(fields[i]);
        //        if (l.tlx < 0 || l.tly < 0 || l.brx < 0 || l.bry < 0)
        //        {
        //            continue;
        //        }

        //        if (l.outofview == false)
        //        {
        //            frames.Add(l);
        //            if (!started)
        //            {
        //                started = true;
        //                startTime = l.time;
        //            }
        //            else
        //            {
        //                endTime = l.time;
        //            }
        //        }
        //        else
        //        {
        //            if (started == true)
        //            {
        //                endTime = l.time;
        //                frames.Add(l);
        //                brokeInMiddle = true;
        //                break;
        //            }
        //        }
        //    }
        //    //// commented on 04/24/2018 for stitching, double check effect on others
        //    //if (brokeInMiddle == false)
        //    //{
        //    //    if (endTime < videoEndTime)
        //    //    {
        //    //        endTime = videoEndTime;
        //    //    }
        //    //}
        //}


        public VATIC_DVA_FrameSequence(string locString, DateTime videoEndTime)
        {
            bool started = false;
            //bool brokeInMiddle = false;
            bool ended = false;
            string[] fields = locString.Split(':');
            for (int i = 1; i < fields.Length; i++)
            {
                VATIC_DVA_Frame l = new VATIC_DVA_Frame(fields[i]);
                if (l.tlx < 0 || l.tly < 0 || l.brx < 0 || l.bry < 0)
                {
                    continue;
                }
                

                if (l.time == videoEndTime && frames.Count>0)
                {
                    // 05012018 hack to fix the UI bug of concatenating a frame at the end of sequence incorrect
                    //if last frame box != second last box
                    //If last frame exactly = one of the box other than second last box,
                    //Then change the last box to be the second last box, together with the booleans….
                    VATIC_DVA_Frame secondLastFrame = frames[frames.Count - 1];
                    if (l.IsSameExceptTime(secondLastFrame))
                    {
                        // the last frame is auto inserted correctly
                    }
                    else
                    {
                        foreach (VATIC_DVA_Frame prev in frames)
                        {
                            if (l.IsSameExceptTime(prev))
                            {
                                l.tlx = secondLastFrame.tlx;
                                l.tly = secondLastFrame.tly;
                                l.brx = secondLastFrame.brx;
                                l.bry = secondLastFrame.bry;
                                l.occluded = secondLastFrame.occluded;
                                l.outofview = secondLastFrame.outofview;
                                break;
                            }
                        }
                    }

                }

                frames.Add(l);
                if (l.outofview == false)
                {
                    if (!started)
                    {
                        started = true;
                        startTime = l.time;
                    }
                    else
                    {
                        endTime = l.time;
                    }
                    ended = false;
                }
                else
                {
                    if (started == true && ended == false)
                    {
                        endTime = l.time;
                        //brokeInMiddle = true;
                        ended = true;
                        break; // must break it because the result will always come as an end a in view not occluded extrapolated frame...yea....
                    }
                }
            }
            //// commented on 04/24/2018 for stitching, double check effect on others
            //if (brokeInMiddle == false)
            //{
            //    if (endTime < videoEndTime)
            //    {
            //        endTime = videoEndTime;
            //    }
            //}
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Locations:");
            for (int i = 0; i < frames.Count; i++)
            {
                string s = frames[i].ToString();
                sb.Append(":" + s);
            }
            return sb.ToString();
        }

        public CompressedSpaceTimeTrack getCompressedSpaceTimeTrack()
        {
            CompressedSpaceTimeTrack cstt = new CompressedSpaceTimeTrack();
            cstt.startTime = startTime;
            cstt.endTime = endTime;
            foreach (VATIC_DVA_Frame f in frames)
            {
                SpaceTime st = new SpaceTime(f.time, f.tlx, f.tly, f.brx, f.bry);
                cstt.space_time_track.Add(st);
            }

            //remove redundancies to be done


            return cstt;
        }

        public CompressedBooleanAttributeTrack getOcclusionTrack(DateTime l_startTime, DateTime l_endTime)
        {
            CompressedBooleanAttributeTrack cbat = new CompressedBooleanAttributeTrack();
            cbat.attribute_name = "occlusion";
            cbat.startTime = l_startTime;
            cbat.endTime = l_endTime;
            //bool started = false;
            //BooleanAttribute lastBA = null;
            foreach (VATIC_DVA_Frame f in frames)
            {
                BooleanAttribute ba = new BooleanAttribute(f.time, f.occluded);
                cbat.attribute_track.Add(ba);
            }
            return cbat;
        }


        public CompressedBooleanAttributeTrack getOutOfViewTrack(DateTime l_startTime, DateTime l_endTime)
        {
            CompressedBooleanAttributeTrack cbat = new CompressedBooleanAttributeTrack();
            cbat.attribute_name = "outofview";
            cbat.startTime = l_startTime;
            cbat.endTime = l_endTime;
            //bool started = false;
            //BooleanAttribute lastBA = null;
            foreach (VATIC_DVA_Frame f in frames)
            {
                BooleanAttribute ba = new BooleanAttribute(f.time, f.outofview);
                cbat.attribute_track.Add(ba);
            }
            return cbat;
        }

        //public CompressedBooleanAttributeTrack getOcclusionTrack(DateTime l_startTime, DateTime l_endTime)
        //{
        //    CompressedBooleanAttributeTrack cbat = new CompressedBooleanAttributeTrack();
        //    cbat.attribute_name = "occlusion";
        //    cbat.startTime = l_startTime;
        //    cbat.endTime = l_endTime;
        //    bool started = false;
        //    BooleanAttribute lastBA = null;
        //    foreach (VATIC_DVA_Frame f in frames)
        //    {
        //        if (!started)
        //        {
        //            if (f.occluded == true)
        //            {
        //                BooleanAttribute ba = new BooleanAttribute(f.time, f.occluded);
        //                lastBA = ba;
        //                cbat.attribute_track.Add(ba);
        //                started = true;
        //            }
        //        }
        //        else
        //        {
        //            if (lastBA.value != f.occluded)
        //            {
        //                BooleanAttribute ba = new BooleanAttribute(f.time, f.occluded);
        //                lastBA = ba;
        //                cbat.attribute_track.Add(ba);
        //            }
        //        }
        //    }
        //    return cbat;
        //}


        //public CompressedBooleanAttributeTrack getOutOfViewTrack(DateTime l_startTime, DateTime l_endTime)
        //{
        //    CompressedBooleanAttributeTrack cbat = new CompressedBooleanAttributeTrack();
        //    cbat.attribute_name = "outofview";
        //    cbat.startTime = l_startTime;
        //    cbat.endTime = l_endTime;
        //    bool started = false;
        //    BooleanAttribute lastBA = null;
        //    foreach (VATIC_DVA_Frame f in frames)
        //    {
        //        if (!started)
        //        {
        //            if (f.outofview == true)
        //            {
        //                BooleanAttribute ba = new BooleanAttribute(f.time, f.outofview);
        //                lastBA = ba;
        //                cbat.attribute_track.Add(ba);
        //                started = true;
        //            }
        //        }
        //        else
        //        {
        //            if (lastBA.value != f.outofview)
        //            {
        //                BooleanAttribute ba = new BooleanAttribute(f.time, f.outofview);
        //                lastBA = ba;
        //                cbat.attribute_track.Add(ba);
        //            }
        //        }
        //    }
        //    return cbat;
        //}
    }


    public class VATIC_DVA_BooleanAttribute
    {
        public bool value;
        private DateTime t;
        public DateTime time
        {
            get { return t; }
            set { t = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public VATIC_DVA_BooleanAttribute(DateTime l_time, bool l_value)
        {
            time = l_time;
            value = l_value;
        }

        public VATIC_DVA_BooleanAttribute(string attributeString)
        {
            string[] fields = attributeString.Split(',');
            time = Utilities.DateTimeUtilities.getDateTimeFromString(fields[0]);
            value = Convert.ToBoolean(fields[1]);
        }

        public override string ToString()
        {
            return (Utilities.DateTimeUtilities.convertDateTimeToString(time) + "," + value);
        }
    }

    public class VATIC_DVA_BooleanAttributeSequence
    {
        public List<VATIC_DVA_BooleanAttribute> attribute_track = new List<VATIC_DVA_BooleanAttribute>();
        public String attribute_name;
        private DateTime start;
        private DateTime end;
        public DateTime startTime
        {
            get { return start; }
            set { start = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }

        public DateTime endTime
        {
            get { return end; }
            set { end = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public VATIC_DVA_BooleanAttributeSequence(string attributeString, DateTime l_startTime, DateTime l_endTime)
        {
            startTime = l_startTime;
            endTime = l_endTime;
            string[] fields = attributeString.Split(':');
            attribute_name = fields[0];
            for (int i = 1; i < fields.Length; i++)
            {
                VATIC_DVA_BooleanAttribute a = new VATIC_DVA_BooleanAttribute(fields[i]);
                if (a.time >= startTime && a.time < endTime)
                {
                    attribute_track.Add(a);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(attribute_name);
            for (int i = 0; i < attribute_track.Count; i++)
            {
                string s = attribute_track[i].ToString();
                sb.Append(":" + s);
            }
            return sb.ToString();
        }

        // null if all false, only insert a frame when there is true
        public CompressedBooleanAttributeTrack getCompressedBooleanAttributeTrack(DateTime l_startTime, DateTime l_endTime)
        {
            CompressedBooleanAttributeTrack cbat = new CompressedBooleanAttributeTrack();
            cbat.attribute_name = attribute_name;
            cbat.startTime = l_startTime;
            cbat.endTime = l_endTime;
            bool started = false;
            BooleanAttribute lastBA = null;

            if (attribute_track.Count == 0)
            {
                BooleanAttribute ba = new BooleanAttribute(l_startTime, false);
                lastBA = ba;
                cbat.attribute_track.Add(ba);
                return cbat;
            }

            foreach (VATIC_DVA_BooleanAttribute f in attribute_track)
            {
                if (!started)
                {
                    // put an initial dummy data anyway to start
                    //if (f.value == true)
                    //{
                        BooleanAttribute ba = new BooleanAttribute(f.time, f.value);
                        lastBA = ba;
                        cbat.attribute_track.Add(ba);
                        started = true;
                    //}
                }
                else
                {
                    if (lastBA.value != f.value)
                    {
                        BooleanAttribute ba = new BooleanAttribute(f.time, f.value);
                        lastBA = ba;
                        cbat.attribute_track.Add(ba);
                    }
                }
            }
            return cbat;
        }

    }

    public class VATIC_DVA_CompleteTrack
    {
        private DateTime start;
        private DateTime end;
        public DateTime startTime
        {
            get { return start; }
            set { start = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public DateTime endTime
        {
            get { return end; }
            set { end = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public String label;
        public string cameraId;
        public VATIC_DVA_FrameSequence locationTrack;
        public Dictionary<string, VATIC_DVA_BooleanAttributeSequence> booleanAttributeTracks = new Dictionary<string, VATIC_DVA_BooleanAttributeSequence>();

        public VATIC_DVA_CompleteTrack(string annotatedString, String l_cameraID, DateTime videoEndTime)
        {
            cameraId = l_cameraID;
            
            string[] fields = annotatedString.Split('_');
            label = fields[0];
            locationTrack = new VATIC_DVA_FrameSequence(fields[1], videoEndTime);
            startTime = locationTrack.startTime;
            endTime = locationTrack.endTime;
            for (int i = 2; i < fields.Length; i++)
            {
                VATIC_DVA_BooleanAttributeSequence bt = new VATIC_DVA_BooleanAttributeSequence(fields[i], startTime, endTime);
                string attributeType = bt.attribute_name;
                booleanAttributeTracks.Add(attributeType, bt);
            }
        }

        public CompressedTrack getCompressedTrack()
        {
            CompressedTrack ct = new CompressedTrack();
            ct.startTime = startTime;
            ct.endTime = endTime;
            ct.label = label;
            ct.cameraId = cameraId;
            ct.spaceTimeTrack = locationTrack.getCompressedSpaceTimeTrack();
            ct.booleanAttributeTracks = getBooleanAttributeTracks(startTime, endTime);
            return ct;
        }

        public Dictionary<string, CompressedBooleanAttributeTrack> getBooleanAttributeTracks(DateTime l_startTime, DateTime l_endTime)
        {
            Dictionary<string, CompressedBooleanAttributeTrack> ret = new Dictionary<string, CompressedBooleanAttributeTrack>();
            CompressedBooleanAttributeTrack occlusionTrack = locationTrack.getOcclusionTrack(l_startTime, l_endTime);
            ret.Add("occlusion", occlusionTrack);
            CompressedBooleanAttributeTrack outofviewTrack = locationTrack.getOutOfViewTrack(l_startTime, l_endTime);
            ret.Add("outofview", outofviewTrack);
            foreach (KeyValuePair<string, VATIC_DVA_BooleanAttributeSequence> entry in booleanAttributeTracks)
            {
                VATIC_DVA_BooleanAttributeSequence vba = entry.Value;
                CompressedBooleanAttributeTrack cbat = vba.getCompressedBooleanAttributeTrack(l_startTime, l_endTime);
                ret.Add(cbat.attribute_name, cbat);
            }
            return ret;
        }
    }


    



    public class VATIC_DVA_CrowdsourcedResult
    {
        public List<VATIC_DVA_CompleteTrack> completeTrackList = new List<VATIC_DVA_CompleteTrack>();
        private DateTime start;
        private DateTime end;
        public DateTime startTime
        {
            get { return start; }
            set { start = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public DateTime endTime
        {
            get { return end; }
            set { end = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public string cameraId;
        public string UID;

        public VATIC_DVA_CrowdsourcedResult()
        {

        }

        public VATIC_DVA_CrowdsourcedResult(string TrackAnnotationString, string videoName, string l_UID, int totalFrameCount, int fps = 10)
        {
            startTime = DateTime.MinValue;
            double MilSecondsPerFrame = (double)1000 / (double)fps;
            endTime = startTime + DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)((totalFrameCount-1) * MilSecondsPerFrame));

            cameraId = videoName;
            UID = l_UID;
            string[] fields = TrackAnnotationString.Split('|');
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] == "")
                {
                    continue;
                }
                VATIC_DVA_CompleteTrack t = new VATIC_DVA_CompleteTrack(fields[i], cameraId, endTime);
                if (t.locationTrack.frames.Count == 0)
                {
                    continue;
                }
                //if (t.endTime > endTime)
                //{
                //    endTime = t.endTime;
                //}
                completeTrackList.Add(t);
            }
        }

        public VATIC_DVA_CrowdsourcedResult(DateTime start, string TrackAnnotationString, string videoName, string l_UID, int totalFrameCount, int fps = 10)
        {
            startTime = start;
            double MilSecondsPerFrame = (double)1000 / (double)fps;
            endTime = startTime + DateTimeUtilities.getTimeSpanFromTotalMilliSeconds((int)((totalFrameCount-1) * MilSecondsPerFrame));

            cameraId = videoName;
            UID = l_UID;
            string[] fields = TrackAnnotationString.Split('|');
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] == "")
                {
                    continue;
                }
                VATIC_DVA_CompleteTrack t = new VATIC_DVA_CompleteTrack(fields[i], cameraId, endTime);
                if (t.locationTrack.frames.Count == 0)
                {
                    continue;
                }
                //if (t.endTime > endTime)
                //{
                //    endTime = t.endTime;
                //}
                completeTrackList.Add(t);
            }
        }


        //public static VATIC_DVA_CrowdsourcedResult createVATIC_DVA_CrowdsourcedResultUsingSatyamBlobImageCount(string TrackAnnotationString, string DirectoryURL, string l_UID, int fps = 10)
        //{
        //    SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
        //    List<string> ImageURLs = satyamStorage.getURLListOfSubDirectoryByURL(DirectoryURL);
        //    int totalFrameCount = ImageURLs.Count;

        //    string videoName = URIUtilities.filenameFromURI(DirectoryURL);

        //    return new VATIC_DVA_CrowdsourcedResult(TrackAnnotationString, videoName, l_UID, totalFrameCount, fps);
        //}

        public List<CompressedTrack> getCompressedTracks()
        {
            List<CompressedTrack> ret = new List<CompressedTrack>();
            foreach (VATIC_DVA_CompleteTrack vdct in completeTrackList)
            {
                CompressedTrack ct = vdct.getCompressedTrack();
                ret.Add(ct);
            }
            return ret;
        }

        public MultiObjectTrackingResult getCompressedTracksInTimeSegment()
        {
            MultiObjectTrackingResult ctts = new MultiObjectTrackingResult(UID);
            ctts.cameraId = cameraId;
            ctts.VideoSegmentStartTime = startTime;
            ctts.VideoSegmentEndTime = endTime;
            foreach (VATIC_DVA_CompleteTrack vdct in completeTrackList)
            {
                CompressedTrack ct = vdct.getCompressedTrack();
                ctts.tracks.Add(ct);
            }
            return ctts;
        }

    }
    ///
    /// something else
    ///
    public class FrameTrack
    {
        public FrameSequence frame_sequence;
        public BooleanAttributeCollection frame_attributes;
        public string label;
        public double frames_per_second;
        public string id;

        public FrameTrack(double l_frames_per_second, string l_label, string l_id)
        {
            frames_per_second = l_frames_per_second;
            frame_sequence = new FrameSequence();
            frame_attributes = new BooleanAttributeCollection();
            id = l_id;
            label = l_label;
        }
    }
    public class FrameTrackCollection
    {
        public List<FrameTrack> frame_tracks;
        public string id;
        public double frames_per_second;

        public FrameTrackCollection(string l_id, double l_frames_per_second)
        {
            id = l_id;
            frames_per_second = l_frames_per_second;
            frame_tracks = new List<FrameTrack>();
        }
    }

    public class FrameSequence
    {
        public List<BoundingBox> track;

        public FrameSequence()
        {
            track = new List<BoundingBox>();
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < track.Count; i++)
            {
                if (i == track.Count - 1)
                {
                    s.Append(track[i]);
                }
                else
                {
                    s.Append(track[i] + "\t");
                }
            }
            return s.ToString();
        }
    }

    public class BooleanAttributeSequence
    {
        public List<bool> attribute_sequence;
        public string attribute_name;

        public BooleanAttributeSequence(string l_name)
        {
            attribute_sequence = new List<bool>();
            attribute_name = l_name;
        }
    }

    public class BooleanAttributeCollection
    {
        public Dictionary<string, BooleanAttributeSequence> attributes;

        public BooleanAttributeCollection()
        {
            attributes = new Dictionary<string, BooleanAttributeSequence>();
        }
    }

    /// <summary>
    /// compressed version
    /// </summary>

    public class MultiObjectTrackingResult
    {
        private DateTime start;
        private DateTime end;
        public DateTime VideoSegmentStartTime
        {
            get { return start; }
            set { start = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public DateTime VideoSegmentEndTime
        {
            get { return end; }
            set { end = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public string cameraId;
        public string UID;
        public List<CompressedTrack> tracks = new List<CompressedTrack>();

        public MultiObjectTrackingResult()
        {
            tracks = new List<CompressedTrack>();
        }

        public MultiObjectTrackingResult(string l_uid)
        {
            tracks = new List<CompressedTrack>();
            UID = l_uid;
        }

        public static MultiObjectTrackingResult ConvertAnnotationStringToMultiObjectTrackingResult(DateTime start, string TrackAnnotationString, string videoName, string l_UID, int totalFramesCount, int fps = 10)
        {
            VATIC_DVA_CrowdsourcedResult raw = new VATIC_DVA_CrowdsourcedResult(start, TrackAnnotationString, videoName, videoName, totalFramesCount, fps);
            MultiObjectTrackingResult ret = raw.getCompressedTracksInTimeSegment();
            return ret;
        }

        public void postpone(TimeSpan timeSpanToPostponeInSeconds)
        {
            VideoSegmentStartTime += timeSpanToPostponeInSeconds;
            VideoSegmentEndTime += timeSpanToPostponeInSeconds;
            foreach (CompressedTrack entity in tracks)
            {
                /// postpone the timestamp of the second trace
                entity.startTime += timeSpanToPostponeInSeconds;
                entity.endTime += timeSpanToPostponeInSeconds;
                entity.spaceTimeTrack.startTime += timeSpanToPostponeInSeconds;
                entity.spaceTimeTrack.endTime += timeSpanToPostponeInSeconds;

                //foreach (SpaceTime st in entity.spaceTimeTrack.space_time_track)
                for (int i = 0; i < entity.spaceTimeTrack.space_time_track.Count; i++)
                {
                    entity.spaceTimeTrack.space_time_track[i].time += timeSpanToPostponeInSeconds;
                }
                //foreach (CompressedBooleanAttributeTrack booleanAttributeTrack in entity.booleanAttributeTracks.Values)
                foreach (string key in entity.booleanAttributeTracks.Keys)
                {
                    entity.booleanAttributeTracks[key].startTime += timeSpanToPostponeInSeconds;
                    entity.booleanAttributeTracks[key].endTime += timeSpanToPostponeInSeconds;

                    //foreach (BooleanAttribute ba in entity.booleanAttributeTracks[key].attribute_track)
                    for (int i = 0; i < entity.booleanAttributeTracks[key].attribute_track.Count; i++)
                    {
                        entity.booleanAttributeTracks[key].attribute_track[i].time += timeSpanToPostponeInSeconds;
                    }
                }
            }
        }


        public MultiObjectTrackingResult getSubTimeSegment(DateTime start, DateTime end, int fps=10)
        {
            if (start > end) return null;
            List<string> filteredAnnotationStrings = new List<string>();
            foreach (CompressedTrack track in tracks)
            {
                string annotationString = track.unCompressToTrackAnnotationString();
                string[] fields = annotationString.Split(':');
                List<string> TimeFilteredSegmentStringList = new List<string>();
                TimeFilteredSegmentStringList.Add(fields[0]); // adds the label back
                // remove the label of the second string
                int count = 0;
                for (int j = 1; j < fields.Length; j++)
                {
                    // remove the overlapping part.
                    DateTime time = DateTimeUtilities.getDateTimeFromString(fields[j].Split(',')[0]);
                    if (time < start || time > end) continue;

                    TimeFilteredSegmentStringList.Add(fields[j]);
                    count++;
                }
                if (count == 0) continue;
                string filteredAnnotationString = ObjectsToStrings.ListString(TimeFilteredSegmentStringList, ':');
                filteredAnnotationStrings.Add(filteredAnnotationString);
            }
            string totalFilteredAnnotationString = ObjectsToStrings.ListString(filteredAnnotationStrings, '|');

            int totalFrames = (int)(end - start).TotalSeconds * fps + 1;// added the end frame

            MultiObjectTrackingResult ret = ConvertAnnotationStringToMultiObjectTrackingResult(start, totalFilteredAnnotationString, cameraId, UID, totalFrames, fps);
            return ret;
        }

        public FrameTrackCollection unCompressToFrameTrackCollection(double l_frames_per_second, string l_id)
        {
            FrameTrackCollection ftc = new FrameTrackCollection(l_id, l_frames_per_second);

            Dictionary<string, int> label_ctr = new Dictionary<string, int>();

            DateTime maxEndTime = VideoSegmentStartTime;
            foreach (CompressedTrack ct in tracks)
            {
                if (ct.endTime > maxEndTime)
                {
                    maxEndTime = ct.endTime;
                }
            }

            foreach (CompressedTrack ct in tracks)
            {

                if (!label_ctr.ContainsKey(ct.label))
                {
                    label_ctr.Add(ct.label, 0);
                }
                else
                {
                    label_ctr[ct.label]++;
                }

                FrameTrack f = new FrameTrack(l_frames_per_second, ct.label, (l_id + "_" + ct.label + "_" + label_ctr[ct.label]));

                List<string> attribute_list = ct.booleanAttributeTracks.Keys.ToList();
                foreach (string attribute in attribute_list)
                {
                    BooleanAttributeSequence fas = new BooleanAttributeSequence(attribute);
                    f.frame_attributes.attributes.Add(attribute, fas);
                }

                int cntr = 0;

                DateTime t = VideoSegmentStartTime;

                do
                {
                    SpaceTime st = ct.getSpaceTimeAt(t);
                    if (st != null)
                    {
                        BoundingBox b = new BoundingBox(st.region);
                        f.frame_sequence.track.Add(b);
                    }
                    else
                    {
                        BoundingBox b = new BoundingBox(BoundingBox.NullBoundingBox);
                        f.frame_sequence.track.Add(b);
                    }
                    foreach (string attribute in attribute_list)
                    {
                        BooleanAttribute attr = ct.getAttributeAt(attribute, t);
                        f.frame_attributes.attributes[attribute].attribute_sequence.Add(attr.value);
                    }

                    cntr++;

                    int next_offset_in_ms = (int)Math.Floor((cntr * 1000.0) / l_frames_per_second);
                    TimeSpan dt = DateTimeUtilities.getTimeSpanFromTotalMilliSeconds(next_offset_in_ms);
                    t = VideoSegmentStartTime + dt;

                } while (t <= maxEndTime);


                ftc.frame_tracks.Add(f);

            }

            return ftc;
        }

        public override string ToString()
        {
            string trackAnnotationString = "";
            for (int i = 0; i < tracks.Count; i++)
            {
                trackAnnotationString += tracks[i].unCompressToTrackAnnotationString();
                if (i == tracks.Count - 1) break;
                trackAnnotationString += "|";
            }
            return trackAnnotationString;
        }
    }



    

    public class CompressedTrack
    {
        private DateTime start;
        private DateTime end;
        public DateTime startTime
        {
            get { return start; }
            set { start = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public DateTime endTime
        {
            get { return end; }
            set { end = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public String label;
        public string cameraId;
        public string taskName;
        public string UID;
        public CompressedSpaceTimeTrack spaceTimeTrack = new CompressedSpaceTimeTrack();
        public Dictionary<string, CompressedBooleanAttributeTrack> booleanAttributeTracks = new Dictionary<string, CompressedBooleanAttributeTrack>();

        

        public VATIC_DVA_Frame getFrameAt(DateTime time)
        {
            SpaceTime st = getSpaceTimeAt(time);
            
            BooleanAttribute outofview = getAttributeAt("outofview", time);
            BooleanAttribute occlusion = getAttributeAt("occlusion", time);
            if (st == null || outofview == null || outofview.value == true ||  occlusion == null) return null;
            BoundingBox box = st.region;
            return new VATIC_DVA_Frame(time, box.tlx, box.tly, box.brx, box.bry, occlusion.value, outofview.value);
        }

        public SpaceTime getSpaceTimeAt(DateTime time)
        {
            CompressedSpaceTimeTrack t = spaceTimeTrack;
            if (time < t.startTime || time > t.endTime)
            {
                return null; //it does not exist
            }
            if (t.space_time_track.Count == 1) //there is only one location
            {
                SpaceTime l = new SpaceTime(t.space_time_track[0].region, time);
                return l;
            }
            for (int i = 1; i < t.space_time_track.Count; i++)
            {
                SpaceTime l = t.space_time_track[i];
                if (time == l.time)
                {
                    return new SpaceTime(l);
                }
                if (time < l.time)
                {
                    SpaceTime prevLocation = t.space_time_track[i - 1];
                    return SpaceTime.interpolate(prevLocation, l, time);
                }
                if (i == t.space_time_track.Count - 1)
                {
                    return new SpaceTime(t.space_time_track[i].region, time);
                }
            }
            return null;
        }
        public BooleanAttribute getAttributeAt(string attribute_name, DateTime t)
        {
            if (booleanAttributeTracks.ContainsKey(attribute_name))
            {
                CompressedBooleanAttributeTrack cbat = booleanAttributeTracks[attribute_name];
                return cbat.getAttributeAt(t);
            }
            return null;
        }

        //public FrameTrack unCompressToFrameTrack(string l_id, double frames_per_second)
        //{
        //    FrameTrack f = new FrameTrack(frames_per_second, label, l_id);

        //    List<string> attribute_list = booleanAttributeTracks.Keys.ToList();
        //    foreach (string attribute in attribute_list)
        //    {
        //        BooleanAttributeSequence fas = new BooleanAttributeSequence(attribute);
        //        f.frame_attributes.attributes.Add(attribute, fas);
        //    }

        //    int cntr = 0;

        //    DateTime t = startTime;

        //    do
        //    {
        //        SpaceTime st = getSpaceTimeAt(t);
        //        if (st != null)
        //        {
        //            BoundingBox b = new BoundingBox(st.region);
        //            f.frame_sequence.track.Add(b);
        //        }
        //        else
        //        {
        //            BoundingBox b = new BoundingBox(BoundingBox.NullBoundingBox);
        //            f.frame_sequence.track.Add(b);
        //        }
        //        foreach (string attribute in attribute_list)
        //        {
        //            BooleanAttribute attr = getAttributeAt(attribute, t);
        //            f.frame_attributes.attributes[attribute].attribute_sequence.Add(attr.value);
        //        }

        //        cntr++;

        //        int next_offset_in_ms = (int)Math.Floor((cntr * 1000.0) / frames_per_second);
        //        TimeSpan dt = DateTimeUtilities.getTimeSpanFromTotalMilliSeconds(next_offset_in_ms);
        //        t = startTime + dt;

        //    } while (t <= endTime);

        //    return f;
        //}
        public string unCompressToTrackAnnotationString()
        {
            string ret = "";
            ret += label + "_Location:";
            //foreach (SpaceTime st in spaceTimeTrack.space_time_track)
            for (int i = 0; i < spaceTimeTrack.space_time_track.Count; i++)
            {
                SpaceTime st = spaceTimeTrack.space_time_track[i];
                //ret += st.time.ToString("yyyy-MM-dd-HH-mm-ss-fff") + ",";
                ret += DateTimeUtilities.convertDateTimeToString(st.time) + ",";
                ret += st.region.tlx + "," + st.region.tly + "," + st.region.brx + "," + st.region.bry;
                //foreach (KeyValuePair<string, CompressedBooleanAttributeTrack> attr in booleanAttributeTracks)
                
                
                CompressedBooleanAttributeTrack occlusionTrack = booleanAttributeTracks["occlusion"];
                ret += ",";
                if (occlusionTrack.attribute_track[i].value)
                {
                    ret += "true";
                }
                else
                {
                    ret += "false";
                }
                CompressedBooleanAttributeTrack outofviewTrack = booleanAttributeTracks["outofview"];
                ret += ",";
                if (outofviewTrack.attribute_track[i].value)
                {
                    ret += "true";
                }
                else
                {
                    ret += "false";
                }
                if (i == spaceTimeTrack.space_time_track.Count - 1) break;
                ret += ":";
            }
            foreach (string attr in booleanAttributeTracks.Keys)
            {
                if (attr == "occlusion" || attr == "outofview") continue;
                ret += "_" + attr + ":";
                for (int i = 0; i < booleanAttributeTracks[attr].attribute_track.Count; i++)
                {
                    BooleanAttribute bt = booleanAttributeTracks[attr].attribute_track[i];
                    ret += DateTimeUtilities.convertDateTimeToString(bt.time) + ",";
                    if (bt.value)
                    {
                        ret += "true";
                    }
                    else
                    {
                        ret += "false";
                    }
                    if (i == booleanAttributeTracks[attr].attribute_track.Count - 1) break;
                    ret += ":";
                }
            }

            
            return ret;
        }

        /// <summary>
        /// assume  tck1 and tck2 are connecting in time and space and attribute, tck2 must be later than tck1
        /// </summary>
        /// <param name="tck1"></param>
        /// <param name="tck2"></param>
        /// <returns></returns>
        public static CompressedTrack stitchTwoAdjacentTrack(CompressedTrack tck1, CompressedTrack tck2)
        {

            tck1.endTime = tck2.endTime;
            tck1.spaceTimeTrack.endTime = tck2.spaceTimeTrack.endTime;
            for (int i = 0; i < tck2.spaceTimeTrack.space_time_track.Count; i++)
            {
                tck1.spaceTimeTrack.space_time_track.Add(tck2.spaceTimeTrack.space_time_track[i]);
            }
            
            foreach (string key in tck2.booleanAttributeTracks.Keys)
            {
                
                if (!tck1.booleanAttributeTracks.ContainsKey(key))
                {
                    tck1.booleanAttributeTracks.Add(key, tck2.booleanAttributeTracks[key]);
                    continue;
                }
                tck1.booleanAttributeTracks[key].endTime = tck2.booleanAttributeTracks[key].endTime;

                for (int j=0;j< tck2.booleanAttributeTracks[key].attribute_track.Count; j++)
                {
                    tck1.booleanAttributeTracks[key].attribute_track.Add(tck2.booleanAttributeTracks[key].attribute_track[j]);
                }
            }
            return tck1;
        }
    }

    public class CompressedTrackGroup
    {
        public CompressedTrack mergedCompressedTrack;
        public List<CompressedTrack> CompressedTrackList = new List<CompressedTrack>();
        public List<string> identifiers = new List<string>();
    }


    public class SpaceTime
    {
        private DateTime t;
        public DateTime time
        {
            get { return t; }
            set { t = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public BoundingBox region;
        public SpaceTime()
        {

        }

        public SpaceTime(DateTime l_t, int l_tlx, int l_tly, int l_brx, int l_bry)
        {
            time = new DateTime(l_t.Ticks);
            region = new BoundingBox(l_tlx, l_tly, l_brx, l_bry);
        }

        public SpaceTime(BoundingBox loc, DateTime t)
        {
            region = new BoundingBox(loc);
            time = new DateTime(t.Ticks);
        }

        public SpaceTime(SpaceTime loc)
        {
            region = new BoundingBox(loc.region);
            time = new DateTime(loc.time.Ticks);
        }

        public static SpaceTime interpolate(SpaceTime l1, SpaceTime l2, DateTime t)
        {

            if (l1.time == t)
            {
                return new SpaceTime(l1);
            }
            if (l2.time == t)
            {
                return new SpaceTime(l2);
            }

            DateTime t0 = l1.time;
            int tlx1 = l1.region.tlx;
            int tly1 = l1.region.tly;
            int brx1 = l1.region.brx;
            int bry1 = l1.region.bry;
            DateTime t1 = l2.time;
            int tlx2 = l2.region.tlx;
            int tly2 = l2.region.tly;
            int brx2 = l2.region.brx;
            int bry2 = l2.region.bry;

            if (t0 > t1)
            {
                t0 = l2.time;
                t1 = l1.time;
                tlx2 = l1.region.tlx;
                tly2 = l1.region.tly;
                brx2 = l1.region.brx;
                bry2 = l1.region.bry;
                tlx1 = l2.region.tlx;
                tly1 = l2.region.tly;
                brx1 = l2.region.brx;
                bry1 = l2.region.bry;
            }

            TimeSpan ts = t1 - t0;
            double totalMillisec = ts.TotalMilliseconds;
            TimeSpan dts = t - t0;
            double ratioMillisec = dts.TotalMilliseconds;
            double ratio = ratioMillisec / (totalMillisec);

            int tlx = (int)Math.Floor((double)tlx1 + (double)(tlx2 - tlx1) * ratio);
            int tly = (int)Math.Floor((double)tly1 + (double)(tly2 - tly1) * ratio);
            int brx = (int)Math.Floor((double)brx1 + (double)(brx2 - brx1) * ratio);
            int bry = (int)Math.Floor((double)bry1 + (double)(bry2 - bry1) * ratio);

            SpaceTime l = new SpaceTime(t, tlx, tly, brx, bry);
            return l;
        }
        public override string ToString()
        {
            return (time.Year + "-" + time.Month + "-" + time.Day + "-" + time.Hour + "-" + time.Minute + "-" + time.Second + "-" + time.Millisecond + "," + region.ToString());
        }
    }
    public class CompressedSpaceTimeTrack
    {
        public List<SpaceTime> space_time_track= new List<SpaceTime>();
        private DateTime start;
        private DateTime end;
        public DateTime startTime
        {
            get { return start; }
            set { start = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public DateTime endTime
        {
            get { return end; }
            set { end = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
    }

    public class BooleanAttribute
    {
        public bool value;
        private DateTime t;
        public DateTime time
        {
            get { return t; }
            set { t = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public BooleanAttribute()
        {

        }
        public BooleanAttribute(DateTime l_time, bool l_value)
        {
            time = l_time;
            value = l_value;
        }
    }
    public class CompressedBooleanAttributeTrack
    {

        public List<BooleanAttribute> attribute_track;
        public String attribute_name;
        private DateTime start;
        private DateTime end;
        public DateTime startTime
        {
            get { return start; }
            set { start = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public DateTime endTime
        {
            get { return end; }
            set { end = DateTime.SpecifyKind(value, DateTimeKind.Local); }
        }
        public CompressedBooleanAttributeTrack()
        {
            attribute_track = new List<BooleanAttribute>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(attribute_name);
            for (int i = 0; i < attribute_track.Count; i++)
            {
                string s = attribute_track[i].ToString();
                sb.Append(":" + s);
            }
            return sb.ToString();
        }

        public BooleanAttribute getAttributeAt(DateTime time)
        {
            if (time < startTime || time > endTime)
            {
                return null;
            }
            if (attribute_track.Count == 0)
            {
                return new BooleanAttribute(time, false);
            }

            for (int i = 0; i < attribute_track.Count; i++)
            {
                if (attribute_track[i].time == time)
                {
                    return attribute_track[i];
                }
                if (attribute_track[i].time > time)
                {
                    if (i == 0)
                    {
                        return new BooleanAttribute(time, false);
                    }
                    return attribute_track[i - 1];
                }
            }

            return attribute_track[attribute_track.Count - 1];
        }


        public BooleanAttribute getAttributeAt_DefaultNull(DateTime time)
        {
            if (time < startTime || time > endTime)
            {
                return null;
            }
            if (attribute_track.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < attribute_track.Count; i++)
            {
                if (attribute_track[i].time == time)
                {
                    return attribute_track[i];
                }
                if (attribute_track[i].time > time)
                {
                    if (i == 0)
                    {
                        return null;
                    }
                    return attribute_track[i - 1];
                }
            }

            return attribute_track[attribute_track.Count - 1];
        }
    }

    public static class Raw_VATIC_DVA_Crowdsourced_Track_Collection
    {
        public static String Raw_VATIC_DVA_Crowdsourced_Track_Collection_ToTrackStrings(string Raw_VATIC_DVA_Crowdsourced_String, List<DateTime> l_frameTimes)
        {
            String[] trackStrings = Raw_VATIC_DVA_Crowdsourced_String.Split('|');
            StringBuilder returnString = new StringBuilder();
            for (int i = 0; i < trackStrings.Length; i++)
            {
                string s = Raw_VATIC_DVA_Crowdsourced_Track_ToTrackString(trackStrings[i], l_frameTimes);
                if (i == 0)
                {
                    returnString.Append(s);
                }
                else
                {
                    returnString.Append("|" + s);
                }
            }
            return returnString.ToString();
        }

        public static String Raw_VATIC_DVA_Crowdsourced_Track_Collection_ToTrackStrings(string trackString, DateTime VideoSegmentStartTime, int fps)
        {
            int deltaT = (int)Math.Floor(1000.0 / (double)fps);
            TimeSpan dt = new TimeSpan(0, 0, 0, 0, deltaT);
            List<DateTime> FrameTimes = new List<DateTime>();
            FrameTimes.Add(VideoSegmentStartTime);
            DateTime t = VideoSegmentStartTime;
            for (int i = 1; i < 5000; i++)
            {
                t += dt;
                FrameTimes.Add(t);
            }
            return Raw_VATIC_DVA_Crowdsourced_Track_Collection_ToTrackStrings(trackString, FrameTimes);
        }


        public static string Raw_VATIC_DVA_Crowdsourced_Track_ToTrackString(string trackString, List<DateTime> l_frameTimes)
        {
            if (trackString == "") //if there was nothing labeled
            {
                return "";
            }
            StringBuilder returnString = new StringBuilder();
            string[] fields = trackString.Split('_');
            returnString.Append(fields[0]); //label
            string locationString = Raw_VATIC_DVA_Crowdsourced_Locations_To_TrackSubstring(fields[1], l_frameTimes);
            returnString.Append("_" + "Location" + locationString);
            int noAttributes = (fields.Length - 2) / 2;
            for (int i = 0; i < noAttributes; i++)
            {
                returnString.Append("_" + fields[2 + 2 * i]);  //name of the attribute
                string attributeString = Raw_VATIC_DVA_Crowdsourced_Attribute_To_TrackSubstring(fields[2 * i + 3], l_frameTimes);
                returnString.Append(attributeString);
            }
            return returnString.ToString();
        }

        public static string Raw_VATIC_DVA_Crowdsourced_Locations_To_TrackSubstring(string inputString, List<DateTime> l_frameTimes)
        {
            StringBuilder returnString = new StringBuilder();
            string[] fields = inputString.Split(':');

            for (int i = 0; i < fields.Length; i++)
            {
                string[] subfields = fields[i].Split(',');
                int frameNo = Convert.ToInt32(subfields[0]);

                if (frameNo >= l_frameTimes.Count) break;
                if (frameNo < 0) continue;

                DateTime t = l_frameTimes[frameNo];
                string timeString = Utilities.DateTimeUtilities.convertDateTimeToString(t);
                returnString.Append(":" + timeString);
                for (int j = 1; j < 5; j++)
                {
                    returnString.Append("," + (int)Math.Floor(Convert.ToDouble(subfields[j])));
                }
                returnString.Append("," + subfields[5]);
                returnString.Append("," + subfields[6]);
            }
            return returnString.ToString();
        }

        public static string Raw_VATIC_DVA_Crowdsourced_Attribute_To_TrackSubstring(string inputString, List<DateTime> l_frameTimes)
        {
            StringBuilder returnString = new StringBuilder();
            string[] fields = inputString.Split(':');
            for (int i = 0; i < fields.Length; i++)
            {
                string[] subfields = fields[i].Split(',');
                int frameNo = Convert.ToInt32(subfields[0]);
                DateTime t = l_frameTimes[frameNo];
                string timeString = Utilities.DateTimeUtilities.convertDateTimeToString(t);
                returnString.Append(":" + timeString + "," + subfields[1]);
            }
            return returnString.ToString();
        }
    }
}
