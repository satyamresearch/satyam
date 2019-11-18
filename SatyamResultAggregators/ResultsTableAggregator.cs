using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLTables;
using SatyamTaskResultClasses;
using Utilities;
using Constants;

namespace SatyamResultAggregators
{
    public static class ResultsTableAggregator
    {
        //public static void Aggregate()
        //{
        //    //first get all the results that are not aggregated
        //    SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
        //    List<SatyamResultsTableEntry> results = resultsDB.getEntriesByStatus();

        //    //first group all the results by the tasks since wach task may ahve multiple entries
        //    Dictionary<int, List<SatyamResultsTableEntry>> collectedResults = new Dictionary<int, List<SatyamResultsTableEntry>>();
        //    foreach (SatyamResultsTableEntry entry in results)
        //    {
        //        int taskID = entry.SatyamTaskTableEntryID;
        //        if (!collectedResults.ContainsKey(taskID))
        //        {
        //            collectedResults.Add(taskID, new List<SatyamResultsTableEntry>());
        //        }
        //        collectedResults[taskID].Add(entry);
        //    }

        //    //now aggreagate the results for eacht ask.
        //    List<int> taskIDList = collectedResults.Keys.ToList();
        //    List<SatyamAggregatedResultsTableEntry> aggEntries = new List<SatyamAggregatedResultsTableEntry>();
        //    foreach (int taskId in taskIDList)
        //    {
        //        SatyamAggregatedResultsTableEntry aggEntry = GetAggregatedResultString(taskId, collectedResults[taskId]);
        //        if (aggEntry != null)
        //        {
        //            aggEntries.Add(aggEntry);
        //        }
        //    }
        //    SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
        //    aggDB.AddEntries(aggEntries);
        //    resultsDB.close();
        //    aggDB.close();
        //}

        public static SatyamAggregatedResultsTableEntry GetAggregatedResultString(int taskId, List<SatyamResultsTableEntry> resultEntries)
        {
            SatyamAggregatedResultsTableEntry aggEntry = null;
            string templateType = resultEntries[0].JobTemplateType;
            string aggResultString = null;
            switch (templateType)
            {
                case TaskConstants.Classification_Image:
                case TaskConstants.Classification_Image_MTurk:
                case TaskConstants.Classification_Video:
                case TaskConstants.Classification_Video_MTurk:
                    aggResultString = SingleObjectLabelingAggregator.GetAggregatedResultString(resultEntries);
                    break;
                case TaskConstants.Counting_Image:
                case TaskConstants.Counting_Image_MTurk:
                case TaskConstants.Counting_Video:
                case TaskConstants.Counting_Video_MTurk:
                    aggResultString = ObjectCountingAggregator.GetAggregatedResultString(resultEntries);
                    break;
                case TaskConstants.Detection_Image:
                case TaskConstants.Detection_Image_MTurk:
                    aggResultString = MultiObjectLocalizationAndLabelingAggregator.GetAggregatedResultString(resultEntries);
                    break;
                case TaskConstants.Tracking:
                    aggResultString = null;//Done testing, marked null so it becomes a demo page.
                    break;
                case TaskConstants.Tracking_MTurk:
                    aggResultString = MultiObjectTrackingAggregator.GetAggregatedResultString(resultEntries);
                    break;
                case TaskConstants.TrackletLabeling:
                case TaskConstants.TrackletLabeling_MTurk:
                    aggResultString = TrackletLabelingAggregator.GetAggregatedResultString(resultEntries);
                    break;
                case TaskConstants.Segmentation_Image:
                    aggResultString = null;//Done testing, marked null so it becomes a demo page.
                    break;
                case TaskConstants.Segmentation_Image_MTurk:
                    aggResultString = ImageSegmentationAggregator.GetAggregatedResultString(resultEntries);
                    break;
                case TaskConstants.OpenEndedQuestion_Image:
                case TaskConstants.OpenEndedQuestion_Image_MTurk:
                    break;
            }
            if (aggResultString != null)
            {
                aggEntry = new SatyamAggregatedResultsTableEntry();
                aggEntry.JobGUID = resultEntries[0].JobGUID;
                aggEntry.JobTemplateType = resultEntries[0].JobTemplateType;
                aggEntry.SatyamTaskTableEntryID = resultEntries[0].SatyamTaskTableEntryID;
                aggEntry.UserID = resultEntries[0].UserID;
                aggEntry.ResultString = aggResultString;
            }
            return aggEntry;
        }


    }
}
