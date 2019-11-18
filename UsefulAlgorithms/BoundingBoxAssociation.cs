using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HelperClasses;

namespace UsefulAlgorithms
{
    public static class BoundingBoxAssociation
    {
        public static double[,] computeSimilarities(List<BoundingBox> bbList1, List<BoundingBox> bbList2)
        {
            double[,] mat = new double[bbList1.Count, bbList2.Count];

            for (int i = 0; i < bbList1.Count; i++)
            {
                for (int j = 0; j < bbList2.Count; j++)
                {
                    //mat[i, j] = bbList1[i].ComputeOverlapAreaFraction(bbList2[j]);
                    //mat[i, j] = BoundingBox.ComputeOverlapAreaFraction(bbList2[j], bbList1[j]); //this is a symmetric measure
                    mat[i, j] = BoundingBox.ComputeIntersectionOverUnion(bbList2[j], bbList1[i]); //this is a symmetric measure
                }
            }
            return mat;
        }

        public static MultipartiteWeightTensor computeSimilarityTensor(List<List<BoundingBox>> boundingBoxes)
        {
            MultipartiteWeightTensor ret = new MultipartiteWeightTensor(boundingBoxes.Count);
            for (int i = 0; i < ret.noParts; i++)
            {
                ret.setNumPartitionElements(i, boundingBoxes[i].Count);
            }
            for (int i = 0; i < ret.noParts - 1; i++)
            {
                for (int j = i + 1; j < ret.noParts; j++)
                {
                    double[,] sim = computeSimilarities(boundingBoxes[i], boundingBoxes[j]);
                    ret.setWeightMatrix(i, j, sim);
                }
            }
            return ret;
        }


        public static List<MultipartiteWeightedMatch> computeBoundingBoxAssociations(List<List<BoundingBox>> boundingBoxes)
        {
            MultipartiteWeightTensor t = computeSimilarityTensor(boundingBoxes);
            MultipartiteWeightedMatching.GreedyMean matching = new MultipartiteWeightedMatching.GreedyMean();
            List<MultipartiteWeightedMatch> ret = matching.getMatching(t);
            return ret;
        }


    }

}
