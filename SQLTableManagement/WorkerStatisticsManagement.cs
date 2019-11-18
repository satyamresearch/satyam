using SQLTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLTableManagement
{
    public class WorkerStatisticsManagement
    {
        public static void UpdateWorkerStatistics(string workerID, string jobTemplateType, bool approved, DateTime lastUpdateTime)
        {
            if (workerID == "") return;
            WorkerStatisticsAccess wsa = new WorkerStatisticsAccess();
            wsa.UpdateSingleEntry(workerID, jobTemplateType, approved, lastUpdateTime);
            wsa.close();
        }

        public static bool IsGoodWorker(string workerID,
            string jobTemplateType,
            int minChancesGiven = 5,
            double anotherChanceProbablity = 0.2,
            double approvalRatioThreshold = 0.5)
        {
            if (workerID == "") return true;
            WorkerStatisticsAccess wsa = new WorkerStatisticsAccess();
            WorkerStatisticsTableEntry workerStatistics = wsa.getWorkerStatistics(workerID,jobTemplateType);
            wsa.close();

            //bool filtered = false;

            if (workerStatistics == null) return true;// first time worker.

            int noTasksDone = workerStatistics.TasksDone;
            if (noTasksDone >= minChancesGiven)
            {
                double approvalRatio = workerStatistics.SuccessFraction;
                
                if (approvalRatio < approvalRatioThreshold)
                {
                    // roll a dice to decide whether to skip this worker
                    int rnd = new Random().Next(100);
                    if (rnd > 100 * anotherChanceProbablity)
                    {
                        return false;
                    }
                    else
                    {
                        // another chance granted
                    }
                }
            }
            return true;
        }
    }
}
