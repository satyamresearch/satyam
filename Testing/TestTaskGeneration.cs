using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLTableManagement;


namespace Testing
{
    public static class TestTaskGeneration
    {
        public static void RunTest()
        {
            SatyamJobSubmissionsTableManagement.preprocessSubmittedData();
            SatyamJobSubmissionsTableManagement.processPreprocessedJobs();
        }
    }
}
