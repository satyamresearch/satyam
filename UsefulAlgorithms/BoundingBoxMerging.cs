using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HelperClasses;

namespace UsefulAlgorithms
{
    public class BoundingBoxGroup
    {
        public BoundingBox mergedBoundingBox;
        public List<BoundingBox> boundingBoxList = new List<BoundingBox>();
        public List<string> identifiers = new List<string>();
    }

    public static class MergeAndGroupBoundingBoxes
    {

        public static BoundingBox computeMeanBoudingBoxFromTwoGroups(List<BoundingBox> bgroup1, List<BoundingBox> bgroup2)
        {
            BoundingBox meanB = new BoundingBox(bgroup1[0].tlx, bgroup1[0].tly, bgroup1[0].brx, bgroup1[0].bry);
            int no = 1;
            for (int i = 1; i < bgroup1.Count; i++)
            {
                meanB.tlx += bgroup1[i].tlx;
                meanB.tly += bgroup1[i].tly;
                meanB.brx += bgroup1[i].brx;
                meanB.bry += bgroup1[i].bry;
                no++;
            }
            for (int i = 0; i < bgroup2.Count; i++)
            {
                meanB.tlx += bgroup2[i].tlx;
                meanB.tly += bgroup2[i].tly;
                meanB.brx += bgroup2[i].brx;
                meanB.bry += bgroup2[i].bry;
                no++;
            }

            meanB.tlx = meanB.tlx / no;
            meanB.tly = meanB.tly / no;
            meanB.brx = meanB.brx / no;
            meanB.bry = meanB.bry / no;

            return meanB;
        }

        public static List<BoundingBoxGroup> GreedyMeanHierarchicalMergeByPixelDeviation(List<BoundingBox> originalBoundingBoxes, List<string> originalIdentifiers, double deviationToleranceThreshold)
        {

            List<BoundingBoxGroup> ret = new List<BoundingBoxGroup>();

            //initialize by creating one group per bounding box
            for (int i = 0; i < originalBoundingBoxes.Count; i++)
            {
                BoundingBox b = originalBoundingBoxes[i];
                BoundingBoxGroup bgroup = new BoundingBoxGroup();
                bgroup.mergedBoundingBox = new BoundingBox(b.tlx, b.tly, b.brx, b.bry);
                bgroup.boundingBoxList.Add(b);
                bgroup.identifiers.Add(originalIdentifiers[i]);
                ret.Add(bgroup);
            }

            bool changed = false;

            do
            {
                changed = false;
                //find the closest mergable bouding box group pair and merge by averaging
                int index1 = -1;
                int index2 = -1;
                //double bestSimilarity = 0;
                double bestDeviation = 10000000000;
                BoundingBox bestMergedBoundingBox = null;
                for (int i = 0; i < ret.Count - 1; i++)
                {
                    for (int j = i + 1; j < ret.Count; j++)
                    {
                        //compute the merged bouding box
                        BoundingBox meanB = computeMeanBoudingBoxFromTwoGroups(ret[i].boundingBoxList, ret[j].boundingBoxList);
                        double deviation1 = meanB.ComputeMaxDeviationMetric(ret[i].mergedBoundingBox);
                        double deviation2 = meanB.ComputeMaxDeviationMetric(ret[j].mergedBoundingBox);
                        double maxDeviation = Math.Max(deviation1, deviation2);
                        if (maxDeviation <= deviationToleranceThreshold)
                        {
                            if (maxDeviation < bestDeviation)
                            {
                                bestDeviation = maxDeviation;
                                index1 = i;
                                index2 = j;
                                bestMergedBoundingBox = meanB;
                                changed = true;
                            }
                        }
                    }
                }

                if (changed) //there is something worth merging
                {
                    //merge 2 into 1
                    for (int i = 0; i < ret[index2].boundingBoxList.Count; i++)
                    {
                        ret[index1].boundingBoxList.Add(ret[index2].boundingBoxList[i]);
                        ret[index1].identifiers.Add(ret[index2].identifiers[i]);
                    }
                    ret[index1].mergedBoundingBox = bestMergedBoundingBox;

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


        public static List<BoundingBoxGroup> GreedyMeanHierarchicalMergeByPixelDeviation(List<BoundingBox> originalBoundingBoxes, List<string> originalIdentifiers, double deviationToleranceThresholdX, double deviationToleranceThresholdY)
        {

            List<BoundingBoxGroup> ret = new List<BoundingBoxGroup>();

            //initialize by creating one group per bounding box
            for (int i = 0; i < originalBoundingBoxes.Count; i++)
            {
                BoundingBox b = originalBoundingBoxes[i];
                BoundingBoxGroup bgroup = new BoundingBoxGroup();
                bgroup.mergedBoundingBox = new BoundingBox(b.tlx, b.tly, b.brx, b.bry);
                bgroup.boundingBoxList.Add(b);
                bgroup.identifiers.Add(originalIdentifiers[i]);
                ret.Add(bgroup);
            }

            bool changed = false;

            do
            {
                changed = false;
                //find the closest mergable bouding box group pair and merge by averaging
                int index1 = -1;
                int index2 = -1;
                //double bestSimilarity = 0;
                double bestDeviation = 10000000000;
                BoundingBox bestMergedBoundingBox = null;
                for (int i = 0; i < ret.Count - 1; i++)
                {
                    for (int j = i + 1; j < ret.Count; j++)
                    {
                        //compute the merged bouding box
                        BoundingBox meanB = computeMeanBoudingBoxFromTwoGroups(ret[i].boundingBoxList, ret[j].boundingBoxList);
                        double deviation1 = meanB.ComputeNormalizedMaxDeviationMetric(ret[i].mergedBoundingBox,deviationToleranceThresholdX,deviationToleranceThresholdY);
                        double deviation2 = meanB.ComputeNormalizedMaxDeviationMetric(ret[j].mergedBoundingBox, deviationToleranceThresholdX, deviationToleranceThresholdY);
                        double maxDeviation = Math.Max(deviation1, deviation2);
                        if (maxDeviation <= 1)
                        {
                            if (maxDeviation < bestDeviation)
                            {
                                bestDeviation = maxDeviation;
                                index1 = i;
                                index2 = j;
                                bestMergedBoundingBox = meanB;
                                changed = true;
                            }
                        }
                    }
                }

                if (changed) //there is something worth merging
                {
                    //merge 2 into 1
                    for (int i = 0; i < ret[index2].boundingBoxList.Count; i++)
                    {
                        ret[index1].boundingBoxList.Add(ret[index2].boundingBoxList[i]);
                        ret[index1].identifiers.Add(ret[index2].identifiers[i]);
                    }
                    ret[index1].mergedBoundingBox = bestMergedBoundingBox;

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


    }
}
