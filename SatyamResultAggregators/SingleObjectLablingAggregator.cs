using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using SatyamTaskResultClasses;
using SQLTables;
using Utilities;

namespace SatyamResultAggregators
{

    public class SingleObjectAggregatedResultMetaData
    {
        public int TotalCount;
        public Dictionary<string, int> CategoryCounts; 
    }

    public class SingleObjectLabelingAggregatedResult
    {
        public string Category;
        public SingleObjectAggregatedResultMetaData metaData;
    }
    public static class SingleObjectLabelingAggregator
    {
        public static List<string> filterCategories = new List<string>()
        {
            "ambulance",
            "None of the Above",
        };

        public static SingleObjectLabelingAggregatedResult getAggregatedResult(List<SingleObjectLabelingResult> results, 
            int MinResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE, 
            int MaxResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE, 
            double probabilityThreshold = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAJORITY_THRESHOLD)
        {
            if(results.Count < MinResults)
            {
                return null;
            }
            SingleObjectLabelingAggregatedResult aggresult = new SingleObjectLabelingAggregatedResult();

            Dictionary<string, int> resultCounts = new Dictionary<string, int>();

            foreach(SingleObjectLabelingResult result in results)
            {
                if (filterCategories.Contains(result.Category)) continue;
                if (!resultCounts.ContainsKey(result.Category))
                {
                    resultCounts.Add(result.Category,0);
                }
                resultCounts[result.Category]++;
            }

            //double probabilityThreshold = MajorityThreshold;

            List<string> categories = resultCounts.Keys.ToList();
            if (categories.Count == 0) return null;
            string aggCategory = "";
            if(categories.Count==1) //if all aggree and there are 3 or more the done
            {
                aggCategory = categories[0];
            }
            else
            {
                int maxCount = resultCounts[categories[0]];
                int index = 0;
                for(int i=1;i<categories.Count;i++)
                {
                    if(maxCount < resultCounts[categories[i]])
                    {
                        maxCount = resultCounts[categories[i]];
                        index = i;
                    }
                }
                double probability = ((double)maxCount+1) / ((double)results.Count+2);
                if(probability<probabilityThreshold && results.Count < MaxResults)
                {
                    return null;
                }
                aggCategory = categories[index];
            }

            SingleObjectAggregatedResultMetaData meta = new SingleObjectAggregatedResultMetaData();
            meta.TotalCount = results.Count;
            meta.CategoryCounts = resultCounts;

            aggresult.Category = aggCategory;
            aggresult.metaData = meta;
            return aggresult;
        }

        public static string GetAggregatedResultString(List<SatyamResultsTableEntry> results,
            int MinResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE,
            int MaxResults = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE,
            double probabilityThreshold = TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAJORITY_THRESHOLD)
        {
            string resultString = null;
            List<SingleObjectLabelingResult> resultList = new List<SingleObjectLabelingResult>();
            foreach (SatyamResultsTableEntry entry in results)
            {
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SingleObjectLabelingResult taskr = JSonUtils.ConvertJSonToObject<SingleObjectLabelingResult>(res.TaskResult);
                resultList.Add(taskr);
            }


            SingleObjectLabelingAggregatedResult r = getAggregatedResult(resultList, MinResults,MaxResults,probabilityThreshold);
            if(r != null)
            {
                string rString =  JSonUtils.ConvertObjectToJSon<SingleObjectLabelingAggregatedResult>(r);
                SatyamAggregatedResult aggResult = new SatyamAggregatedResult();
                aggResult.SatyamTaskTableEntryID = results[0].SatyamTaskTableEntryID;
                aggResult.AggregatedResultString = rString;
                SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
                aggResult.TaskParameters = res.TaskParametersString;
                resultString = JSonUtils.ConvertObjectToJSon<SatyamAggregatedResult>(aggResult);
            }
            return resultString;
        }


        public static bool IsAcceptable(SatyamAggregatedResultsTableEntry aggResult,  SatyamResultsTableEntry result) 
        {
            SatyamResult res = JSonUtils.ConvertJSonToObject<SatyamResult>(result.ResultString);
            SatyamAggregatedResult aggres = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(aggResult.ResultString);

            SingleObjectLabelingResult r = JSonUtils.ConvertJSonToObject<SingleObjectLabelingResult>(res.TaskResult);
            SingleObjectLabelingAggregatedResult ragg = JSonUtils.ConvertJSonToObject<SingleObjectLabelingAggregatedResult>(aggres.AggregatedResultString);
            if(r.Category == ragg.Category)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
