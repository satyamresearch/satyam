using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Constants
{
    //public static class AWSExceptionStatus
    //{
    //    public const string NotExist = "Amazon.WebServices.MechanicalTurk.Exceptions.ObjectDoesNotExistException";
    //    public const string Reviewable = "Amazon.WebServices.MechanicalTurk.Exceptions.InvalidStateException";
    //}

    public static class HitStatus
    {
        public const string pending = "pending";
        public const string taken = "taken";
        public const string submitted = "submitted";
        public const string accepted = "accepted";
        public const string rejected = "rejected";
        public const string expired = "expired";
    }

    public static class JobStatus
    {
        public const string submitted = "Submitted";
        public const string preprocessed = "Preprocessed";
        public const string ready = "Ready";
        public const string launched = "Launched";
        public const string completed = "Completed";
    }

    public static class ResultStatus
    {
        public const string inconclusive = "inconclusive";
        public const string accepted = "accepted";
        public const string rejected = "rejected";
        public const string accepted_Paid = "accepted_and_paid";
        public const string accepted_NotPaid = "accepted_and_notpaid";
        public const string rejected_Paid = "rejected_and_paid";
        public const string rejected_NotPaid = "rejected_and_notpaid";
        public const string outdated = "outdated";
        public static List<string> acceptedStatusList = new List<string>()
        {
            accepted,accepted_Paid,accepted_NotPaid
        };
        public static List<string> rejectedStatusList = new List<string>()
        {
            rejected,rejected_Paid,rejected_NotPaid
        };
        public static List<string> paidStatusList = new List<string>()
        {
            accepted_Paid,rejected_Paid
        };
        public static List<string> notPaidStatusList = new List<string>()
        {
            rejected_NotPaid,accepted_NotPaid
        };
        public static List<string> HandledStatusList = new List<string>()
        {
            accepted_NotPaid, accepted_Paid, rejected_NotPaid, rejected_Paid
        };


    }

    
}
