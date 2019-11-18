using AzureBlobStorage;
using Constants;
using HelperClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulAlgorithms;
using Utilities;

namespace SatyamResultAggregators
{
    
    public class ImageSegmentationAggregatedResultMetaData
    {
        public int TotalCount;
        public string PNG_URL;
    }

    public class ImageSegmentationAggregatedResult
    {
        public ImageSegmentationResult boxesAndCategories;
        public ImageSegmentationAggregatedResultMetaData metaData;
    }

    public static class ImageSegmentationAggregator
    {

        public static ImageSegmentationAggregatedResult getAggregatedResult(List<ImageSegmentationResult> originalResults,
            string SatyamURL,
            string guid,
            int MinResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double CategoryMajorityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAJORITY_CATEGORY_THRESHOLD,
            double PolygonBoundaryMajorityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAJORITY_POLYGON_BOUNDARY_THRESHOLD,
            double ObjectsCoverageThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_AGGREGATION_TERMINATION,
            double minSimilarityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_POLYGON_IOU_THRESHOLD,
            int minResultsForConsensus = TaskConstants.IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_FOR_CONSENSUS
            )
        {

            List<ImageSegmentationResult> results = new List<ImageSegmentationResult>();
            // Warning: filter empty results: strong assumption that there has to be something for now!!!
            for (int i = 0; i < originalResults.Count; i++)
            {
                if (originalResults[i] != null && originalResults[i].objects.Count!=0) 
                {
                    results.Add(originalResults[i]);
                }
            }

            Console.WriteLine("Filetered Results: {0}", results.Count);

            if (results.Count < MinResults) //need at least three results!
            {
                return null;
            }

            int ImageWidth = results[0].imageWidth;
            int ImageHeight = results[0].imageHeight;
            
            //auto padding data to make the stride a multiple of 4. required by bmpdata for output
            int paddedWidth = ImageWidth;
            if (paddedWidth % 4 != 0)
            {
                paddedWidth = (paddedWidth / 4 + 1) * 4;
            }
            byte[] PNG = new byte[ImageHeight * paddedWidth];


            ImageSegmentationAggregatedResult aggResult = new ImageSegmentationAggregatedResult();
            aggResult.metaData = new ImageSegmentationAggregatedResultMetaData();
            aggResult.metaData.TotalCount = results.Count;

            for (int i = 0; i < PNG.Length; i++)
            {
                PNG[i] = 0;
            }

            aggResult.boxesAndCategories = new ImageSegmentationResult();
            aggResult.boxesAndCategories.objects = new List<ImageSegmentationResultSingleEntry>();
            aggResult.boxesAndCategories.displayScaleReductionX = results[0].displayScaleReductionX;
            aggResult.boxesAndCategories.displayScaleReductionY = results[0].displayScaleReductionY;
            aggResult.boxesAndCategories.imageHeight = ImageHeight;
            aggResult.boxesAndCategories.imageWidth = ImageWidth;

            //first use multipartitie wieghted matching to associated the boxes disregarding the labels since
            //people might make mistake with lables but boxes are usually right
            List<List<Segment>> AllPolygons = new List<List<Segment>>();
            List<int> noPolygonsPerResult = new List<int>();

            foreach (ImageSegmentationResult res in results)
            {
                AllPolygons.Add(new List<Segment>());

                if (res == null)
                {
                    noPolygonsPerResult.Add(0);
                    continue;
                }

                List<Segment> polygonList = AllPolygons[AllPolygons.Count - 1];

                foreach (ImageSegmentationResultSingleEntry entry in res.objects)
                {
                    polygonList.Add(entry.segment);
                }
                noPolygonsPerResult.Add(polygonList.Count);
            }
            //now associate boxes across the various results
            //List<MultipartiteWeightedMatch> polyAssociation = PolygonAssociation.computeGenericPolygonAssociations(AllPolygons);

            List<MultipartiteWeightedMatch> polyAssociation = GenericObjectAssociation.computeAssociations<Segment>(AllPolygons, Segment.computeIoU_PixelSweep);

            int noObjects = polyAssociation.Count;
            int noAssociatedPolygons = 0;

            //how many of the drawn boxes for each result were actually associated by two or more people for each user?
            SortedDictionary<int, int> noMultipleAssociatedPolygonsPerResult = new SortedDictionary<int, int>();

            for (int i = 0; i < results.Count; i++)
            {
                noMultipleAssociatedPolygonsPerResult.Add(i, 0);
            }
            foreach (MultipartiteWeightedMatch match in polyAssociation)
            {
                if (match.elementList.Count > 1) //this has been corroborated by two people
                {
                    noAssociatedPolygons++;
                    foreach (KeyValuePair<int, int> entry in match.elementList)
                    {
                        noMultipleAssociatedPolygonsPerResult[entry.Key]++;
                    }
                }
            }

            //count how many people have a high association ratio
            int noHighAssociationRatio = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if (noPolygonsPerResult[i] == 0) continue;
                double ratio = (double)noMultipleAssociatedPolygonsPerResult[i] / (double)noPolygonsPerResult[i];
                if (ratio > ObjectsCoverageThreshold)
                {
                    noHighAssociationRatio++;
                }
            }
            if (noHighAssociationRatio < MinResults && results.Count < MaxResults) //at least three people should have all their boxes highly corroborated by one other person
            {
                return null;
            }

            //int noAcceptedPolygons = 0;

            SortedDictionary<int, List<int>> noHighQualityAssociation = new SortedDictionary<int, List<int>>();

            for (int idx = 0; idx < polyAssociation.Count; idx++)
            {
                MultipartiteWeightedMatch match = polyAssociation[idx];
                List<Segment> polyList = new List<Segment>();
                List<string> identifiers = new List<string>();
                foreach (KeyValuePair<int, int> entry in match.elementList)
                {
                    polyList.Add(AllPolygons[entry.Key][entry.Value]);
                    identifiers.Add(entry.Key + "_" + entry.Value);
                }

                SegmentGroup majorityGroup = getMajorityGroupPolygons(identifiers, polyList, minSimilarityThreshold, match.weightMatrix);

                if (majorityGroup == null) continue;

                if (majorityGroup.segments.Count< minResultsForConsensus)
                {
                    continue;
                }
                Segment aggregatedPolygon = new Segment();// dummy polygon
                byte[] png = GetAggregatedGenericPolygon_PixelSweep(majorityGroup.segments, ImageWidth, ImageHeight, PolygonBoundaryMajorityThreshold);


                // log where the results come from
                foreach (string id in majorityGroup.identifiers)
                {
                    string[] fields = id.Split('_');
                    int k = Convert.ToInt32(fields[0]);
                    int v = Convert.ToInt32(fields[1]);
                    if (!noHighQualityAssociation.ContainsKey(k))
                    {
                        noHighQualityAssociation.Add(k, new List<int>());
                    }
                    noHighQualityAssociation[k].Add(v);
                }


                // category aggregation
                Dictionary<string, int> categoryNames = new Dictionary<string, int>();
                int totalCount = match.elementList.Count;
                int maxCount = 0;
                string maxCategory = "";
                foreach (KeyValuePair<int, int> entry in match.elementList)
                {
                    string category = results[entry.Key].objects[entry.Value].Category;
                    if (!categoryNames.ContainsKey(category))
                    {
                        categoryNames.Add(category, 0);
                    }
                    categoryNames[category]++;
                    if (maxCount < categoryNames[category])
                    {
                        maxCount = categoryNames[category];
                        maxCategory = category;
                    }
                }
                double probability = ((double)maxCount + 1) / ((double)totalCount + 2);
                if (probability < CategoryMajorityThreshold && results.Count < MaxResults) //this is not a valid category need more work
                {
                    return null;
                }


                // now we have one segment ready
                ImageSegmentationResultSingleEntry aggregated = new ImageSegmentationResultSingleEntry();
                aggregated.segment = aggregatedPolygon;
                aggregated.Category = maxCategory;

                aggResult.boxesAndCategories.objects.Add(aggregated);
                for ( int i = 0; i < ImageWidth; i++)
                {
                    for ( int j = 0; j < ImageHeight; j++)
                    {
                        int pospng = j * ImageWidth + i;
                        int pos = j * paddedWidth + i;
                        if (PNG[pos] == 0 && png[pospng] != 0)
                        {
                            PNG[pos] = (byte)(idx + 1);
                        }
                    }
                }
            }


            // check no. of high association results
            int noResultsWithHighQualityObjectCoverage = 0;
            for (int i = 0; i < results.Count; i++)
            {
                if (noPolygonsPerResult[i] == 0) continue;
                if (!noHighQualityAssociation.ContainsKey(i)) continue;
                double ratio = ((double)noHighQualityAssociation[i].Count) / (double)noPolygonsPerResult[i];
                if (ratio > ObjectsCoverageThreshold)
                {
                    noResultsWithHighQualityObjectCoverage++;
                }
            }
            if (noResultsWithHighQualityObjectCoverage < MinResults && results.Count < MaxResults) //at least three people should have most of their boxes highly corroborated by one other person
            {
                return null;
            }

            // save and upload to azure
            string filename = URIUtilities.filenameFromURINoExtension(SatyamURL);
            //string filepath = DirectoryConstants.defaultTempDirectory + filename + "_aggregated.PNG";
            string filepath = DirectoryConstants.defaultAzureTempDirectory + filename + "_aggregated.PNG";
            ImageUtilities.savePNGRawData(filepath, ImageWidth, ImageHeight, PNG);

            SatyamJobStorageAccountAccess blob = new SatyamJobStorageAccountAccess();
            string container = SatyamTaskGenerator.JobTemplateToSatyamContainerNameMap[TaskConstants.Segmentation_Image_MTurk];
            string directoryPath = guid + "_aggregated";
            blob.UploadALocalFile(filepath, container, directoryPath);

            aggResult.metaData.PNG_URL = TaskConstants.AzureBlobURL + container + "/" + directoryPath + "/" + filename + "_aggregated.PNG";

            //clean up
            File.Delete(filepath);
            return aggResult;
        }

        public static string GetAggregatedResultString(List<SatyamResultsTableEntry> results)
        {
            if (results.Count == 0) return null;

            string resultString = null;
            List<ImageSegmentationResult> resultList = new List<ImageSegmentationResult>();
            List<string> WorkersPerTask = new List<string>();

            SatyamResult res0 = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(res0.TaskParametersString);
            string SatyamURL = task.SatyamURI;
            string guid = results[0].JobGUID;

            foreach (SatyamResultsTableEntry entry in results)
            {
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);

                // remove duplicate workers result
                string workerID = res.amazonInfo.WorkerID;
                if (WorkersPerTask.Contains(workerID))
                {
                    continue;
                }

                string assignmentID = res.amazonInfo.AssignmentID;
                if (assignmentID == "" || assignmentID == "ASSIGNMENT_ID_NOT_AVAILABLE") continue;

                //enclose only non-duplicate results, one per each worker.
                if (workerID != "" && workerID != TaskConstants.AdminID)
                {
                    // make a pass for test and admins
                    WorkersPerTask.Add(workerID);
                }


                ImageSegmentationResult taskr = JSonUtils.ConvertJSonToObject<ImageSegmentationResult>(res.TaskResult);
                resultList.Add(taskr);
            }


            ImageSegmentationAggregatedResult r = getAggregatedResult(resultList, SatyamURL, guid);
            if (r != null)
            {
                string rString = JSonUtils.ConvertObjectToJSon<ImageSegmentationAggregatedResult>(r);
                SatyamAggregatedResult aggResult = new SatyamAggregatedResult();
                aggResult.SatyamTaskTableEntryID = results[0].SatyamTaskTableEntryID;
                aggResult.AggregatedResultString = rString;
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
                aggResult.TaskParameters = res.TaskParametersString;
                resultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(aggResult);
            }
            return resultString;
        }


        public static byte[] GetAggregatedGenericPolygon_PixelSweep(
            List<Segment> polyList, int ImageWidth, int ImageHeight,
            double PolygonBoundaryMajorityThreshold)
        {
            byte[] pixels = new byte[ImageWidth * ImageHeight];
            for (int i = 0; i < ImageWidth; i++)
            {
                for (int j = 0; j < ImageHeight; j++)
                {
                    double ratio = getInPolygonVotesRatio(i, j, polyList);
                    if (ratio > PolygonBoundaryMajorityThreshold)
                    {
                        //pixels[j*ImageHeight+i] = 255;
                        pixels[j * ImageWidth + i] = 255;
                    }
                    else
                    {
                        pixels[j * ImageWidth + i] = 0;
                    }
                }
            }
            return pixels;
        }
        public static double getInPolygonVotesRatio(int x, int y, List<Segment> allPolygons)
        {
            int votes = 0;
            double noResults = (double)allPolygons.Count;
            foreach (Segment seg in allPolygons)
            {
                if (seg.PointIsInSegment(x, y))
                {
                    votes++;
                }
            }
            return (double)(votes / noResults);
        }

        public class SegmentGroup
        {
            public Segment MergedSegment;
            public List<Segment> segments;
            public List<string> identifiers;

            public SegmentGroup()
            {
                segments = new List<Segment>();
                identifiers = new List<string>();
            }
        }


        /// <summary>
        /// TODO: can be generalized across multiple tasks
        /// </summary>
        /// <param name="identifiers"></param>
        /// <param name="polyList"></param>
        /// <param name="minIoUThreshold"></param>
        /// <returns></returns>
        public static List<SegmentGroup> GreedyMeanHierarchicalMerge(List<string> identifiers, 
            List<Segment> polyList, double minOverlapThreshold, 
            Dictionary<string, double> similarityMatrixAmongPartitions)
        {
            List<SegmentGroup> ret = new List<SegmentGroup>();
            for (int i = 0; i < polyList.Count; i++)
            {
                Segment b = polyList[i];
                SegmentGroup segGroup = new SegmentGroup();
                segGroup.MergedSegment = polyList[i];
                segGroup.segments.Add(b);
                segGroup.identifiers.Add(identifiers[i]);
                ret.Add(segGroup);
            }


            bool changed = true;

            while (changed)
            {
                changed = false;
                //find the closest mergable polygon group pair and merge
                int index1 = -1;
                int index2 = -1;

                double bestSimilarity = 0;

                
                for (int i = 0; i < ret.Count - 1; i++)
                {
                    string part1 = ret[i].identifiers[0].Split('_')[0];
                    for (int j = i + 1; j < ret.Count; j++)
                    {
                        string part2 = ret[j].identifiers[0].Split('_')[0];
                        string idx1 = part1 + '_' + part2;
                        if (!similarityMatrixAmongPartitions.ContainsKey(idx1))
                        {
                            continue;
                        }

                        double sim = similarityMatrixAmongPartitions[idx1];
                        if (sim >= minOverlapThreshold)
                        {
                            if (sim > bestSimilarity)
                            {
                                bestSimilarity = sim;
                                index1 = i;
                                index2 = j;
                                changed = true;
                            }
                        }
                    }
                }

                if (changed) //there is something worth merging
                {
                    //merge 2 into 1
                    for (int i = 0; i < ret[index2].segments.Count; i++)
                    {
                        ret[index1].segments.Add(ret[index2].segments[i]);
                        ret[index1].identifiers.Add(ret[index2].identifiers[i]);
                    }

                    //remove 2
                    ret.RemoveAt(index2);
                }
                if (ret.Count == 1)
                {
                    break;
                }
            } 
            return ret;
        }

        public static SegmentGroup getMajorityGroupPolygons(List<string> identifiers, List<Segment> polyList, double minIoUThreshold, Dictionary<string, double> similarityMatrixAmongPartitions)
        {
            
            if (identifiers.Count == 0) return null;

            List<SegmentGroup> SegGroups = GreedyMeanHierarchicalMerge(identifiers, polyList, minIoUThreshold, similarityMatrixAmongPartitions);

            int groupNo = -1;
            if (SegGroups.Count == 1) // only one group
            {
                groupNo = 0;
            }
            else
            {
                //first find the one with the largest number of 
                int maxCount = 0;
                for (int i = 0; i < SegGroups.Count; i++)
                {
                    SegmentGroup gr = SegGroups[i];
                    int cnt = gr.segments.Count;
                    if (cnt > maxCount)
                    {
                        maxCount = cnt;
                        groupNo = i;
                    }
                }
            }
            //foreach (string identifier in SegGroups[groupNo].identifiers)
            //{
            //    string[] fields = identifier.Split('_');
            //    majorityGroup.Add(Convert.ToInt32(fields[0]), Convert.ToInt32(fields[1]));
            //}
            return SegGroups[groupNo];
        }


        public static bool IsAcceptable(
            SatyamAggregatedResultsTableEntry aggResultEntry,
            SatyamResultsTableEntry resultEntry,
            double APPROVAL_RATIO_OF_POLYGONS_THRESHOLD = TaskConstants.IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_PAYMENT, //the person must have made at least 80% of the polygon
            double POLYGON_IOU_THRESHOLD = TaskConstants.IMAGE_SEGMENTATION_MTURK_POLYGON_IOU_THRESHOLD_FOR_PAYMENT
            )
        {

            //most boxes should be within limits 
            //most categories should be right
            SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(aggResultEntry.ResultString);
            ImageSegmentationAggregatedResult aggresult = JSonUtils.ConvertJSonToObject<ImageSegmentationAggregatedResult>(satyamAggResult.AggregatedResultString);
            SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(resultEntry.ResultString);
            ImageSegmentationResult result = JSonUtils.ConvertJSonToObject<ImageSegmentationResult>(satyamResult.TaskResult);

            if (result == null) return false;

            //first check if the number of boxes are within limit
            // the objects are dummy polygons just for counting
            int boxLimit = (int)Math.Ceiling((double)aggresult.boxesAndCategories.objects.Count * APPROVAL_RATIO_OF_POLYGONS_THRESHOLD);
            if (result.objects.Count < boxLimit)
            {
                return false;
            }

            
            //now find how many of the results match aggregated results
            int noAccepted = 0;

            byte[] resPNG = ImageSegmentationResult.PolygonResult2Bitmap(result);

            int width = -1; int height = -1;
            byte[] aggPNG = ImageUtilities.readPNGRawDataFromURL(aggresult.metaData.PNG_URL, out width, out height);

            if (resPNG.Length != aggPNG.Length)
            {
                Console.WriteLine("res and agg res are different size");
                return false;
            }

            SortedDictionary<int, SortedDictionary<int, int>> overlapping = new SortedDictionary<int, SortedDictionary<int, int>>();
            SortedDictionary<int, int> resObjectArea = new SortedDictionary<int, int>();
            SortedDictionary<int, int> aggObjectArea = new SortedDictionary<int, int>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int idx = j * width + i;
                    int resObjID = resPNG[idx];
                    int aggObjID = aggPNG[idx];
                    if (!overlapping.ContainsKey(resObjID))
                    {
                        overlapping.Add(resObjID, new SortedDictionary<int, int>());
                    }
                    if (!overlapping[resObjID].ContainsKey(aggObjID))
                    {
                        overlapping[resObjID].Add(aggObjID, 0);
                    }

                    overlapping[resObjID][aggObjID]++;

                    if (!resObjectArea.ContainsKey(resObjID))
                    {
                        resObjectArea.Add(resObjID, 0);
                    }
                    resObjectArea[resObjID]++;

                    if (!aggObjectArea.ContainsKey(aggObjID))
                    {
                        aggObjectArea.Add(aggObjID, 0);
                    }
                    aggObjectArea[aggObjID]++;
                }
            }

            foreach (int id in resObjectArea.Keys)
            {
                if (id == 0) continue;

                int maxOverlap = -1;
                int maxOverlapAggObjID = -1;


                SortedDictionary<int, int> overlapArea = overlapping[id];
                foreach (int aggid in overlapArea.Keys)
                {
                    if (aggid == 0) continue;   
                    if (overlapArea[aggid] > maxOverlap)
                    {
                        maxOverlap = overlapArea[aggid];
                        maxOverlapAggObjID = aggid;
                    }
                }

                if (maxOverlapAggObjID == -1) continue;

                double iou = (double)maxOverlap / (double)(resObjectArea[id] + aggObjectArea[maxOverlapAggObjID] - maxOverlap);

                if (iou >= POLYGON_IOU_THRESHOLD)
                {
                    noAccepted++;
                    ////now check category
                    //if (result.objects[match.elementList[0]].Category == aggresult.boxesAndCategories.objects[match.elementList[1]].Category)
                    //{
                    //    //both category and bounding box tests have passed
                        
                    //}
                }
            }
            
            

            if (noAccepted >= boxLimit)
            {
                return true;
            }

            return false;
        }
    }
}
