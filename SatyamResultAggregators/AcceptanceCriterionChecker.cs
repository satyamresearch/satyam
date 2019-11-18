using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using SQLTables;

namespace SatyamResultAggregators
{
    public static class AcceptanceCriterionChecker
    {
        public static bool IsAcceptable(SatyamAggregatedResultsTableEntry aggEntry, SatyamResultsTableEntry result)
        {
            switch (result.JobTemplateType)
            {
                case TaskConstants.Classification_Image:
                case TaskConstants.Classification_Image_MTurk:
                case TaskConstants.Classification_Video:
                case TaskConstants.Classification_Video_MTurk:
                    return SingleObjectLabelingAggregator.IsAcceptable(aggEntry, result);
                case TaskConstants.Counting_Image:
                case TaskConstants.Counting_Image_MTurk:
                case TaskConstants.Counting_Video:
                case TaskConstants.Counting_Video_MTurk:
                    return ObjectCountingAggregator.IsAcceptable(aggEntry, result);
                case TaskConstants.Detection_Image:
                case TaskConstants.Detection_Image_MTurk:
                    return MultiObjectLocalizationAndLabelingAggregator.IsAcceptable(aggEntry, result);
                case TaskConstants.Tracking:
                case TaskConstants.Tracking_MTurk:
                    return MultiObjectTrackingAggregator.IsAcceptable(aggEntry, result);
                case TaskConstants.Segmentation_Image:
                case TaskConstants.Segmentation_Image_MTurk:
                    return ImageSegmentationAggregator.IsAcceptable(aggEntry, result);
                case TaskConstants.OpenEndedQuestion_Image:
                case TaskConstants.OpenEndedQuestion_Image_MTurk:
                    return true; //TBD
                case TaskConstants.TrackletLabeling:
                case TaskConstants.TrackletLabeling_MTurk:
                    return TrackletLabelingAggregator.IsAcceptable(aggEntry, result);

            }
            return false;
        }
    }
}
