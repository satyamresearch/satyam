using HelperClasses;
using SatyamResultClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulAlgorithms
{
    public class TrackletsAssociation
    {
        public static List<MultipartiteWeightedMatch> AssociateTracklets(List<MultiObjectTrackingResult> compressedResults)
        {
            // multipartitie weighted matching
            // multipartite matching to cluster tracks
            CompressedTrackSimilarityMetric.ICompressedTrackSimilarityMetric metric = new CompressedTrackSimilarityMetric.TubeletIoU();
            MultipartiteWeightedMatching.GreedyMean mwmg = new MultipartiteWeightedMatching.GreedyMean();
            MultipartiteWeightTensor weightTensor = CompressedTrackSimilarityMetric.computeTrackSimilarityTensor(compressedResults, metric);
            List<MultipartiteWeightedMatch> association = mwmg.getMatching(weightTensor);
            return association;
        }
    }

    public class CompressedTrackSimilarityMetric
    {

        public interface ICompressedTrackSimilarityMetric
        {
            double getMetric(CompressedTrack t1, CompressedTrack t2, int fps);
            double getMetric(CompressedTrack t1, CompressedTrack t2);
        }

        public class OverlapIntegral : ICompressedTrackSimilarityMetric
        {

            public OverlapIntegral() { }
            public double getMetric(CompressedTrack t1, CompressedTrack t2, int fps)
            {
                TimeSpan dt = new TimeSpan(0, 0, 0, 0, (int)Math.Floor(1000.0 / fps));

                DateTime startTime1 = t1.startTime;
                DateTime startTime2 = t2.startTime;

                DateTime endTime1 = t1.endTime;
                DateTime endTime2 = t2.endTime;

                //is there any overlap at all?
                if (startTime1 > endTime2 || startTime2 > endTime1)
                {
                    return 0;
                }

                //find overlap times

                DateTime commonStartTime = startTime1;
                if (startTime2 > startTime1)
                {
                    commonStartTime = startTime2;
                }
                DateTime commonEndTime = endTime1;
                if (endTime2 < endTime1)
                {
                    commonEndTime = endTime2;
                }

                //now compute the integral

                double sum = 0;
                for (DateTime t = commonStartTime; t <= commonEndTime; t += dt)
                {
                    SpaceTime l1 = t1.getSpaceTimeAt(t);
                    SpaceTime l2 = t2.getSpaceTimeAt(t);
                    double overlap = BoundingBox.ComputeOverlapArea(l1.region, l2.region);
                    sum += (overlap * dt.Milliseconds) / 1000; //pixel-sec
                }
                return sum;
            }

            public double getMetric(CompressedTrack t1, CompressedTrack t2)
            {
                return getMetric(t1, t2, 20); //20 frames-per-sec is default
            }

        }


        public class SpaceFractionalOverlapIntegral : ICompressedTrackSimilarityMetric
        {
            public SpaceFractionalOverlapIntegral() { }
            public double getMetric(CompressedTrack t1, CompressedTrack t2, int fps)
            {
                TimeSpan dt = new TimeSpan(0, 0, 0, 0, (int)Math.Floor(1000.0 / fps));

                DateTime startTime1 = t1.startTime;
                DateTime startTime2 = t2.startTime;

                DateTime endTime1 = t1.endTime;
                DateTime endTime2 = t2.endTime;

                //is there any overlap at all?
                if (startTime1 > endTime2 || startTime2 > endTime1)
                {
                    return 0;
                }

                //find overlap times

                DateTime commonStartTime = startTime1;
                if (startTime2 > startTime1)
                {
                    commonStartTime = startTime2;
                }
                DateTime commonEndTime = endTime1;
                if (endTime2 < endTime1)
                {
                    commonEndTime = endTime2;
                }

                if (commonEndTime - commonStartTime < dt)
                {
                    return 0;
                }

                //now compute the integral

                double sum = 0;
                for (DateTime t = commonStartTime; t <= commonEndTime; t += dt)
                {
                    SpaceTime l1 = t1.getSpaceTimeAt(t);
                    SpaceTime l2 = t2.getSpaceTimeAt(t);
                    double overlap = BoundingBox.ComputeOverlapAreaFraction(l1.region, l2.region);
                    sum += (overlap * dt.Milliseconds) / 1000; //pixel-sec
                }
                return sum;
            }

            public double getMetric(CompressedTrack t1, CompressedTrack t2)
            {
                return getMetric(t1, t2, 20);
            }
        }


        public class SpaceTimeFractionalOverlapIntegral : ICompressedTrackSimilarityMetric
        {
            public SpaceTimeFractionalOverlapIntegral() { }
            public double getMetric(CompressedTrack t1, CompressedTrack t2, int fps)
            {
                TimeSpan dt = new TimeSpan(0, 0, 0, 0, (int)Math.Floor(1000.0 / fps));

                DateTime startTime1 = t1.startTime;
                DateTime startTime2 = t2.startTime;

                DateTime endTime1 = t1.endTime;
                DateTime endTime2 = t2.endTime;

                //is there any overlap at all?
                if (startTime1 > endTime2 || startTime2 > endTime1)
                {
                    return 0;
                }

                //find overlap times

                DateTime commonStartTime = startTime1;
                if (startTime2 > startTime1)
                {
                    commonStartTime = startTime2;
                }
                DateTime commonEndTime = endTime1;
                if (endTime2 < endTime1)
                {
                    commonEndTime = endTime2;
                }

                if (commonEndTime - commonStartTime < dt)
                {
                    return 0;
                }

                //now compute the integral

                double sum = 0;
                for (DateTime t = commonStartTime; t <= commonEndTime; t += dt)
                {
                    SpaceTime l1 = t1.getSpaceTimeAt(t);
                    SpaceTime l2 = t2.getSpaceTimeAt(t);
                    double overlap = BoundingBox.ComputeOverlapAreaFraction(l1.region, l2.region);
                    sum += (overlap * dt.Milliseconds); //pixel-sec
                }
                TimeSpan deltat1 = endTime1 - startTime1;
                TimeSpan deltat2 = endTime2 - startTime2;
                double max_ms = Math.Max((double)deltat1.TotalMilliseconds, (double)deltat2.TotalMilliseconds);
                return sum / max_ms;
            }

            public double getMetric(CompressedTrack t1, CompressedTrack t2)
            {
                return getMetric(t1, t2, 20);
            }
        }



        public class TubeletIoU : ICompressedTrackSimilarityMetric
        {
            public TubeletIoU() { }
            public double getMetric(CompressedTrack t1, CompressedTrack t2, int fps)
            {
                TimeSpan dt = new TimeSpan(0, 0, 0, 0, (int)Math.Floor(1000.0 / fps));

                DateTime startTime1 = t1.startTime;
                DateTime startTime2 = t2.startTime;

                DateTime endTime1 = t1.endTime;
                DateTime endTime2 = t2.endTime;

                //is there any overlap at all?
                if (startTime1 > endTime2 || startTime2 > endTime1)
                {
                    return 0;
                }

                //find overlap times

                DateTime commonStartTime = startTime1;
                if (startTime2 > startTime1)
                {
                    commonStartTime = startTime2;
                }
                DateTime commonEndTime = endTime1;
                if (endTime2 < endTime1)
                {
                    commonEndTime = endTime2;
                }

                if (commonEndTime - commonStartTime < dt)
                {
                    return 0;
                }

                //now compute the integral

                double IntersectionVolume = 0;
                for (DateTime t = commonStartTime; t <= commonEndTime; t += dt)
                {
                    SpaceTime l1 = t1.getSpaceTimeAt(t);
                    SpaceTime l2 = t2.getSpaceTimeAt(t);
                    double overlap = BoundingBox.ComputeOverlapArea(l1.region, l2.region);
                    IntersectionVolume += (overlap * dt.Milliseconds); 
                }

                double UnionVolume = 0;
                for (DateTime t = startTime1; t <= endTime1; t += dt)
                {
                    SpaceTime l1 = t1.getSpaceTimeAt(t);
                    UnionVolume += (l1.region.ComputeArea() * dt.Milliseconds);
                }
                for (DateTime t = startTime2; t <= endTime2; t += dt)
                {
                    SpaceTime l2 = t2.getSpaceTimeAt(t);
                    UnionVolume += (l2.region.ComputeArea() * dt.Milliseconds);
                }
                UnionVolume -= IntersectionVolume;
                
                return IntersectionVolume / UnionVolume;
            }

            public double getMetric(CompressedTrack t1, CompressedTrack t2)
            {
                return getMetric(t1, t2, 20);
            }
        }

        public static double[,] computeTrackSimilarityMatrix(List<CompressedTrack> trackList, ICompressedTrackSimilarityMetric metric)
        {
            double[,] ret = new double[trackList.Count, trackList.Count];
            for (int i = 0; i < trackList.Count; i++)
            {
                ret[i, i] = ((double)(trackList[i].endTime - trackList[i].startTime).TotalMilliseconds) / 1000;
                for (int j = i + 1; j < trackList.Count; j++)
                {
                    ret[i, j] = metric.getMetric(trackList[i], trackList[j]);
                    ret[j, i] = ret[i, j];
                }
            }
            return ret;
        }

        public static double[,] computeTrackSimilarityMatrix(MultiObjectTrackingResult cts1, MultiObjectTrackingResult cts2, ICompressedTrackSimilarityMetric metric)
        {
            double[,] ret = new double[cts1.tracks.Count, cts2.tracks.Count];
            for (int i = 0; i < cts1.tracks.Count; i++)
            {
                CompressedTrack t1 = cts1.tracks[i];
                for (int j = 0; j < cts2.tracks.Count; j++)
                {
                    CompressedTrack t2 = cts2.tracks[j];
                    ret[i, j] = metric.getMetric(t1, t2);
                }
            }
            return ret;
        }

        public static MultipartiteWeightTensor computeTrackSimilarityTensor(List<MultiObjectTrackingResult> cts_list, ICompressedTrackSimilarityMetric metric)
        {
            MultipartiteWeightTensor ret = new MultipartiteWeightTensor(cts_list.Count);
            for (int i = 0; i < cts_list.Count; i++)
            {
                ret.setNumPartitionElements(i, cts_list[i].tracks.Count);
            }
            for (int i = 0; i < cts_list.Count; i++)
            {
                double[,] sim = computeTrackSimilarityMatrix(cts_list[i], cts_list[i], metric);
                ret.setWeightMatrix(i, i, sim);

                for (int j = i + 1; j < cts_list.Count; j++)
                {
                    sim = computeTrackSimilarityMatrix(cts_list[i], cts_list[j], metric);
                    ret.setWeightMatrix(i, j, sim);
                    double[,] sim_t = MatrixOperations.Transpose(sim);
                    ret.setWeightMatrix(j, i, sim_t);
                }
            }
            return ret;
        }
    }

    public static class MatrixOperations
    {
        public static double[,] Transpose(double[,] mat)
        {
            int rows = mat.GetLength(0);
            int cols = mat.GetLength(1);
            double[,] ret = new double[cols, rows];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    ret[j, i] = mat[i, j];
                }
            }
            return ret;
        }
    }
}
