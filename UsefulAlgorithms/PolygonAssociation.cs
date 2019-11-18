using HelperClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulAlgorithms
{
    public class PolygonAssociation
    {
        public static double[,] computeSimilarities(List<GenericPolygon> polyList1, List<GenericPolygon> polyList2)
        {
            double[,] mat = new double[polyList1.Count, polyList2.Count];

            for (int i = 0; i < polyList1.Count; i++)
            {
                for (int j = 0; j < polyList2.Count; j++)
                {
                    mat[i, j] = BoundingBox.ComputeIntersectionOverUnion(polyList2[j].getBoundingBox(), polyList1[i].getBoundingBox()); //this is a symmetric measure
                }
            }
            return mat;
        }

        public static MultipartiteWeightTensor computeSimilarityTensor(List<List<GenericPolygon>> polygons)
        {
            MultipartiteWeightTensor ret = new MultipartiteWeightTensor(polygons.Count);
            for (int i = 0; i < ret.noParts; i++)
            {
                ret.setNumPartitionElements(i, polygons[i].Count);
            }
            for (int i = 0; i < ret.noParts - 1; i++)
            {
                for (int j = i + 1; j < ret.noParts; j++)
                {
                    double[,] sim = computeSimilarities(polygons[i], polygons[j]);
                    ret.setWeightMatrix(i, j, sim);
                }
            }
            return ret;
        }


        public static List<MultipartiteWeightedMatch> computeGenericPolygonAssociations(List<List<GenericPolygon>> polygons)
        {
            MultipartiteWeightTensor t = computeSimilarityTensor(polygons);
            MultipartiteWeightedMatching.GreedyMean matching = new MultipartiteWeightedMatching.GreedyMean();
            List<MultipartiteWeightedMatch> ret = matching.getMatching(t);
            return ret;
        }


        /// <summary>
        ///  for segments
        /// </summary>
        /// <param name="polyList1"></param>
        /// <param name="polyList2"></param>
        /// <returns></returns>
        public static double[,] computeSimilarities(List<Segment> polyList1, List<Segment> polyList2)
        {
            double[,] mat = new double[polyList1.Count, polyList2.Count];

            for (int i = 0; i < polyList1.Count; i++)
            {
                for (int j = 0; j < polyList2.Count; j++)
                {
                    mat[i, j] = Segment.computeIoU_PixelSweep(polyList2[j], polyList1[i]); //this is a symmetric measure
                }
            }
            return mat;
        }

        public static MultipartiteWeightTensor computeSimilarityTensor(List<List<Segment>> polygons)
        {
            MultipartiteWeightTensor ret = new MultipartiteWeightTensor(polygons.Count);
            for (int i = 0; i < ret.noParts; i++)
            {
                ret.setNumPartitionElements(i, polygons[i].Count);
            }
            for (int i = 0; i < ret.noParts - 1; i++)
            {
                for (int j = i + 1; j < ret.noParts; j++)
                {
                    double[,] sim = computeSimilarities(polygons[i], polygons[j]);
                    ret.setWeightMatrix(i, j, sim);
                }
            }
            return ret;
        }


        public static List<MultipartiteWeightedMatch> computeGenericPolygonAssociations(List<List<Segment>> polygons)
        {
            MultipartiteWeightTensor t = computeSimilarityTensor(polygons);
            MultipartiteWeightedMatching.GreedyMean matching = new MultipartiteWeightedMatching.GreedyMean();
            List<MultipartiteWeightedMatch> ret = matching.getMatching(t);
            return ret;
        }
    }
}
