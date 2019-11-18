using AzureBlobStorage;
using Constants;
using HelperClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UsefulAlgorithms;
using Utilities;

namespace SatyamResultAggregators
{
    

    public class ImageSegmentationAggregatedResultMetaData_NoHoles
    {
        public int TotalCount;
        public string PNG_URL;
    }

    public class ImageSegmentationAggregatedResult_NoHoles
    {
        public ImageSegmentationResult_NoHoles boxesAndCategories;
        public ImageSegmentationAggregatedResultMetaData_NoHoles metaData;
    }

    public static class ImageSegmentationAggregator_NoHoles
    {

        public static ImageSegmentationAggregatedResult_NoHoles getAggregatedResult(List<ImageSegmentationResult_NoHoles> results,
            string SatyamURL,
            string guid,
            int MinResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double CategoryMajorityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAJORITY_CATEGORY_THRESHOLD,
            double PolygonBoundaryMajorityThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_MAJORITY_POLYGON_BOUNDARY_THRESHOLD,
            double ObjectsCoverageThreshold = TaskConstants.IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_AGGREGATION_TERMINATION
            )
        {
            if (results.Count < MinResults) //need at least three results!
            {
                return null;
            }

            int ImageWidth = results[0].imageWidth;
            int ImageHeight = results[0].imageHeight;
            byte[] PNG = new byte[ImageHeight * ImageWidth];


            ImageSegmentationAggregatedResult_NoHoles aggResult = new ImageSegmentationAggregatedResult_NoHoles();
            aggResult.metaData = new ImageSegmentationAggregatedResultMetaData_NoHoles();
            aggResult.metaData.TotalCount = results.Count;
            
            for (int i=0;i<PNG.Length;i++)
            {
                PNG[i] = 0;
            }

            aggResult.boxesAndCategories = new ImageSegmentationResult_NoHoles();
            aggResult.boxesAndCategories.objects = new List<ImageSegmentationResultSingleEntry_NoHoles>();
            aggResult.boxesAndCategories.displayScaleReductionX = results[0].displayScaleReductionX;
            aggResult.boxesAndCategories.displayScaleReductionY = results[0].displayScaleReductionY;
            aggResult.boxesAndCategories.imageHeight = ImageHeight;
            aggResult.boxesAndCategories.imageWidth = ImageWidth;

            //first use multipartitie wieghted matching to associated the boxes disregarding the labels since
            //people might make mistake with lables but boxes are usually right
            List<List<GenericPolygon>> AllPolygons = new List<List<GenericPolygon>>();
            List<int> noPolygonsPerResult = new List<int>();
            
            foreach (ImageSegmentationResult_NoHoles res in results)
            {
                AllPolygons.Add(new List<GenericPolygon>());

                if (res == null)
                {
                    noPolygonsPerResult.Add(0);
                    continue;
                }

                List<GenericPolygon> polygonList = AllPolygons[AllPolygons.Count - 1];

                foreach (ImageSegmentationResultSingleEntry_NoHoles entry in res.objects)
                {
                    polygonList.Add(entry.polygon);
                }
                noPolygonsPerResult.Add(polygonList.Count);
            }
            //now associate boxes across the various results
            List<MultipartiteWeightedMatch> polyAssociation = PolygonAssociation.computeGenericPolygonAssociations(AllPolygons);
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
                double ratio = (double)noMultipleAssociatedPolygonsPerResult[i]  / (double)noPolygonsPerResult[i];
                if (ratio > ObjectsCoverageThreshold)
                {
                    noHighAssociationRatio++;
                }
            }
            if (noHighAssociationRatio < MinResults) //at least three people should have all their boxes highly corroborated by one other person
            {
                return null;
            }

            //int noAcceptedPolygons = 0;

            for (int idx = 0;idx< polyAssociation.Count;idx++)
            {
                MultipartiteWeightedMatch match = polyAssociation[idx];
                List<GenericPolygon> polyList = new List<GenericPolygon>();
                List<string> identifiers = new List<string>();
                foreach (KeyValuePair<int, int> entry in match.elementList)
                {
                    polyList.Add(AllPolygons[entry.Key][entry.Value]);
                    identifiers.Add(entry.Key + "_" + entry.Value);
                }

                //GenericPolygon aggregatedPolygon = GetAggregatedGenericPolygon_Relaxation(polyList, ImageWidth, ImageHeight, PolygonBoundaryMajorityThreshold);
                //GenericPolygon aggregatedPolygon = GetAggregatedGenericPolygon_MajorityEdge(polyList, ImageWidth, ImageHeight, PolygonBoundaryMajorityThreshold);

                GenericPolygon aggregatedPolygon = new GenericPolygon();// dummy polygon
                byte[] png = GetAggregatedGenericPolygon_PixelSweep(polyList, ImageWidth, ImageHeight, PolygonBoundaryMajorityThreshold);


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
                ImageSegmentationResultSingleEntry_NoHoles aggregated = new ImageSegmentationResultSingleEntry_NoHoles();
                aggregated.polygon = aggregatedPolygon;
                aggregated.Category = maxCategory;

                aggResult.boxesAndCategories.objects.Add(aggregated);
                for (int i = 0; i < PNG.Length; i++)
                {
                    if (PNG[i] == 0 && png[i] != 0)
                    {
                        PNG[i] = (byte)(idx+1);
                    }
                }
            }

            // save and upload to azure
            string filename = URIUtilities.filenameFromURINoExtension(SatyamURL);
            string filepath = DirectoryConstants.defaultTempDirectory + filename + "_aggregated.PNG";
            ImageUtilities.savePNGRawData(filepath, ImageWidth,ImageHeight, PNG);

            SatyamJobStorageAccountAccess blob = new SatyamJobStorageAccountAccess();
            string container = SatyamTaskGenerator.JobTemplateToSatyamContainerNameMap[TaskConstants.Segmentation_Image_MTurk];
            string directoryPath = guid + "_aggregated";
            blob.UploadALocalFile(filepath, container, directoryPath);

            aggResult.metaData.PNG_URL = TaskConstants.AzureBlobURL + container + "/" + directoryPath + "/" + filename + "_aggregated.PNG";
            return aggResult;
        }

        public static string GetAggregatedResultString(List<SatyamResultsTableEntry> results)
        {
            if (results.Count == 0) return null;

            string resultString = null;
            List<ImageSegmentationResult_NoHoles> resultList = new List<ImageSegmentationResult_NoHoles>();
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
                //enclose only non-duplicate results, one per each worker.
                if (workerID!="" && workerID != TaskConstants.AdminID)
                {
                    // make a pass for test and admins
                    WorkersPerTask.Add(workerID);
                }
                

                ImageSegmentationResult_NoHoles taskr = JSonUtils.ConvertJSonToObject<ImageSegmentationResult_NoHoles>(res.TaskResult);
                resultList.Add(taskr);
            }


            ImageSegmentationAggregatedResult_NoHoles r = getAggregatedResult(resultList, SatyamURL, guid);
            if (r != null)
            {
                string rString = JSonUtils.ConvertObjectToJSon<ImageSegmentationAggregatedResult_NoHoles>(r);
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
            List<GenericPolygon> polyList, int ImageWidth, int ImageHeight,
            double PolygonBoundaryMajorityThreshold)
        {
            byte[] pixels = new byte[ImageWidth*ImageHeight];
            for (int i = 0; i < ImageWidth; i++)
            {
                for (int j = 0; j < ImageHeight; j++)
                {
                    double ratio = getInPolygonVotesRatio(i, j, polyList);
                    if (ratio > PolygonBoundaryMajorityThreshold)
                    {
                        //pixels[j*ImageHeight+i] = 255;
                        pixels[j * ImageWidth+ i] = 255;
                    }
                    else
                    {
                        pixels[j * ImageWidth + i] = 0;
                    }
                }
            }
            return pixels;
        }

        public class eventPoint
        {
            public int x;
            public int y;
            public List<int> belongingPolygonID;
            public List<int[]> neightbors;

            public eventPoint(int xx, int yy)
            {
                x = xx;
                y = yy;
                belongingPolygonID = new List<int>();
                neightbors = new List<int[]>();
            }

            public eventPoint(int[] xy)
            {
                x = xy[0];
                y = xy[1];
                belongingPolygonID = new List<int>();
                neightbors = new List<int[]>();
            }
            public bool IsSamePoint(eventPoint ep)
            {
                return (x == ep.x && y == ep.y);
            }

            public void removeNeighbor(int nx, int ny)
            {
                List<int[]> newNeighbors = new List<int[]>();
                foreach (int[] n in neightbors)
                {
                    if (n[0]!= nx || n[1] != ny)
                    {
                        newNeighbors.Add(new int[] { n[0], n[1] });
                    }
                }
                neightbors.Clear();
                neightbors = newNeighbors;
            }
        }

        public static GenericPolygon GetAggregatedGenericPolygon_MajorityEdge(
            List<GenericPolygon> polyList, int ImageWidth, int ImageHeight, 
            double PolygonBoundaryMajorityThreshold)
        {
            if (polyList.Count == 0) return null;

            int MinMajorityCount = (int)((double)polyList.Count * PolygonBoundaryMajorityThreshold);

            //List<eventPoint> intersectingPoints = new List<eventPoint>();

            //List<eventPoint> polygonPoints = new List<eventPoint>();

            List<eventPoint> allEventPoints = new List<eventPoint>();

            


            // add all intersecting points (must not be the existing point on any polygon) of all polygons to eventpoints set
            for (int i = 0; i < polyList.Count - 1; i++)
            {
                for (int j = i + 1; j < polyList.Count; j++)
                {
                    // go thru every pair of polygons
                    GenericPolygon poly1 = polyList[i];
                    GenericPolygon poly2 = polyList[j];
                    for (int k = 0; k < poly1.vertices.Count; k++)
                    {
                        int p = (k + 1) % poly1.vertices.Count;
                        int x1 = poly1.vertices[k][0];
                        int y1 = poly1.vertices[k][1];
                        int x2 = poly1.vertices[p][0];
                        int y2 = poly1.vertices[p][1];

                        List<int[]> points2Add_poly1 = new List<int[]>();
                        for (int q = 0; q < poly2.vertices.Count; q++)
                        {
                            // for each line there could be at most one point for poly2
                            int m = (q + 1) % poly2.vertices.Count;
                            int xx1 = poly2.vertices[q][0];
                            int yy1 = poly2.vertices[q][1];
                            int xx2 = poly2.vertices[m][0];
                            int yy2 = poly2.vertices[m][1];

                            double [] intersectCoords = new double[] { -1,-1};
                            if (LineSegment.Intersect(x1, y1, x2, y2, xx1, yy1, xx2, yy2,out intersectCoords))
                            {
                                int[] intersect = new int[] { (int)intersectCoords[0], (int)intersectCoords[1] };
                                if ((intersect[0]!= xx1 || intersect[1] != yy1)  && (intersect[0] != xx2 || intersect[1] != yy2))
                                {
                                    Console.WriteLine("inserting ({0},{1}) in between  ({2},{3})[{7}] and ({4},{5})[{8}], on poly {6}", intersect[0], intersect[1], polyList[j].vertices[q][0], polyList[j].vertices[q][1], polyList[j].vertices[m][0], polyList[j].vertices[m][1], j, q,m);
                                    polyList[j].vertices.Insert(m, intersect);
                                    q++;
                                }
                                if ((intersect[0] != x1 || intersect[1] != y1) && (intersect[0] != x2  || intersect[1] != y2))
                                {
                                    bool exist = false;
                                    foreach(int[] xy in points2Add_poly1)
                                    {
                                        if (xy[0] == intersect[0] && xy[1]==intersect[1])
                                        {
                                            exist = true;
                                            break;
                                        }
                                    }
                                    if (!exist)
                                    {
                                        points2Add_poly1.Add(intersect);
                                    }
                                }
                            }
                        }

                        // insert the intersection point into the polygon
                        bool DescendSort = false;
                        if (x1 < x2)
                        {
                            DescendSort = true;
                        }
                        else
                        {
                            if (x1 > x2)
                            {
                            }
                            else
                            {
                                if (y1 < y2)
                                {
                                    DescendSort = true;
                                }
                                else
                                {
                                }
                            }
                        }


                        if (!DescendSort)
                        {
                            points2Add_poly1.Sort(delegate (int[] c1, int[] c2) {
                                if (c1[0] != c2[0])
                                {
                                    return c1[0].CompareTo(c2[0]);
                                }
                                else
                                {
                                    return c1[1].CompareTo(c2[1]);
                                }
                            });
                        }
                        else
                        {
                            points2Add_poly1.Sort(delegate (int[] c1, int[] c2) {
                                if (c1[0] != c2[0])
                                {
                                    return c2[0].CompareTo(c1[0]);
                                }
                                else
                                {
                                    return c2[1].CompareTo(c1[1]);
                                }
                            });
                        }
                        

                        foreach (int[] xy in points2Add_poly1)
                        {                            
                            Console.WriteLine("inserting ({0},{1}) in between ({2},{3})[{7}] and ({4},{5})[{8}], on poly {6}", xy[0], xy[1], polyList[i].vertices[k][0], polyList[i].vertices[k][1], polyList[i].vertices[p][0], polyList[i].vertices[p][1], i, k,p);
                            polyList[i].vertices.Insert(p, xy);
                            
                        }
                        k+=points2Add_poly1.Count;

                    }
                }
            }
            // add all points of all polygons to polygonpoints set
            for (int i = 0; i < polyList.Count; i++)
            {
                for (int j = 0; j < polyList[i].vertices.Count; j++)
                {
                    int[] xy = polyList[i].vertices[j];

                    eventPoint ep = new eventPoint(xy);
                    // check if the point exist
                    bool exist = false;
                    foreach (eventPoint eep in allEventPoints)
                    {
                        if (eep.IsSamePoint(ep))
                        {
                            ep = eep;
                            exist = true;
                            break;
                        }
                    }

                    // for the following, it doesn't matter if duplicate    
                    ep.belongingPolygonID.Add(i);
                    int next = (j + 1) % polyList[i].vertices.Count;
                    int prev = (j - 1 + polyList[i].vertices.Count) % polyList[i].vertices.Count;
                    ep.neightbors.Add(polyList[i].vertices[next]);
                    ep.neightbors.Add(polyList[i].vertices[prev]);

                    if (!exist)
                    {
                        //polygonPoints.Add(ep);
                        allEventPoints.Add(ep);
                    }
                    else
                    {
                        Console.WriteLine("adding to existing point ({0},{1}) from poly {2}, index {3}", ep.x, ep.y, i, j);
                    }
                }
            }

            //// add all intersecting points (must not be the existing point on any polygon) of all polygons to eventpoints set
            //for (int i = 0; i < polyList.Count-1; i++)
            //{
            //    for (int j = i+1; j < polyList.Count; j++)
            //    {
            //        // go thru every pair of polygons
            //        GenericPolygon poly1 = polyList[i];
            //        GenericPolygon poly2 = polyList[j];
            //        for(int k = 0; k < poly1.vertices.Count; k++)
            //        {
            //            int p = (k + 1) % poly1.vertices.Count;
            //            int x1 = poly1.vertices[k][0];
            //            int y1 = poly1.vertices[k][1];
            //            int x2 = poly1.vertices[p][0];
            //            int y2 = poly1.vertices[p][1];
            //            List<int[]> intersectingLineSegments = new List<int[]>();
            //            List<int[]> points = poly2.getAllIntersectionPointsOfLineSegment(x1, y1, x2, y2, out intersectingLineSegments);

            //            // insert the intersection point into the polygon
                       


            //            for (int q=0;q<points.Count;q++)
            //            {
            //                int[] xy = points[q];
            //                eventPoint ep = new eventPoint(xy);

            //                bool IsPolygonPoint = false;
            //                foreach (eventPoint eep in polygonPoints)
            //                {
            //                    if (eep.IsSamePoint(ep))
            //                    {
            //                        IsPolygonPoint = true;
            //                        break;
            //                    }

            //                }

            //                if (IsPolygonPoint) continue;
                            
            //                // check if the point exist already
            //                bool exist = false;
            //                foreach (eventPoint eep in intersectingPoints)
            //                {
            //                    if (eep.IsSamePoint(ep))
            //                    {
            //                        ep = eep;
            //                        exist = true;
            //                        break;
            //                    }
            //                }

            //                ep.belongingPolygonID.Add(i);
            //                ep.belongingPolygonID.Add(j);

            //                ep.neightbors.Add(new int[] { x1, y1 });
            //                ep.neightbors.Add(new int[] { x2, y2 });
            //                int[] neighbor1 = new int[] { intersectingLineSegments[q][0], intersectingLineSegments[q][1] };
            //                int[] neighbor2 = new int[] { intersectingLineSegments[q][2], intersectingLineSegments[q][3] };
            //                ep.neightbors.Add(neighbor1);
            //                ep.neightbors.Add(neighbor2);

            //                if (!exist)
            //                {
            //                    intersectingPoints.Add(ep);
            //                    allEventPoints.Add(ep);
            //                }

            //                eventPoint n1 = new eventPoint(neighbor1);
            //                eventPoint n2 = new eventPoint(neighbor2);
            //                eventPoint n3 = new eventPoint(x1, y1);
            //                eventPoint n4 = new eventPoint(x2, y2);
            //                // add this point to the neighbor list of neighbors as well
            //                // break the intersecting line segment by removing the other end from the neighbor list
            //                foreach (eventPoint eep in allEventPoints)
            //                {
            //                    if (eep.IsSamePoint(n1) || eep.IsSamePoint(n2) || eep.IsSamePoint(n3) || eep.IsSamePoint(n4))
            //                    {
            //                        eep.neightbors.Add(xy);
            //                    }

            //                    if (eep.IsSamePoint(n1)) { eep.removeNeighbor(n2.x, n2.y); }
            //                    if (eep.IsSamePoint(n2)) { eep.removeNeighbor(n1.x, n1.y); }
            //                    if (eep.IsSamePoint(n3)) { eep.removeNeighbor(n4.x, n4.y); }
            //                    if (eep.IsSamePoint(n4)) { eep.removeNeighbor(n3.x, n3.y); }
            //                }
            //            }
            //        }
            //    }
            //}
            //// sort by y to sweep line top town
            //eventPoints.Sort(delegate (eventPoint c1, eventPoint c2) { return c1.y.CompareTo(c2.y); });


            //// record both a list of left facing edges and list of right facing edges
            //List<List<int[]>> leftFacingEdges = new List<List<int[]>>();
            //List<List<int[]>> rightFacingEdges = new List<List<int[]>>();

            //List<int> ThresholdEdgePointsIndex = new List<int>();
            List<eventPoint> ThresholdEdgePoints = new List<eventPoint>();
            Dictionary<int, bool> pointChecked = new Dictionary<int, bool>();
            int count = 0;
            for (int i=0;i< allEventPoints.Count;i++)
            {
                eventPoint ep = allEventPoints[i];
                // calculate how many polygons is this event point in.
                int InteriorCount = 0;
                int OnCount = 0;
                List<int> InteriorPolyIndex = new List<int>();
                List<int> OnPolyIndex = new List<int>();
                //foreach (GenericPolygon poly in polyList)
                for( int j=0;j<polyList.Count;j++)
                {
                    GenericPolygon poly = polyList[j];
                    
                    if (poly.PointIsOnPolygon(ep.x, ep.y))
                    {
                        OnCount++;
                        //InteriorCount++;
                        OnPolyIndex.Add(j);
                        //InteriorPolyIndex.Add(j);
                    }
                    else
                    {
                        if (poly.PointIsInPolygon(ep.x, ep.y))
                        {
                            InteriorCount++;
                            InteriorPolyIndex.Add(j);
                        }
                    }
                }


                //InteriorCount = InteriorCount - OnCount;// it is at least on one polygon, which actully should be counted.

                if (InteriorCount >= MinMajorityCount)
                {
                    continue;
                }
                if (InteriorCount + OnCount <  MinMajorityCount)
                {
                    continue;
                }

                

                // an event point that is right on the boarder of majority threshold
                ThresholdEdgePoints.Add(ep);
                pointChecked.Add(count, false);
                count++;
            }

            // delete all neighbors thats not in majority edge
            foreach(eventPoint ep in ThresholdEdgePoints)
            {
                List<int[]> ValidNeighbor = new List<int[]>();
                for (int i = 0; i < ep.neightbors.Count; i++)
                {
                    eventPoint tp = new eventPoint(ep.neightbors[i]);
                    foreach (eventPoint eep in ThresholdEdgePoints)
                    {
                        if (eep.IsSamePoint(tp))
                        {
                            ValidNeighbor.Add(ep.neightbors[i]);
                        }
                    }
                }
                ep.neightbors.Clear();
                ep.neightbors = ValidNeighbor;
            }

            string debugstr = JSonUtils.ConvertObjectToJSon(ThresholdEdgePoints);
            string polylistStr = JSonUtils.ConvertObjectToJSon(polyList);
            string allpointsstr = JSonUtils.ConvertObjectToJSon(allEventPoints);

            List<List<int[]>> outputPolygon = new List<List<int[]>>();
            for (int p= 0;p < ThresholdEdgePoints.Count;p++)
            {
                if (pointChecked[p]) continue;
                
                List<int[]> polygon1 = new List<int[]>();
                List<int[]> polygon2 = new List<int[]>();
                polygon1.Add(new int[] { ThresholdEdgePoints[p].x, ThresholdEdgePoints[p].y });
                polygon1 = addNeighbors(ThresholdEdgePoints[p], ThresholdEdgePoints,polygon1,pointChecked);
                if (!pointChecked[p])
                {
                    polygon2.Add(new int[] { ThresholdEdgePoints[p].x, ThresholdEdgePoints[p].y });
                    polygon2 = addNeighbors(ThresholdEdgePoints[p], ThresholdEdgePoints, polygon2, pointChecked);
                    for (int i = polygon2.Count - 1; i >= 0; i--)
                    {
                        polygon1.Add(polygon2[i]);
                    }
                }
                //loop closed
                outputPolygon.Add(polygon1);
                
                
                //debug
                string poly = "";
                foreach(int []xy in polygon1)
                {
                    poly += "[" + xy[0].ToString() + "," + xy[1].ToString() + "] ";
                }
                Console.WriteLine(poly);

            }


            //// debug
            List<LineSegment> lines = new List<LineSegment>();
            for (int k = 0; k < ThresholdEdgePoints.Count; k++)
            {
                
                int x1 = ThresholdEdgePoints[k].x;
                int y1 = ThresholdEdgePoints[k].y;

                foreach(int[] xy in ThresholdEdgePoints[k].neightbors)
                {
                    int x2 = xy[0];
                    int y2 = xy[1];
                    LineSegment line = new LineSegment(x1, y1, x2, y2);
                    lines.Add(line);
                }
            }
            Color c = ColorSet.getColorByObjectType("cat");

            //Image originalImage = ImageUtilities.getImageFromURI("https://satyamresearchjobstorage.blob.core.windows.net/kittisegmentation/test/4-cats-on-tree-fb-cover.jpg");
            Image originalImage = ImageUtilities.getImageFromURI("https://satyamresearchjobstorage.blob.core.windows.net/kittisegmentation/testpascal/2007_000121.jpg");


            Image im = DrawingBoxesAndLinesOnImages.addLinesToImage(originalImage, lines, c);

            //string filename = count.ToString();
            string filename = "majorityEdge";
            ImageUtilities.saveImage(im, DirectoryConstants.defaultTempDirectory, filename);


            // TB changed to Segments
            return new GenericPolygon(outputPolygon[0]);
        }

        static List<int[]> addNeighbors(eventPoint ep, List<eventPoint> eventPoints, List<int[]> polygon, 
            Dictionary<int, bool> pointChecked)
        {
            bool found = false;
            foreach (int[]xy in ep.neightbors)
            {
                eventPoint tp = new eventPoint(xy);
                for (int i=0;i< eventPoints.Count;i++)
                {
                    if (!pointChecked[i] && eventPoints[i].IsSamePoint(tp))
                    {
                        polygon.Add(xy);
                        pointChecked[i] = true;
                        polygon = addNeighbors(eventPoints[i], eventPoints, polygon, pointChecked);
                        found = true;
                        break;
                    }
                }
                // TB proved, each point on the majority edge will have one and only one neighbor unchecked.
                // in other words, its a polygon without half edges.
                if (found)
                {
                    break;
                }
            }

            return polygon;
        }

        public static GenericPolygon GetAggregatedGenericPolygon_Relaxation(List<GenericPolygon> polyList, int ImageWidth, int ImageHeight, double PolygonBoundaryMajorityThreshold)
        {
            if (polyList.Count == 0) return null;
            GenericPolygon startingPoly = polyList[0];

            GenericPolygon ret = PolygonDeformationToMajorityEdge(startingPoly, polyList, PolygonBoundaryMajorityThreshold);

            int count = 0;
            for (int i=1;i<polyList.Count;i++)
            {
                GenericPolygon adjustedPoly = PolygonDeformationToMajorityEdge(polyList[i], polyList, PolygonBoundaryMajorityThreshold);
                // insert the cardinal points from the adjusted poly to the existing ret poly
                foreach (int[] xy in adjustedPoly.vertices)
                {
                    LineSegment closestEdge = new LineSegment();
                    int closestEdgeStartingPointIndex = -1;
                    ret.getClosestPointsOnPolygonToAPoint(xy[0], xy[1], out closestEdge, out closestEdgeStartingPointIndex);

                    if (closestEdgeStartingPointIndex != -1)
                    {
                        // don't insert if it is the same cardinal points of the polygon
                        if (xy[0] == ret.vertices[closestEdgeStartingPointIndex][0] 
                            && xy[1] == ret.vertices[closestEdgeStartingPointIndex][1])
                        {
                            continue;
                        }

                        int closestEdgeEndingPointIndex = (closestEdgeStartingPointIndex + 1) % ret.vertices.Count;
                        if (xy[0] == ret.vertices[closestEdgeEndingPointIndex][0]
                            && xy[1] == ret.vertices[closestEdgeEndingPointIndex][1])
                        {
                            continue;
                        }
                        
                        ret.vertices.Insert(closestEdgeEndingPointIndex, xy);

                        //// debug
                        List<LineSegment> lines = new List<LineSegment>();
                        int newIndex1 = closestEdgeStartingPointIndex;
                        int newIndex2 = (closestEdgeStartingPointIndex + 1) % ret.vertices.Count;
                        int newIndex3 = (closestEdgeStartingPointIndex + 2) % ret.vertices.Count;
                        for (int k = 0; k < ret.vertices.Count; k++)
                        {
                            if (k == closestEdgeStartingPointIndex) continue;
                            if (k == (closestEdgeStartingPointIndex + 1) % ret.vertices.Count) continue;
                            //if (k == (closestEdgeStartingPointIndex + 2) % ret.vertices.Count) continue;
                            int x1 = ret.vertices[k][0];
                            int y1 = ret.vertices[k][1];

                            int j = (k + 1) % ret.vertices.Count;
                            int x2 = ret.vertices[j][0];
                            int y2 = ret.vertices[j][1];
                            LineSegment line = new LineSegment(x1, y1, x2, y2);
                            lines.Add(line);
                        }
                        Color c = ColorSet.getColorByObjectType("cat");

                        //Image originalImage = ImageUtilities.getImageFromURI("https://satyamresearchjobstorage.blob.core.windows.net/kittisegmentation/test/4-cats-on-tree-fb-cover.jpg");
                        Image originalImage = ImageUtilities.getImageFromURI("https://satyamresearchjobstorage.blob.core.windows.net/kittisegmentation/testpascal/2007_000121.jpg");
                        

                        Image im = DrawingBoxesAndLinesOnImages.addLinesToImage(originalImage, lines, c);

                        List<LineSegment> newlines = new List<LineSegment>();
                        newlines.Add(new LineSegment(ret.vertices[newIndex1][0], ret.vertices[newIndex1][1], ret.vertices[newIndex2][0], ret.vertices[newIndex2][1]));
                        newlines.Add(new LineSegment(ret.vertices[newIndex2][0], ret.vertices[newIndex2][1], ret.vertices[newIndex3][0], ret.vertices[newIndex3][1]));

                        im = DrawingBoxesAndLinesOnImages.addLinesToImage(im, newlines, Color.Red);

                        string filename = count.ToString();
                        ImageUtilities.saveImage(im, DirectoryConstants.defaultTempDirectory, filename);

                        count++;
                    }
                    else
                    {
                        Console.WriteLine("Error:");
                    }

                    

                }
            }
            return ret;
        }

        public static double getInPolygonVotesRatio(int x, int y, List<GenericPolygon> allPolygons)
        {
            int votes = 0;
            double noResults = (double)allPolygons.Count;
            foreach (GenericPolygon polygon in allPolygons)
            {
                if (polygon.PointIsInPolygon(x, y))
                {
                    votes++;
                }
            }
            return (double)votes / noResults;
        }

        public static GenericPolygon PolygonDeformationToMajorityEdge(GenericPolygon inputPolygon, List<GenericPolygon> allPolygons, double PolygonBoundaryMajorityThreshold)
        {
            List<int[]> adjustedVertices = new List<int[]>();
            for (int i = 0; i < inputPolygon.vertices.Count; i++)
            {
                int x = inputPolygon.vertices[i][0];
                int y = inputPolygon.vertices[i][1];

                int[] adjustedCoordinate = AdjustPointToMajorityBoundary(x, y, allPolygons, PolygonBoundaryMajorityThreshold);



                //double votes = getInPolygonVotesRatio(x, y, allPolygons);
                //if (votes > PolygonBoundaryMajorityThreshold)
                //{
                //    adjustedCoordinate = RelaxPointToMajorityBoundary(x, y, allPolygons, PolygonBoundaryMajorityThreshold);
                //}
                //else
                //{
                //    adjustedCoordinate = TightenPointToMajorityBoundary(x, y, allPolygons, PolygonBoundaryMajorityThreshold);
                //}

                adjustedVertices.Add(adjustedCoordinate);
            }
            return new GenericPolygon(adjustedVertices);
        }

        /// <summary>
        /// find the closest point (x’,y’) on each and every polygons, calculate votes, and find the one closest to the majority limit. 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="allPolygons"></param>
        /// <param name="PolygonBoundaryMajorityThreshold"></param>
        /// <returns></returns>
        public static int[] AdjustPointToMajorityBoundary(int x, int y, List<GenericPolygon>allPolygons, double PolygonBoundaryMajorityThreshold)
        {
            double closestVoteToThresholdDifference = double.MaxValue;
            int[] adjustedPoint = new int[] { -1, -1 };
            foreach(GenericPolygon poly in allPolygons)
            {
                LineSegment closestEdge = new LineSegment();
                int index = -1;
                int[] xy = poly.getClosestPointsOnPolygonToAPoint(x, y, out closestEdge, out index);
                double votesRatio = getInPolygonVotesRatio(xy[0], xy[1], allPolygons);
                double voteDifference = Math.Abs(votesRatio - PolygonBoundaryMajorityThreshold);
                //double voteDifference = (votesRatio - PolygonBoundaryMajorityThreshold);
                if (voteDifference< closestVoteToThresholdDifference && voteDifference>=0)
                {
                    closestVoteToThresholdDifference = voteDifference;
                    adjustedPoint[0] = xy[0];
                    adjustedPoint[1] = xy[1];
                }
                else
                {

                }
            }
            return adjustedPoint;
        }
        
        
        public static bool IsAcceptable(
            SatyamAggregatedResultsTableEntry aggResultEntry, 
            SatyamResultsTableEntry resultEntry,
            double ACCEPTANCE_NUMBER_OF_POLYGONS_THRESHOLD = TaskConstants.IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_PAYMENT, //the person must have made at least 80% of the boxes
            double POLYGON_IOU_THRESHOLD = TaskConstants.IMAGE_SEGMENTATION_MTURK_POLYGON_IOU_THRESHOLD_FOR_PAYMENT
            )
        {

            //most boxes should be within limits 
            //most categories should be right
            SatyamAggregatedResult satyamAggResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(aggResultEntry.ResultString);
            ImageSegmentationAggregatedResult_NoHoles aggresult = JSonUtils.ConvertJSonToObject<ImageSegmentationAggregatedResult_NoHoles>(satyamAggResult.AggregatedResultString);
            SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(resultEntry.ResultString);
            ImageSegmentationResult_NoHoles result = JSonUtils.ConvertJSonToObject<ImageSegmentationResult_NoHoles>(satyamResult.TaskResult);

            if (result == null) return false;

            //first check if the number of boxes are within limit
            int boxLimit = (int)Math.Ceiling((double)aggresult.boxesAndCategories.objects.Count * ACCEPTANCE_NUMBER_OF_POLYGONS_THRESHOLD);
            if (result.objects.Count < boxLimit)
            {
                return false;
            }

            
            //We fist do a bipartitte matching to find the best assocaition for the boxes
            List<List<GenericPolygon>> allPolygons = new List<List<GenericPolygon>>();
            allPolygons.Add(new List<GenericPolygon>());
            foreach (ImageSegmentationResultSingleEntry_NoHoles entry in result.objects)
            {
                allPolygons[0].Add(entry.polygon);
            }
            allPolygons.Add(new List<GenericPolygon>());
            List<bool> tooSmallToIgnore = new List<bool>();
            foreach (ImageSegmentationResultSingleEntry_NoHoles entry in aggresult.boxesAndCategories.objects)
            {
                allPolygons[1].Add(entry.polygon);
            }
            List<MultipartiteWeightedMatch> polygonAssociation = PolygonAssociation.computeGenericPolygonAssociations(allPolygons);

            //now find how many of the results match aggregated results
            int noAccepted = 0;

            foreach (MultipartiteWeightedMatch match in polygonAssociation)
            {
                if (match.elementList.ContainsKey(1)) // this contains an aggregated box
                {
                    if (match.elementList.ContainsKey(0)) // a result box has been associated
                    {
                        GenericPolygon aggregatedGenericPolygon = allPolygons[1][match.elementList[1]];
                        GenericPolygon resultGenericPolygon = allPolygons[0][match.elementList[0]];

                        //double IoU = GenericPolygon.computeIntersectionOverUnion(aggregatedGenericPolygon, resultGenericPolygon);
                        double IoU = 1;

                        if (IoU >= POLYGON_IOU_THRESHOLD) 
                        {
                            //now check category
                            if (result.objects[match.elementList[0]].Category == aggresult.boxesAndCategories.objects[match.elementList[1]].Category)
                            {
                                //both category and bounding box tests have passed
                                noAccepted++;
                            }
                        }
                    }
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
