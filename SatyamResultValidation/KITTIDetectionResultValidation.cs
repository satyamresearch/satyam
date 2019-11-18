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
using System.Text;
using UsefulAlgorithms;
using Utilities;

namespace SatyamResultValidation
{
    

    public class KITTIDetectionGroundTruth
    {
        public List<MultiObjectLocalizationAndLabelingResultSingleEntry> objects;
        public List<double> truncation;
        public List<int> occlusion;
        public List<bool> BlackListed;        
        public int noBlackListed;

        //public KITTIGroundTruth()
        //{
        //    objects = new List<MultiObjectLocalizationAndLabelingResultSingleEntry>();
        //    truncation = new List<double>();
        //    occlusion = new List<int>();
            
        //}

        public KITTIDetectionGroundTruth(string filePath, int MinHeight = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_HEIGHT,
            int MaxOcclusion = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MAX_OCCLUSION,
            double Max_Truncation = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_TRUNCATION)
        {
            objects = new List<MultiObjectLocalizationAndLabelingResultSingleEntry>();
            truncation = new List<double>();
            occlusion = new List<int>();
            BlackListed = new List<bool>();
            string[] lines = System.IO.File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] elems = line.Split(' ');
                MultiObjectLocalizationAndLabelingResultSingleEntry obj = new MultiObjectLocalizationAndLabelingResultSingleEntry();
                obj.Category = elems[0];

                if (obj.Category == "Truck" || obj.Category == "Tram" || obj.Category == "Misc") continue;

                obj.boundingBox = new BoundingBox((int)Convert.ToDouble(elems[4]), (int)Convert.ToDouble(elems[5]),
                    (int)Convert.ToDouble(elems[6]), (int)Convert.ToDouble(elems[7]));
                objects.Add(obj);
                truncation.Add(Convert.ToDouble(elems[1]));
                occlusion.Add(Convert.ToInt16(elems[2]));
            }
            IgnoreBlackListedObjects(MinHeight, MaxOcclusion, Max_Truncation);
        }

        public void IgnoreBlackListedObjects(int MinHeight = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_HEIGHT, 
            int MaxOcclusion = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MAX_OCCLUSION, 
            double Max_Truncation = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_TRUNCATION)
        {
            //default params are easy criteria.
            noBlackListed = 0;
            BlackListed = new List<bool>();
            for (int i=0;i<objects.Count;i++)
            {
                if (IsBlackListed(objects[i], MinHeight, MaxOcclusion, Max_Truncation, occlusion[i], truncation[i]))
                {
                    BlackListed.Add(true);
                    noBlackListed++;
                }
                else
                {
                    BlackListed.Add(false);
                }
            }
        }

        public static bool IsBlackListed(MultiObjectLocalizationAndLabelingResultSingleEntry obj, int MinHeight, int MaxOcclusion, double Max_Truncation,
            int occlusion = 0, double truncation = 0)
        {
            if (!KITTIDetectionResultValidation.CLASS_NAMES.Contains(obj.Category))
            {
                return true;
            }
            if (obj.boundingBox.getHeight() <= MinHeight)
            {
                return true;
            }
            if (occlusion > MaxOcclusion)
            {
                return true;
            }
            if (truncation > Max_Truncation)
            {
                return true;
            }
            return false;
        }
    }

    public class KITTIDetectionResultValidation
    {
        //double imageDisplayRatio = 0.96;
        public enum KITTIDetectionValidationResultType
        {
            None = 0,
            TP = 1,
            //FP = 1,
            FP_WrongCategoryRightBox = 2, FP_IOU_TOOSMALL = 3, FP_NoGroundTruth = 4,
            FN_DeviatedBox = 5, FN_NoDetection = 6,
        };

        public List<int> MIN_HEIGHT = new List<int>() { 40, 25, 25 };
        public List<int> MAX_OCCLUSION = new List<int>() { 0, 1, 2 };
        public List<double> MAX_TRUNCATION = new List<double>() { 0.15, 0.3, 0.5 };

        public static List<string> CLASS_NAMES = new List<string>() { "Car", "Pedestrian", "Cyclist", "Van", "Person_sitting" };

        public const string GroundTruthLabelDirectory = @"C:\research\dataset\KITTI\Detection\training\label_2\";
        public const string GroundTruthImageDirectory = @"C:\research\dataset\KITTI\Detection\training\image_2\";
        

        

        public double getIOUThreshold(string category, Dictionary<string, double> IoUTreshold)
        {
            if (IoUTreshold.ContainsKey(category.ToLower()))
            {
                return IoUTreshold[category.ToLower()];
            }
            else
            {
                return IoUTreshold["default"];
            }
        }

        public bool IsSameKITTICategory(string detection, string groundtruth)
        {
            if (detection.Equals(groundtruth, StringComparison.InvariantCultureIgnoreCase)){
                return true;
            }
            if (detection.Equals("car", StringComparison.InvariantCultureIgnoreCase) && groundtruth.Equals("Van")){
                return true;
            }
            if (detection.Equals("pedestrian", StringComparison.InvariantCultureIgnoreCase) && groundtruth.Equals("Person_sitting"))
            {
                return true;
            }
            return false;
        }

        public void CleanGroundTruthAndGetStatistics(string ImagePath = GroundTruthImageDirectory, string LabelPath=GroundTruthLabelDirectory,
            int MinHeight = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_HEIGHT,
            int MaxOcclusion = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MAX_OCCLUSION,
            double Max_Truncation = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_TRUNCATION)
        {
            int noTotalObjects = 0;
            int noTotalNonBlackListedObjects = 0;
            SortedDictionary<int, List<string>> noObjectsPerImageHistogram = new SortedDictionary<int, List<string>>();
            SortedDictionary<int, List<string>> noNonBlackListedObjectsPerImageHistogram = new SortedDictionary<int, List<string>>();
            SortedDictionary<string, int> noObjectsPerClassHistogram = new SortedDictionary<string, int>();
            SortedDictionary<string, int> noNonBlackListedObjectsPerClassHistogram = new SortedDictionary<string, int>();
            string[] files = System.IO.Directory.GetFiles(LabelPath);
            foreach(string file in files)
            {
                KITTIDetectionGroundTruth image = new KITTIDetectionGroundTruth(file, MinHeight, MaxOcclusion, Max_Truncation);
                int Count = image.objects.Count;
                noTotalObjects += Count;
                int NonBlackListedCount = image.objects.Count -  image.noBlackListed;
                noTotalNonBlackListedObjects += NonBlackListedCount;
                if (!noObjectsPerImageHistogram.ContainsKey(Count))
                {
                    noObjectsPerImageHistogram.Add(Count, new List<string>());
                }
                noObjectsPerImageHistogram[Count].Add(file);
                if (!noNonBlackListedObjectsPerImageHistogram.ContainsKey(NonBlackListedCount))
                {
                    noNonBlackListedObjectsPerImageHistogram.Add(NonBlackListedCount, new List<string>());
                }
                noNonBlackListedObjectsPerImageHistogram[NonBlackListedCount].Add(file);

                for(int i=0;i< image.objects.Count;i++)
                {
                    if (!noObjectsPerClassHistogram.ContainsKey(image.objects[i].Category))
                    {
                        noObjectsPerClassHistogram.Add(image.objects[i].Category, 0);
                    }
                    noObjectsPerClassHistogram[image.objects[i].Category]++;
                    if (image.BlackListed[i])
                    {
                        if (!noNonBlackListedObjectsPerClassHistogram.ContainsKey(image.objects[i].Category))
                        {
                            noNonBlackListedObjectsPerClassHistogram.Add(image.objects[i].Category, 0);
                        }
                        noNonBlackListedObjectsPerClassHistogram[image.objects[i].Category]++;
                    }
                }
            }
            Console.WriteLine("Total Objects: {0}", noTotalObjects);
            foreach (int num in noObjectsPerImageHistogram.Keys)
            {
                Console.WriteLine("{0}, {1}", num, noObjectsPerImageHistogram[num].Count);
            }            
            foreach(string category in noObjectsPerClassHistogram.Keys)
            {
                Console.WriteLine("{0}, {1}", category, noObjectsPerClassHistogram[category]);
            }
            Console.WriteLine("Total NonBlackListed Objects: {0}", noTotalNonBlackListedObjects);
            foreach (int num in noNonBlackListedObjectsPerImageHistogram.Keys)
            {
                Console.WriteLine("{0}, {1}", num, noNonBlackListedObjectsPerImageHistogram[num].Count);
            }
            foreach (string category in noNonBlackListedObjectsPerClassHistogram.Keys)
            {
                Console.WriteLine("{0}, {1}", category, noNonBlackListedObjectsPerClassHistogram[category]);
            }

            // optional
            CategorizeImagesByObjectNumber(noObjectsPerImageHistogram, ImagePath, DirectoryConstants.defaultTempDirectory);
        }


        public void CategorizeImagesByObjectNumber(SortedDictionary<int, List<string>> noObjectsPerImageHistogram, string srcFilePath, string destFilePath, int step=5)
        {
            int noFolders = 0;
            string newImgFolder = destFilePath + @"\0";
            System.IO.Directory.CreateDirectory(newImgFolder);
            System.IO.Directory.CreateDirectory(newImgFolder + "_label");
            foreach (int no in noObjectsPerImageHistogram.Keys)
            {
                if (no > noFolders * step)
                {
                    newImgFolder = destFilePath + @"\" + (noFolders*step+1).ToString() + "-" + ((noFolders+1)*step).ToString();
                    System.IO.Directory.CreateDirectory(newImgFolder);
                    System.IO.Directory.CreateDirectory(newImgFolder+"_label");
                    noFolders++;
                }
                foreach(string file in noObjectsPerImageHistogram[no])
                {
                    string fileName = URIUtilities.filenameFromDirectory(file).Split('.')[0];
                    /// copy the images
                    string srcImgFile = srcFilePath + "\\" + fileName + ".png";
                    string destImgFile = newImgFolder + "\\" + fileName + ".png";
                    System.IO.File.Copy(srcImgFile, destImgFile, true);
                    ///copy the labels
                    string srcLabelFile = file;
                    string destLabelFile = newImgFolder + "_label\\" + fileName + ".txt";
                    System.IO.File.Copy(srcLabelFile, destLabelFile, true);
                }
            }

        }


        public static void saveGroundtruthImageWithDontCareBox(string outputFolder)
        {
            string [] labelFiles = Directory.GetFiles(KITTIDetectionResultValidation.GroundTruthLabelDirectory);
            string [] imgFiles = Directory.GetFiles(KITTIDetectionResultValidation.GroundTruthImageDirectory);

            
            for (int i = 0; i < labelFiles.Length; i++)
            {
                List<Rectangle> rectangles = new List<Rectangle>();
                List<Color> colors = new List<Color>();
                List<string> ids = new List<string>();
                List<bool> dashed = new List<bool>();

                string label = labelFiles[i];
                string img = imgFiles[i];
                string filename = URIUtilities.filenameFromDirectoryNoExtension(img);
                KITTIDetectionGroundTruth GroundTruthObjects = new KITTIDetectionGroundTruth(label);
                Image im = Image.FromFile(img);
                foreach(MultiObjectLocalizationAndLabelingResultSingleEntry obj in GroundTruthObjects.objects)
                {
                    if (obj.Category == "DontCare")
                    {
                        int x = obj.boundingBox.tlx;
                        int y = obj.boundingBox.tly;
                        int width = obj.boundingBox.brx - obj.boundingBox.tlx;
                        int height = obj.boundingBox.bry - obj.boundingBox.tly;
                        Rectangle r = new Rectangle(x, y, width, height);
                        rectangles.Add(r);
                        colors.Add(Color.Blue);
                        ids.Add("");
                        dashed.Add(false);
                    }
                }
                Image newim = DrawingBoxesAndLinesOnImages.addRectanglesToImage(im, rectangles, colors, ids, dashed);
                ImageUtilities.saveImage(newim, outputFolder, filename);
            }
        }

        public void saveValidatedImage(string imageURI, string ValidationResultFolder, 
            List<MultiObjectLocalizationAndLabelingResultSingleEntry> detectionObjects, 
            KITTIDetectionGroundTruth groundtruthObjects,
            SortedDictionary<int, KITTIDetectionValidationResultType> aggregatedValidationStatus, 
            SortedDictionary<int, KITTIDetectionValidationResultType> groundTruthValidationStatus, Dictionary<int, bool> DetectionBlackListed)
        {

            List<Rectangle> rectangles = new List<Rectangle>();
            List<Color> colors = new List<Color>();
            List<string> ids = new List<string>();
            List<bool> dashed = new List<bool>();
            for (int j = 0; j < detectionObjects.Count; j++)
            {
                if (DetectionBlackListed.ContainsKey(j) && DetectionBlackListed[j]) continue;

                MultiObjectLocalizationAndLabelingResultSingleEntry box = detectionObjects[j];
                int x = box.boundingBox.tlx;
                int y = box.boundingBox.tly;
                int width = box.boundingBox.brx - box.boundingBox.tlx;
                int height = box.boundingBox.bry - box.boundingBox.tly;
                Rectangle r = new Rectangle(x, y, width, height);
                rectangles.Add(r);

                
                int colorIndex = (int)aggregatedValidationStatus[j];
                colors.Add(DrawingBoxesAndLinesOnImages.Colors[colorIndex]);

                string id = j + "-" + box.Category + "-" + aggregatedValidationStatus[j];
                ids.Add(id);

                dashed.Add(false);
            }

            for (int j = 0; j < groundtruthObjects.objects.Count; j++)
            {
                if (groundtruthObjects.BlackListed[j]) continue;

                if (groundTruthValidationStatus[j] == KITTIDetectionValidationResultType.FN_NoDetection)
                {
                    MultiObjectLocalizationAndLabelingResultSingleEntry box = groundtruthObjects.objects[j];
                    int x = box.boundingBox.tlx;
                    int y = box.boundingBox.tly;
                    int width = box.boundingBox.brx - box.boundingBox.tlx;
                    int height = box.boundingBox.bry - box.boundingBox.tly;
                    Rectangle r = new Rectangle(x, y, width, height);
                    rectangles.Add(r);


                    int colorIndex = (int)groundTruthValidationStatus[j];
                    colors.Add(DrawingBoxesAndLinesOnImages.Colors[colorIndex]);

                    string id = j + "-" + box.Category + "-" + groundTruthValidationStatus[j];
                    ids.Add(id);

                    dashed.Add(true);
                }
            }
            DrawingBoxesAndLinesOnImages.DrawBoxAndSaveImage(imageURI, ValidationResultFolder, rectangles, colors, ids, dashed);
            
        }




        public void ValidateSatyamKITTIDetectionAggregationResultByGUID(List<string> guids, Dictionary<string, double> IoUTreshold, bool saveImage = false,  
            int MinHeight = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_HEIGHT,
            int MaxOcclusion = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MAX_OCCLUSION,
            double Max_Truncation = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_TRUNCATION)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = new List<SatyamAggregatedResultsTableEntry>();

            foreach (string guid in guids)
            {
                results.AddRange(resultsDB.getEntriesByGUID(guid));
            }
            resultsDB.close();
            List<Dictionary<int, bool>> BlackListedDetectionPerImage;
            ValidateSatyamKITTIDetectionAggregationResult(results, IoUTreshold, out BlackListedDetectionPerImage,saveImage, MinHeight, MaxOcclusion, Max_Truncation);
        }




        public string ValidateSatyamKITTIDetectionAggregationResult(
            List<SatyamAggregatedResultsTableEntry> resultsPerImage, // this results has incomplete aggregated result information, only jobguid, taskid, and agg result string is available.
            Dictionary<string, double> IoUTreshold, 
            out List<Dictionary<int, bool>> BlackListedDetectionPerImage,
            bool saveImage = false, 
            int MinHeight = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_HEIGHT,            
            int MaxOcclusion = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MAX_OCCLUSION,
            double Max_Truncation = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_TRUNCATION)
        {

            BlackListedDetectionPerImage = new List<Dictionary<int, bool>>();
            int noTotalGroundTruth = 0;
            int noTotalDetection = 0;

            int noTP = 0;

            int noFP = 0;
            int noFP_WrongCategoryRightBox = 0;
            int noFP_IOUTooSmallBox = 0;
            int noFP_IOUTooSmallBoxRightCategory = 0;
            int noFP_NoGroundTruth = 0;

            int noFN = 0;
            int noFN_WrongDetection = 0;
            int noFN_NoDetection = 0;


            // black list version
            int noTotalGroundTruth_BlackList = 0;
            int noTotalDetection_BlackList = 0;

            int noTP_BlackList = 0;

            int noFP_BlackList = 0;
            int noFP_WrongCategoryRightBox_BlackList = 0;
            int noFP_IOUTooSmallBox_BlackList = 0;
            //int noFP_IOUTooSmallBoxRightCategory_BlackList = 0;
            int noFP_NoGroundTruth_BlackList = 0;

            int noFN_BlackList = 0;
            int noFN_WrongDetection_BlackList = 0;
            int noFN_NoDetection_BlackList = 0;

            


            for (int i = 0; i < resultsPerImage.Count; i++)
            {
                bool ImageDoneCorrect = true;

                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(resultsPerImage[i]);
                string fileName = URIUtilities.filenameFromURINoExtension(data.SatyamURI);
                KITTIDetectionGroundTruth GroundTruthObjects = new KITTIDetectionGroundTruth(GroundTruthLabelDirectory + fileName + ".txt", MinHeight,MaxOcclusion,Max_Truncation);

                string jobGUID = resultsPerImage[i].JobGUID;
                int taskID = resultsPerImage[i].SatyamTaskTableEntryID;
                String resultString = data.AggregatedResultString;
                MultiObjectLocalizationAndLabelingAggregatedResult result = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingAggregatedResult>(resultString);
                List<MultiObjectLocalizationAndLabelingResultSingleEntry> allObjectsPerResult = result.boxesAndCategories.objects;
                SortedDictionary<int, KITTIDetectionValidationResultType> aggregatedValidationStatus = new SortedDictionary<int, KITTIDetectionValidationResultType>();
                SortedDictionary<int, KITTIDetectionValidationResultType> groundTruthValidationStatus = new SortedDictionary<int, KITTIDetectionValidationResultType>();

                //We fist do a bipartitte matching to find the best assocaition for the boxes
                List<List<BoundingBox>> allboxes = new List<List<BoundingBox>>();
                allboxes.Add(new List<BoundingBox>());
                foreach (MultiObjectLocalizationAndLabelingResultSingleEntry entry in allObjectsPerResult)
                {
                    allboxes[0].Add(entry.boundingBox);
                }
                allboxes.Add(new List<BoundingBox>());
                List<bool> tooSmallToIgnore = new List<bool>();
                foreach (MultiObjectLocalizationAndLabelingResultSingleEntry entry in GroundTruthObjects.objects)
                {
                    // remove don't care before association?
                    if (entry.Category == "DontCare") continue;

                    allboxes[1].Add(entry.boundingBox);
                }
                List<MultipartiteWeightedMatch> boxAssociation = BoundingBoxAssociation.computeBoundingBoxAssociations(allboxes);

                Dictionary<int, bool> DetectionBlackListed = new Dictionary<int, bool>();

                foreach (MultipartiteWeightedMatch match in boxAssociation)
                {
                    
                    KITTIDetectionValidationResultType resultType = KITTIDetectionValidationResultType.None;
                    if (match.elementList.ContainsKey(1)) // this contains a groundtruth box
                    {
                        noTotalGroundTruth++;
                        int groundTruthObjectIndex = match.elementList[1];
                        string groundTruthCategory = GroundTruthObjects.objects[groundTruthObjectIndex].Category;

                        if (match.elementList.ContainsKey(0)) // an aggregated result box has been associated
                        {
                            noTotalDetection++;
                            int aggregatedObjectIndex = match.elementList[0];
                            string aggregatedCategory = allObjectsPerResult[aggregatedObjectIndex].Category;

                            BoundingBox groundtruthBoundingBox = allboxes[1][groundTruthObjectIndex];
                            BoundingBox aggregatedBoundingBox = allboxes[0][aggregatedObjectIndex];

                            if (BoundingBox.ComputeIntersectionOverUnion(groundtruthBoundingBox, aggregatedBoundingBox) > getIOUThreshold(groundTruthCategory, IoUTreshold))
                            {
                                //now check category
                                if (IsSameKITTICategory(aggregatedCategory,groundTruthCategory)
                                    || groundTruthCategory.Equals("DontCare", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    //both category and bounding box tests have passed
                                    noTP++;
                                    resultType = KITTIDetectionValidationResultType.TP;
                                }
                                else
                                {
                                    noFP++;
                                    resultType = KITTIDetectionValidationResultType.FP_WrongCategoryRightBox;
                                    noFP_WrongCategoryRightBox++;
                                }
                            }
                            else
                            {
                                noFP++;
                                resultType = KITTIDetectionValidationResultType.FP_IOU_TOOSMALL;
                                noFP_IOUTooSmallBox++;
                                if (IsSameKITTICategory(aggregatedCategory, groundTruthCategory))
                                {
                                    noFP_IOUTooSmallBoxRightCategory++;
                                }
                            }
                            aggregatedValidationStatus.Add(aggregatedObjectIndex, resultType);
                            if (resultType != KITTIDetectionValidationResultType.TP)
                            {
                                noFN++;
                                noFN_WrongDetection++;
                            }
                        }
                        else
                        {
                            noFN++;
                            noFN_NoDetection++;
                            resultType = KITTIDetectionValidationResultType.FN_NoDetection;
                        }
                        if (GroundTruthObjects.BlackListed[groundTruthObjectIndex])
                        {
                            noTotalGroundTruth_BlackList++;
                            switch (resultType)
                            {
                                case KITTIDetectionValidationResultType.TP: noTP_BlackList++; break;
                                //case ResultType.FP: noFP_BlackList++;break;
                                case KITTIDetectionValidationResultType.FP_IOU_TOOSMALL: noFP_BlackList++; noFP_IOUTooSmallBox_BlackList++; noFN_BlackList++; noFN_WrongDetection_BlackList++; break;
                                case KITTIDetectionValidationResultType.FP_WrongCategoryRightBox: noFP_BlackList++; noFP_WrongCategoryRightBox_BlackList++; noFN_BlackList++; noFN_WrongDetection_BlackList++; break;
                                case KITTIDetectionValidationResultType.FN_NoDetection: noFN_BlackList++; noFN_NoDetection_BlackList++;  break;
                            }
                            if (match.elementList.ContainsKey(0))
                            {
                                DetectionBlackListed.Add(match.elementList[0], true);
                                noTotalDetection_BlackList++;
                            }
                        }
                        else
                        {
                            if (resultType!=KITTIDetectionValidationResultType.TP) ImageDoneCorrect = false;
                        }
                        groundTruthValidationStatus.Add(groundTruthObjectIndex, resultType);
                    }
                    else
                    {
                        noTotalDetection++;
                        noFP++;
                        resultType = KITTIDetectionValidationResultType.FP_NoGroundTruth;
                        noFP_NoGroundTruth++;

                        int aggregatedObjectIndex = match.elementList[0];
                        string aggregatedCategory = allObjectsPerResult[aggregatedObjectIndex].Category;

                        // check whether the detection should be blacklisted.
                        if (KITTIDetectionGroundTruth.IsBlackListed(allObjectsPerResult[match.elementList[0]], MinHeight,MaxOcclusion, Max_Truncation))
                        {
                            noTotalDetection_BlackList++;
                            noFP_BlackList++;
                            noFP_NoGroundTruth_BlackList++;
                            DetectionBlackListed.Add(match.elementList[0], true);
                        }
                        else
                        {
                            bool blacklisted = false;   
                            // check whether this detection falls into one of the DontCare Region that has been matched to other boxes.
                            foreach (MultiObjectLocalizationAndLabelingResultSingleEntry gt in GroundTruthObjects.objects)
                            {
                                if (gt.Category.Equals("DontCare", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    if (BoundingBox.ComputeIntersectionOverUnion(gt.boundingBox, allObjectsPerResult[match.elementList[0]].boundingBox) > getIOUThreshold(allObjectsPerResult[match.elementList[0]].Category, IoUTreshold))
                                    {
                                        noFP_BlackList++;
                                        noFP_NoGroundTruth_BlackList++;
                                        blacklisted = true; break;
                                    }
                                }
                            }
                            if (!blacklisted) ImageDoneCorrect = false;
                        }
                        aggregatedValidationStatus.Add(match.elementList[0], resultType);
                    }

                }

                SatyamResultsTableAccess resultDB = new SatyamResultsTableAccess();
                if (saveImage)
                {
                    
                    if (!ImageDoneCorrect)
                    {
                        string outputDir = DirectoryConstants.defaultTempDirectory + "\\" + jobGUID + "\\Results";
                        //Directory.CreateDirectory(outputDir);
                        //List<SatyamResultsTableEntry> resultsForThisTask = resultDB.getEntriesByGUIDAndTaskID(jobGUID, taskID);
                        //MultiObjectLabelingAndLocalizationAnalysis.SaveResultImagesLocally(resultsForThisTask, outputDir);

                        outputDir = DirectoryConstants.defaultTempDirectory + "\\" + jobGUID + "\\AggregatedResults";
                        Directory.CreateDirectory(outputDir);
                        List<SatyamAggregatedResultsTableEntry> aggResultList = new List<SatyamAggregatedResultsTableEntry>() {
                            resultsPerImage[i]
                        };
                        MultiObjectLabelingAndLocalizationAnalysis.SaveAggregatedResultImagesLocally(aggResultList,outputDir);

                        outputDir = DirectoryConstants.defaultTempDirectory + "\\" + jobGUID + "\\ValidationResults";
                        Directory.CreateDirectory(outputDir);
                        saveValidatedImage(data.SatyamURI, outputDir, allObjectsPerResult, GroundTruthObjects, aggregatedValidationStatus, groundTruthValidationStatus, DetectionBlackListed);
                    }
                }

                resultDB.close();

                BlackListedDetectionPerImage.Add(DetectionBlackListed);
            }



            //Console.WriteLine("noTotal {0}, the following number is in NonBlackListed / All\n" +
            //    "noTP {1} / {2}\n" +
            //    "noFP {3} / {4}\n" +
            //    "\t noWrongCategoryRightBox {5} / {6}\n " +
            //    "\t noIOUTooSmallBox {7} / {8}\n" +
            //    "\t noNoGroundTruth {9} / {10}\n" +
            //    "noFN {11} / {12}\n" +
            //    "\t noFN_WrongDetection {13} / {14}\n" +
            //    "\t noFN_NoDetection {15} / {16}\n",
            //    noTotal,
            //    noTP - noTP_BlackList, noTP,
            //    noFP - noFP_BlackList, noFP,
            //    noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList, noFP_WrongCategoryRightBox,
            //    noFP_IOUTooSmallBox - noFP_IOUTooSmallBox_BlackList, noFP_IOUTooSmallBox,
            //    noFP_NoGroundTruth-noFP_NoGroundTruth_BlackList, noFP_NoGroundTruth,
            //    noFN - noFN_BlackList, noFN,
            //    noFN_WrongDetection - noFN_WrongDetection_BlackList, noFN_WrongDetection,
            //    noFN_NoDetection - noFN_NoDetection_BlackList, noFN_NoDetection);

            //Console.WriteLine("Box + Category");
            //printAccuracyData(noTP, noTP_BlackList, noFP, noFP_BlackList, noFN, noFN_BlackList);

            //Console.WriteLine("Box Only");
            //printAccuracyData(noTP + noFP_WrongCategoryRightBox, noTP_BlackList,
            //    noFP - noFP_WrongCategoryRightBox, noFP_BlackList,
            //    noFN - noFP_WrongCategoryRightBox, noFN_BlackList);



            // Print in record format. In each row, first no is without BlackList, second no with BlackList
            
            //String s  = String.Format("Precision {17}\n" +
            //    "Recall {18}\n" +
            //    "noTotalGroundTruth {0} {30}\n" +
            //    "noTotalDetection {31} {32}\n" +
            //    "noTP {1} {2}\n" +
            //    "noFP {3} {4}\n" +
            //    "noFN {11} {12}\n" +
            //    "noWrongCategoryRightBox {5} {6}\n " +
            //    "noIOUTooSmallBox {7} {8}\n" +
            //    "noNoGroundTruth {9} {10}\n" +
            //    "noFN_WrongDetection {13} {14}\n" +
            //    "noFN_NoDetection {15} {16}\n\n" +
            //    "TP_Include_BlackListed {21}\n" +
            //    "Precision {19}\n" +
            //    "Recall {20}\n\n" +
            //    "noTP_BoxOnly {22}\n" +
            //    "noFP_BoxOnly {23}\n" +
            //    "noFN_BoxOnly {24}\n" +
            //    "Precision {25}\n" +
            //    "Recall {26}\n\n" +
            //    "noTP_BoxOnly_BlackListIncluded {27}\n"+
            //    "precision {28}\n"+
            //    "recall {29}\n\n\n",
                
            //    noTotalGroundTruth - noTotalGroundTruth_BlackList,
            //    noTP - noTP_BlackList, noTP,
            //    noFP - noFP_BlackList, noFP,
            //    noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList, noFP_WrongCategoryRightBox,
            //    noFP_IOUTooSmallBox - noFP_IOUTooSmallBox_BlackList, noFP_IOUTooSmallBox,
            //    noFP_NoGroundTruth - noFP_NoGroundTruth_BlackList, noFP_NoGroundTruth,
            //    noFN - noFN_BlackList, noFN,
            //    noFN_WrongDetection - noFN_WrongDetection_BlackList, noFN_WrongDetection,
            //    noFN_NoDetection - noFN_NoDetection_BlackList, noFN_NoDetection,
            //    (double)(noTP - noTP_BlackList) / (double)(noTP - noTP_BlackList + noFP - noFP_BlackList),
            //    (double)(noTP - noTP_BlackList) / (double)(noTP - noTP_BlackList + noFN - noFN_BlackList),
            //    (double)(noTP) / (double)(noTP + noFP - noFP_BlackList),
            //    (double)(noTP) / (double)(noTP + noFN - noFN_BlackList),
            //    noTP,
            //    noTP - noTP_BlackList + (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList),
            //    noFP - noFP_BlackList - (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList),
            //    noFN - noFN_BlackList - (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList),
            //    (double)(noTP - noTP_BlackList + (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList)) / (double)(noTP - noTP_BlackList + noFP - noFP_BlackList),
            //    (double)(noTP - noTP_BlackList + (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList)) / (double)(noTP - noTP_BlackList + noFN - noFN_BlackList),
            //    noTP + noFP_WrongCategoryRightBox,
            //    (double)(noTP + noFP_WrongCategoryRightBox) / (double)(noTP + noFP - (noFP_BlackList - noFP_WrongCategoryRightBox_BlackList)),
            //    (double)(noTP + noFP_WrongCategoryRightBox) / (double)(noTP + noFN - (noFN_BlackList - noFP_WrongCategoryRightBox_BlackList)),
            //    noTotalGroundTruth,
            //    noTotalDetection - noTotalDetection_BlackList, noTotalDetection
            //    );

            String s = String.Format("{17} {18} {0} {30} {31} {32} {1} {2} {3} {4} {11} {12} {5} {6} {7} {8} {9} {10} {13} {14} {15} {16} {21} {19} " +
                "{20} {22} {23} {24} {25} {26} {27} {28} {29}\n",

                noTotalGroundTruth - noTotalGroundTruth_BlackList,
                noTP - noTP_BlackList, noTP,
                noFP - noFP_BlackList, noFP,
                noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList, noFP_WrongCategoryRightBox,
                noFP_IOUTooSmallBox - noFP_IOUTooSmallBox_BlackList, noFP_IOUTooSmallBox,
                noFP_NoGroundTruth - noFP_NoGroundTruth_BlackList, noFP_NoGroundTruth,
                noFN - noFN_BlackList, noFN,
                noFN_WrongDetection - noFN_WrongDetection_BlackList, noFN_WrongDetection,
                noFN_NoDetection - noFN_NoDetection_BlackList, noFN_NoDetection,
                (double)(noTP - noTP_BlackList) / (double)(noTP - noTP_BlackList + noFP - noFP_BlackList),
                (double)(noTP - noTP_BlackList) / (double)(noTP - noTP_BlackList + noFN - noFN_BlackList),
                (double)(noTP) / (double)(noTP + noFP - noFP_BlackList),
                (double)(noTP) / (double)(noTP + noFN - noFN_BlackList),
                noTP,
                noTP - noTP_BlackList + (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList),
                noFP - noFP_BlackList - (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList),
                noFN - noFN_BlackList - (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList),
                (double)(noTP - noTP_BlackList + (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList)) / (double)(noTP - noTP_BlackList + noFP - noFP_BlackList),
                (double)(noTP - noTP_BlackList + (noFP_WrongCategoryRightBox - noFP_WrongCategoryRightBox_BlackList)) / (double)(noTP - noTP_BlackList + noFN - noFN_BlackList),
                noTP + noFP_WrongCategoryRightBox,
                (double)(noTP + noFP_WrongCategoryRightBox) / (double)(noTP + noFP - (noFP_BlackList - noFP_WrongCategoryRightBox_BlackList)),
                (double)(noTP + noFP_WrongCategoryRightBox) / (double)(noTP + noFN - (noFN_BlackList - noFP_WrongCategoryRightBox_BlackList)),
                noTotalGroundTruth,
                noTotalDetection - noTotalDetection_BlackList, noTotalDetection
                );

            System.IO.File.AppendAllText(DirectoryConstants.defaultTempDirectory + "\\tempout.txt", s);
            Console.WriteLine(s);
            return s;

        }

        public void printAccuracyData(int noTP, int noTP_BlackList, int noFP, int noFP_BlackList, int noFN, int noFN_BlackList)
        {
            Console.WriteLine("TP of BlackListed objects are ignored");
            Console.WriteLine("Result: TP {0} / FP {1} / FN {2}\nPrecision {3}/{4}: {5}, Recall {6}/{7}: {8}",
                   noTP - noTP_BlackList,
                   noFP - noFP_BlackList,
                   noFN - noFN_BlackList,
                   (noTP - noTP_BlackList),
                   (noTP - noTP_BlackList + noFP - noFP_BlackList),
                   (double)(noTP - noTP_BlackList) / (double)(noTP - noTP_BlackList + noFP - noFP_BlackList),
                   (noTP - noTP_BlackList),
                   (noTP - noTP_BlackList + noFN - noFN_BlackList),
                   (double)(noTP - noTP_BlackList) / (double)(noTP - noTP_BlackList + noFN - noFN_BlackList));
            Console.WriteLine("TP of BlackListed objects are not ignored");
            Console.WriteLine("Result: TP {0} / FP {1} / FN {2}\nPrecision {3}/{4}: {5}, Recall {6}/{7}: {8}",
                   noTP,
                   noFP - noFP_BlackList,
                   noFN - noFN_BlackList,
                   (noTP),
                   (noTP + noFP - noFP_BlackList),
                   (double)(noTP) / (double)(noTP + noFP - noFP_BlackList),
                   (noTP),
                   (noTP + noFN - noFN_BlackList),
                   (double)(noTP) / (double)(noTP + noFN - noFN_BlackList));
        }

        

        public void AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(List<string> guids, 
            Dictionary<string, double> IoUTreshold, 
            bool saveImage = false, bool prepareTrainingSet = false, string outputDirectory = null,
            int MinHeight = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_HEIGHT,
            int MaxOcclusion = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MAX_OCCLUSION,
            double Max_Truncation = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_TRUNCATION,
            int MinResults = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double CategoryMajorityThreshold = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MAJORITY_CATEGORY_THRESHOLD,
            double ObjectsCoverageThreshold = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_AGGREGATION_TERMINATION,
            double DeviationPixelThreshold = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_DEVIATION_THRESHOLD,
            bool overwrite = false,
            bool approvalAnalysis = false)
        {
            string configString = "Min_" + MinResults + "_Max_" + MaxResults + "_Dev_" + DeviationPixelThreshold + "_Ratio_" + ObjectsCoverageThreshold;
            Console.WriteLine("Aggregating for param set " + configString);
            if (!overwrite && File.Exists(DirectoryConstants.defaultTempDirectory + guids[0] + "\\" + configString + ".txt"))
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

            SortedDictionary<DateTime, List<SatyamResultsTableEntry>>entriesBySubmitTime = 
                SatyamResultValidationToolKit.SortResultsBySubmitTime_OneResultPerTurkerPerTask(entries);

            Dictionary<int, List<MultiObjectLocalizationAndLabelingResult>> ResultsPerTask = new Dictionary<int, List<MultiObjectLocalizationAndLabelingResult>>();
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
                        ResultsPerTask.Add(taskEntryID, new List<MultiObjectLocalizationAndLabelingResult>());
                    }
                    ////display ratio tweak
                    satyamResult.TaskResult = displayRatioTweak(satyamResult.TaskResult);

                    ResultsPerTask[taskEntryID].Add(JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingResult>(satyamResult.TaskResult));

                    // check log if enough results are collected
                    if (noResultsNeededForAggregation != null && noResultsNeededForAggregation.ContainsKey(taskEntryID)
                        && ResultsPerTask[taskEntryID].Count < noResultsNeededForAggregation[taskEntryID])
                    {
                        continue;
                    }

                    if (ResultsPerTask[taskEntryID].Count > MaxResults)
                    {
                        continue;
                    }

                    MultiObjectLocalizationAndLabelingAggregatedResult aggResult = MultiObjectLocalizationAndLabelingAggregator.getAggregatedResult(ResultsPerTask[taskEntryID], MinResults, MaxResults, CategoryMajorityThreshold, ObjectsCoverageThreshold, DeviationPixelThreshold);
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
                    SatyamAggResult.AggregatedResultString = JSonUtils.ConvertObjectToJSon<MultiObjectLocalizationAndLabelingAggregatedResult>(aggResult);
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

            List<Dictionary<int, bool>> BlackListedDetectionPerImage;
            string r = ValidateSatyamKITTIDetectionAggregationResult(aggEntries, IoUTreshold, out BlackListedDetectionPerImage, saveImage,  MinHeight, MaxOcclusion, Max_Truncation);
            r = noTotalConverged + " " + noTerminatedTasks + " " + r;
            File.WriteAllText(DirectoryConstants.defaultTempDirectory + guids[0] + "\\" + configString + ".txt", r);

            if (prepareTrainingSet)
            {
                if (!Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                writeAggregationDetectionResult_KITTIFormat(aggEntries, outputDirectory, NonBlackListedOnly:true, BlackListedDetectionPerImage:BlackListedDetectionPerImage);
            }
            if (approvalAnalysis)
            {
                string approvalString = configString + "_PayCover_" + TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_PAYMENT +
                "_PayDev_" + TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_DEVIATION_THRESHOLD_FOR_PAYMENT;
                //for (double ratio = 0; ratio < 1; ratio += 0.2)
                //{
                //    SatyamResultsAnalysis.AnalyzeApprovalRate(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guids[0], configString, approvalRatioThreshold: ratio);
                //}
                SatyamResultsAnalysis.AggregationAnalysis(aggEntries, entriesBySubmitTime, noResultsNeededForAggregation, noResultsNeededForAggregation_new, guids[0], configString);
            }
            
        }


        public static void writeAggregationDetectionResult_KITTIFormat(List<SatyamAggregatedResultsTableEntry> aggEntries, string outputDirectory,
            bool NonBlackListedOnly = false,
            List<Dictionary<int, bool>> BlackListedDetectionPerImage = null)
        {
            for (int i=0;i<aggEntries.Count;i++)
            {
                SatyamAggregatedResultsTableEntry entry = aggEntries[i];
                Dictionary<int, bool> DetectionBlackListed = BlackListedDetectionPerImage[i];
                List<string> outputs = new List<string>();
                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(entry);
                string fileName = URIUtilities.filenameFromURINoExtension(data.SatyamURI);

                Console.WriteLine("Saving {0}", fileName);

                string jobGUID = entry.JobGUID;
                int taskID = entry.SatyamTaskTableEntryID;
                String resultString = data.AggregatedResultString;
                MultiObjectLocalizationAndLabelingAggregatedResult result = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingAggregatedResult>(resultString);
                List<MultiObjectLocalizationAndLabelingResultSingleEntry> objects = result.boxesAndCategories.objects;

                for (int j=0;j<objects.Count;j++)
                {
                    if (DetectionBlackListed.ContainsKey(j) && DetectionBlackListed[j]) continue;

                    MultiObjectLocalizationAndLabelingResultSingleEntry o = objects[j];
                    BoundingBox box = o.boundingBox;
                    string s = o.Category + " 0 0 0 " + box.tlx.ToString() + " " + box.tly.ToString() + " " + box.brx.ToString() + " " + box.bry.ToString() + " 0 0 0 0 0 0 0";
                    outputs.Add(s);    
                }

                System.IO.File.WriteAllLines(outputDirectory + "\\" + fileName + ".txt", outputs.ToArray());
            }
        }

        public static string getKITTIDetectionFormatString(string category, BoundingBox box)
        {
            return category + " 0 0 0 " + box.tlx.ToString() + " " + box.tly.ToString() + " " + box.brx.ToString() + " " + box.bry.ToString() + " 0 0 0 0 0 0 0";
        }


        public static string getKITTIDetectionFormatString(MultiObjectLocalizationAndLabelingResultSingleEntry o)
        {
            string category = o.Category;
            BoundingBox box = o.boundingBox;
            return getKITTIDetectionFormatString(category, box);
        }

        public static void FilterGroundTruthFile(string filepath, string outputDir, out int omitted)
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            omitted = 0;
            string fileName = URIUtilities.filenameFromDirectoryNoExtension(filepath);
            KITTIDetectionGroundTruth gt = new KITTIDetectionGroundTruth(filepath);
            List<string> filteredGT = new List<string>();
            for (int i = 0; i < gt.objects.Count; i++)
            {
                if (gt.BlackListed[i])
                {
                    if (gt.objects[i].Category.ToLower() == "car")
                    {
                        omitted++;
                    }
                    continue;
                }
                filteredGT.Add(getKITTIDetectionFormatString(gt.objects[i]));
            }
            File.WriteAllLines(outputDir + "\\" + fileName + ".txt", filteredGT.ToArray());
        }




        public string displayRatioTweak(string result)
        {
            MultiObjectLocalizationAndLabelingResult tmpRes = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingResult>(result);
            if (tmpRes == null) return result;

            //double ratio = 1.03;
            int addition = 3;
            foreach (MultiObjectLocalizationAndLabelingResultSingleEntry entry in tmpRes.objects)
            {
                //entry.boundingBox.brx = (int)((double)entry.boundingBox.brx * ratio);
                //entry.boundingBox.bry = (int)((double)entry.boundingBox.bry * ratio);
                //entry.boundingBox.tlx = (int)((double)entry.boundingBox.tlx * ratio);
                //entry.boundingBox.tly = (int)((double)entry.boundingBox.tly * ratio);

                entry.boundingBox.brx += addition;
                entry.boundingBox.bry += addition;
                entry.boundingBox.tlx += addition;
                entry.boundingBox.tly += addition;
            }
            return JSonUtils.ConvertObjectToJSon(tmpRes);
        }
    }
}
