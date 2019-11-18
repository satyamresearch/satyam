using Constants;
using HelperClasses;
using JobTemplateClasses;
using SatyamAnalysis;
using SatyamResultAggregators;
using SatyamResultsSaving;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UsefulAlgorithms;
using Utilities;

namespace SatyamResultValidation
{
    
    public class PascalVOCSegmentationGroundTruth
    {
        byte[] instanceSegments;
        //byte[] classSegments;
        int ImageWidth;
        int ImageHeight;

        public PascalVOCSegmentationGroundTruth(string fileName)
        {
            
            string filePath = DirectoryConstants.PASCALVOCSegmentationInstanceSegments + fileName + ".PNG";
            if (!File.Exists(filePath)) {
                Console.WriteLine("file does not exist");
                return;
            }

            instanceSegments = ImageUtilities.readLocalPNGRawData(filePath, out ImageWidth, out ImageHeight);
        }

        /// <summary>
        /// get IoUs by comparing pixels
        /// 
        /// </summary>
        /// <param name="segments"></param>
        /// <returns></returns>
        public List<List<double>> getIoUs(string PNG_URL, out SortedDictionary<int, int> noDetectionPixels, out SortedDictionary<int, int> noGroundTruthPixels)
        {
            noGroundTruthPixels = new SortedDictionary<int, int>();
            noDetectionPixels = new SortedDictionary<int, int>();
            int width, height;
            byte[] segments = ImageUtilities.readPNGRawDataFromURL(PNG_URL, out width, out height);

            if (width != ImageWidth)
            {
                Console.WriteLine("Error: width different between gt and agg result");
                return null;
            }

            List<List<double>> IoUs = new List<List<double>>();

            
            Dictionary<int, Dictionary<int, int>> noOverlappingPixelsBetweenDetectionsAndGroundTruth = new Dictionary<int, Dictionary<int, int>>();

            // going thru all gt segments
            for (int i = 0; i < Math.Min(instanceSegments.Length, segments.Length); i++)
            {
                int GTObjId = -1;
                int DetObjID = -1;
                if (instanceSegments[i] == 255) continue;// edge doesn't count

                if (instanceSegments[i] < 255 && instanceSegments[i] != 0)
                {
                    GTObjId = (int)instanceSegments[i];
                    if (!noGroundTruthPixels.ContainsKey(GTObjId))
                    {
                        noGroundTruthPixels.Add(GTObjId, 0);
                        noOverlappingPixelsBetweenDetectionsAndGroundTruth.Add(GTObjId, new Dictionary<int, int>());
                    }

                    noGroundTruthPixels[GTObjId]++;
                }

                

                if (segments[i] != 0)
                {
                    DetObjID = (int)segments[i];
                    if (!noDetectionPixels.ContainsKey(DetObjID))
                    {
                        noDetectionPixels.Add(DetObjID, 0);
                    }
                    noDetectionPixels[DetObjID]++;
                    if (GTObjId != -1)
                    {
                        if (!noOverlappingPixelsBetweenDetectionsAndGroundTruth[GTObjId].ContainsKey(DetObjID))
                        {
                            noOverlappingPixelsBetweenDetectionsAndGroundTruth[GTObjId].Add(DetObjID, 0);
                        }
                        noOverlappingPixelsBetweenDetectionsAndGroundTruth[GTObjId][DetObjID]++;
                    }
                    
                }
                        
            }

            foreach (int gtId in noGroundTruthPixels.Keys)
            {
                IoUs.Add(new List<double>());
                foreach (int detId in noDetectionPixels.Keys)
                {
                    double tempIoU = 0;
                    if (noOverlappingPixelsBetweenDetectionsAndGroundTruth.ContainsKey(gtId))
                    {
                        if (noOverlappingPixelsBetweenDetectionsAndGroundTruth[gtId].ContainsKey(detId))
                        {
                            int overlapping = noOverlappingPixelsBetweenDetectionsAndGroundTruth[gtId][detId];
                            tempIoU = (double)overlapping / (double)(noGroundTruthPixels[gtId] + noDetectionPixels[detId] - overlapping);
                        }
                    }

                    IoUs[gtId - 1].Add(tempIoU);
                }
            }

            return IoUs;
        }


        public List<List<double>> getGTOverlaps(string PNG_URL, out SortedDictionary<int, int> noDetectionPixels, out SortedDictionary<int, int> noGroundTruthPixels)
        {
            noGroundTruthPixels = new SortedDictionary<int, int>();
            noDetectionPixels = new SortedDictionary<int, int>();
            int width, height;
            byte[] segments = ImageUtilities.readPNGRawDataFromURL(PNG_URL, out width, out height);

            if (width != ImageWidth)
            {
                Console.WriteLine("Error: width different between gt and agg result");
                return null;
            }

            List<List<double>> IoUs = new List<List<double>>();


            Dictionary<int, Dictionary<int, int>> noOverlappingPixelsBetweenDetectionsAndGroundTruth = new Dictionary<int, Dictionary<int, int>>();

            // going thru all gt segments
            for (int i = 0; i < Math.Min(instanceSegments.Length, segments.Length); i++)
            {
                int GTObjId = -1;
                int DetObjID = -1;
                if (instanceSegments[i] == 255) continue;// edge doesn't count

                if (instanceSegments[i] < 255 && instanceSegments[i] != 0)
                {
                    GTObjId = (int)instanceSegments[i];
                    if (!noGroundTruthPixels.ContainsKey(GTObjId))
                    {
                        noGroundTruthPixels.Add(GTObjId, 0);
                        noOverlappingPixelsBetweenDetectionsAndGroundTruth.Add(GTObjId, new Dictionary<int, int>());
                    }

                    noGroundTruthPixels[GTObjId]++;
                }



                if (segments[i] != 0)
                {
                    DetObjID = (int)segments[i];
                    if (!noDetectionPixels.ContainsKey(DetObjID))
                    {
                        noDetectionPixels.Add(DetObjID, 0);
                    }
                    noDetectionPixels[DetObjID]++;
                    if (GTObjId != -1)
                    {
                        if (!noOverlappingPixelsBetweenDetectionsAndGroundTruth[GTObjId].ContainsKey(DetObjID))
                        {
                            noOverlappingPixelsBetweenDetectionsAndGroundTruth[GTObjId].Add(DetObjID, 0);
                        }
                        noOverlappingPixelsBetweenDetectionsAndGroundTruth[GTObjId][DetObjID]++;
                    }

                }

            }

            foreach (int gtId in noGroundTruthPixels.Keys)
            {
                IoUs.Add(new List<double>());
                foreach (int detId in noDetectionPixels.Keys)
                {
                    double tempIoU = 0;
                    if (noOverlappingPixelsBetweenDetectionsAndGroundTruth.ContainsKey(gtId))
                    {
                        if (noOverlappingPixelsBetweenDetectionsAndGroundTruth[gtId].ContainsKey(detId))
                        {
                            int overlapping = noOverlappingPixelsBetweenDetectionsAndGroundTruth[gtId][detId];
                            tempIoU = (double)overlapping / (double)(noGroundTruthPixels[gtId]);
                        }
                    }

                    IoUs[gtId - 1].Add(tempIoU);
                }
            }

            return IoUs;
        }

        /// <summary>
        /// Return a Dictionary of IoUs per each object ID, starting from 1 in the groundtruth
        /// Note that gt id starts from 1
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public List<List<double>> getIoUs(ImageSegmentationResult res)
        {
            List<List<double>> IoUs = new List<List<double>>();

            SortedDictionary<int, int> noGroundTruthPixels = new SortedDictionary<int, int>();
            SortedDictionary<int, int> noDetectionPixels = new SortedDictionary<int, int>();
            Dictionary<int, Dictionary<int, int>> noOverlappingPixelsBetweenDetectionsAndGroundTruth = new Dictionary<int, Dictionary<int, int>>();

            // going thru all gt segments
            for (int i = 0; i < instanceSegments.Length; i++)
            {
                if (instanceSegments[i]<255 && instanceSegments[i] != 0)
                {
                    int objId = (int)instanceSegments[i];
                    if (!noGroundTruthPixels.ContainsKey(objId))
                    {
                        noGroundTruthPixels.Add(objId, 0);
                        noOverlappingPixelsBetweenDetectionsAndGroundTruth.Add(objId, new Dictionary<int, int>());
                    }

                    noGroundTruthPixels[objId]++;

                    // most naive approach: iterate every pixel
                    int x = i % res.imageWidth;
                    int y = i / res.imageWidth;

                    // going thru each detection polygons
                    for (int j = 0; j < res.objects.Count; j++)
                    {
                        
                        Segment seg = res.objects[j].segment;

                        if (seg.PointIsInSegment(x, y))
                        {
                            if (!noDetectionPixels.ContainsKey(j))
                            {
                                noDetectionPixels.Add(j, 0);
                            }
                            noDetectionPixels[j]++;

                            if (!noOverlappingPixelsBetweenDetectionsAndGroundTruth[objId].ContainsKey(j))
                            {
                                noOverlappingPixelsBetweenDetectionsAndGroundTruth[objId].Add(j, 0);
                            }
                            noOverlappingPixelsBetweenDetectionsAndGroundTruth[objId][j]++;
                        }
                    }
                }
            }

            foreach(int gtId in noGroundTruthPixels.Keys)
            {
                IoUs.Add(new List<double>());
                foreach (int detId in noDetectionPixels.Keys)
                {
                    double tempIoU = 0;
                    if (noOverlappingPixelsBetweenDetectionsAndGroundTruth.ContainsKey(gtId))
                    {
                        if (noOverlappingPixelsBetweenDetectionsAndGroundTruth[gtId].ContainsKey(detId))
                        {
                            int overlapping = noOverlappingPixelsBetweenDetectionsAndGroundTruth[gtId][detId];
                            tempIoU = (double)overlapping / (double)(noGroundTruthPixels[gtId] + noDetectionPixels[detId] - overlapping);
                        }
                    }

                    IoUs[gtId-1].Add(tempIoU);
                }
            }

            return IoUs;
        }


        public static void LoadAnnotations()
        {
            string folder = @"C:\research\dataset\PascalVOC\VOCtrainval_11-May-2012\VOCdevkit\VOC2012\SegmentationImages\";
            string annofolder = @"C:\research\dataset\PascalVOC\VOCtrainval_11-May-2012\VOCdevkit\VOC2012\Annotations\";

            SortedDictionary<string, SortedDictionary<string, int>> corr = new SortedDictionary<string, SortedDictionary<string, int>>();
            string[] files = Directory.GetFiles(folder);
            foreach (string file in files)
            {
                string filename = URIUtilities.filenameFromDirectoryNoExtension(file);
                string annofile = annofolder + filename + ".xml";
                XElement anno = XElement.Load(annofile);
                List<string> names = (from el in anno.Descendants("name")
                 select (string)el).ToList<string>();

                names.Sort();
                for (int i=0;i<names.Count;i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        string name1 = names[i];
                        string name2 = names[j];
                        if (!corr.ContainsKey(name1))
                        {
                            corr.Add(name1, new SortedDictionary<string, int>());
                        }
                        if (!corr[name1].ContainsKey(name2))
                        {
                            corr[name1].Add(name2, 0);
                        }
                        corr[name1][name2]++;
                    }
                }    
                
            }

            foreach(string k in corr.Keys)
            {
                string line = k + " ";
                foreach(string v in corr.Keys)
                {

                    if (corr[k].ContainsKey(v))
                    {
                        line += corr[k][v] + " ";
                    }
                    else
                    {
                        line += "0 ";
                    }
                    
                }
                Console.WriteLine(line);
            }
        }
    }


    
    public class PascalVOCImageSegmentationValidation
    {

        public static Dictionary<string, int> missingGroundTruth = new Dictionary<string, int>()
        {
            { "2007_000364",1 },
            { "2007_001340",1 },
            { "2007_002120",1 },
            { "2007_002293",1 },
            { "2007_002426",1 },
            { "2007_002470",1 },
            { "2007_002668",1 },
            { "2007_003106",1 },
            { "2007_003373",1 },
            { "2007_003506",1 },
            { "2007_003991",1 },
            { "2007_004081",1 },
            { "2007_004705",1 },
            { "2007_004998",1 },
            { "2008_002749",1 },
        };

        public static Dictionary<string, int> GroundTruthFilter = new Dictionary<string, int>()
        {
            { "2007_001763",1 },
            { "2007_002668",1 },
            { "2007_003169",1 },
            { "2007_001430",1 },
            { "2007_003742",1 },
        };

        public static void ValidatePascalVOCImageSegmentationByGUID(List<string> guids, double IoUTreshold, bool saveImage = false)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = new List<SatyamAggregatedResultsTableEntry>();

            foreach (string guid in guids)
            {
                results.AddRange(resultsDB.getEntriesByGUID(guid));
            }
            resultsDB.close();

            ValidatePascalVOCImageSegmentationResult_InstanceLevel(results, IoUTreshold);

            ValidatePascalVOCImageSegmentationResult_ClassLevel(results, new List<double>() { 0.5,0.55,0.6,0.65,0.7,0.75,0.8,0.85,0.9,0.95});
        }

        public static string ValidatePascalVOCImageSegmentationResult_InstanceLevel(
            List<SatyamAggregatedResultsTableEntry> resultsPerImage, 
            double IoUTreshold)
        {
            SortedDictionary<int, SatyamAggregatedResultsTableEntry> aggResults = new SortedDictionary<int, SatyamAggregatedResultsTableEntry>();
            for (int i = 0; i < resultsPerImage.Count; i++)
            {
                int taskID = resultsPerImage[i].SatyamTaskTableEntryID;
                aggResults.Add(taskID, resultsPerImage[i]);
            }


            int tp = 0;
            int fp = 0;
            int fn = 0;
            foreach (int id in aggResults.Keys)
            {
                
                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(aggResults[id]);
                string fileName = URIUtilities.filenameFromURINoExtension(data.SatyamURI);
                PascalVOCSegmentationGroundTruth gt = new PascalVOCSegmentationGroundTruth(fileName);

                Console.WriteLine("Results for {0}", fileName);

                String resultString = data.AggregatedResultString;
                ImageSegmentationAggregatedResult result = JSonUtils.ConvertJSonToObject<ImageSegmentationAggregatedResult>(resultString);

                string PNG_URL = result.metaData.PNG_URL;

                //List<List<double>> IoUs = gt.getIoUs(result.boxesAndCategories);
                SortedDictionary<int, int> noGroundTruthPixels = new SortedDictionary<int, int>();
                SortedDictionary<int, int> noDetectionPixels = new SortedDictionary<int, int>();

                List<List<double>> IoUs = gt.getIoUs(PNG_URL, out noDetectionPixels, out noGroundTruthPixels);


                List<int> DetectionAreas = noDetectionPixels.Values.ToList();
                List<int> GroundtruthAreas = noGroundTruthPixels.Values.ToList();
                if (IoUs == null) continue;

                MultipartiteWeightTensor matches = new MultipartiteWeightTensor(2);
                
                matches.setNumPartitionElements(0, IoUs.Count);
                matches.setNumPartitionElements(1, IoUs[0].Count);

                double[,] array = new double[IoUs.Count, IoUs[0].Count];
                for (int j = 0; j < IoUs.Count; j++)
                {
                    for (int k = 0; k < IoUs[0].Count; k++)
                    {
                        array[j, k] = IoUs[j][k];
                    }
                }

                matches.setWeightMatrix(0, 1, array);
                MultipartiteWeightedMatching.GreedyMean matching = new MultipartiteWeightedMatching.GreedyMean();
                List<MultipartiteWeightedMatch> polygonAssociation = matching.getMatching(matches);

                //double smallAreaToIgnore = 1000;//px
                //double smallAreaToIgnore = 625;//px
                double smallAreaToIgnore = 1600;//px
                foreach (MultipartiteWeightedMatch match in polygonAssociation)
                {
                    if (match.elementList.ContainsKey(0)) // this contains gt
                    {
                        if (GroundtruthAreas[match.elementList[0]] < smallAreaToIgnore) continue;
                        if (match.elementList.ContainsKey(1)) // an aggregated result box has been associated
                        {
                            if (DetectionAreas[match.elementList[1]] < smallAreaToIgnore) continue;

                            double IoU = IoUs[match.elementList[0]][match.elementList[1]];
                            Console.WriteLine("Match: {0}", IoU);
                            if (IoU >= IoUTreshold)
                            {
                                tp++;

                            }
                            else
                            {
                                
                                fp++;
                                fn++;
                            }
                        }
                        else
                        {
                            Console.WriteLine("No Match");
                            fn++;
                        }
                    }
                    else
                    {
                        if (DetectionAreas[match.elementList[1]] < smallAreaToIgnore) continue;
                        Console.WriteLine("No Groundtruth");
                        fp++;
                    }
                }
            }

            double ap = (double)tp / (double)(tp + fp + fn);
            double prec = (double)tp / (double)(tp + fp);
            double recl = (double)tp / (double)(tp + fn);
            string ret = String.Format("TP: {0}, FP: {1}, FN {2}, AP, {3}, Precision, {4}, Recall, {5}", tp, fp, fn, ap, prec, recl);
            Console.WriteLine(ret);
            return ret;
        }



        public static void ValidatePascalVOCImageSegmentationResult_ClassLevel(
            List<SatyamAggregatedResultsTableEntry> resultsPerImage,
            List<double> IoUTresholds)
        {
            SortedDictionary<int, SatyamAggregatedResultsTableEntry> aggResults = new SortedDictionary<int, SatyamAggregatedResultsTableEntry>();
            for (int i = 0; i < resultsPerImage.Count; i++)
            {
                int taskID = resultsPerImage[i].SatyamTaskTableEntryID;
                aggResults.Add(taskID, resultsPerImage[i]);
            }
            

            SortedDictionary<double, int> tpPerIoU = new SortedDictionary<double, int>();
            SortedDictionary<double, int> fpPerIoU = new SortedDictionary<double, int>();
            SortedDictionary<double, int> fnPerIoU = new SortedDictionary<double, int>();

            foreach (double iou in IoUTresholds)
            {
                tpPerIoU.Add(iou, 0);
                fpPerIoU.Add(iou, 0);
                fnPerIoU.Add(iou, 0);
            }

            foreach (int id in aggResults.Keys)
            {

                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(aggResults[id]);
                string fileName = URIUtilities.filenameFromURINoExtension(data.SatyamURI);
                PascalVOCSegmentationGroundTruth gt = new PascalVOCSegmentationGroundTruth(fileName);

                Console.WriteLine("Results for {0}", fileName);

                String resultString = data.AggregatedResultString;
                ImageSegmentationAggregatedResult result = JSonUtils.ConvertJSonToObject<ImageSegmentationAggregatedResult>(resultString);

                string PNG_URL = result.metaData.PNG_URL;

                //List<List<double>> IoUs = gt.getIoUs(result.boxesAndCategories);
                SortedDictionary<int, int> noGroundTruthPixels = new SortedDictionary<int, int>();
                SortedDictionary<int, int> noDetectionPixels = new SortedDictionary<int, int>();
                //List<List<double>> IoUs = gt.getIoUs(PNG_URL, out noDetectionPixels, out noGroundTruthPixels);
                List<List<double>> IoUs = gt.getGTOverlaps(PNG_URL, out noDetectionPixels, out noGroundTruthPixels);

                List<int> DetectionAreas = noDetectionPixels.Values.ToList();
                List<int> GroundtruthAreas = noGroundTruthPixels.Values.ToList();
                if (IoUs == null) continue;

                
                //double smallAreaToIgnore = 1000;//px
                //double smallAreaToIgnore = 625;//px
                double smallAreaToIgnore = 1600;//px

                List<int> matchedDetections = new List<int>();
                for ( int i=0;i<IoUs.Count;i++)
                {
                    if (GroundtruthAreas[i] < smallAreaToIgnore) continue;
                    List<double> ious = IoUs[i];
                    double maxIoU = 0;
                    int idx = -1;
                    for ( int j=0;j<ious.Count;j++)
                    {
                        if (ious[j] > maxIoU)
                        {
                            maxIoU = ious[j];
                            idx = j;
                        }
                    }

                    if (maxIoU == 0)
                    {
                        Console.WriteLine("No Match");

                        foreach(double iou in IoUTresholds)
                        {
                            fnPerIoU[iou]++;
                        }
                        continue;
                    }

                    if (DetectionAreas[idx] < smallAreaToIgnore) continue;

                    
                    Console.WriteLine("Match: {0}", maxIoU);
                    matchedDetections.Add(idx);

                    foreach ( double iou in IoUTresholds)
                    {
                        if (maxIoU >= iou)
                        {
                            tpPerIoU[iou]++;
                        }
                        else
                        {
                            
                            fpPerIoU[iou]++;
                            fnPerIoU[iou]++;
                        }
                    }
                    
                }

                for (int j = 0; j < IoUs[0].Count; j++)
                {
                    if (matchedDetections.Contains(j)) continue;
                    if (DetectionAreas[j] < smallAreaToIgnore) continue;

                    double maxIoU = 0;
                    int idx = -1;
                    for ( int i = 0; i < IoUs.Count; i++)
                    {
                        if (IoUs[i][j] > maxIoU)
                        {
                            maxIoU = IoUs[i][j];
                            idx = j;
                        }                        
                    }

                    bool alreadyCounted = false;
                    foreach(double iou in IoUTresholds)
                    {
                        if (maxIoU >= iou)
                        {
                            alreadyCounted = true;break;
                        }
                    }

                    if (alreadyCounted) continue;

                    Console.WriteLine("No Groundtruth");
                  
                    foreach(double iou in IoUTresholds)
                    {
                        fpPerIoU[iou]++;
                    }
                    
                }
            }
            double AvgPrec = 0;
            foreach (double iou in IoUTresholds)
            {
                int tp = tpPerIoU[iou] + missingGroundTruth.Count;
                int fp = fpPerIoU[iou] - missingGroundTruth.Count;
                int fn = fnPerIoU[iou] - GroundTruthFilter.Count;
                double ap = (double)tp / (double)(tp + fp + fn);
                double prec = (double)tp / (double)(tp + fp);
                double recl = (double)tp / (double)(tp + fn);
                string ret = String.Format("TP: {0}, FP: {1}, FN {2}, AP, {3}, Precision, {4}, Recall, {5}", tp, fp, fn, ap, prec, recl);
                Console.WriteLine(ret);
                AvgPrec += prec;
            }
            AvgPrec /= IoUTresholds.Count;
            Console.WriteLine("AvgPrec :{0}", AvgPrec);

        }


        public static void StaticOfflineAggregationWithParameterAndValidation(List<string> guids,
            double IoUTreshold,
            int MinResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double CategoryMajorityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAJORITY_CATEGORY_THRESHOLD,
            double PolygonBoundaryMajorityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAJORITY_POLYGON_BOUNDARY_THRESHOLD,
            double ObjectsCoverageThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_AGGREGATION_TERMINATION,
            double minSimilarityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_POLYGON_IOU_THRESHOLD,
            int minResultsForConsensus = TaskConstants.IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_FOR_CONSENSUS)
        {
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();

            Dictionary<int, List<string>> WorkersPerTask = new Dictionary<int, List<string>>();

            List<SatyamResultsTableEntry> entries = new List<SatyamResultsTableEntry>();
            foreach (string guid in guids)
            {
                entries.AddRange(resultsDB.getEntriesByGUIDOrderByID(guid));
            }
            resultsDB.close();

            SortedDictionary<int, List<SatyamResultsTableEntry>> EntriesPerTask = new SortedDictionary<int, List<SatyamResultsTableEntry>>();
            SortedDictionary<int, List<ImageSegmentationResult>> ResultsPerTask = new SortedDictionary<int, List<ImageSegmentationResult>>();
            List<int> aggregatedTasks = new List<int>();

            int noTotalConverged = 0;
            //int noCorrect = 0;
            int noTerminatedTasks = 0;

            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();
            // Organize by taskID
            foreach (SatyamResultsTableEntry entry in entries)
            {
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                int taskEntryID = entry.SatyamTaskTableEntryID;
                
                if (!EntriesPerTask.ContainsKey(taskEntryID))
                {
                    EntriesPerTask.Add(taskEntryID, new List<SatyamResultsTableEntry>());
                    ResultsPerTask.Add(taskEntryID, new List<ImageSegmentationResult>());
                }
                EntriesPerTask[taskEntryID].Add(entry);
                ResultsPerTask[taskEntryID].Add(JSonUtils.ConvertJSonToObject<ImageSegmentationResult>(satyamResult.TaskResult));
            }
            foreach (int taskEntryID in EntriesPerTask.Keys)
            {
                
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(EntriesPerTask[taskEntryID][0].ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                SatyamJob job = task.jobEntry;
                string fileName = URIUtilities.filenameFromURINoExtension(task.SatyamURI);

                List<int> taskidfilter = new List<int>()
                {
                    40430,
                    40432,
                    40433,
                    40434,
                    40440,
                    40447,
                    40451,
                    40460,
                };

                //if (fileName != "2007_000549") continue;

                if (!taskidfilter.Contains(satyamResult.TaskTableEntryID)) continue;

                Console.WriteLine("Aggregating task {0}: {1} results", taskEntryID, EntriesPerTask[taskEntryID].Count);
                ImageSegmentationAggregatedResult aggResult = ImageSegmentationAggregator.getAggregatedResult(
                    ResultsPerTask[taskEntryID], task.SatyamURI, job.JobGUIDString, 
                    MinResults, MaxResults, CategoryMajorityThreshold, 
                    PolygonBoundaryMajorityThreshold, ObjectsCoverageThreshold, 
                    minSimilarityThreshold, minResultsForConsensus);
                if (aggResult == null)
                {
                    continue;
                }

                // aggregation happen
                    
                aggregatedTasks.Add(taskEntryID);
                noTotalConverged++;
                if (ResultsPerTask[taskEntryID].Count >= MaxResults)
                {
                    noTerminatedTasks++;
                }
                SatyamAggregatedResult SatyamAggResult = new SatyamAggregatedResult();
                SatyamAggResult.SatyamTaskTableEntryID = taskEntryID;
                SatyamAggResult.AggregatedResultString = JSonUtils.ConvertObjectToJSon<ImageSegmentationAggregatedResult>(aggResult);
                SatyamAggResult.TaskParameters = satyamResult.TaskParametersString;

                SatyamAggregatedResultsTableEntry aggEntry = new SatyamAggregatedResultsTableEntry();
                aggEntry.SatyamTaskTableEntryID = taskEntryID;
                aggEntry.JobGUID = job.JobGUIDString;
                aggEntry.ResultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(SatyamAggResult);


                aggEntries.Add(aggEntry);
                List<SatyamAggregatedResultsTableEntry> tmpEntries = new List<SatyamAggregatedResultsTableEntry>();
                tmpEntries.Add(aggEntry);

                ValidatePascalVOCImageSegmentationResult_InstanceLevel(tmpEntries, IoUTreshold);


            }
            Console.WriteLine("Total_Aggregated_Tasks: {0}", noTotalConverged);
            Console.WriteLine("Total_Terminated_Tasks: {0}", noTerminatedTasks);

            string r = ValidatePascalVOCImageSegmentationResult_InstanceLevel(aggEntries, IoUTreshold);
        }


        /// <summary>
        /// function generalizable.. TODO... to all tasks
        /// </summary>
        /// <param name="guids"></param>
        /// <param name="IoUTreshold"></param>
        /// <param name="saveImage"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="MinResults"></param>
        /// <param name="MaxResults"></param>
        /// <param name="CategoryMajorityThreshold"></param>
        /// <param name="PolygonBoundaryMajorityThreshold"></param>
        /// <param name="ObjectsCoverageThreshold"></param>
        /// <param name="overwrite"></param>
        /// <param name="approvalAnalysis"></param>
        public static void AggregateWithParameterAndValidatePascalVOCImageSegmentationByGUID(List<string> guids,
            double IoUTreshold,
            bool saveImage = false, string outputDirectory = null,
            int MinResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double CategoryMajorityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAJORITY_CATEGORY_THRESHOLD,
            double PolygonBoundaryMajorityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAJORITY_POLYGON_BOUNDARY_THRESHOLD,
            double ObjectsCoverageThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_AGGREGATION_TERMINATION,
            double minSimilarityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_POLYGON_IOU_THRESHOLD,
            int minResultsForConsensus = TaskConstants.IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_FOR_CONSENSUS,
            bool overwrite = false,
            bool approvalAnalysis = false)
        {

        
            string configString = "Min_" + MinResults + "_Max_" + MaxResults + "_Majority_" + PolygonBoundaryMajorityThreshold + "_Ratio_" + ObjectsCoverageThreshold;
            Console.WriteLine("Aggregating for param set " + configString);
            if (!overwrite && File.Exists(DirectoryConstants.defaultTempDirectory + "\\ImageSegmentationResult\\" + configString + ".txt"))
            {
                return;
            }

            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();

            Dictionary<int, List<string>> WorkersPerTask = new Dictionary<int, List<string>>();

            List<SatyamResultsTableEntry> entries = new List<SatyamResultsTableEntry>();
            foreach (string guid in guids)
            {
                entries.AddRange(resultsDB.getEntriesByGUIDOrderByID(guid));
            }
            resultsDB.close();

            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime =
                SatyamResultValidationToolKit.SortResultsBySubmitTime_OneResultPerTurkerPerTask(entries);

            Dictionary<int, List<ImageSegmentationResult>> ResultsPerTask = new Dictionary<int, List<ImageSegmentationResult>>();
            List<int> aggregatedTasks = new List<int>();

            int noTotalConverged = 0;
            //int noCorrect = 0;
            int noTerminatedTasks = 0;

            List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();
            Dictionary<int, int> noResultsNeededForAggregation = SatyamResultsAnalysis.getNoResultsNeededForAggregationFromLog(configString, guids[0]);
            Dictionary<int, int> noResultsNeededForAggregation_new = new Dictionary<int, int>();
            // play back by time
            foreach (DateTime t in entriesBySubmitTime.Keys)
            {
                //Console.WriteLine("Processing Results of time: {0}", t);
                List<SatyamResultsTableEntry> ResultEntries = entriesBySubmitTime[t];
                foreach (SatyamResultsTableEntry entry in ResultEntries)
                {
                    SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                    SatyamJob job = task.jobEntry;
                    string fileName = URIUtilities.filenameFromURINoExtension(task.SatyamURI);
                    int taskEntryID = entry.SatyamTaskTableEntryID;
                    if (aggregatedTasks.Contains(taskEntryID))
                    {
                        continue;
                    }
                    if (!ResultsPerTask.ContainsKey(taskEntryID))
                    {
                        ResultsPerTask.Add(taskEntryID, new List<ImageSegmentationResult>());
                    }

                    ResultsPerTask[taskEntryID].Add(JSonUtils.ConvertJSonToObject<ImageSegmentationResult>(satyamResult.TaskResult));

                    // check log if enough results are collected
                    if (noResultsNeededForAggregation != null && noResultsNeededForAggregation.ContainsKey(taskEntryID)
                        && ResultsPerTask[taskEntryID].Count < noResultsNeededForAggregation[taskEntryID])
                    {
                        continue;
                    }

                    ImageSegmentationAggregatedResult aggResult = ImageSegmentationAggregator.getAggregatedResult(
                        ResultsPerTask[taskEntryID], task.SatyamURI,entry.JobGUID, 
                        MinResults, MaxResults, CategoryMajorityThreshold, 
                        PolygonBoundaryMajorityThreshold,ObjectsCoverageThreshold, 
                        minSimilarityThreshold, minResultsForConsensus);
                    if (aggResult == null)
                    {
                        continue;
                    }

                    // aggregation happen
                    // record logs
                    if (noResultsNeededForAggregation == null || !noResultsNeededForAggregation.ContainsKey(taskEntryID))
                    {
                        noResultsNeededForAggregation_new.Add(taskEntryID, ResultsPerTask[taskEntryID].Count);
                    }
                    aggregatedTasks.Add(taskEntryID);
                    noTotalConverged++;
                    if (ResultsPerTask[taskEntryID].Count >= MaxResults)
                    {
                        noTerminatedTasks++;
                    }
                    SatyamAggregatedResult SatyamAggResult = new SatyamAggregatedResult();
                    SatyamAggResult.SatyamTaskTableEntryID = taskEntryID;
                    SatyamAggResult.AggregatedResultString = JSonUtils.ConvertObjectToJSon<ImageSegmentationAggregatedResult>(aggResult);
                    SatyamAggResult.TaskParameters = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString).TaskParametersString;

                    SatyamAggregatedResultsTableEntry aggEntry = new SatyamAggregatedResultsTableEntry();
                    aggEntry.SatyamTaskTableEntryID = taskEntryID;
                    aggEntry.JobGUID = entry.JobGUID;
                    aggEntry.ResultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(SatyamAggResult);


                    aggEntries.Add(aggEntry);
                    List<SatyamAggregatedResultsTableEntry> tmpEntries = new List<SatyamAggregatedResultsTableEntry>();
                    tmpEntries.Add(aggEntry);

                    //ValidateSatyamKITTIDetectionAggregationResult(tmpEntries, saveImage, MinHeight, MaxOcclusion, Max_Truncation);

                }
            }
            Console.WriteLine("Total_Aggregated_Tasks: {0}", noTotalConverged);
            Console.WriteLine("Total_Terminated_Tasks: {0}", noTerminatedTasks);

            SatyamResultsAnalysis.RecordAggregationLog(noResultsNeededForAggregation_new, configString, guids[0]);

            string r = ValidatePascalVOCImageSegmentationResult_InstanceLevel(aggEntries, IoUTreshold);

            r = noTotalConverged + " " + noTerminatedTasks + " " + r;
            File.WriteAllText(DirectoryConstants.defaultTempDirectory + "\\ImageSegmentationResult\\" + configString + ".txt", r);

            
            if (approvalAnalysis)
            {
                string approvalString = configString + "_PayCover_" + TaskConstants.IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_PAYMENT +
                "_PayIoU_" + TaskConstants.IMAGE_SEGMENTATION_MTURK_POLYGON_IOU_THRESHOLD_FOR_PAYMENT;
                //for (double ratio = 0; ratio < 1; ratio += 0.2)
                //{
                //    SatyamResultsAnalysis.AnalyzeApprovalRate(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guids[0], configString, approvalRatioThreshold: ratio);
                //}
                SatyamResultsAnalysis.AggregationAnalysis(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guids[0], configString);
            }

        }
    }
}
