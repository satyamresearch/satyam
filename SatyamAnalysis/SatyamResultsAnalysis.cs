using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using SatyamResultsSaving;
using SQLTables;
using SatyamTaskResultClasses;
using Utilities;
using JobTemplateClasses;
using SatyamTaskGenerators;
using Constants;
using SatyamResultAggregators;

namespace SatyamAnalysis
{

    public class SatyamResultPerJobDataAnalysis
    {
        public string JobGUID;
        public SatyamJob job;
        public int noTasks;
        public int totalResults;
        public int totalAcceptedResults;
        public int totalpaidResults;
        public double totalMoneyPaid;
        public int noTasksWithDuplicateResultsFromSameWorker;
        public int noDuplicateResultsFromSameWorker;
        public int noTaskWithDuplicateResultsAsMajority;
        public int noTaskWhoseDuplicateResultsChangedAggregation;
        public int noTaskWhoseDuplicateResultsChangedAggregationIncorrectly;
        public int noDecisionChangeAmongDuplicateResultsOfSameWorkerSameTask;
        public int noCorrectDecisionsAmongDuplicates;
        public int noTasksWithMixedResultsFromSameWorker;
        //public int noCorrectDuplicateResultsOfSameWorkerSameTask;
        public int noTasksWithCorrectDuplicateResultsOfSameWorkerSameTask;
        public int noWorkerSwitchedToCorrect;
        public int noWorkerSwitchedToCorrectAndMaintained;
        public int noTasksWithWorkerSwitchedToCorrect;
        public int noTasksWithWorkerSwitchedToCorrectAndMaintained;
        public Dictionary<int, int> noResultsPerTask;
        public Dictionary<int, int> noAcceptedResultsPerTask;
        public Dictionary<int, int> noPaidResultsPerTask;
        public Dictionary<int, double> moneyPaidPerTask;
        public SortedDictionary<int, double> PricePerResult;
        public SortedDictionary<int, Dictionary<string, List<string>>> ResultsPerWorkerPerTask;
        public SortedDictionary<int, Dictionary<string, int>> noDifferentDuplicateResultsPerWorkerPerTask;
        public Dictionary<int, string> taskSatyamUri;
        public Dictionary<int, double> timeTakenTillAggregationPerTask;

        public Dictionary<int, DateTime> finalTaskEndTime;
        public Dictionary<string, double> finalHITEndTime; //seconds from start

        public List<double> resultArrivalTimes; //seconds from start
        public List<double> timeTakenPerResult;
        public Dictionary<int, List<double>> TimePerSequenceNumber;
        public Dictionary<int, List<double>> TimePerTask;
        public Dictionary<int, List<double>> AcceptedTimePerSequenceNumber;
        public Dictionary<int, List<double>> AcceptedTimePerTask;
        public List<double> acceptedTimeTakenPerResult;
        public List<int> SequenceNumberOfResultPerTask;
        public List<int> SequenceNumberOfResultPerWorker;



        public double medianTimeTakenPerResultInSeconds;
        public double medianAcceptedTimeTakenPerResultInSeconds;

        public double avgNoPaidResultsPerTask;

        public Dictionary<string, int> noJobsPerWorker;
        public Dictionary<string, int> noAcceptedJobsPerWorker;
        public Dictionary<string, int> noPaidJobsPerWorker;
        public DateTime jobSubmitTime;
        public DateTime lastTaskEndTime;
        public TimeSpan totalTimeTakenForJob;
        public SortedDictionary<int, int> resultsPerTaskHistogram;
        public SortedDictionary<int, int> resultsAcceptedPerTaskHistogram;
        public SortedDictionary<int, int> resultsPaidPerTaskHistogram;
        public SortedDictionary<int, int> moneyPaidPerTaskHistogram;
        public SortedDictionary<int, int> ResultsPerWorkerPerTaskHistogram;


        public SatyamResultPerJobDataAnalysis()
        {
            noResultsPerTask = new Dictionary<int, int>();
            noAcceptedResultsPerTask = new Dictionary<int, int>();
            noPaidResultsPerTask = new Dictionary<int, int>();
            moneyPaidPerTask = new Dictionary<int, double>();
            PricePerResult = new SortedDictionary<int, double>();
            ResultsPerWorkerPerTask = new SortedDictionary<int, Dictionary<string, List<string>>>();
            noDifferentDuplicateResultsPerWorkerPerTask = new SortedDictionary<int, Dictionary<string, int>>();
            taskSatyamUri = new Dictionary<int, string>();
            timeTakenTillAggregationPerTask = new Dictionary<int, double>();
            finalTaskEndTime = new Dictionary<int, DateTime>();
            finalHITEndTime = new Dictionary<string, double>();
            totalResults = 0;
            totalAcceptedResults = 0;
            totalpaidResults = 0;
            totalMoneyPaid = 0;
            noTasksWithDuplicateResultsFromSameWorker = 0;
            noTasksWithMixedResultsFromSameWorker = 0;
            noDuplicateResultsFromSameWorker = 0;
            noTaskWithDuplicateResultsAsMajority = 0;
            noTaskWhoseDuplicateResultsChangedAggregation = 0;
            noTaskWhoseDuplicateResultsChangedAggregationIncorrectly = 0;
            noDecisionChangeAmongDuplicateResultsOfSameWorkerSameTask = 0;
            noCorrectDecisionsAmongDuplicates = 0;

            avgNoPaidResultsPerTask = 0;

            //ana.noCorrectDuplicateResultsOfSameWorkerSameTask = 0;
            noTasksWithCorrectDuplicateResultsOfSameWorkerSameTask = 0;
            noWorkerSwitchedToCorrect = 0;
            noWorkerSwitchedToCorrectAndMaintained = 0;
            noTasksWithWorkerSwitchedToCorrect = 0;
            noTasksWithWorkerSwitchedToCorrectAndMaintained = 0;

            timeTakenPerResult = new List<double>();
            TimePerSequenceNumber = new Dictionary<int, List<double>>();
            TimePerTask = new Dictionary<int, List<double>>();
            AcceptedTimePerSequenceNumber = new Dictionary<int, List<double>>();
            AcceptedTimePerTask = new Dictionary<int, List<double>>();
            acceptedTimeTakenPerResult = new List<double>();
            SequenceNumberOfResultPerTask = new List<int>();
            SequenceNumberOfResultPerWorker = new List<int>();

           


            noJobsPerWorker = new Dictionary<string, int>();
            noAcceptedJobsPerWorker = new Dictionary<string, int>();
            noPaidJobsPerWorker = new Dictionary<string, int>();


            resultArrivalTimes = new List<double>();
        }
    }

    public static class SatyamResultsAnalysis
    {
        public static SatyamResultPerJobDataAnalysis AnalyzeFromResultsTable(List<string> jobGUIDs)
        {
            Console.WriteLine("Analyzing Results Statistics of {0}", jobGUIDs[0]);
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = new List<SatyamResultsTableEntry>();
            foreach (string guid in jobGUIDs)
            {
                entries.AddRange(resultsDB.getEntriesByGUIDOrderByID(guid));
            }
            
            resultsDB.close();

            if (entries.Count == 0) return null;

            SatyamResultPerJobDataAnalysis ana = new SatyamResultPerJobDataAnalysis();
            ana.JobGUID = entries[0].JobGUID;
            ana.totalResults = entries.Count;

            for (int i=0;i<entries.Count;i++)
            {
                SatyamResultsTableEntry entry = entries[i];
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                SatyamJob job = task.jobEntry;


                if(ana.job == null)
                {
                    ana.job = job;
                }


                ana.jobSubmitTime = job.JobSubmitTime;

                double moneyPerResult = 0;
                if (job.TasksPerJob > 0)
                {
                    moneyPerResult = (double)satyamResult.amazonInfo.PricePerHIT / (double)job.TasksPerJob;
                }

                ana.PricePerResult.Add(entry.ID, moneyPerResult);


                int taskEntryID = entry.SatyamTaskTableEntryID;
                if(!ana.noResultsPerTask.ContainsKey(taskEntryID))
                {
                    ana.noResultsPerTask.Add(taskEntryID, 0);
                    ana.noAcceptedResultsPerTask.Add(taskEntryID, 0);
                    ana.noPaidResultsPerTask.Add(taskEntryID, 0);
                    ana.moneyPaidPerTask.Add(taskEntryID, 0);
                    ana.finalTaskEndTime.Add(taskEntryID, entry.SubmitTime);
                    
                    ana.ResultsPerWorkerPerTask.Add(taskEntryID, new Dictionary<string, List<string>>());
                    ana.noDifferentDuplicateResultsPerWorkerPerTask.Add(taskEntryID, new Dictionary<string, int>());
                    ana.taskSatyamUri.Add(taskEntryID, task.SatyamURI);
                }
                ana.noResultsPerTask[taskEntryID]++;
                ana.SequenceNumberOfResultPerTask.Add(ana.noResultsPerTask[taskEntryID]);
                if(entry.Status == ResultStatus.accepted || entry.Status == ResultStatus.accepted_Paid || entry.Status == ResultStatus.accepted_NotPaid)
                {
                    ana.noAcceptedResultsPerTask[taskEntryID]++;
                    ana.totalAcceptedResults++;
                }
                if (entry.Status == ResultStatus.accepted_Paid || entry.Status == ResultStatus.rejected_Paid)
                {
                    ana.noPaidResultsPerTask[taskEntryID]++;
                    ana.totalpaidResults++;
                    ana.moneyPaidPerTask[taskEntryID] += moneyPerResult;
                    ana.totalMoneyPaid += moneyPerResult;
                }
                if(ana.finalTaskEndTime[taskEntryID] < entry.SubmitTime)
                {
                    ana.finalTaskEndTime[taskEntryID] = entry.SubmitTime;
                }
                
                if (ana.lastTaskEndTime < entry.SubmitTime)
                {
                    ana.lastTaskEndTime = entry.SubmitTime;
                }
                TimeSpan dt = satyamResult.TaskEndTime - satyamResult.TaskStartTime;
                ana.timeTakenPerResult.Add(dt.TotalSeconds);

                if (!ana.AcceptedTimePerSequenceNumber.ContainsKey(ana.noResultsPerTask[taskEntryID]))
                {
                    ana.AcceptedTimePerSequenceNumber.Add(ana.noResultsPerTask[taskEntryID], new List<double>());
                    ana.TimePerSequenceNumber.Add(ana.noResultsPerTask[taskEntryID], new List<double>());
                }
                //ana.acceptedTimeTakenPerResultOrganizedByResultSequencePerTask[ana.noResultsPerTask[taskEntryID]].Add(dt.TotalSeconds);
                ana.TimePerSequenceNumber[ana.noResultsPerTask[taskEntryID]].Add(dt.TotalSeconds);

                if (!ana.AcceptedTimePerTask.ContainsKey(taskEntryID))
                {
                    ana.AcceptedTimePerTask.Add(taskEntryID, new List<double>());
                    ana.TimePerTask.Add(taskEntryID, new List<double>());
                }
                //ana.AcceptedTimePerTask[taskEntryID].Add(dt.TotalSeconds);
                ana.TimePerTask[taskEntryID].Add(dt.TotalSeconds);


                if (ana.lastTaskEndTime < satyamResult.TaskEndTime)
                {
                    ana.lastTaskEndTime = satyamResult.TaskEndTime;
                }

                if (!ana.finalHITEndTime.ContainsKey(satyamResult.amazonInfo.HITID))
                {
                    ana.finalHITEndTime.Add(satyamResult.amazonInfo.HITID, (entry.SubmitTime - job.JobSubmitTime).TotalSeconds);
                }
                
                if (ana.finalHITEndTime[satyamResult.amazonInfo.HITID] < (entry.SubmitTime - job.JobSubmitTime).TotalSeconds)
                {
                    ana.finalHITEndTime[satyamResult.amazonInfo.HITID] = (entry.SubmitTime - job.JobSubmitTime).TotalSeconds;
                }


                if (entry.Status == ResultStatus.accepted || entry.Status == ResultStatus.accepted_Paid || entry.Status == ResultStatus.accepted_NotPaid)
                {
                    ana.acceptedTimeTakenPerResult.Add(dt.TotalSeconds);
                    ana.AcceptedTimePerTask[taskEntryID].Add(dt.TotalSeconds);
                    ana.AcceptedTimePerSequenceNumber[ana.noResultsPerTask[taskEntryID]].Add(dt.TotalSeconds);
                }

                ana.resultArrivalTimes.Add((entry.SubmitTime-job.JobSubmitTime).TotalSeconds);


                if(!ana.noJobsPerWorker.ContainsKey(satyamResult.amazonInfo.WorkerID))
                {
                    ana.noJobsPerWorker.Add(satyamResult.amazonInfo.WorkerID,0);
                    ana.noAcceptedJobsPerWorker.Add(satyamResult.amazonInfo.WorkerID, 0);
                    ana.noPaidJobsPerWorker.Add(satyamResult.amazonInfo.WorkerID, 0);
                }
                ana.noJobsPerWorker[satyamResult.amazonInfo.WorkerID]++;
                ana.SequenceNumberOfResultPerWorker.Add(ana.noJobsPerWorker[satyamResult.amazonInfo.WorkerID]);
                if(entry.Status == ResultStatus.accepted || entry.Status == ResultStatus.accepted_Paid || entry.Status == ResultStatus.accepted_NotPaid)
                {
                    ana.noAcceptedJobsPerWorker[satyamResult.amazonInfo.WorkerID]++;
                }
                if (entry.Status == ResultStatus.accepted_Paid || entry.Status == ResultStatus.rejected_Paid)
                {
                    ana.noPaidJobsPerWorker[satyamResult.amazonInfo.WorkerID]++;
                }

                if (!ana.ResultsPerWorkerPerTask[taskEntryID].ContainsKey(satyamResult.amazonInfo.WorkerID))
                {
                    ana.ResultsPerWorkerPerTask[taskEntryID].Add(satyamResult.amazonInfo.WorkerID, new List<string>());
                    ana.noDifferentDuplicateResultsPerWorkerPerTask[taskEntryID].Add(satyamResult.amazonInfo.WorkerID, 0);
                }
                else
                {
                    // this worker has done this task before
                    ana.noDuplicateResultsFromSameWorker++;
                }
                if (!ana.ResultsPerWorkerPerTask[taskEntryID][satyamResult.amazonInfo.WorkerID].Contains(satyamResult.TaskResult))
                {
                    ana.noDifferentDuplicateResultsPerWorkerPerTask[taskEntryID][satyamResult.amazonInfo.WorkerID]++;
                    if (ana.noDifferentDuplicateResultsPerWorkerPerTask[taskEntryID][satyamResult.amazonInfo.WorkerID] > 1)
                    {
                        ana.noDecisionChangeAmongDuplicateResultsOfSameWorkerSameTask++;
                    }
                }
                ana.ResultsPerWorkerPerTask[taskEntryID][satyamResult.amazonInfo.WorkerID].Add(satyamResult.TaskResult);

                ///////////////// imagenet only
                //List<SingleObjectLabelingResult> CurrentResultsOfThisTask = new List<SingleObjectLabelingResult>();
                //foreach(string worker in ana.ResultsPerWorkerPerTask[taskEntryID].Keys)
                //{
                //    foreach(string workerResult in ana.ResultsPerWorkerPerTask[taskEntryID][worker])
                //    {
                //        CurrentResultsOfThisTask.Add(JSonUtils.ConvertJSonToObject<SingleObjectLabelingResult>(workerResult));
                //    }
                //}
                //if (SingleObjectLabelingAggregator.getAggregatedResult(CurrentResultsOfThisTask) != null)
                //{
                //    // aggregation happened
                //    if (!ana.timeTakenTillAggregationPerTask.ContainsKey(taskEntryID))
                //    {
                //        // for the first time aggregated
                //        ana.timeTakenTillAggregationPerTask.Add(taskEntryID, (entry.SubmitTime - job.JobSubmitTime).TotalSeconds);
                //    }
                //}
                ///////////////////////
            }

            ana.resultArrivalTimes.Sort();

            ana.noTasks = ana.noResultsPerTask.Count;

            ana.timeTakenPerResult.Sort();
            ana.medianTimeTakenPerResultInSeconds = ana.timeTakenPerResult[ana.timeTakenPerResult.Count / 2];

            if (ana.acceptedTimeTakenPerResult.Count != 0)
            {
                ana.acceptedTimeTakenPerResult.Sort();
                ana.medianAcceptedTimeTakenPerResultInSeconds = ana.acceptedTimeTakenPerResult[ana.acceptedTimeTakenPerResult.Count / 2];
            }

            

            ana.totalTimeTakenForJob = ana.lastTaskEndTime - ana.jobSubmitTime;

            ///////////////////////////////////// Per Task Analysis //////////////////////////////////////
            ana.resultsPerTaskHistogram = new SortedDictionary<int, int>();
            ana.resultsAcceptedPerTaskHistogram = new SortedDictionary<int, int>();
            ana.resultsPaidPerTaskHistogram = new SortedDictionary<int, int>();
            ana.moneyPaidPerTaskHistogram = new SortedDictionary<int, int>(); //cents
            ana.ResultsPerWorkerPerTaskHistogram = new SortedDictionary<int, int>();
            int noAggregatedTasks = 0;
            List<int> taskIDs = ana.noResultsPerTask.Keys.ToList();
            foreach (int taskID in taskIDs)
            {
                if(!ana.resultsPerTaskHistogram.ContainsKey(ana.noResultsPerTask[taskID]))
                {
                    ana.resultsPerTaskHistogram.Add(ana.noResultsPerTask[taskID],0);
                }
                ana.resultsPerTaskHistogram[ana.noResultsPerTask[taskID]]++;

                if (!ana.resultsAcceptedPerTaskHistogram.ContainsKey(ana.noAcceptedResultsPerTask[taskID]))
                {
                    ana.resultsAcceptedPerTaskHistogram.Add(ana.noAcceptedResultsPerTask[taskID], 0);
                }
                ana.resultsAcceptedPerTaskHistogram[ana.noAcceptedResultsPerTask[taskID]]++;

                if (!ana.resultsPaidPerTaskHistogram.ContainsKey(ana.noPaidResultsPerTask[taskID]))
                {
                    ana.resultsPaidPerTaskHistogram.Add(ana.noPaidResultsPerTask[taskID], 0);
                }
                ana.resultsPaidPerTaskHistogram[ana.noPaidResultsPerTask[taskID]]++;

                int moneyPaid = (int)Math.Floor(ana.moneyPaidPerTask[taskID]*100);
                if (!ana.moneyPaidPerTaskHistogram.ContainsKey(moneyPaid))
                {
                    ana.moneyPaidPerTaskHistogram.Add(moneyPaid, 0);
                }
                ana.moneyPaidPerTaskHistogram[moneyPaid]++;

                if (ana.ResultsPerWorkerPerTask[taskID].Count != ana.noResultsPerTask[taskID])
                {
                    //has multiple results from same turker
                    ana.noTasksWithDuplicateResultsFromSameWorker++;
                }          
                
                if (ana.noPaidResultsPerTask[taskID] != 0)
                {
                    noAggregatedTasks++;
                }
            }

            ana.avgNoPaidResultsPerTask = (double)ana.totalpaidResults / (double)noAggregatedTasks;

            //////////// duplicate analysis
            //Console.WriteLine("DuplicateResultsHistogram");
            //foreach (int no in ana.ResultsPerWorkerPerTaskHistogram.Keys)
            //{
            //    Console.WriteLine("{0}, {1}", no, ana.ResultsPerWorkerPerTaskHistogram[no]);
            //}

            //Console.WriteLine("{0} images (in total {1}({2})) has duplicate results from same turker, \n" +
            //    "\t{3} images have >=1 workers making more than majority number of results.\n" +
            //    "\t{11} images aggregation is changed ({12} of which incorrectly) by duplicate results from same turker.\n" +
            //    "\t{4} images(In total {5} times) with duplicate results include mixed(>= 2) decisions from same worker.\n" +
            //    //"\t\t{6} images has correct decision among duplicate decisions.\n" +
            //    //"\t\t{7} images({8} times) a worker has switched from incorrect to correct choice\n" +
            //    //"\t\t\t{9} images({10} times) of which maintained the correct choice till the last time of their job on that image.",
            //    ana.noTasksWithDuplicateResultsFromSameWorker, ana.noDuplicateResultsFromSameWorker, 0,
            //    ana.noTaskWithDuplicateResultsAsMajority,
            //    ana.noTasksWithMixedResultsFromSameWorker,ana.noDecisionChangeAmongDuplicateResultsOfSameWorkerSameTask,
            //    ana.noTasksWithCorrectDuplicateResultsOfSameWorkerSameTask
            //    //ana.noTasksWithWorkerSwitchedToCorrect, ana.noWorkerSwitchedToCorrect,
            //    //ana.noTasksWithWorkerSwitchedToCorrectAndMaintained, ana.noWorkerSwitchedToCorrectAndMaintained,
            //    //ana.noTaskWhoseDuplicateResultsChangedAggregation, ana.noTaskWhoseDuplicateResultsChangedAggregationIncorrectly
            //);
            //////////////////////////////


            string outputDir = DirectoryConstants.defaultTempDirectory + jobGUIDs[0] + "\\ResultStatistics\\";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            //Console.WriteLine("timePerResult");
            TextWriter tw = new StreamWriter(outputDir + "timePerResult.txt");
            foreach (double time in ana.timeTakenPerResult)
            {
                tw.WriteLine(time);
            }
            tw.Close();

            tw = new StreamWriter(outputDir + "AcceptedTimePerSequenceNumber.txt");
            foreach (int seq in ana.AcceptedTimePerSequenceNumber.Keys)
            {
                foreach(double time in ana.AcceptedTimePerSequenceNumber[seq])
                {
                    tw.WriteLine(time);
                }
                tw.WriteLine("");
            }
            tw.Close();

            tw = new StreamWriter(outputDir + "AcceptedTimePerTask.txt");
            foreach (int taskid in ana.AcceptedTimePerTask.Keys)
            {
                foreach (double time in ana.AcceptedTimePerTask[taskid])
                {
                    tw.WriteLine(time);
                }
                tw.WriteLine("");
            }
            tw.Close();

            tw = new StreamWriter(outputDir + "TimePerSequenceNumber.txt");
            foreach (int seq in ana.TimePerSequenceNumber.Keys)
            {
                foreach (double time in ana.TimePerSequenceNumber[seq])
                {
                    tw.WriteLine(time);
                }
                tw.WriteLine("");
            }
            tw.Close();

            tw = new StreamWriter(outputDir + "TimePerTask.txt");
            foreach (int taskid in ana.TimePerTask.Keys)
            {
                foreach (double time in ana.TimePerTask[taskid])
                {
                    tw.WriteLine(time);
                }
                tw.WriteLine("");
            }
            tw.Close();


            //Console.WriteLine("resultArrivalTime");
            TextWriter tw0 = new StreamWriter(outputDir + "resultArrivalTime.txt");
            foreach (double time in ana.resultArrivalTimes)
            {
                tw0.WriteLine(time);
            }
            tw0.Close();

            //Console.WriteLine("HITFinishTime");
            TextWriter tw2 = new StreamWriter(outputDir + "HITFinishTime.txt");
            foreach (double time in ana.finalHITEndTime.Values)
            {
                tw2.WriteLine(time);
            }
            tw2.Close();

            //Console.WriteLine("SequenceNumberOfResultsPerTask");
            TextWriter tw3 = new StreamWriter(outputDir + "SequenceNumberOfResultsPerTask.txt");
            foreach (int seq in ana.SequenceNumberOfResultPerTask)
            {
                tw3.WriteLine(seq);
            }
            tw3.Close();
            //Console.WriteLine("SequenceNumberOfResultsPerWorker");
            TextWriter tw4 = new StreamWriter(outputDir + "SequenceNumberOfResultsPerWorker.txt");
            foreach (int seq in ana.SequenceNumberOfResultPerWorker)
            {
                tw4.WriteLine(seq);
            }
            tw4.Close();

            //Console.WriteLine("MoneyPaidPerResult");
            TextWriter tw5 = new StreamWriter(outputDir + "MoneyPaidPerResult.txt");
            foreach (int seq in ana.PricePerResult.Keys)
            {
                tw5.WriteLine(ana.PricePerResult[seq]);
            }
            tw5.Close();

            TextWriter tw6 = new StreamWriter(outputDir + "acceptedTimePerResult.txt");
            foreach (double time in ana.acceptedTimeTakenPerResult)
            {
                tw6.WriteLine(time);
            }
            tw6.Close();
            ///////////////////// aggregation time histogram, out of scope
            //Console.WriteLine("resultArrivalTime",
            //    TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE, TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE);


            //string filepath = String.Format(@"C:\research\MSRDVA\SatyamResearch\aggregationTimePerTask_MinNoResults_{0}_MaxNoResults_{1}.txt",
            //    TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MIN_RESULTS_TO_AGGREGATE, TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAX_RESULTS_TO_AGGREGATE);
            //TextWriter tw1 = new StreamWriter(filepath);
            //foreach (double time in ana.timeTakenTillAggregationPerTask.Values)
            //{
            //    tw1.WriteLine(time);
            //}
            //tw1.Close();

            Console.WriteLine("Median Time For Accepted Task: {0}", ana.medianAcceptedTimeTakenPerResultInSeconds);
            Console.WriteLine("Median Time For All Task: {0}", ana.medianTimeTakenPerResultInSeconds);
            Console.WriteLine("Total Tasks: {0}", ana.noTasks);
            Console.WriteLine("Total Accepted Results: {0}", ana.totalAcceptedResults);
            Console.WriteLine("Total Results: {0}", ana.totalResults);
            Console.WriteLine("Avg Paid Results Per Task: {0}", ana.avgNoPaidResultsPerTask);

            return ana;

        }


        //public static void AnalyzeApprovalRateByGUID(List<string> guids)
        //{
        //    Console.WriteLine("Analyzing Results Statistics of {0}", guids[0]);
        //    SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
        //    List<SatyamResultsTableEntry> entries = new List<SatyamResultsTableEntry>();
        //    foreach (string guid in guids)
        //    {
        //        entries.AddRange(resultsDB.getEntriesByGUIDOrderByID(guid));
        //    }

        //    resultsDB.close();
        //}





        public static void AggregationAnalysis(List<SatyamAggregatedResultsTableEntry> aggEntries, 
            //List<SatyamResultsTableEntry> ResultEntries,
            SortedDictionary<DateTime, List<SatyamResultsTableEntry>> entriesBySubmitTime,
            Dictionary<int, int> noResultsNeededForAggregation,
            Dictionary<int, int> noResultsNeededForAggregation_new,
            string guid, string configString,
            int minChancesGiven = 3,
            double anotherChanceProbablity = 0,
            double approvalRatioThreshold = 0.5,
            bool usePreApprovalResult= false)
        {
            if (noResultsNeededForAggregation == null)
            {
                noResultsNeededForAggregation = new Dictionary<int, int>();
            }

            if (noResultsNeededForAggregation_new == null)
            {
                noResultsNeededForAggregation_new = new Dictionary<int, int>();
            }

            string approvalString = configString +
                "_Thresh_" + approvalRatioThreshold + 
                "_Explore_" + anotherChanceProbablity;

            Dictionary<int, SatyamAggregatedResultsTableEntry> aggEntriesByTaskID = new Dictionary<int, SatyamAggregatedResultsTableEntry>();
            Dictionary<int, List<SatyamResultsTableEntry>> ResultEntriesByTaskID = new Dictionary<int, List<SatyamResultsTableEntry>>();

            //Dictionary<DateTime, double> TotalApprovalRatioByTime = new Dictionary<DateTime, double>();
            List<string> TotalApprovalRatioByTime = new List<string>();
            List<string> TotalApprovalRatioByResult = new List<string>();
            List<string> FilteredTotalApprovalRatioByTime = new List<string>();
            List<string> FilteredTotalApprovalRatioByResult = new List<string>();

            List<SatyamResultsTableEntry> approvedEntries = new List<SatyamResultsTableEntry>();
            List<SatyamResultsTableEntry> rejectedEntries = new List<SatyamResultsTableEntry>();
            List<SatyamResultsTableEntry> InconclusiveEntries = new List<SatyamResultsTableEntry>();
            List<SatyamResultsTableEntry> approvedEntriesActuallyPaid = new List<SatyamResultsTableEntry>();
            

            Dictionary<string, int> noTasksApprovedPerWorker = new Dictionary<string, int>();
            Dictionary<string, int> noTasksDonePerWorker = new Dictionary<string, int>();
            Dictionary<string, double> ApprovalRatioPerWorker = new Dictionary<string, double>();
            Dictionary<string, bool> FirstTimersWorkersRecord = new Dictionary<string, bool>();
            

            List<SatyamResultsTableEntry> FilteredApprovedEntries = new List<SatyamResultsTableEntry>();
            List<SatyamResultsTableEntry> filteredRejectedEntries = new List<SatyamResultsTableEntry>();


            Dictionary<int, double> AggregationTimePerTask = new Dictionary<int, double>();

            foreach (SatyamAggregatedResultsTableEntry aggEntry in aggEntries)
            {
                if (!aggEntriesByTaskID.ContainsKey(aggEntry.SatyamTaskTableEntryID))
                {
                    aggEntriesByTaskID.Add(aggEntry.SatyamTaskTableEntryID, aggEntry);
                }
                else
                {
                    // duplicate agg entry for a task
                }
            }

            //Dictionary<int, int> noResultsNeededForAggregation = getNoResultsNeededForAggregationFromLog(approvalString, guid);
            foreach (int k in noResultsNeededForAggregation_new.Keys)
            {
                noResultsNeededForAggregation.Add(k, noResultsNeededForAggregation_new[k]);
            }
            int count = 0;
            foreach (DateTime t in entriesBySubmitTime.Keys)
            {
                //Console.WriteLine("Processing Results of time: {0}", t);
                Console.WriteLine("Judging result: {0}/{1}", count, entriesBySubmitTime.Count);
                count++;

                List<SatyamResultsTableEntry> ResultEntries = entriesBySubmitTime[t];
                foreach (SatyamResultsTableEntry res in ResultEntries)
                {
                    

                    SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(res.ResultString);
                    


                    int taskID = res.SatyamTaskTableEntryID;

                    if (!ResultEntriesByTaskID.ContainsKey(taskID))
                    {
                        ResultEntriesByTaskID.Add(taskID, new List<SatyamResultsTableEntry>());
                    }
                    ResultEntriesByTaskID[taskID].Add(res);

                    if (!aggEntriesByTaskID.ContainsKey(taskID))
                    {
                        //foreach (SatyamResultsTableEntry res in ResultEntriesByTaskID[taskID])
                        //{
                        //InconclusiveEntries.Add(res);
                        //}
                        InconclusiveEntries.Add(res);
                        continue;
                    }
                    SatyamAggregatedResultsTableEntry agg = aggEntriesByTaskID[taskID];
                    //foreach (SatyamResultsTableEntry res in ResultEntriesByTaskID[taskID])
                    //{
                    //SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(res.ResultString);
                    string workerId = satyamResult.amazonInfo.WorkerID;

                    // the worker pre-filtering step
                    bool filtered = false;
                    if (noTasksDonePerWorker.ContainsKey(workerId) && noTasksDonePerWorker[workerId] >= minChancesGiven)
                    {
                        double approvalRatio = (double)noTasksApprovedPerWorker[workerId] / (double)noTasksDonePerWorker[workerId];
                        ApprovalRatioPerWorker[workerId] = approvalRatio;
                        if (approvalRatio < approvalRatioThreshold)
                        {
                            // roll a dice to decide whether to skip this worker
                            int rnd = new Random().Next(100);
                            if (rnd > 100 * anotherChanceProbablity)
                            {
                                filtered = true;
                            }
                            else
                            {
                                // another chance granted
                            }
                        }
                    }

                    if (!noTasksDonePerWorker.ContainsKey(workerId))
                    {
                        noTasksDonePerWorker.Add(workerId, 0);
                        noTasksApprovedPerWorker.Add(workerId, 0);
                        ApprovalRatioPerWorker.Add(workerId, -1);
                    }
                    if (!filtered)
                    {
                        noTasksDonePerWorker[workerId]++;
                    }

                    bool acceptable = false;
                    if (usePreApprovalResult)
                    {
                        if (ResultStatus.acceptedStatusList.Contains(res.Status)) acceptable = true;
                    }
                    else
                    {
                        acceptable = AcceptanceCriterionChecker.IsAcceptable(agg, res);
                    }

                    if (acceptable)
                    {
                        
                        approvedEntries.Add(res);
                        if (!FirstTimersWorkersRecord.ContainsKey(workerId))
                        {
                            FirstTimersWorkersRecord.Add(workerId, true);
                        }

                        if (!filtered)
                        {
                            noTasksApprovedPerWorker[workerId]++;
                            FilteredApprovedEntries.Add(res);
                        }

                        
                        if (noResultsNeededForAggregation != null && noResultsNeededForAggregation.ContainsKey(taskID) 
                            && ResultEntriesByTaskID[taskID].Count <= noResultsNeededForAggregation[taskID])
                        {
                            approvedEntriesActuallyPaid.Add(res);
                        }
                        
                    }
                    else
                    {
                        rejectedEntries.Add(res);
                        if (!FirstTimersWorkersRecord.ContainsKey(workerId))
                        {
                            FirstTimersWorkersRecord.Add(workerId, false);
                        }
                        if (!filtered)
                        {
                            filteredRejectedEntries.Add(res);
                        }
                    }
                    //}

                    /// timetaken till aggregation
                    if (noResultsNeededForAggregation != null && noResultsNeededForAggregation.ContainsKey(taskID)
                            && ResultEntriesByTaskID[taskID].Count == noResultsNeededForAggregation[taskID])
                    {
                        // aggregation happen
                        SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                        SatyamJob job = task.jobEntry;
                        if (!AggregationTimePerTask.ContainsKey(taskID))
                        {
                            AggregationTimePerTask.Add(taskID, (res.SubmitTime - job.JobSubmitTime).TotalSeconds);
                        }
                    }




                    string ApprovalString = approvedEntries.Count + " " + (approvedEntries.Count + rejectedEntries.Count) + " "
                        + (double)approvedEntries.Count / (double)(approvedEntries.Count + rejectedEntries.Count);
                    TotalApprovalRatioByResult.Add(ApprovalString);

                    string filteredApprovalString = FilteredApprovedEntries.Count + " " + (FilteredApprovedEntries.Count + filteredRejectedEntries.Count) + " "
                        + (double)FilteredApprovedEntries.Count / (double)(FilteredApprovedEntries.Count + filteredRejectedEntries.Count);
                    FilteredTotalApprovalRatioByResult.Add(filteredApprovalString);
                }

                string approvalStringByTime = t.ToString() + " " + approvedEntries.Count + " " + (approvedEntries.Count + rejectedEntries.Count) + " "
                        + (double)approvedEntries.Count / (double)(approvedEntries.Count + rejectedEntries.Count);
                TotalApprovalRatioByTime.Add(approvalStringByTime);

                string filteredApprovalStringByTime = t.ToString() + " " + FilteredApprovedEntries.Count + " " + (FilteredApprovedEntries.Count + filteredRejectedEntries.Count) + " "
                        + (double)FilteredApprovedEntries.Count / (double)(FilteredApprovedEntries.Count + filteredRejectedEntries.Count);
                FilteredTotalApprovalRatioByTime.Add(filteredApprovalStringByTime);

            }
            // count first timer succ rate
            int FirstTimeSucc = 0;
            foreach (KeyValuePair<string, bool> wid in FirstTimersWorkersRecord)
            {
                if (wid.Value) { FirstTimeSucc++; }
            }

            double FirstTimeSuccRate = (double)FirstTimeSucc / FirstTimersWorkersRecord.Count;

            Console.WriteLine("Approval #, ratio: {0},{1}", approvedEntries.Count, (double)approvedEntries.Count / (double)(approvedEntries.Count+rejectedEntries.Count));
            Console.WriteLine("Filtered Approval #, ratio: {0},{1}", FilteredApprovedEntries.Count, (double)FilteredApprovedEntries.Count / (double)(FilteredApprovedEntries.Count + filteredRejectedEntries.Count));

            int approvePrevRejected = 0;

            List<string> ApprovedAssignmentIDs = new List<string>();
            foreach(SatyamResultsTableEntry approved in approvedEntries)
            {
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(approved.ResultString);
                ApprovedAssignmentIDs.Add(satyamResult.amazonInfo.AssignmentID);
                if (approved.Status == ResultStatus.accepted_NotPaid || approved.Status == ResultStatus.rejected_NotPaid || approved.Status == ResultStatus.rejected)
                {
                    approvePrevRejected++;
                }
            }

            Console.WriteLine("Approving Previously Rejected: {0}, {1}", approvePrevRejected, (double)approvePrevRejected / (double)approvedEntries.Count);


            string outputFileDir = DirectoryConstants.defaultTempDirectory + guid + "\\ApprovalAnalysis\\";
            
            if (guid!=null && approvalString != null)
            {
                if (!Directory.Exists(outputFileDir))
                {
                    Directory.CreateDirectory(outputFileDir);
                }
            }

            string summaryFile = outputFileDir + "resultSummary.txt";
            string summary = String.Format("{0} {1} {2} {3} {4} {5} {6} {7}\n",
                approvalString,
                approvedEntries.Count, (double)approvedEntries.Count / (double)(approvedEntries.Count + rejectedEntries.Count),
                approvedEntriesActuallyPaid.Count,
                FilteredApprovedEntries.Count, (double)FilteredApprovedEntries.Count / (double)(FilteredApprovedEntries.Count + filteredRejectedEntries.Count),
                approvePrevRejected, (double)approvePrevRejected / (double)approvedEntries.Count);
            File.AppendAllText(summaryFile, summary);

            string outputFile = outputFileDir + approvalString + ".txt";
            File.WriteAllLines(outputFile, ApprovedAssignmentIDs.ToArray());

            //string outputFilePaid = outputFileDir + approvalString + "_ActuallyPaid.txt";
            //File.WriteAllText(outputFilePaid, approvedEntriesActuallyPaid.Count.ToString());

            List<string> approvalRates = new List<string>();
            foreach(string worker in ApprovalRatioPerWorker.Keys)
            {
                approvalRates.Add(ApprovalRatioPerWorker[worker].ToString());
            }
            string approvalDistributionFile = outputFileDir + approvalString + "_dist.txt";
            File.WriteAllLines(approvalDistributionFile, approvalRates.ToArray());

            string approvalByTimeFile = outputFileDir + approvalString + "_approvalRateByTime.txt";
            string approvalByResultFile = outputFileDir + approvalString + "_approvalRateByResults.txt";
            File.WriteAllLines(approvalByTimeFile, TotalApprovalRatioByTime.ToArray());
            File.WriteAllLines(approvalByResultFile, TotalApprovalRatioByResult.ToArray());

            approvalByTimeFile = outputFileDir + approvalString + "_approvalRateByTime_filtered.txt";
            approvalByResultFile = outputFileDir + approvalString + "_approvalRateByResults_filtered.txt";
            File.WriteAllLines(approvalByTimeFile, FilteredTotalApprovalRatioByTime.ToArray());
            File.WriteAllLines(approvalByResultFile, FilteredTotalApprovalRatioByResult.ToArray());

            List<string> aggregationTime = new List<string>();
            foreach (int task in AggregationTimePerTask.Keys)
            {
                aggregationTime.Add(task.ToString() + " "+ AggregationTimePerTask[task].ToString());
            }
            string aggregationTimeFile = outputFileDir + approvalString + "_aggregationTime.txt";
            File.WriteAllLines(aggregationTimeFile, aggregationTime.ToArray());
        }

        public static double getPaymentRateByGUID(string guid, out int totalResults, out int totalAggregated)
        {
            SatyamResultsTableAccess resDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> results = resDB.getEntriesByGUID(guid);
            int paid = 0;
            int notPaid = 0;
            totalResults = results.Count;
            totalAggregated = 0;

            foreach (SatyamResultsTableEntry entry in results)
            {
                if (ResultStatus.paidStatusList.Contains(entry.Status))
                {
                    paid++;
                    totalAggregated++;
                }
                if (ResultStatus.notPaidStatusList.Contains(entry.Status))
                {
                    notPaid++;
                    totalAggregated++;
                }
            }
            double ratio = (double)paid / (double)(paid + notPaid);
            return ratio;
        }



        public static void RecordAggregationLog(Dictionary<int, int> noResultsNeededForAggregationPerTask, string configString, string guid)
        {
            string outputDir = DirectoryConstants.defaultTempDirectory + guid + "\\logs\\";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            string logFile = outputDir + configString + "_log.txt";

            List<string> logs = new List<string>();
            foreach(int k in noResultsNeededForAggregationPerTask.Keys)
            {
                logs.Add(k.ToString() + " " + noResultsNeededForAggregationPerTask[k].ToString());
            }
            File.AppendAllLines(logFile, logs.ToArray());
        }

        public static Dictionary<int, int> getNoResultsNeededForAggregationFromLog(string configString, string guid)
        {
            Dictionary<int, int> noResultsNeededForAggregationPerTask = new Dictionary<int, int>();
            string outputDir = DirectoryConstants.defaultTempDirectory + guid + "\\logs\\";
            string logFile = outputDir + configString + "_log.txt";
            if (!File.Exists(logFile))
            {
                return null;
            }
            List<string> content = File.ReadAllLines(logFile).ToList();
            foreach (string c in content)
            {
                string[] fields = c.Split(' ');
                int task = Convert.ToInt32(fields[0]);
                int no = Convert.ToInt32(fields[1]);
                noResultsNeededForAggregationPerTask.Add(task, no);
                //if (taskID == task)
                //{
                //    return no;
                //}
            }
            //return -1;
            return noResultsNeededForAggregationPerTask;
        }
    }
}
