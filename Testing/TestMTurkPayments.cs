using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Constants;
using SQLTableManagement;
using SQLTables;

namespace Testing
{
    public static class TestMTurkPayments
    {
        public static void RunTest()
        {
            AmazonMTurkPayments.MakePayments();
        }        


        public static void TestManualAcceptReject()
        {
            //string path = @"C:\research\listturker.txt";
            string path = @"C:\research\topay.txt";
            string guid = "dff5415b-b278-43db-8e32-4f654ca4b3a1";

            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();

            List<SatyamResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);

            foreach (string line in File.ReadLines(path))
            {
                string[] cs = line.Split('-');
                string id = cs[cs.Length - 1].Split('.')[0];
                //Console.WriteLine(id);
                resultsDB.UpdateStatusByID(Convert.ToInt32(id), ResultStatus.accepted);
            }
        }
    }


    
}
