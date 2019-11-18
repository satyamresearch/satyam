using SatyamAnalysis;
using SatyamTaskResultClasses;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFServingClient;
using Utilities;

namespace Testing
{
    public class TestTFServing
    {

        public static void TestDetectionRequest()
        {
            string image_url = "https://satyamresearchjobstorage.blob.core.windows.net/longdurationblob/SeattleLive-5-Westlake-NS/SeattleLive-5-Westlake-NS_2017-09-28-15-20-19-000_2017-09-28-15-20-22-000/SeattleLive-5-Westlake-NS-000001.jpg";
            //TFServingClient.ImageDetectionRequest(image_url);
            List<string> categories = new List<string>();
            categories.Add("Car");
            categories.Add("Ped");
            MultiObjectLocalizationAndLabelingResult res = TensorflowServingClient.GetImageDetectionResult(image_url, categories);
            Image im = MultiObjectLabelingAndLocalizationAnalysis.DrawImageDetectionResult(res, image_url, categories);
            ImageUtilities.saveImage(im, Constants.DirectoryConstants.DefaultResultDirectory, "test");
        }
    }
}
