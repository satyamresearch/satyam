using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsefulAlgorithms
{
    public static class GenericObjectAssociation
    {
        public delegate double ComputeSimilarityFunction<T>(T obj1, T obj2);

        public static double[,] computeSimilarities<T>(List<T> objList1, List<T> objList2, ComputeSimilarityFunction<T> similarity)
        {
            double[,] mat = new double[objList1.Count, objList2.Count];

            for (int i = 0; i < objList1.Count; i++)
            {
                for (int j = 0; j < objList2.Count; j++)
                {
                    mat[i, j] = similarity(objList2[j], objList1[i]); //this is a symmetric measure
                }
            }
            return mat;
        }

        public static MultipartiteWeightTensor computeSimilarityTensor<T>(List<List<T>> objs, ComputeSimilarityFunction<T> similarity)
        {
            MultipartiteWeightTensor ret = new MultipartiteWeightTensor(objs.Count);
            for (int i = 0; i < ret.noParts; i++)
            {
                ret.setNumPartitionElements(i, objs[i].Count);
            }
            for (int i = 0; i < ret.noParts - 1; i++)
            {
                for (int j = i + 1; j < ret.noParts; j++)
                {
                    double[,] sim = computeSimilarities<T>(objs[i], objs[j],similarity);
                    ret.setWeightMatrix(i, j, sim);
                }
            }
            return ret;
        }


        public static List<MultipartiteWeightedMatch> computeAssociations<T>(List<List<T>> boundingBoxes, ComputeSimilarityFunction<T> similarity)
        {
            MultipartiteWeightTensor t = computeSimilarityTensor<T>(boundingBoxes, similarity);
            MultipartiteWeightedMatching.GreedyMean matching = new MultipartiteWeightedMatching.GreedyMean();
            List<MultipartiteWeightedMatch> ret = matching.getMatching(t);
            return ret;
        }
    }
}
