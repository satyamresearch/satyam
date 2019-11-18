using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SatyamTaskResultClasses;
using SQLTables;
using Utilities;
using UsefulAlgorithms;
using HelperClasses;
using Constants;

namespace SatyamResultAggregators
{

    public class MultiObjectLocalizationAndLabelingAggregatedResultMetaData
    {
        public int TotalCount;
    }

    public class MultiObjectLocalizationAndLabelingAggregatedResult
    {
        public MultiObjectLocalizationAndLabelingResult boxesAndCategories;
        public MultiObjectLocalizationAndLabelingAggregatedResultMetaData metaData;
    }

    public static class MultiObjectLocalizationAndLabelingAggregator
    {

        

        public static int MIN_BOX_DIMENSION_FOR_CONSIDERATION = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MIN_BOX_DIMENSION_FOR_CONSIDERATION; //boxes smaller than this pixels are ignored for accpetance decision

        public static void getMaxCountGroup(List<BoundingBoxGroup> boxGroups, out int maxCount, out int maxGroupIndex) 
        {
            //find the boxgroup with the maxCount
            maxCount = 0;
            maxGroupIndex = -1;
            for (int i = 0; i < boxGroups.Count; i++)
            {
                if (boxGroups[i].boundingBoxList.Count > maxCount) // there are two boxes within acceptance range
                {
                    maxCount = boxGroups[i].boundingBoxList.Count;
                    maxGroupIndex = i;
                }
            }
        }

        public static MultiObjectLocalizationAndLabelingAggregatedResult getAggregatedResult(List<MultiObjectLocalizationAndLabelingResult> results,
            int MinResults = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double CategoryMajorityThreshold = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MAJORITY_CATEGORY_THRESHOLD,
            double ObjectsCoverageThreshold = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_AGGREGATION_TERMINATION,
            double DeviationPixelThreshold = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_DEVIATION_THRESHOLD
            )
        {
            if (results.Count < MinResults) 
            {
                return null;
            }

            //if the image was scaled down during display, errors get scaled up 
            //so we need to scale our deviation thresholds
            double minboxdimension_threshold_x = MIN_BOX_DIMENSION_FOR_CONSIDERATION * results[0].displayScaleReductionX;
            double minboxdimension_threshold_y = MIN_BOX_DIMENSION_FOR_CONSIDERATION * results[0].displayScaleReductionY;


            //first use multipartitie wieghted matching to associated the boxes disregarding the labels since
            //people might make mistake with lables but boxes are usually right
            List<List<BoundingBox>> allboxes = new List<List<BoundingBox>>();
            List<int> noBoxesPerResult = new List<int>(); //how many boxes did each person draw?
            List<List<bool>> largeBoxes = new List<List<bool>>(); //was this box was bigger than MIN_BOX_DIMENSION_FOR_CONSIDERATION?
            foreach (MultiObjectLocalizationAndLabelingResult res in results)
            {
                allboxes.Add(new List<BoundingBox>());
                largeBoxes.Add(new List<bool>());

                if (res == null)
                {
                    noBoxesPerResult.Add(0);
                    continue;
                }
                
                List<BoundingBox> boxList = allboxes[allboxes.Count - 1];
                List<bool> largeBoxList = largeBoxes[largeBoxes.Count - 1];
                foreach(MultiObjectLocalizationAndLabelingResultSingleEntry entry in res.objects)
                {
                    boxList.Add(entry.boundingBox);
                    if(entry.boundingBox.getWidth() < minboxdimension_threshold_x && entry.boundingBox.getHeight() < minboxdimension_threshold_y)
                    {
                        largeBoxList.Add(false);
                    }
                    else
                    {
                        largeBoxList.Add(true);
                    }
                }
                noBoxesPerResult.Add(boxList.Count);
            }
            //now associate boxes across the various results
            List<MultipartiteWeightedMatch> boxAssociation = BoundingBoxAssociation.computeBoundingBoxAssociations(allboxes);
            int noBoxes = boxAssociation.Count;
            int noAssociatedBoxes = 0;
            int noIgnorable = 0;
            //how many of the drawn boxes for each result were actually associated by two or more people for each user?
            SortedDictionary<int,int> noMultipleAssociatedBoxesPerResult = new SortedDictionary<int, int>();
            SortedDictionary<int, int> noSmallIgnorableBoxesPerResult = new SortedDictionary<int, int>();

            for (int i=0;i<results.Count;i++)
            {
                noMultipleAssociatedBoxesPerResult.Add(i,0);
                noSmallIgnorableBoxesPerResult.Add(i, 0);
            }
            foreach (MultipartiteWeightedMatch match in boxAssociation)
            {
                if(match.elementList.Count > 1) //this has been corroborated by two people
                {

                    noAssociatedBoxes++;
                    foreach (KeyValuePair<int, int> entry in match.elementList)
                    {
                        noMultipleAssociatedBoxesPerResult[entry.Key]++;
                    }
                }
                else
                {
                    List<int> keys = match.elementList.Keys.ToList();
                    if(largeBoxes[keys[0]][match.elementList[keys[0]]] == false) //the box was a small box can be ignored
                    {
                        noIgnorable++;
                        noSmallIgnorableBoxesPerResult[keys[0]]++;
                    }
                }
            }

            //count how many people have a high association ratio
            int noHighAssociationRatio = 0;
            for(int i=0;i<results.Count;i++)
            {
                if (noBoxesPerResult[i] == 0) continue;
                double ratio = ((double)noMultipleAssociatedBoxesPerResult[i] + (double)noSmallIgnorableBoxesPerResult[i])/ (double)noBoxesPerResult[i];
                if(ratio > ObjectsCoverageThreshold)
                {
                    noHighAssociationRatio++;
                }
            }
            if(noHighAssociationRatio < MinResults) //at least three people should have all their boxes highly corroborated by one other person
            {
                return null;
            }

            //now we have to find out if the boxes were drawn well and only take the boxes that are within
            //a reasonable deviation of each other.


            //if the image was scaled down during display, errors get scaled up 
            //so we need to scale our deviation thresholds
            double deviation_threshold_x = DeviationPixelThreshold * results[0].displayScaleReductionX;
            double deviation_threshold_y = DeviationPixelThreshold * results[0].displayScaleReductionY;

            int noAcceptedBoxes = 0;
            int noSmallIgnore = 0;
            List<List<BoundingBoxGroup>> allBoxGroups = new List<List<BoundingBoxGroup>>();
            List<BoundingBox> finalBoxes = new List<BoundingBox>(); //stores the aggregated boxes
            SortedDictionary<int, List<int>> HighQualityAssociatedBoxPerResult = new SortedDictionary<int, List<int>>();

            foreach (MultipartiteWeightedMatch match in boxAssociation)
            {
                List<BoundingBox> boxList = new List<BoundingBox>();
                List<string> identifiers = new List<string>();
                foreach(KeyValuePair<int,int> entry in match.elementList)
                {
                    boxList.Add(allboxes[entry.Key][entry.Value]);
                    identifiers.Add(entry.Key + "_" + entry.Value);
                }
                //List<BoundingBoxGroup> boxGroups = MergeAndGroupBoundingBoxes.GreedyMeanHierarchicalMergeByPixelDeviation(boxList, identifiers, DEVIATION_THRESHOLD);
                List<BoundingBoxGroup> boxGroups = MergeAndGroupBoundingBoxes.GreedyMeanHierarchicalMergeByPixelDeviation(boxList, identifiers, deviation_threshold_x,deviation_threshold_y);

                allBoxGroups.Add(boxGroups);

                int maxCount = 0;
                int index = -1;
                getMaxCountGroup(boxGroups, out maxCount, out index);
                ////find the boxgroup with the maxCount
                //int maxCount = 0;
                //int index = -1;
                //for(int i=0;i<boxGroups.Count;i++)
                //{
                //    if(boxGroups[i].boundingBoxList.Count > maxCount) // there are two boxes within acceptance range
                //    {
                //        maxCount = boxGroups[i].boundingBoxList.Count;
                //        index = i;
                //    }
                //}
                if (maxCount > 1)
                {
                    noAcceptedBoxes++;
                    finalBoxes.Add(boxGroups[index].mergedBoundingBox);
                    // count the majority group high associations
                    foreach (string id in boxGroups[index].identifiers)
                    {
                        string[] fields = id.Split('_');
                        int partId = Convert.ToInt32(fields[0]);
                        int boxId = Convert.ToInt32(fields[1]);
                        if (!HighQualityAssociatedBoxPerResult.ContainsKey(partId))
                        {
                            HighQualityAssociatedBoxPerResult.Add(partId, new List<int>());
                        }
                        HighQualityAssociatedBoxPerResult[partId].Add(boxId);
                    }

                }
                else
                {
                    finalBoxes.Add(BoundingBox.NullBoundingBox);
                    //were all the boxes drawn too small?
                    bool small = true;
                    for(int i = 0; i < boxGroups.Count; i++)
                    {
                        string[] parts = boxGroups[i].identifiers[0].Split('_');
                        int resultNo = Convert.ToInt32(parts[0]);
                        int boxNo = Convert.ToInt32(parts[1]);
                        if(largeBoxes[resultNo][boxNo]==true)
                        {
                            small = false;
                            break;
                        }
                    }
                    if(small)
                    {
                        noSmallIgnore++;
                    }
                }
            }

            if (((double)noAcceptedBoxes + (double)noSmallIgnore)/(double)noAssociatedBoxes < ObjectsCoverageThreshold 
                && results.Count < MaxResults) //this is acceptable
            {
                return null;
            }

            //count how many people have a high "quality" association ratio
            int noResultsWithHighQualityObjectCoverage = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if (noBoxesPerResult[i] == 0) continue;
                if (!HighQualityAssociatedBoxPerResult.ContainsKey(i)) continue;
                double ratio = ((double)HighQualityAssociatedBoxPerResult[i].Count) / (double)noBoxesPerResult[i];
                if (ratio > ObjectsCoverageThreshold)
                {
                    noResultsWithHighQualityObjectCoverage++;
                }
            }
            if (noResultsWithHighQualityObjectCoverage < MinResults && results.Count < MaxResults) //at least three people should have most of their boxes highly corroborated by one other person
            {
                return null;
            }



            //now we need to see if the categores have a supermajority
            List<string> finalCategories = new List<string>(); //stores the aggregated categories
            int cntr = -1;
            foreach (MultipartiteWeightedMatch match in boxAssociation)
            {
                cntr++;
                if(BoundingBox.IsNullBoundingBox(finalBoxes[cntr])) //this is not an acceptable box
                {
                    finalCategories.Add("");
                    continue;
                }
                Dictionary<string, int> categoryNames = new Dictionary<string, int>();
                int totalCount = match.elementList.Count;
                int maxCount = 0;
                string maxCategory="";
                foreach (KeyValuePair<int,int> entry in match.elementList)
                {
                    string category = results[entry.Key].objects[entry.Value].Category;
                    if(!categoryNames.ContainsKey(category))
                    {
                        categoryNames.Add(category,0);
                    }
                    categoryNames[category]++;
                    if(maxCount < categoryNames[category])
                    {
                        maxCount = categoryNames[category];
                        maxCategory = category;
                    }
                }
                double probability = ((double)maxCount + 1) / ((double)totalCount + 2);
                if(probability < CategoryMajorityThreshold && results.Count < MaxResults) //this is not a valid category need more work
                {
                    return null;
                }
                else
                {
                    finalCategories.Add(maxCategory);
                }
            }

            //now we have enough information to create the aggregate results

            MultiObjectLocalizationAndLabelingAggregatedResult aggResult = new MultiObjectLocalizationAndLabelingAggregatedResult();
            aggResult.metaData = new MultiObjectLocalizationAndLabelingAggregatedResultMetaData();
            aggResult.metaData.TotalCount = results.Count;
            aggResult.boxesAndCategories = new MultiObjectLocalizationAndLabelingResult();
            aggResult.boxesAndCategories.objects = new List<MultiObjectLocalizationAndLabelingResultSingleEntry>();
            
            cntr = -1;

            foreach (MultipartiteWeightedMatch match in boxAssociation)
            {
                cntr++;
                if(BoundingBox.IsNullBoundingBox(finalBoxes[cntr]))
                {
                    continue;
                }
                MultiObjectLocalizationAndLabelingResultSingleEntry entry = new MultiObjectLocalizationAndLabelingResultSingleEntry();
                entry.boundingBox = finalBoxes[cntr];
                entry.Category = finalCategories[cntr];
                aggResult.boxesAndCategories.objects.Add(entry);
            }

            aggResult.boxesAndCategories.displayScaleReductionX = results[0].displayScaleReductionX;
            aggResult.boxesAndCategories.displayScaleReductionY = results[0].displayScaleReductionY;
            aggResult.boxesAndCategories.imageHeight = results[0].imageHeight;
            aggResult.boxesAndCategories.imageWidth = results[0].imageWidth;


            return aggResult;
        }

        public static string GetAggregatedResultString(List<SatyamResultsTableEntry> results)
        {
            if (results.Count == 0) return null;

            List<MultiObjectLocalizationAndLabelingResult> resultList = new List<MultiObjectLocalizationAndLabelingResult>();
            List<string> WorkersPerTask = new List<string>();
            foreach (SatyamResultsTableEntry entry in results)
            {
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);

                // remove duplicate workers result
                // "" works for internal test!!!
                string workerID = res.amazonInfo.WorkerID;
                if (workerID!="" &&  WorkersPerTask.Contains(workerID))
                {
                    continue;
                }
                //enclose only non-duplicate results, one per each worker.
                WorkersPerTask.Add(workerID);

                MultiObjectLocalizationAndLabelingResult taskr = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingResult>(res.TaskResult);
                resultList.Add(taskr);
            }

            /// Store Progressive results in progressive table, for consensus loading on turker task pages.
            // check  if there are any new valid results first
            SatyamAggregatedProgressiveResultsTableAccess progDBtemp = new SatyamAggregatedProgressiveResultsTableAccess();
            int LatestResultsAggregated = progDBtemp.getLatestNoResultsAggregatedByTaskID(results[0].SatyamTaskTableEntryID);// default value is -1
            progDBtemp.close();

            if (LatestResultsAggregated >= resultList.Count) return null;
            /// safely return null here, since if it's aggregated by standard, it won't be executed again.
            /// it will be only executed when there is no standard aggregation result. 
            /// so return null when no new results came in to even produce a progress aggregation result.


            int MaxCount = Math.Min(resultList.Count, TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE);
            MultiObjectLocalizationAndLabelingAggregatedResult tempr = getAggregatedResult(resultList, MinResults: 2);
            if (tempr != null)
            {
                string rString = JSonUtils.ConvertObjectToJSon<MultiObjectLocalizationAndLabelingAggregatedResult>(tempr);
                SatyamAggregatedResult aggResult = new SatyamAggregatedResult();
                aggResult.SatyamTaskTableEntryID = results[0].SatyamTaskTableEntryID;
                aggResult.AggregatedResultString = rString;
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
                aggResult.TaskParameters = res.TaskParametersString;
                string resultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(aggResult);

                // create progressive entry
                SatyamAggregatedProgressiveResultsTableEntry aggProgEntry = new SatyamAggregatedProgressiveResultsTableEntry();
                aggProgEntry.JobGUID = results[0].JobGUID;
                aggProgEntry.JobTemplateType = results[0].JobTemplateType;
                aggProgEntry.SatyamTaskTableEntryID = results[0].SatyamTaskTableEntryID;
                aggProgEntry.UserID = results[0].UserID;
                aggProgEntry.ResultString = resultString;
                aggProgEntry.ResultsAggregated = resultList.Count; // use the actual valid non-duplicate results number here.
                SatyamAggregatedProgressiveResultsTableAccess aggProgDB = new SatyamAggregatedProgressiveResultsTableAccess();
                aggProgDB.AddEntry(aggProgEntry);
                aggProgDB.close();
            }
            else { return null; }


            MultiObjectLocalizationAndLabelingAggregatedResult r = getAggregatedResult(resultList);
            if (r != null)
            {
                string rString = JSonUtils.ConvertObjectToJSon<MultiObjectLocalizationAndLabelingAggregatedResult>(r);
                SatyamAggregatedResult aggResult = new SatyamAggregatedResult();
                aggResult.SatyamTaskTableEntryID = results[0].SatyamTaskTableEntryID;
                aggResult.AggregatedResultString = rString;
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
                aggResult.TaskParameters = res.TaskParametersString;
                string resultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(aggResult);
                return resultString;
            }
            else {
                return null;
            }
        }

        public static double ACCEPTANCE_NUMBER_OF_BOXES_THRESHOLD = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_PAYMENT; //the person must have made at least 80% of the boxes
        public static bool IsAcceptable(SatyamAggregatedResultsTableEntry aggResultEntry, SatyamResultsTableEntry resultEntry,
            double DeviationPixelThreshold = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_DEVIATION_THRESHOLD_FOR_PAYMENT)
        {
            
            //most boxes should be within limits 
            //most categories should be right
            SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(aggResultEntry.ResultString);
            MultiObjectLocalizationAndLabelingAggregatedResult aggresult = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingAggregatedResult>(satyamAggResult.AggregatedResultString);
            SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(resultEntry.ResultString);
            MultiObjectLocalizationAndLabelingResult result = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingResult>(satyamResult.TaskResult);

            if (result == null) return false;

            //first check if the number of boxes are within limit
            int boxLimit = (int)Math.Ceiling((double)aggresult.boxesAndCategories.objects.Count* ACCEPTANCE_NUMBER_OF_BOXES_THRESHOLD);
            if(result.objects.Count < boxLimit)
            {
                return false;
            }

            double minboxdimension_threshold_x = MIN_BOX_DIMENSION_FOR_CONSIDERATION * result.displayScaleReductionX;
            double minboxdimension_threshold_y = MIN_BOX_DIMENSION_FOR_CONSIDERATION * result.displayScaleReductionY;
            //We fist do a bipartitte matching to find the best assocaition for the boxes
            List<List<BoundingBox>> allboxes = new List<List<BoundingBox>>();
            allboxes.Add(new List<BoundingBox>());
            foreach(MultiObjectLocalizationAndLabelingResultSingleEntry entry in result.objects)
            {
                allboxes[0].Add(entry.boundingBox);
            }
            allboxes.Add(new List<BoundingBox>());
            List<bool> tooSmallToIgnore = new List<bool>();
            foreach (MultiObjectLocalizationAndLabelingResultSingleEntry entry in aggresult.boxesAndCategories.objects)
            {
                allboxes[1].Add(entry.boundingBox);
                if(entry.boundingBox.getWidth() < minboxdimension_threshold_x && entry.boundingBox.getHeight() < minboxdimension_threshold_y)
                {
                    tooSmallToIgnore.Add(true);
                }
                else
                {
                    tooSmallToIgnore.Add(false);
                }
            }
            List<MultipartiteWeightedMatch> boxAssociation = BoundingBoxAssociation.computeBoundingBoxAssociations(allboxes);

            //now find how many of the results match aggregated results
            int noAccepted = 0;

            double deviation_threshold_x = DeviationPixelThreshold * result.displayScaleReductionX;
            double deviation_threshold_y = DeviationPixelThreshold * result.displayScaleReductionY;

            int noIgnorable = 0;
            foreach (MultipartiteWeightedMatch match in boxAssociation)
            {
                if(match.elementList.ContainsKey(1)) // this contains an aggregated box
                {
                    if(match.elementList.ContainsKey(0)) // a result box has been associated
                    {
                        BoundingBox aggregatedBoundingBox = allboxes[1][match.elementList[1]];
                        BoundingBox resultBoundingBox = allboxes[0][match.elementList[0]];
                        //double deviation = aggregatedBoundingBox.ComputeMaxDeviationMetric(resultBoundingBox);
                        double deviation = aggregatedBoundingBox.ComputeNormalizedMaxDeviationMetric(resultBoundingBox, deviation_threshold_x, deviation_threshold_y);
                        if (deviation <= 1) //deviation test passed
                        {
                            //now check category
                            if(result.objects[match.elementList[0]].Category == aggresult.boxesAndCategories.objects[match.elementList[1]].Category)
                            {
                                //both category and bounding box tests have passed
                                noAccepted++;
                            }
                        }
                        else
                        {
                            if(tooSmallToIgnore[match.elementList[1]])
                            {
                                noIgnorable++;
                            }
                        }
                    }
                }
            }

            if(noAccepted >= boxLimit-noIgnorable)
            {
                return true;
            }

            return false;
        }

    }

    

}
