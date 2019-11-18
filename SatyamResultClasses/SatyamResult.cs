using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SatyamTaskResultClasses
{
    public class AmazonTaskResultInfo
    {
        public string AssignmentID;
        public string WorkerID;
        public string HITID;
        public double PricePerHIT;
    }
    public class SatyamResult
    {
        public string TaskParametersString;
        public DateTime TaskStartTime;
        public DateTime TaskEndTime;
        public int TaskTableEntryID;
        public string TaskResult;
        public AmazonTaskResultInfo amazonInfo;
        public int PrevResultID =0;// added for recursive workload, turker work ontop of existing result
    }
    

}
