using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLTableManagement;

namespace Testing
{
    public static class TestResultAggregation
    {
        public static void RunTest()
        {
            SatyamResultsTableManagement.AggregateResults();
            SatyamResultsTableManagement.AcceptRejectResults();
        }
    }
}
