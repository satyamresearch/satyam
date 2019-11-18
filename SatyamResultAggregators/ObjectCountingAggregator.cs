using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using SatyamTaskResultClasses;
using SQLTables;
using Utilities;

namespace SatyamResultAggregators
{
    public class ObjectCountingAggregatedResultMetaData
    {
        public int TotalCount;
        public Dictionary<string, int> CountsHistogram;
    }

    public class ObjectCountingAggregatedResult
    {
        public double Count;
        public ObjectCountingAggregatedResultMetaData metaData;
    }

    
    public static class ObjectCountingAggregator
    {
        public static void getMaxGroup(Dictionary<double, int> clusteredCounts, out int maxCount, out int maxGroupIndex)
        {
            List<double> mergedCounts = clusteredCounts.Keys.ToList();
            maxCount = clusteredCounts[mergedCounts[0]];
            maxGroupIndex = 0;
            for (int i = 1; i < clusteredCounts.Count; i++)
            {
                if (maxCount < clusteredCounts[mergedCounts[i]])
                {
                    maxCount = clusteredCounts[mergedCounts[i]];
                    maxGroupIndex = i;
                }
            }

        }

        //getMaxWeightedGroupByWorkerStatistics
        public static ObjectCountingAggregatedResult getAggregatedResultUsingWorkerStatistics(List<SatyamResult> SatyamResults,
            int MinResults = TaskConstants.OBJECT_COUNTING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.OBJECT_COUNTING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND = TaskConstants.OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND,
            double MAX_DEVIATION_FRACTION = TaskConstants.OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION,
            double SUPER_MAJORITY_VALUE = TaskConstants.OBJECT_COUNTING_MTURK_SUPER_MAJORITY_VALUE)
        {
            if (SatyamResults.Count < MinResults)
            {
                return null;
            }

            Dictionary<int, List<SatyamResult>> resultsCount = new Dictionary<int, List<SatyamResult>>();
            Dictionary<double, List<SatyamResult>> clusteredCounts = ClusterCountsSatyamResultByMaxDeviation(SatyamResults, MAX_DEVIATION_FRACTION, MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND, out resultsCount);

            
            List<double> mergedCounts = clusteredCounts.Keys.ToList();
            double finalCount = -1;
            int maxCount = 0;
            //now check if there is consensus by super majority in the merged values
            if (clusteredCounts.Count == 1) //if there is only one cluster we are done!!
            {
                finalCount = mergedCounts[0];
                maxCount = clusteredCounts[finalCount].Count;
            }
            else
            {
                ////find the one with the largest worker success rate, if one of the group has no statistics, just fall back to use maxCounts
                maxCount = -1;
                int index = 0;
                double maxSuccessRate = -1;
                int maxSuccRateIndex = 0;
                WorkerStatisticsAccess wsa = new WorkerStatisticsAccess();
                bool useWorkerStatistics = true;
                for (int i = 0; i < clusteredCounts.Count; i++)
                {
                    // using count
                    if (maxCount < clusteredCounts[mergedCounts[i]].Count)
                    {
                        maxCount = clusteredCounts[mergedCounts[i]].Count;
                        index = i;
                    }

                    // using succ rate
                    //if (!useWorkerStatistics) continue;

                    int ResultHasWorkerStatistics = 0;
                    double successRate = 1;
                    for (int j = 0; j < clusteredCounts[mergedCounts[i]].Count; j++)
                    {
                        WorkerStatisticsTableEntry stats =  wsa.getWorkerStatistics(clusteredCounts[mergedCounts[i]][j].amazonInfo.WorkerID, TaskConstants.Counting_Image_MTurk);
                        if (stats!=null)
                        {
                            successRate *= (1-stats.SuccessFraction);
                        }
                        else
                        {
                            successRate *= 0.93;
                        }
                        ResultHasWorkerStatistics++;
                    }
                    
                    //if (ResultHasWorkerStatistics == 0)
                    //{
                    //    useWorkerStatistics = false;
                    //    continue;
                    //}

                    //successRate = Math.Pow(successRate, 1/(double)ResultHasWorkerStatistics);
                    successRate = 1 - successRate;

                    if (maxSuccessRate < successRate)
                    {
                        maxSuccessRate = successRate;
                        maxSuccRateIndex = i;
                    }

                }

                wsa.close();


                if (useWorkerStatistics)
                {
                    finalCount = mergedCounts[maxSuccRateIndex];
                    
                }
                else
                {
                    finalCount = mergedCounts[index];
                    Console.WriteLine("Not using statistics");
                }
            }

            if (maxCount < SatyamResults.Count * SUPER_MAJORITY_VALUE && SatyamResults.Count < MaxResults) //there was no consensus
            {
                return null;
            }

            ObjectCountingAggregatedResult aggresult = new ObjectCountingAggregatedResult();

            ObjectCountingAggregatedResultMetaData meta = new ObjectCountingAggregatedResultMetaData();
            meta.TotalCount = SatyamResults.Count;
            meta.CountsHistogram = new Dictionary<string, int>();
            foreach (KeyValuePair<int, List<SatyamResult>> entry in resultsCount)
            {
                meta.CountsHistogram.Add(entry.Key.ToString(), entry.Value.Count);
            }

            aggresult.Count = finalCount;
            aggresult.metaData = meta;
            return aggresult;
        }


        public static ObjectCountingAggregatedResult getAggregatedResult(List<ObjectCountingResult> results,
            int MinResults = TaskConstants.OBJECT_COUNTING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.OBJECT_COUNTING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND = TaskConstants.OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND,
            double MAX_DEVIATION_FRACTION = TaskConstants.OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION,
            double SUPER_MAJORITY_VALUE = TaskConstants.OBJECT_COUNTING_MTURK_SUPER_MAJORITY_VALUE)
        {
            if (results.Count < MinResults)
            {
                return null;
            }
            

            Dictionary<int, int> resultCounts = new Dictionary<int, int>();



            foreach (ObjectCountingResult result in results)
            {
                if (!resultCounts.ContainsKey(result.Count))
                {
                    resultCounts.Add(result.Count, 0);
                }
                resultCounts[result.Count]++;
            }

            //First hierachically cluster the counts

            Dictionary<double, int> clusteredCounts = ClusterCountsByMaxDeviation(resultCounts, MAX_DEVIATION_FRACTION, MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND);
            List<double> mergedCounts = clusteredCounts.Keys.ToList();
            double finalCount = -1;
            int maxCount = 0;
           //now check if there is consensus by super majority in the merged values
           if(clusteredCounts.Count == 1) //if there is only one cluster we are done!!
           {
                finalCount = mergedCounts[0];
                maxCount = clusteredCounts[finalCount];
           }
           else
           {
                ////find the one with the largest value
                //maxCount = clusteredCounts[mergedCounts[0]];
                int index = 0;
                //for(int i=1;i<clusteredCounts.Count;i++)
                //{
                //    if(maxCount < clusteredCounts[mergedCounts[i]])
                //    {
                //        maxCount = clusteredCounts[mergedCounts[i]];
                //        index = i;
                //    }
                //}

                getMaxGroup(clusteredCounts, out maxCount, out index);
                finalCount = mergedCounts[index];
           }
           
           if(maxCount < results.Count * SUPER_MAJORITY_VALUE && results.Count < MaxResults) //there was no consensus
           {
                return null;
           }

            ObjectCountingAggregatedResult aggresult = new ObjectCountingAggregatedResult();

            ObjectCountingAggregatedResultMetaData meta = new ObjectCountingAggregatedResultMetaData();
            meta.TotalCount = results.Count;
            meta.CountsHistogram = new Dictionary<string, int>();
            foreach(KeyValuePair<int,int> entry in resultCounts)
            {
                meta.CountsHistogram.Add(entry.Key.ToString(),entry.Value);
            }

            aggresult.Count = finalCount;
            aggresult.metaData = meta;
            return aggresult;
        }

        public static string GetAggregatedResultString(List<SatyamResultsTableEntry> results,
            int MinResults = TaskConstants.OBJECT_COUNTING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.OBJECT_COUNTING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND = TaskConstants.OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND,
            double MAX_DEVIATION_FRACTION = TaskConstants.OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION,
            double SUPER_MAJORITY_VALUE = TaskConstants.OBJECT_COUNTING_MTURK_SUPER_MAJORITY_VALUE)
        {
            Console.WriteLine("Aggregating");
            string resultString = null;
            List<ObjectCountingResult> resultList = new List<ObjectCountingResult>();
            List<SatyamResult> SatyamResultList = new List<SatyamResult>();
            foreach (SatyamResultsTableEntry entry in results)
            {
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamResultList.Add(res);
                ObjectCountingResult taskr = JSonUtils.ConvertJSonToObject<ObjectCountingResult>(res.TaskResult);
                resultList.Add(taskr);
            }
            //ObjectCountingAggregatedResult r = getAggregatedResult(resultList, MinResults, MaxResults, MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND, MAX_DEVIATION_FRACTION, SUPER_MAJORITY_VALUE);
            ObjectCountingAggregatedResult r = getAggregatedResultUsingWorkerStatistics(SatyamResultList, MinResults, MaxResults, MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND, MAX_DEVIATION_FRACTION, SUPER_MAJORITY_VALUE);
            if (r != null)
            {
                string rString = JSonUtils.ConvertObjectToJSon<ObjectCountingAggregatedResult>(r);
                SatyamAggregatedResult aggResult = new SatyamAggregatedResult();
                aggResult.SatyamTaskTableEntryID = results[0].SatyamTaskTableEntryID;
                aggResult.AggregatedResultString = rString;
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
                aggResult.TaskParameters = res.TaskParametersString;
                resultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(aggResult);
            }
            return resultString;
        }

        //public static string GetAggregatedResultString(List<SatyamResultsTableEntry> results,
        //    int MinResults = TaskConstants.OBJECT_COUNTING_MTURK_MIN_RESULTS_TO_AGGREGATE,
        //    int MaxResults = TaskConstants.OBJECT_COUNTING_MTURK_MAX_RESULTS_TO_AGGREGATE,
        //    double MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND = TaskConstants.OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND,
        //    double MAX_DEVIATION_FRACTION = TaskConstants.OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION,
        //    double SUPER_MAJORITY_VALUE = TaskConstants.OBJECT_COUNTING_MTURK_SUPER_MAJORITY_VALUE)
        //{
        //    string resultString = null;
        //    List<ObjectCountingResult> resultList = new List<ObjectCountingResult>();
        //    foreach (SatyamResultsTableEntry entry in results)
        //    {
        //        SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
        //        ObjectCountingResult taskr = JSonUtils.ConvertJSonToObject<ObjectCountingResult>(res.TaskResult);
        //        resultList.Add(taskr);
        //    }
        //    ObjectCountingAggregatedResult r = getAggregatedResult(resultList, MinResults, MaxResults, MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND, MAX_DEVIATION_FRACTION, SUPER_MAJORITY_VALUE);
        //    if (r != null)
        //    {
        //        string rString = JSonUtils.ConvertObjectToJSon<ObjectCountingAggregatedResult>(r);
        //        SatyamAggregatedResult aggResult = new SatyamAggregatedResult();
        //        aggResult.SatyamTaskTableEntryID = results[0].SatyamTaskTableEntryID;
        //        aggResult.AggregatedResultString = rString;
        //        SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
        //        aggResult.TaskParameters = res.TaskParametersString;
        //        resultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(aggResult);
        //    }
        //    return resultString;
        //}


        public static bool IsAcceptable(SatyamAggregatedResultsTableEntry aggResult, SatyamResultsTableEntry result,
            double MAX_ABSOLUTE_COUNT_DEVIATION = TaskConstants.OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_FOR_PAYMENT,
            double MAX_DEVIATION_FRACTION = TaskConstants.OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION_FOR_PAYMENT)
        {
            SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(result.ResultString);
            SatyamAggregatedResult aggres = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(aggResult.ResultString);

            ObjectCountingResult r = JSonUtils.ConvertJSonToObject<ObjectCountingResult>(res.TaskResult);
            ObjectCountingAggregatedResult ragg = JSonUtils.ConvertJSonToObject<ObjectCountingAggregatedResult>(aggres.AggregatedResultString);

            double maxdev = MAX_ABSOLUTE_COUNT_DEVIATION;
            if (ragg.Count * MAX_DEVIATION_FRACTION > maxdev)
            {
                maxdev = ragg.Count * MAX_DEVIATION_FRACTION;
            }

            if (Math.Abs(ragg.Count-r.Count) <= maxdev)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //hierchically clusters all the counts
        private static Dictionary<double,int> ClusterCountsByMaxDeviation(Dictionary<int,int> resultsCount, double MAX_DEVIATION_FRACTION, double MAX_ABSOLUTE_COUNT_DEVIATION)
        {
            List<int> countsList = resultsCount.Keys.ToList();
            //Dictionary<double, int> clusterHeads = new Dictionary<double, int>();
            List<KeyValuePair<double, int>> clusterHeads = new List<KeyValuePair<double, int>>();

            //initialize clusterheads with same entries
            foreach (KeyValuePair<int,int> entry in resultsCount)
            {
                clusterHeads.Add(new KeyValuePair<double, int>(entry.Key, entry.Value));
            }

            //first cluster them hierchically
            bool merged = false;
            do
            {
                List<double> clusterValues = new List<double>();
                foreach (KeyValuePair<double, int> ch in clusterHeads)
                {
                    clusterValues.Add(ch.Key);
                }
                //List<double> clusterValues = clusterHeads.Keys.ToList();
                //first find the closest numbers with the max counts
                int index1 = -1, index2 = -1;
                int maxCount = -1;
                double bestMergedValue = -1;
                merged = false;
                for(int i=0;i<clusterValues.Count;i++)
                {
                    for(int j=i+1;j<clusterValues.Count;j++)
                    {
                        //merged value is the weighted average
                        double mergedValue = (clusterValues[i] * clusterHeads[i].Value + clusterValues[j] * clusterHeads[j].Value) / (clusterHeads[i].Value + clusterHeads[j].Value);
                        //find the maximum possible deviation
                        double maxdev = MAX_ABSOLUTE_COUNT_DEVIATION;
                        if (Math.Max(clusterValues[i],clusterValues[j])*MAX_DEVIATION_FRACTION > maxdev)
                        {
                            maxdev = Math.Max(clusterValues[i], clusterValues[j]) * MAX_DEVIATION_FRACTION;
                        }
                        //check if they match the deviation criterion
                        if (Math.Abs(clusterValues[i]-clusterValues[j]) <= maxdev )
                        {
                            int mergedCount = clusterHeads[i].Value + clusterHeads[j].Value;
                            if(mergedCount > maxCount)
                            {
                                index1 = i;
                                index2 = j;
                                maxCount = mergedCount;
                                merged = true;
                                bestMergedValue = mergedValue;
                            }
                        }
                    }
                }
                if(merged) //there are mergable clusterheads
                {
                    //add the merged cluster head
                    clusterHeads.Add(new KeyValuePair<double, int>(bestMergedValue,maxCount));
                    //remove the orignial clusterheads
                    clusterHeads.RemoveAt(index1);
                    clusterHeads.RemoveAt(index2-1); // index 2 is always larger than index 1
                }
            } while (merged);

            Dictionary<double, int> aggregatedClusterHeads = new Dictionary<double, int>();
            for (int i = 0; i < clusterHeads.Count; i++)
            {
                aggregatedClusterHeads.Add(clusterHeads[i].Key, clusterHeads[i].Value);
            }

            return aggregatedClusterHeads; 
        }

        

        private static Dictionary<double, List<SatyamResult>> ClusterCountsSatyamResultByMaxDeviation(List<SatyamResult> satyamResults, double MAX_DEVIATION_FRACTION, double MAX_ABSOLUTE_COUNT_DEVIATION, out Dictionary<int, List<SatyamResult>> resultsCount)
        {
            resultsCount = new Dictionary<int, List<SatyamResult>>();
            foreach (SatyamResult r in satyamResults)
            {
                ObjectCountingResult result = JSonUtils.ConvertJSonToObject<ObjectCountingResult>(r.TaskResult);
                if (!resultsCount.ContainsKey(result.Count))
                {
                    resultsCount.Add(result.Count, new List<SatyamResult>());
                }
                resultsCount[result.Count].Add(r);
            }


            List<int> countsList = resultsCount.Keys.ToList();
            //Dictionary<double, int> clusterHeads = new Dictionary<double, int>();
            List<KeyValuePair<double, List<SatyamResult>>> clusterHeads = new List<KeyValuePair<double, List<SatyamResult>>>();

            //initialize clusterheads with same entries
            foreach (KeyValuePair<int, List<SatyamResult>> entry in resultsCount)
            {
                clusterHeads.Add(new KeyValuePair<double, List<SatyamResult>>(entry.Key, entry.Value));
            }

            //first cluster them hierchically
            bool merged = false;
            do
            {
                List<double> clusterValues = new List<double>();
                foreach (KeyValuePair<double, List<SatyamResult>> ch in clusterHeads)
                {
                    clusterValues.Add(ch.Key);
                }
                //List<double> clusterValues = clusterHeads.Keys.ToList();
                //first find the closest numbers with the max counts
                int index1 = -1, index2 = -1;
                int maxCount = -1;
                double bestMergedValue = -1;
                merged = false;
                for (int i = 0; i < clusterValues.Count; i++)
                {
                    for (int j = i + 1; j < clusterValues.Count; j++)
                    {
                        //merged value is the weighted average
                        double mergedValue = (clusterValues[i] * clusterHeads[i].Value.Count + clusterValues[j] * clusterHeads[j].Value.Count) / (clusterHeads[i].Value.Count + clusterHeads[j].Value.Count);
                        //find the maximum possible deviation
                        double maxdev = MAX_ABSOLUTE_COUNT_DEVIATION;
                        if (Math.Max(clusterValues[i], clusterValues[j]) * MAX_DEVIATION_FRACTION > maxdev)
                        {
                            maxdev = Math.Max(clusterValues[i], clusterValues[j]) * MAX_DEVIATION_FRACTION;
                        }
                        //check if they match the deviation criterion
                        if (Math.Abs(clusterValues[i] - clusterValues[j]) <= maxdev)
                        {
                            int mergedCount = clusterHeads[i].Value.Count + clusterHeads[j].Value.Count;
                            if (mergedCount > maxCount)
                            {
                                index1 = i;
                                index2 = j;
                                maxCount = mergedCount;
                                merged = true;
                                bestMergedValue = mergedValue;
                            }
                        }
                    }
                }
                if (merged) //there are mergable clusterheads
                {
                    //add the merged cluster head
                    List<SatyamResult> mergedResults = new List<SatyamResult>();
                    foreach (SatyamResult r in clusterHeads[index1].Value)
                    {
                        mergedResults.Add(r);
                    }

                    foreach (SatyamResult r in clusterHeads[index2].Value)
                    {
                        mergedResults.Add(r);
                    }
                    clusterHeads.Add(new KeyValuePair<double, List<SatyamResult>>(bestMergedValue, mergedResults));
                    
                    //remove the orignial clusterheads
                    clusterHeads.RemoveAt(index1);
                    clusterHeads.RemoveAt(index2 - 1); // index 2 is always larger than index 1
                }
            } while (merged);

            Dictionary<double, List<SatyamResult>> aggregatedClusterHeads = new Dictionary<double, List<SatyamResult>>();
            for (int i = 0; i < clusterHeads.Count; i++)
            {
                aggregatedClusterHeads.Add(clusterHeads[i].Key, clusterHeads[i].Value);
            }

            return aggregatedClusterHeads;
        }
    }
 }
