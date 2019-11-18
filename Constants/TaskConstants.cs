using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constants
{
    public static class DataFormat
    {
        public const string LiveStream = "Video";
        public const string Video = "Video";
        public const string Image = "Image";
        public const string VideoFrame = "Image";

    }


    public static class TaskConstants
    {
        public const bool DEBUG_MODE = true;
        public const string AdminID = "";//specify Admin Mechanical Turker ID
        public const string AdminName = "Admin";
        public const bool AdminPrivilege = false;

        public static string AzureBlobURL = ConfigConstants.AzureBlobURL;

        // keep the labling typo for now, changes will affect backward compatibility with old tasks and data
        public const string Classification_Image = "SINGLE_OBJECT_LABLING";
        public const string Classification_Image_MTurk = "SINGLE_OBJECT_LABLING_MTURK";
        public const string Classification_Video = "SINGLE_OBJECT_LABLING_IN_VIDEO";
        public const string Classification_Video_MTurk = "SINGLE_OBJECT_LABLING_IN_VIDEO_MTURK";
        public const string Detection_Image = "MULTI_OBJECT_LOCALIZATION_AND_LABLING";
        public const string Detection_Image_MTurk = "MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK";
        public const string Tracking = "MULTI_OBJECT_TRACKING";
        public const string Tracking_MTurk = "MULTI_OBJECT_TRACKING_MTURK";
        public const string Tracking_Demo = "MULTI_OBJECT_TRACKING_DEMO";
        public const string TrackletLabeling = "TRACKLET_LABELING";
        public const string TrackletLabeling_MTurk = "TRACKLET_LABELING_MTURK";
        public const string TrackletLabeling_DEMO = "TRACKLET_LABELING_DEMO";
        public const string Counting_Image = "OBJECT_COUNTING_IN_IMAGE";
        public const string Counting_Image_MTurk = "OBJECT_COUNTING_IN_IMAGE_MTURK";
        public const string Counting_Video = "OBJECT_COUNTING_IN_VIDEO";
        public const string Counting_Video_MTurk = "OBJECT_COUNTING_IN_VIDEO_MTURK";
        public const string Segmentation_Image = "OBJECT_SEGMENTATION_IN_IMAGE";
        public const string Segmentation_Image_MTurk = "OBJECT_SEGMENTATION_IN_IMAGE_MTURK";

        public const string OpenEndedQuestion_Image = "OPENENDEDQUESTION_IMAGE";
        public const string OpenEndedQuestion_Image_MTurk = "OPENENDEDQUESTION_IMAGE_MTURK";
        public const string CameraPoseEsitmation = "CAMERAPOSEESTIMATION";
        public const string CameraPoseEsitmation_MTurk = "CAMERAPOSEESTIMATION_MTURK";

        public static List<string> MTurkTaskTemplates = new List<string>()
        {
            Classification_Image_MTurk,
            Classification_Video_MTurk,
            Counting_Image_MTurk,
            Counting_Video_MTurk,
            Detection_Image_MTurk,
            Tracking_MTurk,
            TrackletLabeling_MTurk,
            Segmentation_Image_MTurk,
            OpenEndedQuestion_Image_MTurk,
            CameraPoseEsitmation_MTurk,
        };

        public static Dictionary<string, string> JobTemplateTypeToTaskURI = new Dictionary<string, string>()
        {
            { TaskConstants.Classification_Image, ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/SingleObjectLabeling.aspx" },
            { TaskConstants.Classification_Image_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/SingleObjectLabelingMTurk.aspx" },
            { TaskConstants.Classification_Video,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/SingleObjectLabelingInVideo.aspx" },
            { TaskConstants.Classification_Video_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/SingleObjectLabelingInVideoMTurk.aspx" },
            { TaskConstants.Counting_Image,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/ObjectCountingInImage.aspx" },
            { TaskConstants.Counting_Image_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/ObjectCountingInImageMTurk.aspx" },
            { TaskConstants.Counting_Video,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/ObjectCountingInVideo.aspx" },
            { TaskConstants.Counting_Video_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/ObjectCountingInVideoMTurk.aspx" },
             //NoRevision version.
            { TaskConstants.Detection_Image,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/MultiObjectLocalizationAndLabeling.aspx" },
            { TaskConstants.Detection_Image_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/MultiObjectLocalizationAndLabelingMTurk.aspx" },
            // Revision version. 
            //{ TaskConstants.Detection_Image,ConfigConstants.SERVER_ADDRESS + @"/MultiObjectDetectionRevisionTask.aspx" },
            //{ TaskConstants.Detection_Image_MTurk,ConfigConstants.SERVER_ADDRESS + @"/MultiObjectDetectionRevisionTaskMTurk.aspx" },

            { TaskConstants.Tracking,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/MultiObjectTracking.aspx" },
            { TaskConstants.Tracking_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/MultiObjectTrackingMTurk.aspx" },
            { TaskConstants.TrackletLabeling,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/TrackletLabeling.aspx" },
            { TaskConstants.TrackletLabeling_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/TrackletLabeling_MTurk.aspx" },
            { TaskConstants.Segmentation_Image,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/ImageSegmentation.aspx" },
            { TaskConstants.Segmentation_Image_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/ImageSegmentation_MTurk.aspx" },
            { TaskConstants.OpenEndedQuestion_Image,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/OpenEndedQuestionsForImage.aspx" },
            { TaskConstants.OpenEndedQuestion_Image_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/OpenEndedQuestionsForImage_MTurk.aspx" },
            { TaskConstants.CameraPoseEsitmation,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/CamPoseEstimation.aspx" },
            { TaskConstants.CameraPoseEsitmation_MTurk,ConfigConstants.TASKPAGE_SERVER_ADDRESS + @"/CamPoseEstimation_MTurk.aspx" },
        };
        
        //public static Dictionary<string, int> TasksPerHIT = new Dictionary<string, int>() {
        //    { TaskConstants.Classification_Image_MTurk, SINGLE_OBJECT_LABLING_MTURK_MAX_IMAGES_PER_TASK},
        //    { TaskConstants.Counting_Image_MTurk, OBJECT_COUNTING_MTURK_MAX_IMAGES_PER_TASK },
        //};
        //public static Dictionary<string,double> MinMoneyPerTask = new Dictionary<string, double>(){
        //    { TaskConstants.Detection_Image_MTurk, MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MIN_MONEY_PER_TASK},
        //};

        /// <summary>
        /// SINGLE OBJECT LABLING TASK
        /// </summary>
        public const int SINGLE_OBJECT_LABLING_MTURK_MAX_IMAGES_PER_TASK = 20;
        public const int SINGLE_OBJECT_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE = 10;
        public const int SINGLE_OBJECT_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE = 20;
        public const double SINGLE_OBJECT_LABLING_MTURK_MAJORITY_THRESHOLD = 0.9;
        /// <summary>
        /// SINGLE OBJECT LABLING IN VIDEO
        /// </summary>
        public const int SINGLE_OBJECT_LABLING_IN_VIDEO_MTURK_MAX_VIDEOS_PER_TASK = 10;

        /// <summary>
        /// OBJECT COUNTING
        /// </summary>
        public const int OBJECT_COUNTING_MTURK_MAX_IMAGES_PER_TASK = 20;
        public const int OBJECT_COUNTING_MTURK_MIN_RESULTS_TO_AGGREGATE = 3;//was 10
        public const int OBJECT_COUNTING_MTURK_MAX_RESULTS_TO_AGGREGATE = 5;//was 20
        public const double OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_LOWERBOUND = 1.5;
        public const double OBJECT_COUNTING_MTURK_MAX_ABSOLUTE_COUNT_DEVIATION_FOR_PAYMENT = 3;
        public const double OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION = 0.05; //5 percent
        public const double OBJECT_COUNTING_MTURK_MAX_DEVIATION_FRACTION_FOR_PAYMENT = 0.2;
        public const double OBJECT_COUNTING_MTURK_SUPER_MAJORITY_VALUE = 0.9;
        // counting is a different standard than detection, but whiten the blacklist would do the work, don't have to change it
        public const int OBJECT_COUNTING_VALIDATION_MIN_HEIGHT = 0;
        public const int OBJECT_COUNTING_VALIDATION_MAX_OCCLUSION = 5;
        public const double OBJECT_COUNTING_VALIDATION_MIN_TRUNCATION = 1;

        /// <summary>
        /// OBJECT COUNTING IN VIDEO
        /// </summary>
        public const int OBJECT_COUNTING_MTURK_MAX_VIDEOS_PER_TASK = 20;

        /// <summary>
        /// MULTI OBJECT LABLING AND LOCALIZATION
        /// </summary>
        public const double MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MIN_MONEY_PER_TASK = 0.05;//dollars
        public const int MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE = 5;
        public const int MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE = 20;
        public const double MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MAJORITY_CATEGORY_THRESHOLD = 0.6;
        public const double MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_AGGREGATION_TERMINATION = 0.9;
        public const double MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_PAYMENT = 0.5;
        public const int MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MIN_BOX_DIMENSION_FOR_CONSIDERATION = 15; //pixels
        public const int MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_DEVIATION_THRESHOLD = 5; //pixels
        public const int MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_DEVIATION_THRESHOLD_FOR_PAYMENT = 25; //pixels
        // kitti moderate standard
        public const int MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_HEIGHT = 25;
        public const int MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MAX_OCCLUSION = 1;
        public const double MULTI_OBJECT_LOCALIZATION_AND_LABLING_VALIDATION_MIN_TRUNCATION = 0.3;
        public static Dictionary<string, double> IoUTreshold =  new  Dictionary<string, double>()
        {
            {"car", 0.7},
            {"pedestrian", 0.5},
            {"cyclist", 0.5},
            {"default", 0.5},
        };

        /// <summary>
        /// MULTI OBJECT DETECTION REVISION TASK
        /// </summary>
        public const int MULTI_OBJECT_LOCALIZATION_AND_LABLING_MAX_DONE_SCORE = 10;//maximum stragglers tolerable for revision task

        /// <summary>
        /// IMAGE SEGMENTATION
        /// </summary>
        public const double IMAGE_SEGMENTATION_MTURK_MIN_MONEY_PER_TASK = 0.01;//dollars
        public const int IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_TO_AGGREGATE = 10;
        public const int IMAGE_SEGMENTATION_MTURK_MAX_RESULTS_TO_AGGREGATE = 10;
        public const double IMAGE_SEGMENTATION_MTURK_MAJORITY_CATEGORY_THRESHOLD = 0.6;
        public const double IMAGE_SEGMENTATION_MTURK_MAJORITY_POLYGON_BOUNDARY_THRESHOLD = 0.5; // because people would either draw larger or smaller boundaries?  assuming the expectation is correct, right on the correct boundary
        public const double IMAGE_SEGMENTATION_MTURK_POLYGON_IOU_THRESHOLD = 0.5; // for outlier removal
        public const double IMAGE_SEGMENTATION_MTURK_POLYGON_IOU_THRESHOLD_FOR_PAYMENT = 0.2;//to pay more
        public const double IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_AGGREGATION_TERMINATION = 0.9;
        public const double IMAGE_SEGMENTATION_MTURK_OBJECT_COVERAGE_THRESHOLD_FOR_PAYMENT = 0.3;
        public const int IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_FOR_CONSENSUS = 2;



        /// <summary>
        /// MULTI OBJECT TRAKCING
        /// </summary>
        public const double MULTI_OBJ_TRACKING_BOX_DEVIATION_THRESHOLD = 15;
        public const double MULTI_OBJ_TRACKING_APPROVALRATIO_PER_VIDEO = 0.9;
        public const double MULTI_OBJ_TRACKING_APPROVALRATIO_PER_VIDEO_FOR_PAYMENT = 0.7;
        public const double MULTI_OBJ_TRACKING_APPROVALRATIO_PER_TRACK = 0.9;
        public const int MULTI_OBJ_TRACKING_MAX_APPROVAL_DIFFERENCE = 2;
        public const int MULTI_OBJ_TRACKING_MIN_RESULTS_FOR_CONSENSUS = 3;
        public const double MULTI_OBJ_TRACKING_MIN_TUBELET_SIMILARITY_THRESHOLD = 0.5;
        public const double MULTI_OBJ_TRACKING_MIN_TUBELET_SIMILARITY_THRESHOLD_FOR_PAYMENT = 0.3;
        //public const int MULTI_OBJ_TRACKING_MTURK_MIN_RESULTS_TO_AGGREGATE = 10;
        //public const int MULTI_OBJ_TRACKING_MTURK_MAX_RESULTS_TO_AGGREGATE = 20;
        /// <summary>
        ///  production params
        /// </summary>
        public const int MULTI_OBJ_TRACKING_MTURK_MIN_RESULTS_TO_AGGREGATE = 7;
        public const int MULTI_OBJ_TRACKING_MTURK_MAX_RESULTS_TO_AGGREGATE = 15;

        public const int MULTI_OBJ_TRACKING_VALIDATION_MAX_OCCLUSION = 1;
        public const double MULTI_OBJ_TRACKING_VALIDATION_MIN_TRUNCATION = 0;

        public static List<string> masterGUIDs = new List<string>()
        {
            "a206a07b-1ca2-462d-aa29-f2c020f29bf9", // masters all
            "d9c96b28-6778-428d-b059-01a5f9cdad39", // masters 0011 only
        };


        /// <summary>
        /// TRACKLET LABELING
        /// </summary>
        public const double TRACKLET_LABELING_BOX_DEVIATION_THRESHOLD = 15;
        public const double TRACKLET_LABELING_APPROVALRATIO_PER_VIDEO = 0.9;
        public const double TRACKLET_LABELING_APPROVALRATIO_PER_TRACK = 0.9;
        public const int TRACKLET_LABELING_MIN_RESULTS_FOR_CONSENSUS = 2;
        public const double TRACKLET_LABELING_MIN_TUBELET_SIMILARITY_THRESHOLD = 0.5;
        public const double TRACKLET_LABELING_MIN_TUBELET_SIMILARITY_THRESHOLD_FOR_PAYMENT = 0.2;
        public const int TRACKLET_LABELING_MTURK_MIN_RESULTS_TO_AGGREGATE = 8;
        public const int TRACKLET_LABELING_MTURK_MAX_RESULTS_TO_AGGREGATE = 50;
        public const double TRACKLET_LABELING_MTURK_ATTRIBUTE_MAJORITY_THRESHOLD = 0.25;



        public static int getMinResultsByTemplate(string TemplateName)
        {
            int MinResults = -1;
            switch (TemplateName)
            {
                case TaskConstants.Classification_Image:
                case TaskConstants.Classification_Image_MTurk:
                case TaskConstants.Classification_Video:
                case TaskConstants.Classification_Video_MTurk:
                    MinResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE;
                    break;
                case TaskConstants.Counting_Image:
                case TaskConstants.Counting_Image_MTurk:
                case TaskConstants.Counting_Video:
                case TaskConstants.Counting_Video_MTurk:
                    MinResults = TaskConstants.OBJECT_COUNTING_MTURK_MIN_RESULTS_TO_AGGREGATE;
                    break;
                case TaskConstants.Detection_Image:
                case TaskConstants.Detection_Image_MTurk:
                    MinResults = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE;
                    break;
                case TaskConstants.Tracking:
                case TaskConstants.Tracking_MTurk:
                    MinResults = TaskConstants.MULTI_OBJ_TRACKING_MTURK_MIN_RESULTS_TO_AGGREGATE;
                    break;
                case TaskConstants.TrackletLabeling:
                case TaskConstants.TrackletLabeling_MTurk:
                    MinResults = TaskConstants.TRACKLET_LABELING_MTURK_MIN_RESULTS_TO_AGGREGATE;
                    break;
                case TaskConstants.Segmentation_Image:
                case TaskConstants.Segmentation_Image_MTurk:
                    MinResults = TaskConstants.IMAGE_SEGMENTATION_MTURK_MIN_RESULTS_TO_AGGREGATE;
                    break;
                case TaskConstants.OpenEndedQuestion_Image:
                case TaskConstants.OpenEndedQuestion_Image_MTurk:
                    break;
                default:
                    break;
            }
            return MinResults;
        }
    }
}
