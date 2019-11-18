using AzureBlobStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testing
{
    public class TestAzureBlobStorage
    {
        public static void RunTest()
        {
            testGetURLListOfSubDirectoryByURL();


        }

        public static void testNonFlatListing()
        {
            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            string satyamContainerName = "kittitracking";
            string satyamDirectoryName = "all/Tracking_0000_153frame";
            List<string> satyamURIList = satyamStorage.getURLList(satyamContainerName, satyamDirectoryName, false);
            foreach (string uri in satyamURIList)
            {
                Console.WriteLine(uri);
            }
        }

        public static void testGetURLListOfSubDirectoryByURL()
        {
            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            string url = "https://satyamresearchjobstorage.blob.core.windows.net/multiobjecttracking/df2cedef-e707-4c32-b19e-c2599f9aaea3/Video_Bellevue_BellevueWay_NE8th_2016-10-13-8-2-30-000_2016-10-13-8-2-40-000_startingFrame_0/";
            List<string> satyamURIList = satyamStorage.getURLListOfSubDirectoryByURL(url);
            foreach (string uri in satyamURIList)
            {
                Console.WriteLine(uri);
            }
        }
    }
}
