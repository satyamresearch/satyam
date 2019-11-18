using HelperClasses;
using JobTemplateClasses;
using SatyamResultAggregators;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SatyamAnalysis
{
    public class ImageSegmentationResultAnalysis
    {
        


        public static Image DrawResultsOnImage(ImageSegmentationResult res, Image im)
        {
            if (res == null) return null;
            foreach (ImageSegmentationResultSingleEntry obj in res.objects)
            {
                foreach(GenericPolygon poly in obj.segment.polygons)
                {
                    string category = obj.Category;
                    List<LineSegment> lines = new List<LineSegment>();
                    for (int i = 0; i < poly.vertices.Count; i++)
                    {
                        int x1 = poly.vertices[i][0];
                        int y1 = poly.vertices[i][1];

                        int j = (i + 1) % poly.vertices.Count;
                        int x2 = poly.vertices[j][0];
                        int y2 = poly.vertices[j][1];
                        LineSegment line = new LineSegment(x1, y1, x2, y2);
                        lines.Add(line);
                    }
                    Color c = ColorSet.getColorByObjectType(category);

                    im = DrawingBoxesAndLinesOnImages.addLinesToImage(im, lines, c);
                }
            }
            return im;
        }


        public static void SaveResultImagesLocally(string jobGUID, string directoryName)
        {
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            resultsDB.close();
            SaveResultImagesLocally(entries, directoryName);
        }

        public static void SaveResultImagesLocally(List<SatyamResultsTableEntry> entries, string directoryName)
        {
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            directoryName = directoryName + "\\Raw";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            // sort by task id
            SortedDictionary<int, List<SatyamResultsTableEntry>> taskResults = new SortedDictionary<int, List<SatyamResultsTableEntry>>();
            for (int i = 0; i < entries.Count; i++)
            {
                if (!taskResults.ContainsKey(entries[i].SatyamTaskTableEntryID))
                {
                    taskResults.Add(entries[i].SatyamTaskTableEntryID, new List<SatyamResultsTableEntry>());
                }
                taskResults[entries[i].SatyamTaskTableEntryID].Add(entries[i]);
            }

            foreach (int taskID in taskResults.Keys)
            {
                for (int i = 0; i < taskResults[taskID].Count; i++)
                {
                    SatyamResultsTableEntry entry = taskResults[taskID][i];
                    SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                    SatyamJob job = task.jobEntry;
                    ImageSegmentationResult res = JSonUtils.ConvertJSonToObject<ImageSegmentationResult>(satyamResult.TaskResult);
                    if (res == null) continue;

                    string fileName = URIUtilities.filenameFromURINoExtension(task.SatyamURI);




                    fileName = fileName + "-Result";
                    if (satyamResult.amazonInfo.AssignmentID != "")
                    {
                        fileName = fileName + "-" + satyamResult.amazonInfo.AssignmentID;
                    }
                    fileName += "-" + entry.ID;


                    if (File.Exists(directoryName + "\\" + fileName + ".jpg"))
                    {
                        continue;
                    }

                    Console.WriteLine("Saving Turker Result {0}", fileName);

                    Image originalImage = ImageUtilities.getImageFromURI(task.SatyamURI);

                    Image ResultImage = DrawResultsOnImage(res, originalImage);

                    byte[] png = ImageSegmentationResult.PolygonResult2Bitmap(res);

                    if (ResultImage == null) continue;

                    ImageUtilities.saveImage(ResultImage, directoryName, fileName);

                    ImageUtilities.savePNGRawData(directoryName + "\\" + fileName + "_bitmap.jpg", originalImage.Width, originalImage.Height, png);
                }
            }
            
        }

        public static void SaveAggregatedResultImagesLocally(string jobGUID, string directoryName)
        {
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            resultsDB.close();
            SaveAggregatedResultImagesLocally(entries, directoryName);
        }


        public static void SaveAggregatedResultImagesLocally(List<SatyamAggregatedResultsTableEntry> entries, string directoryName)
        {

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            directoryName = directoryName + "\\Aggregated";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                SatyamAggregatedResultsTableEntry entry = entries[i];
                SatyamAggregatedResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParameters);
                SatyamJob job = task.jobEntry;
                ImageSegmentationAggregatedResult res = JSonUtils.ConvertJSonToObject<ImageSegmentationAggregatedResult>(satyamResult.AggregatedResultString);


                //string fileName = URIUtilities.filenameFromURINoExtension(task.SatyamURI);

                //Image originalImage = ImageUtilities.getImageFromURI(task.SatyamURI);

                //Image ResultImage = DrawResultsOnImage(res.boxesAndCategories, originalImage);

                //fileName = fileName + "-AggregatedResult";
                //ImageUtilities.saveImage(ResultImage, directoryName, fileName);

                WebClient wb = new WebClient();
                Image im = Image.FromStream(wb.OpenRead(res.metaData.PNG_URL));
                string fileName = URIUtilities.filenameFromURI(res.metaData.PNG_URL);
                //wb.DownloadFile(directoryName + "\\" + fileName, res.metaData.PNG_URL);
                im.Save(directoryName + "\\" + fileName);

            }
        }
    }
}
