using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using AzureBlobStorage;
using SatyamTaskGenerators;
using SQLTables;
using SatyamResultAggregators;
using SatyamResultsSaving;
using SQLTableManagement;
using Utilities;
using AmazonMechanicalTurkAPI;
using SatyamAnalysis;
using Constants;
using SatyamResultValidation;
using System.IO;
using System.Xml.Linq;
using TFServingClient;

namespace Testing
{
    class Program
    {

        static void Main(string[] args)
        {

            //TestJobManagement.reopenGUID();
            //TestJobManagement.TestChangeGUIDPrice();

            PeriodicManagement.Run();
            //PeriodicManagement.RunLoop();

            ///Analyzer and Visualization
            //TestAnalyzeResult.TestSatyamResultAnalysis();
            //TestAnalyzeResult.TestGetGlobalStatistics();
            //TestAnalyzeResult.TestAggregationAnalysis();


            /// Image Classification Task
            //TestValidateClassificationResult.TestAggregateWithParameterValidateImageNetClassificationResult();
            //TestValidateClassificationResult.TestParamSweep();
            //TestValidateClassificationResult.TestValidateImageNet1000ClassDetectionResult();

            /// Video Classification Task
            //TestValidateClassificationResult.TestAggregateWithParameterAndValidateJHMDBVideoClassificationResult();
            //TestValidateClassificationResult.TestParamSweepVideoClassification();

            /// Object Counting Task
            //TestValidateObjectCountingResult.TestAggregateWithParameterValidateObjectCountingResult();
            //TestValidateObjectCountingResult.TestParamSweep();

            ///Detection Task
            //TestAnalyzeResult.TestSaveDetectionResult();
            //TestValidateDetectionResult.TestAggregateWithParameterVaidateKITTIResult();
            //TestValidateDetectionResult.TestParamSweep();
            //TestValidateDetectionResult.TestGetKITTIDetectionGroundTruthStatistics();
            //TestValidateDetectionResult.summarize();
            //TestValidateDetectionResult.TestFilterGroundTruthFile();


            /// Tracking Task
            //TestValidateTrackingResult.TestSavingTrackingAggregatedResult_KITTIFormat();
            //TestAnalyzeResult.TestVisualizeTrackingResult();
            //TestAnalyzeResult.TestVisualizeKITTITrackingResult();
            //TestValidateTrackingResult.TestVisualizeKITTITrackingGroundTruthUsingLocalImages();
            //TestValidateTrackingResult.TestVisualizeKITTIAggregatedGroundTruthUsingLocalImages();

            //TestValidateTrackingResult.TestParamSweep();
            //TestValidateTrackingResult.TestAggregateWithParameterAndValidateSatyamKITTITrackingAggregationResult();
            //TestValidateTrackingResult.TestGetKITTITrackingGroundTruthStatistics();
            //TestValidateTrackingResult.TestGetChunkPerformanceVSStatistics();

            /// Tracklet Labeling
            //TestAnalyzeResult.TestSaveTrackletLabelingResult();
            //TestValidateTrackletLabelingResult.TestAggregateWithParameter();

            /// Image Segmentation Task
            //TestValidateImageSegmentationResult.TestValidatePascalVOCImageSegmentationAggregationResult();
            //TestValidateImageSegmentationResult.DebugStaticOfflineAggregationWithParameterAndValidation();
            //TestAnalyzeResult.TestSaveImageSegmentationResult();

            /// Camera Pose Annotation Task
            //TestAnalyzeResult.TestSaveCamPoseResult();
            //TestAnalyzeResult.TestVisualizeCameraPoseAnnotationResult();

            /// data preparation
            //TestLongDurationDataPrep.SampleLongDurationData();
            //TestVATICDataPrep.TrimAnnotationFile();

            /// Open Questionair
            //TestAnalyzeResult.TestSaveOpenQuestionairResult();


            ///misc
            //TFServingTest.TestDetectionRequest();
            //TestTFServing.TestDetectionRequest();

            //TestMTurkPayments.TestManualAcceptReject();
            //TestFFMPEGWrappers.testAVI2MP4();
            //TestFFMPEGWrappers.stitchImage2Video();
            //TestAzureBlobStorage.RunTest();
            //TestFFMPEGWrappers.TestExtractFrame();

            //TestCreateDemoTasks();
            //TestTaskGeneration.RunTest();
            //TestResultAggregation.RunTest();
            //TestMTurkPayments.RunTest();
            //TestCleaningUpTaskTable.RunTest();
            //TestFinishingJobs.RunTest();
            //disposeAHITFromDVA();
            //MultiObjectLocalizationAndLabelingResultStringify.testJSONString();
            //approveHITSFromDVA();

            //KITTIDetectionResultValidation.saveGroundtruthImageWithDontCareBox(@"C:\research\dataset\KITTI\Detection\training\imageWithDontCare");


            //TestTracklet.TestReadTrackletsFromVIRAT();
            //TestTracklet.TestGetVideoChunksWithAnnotationsPerObject();

            //TestImageUtilities.TestGetImageFromLocalPath();
            //TestImageUtilities.TestSaveImageFromLocalPath();
            //TestImageUtilities.TestSaveImageFromURL();


        }
    }
}
