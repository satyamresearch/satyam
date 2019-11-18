using Constants;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SatyamResultValidation
{
    public class SatyamResultValidationToolKit
    {
        public static SortedDictionary<DateTime, List<SatyamResultsTableEntry>> SortResultsBySubmitTime(List<SatyamResultsTableEntry> entries)
        {
            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime = new SortedDictionary<DateTime, List<SatyamResultsTableEntry>>();
            Dictionary<int, List<string>> WorkersPerTask = new Dictionary<int, List<string>>();
            foreach (SatyamResultsTableEntry entry in entries)
            {
                if (!entriesBySubmitTime.ContainsKey(entry.SubmitTime))
                {
                    entriesBySubmitTime.Add(entry.SubmitTime, new List<SatyamResultsTableEntry>());
                }
                entriesBySubmitTime[entry.SubmitTime].Add(entry);
            }
            return entriesBySubmitTime;
        }

        public static SortedDictionary<DateTime, List<SatyamResultsTableEntry>> SortResultsBySubmitTime_OneResultPerTurkerPerTask(List<SatyamResultsTableEntry> entries)
        {
            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime = new SortedDictionary<DateTime, List<SatyamResultsTableEntry>>();
            Dictionary<int, List<string>> WorkersPerTask = new Dictionary<int, List<string>>();
            foreach (SatyamResultsTableEntry entry in entries)
            {
                if (!WorkersPerTask.ContainsKey(entry.SatyamTaskTableEntryID))
                {
                    WorkersPerTask.Add(entry.SatyamTaskTableEntryID, new List<string>());
                }
                string workerID = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString).amazonInfo.WorkerID;
                if (!WorkersPerTask[entry.SatyamTaskTableEntryID].Contains(workerID))
                {
                    //enclose only non-duplicate results, one per each worker.
                    WorkersPerTask[entry.SatyamTaskTableEntryID].Add(workerID);
                    if (!entriesBySubmitTime.ContainsKey(entry.SubmitTime))
                    {
                        entriesBySubmitTime.Add(entry.SubmitTime, new List<SatyamResultsTableEntry>());
                    }
                    entriesBySubmitTime[entry.SubmitTime].Add(entry);
                }
            }
            return entriesBySubmitTime;
        }


        public static void getConfusionMat(List<KeyValuePair<string, string>> dets_gts,
            out SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth,
            out SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res)
        {
            confusionMatrix_res_groundtruth = new SortedDictionary<string, Dictionary<string, int>>();
            confusionMatrix_groundtruth_res = new SortedDictionary<string, Dictionary<string, int>>();
            for (int i = 0; i < dets_gts.Count; i++)
            {
                string dets = dets_gts[i].Key;
                string gts = dets_gts[i].Value;
                if (!confusionMatrix_res_groundtruth.ContainsKey(dets))
                {
                    confusionMatrix_res_groundtruth.Add(dets, new Dictionary<string, int>());
                }
                if (!confusionMatrix_res_groundtruth[dets].ContainsKey(gts))
                {
                    confusionMatrix_res_groundtruth[dets].Add(gts, 0);
                }
                if (!confusionMatrix_groundtruth_res.ContainsKey(gts))
                {
                    confusionMatrix_groundtruth_res.Add(gts, new Dictionary<string, int>());
                }
                if (!confusionMatrix_groundtruth_res[gts].ContainsKey(dets))
                {
                    confusionMatrix_groundtruth_res[gts].Add(dets, 0);
                }
                confusionMatrix_res_groundtruth[dets][gts]++;
                confusionMatrix_groundtruth_res[gts][dets]++;
            }
        }

        public static void printConfusionMatrix(SortedDictionary<string, Dictionary<string, int>> confusionMatrix_groundtruth_res,
            SortedDictionary<string, Dictionary<string, int>> confusionMatrix_res_groundtruth,
            string outputFile)
        {
            string row = "\t";
            foreach (string resultCategory in confusionMatrix_res_groundtruth.Keys)
            {
                row += resultCategory + "\t";
            }
            row += "\n";
            foreach (string groundTruthCategory in confusionMatrix_groundtruth_res.Keys)
            {
                row += groundTruthCategory + "\t";
                foreach (string resultCategory in confusionMatrix_res_groundtruth.Keys)
                {
                    if (confusionMatrix_groundtruth_res[groundTruthCategory].ContainsKey(resultCategory))
                    {
                        row += confusionMatrix_groundtruth_res[groundTruthCategory][resultCategory].ToString();
                    }
                    else
                    {
                        row += "0";
                    }
                    row += "\t";
                }
                row += "\n";
            }
            Console.WriteLine(row);
            File.WriteAllText(outputFile, row);
        }
    }
}
