using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//There is a set of sets {1,2,3} {4,5,6,7,8} {,9,10,11} 
//A mathcing tries to parition them in a way that each parition has at most one element from each set
// and all the elements in each set are most similar to each other
//eg {1,4},{2},{3,5,9}, {6,10},{7},{8,11}
//the similary tensor is provided as s(i,j) = matrix() as input
//i.e. similarlity matrtix between elements in set i and set j
//eg. s(0,1) = [3 x 5] matrix

//A match is a single paritition that lists the set index and elemtent index in elementList
//for example in the above example, that mathchings are {0-0 (1),1-0 (4)}, {0-1 (2)}, {0-2(3),1-1(5),2-0(9)} and so on

namespace UsefulAlgorithms
{
    public class MultipartiteWeightedMatch  //eg {0-1, 1-2, 2-0} means, element 1 from input set 0, element 2 from input set 1 and element 0 from input set 2 are similar and belong to the same parititioned set
    {
        public Dictionary<int, int> elementList; //Key inputSet Id, Value is index of the element within the set
        public Dictionary<string, double> weightMatrix; //the similarity matrix between the elements

        public MultipartiteWeightedMatch()
        {
            elementList = new Dictionary<int, int>();
            weightMatrix = new Dictionary<string, double>();
        }

        public void update(int partitionId, int node)
        {
            if (elementList.ContainsKey(partitionId))
            {
                elementList[partitionId] = node;
            }
            else
            {
                elementList.Add(partitionId, node);
            }
        }

        public void updateWeight(int partitionId1, int partitionId2, double value)
        {
            string key = partitionId1 + "_" + partitionId2;
            if (!weightMatrix.ContainsKey(key))
            {
                weightMatrix.Add(key, value);
            }
            else
            {
                weightMatrix[key] = value;
            }
        }

        public double getWeight(int partitionId1, int partitionId2)
        {
            string key = partitionId1 + "_" + partitionId2;
            if (weightMatrix.ContainsKey(key))
            {
                return weightMatrix[key];
            }
            else
            {
                return 0;
            }
        }
    }

    public class MultipartiteWeightTensor //set of similarity matrices between each of the sets
    {
        public int noParts; //number of sets
        Dictionary<string, double[,]> interPartWeightMatrices; //index is setID1_setID2, key is the similarity matrix
        int[] noElements; //no of elements in each set

        public MultipartiteWeightTensor(int l_numParts)
        {
            interPartWeightMatrices = new Dictionary<string, double[,]>();
            noParts = l_numParts;
            noElements = new int[noParts];
        }

        public double[,] getWeightMatrix(int index1, int index2)
        {
            string key = index1.ToString() + "_" + index2.ToString();
            if (interPartWeightMatrices.ContainsKey(key))
            {
                return interPartWeightMatrices[key];
            }
            return null;
        }

        public void setWeightMatrix(int index1, int index2, double[,] mat)
        {
            string key = index1.ToString() + "_" + index2.ToString();
            if (!interPartWeightMatrices.ContainsKey(key))
            {
                interPartWeightMatrices.Add(key, mat);
            }
            else
            {
                interPartWeightMatrices[key] = mat;
            }
        }

        public double getWeight(int index1, int node1, int index2, int node2)
        {
            double[,] mat = getWeightMatrix(index1, index2);
            if (mat != null)
            {
                return mat[node1, node2];
            }
            else
            {
                return 0;
            }
        }

        public int getNumPartitionElements(int partitionNum)
        {
            return noElements[partitionNum];
        }

        public void setNumPartitionElements(int partitionNum, int num)
        {
            noElements[partitionNum] = num;
        }
    }

    public interface IMultipartiteWeightedMatching
    {
        List<MultipartiteWeightedMatch> getMatching(MultipartiteWeightTensor similarityTensor);
    }

    public static class MultipartiteWeightedMatching
    {
        public class GreedyMean : IMultipartiteWeightedMatching
        {

            //the similarity between two sets is the average similarity between all their elements
            double computeInterParitionWeight(Dictionary<int, int> set1, Dictionary<int, int> set2, MultipartiteWeightTensor weightTensor)
            {
                double sum = 0;
                int no = 0;
                foreach (KeyValuePair<int, int> entry1 in set1)
                {
                    int part1 = entry1.Key;
                    int node1 = entry1.Value;
                    foreach (KeyValuePair<int, int> entry2 in set2)
                    {
                        int part2 = entry2.Key;
                        int node2 = entry2.Value;
                        double weight = weightTensor.getWeight(part1, node1, part2, node2);
                        sum += weight;
                        no++;
                    }
                }
                if (no > 0)
                {
                    sum = sum / no;
                }
                return sum;
            }

            //for two partitions to be mergable, they cannot have elements from the same set since at most
            //one element from each set can go into a parition
            bool setsAreMergable(Dictionary<int, int> set1, Dictionary<int, int> set2)
            {
                bool mergable = true;
                foreach (KeyValuePair<int, int> entry in set1)
                {
                    if (set2.ContainsKey(entry.Key)) //if there is a common parition they are not mertgable
                    {
                        mergable = false;
                        break;
                    }
                }
                return mergable;
            }

            public List<MultipartiteWeightedMatch> getMatching(MultipartiteWeightTensor weightTensor)
            {
                List<MultipartiteWeightedMatch> match_list = new List<MultipartiteWeightedMatch>();

                //each dictionary is a set to be merged heirchically
                List<Dictionary<int, int>> sets = new List<Dictionary<int, int>>(); //this is the paritition

                //initially all tracks are individually kept in the match list
                //e.g. {0-0},{0-1},{0-2},{1-0},{1-1},{1-2},....,{1-4},{2-0},...
                //each element is a set in the paritition
                for (int i = 0; i < weightTensor.noParts; i++)
                {
                    for (int j = 0; j < weightTensor.getNumPartitionElements(i); j++)
                    {
                        Dictionary<int, int> set = new Dictionary<int, int>();
                        set.Add(i, j); 
                        sets.Add(set);
                    }
                }


                //now we heirchically merge two most similar sets
                //until there is no longer any mergable subset

                bool merged = false;
                int iterations = 0;
                do
                {
                    merged = false;
                    int set1Index = -1;
                    int set2Index = -1;
                    double maxSimilarlity = 0;
                    for (int i = 0; i < sets.Count - 1; i++)
                    {
                        Dictionary<int, int> set1 = sets[i];
                        for (int j = i + 1; j < sets.Count; j++)
                        {
                            Dictionary<int, int> set2 = sets[j]; 
                            if (!setsAreMergable(set1, set2)) //check if the sets are mergable (they do not have an input set in common) e.g. {1-1} and {1-2} cannot be merged since they are both from 1.
                            {
                                continue;
                            }
                            double sim = computeInterParitionWeight(set1, set2, weightTensor);
                            if (sim > maxSimilarlity)
                            {

                                set1Index = i;
                                set2Index = j;
                                maxSimilarlity = sim;
                            }
                        }
                    }
                    if (maxSimilarlity > 0) //if two mergable sets were found then merge them!
                    {
                        Dictionary<int, int> set1 = sets[set1Index];
                        Dictionary<int, int> set2 = sets[set2Index];
                        //first merge set2 into set1
                        foreach (KeyValuePair<int, int> entry in set2)
                        {
                            set1.Add(entry.Key, entry.Value);
                        }
                        sets.Remove(set2);
                        merged = true;
                    }
                    iterations++;
                } while (merged);

                foreach (Dictionary<int, int> set in sets)
                {
                    MultipartiteWeightedMatch m = new MultipartiteWeightedMatch();
                    List<int> partitionList = set.Keys.ToList();
                    partitionList.Sort();

                    foreach (int part in partitionList)
                    {
                        m.update(part, set[part]);
                    }



                    for (int i = 0; i < partitionList.Count - 1; i++)
                    {
                        for (int j = i + 1; j < partitionList.Count; j++)
                        {
                            double val = weightTensor.getWeight(partitionList[i], m.elementList[partitionList[i]], partitionList[j], m.elementList[partitionList[j]]);
                            m.updateWeight(partitionList[i], partitionList[j], val);
                        }
                    }
                    match_list.Add(m);
                }
                return match_list;
            }

            public List<MultipartiteWeightedMatch> getMatching(MultipartiteWeightTensor weightTensor, double MergeLowerBound)
            {
                List<MultipartiteWeightedMatch> match_list = new List<MultipartiteWeightedMatch>();

                //each dictionary is a set to be merged heirchically
                List<Dictionary<int, int>> sets = new List<Dictionary<int, int>>();

                //initially all tracks are individually kept in the match list
                for (int i = 0; i < weightTensor.noParts; i++)
                {
                    for (int j = 0; j < weightTensor.getNumPartitionElements(i); j++)
                    {
                        Dictionary<int, int> set = new Dictionary<int, int>();
                        set.Add(i, j);
                        sets.Add(set);
                    }
                }

                //now we heirchically merge two most similar sets
                //until there is no longer any mergable subset

                bool merged = false;
                int iterations = 0;
                do
                {
                    merged = false;
                    int set1Index = -1;
                    int set2Index = -1;
                    double maxSimilarlity = 0;
                    for (int i = 0; i < sets.Count - 1; i++)
                    {
                        /*                       if(i==5 && iterations==1)
                                               {
                                                   Console.WriteLine("Here");
                                               }*/
                        Dictionary<int, int> set1 = sets[i];
                        for (int j = i + 1; j < sets.Count; j++)
                        {
                            Dictionary<int, int> set2 = sets[j];
                            if (!setsAreMergable(set1, set2))
                            {
                                continue;
                            }
                            double sim = computeInterParitionWeight(set1, set2, weightTensor);
                            if (sim > maxSimilarlity)
                            {

                                set1Index = i;
                                set2Index = j;
                                maxSimilarlity = sim;
                            }
                        }
                    }
                    if (maxSimilarlity > MergeLowerBound)
                    {
                        Dictionary<int, int> set1 = sets[set1Index];
                        Dictionary<int, int> set2 = sets[set2Index];
                        //first merge set2 into set1
                        foreach (KeyValuePair<int, int> entry in set2)
                        {
                            set1.Add(entry.Key, entry.Value);
                        }
                        sets.Remove(set2);
                        merged = true;
                    }
                    iterations++;
                } while (merged);

                foreach (Dictionary<int, int> set in sets)
                {
                    MultipartiteWeightedMatch m = new MultipartiteWeightedMatch();
                    List<int> partitionList = set.Keys.ToList();
                    partitionList.Sort();

                    foreach (int part in partitionList)
                    {
                        m.update(part, set[part]);
                    }



                    for (int i = 0; i < partitionList.Count - 1; i++)
                    {
                        for (int j = i + 1; j < partitionList.Count; j++)
                        {
                            double val = weightTensor.getWeight(partitionList[i], m.elementList[partitionList[i]], partitionList[j], m.elementList[partitionList[j]]);
                            m.updateWeight(partitionList[i], partitionList[j], val);
                        }
                    }
                    match_list.Add(m);
                }
                return match_list;
            }

        }
    }
}
