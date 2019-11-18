using Constants;
using SatyamResultValidation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.Drawing;

namespace Testing
{
    public class TestFFMPEGWrappers
    {
        static string ffmpeg = DirectoryConstants.ffmpeg;
        public static void testAVI2MP4()
        {
            string ffmpeg = DirectoryConstants.ffmpeg;
            string srcFolder = @"C:\research\dataset\UCF101\demoSet";
            string destFolder = @"C:\research\dataset\UCF101\demoSetMp4";
            FFMpegWrappers.ConvertLocalVideoFolderToMP4(ffmpeg, srcFolder, destFolder);
        }

        public static void stitchImage2Video()
        {
            string videoname = "Video_VIRAT_S_010005_02_000177_000203";
            string dir = @"C:\research\MSRDVA\temp\0efdc91e-62e2-4a70-b5fd-18720c170e03\AggregatedStitched\Video_VIRAT_S_010005_02_000177_000203\";
            FFMpegWrappers.generateVideoFromFolderofFrames(videoname, dir);
        }

        public static void TestExtractFrame()
        {
            
            string dataPath = @"C:\research\dataset\TomLaPorta\";
            string[] files = Directory.GetFiles(dataPath);
            foreach (string f in files)
            {
                string fn = URIUtilities.filenameFromDirectory(f);
                string[] ff = fn.Split('.');
                string fname = ff[0];
                string outputFolder = dataPath + fname + "\\";
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }
                FFMpegWrappers.ExtractFrames(ffmpeg, f,
                    outputFolder, fname, DateTime.MinValue, 0.1);

            }
            

        }
    }
}
