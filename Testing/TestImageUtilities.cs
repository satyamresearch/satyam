using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Testing
{
    public class TestImageUtilities
    {
        public static void TestGetImageFromLocalPath()
        {
            string fp = @"C:\research\dataset\PascalVOC\VOCtrainval_11-May-2012\VOCdevkit\VOC2012\SegmentationObject\2007_000121.PNG";
            int width, height;
            ImageUtilities.readLocalPNGRawData(fp,out width,out height);
        }

        public static void TestSaveImageFromLocalPath()
        {
            string fp = @"C:\research\dataset\PascalVOC\VOCtrainval_11-May-2012\VOCdevkit\VOC2012\SegmentationObject\2007_000121.PNG";
            int width, height;
            byte[] data = ImageUtilities.readLocalPNGRawData(fp, out width, out height);
            ImageUtilities.savePNGRawData(DirectoryConstants.defaultTempDirectory + "tmp.png", width, height, data);
        }

        public static void TestSaveImageFromURL()
        {
            string fp = "https://satyamresearchjobstorage.blob.core.windows.net/segmentation/91a14bb7-3c13-4d6f-9044-438beae9e559_aggregated/2007_000121_aggregated.PNG";
            int width, height;
            byte[] data = ImageUtilities.readPNGRawDataFromURL(fp, out width, out height);
            ImageUtilities.savePNGRawData(DirectoryConstants.defaultTempDirectory + "tmp.png", width, height, data);
        }
    }
}
