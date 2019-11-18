using Constants;
using SatyamResultValidation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Testing
{

    public class TestValidateClassificationResult
    {
        public static void TestAggregateWithParameterValidateImageNetClassificationResult()
        {
            //the 2img10class set "03977c5f-0e87-49ab-9c49-f20bfb47726a"
            //the allimg10class set "9c6283dd-f218-4588-a0d9-8c0afe841503"
            string guid = "9c6283dd-f218-4588-a0d9-8c0afe841503";
            string filePath = @"C:\research\MSRDVA\SatyamResearch\ConfusingImageList.txt";

            //ImageNetClassificationResultValidation.ValidateSatyamImageNetClassificationAggregationResultByGUID(guid, filePath);
            ImageNetClassificationResultValidation.AggregateWithParameterAndValidateSatyamImageNetClassificationResultByGUID(
                        guid, confusingImageListFilePath:filePath);


            //// validate and prepare dataset for training
            //string outputDirectory = DirectoryConstants.defaultTempDirectory + guid + "\\ResultDataForTraining\\";
            //ImageNetClassificationResultValidation.ValidateSatyamImageNetClassificationAggregationResultByGUID(guid, filePath, true, outputDirectory);
        }

        public static void TestParamSweep()
        {
            //the 2img10class set "03977c5f-0e87-49ab-9c49-f20bfb47726a"
            //the allimg10class set "9c6283dd-f218-4588-a0d9-8c0afe841503"
            string guid = "9c6283dd-f218-4588-a0d9-8c0afe841503";
            string filePath = @"C:\research\MSRDVA\SatyamResearch\ConfusingImageList.txt";
            
            /// eval -- param sweep
            List<double> majorityThresholds = new List<double>()
            {
                0.5, 0.6, 0.7, 0.8, 0.9
            };
            List<int> minResults = new List<int>()
            {
                2, 3 ,4, 5, 6, 7, 8, 9, 10
            };

            foreach (int minResult in minResults)
            {
                foreach (double majorityThresh in majorityThresholds)
                {
                    ImageNetClassificationResultValidation.AggregateWithParameterAndValidateSatyamImageNetClassificationResultByGUID(
                        guid, minResult, 20, majorityThresh, filePath);
                }
            }
        }


        public static void TestValidateImageNet1000ClassDetectionResult()
        {
            ImageNetClassificationResultValidation.validateImageNet1000ClassDetectionResult();

        }

        public static void TestAggregateWithParameterAndValidateJHMDBVideoClassificationResult()
        {
            string guid = "ca2a97e3-3c32-48cc-b7d9-fdd945b50f23"; //10 class human gesture in JHMDB
            string DefaultoutputFilepath = DirectoryConstants.defaultTempDirectory + "\\VideoClassification.txt";

            //JHMDBVideoClassifciationValidation.ValidateSatyamJHMDBVideoClassificationAggregationResultByGUID(guid, DefaultoutputFilepath);

            //// validate and prepare dataset for training
            //string outputDirectory = DirectoryConstants.defaultTempDirectory + guid + "\\ResultDataForTraining\\";
            //JHMDBVideoClassifciationValidation.ValidateSatyamJHMDBVideoClassificationAggregationResultByGUID(guid, DefaultoutputFilepath, true, outputDirectory);

            JHMDBVideoClassifciationValidation.AggregateWithParameterAndValidateSatyamVideoClassificationResultByGUID(guid);
        }

        public static void TestParamSweepVideoClassification()
        {
            string guid = "ca2a97e3-3c32-48cc-b7d9-fdd945b50f23"; //10 class human gesture in JHMDB
            string DefaultoutputFilepath = DirectoryConstants.defaultTempDirectory + "\\VideoClassification.txt";

            /// eval -- param sweep
            List<double> majorityThresholds = new List<double>()
            {
                0.5, 0.6, 0.7, 0.8, 0.9
            };
            List<int> minResults = new List<int>()
            {
                2, 3 ,4, 5, 6, 7, 8, 9, 10
            };
            foreach (int minResult in minResults)
            {
                foreach (double majorityThresh in majorityThresholds)
                {
                    JHMDBVideoClassifciationValidation.AggregateWithParameterAndValidateSatyamVideoClassificationResultByGUID(
                        guid, minResult, 20, majorityThresh);
                }
            }
        }
    }

    public class TestValidateObjectCountingResult
    {
        public static void TestAggregateWithParameterValidateObjectCountingResult()
        {
            //string guid = "58a85bc8-4fb9-4672-8719-38cf62352fbc"; // KITTI all 7481
            //string guid = "3b6b1625-a8ef-47dd-9fb6-27e0248f8283"; // first 100 img
            //string guid = "8c9990e5-52bf-4cc7-b39d-a47bed7c0e68"; // KITTI all with dontcare boxes
            string guid = "94979929-4315-4c81-9782-55c8955222a8"; // CARPK NTU 20161029


            //ObjectCountingValidation.ValidateSatyamKITTIObjectCountingAggregationResultByGUID(guid);
            ObjectCountingValidation.AggregateWithParameterAndValidateObjectCountingResultByGUID(guid);

        }

        public static void TestParamSweep()
        {
            //string guid = "58a85bc8-4fb9-4672-8719-38cf62352fbc"; // KITTI all 7481
            //string guid = "3b6b1625-a8ef-47dd-9fb6-27e0248f8283"; // first 100 img
            //string guid = "8c9990e5-52bf-4cc7-b39d-a47bed7c0e68"; // KITTI all with dontcare boxes
            string guid = "94979929-4315-4c81-9782-55c8955222a8"; // CARPK NTU 20161029


            // paramsweep
            for (int min = 2; min <= 10; min++)
            {
                for (double majority = 0.5; majority <= 0.9; majority += 0.1)
                {
                    for (double DeviationFraction = 0.05; DeviationFraction < 0.25; DeviationFraction += 0.05)
                    {
                        ObjectCountingValidation.AggregateWithParameterAndValidateObjectCountingResultByGUID(guid, false, null,
                        25, 1, 0.3, min, 20, 1.5, DeviationFraction, majority);
                    }

                }
            }
        }
    }



    public class TestValidateDetectionResult
    {
        public static void TestAggregateWithParameterVaidateKITTIResult()
        {
            List<string> guids = new List<string>()
            {
                ////KITTI set
                "3609af9c-e734-41dc-997c-94c060a1f63d",
                "5f14ecaa-1c4c-41d6-ad05-6ee0929e494e",
                "8fbc6ca1-e6c0-4720-93a3-1d49aa873a65",
                "d6f1c3ee-cd59-4197-b40a-0f45512596eb",
                //misc
                //"53d3aedc-7141-4951-a7fb-4beea9dc0d0b",
            };
            KITTIDetectionResultValidation validate = new KITTIDetectionResultValidation();
            //validate.ValidateSatyamKITTIDetectionAggregationResultByGUID("dca00b33-9336-4146-be3a-cc0e20fb3e8d");
            //validate.ValidateSatyamKITTIDetectionAggregationResultByGUID("b4ec157d-f8ac-4c46-9b81-f99869643e66");



            //KITTI major launch
            //validate.ValidateSatyamKITTIDetectionAggregationResultByGUID(guids);
            //validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids, new Dictionary<string, double>() { { "car", 0.5 }, { "pedestrian", 0.5}, { "cyclist", 0.5 }, { "default", 0.5 } }, overwrite: true);


            // save aggregated images
            //validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids, TaskConstants.IoUTreshold, true, overwrite: true);
            
            // prepare data for training
            string outputDirectory = DirectoryConstants.defaultTempDirectory + "\\KITTIDetectionResultDataForTraining\\";
            validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids, TaskConstants.IoUTreshold, false, true, outputDirectory, overwrite: true);

            //validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids,
            //    new Dictionary<string, double>() { { "car", 0.3 }, { "pedestrian", 0.3 }, { "cyclist", 0.3 }, { "default", 0.3 } },
            //        false, false, null, 25,1,0.3,5,12,0.6,0.9,15);
            
        }

        public static void TestParamSweep()
        {
            List<string> guids = new List<string>()
            {
                ////KITTI set
                //"3609af9c-e734-41dc-997c-94c060a1f63d",
                //"5f14ecaa-1c4c-41d6-ad05-6ee0929e494e",
                //"8fbc6ca1-e6c0-4720-93a3-1d49aa873a65",
                //"d6f1c3ee-cd59-4197-b40a-0f45512596eb",
                //misc
                //"53d3aedc-7141-4951-a7fb-4beea9dc0d0b",
                
                //"2c239102-e38f-4f0e-b639-6fae05250475", //detection KITTI 10
                "aefd12fc-42ed-4ec5-8c9a-6c41703a843f", //revision KITTI 10
                
            };
            KITTIDetectionResultValidation validate = new KITTIDetectionResultValidation();

            //string outputDirectory = DirectoryConstants.defaultTempDirectory + "\\KITTIDetectionResultDataForTraining\\";

            /////////////////////////// param sweep
            //for (int min = 2; min <= 10; min++)
            //{
            //    for (int Deviation = 5; Deviation < 21; Deviation += 5)
            //    {
            //        for (double coverage = 0.5; coverage < 1; coverage += 0.1)
            //        {
            //            validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids,
            //    new Dictionary<string, double>() { { "car", 0.3 }, { "pedestrian", 0.3 }, { "cyclist", 0.3 }, { "default", 0.3 } },
            //        //new Dictionary<string, double>() { { "car", 0.5 }, { "pedestrian", 0.5 }, { "cyclist", 0.5 }, { "default", 0.5 } },
            //        false, false, null, 25, 1, 0.3, min, 20, 0.6, coverage, Deviation, overwrite: true, approvalAnalysis:true);
            //        }
            //    }
            //}


            //param sweep min, to compare reivision with no revision template, setting coverage to 0, fixing deviation to 15pixel, not very sensitive anyway
            for (int min = 2; min <= 10; min++)
            {
                validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids,
                    new Dictionary<string, double>() { { "car", 0.3 }, { "pedestrian", 0.3 }, { "cyclist", 0.3 }, { "default", 0.3 } },
                    //new Dictionary<string, double>() { { "car", 0.5 }, { "pedestrian", 0.5 }, { "cyclist", 0.5 }, { "default", 0.5 } },
                    MinResults: min, overwrite: true);
            }

            //// param sweep max, produce the best effort aggregation, to compare reivision with no revision template, setting coverage to 0, fixing deviation to 15pixel, not very sensitive anyway
            //for (int min = 2; min <= 4; min++)
            //{
            //    for (int max = 2; max <= 8; max++)
            //    {
            //        validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids,
            //            new Dictionary<string, double>() { { "car", 0.3 }, { "pedestrian", 0.3 }, { "cyclist", 0.3 }, { "default", 0.3 } },
            //            //new Dictionary<string, double>() { { "car", 0.5 }, { "pedestrian", 0.5 }, { "cyclist", 0.5 }, { "default", 0.5 } },
            //            MinResults: min, MaxResults: max, overwrite: true);
            //    }
            //}

            //for (int max = 5; max<21;max += 5)
            //{
            //    validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids,
            //    new Dictionary<string, double>() { { "car", 0.3 }, { "pedestrian", 0.3 }, { "cyclist", 0.3 }, { "default", 0.3 } },
            //    //new Dictionary<string, double>() { { "car", 0.5 }, { "pedestrian", 0.5 }, { "cyclist", 0.5 }, { "default", 0.5 } },
            //        MaxResults:max);

            //}

            //List<Dictionary<string, double>> IoUTresholds = new List<Dictionary<string, double>>();
            //IoUTresholds.Add(new Dictionary<string, double>() { { "car", 0.4 }, { "pedestrian", 0.5 }, { "cyclist", 0.5 }, { "default", 0.5 } });
            //IoUTresholds.Add(new Dictionary<string, double>() { { "car", 0.5 }, { "pedestrian", 0.5 }, { "cyclist", 0.5 }, { "default", 0.5 } });
            //IoUTresholds.Add(new Dictionary<string, double>() { { "car", 0.6 }, { "pedestrian", 0.5 }, { "cyclist", 0.5 }, { "default", 0.5 } });
            //IoUTresholds.Add(new Dictionary<string, double>() { { "car", 0.7 }, { "pedestrian", 0.5 }, { "cyclist", 0.5 }, { "default", 0.5 } });
            ////MinHeight iou
            //foreach(Dictionary<string, double> IoUThreshold in IoUTresholds)
            //{
            //    for (int MinHeight = 10; MinHeight < 50; MinHeight += 5)
            //    {
            //        validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids, IoUThreshold, false, MinHeight);
            //    }
            //}

            //for (int Deviation = 5;Deviation < 21;Deviation += 5)
            //{
            //    for (int MinHeight = 10; MinHeight < 50; MinHeight += 5)
            //    {
            //        validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids, TaskConstants.IoUTreshold, false, MinHeight, DeviationPixelThreshold:Deviation);
            //    }
            //}

            //for (double coverage = 0.5; coverage < 1; coverage += 0.1)
            //{
            //    validate.AggregateWithParameterAndValidateSatyamKITTIDetectionResultByGUID(guids, TaskConstants.IoUTreshold, false, ObjectsCoverageThreshold:coverage);               
            //}

            summarize(guids[0]);

        }

        public static void TestGetKITTIDetectionGroundTruthStatistics()
        {
            KITTIDetectionResultValidation validate = new KITTIDetectionResultValidation();
            validate.CleanGroundTruthAndGetStatistics();
            //validate.CleanGroundTruthAndGetStatistics(@"C:\research\dataset\KITTI\Detection\10img\image_2", @"C:\research\dataset\KITTI\Detection\10img\label_2");
        }


        public static void summarize(string guid)
        {
            string resultDir = DirectoryConstants.defaultTempDirectory + guid + "\\";
            string outputFile = resultDir + "resultSummary.txt";
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }

            List<string> files = Directory.GetFiles(resultDir).ToList();
            SortedDictionary<double, SortedDictionary<double, SortedDictionary<double, SortedDictionary<double, string>>>> results = 
                new SortedDictionary<double, SortedDictionary<double, SortedDictionary<double, SortedDictionary<double, string>>>>();
            foreach(string file in files)
            {
                string filename = URIUtilities.filenameFromDirectoryNoExtension(file);
                string[] fields = filename.Split('_');
                double min = Convert.ToDouble(fields[1]);
                double max = Convert.ToDouble(fields[3]);
                double dev = Convert.ToDouble(fields[5]);
                double ratio = Convert.ToDouble(fields[7]);
                string content = File.ReadAllText(file).Replace("\n", String.Empty);
                if (!results.ContainsKey(min))
                {
                    results.Add(min, new SortedDictionary<double, SortedDictionary<double, SortedDictionary<double, string>>>());
                }
                if (!results[min].ContainsKey(max))
                {
                    results[min].Add(max, new SortedDictionary<double, SortedDictionary<double, string>>());
                }
                if (!results[min][max].ContainsKey(dev))
                {
                    results[min][max].Add(dev, new SortedDictionary<double, string>());
                }
                if (!results[min][max][dev].ContainsKey(ratio))
                {
                    results[min][max][dev].Add(ratio, filename + " " + content);
                }
            }

            List<string> contents = new List<string>();
            foreach( double m in results.Keys)
            {
                foreach (double n in results[m].Keys)
                {
                    foreach (double d in results[m][n].Keys)
                    {
                        foreach (double r in results[m][n][d].Keys)
                        {
                            contents.Add(results[m][n][d][r]);
                        }
                    }
                }
            }

            File.WriteAllLines(outputFile, contents.ToArray());
        }


        public static void TestFilterGroundTruthFile()
        {
            string outputDir = DirectoryConstants.defaultTempDirectory + "filteredKITTIGroundTruth\\";
            string outputDir_Seattle = outputDir + "SeattleLive-5-WestLake-NS\\";
            string outputDir_Stewart = outputDir + "1-Stewart-NS\\";
            string outputDir_BellevueNE = outputDir + "Bellevue-148th-NE29th\\";
            string outputDir_BellevueMain = outputDir + "Bellevue-148th-Main\\";

            string inputDir = @"C:\research\dataset\LongDuration\";
            string inputDir_Seattle = inputDir + "SeattleLive-5-WestLake-NS\\";
            string inputDir_Stewart = inputDir + "1-Stewart-NS\\";
            string inputDir_BellevueNE = inputDir + "Bellevue-148th-NE29th\\";
            string inputDir_BellevueMain = inputDir + "Bellevue-148th-Main\\";

            List<string> inputs = new List<string>()
            {
                inputDir_BellevueNE,
                inputDir_BellevueMain,
                inputDir_Seattle,
                inputDir_Stewart,
            };
            List<string> outputs = new List<string>()
            {
                outputDir_BellevueNE,
                outputDir_BellevueMain,
                outputDir_Seattle,
                outputDir_Stewart,
            };


            for (int i = 0; i < inputs.Count; i++)
            {
                string[] subdirs = Directory.GetDirectories(inputs[i] + "labels\\");
                foreach (string subdir in subdirs)
                {
                    int omitted = 0;
                    string[] files = Directory.GetFiles(subdir);
                    if (files.Length == 0)
                    {
                        string [] reward = Directory.GetDirectories(subdir);
                        files = Directory.GetFiles(reward[0]);
                    }
                    string groupName = URIUtilities.localDirectoryNameFromDirectory(subdir);
                    foreach (string file in files)
                    {
                        int tmp;
                        KITTIDetectionResultValidation.FilterGroundTruthFile(file, outputs[i] + groupName, out tmp);
                        omitted += tmp;
                    }
                    Console.WriteLine("{0}: omitted {1}", subdir, omitted);
                }
                
            }
        }
    }

    

    public class TestValidateTrackingResult
    {
        public static List<string> KITTIImageDirs = new List<string>()
            {
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0000_153frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0001_446frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0002_232frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0003_143frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0004_313frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0005_296frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0006_269frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0007_799frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0008_389frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0009_802frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0010_293frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0011_372frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0012_77frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0013_339frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0014_105frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0015_375frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0016_208frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0017_144frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0018_338frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0019_1058frame/",
                "https://satyamresearchjobstorage.blob.core.windows.net/kittitracking/all/Tracking_0020_836frame/"
            };

        public static string SatyamGroundtruthPath = @"C:\research\dataset\KITTI\Tracking\devkit\python\results\1e43a983-548d-4a2e-8161-5537eb985902\data\";

        public static void TestVisualizeKITTITrackingGroundTruthUsingLocalImages()
        {
            int count = 21;
            for (int i= 0;i < count;i++)
            {
                KITTITrackingGroundTruth gt = new KITTITrackingGroundTruth();
                gt.SaveGroundTruthImagesUsingLocalImages(DirectoryConstants.KITTITrackingLabels + i.ToString("0000") + ".txt", DirectoryConstants.KITTITrackingImages + i.ToString("0000") + "\\",
                DirectoryConstants.DefaultResultDirectory + "\\KITTI_Tracking\\");
            }
        }

        public static void TestVisualizeKITTIAggregatedGroundTruthUsingLocalImages()
        {
            int count = 21;
            for (int i = 0; i < count; i++)
            {
                KITTITrackingGroundTruth gt = new KITTITrackingGroundTruth();
                gt.SaveGroundTruthImagesUsingAzureBlobImages(SatyamGroundtruthPath + i.ToString("0000") + ".txt", KITTIImageDirs[i],
                DirectoryConstants.DefaultResultDirectory + "\\KITTI_Tracking_Test\\" + i.ToString("0000"));
            }
        }

        public static void TestGetKITTITrackingGroundTruthStatistics()
        {
            KITTITrackingResultValidation validate = new KITTITrackingResultValidation();
            validate.CleanGroundTruthAndGetStatistics(3);
        }

        public static void TestGetChunkPerformanceVSStatistics()
        {
            KITTITrackingResultValidation validate = new KITTITrackingResultValidation();
            SortedDictionary<int, List<string>> noTotalObjectFramesPerChunkHistogram = validate.CleanGroundTruthAndGetStatistics(3,1);

            string carFile = @"C:\research\dataset\KITTI\Tracking\devkit\python\chunkstatistics_car.txt";
            string pedFile = @"C:\research\dataset\KITTI\Tracking\devkit\python\chunkstatistics_ped.txt";
            Dictionary<string, int> totalFNs = new Dictionary<string, int>();
            Dictionary<string, int> FNs = new Dictionary<string, int>();
            Dictionary<string, int> GTs = new Dictionary<string, int>();
            Dictionary<string, int> totalGTs = new Dictionary<string, int>();
            parsePerformanceFile(carFile, out totalFNs, out totalGTs);
            parsePerformanceFile(carFile, out FNs, out GTs);
            foreach (string chunk in FNs.Keys)
            {
                totalFNs[chunk] += FNs[chunk];
                totalGTs[chunk]+= GTs[chunk];
            }
            foreach (string chunk in totalFNs.Keys)
            {
                foreach (int k in noTotalObjectFramesPerChunkHistogram.Keys)
                {
                    if (noTotalObjectFramesPerChunkHistogram[k].Contains(chunk))
                    {
                        if (totalGTs[chunk] == 0)
                        {
                            Console.WriteLine("{0},{1},{2}", chunk, totalGTs[chunk], 1);
                        }
                        else
                        {
                            //Console.WriteLine("{0},{1}", k, 1-(double)totalFNs[chunk] / (double)totalGTs[chunk]);
                            //Console.WriteLine("{0},{1}", totalGTs[chunk], 1 - (double)totalFNs[chunk] / (double)totalGTs[chunk]);
                            Console.WriteLine("{0},{1},{2}",chunk, totalGTs[chunk], totalFNs[chunk]);
                        }
                        
                    }
                }
            }
                
            
        }

        public static void parsePerformanceFile(string file, out Dictionary<string, int> FNs, out Dictionary<string, int> GTs)
        {
            FNs = new Dictionary<string, int>();
            GTs = new Dictionary<string, int>();
            string[] Performance = System.IO.File.ReadAllLines(file);
            foreach(string perf in Performance)
            {
                string[] fields = perf.Split(',');
                string chunkName = Convert.ToInt32(fields[1]).ToString("0000") + "_" + Convert.ToInt32(fields[3]).ToString();
                FNs.Add(chunkName, Convert.ToInt32(fields[5]));
                GTs.Add(chunkName, Convert.ToInt32(fields[7]));
            }
        }


        public static void TestSavingTrackingAggregatedResult_KITTIFormat()
        {
            //string resultDirectory = @"C:\research\dataset\KITTI\Tracking\devkit\python\results\";
            //string guid = "1e43a983-548d-4a2e-8161-5537eb985902";// KITTI Tracking all
            //string outputDirectory = resultDirectory + guid;

            string guid = "7ff1bdb4-d0e5-4d3d-aed1-92a184ba688b";// KITTI Tracking all
            //string guid = "2bb180e1-8bb7-410f-b49e-8f3ed8cd06f5";// small 0012 test
            string outputDirectory = DirectoryConstants.DefaultResultDirectory + guid;
            KITTITrackingResultValidation.SavingSatyamTrackingAggregationResultByGUID_KITTIFormat(guid,outputDirectory);
        }

        public static void TestAggregateWithParameterAndValidateSatyamKITTITrackingAggregationResult()
        {
            List<string> guids = new List<string>()
            {
                //"1be478ff-d390-47d7-9535-01b5dd744797", // 0017, $0.5 per chunk, 3 sec chunk, 0.5 overlap
                //"2bb180e1-8bb7-410f-b49e-8f3ed8cd06f5", // 0000, $0.5 per chunk, 3 sec chunk, 0.5 overlap
                "1e43a983-548d-4a2e-8161-5537eb985902", // KITTI all
                "77c4f6af-f669-42fb-925a-4ff670cf9ae2", // the missing 0012
                //"62e64a6f-0982-4471-bba0-4c0803d932d0", // 0011 $1 per chunk, 3 sec 0.5 overlap
                //"d9c96b28-6778-428d-b059-01a5f9cdad39", // 0011 $1 chunk, masters turkers only
                //"a206a07b-1ca2-462d-aa29-f2c020f29bf9", // KITTI all, $1 masters, 
            };            

            foreach (string guid in guids)
            {
                string outputDirectory = DirectoryConstants.defaultTempDirectory + guid;
                if (guid == "77c4f6af-f669-42fb-925a-4ff670cf9ae2")
                {
                    outputDirectory = DirectoryConstants.defaultTempDirectory + "1e43a983-548d-4a2e-8161-5537eb985902";
                }

                //// default test, 
                ////automatically write to KITTI format to prepare for training
                //KITTITrackingResultValidation.AggregateWithParameterAndValidateSatyamKITTITrackingAggregationResultByGUID(guid, outputDirectory, overwrite: true);

                KITTITrackingResultValidation.AggregateWithParameterAndValidateSatyamKITTITrackingAggregationResultByGUID(
                                guid, outputDirectory, 15, 0.9, 0.9, 3, 0.5, 7, 20, overwrite: true);
            }
        }

        public static void TestParamSweep()
        {
            List<string> guids = new List<string>()
            {
                //"1be478ff-d390-47d7-9535-01b5dd744797", // 0017, $0.5 per chunk, 3 sec chunk, 0.5 overlap
                //"2bb180e1-8bb7-410f-b49e-8f3ed8cd06f5", // 0000, $0.5 per chunk, 3 sec chunk, 0.5 overlap
                "1e43a983-548d-4a2e-8161-5537eb985902", // KITTI all
                "77c4f6af-f669-42fb-925a-4ff670cf9ae2", // the missing 0012
                //"62e64a6f-0982-4471-bba0-4c0803d932d0", // 0011 $1 per chunk, 3 sec 0.5 overlap
                //"d9c96b28-6778-428d-b059-01a5f9cdad39", // 0011 $1 chunk, masters turkers only
                //"a206a07b-1ca2-462d-aa29-f2c020f29bf9", // KITTI all, $1 masters, 
            };

            List<double> IoUs = new List<double>()
            {
                //0.1, 0.2, 0.3, 0.4,
                0.5
            };

            List<int> mins = new List<int>()
            {
                //2,
                3,
                //4,
                5,
                //6,
                7,
                //8, // very sensitive
                9,
                //10
            };

            List<double> ObjCoverage = new List<double>()
            {
                0.7,
                //0.75,
                0.8,
                //0.85,
                0.9
            };


            List<string> remainingConfigs = new List<string>();
            foreach (string guid in guids)
            {
                string outputDirectory = DirectoryConstants.defaultTempDirectory + guid;
                if (guid == "77c4f6af-f669-42fb-925a-4ff670cf9ae2")
                {
                    outputDirectory = DirectoryConstants.defaultTempDirectory + "1e43a983-548d-4a2e-8161-5537eb985902";
                }


                // param sweep
                foreach (int min in mins)
                {
                    foreach (double iou in IoUs)
                    {
                        foreach (double coverage in ObjCoverage)
                        {
                            int consensusNumber = 2;
                            bool success = KITTITrackingResultValidation.AggregateWithParameterAndValidateSatyamKITTITrackingAggregationResultByGUID(
                                guid, outputDirectory, 15, coverage, 0.9, consensusNumber, iou, min, 20, overwrite: true);
                            if (!success)
                            {
                                string configString = "GUID_" + guid + "_Min_" + min + "_Max_" + 20 + "_IoU_" + iou + "_Ratio_" + coverage;
                                remainingConfigs.Add(configString);
                            }
                        }
                    }
                }

                // Cherry Pick
                //foreach (int min in mins)
                //{
                //    foreach (double iou in IoUs)
                //    {
                //        double coverage = 0.9;
                //        KITTITrackingResultValidation.AggregateWithParameterAndValidateSatyamKITTITrackingAggregationResultByGUID(
                //                guid, outputDirectory, 15, coverage, 0.9, 2, iou, min, 20);

                //    }
                //}
                //foreach (int min in mins)
                //{
                //    foreach (double iou in IoUs)
                //    {
                //        if (iou != 0.4 && iou != 0.5) continue;
                //        foreach (double coverage in ObjCoverage)
                //        {
                //            if (coverage == 0.9) continue;
                //            KITTITrackingResultValidation.AggregateWithParameterAndValidateSatyamKITTITrackingAggregationResultByGUID(
                //                guid, outputDirectory, 15, coverage, 0.9, 2, iou, min, 20);
                //        }
                //    }
                //}

            }
            foreach (string config in remainingConfigs)
            {
                Console.WriteLine(config);
            }
        }
    }


    public class TestValidateTrackletLabelingResult
    {
        public static void TestAggregateWithParameter()
        {
            //string guid = "0efdc91e-62e2-4a70-b5fd-18720c170e03"; // test3
            //string guid = "c627ab13-f1c4-47d3-83e0-6df1cd3ff5e7"; //test 3 decouple attributes, all others than motion actions
            //string guid = "29b74b77-a8bf-4382-b998-190cb3491922"; // test 4 motion actions only
            string guid = "9a46e24d-42e2-4b4e-a7df-dffc307fc4d6"; // test 5, all false invalid

            TrackletLabelingValidation.AggregateWithParameter(guid);
            //TrackletLabelingValidation.AggregateWithParameter(guid,MinResults:5, allFalseAttributeInvalid: true);

        }
    }


    public class TestValidateImageSegmentationResult
    {
        public static void TestValidatePascalVOCImageSegmentationAggregationResult()
        {
            List<string> guids = new List<string>() {
                //"91a14bb7-3c13-4d6f-9044-438beae9e559",
                //"a2c3fe42-98b0-4abc-8b01-f59c0edbca2f",
                //"e14ec717-8689-43c9-b087-221c3b5201f2",// mturk test 
                "86cbfe43-175a-4a5a-ba89-073f3004e7df",//mturk all pascal
                //"61bf62b5-d796-4b3c-9de9-9c67f04cf26f",// pascal 5 after task page change
            };


            PascalVOCImageSegmentationValidation.ValidatePascalVOCImageSegmentationByGUID(guids, 0.5);
        }


        public static void TestAggregateWithParameterAndValidatePASCALVOCImageSegmentationResult()
        {
            List<string> guids = new List<string>() {
                //"91a14bb7-3c13-4d6f-9044-438beae9e559",
                //"a2c3fe42-98b0-4abc-8b01-f59c0edbca2f",
                //"e14ec717-8689-43c9-b087-221c3b5201f2",// mturk test 
                "86cbfe43-175a-4a5a-ba89-073f3004e7df",//mturk 20 class pascal all
            };
            
            PascalVOCImageSegmentationValidation.AggregateWithParameterAndValidatePascalVOCImageSegmentationByGUID(guids, 0.9);
        }

        public static void DebugStaticOfflineAggregationWithParameterAndValidation()
        {
            List<string> guids = new List<string>() {
                //"91a14bb7-3c13-4d6f-9044-438beae9e559",
                //"a2c3fe42-98b0-4abc-8b01-f59c0edbca2f",
                //"e14ec717-8689-43c9-b087-221c3b5201f2",// mturk test 
                "86cbfe43-175a-4a5a-ba89-073f3004e7df",//mturk 20 class pascal all
            };
            
            PascalVOCImageSegmentationValidation.StaticOfflineAggregationWithParameterAndValidation(guids, 0.5);
        }
    }
}