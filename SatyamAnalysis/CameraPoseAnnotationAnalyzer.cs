using Constants;
using HelperClasses;
using JobTemplateClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SatyamAnalysis
{
    public class CameraPoseAnnotationAnalyzer
    {

        

        public static Image DrawResultStringOnImage(string resultString, Image originalImage)
        {
            CameraPoseAnnotationResult res = JSonUtils.ConvertJSonToObject<CameraPoseAnnotationResult>(resultString);

            List<LineSegment> xparallel = new List<LineSegment>();
            List<LineSegment> yparallel = new List<LineSegment>();
            List<LineSegment> zparallel = new List<LineSegment>();

            int[] origin = new int[2];
            int count = 0;

            foreach (int[] vertex in res.objects.caxis.vertices)
            {
                if (count == 0)
                {
                    origin[0] = vertex[0];
                    origin[1] = vertex[1];
                }
                LineSegment line = new LineSegment(origin[0], origin[1], vertex[0], vertex[1]);

                if (count == 1)
                    xparallel.Add(line);
                if (count == 2)
                    yparallel.Add(line);
                if (count == 3)
                    zparallel.Add(line);
                count = count + 1;

            }

            count = 0;
            foreach (int[] vertex in res.objects.xvppoints.vertices)
            {
                if (count % 2 == 0)
                {
                    origin[0] = vertex[0];
                    origin[1] = vertex[1];
                }
                else
                {
                    LineSegment line = new LineSegment(origin[0], origin[1], vertex[0], vertex[1]);
                    xparallel.Add(line);
                }
                count = count + 1;

            }
            count = 0;
            foreach (int[] vertex in res.objects.yvppoints.vertices)
            {
                if (count % 2 == 0)
                {
                    origin[0] = vertex[0];
                    origin[1] = vertex[1];
                }
                else
                {
                    LineSegment line = new LineSegment(origin[0], origin[1], vertex[0], vertex[1]);
                    yparallel.Add(line);
                }
                count = count + 1;

            }
            count = 0;
            foreach (int[] vertex in res.objects.zvppoints.vertices)
            {
                if (count % 2 == 0)
                {
                    origin[0] = vertex[0];
                    origin[1] = vertex[1];
                }
                else
                {
                    LineSegment line = new LineSegment(origin[0], origin[1], vertex[0], vertex[1]);
                    zparallel.Add(line);
                }
                count = count + 1;

            }
            Image imageWithX = DrawingBoxesAndLinesOnImages.addLinesToImage(originalImage, xparallel, Color.Red, true);
            Image imageWithY = DrawingBoxesAndLinesOnImages.addLinesToImage(imageWithX, yparallel, Color.Blue, true);
            Image imageWithZ = DrawingBoxesAndLinesOnImages.addLinesToImage(imageWithY, zparallel, Color.Yellow, true);

            return imageWithZ;
        }

        public static void SaveResultImagesLocally(List<SatyamResultsTableEntry> entries, string directoryName)
        {
            
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            directoryName = directoryName + "\\Raw\\";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                SatyamResultsTableEntry entry = entries[i];
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                SatyamJob job = task.jobEntry;

                string result = satyamResult.TaskResult;
                Image originalImage = ImageUtilities.getImageFromURI(task.SatyamURI);

                Image ResultImage = DrawResultStringOnImage(result, originalImage);

                string ofilename = URIUtilities.filenameFromURI(task.SatyamURI);
                string[] fields = ofilename.Split('.');
                string fileName = "";
                for (int j=0;j<fields.Count();j++)
                {
                    fileName += fields[j];
                    if (j == fields.Count() - 1) break;
                    fileName += ".";
                }


                //string fileName = ofilename;
                fileName = fileName + "-Result";
                
                fileName = fileName + "-" + entry.ID;

                Console.WriteLine("Saving " + fileName);

                ImageUtilities.saveImage(ResultImage, directoryName, fileName);
                string resultFile = directoryName + fileName + ".txt";
                StreamWriter f = new System.IO.StreamWriter(resultFile);
                f.WriteLine(result);
                f.Close();
            }
        }

        public static void SaveResultImagesLocally(string jobGUID)
        {
            string directoryName = DirectoryConstants.defaultTempDirectory + "\\" + jobGUID + "\\";
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            SaveResultImagesLocally(entries, directoryName);
            resultsDB.close();
        }
    }
}
