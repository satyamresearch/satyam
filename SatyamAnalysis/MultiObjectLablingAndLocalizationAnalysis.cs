using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;


using SatyamTaskResultClasses;
using Utilities;
using JobTemplateClasses;
using SatyamTaskGenerators;
using SQLTables;
using HelperClasses;
using SatyamResultAggregators;


namespace SatyamAnalysis
{
    public class MultiObjectLabelingAndLocalizationAnalysisPerJob
    {
        public string jobTemplateType;
    }

    public static class MultiObjectLabelingAndLocalizationAnalysis
    {
        public static MultiObjectLabelingAndLocalizationAnalysisPerJob Analyse(List<SatyamResultsTableEntry> entries)
        {
            MultiObjectLabelingAndLocalizationAnalysisPerJob ana = new MultiObjectLabelingAndLocalizationAnalysisPerJob();

            ana.jobTemplateType = entries[0].JobTemplateType;
            

            for (int i = 0; i < entries.Count; i++)
            {
                SatyamResultsTableEntry entry = entries[i];
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                SatyamJob job = task.jobEntry;

                MultiObjectLocalizationAndLabelingResult res = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingResult>(satyamResult.TaskResult);


            }

                return ana;
        }

        public static void SaveResultImagesLocally(string jobGUID, string directoryName)
        {
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            SaveResultImagesLocally(entries, directoryName);
            resultsDB.close();
        }

        public static void SaveAggregatedResultImagesLocally(string jobGUID, string directoryName)
        {
            //SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            //List<SatyamAggregatedResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            //resultsDB.close();
            //SaveAggregatedResultImagesLocally(entries, directoryName);
            SatyamAggregatedProgressiveResultsTableAccess resultsDB = new SatyamAggregatedProgressiveResultsTableAccess();
            List<SatyamAggregatedProgressiveResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            resultsDB.close();
            SaveAggregatedProgressiveResultImagesLocally(entries, directoryName);
        }


        public static void SaveResultImagesLocally(List<SatyamResultsTableEntry> entries, string directoryName)
        {

            if(!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            directoryName = directoryName + "\\Raw";

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

                

                MultiObjectLocalizationAndLabelingResult res = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingResult>(satyamResult.TaskResult);

                string ofilename = URIUtilities.filenameFromURI(task.SatyamURI);
                string[] fields = ofilename.Split('.');
                string fileName = fields[0];

                Image originalImage = ImageUtilities.getImageFromURI(task.SatyamURI);

                MultiObjectLocalizationAndLabelingSubmittedJob jobDefinition = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingSubmittedJob>(job.JobParameters);

                Image imageWithBoundary = DrawingBoxesAndLinesOnImages.addLinesToImage(originalImage, jobDefinition.BoundaryLines, Color.Red, true);

                List<Rectangle> rectangles = new List<Rectangle>();
                List<Color> colors = new List<Color>();
                List<string> ids = new List<string>();
                List<bool> dashed = new List<bool>();
                for(int j=0; j<res.objects.Count;j++)
                {
                    MultiObjectLocalizationAndLabelingResultSingleEntry box = res.objects[j];
                    int x = box.boundingBox.tlx;
                    int y = box.boundingBox.tly;
                    int width = box.boundingBox.brx - box.boundingBox.tlx;
                    int height = box.boundingBox.bry - box.boundingBox.tly;
                    Rectangle r = new Rectangle(x, y, width, height);
                    rectangles.Add(r);

                    string category = box.Category;
                    int colorIndex = jobDefinition.Categories.IndexOf(category);
                    colors.Add(DrawingBoxesAndLinesOnImages.Colors[colorIndex]);

                    string id = j + "-" + category;
                    ids.Add(id);

                    dashed.Add(false);
                }
                Image imageWithBoxesAndBoundary = DrawingBoxesAndLinesOnImages.addRectanglesToImage(imageWithBoundary,rectangles,colors,ids,dashed);

                fileName = fileName + "-Result";
                if(satyamResult.amazonInfo.AssignmentID!="")
                {
                    //fileName = fileName + "-" + satyamResult.amazonInfo.AssignmentID + "_" + entry.ID;
                    fileName = fileName + "-" + entry.ID + "_" + satyamResult.PrevResultID;
                }

                ImageUtilities.saveImage(imageWithBoxesAndBoundary,directoryName,fileName);
            }
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



                MultiObjectLocalizationAndLabelingAggregatedResult res = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingAggregatedResult>(satyamResult.AggregatedResultString);

                string ofilename = URIUtilities.filenameFromURI(task.SatyamURI);
                string[] fields = ofilename.Split('.');
                string fileName = fields[0];

                Image originalImage = ImageUtilities.getImageFromURI(task.SatyamURI);

                MultiObjectLocalizationAndLabelingSubmittedJob jobDefinition = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingSubmittedJob>(job.JobParameters);

                Image imageWithBoundary = DrawingBoxesAndLinesOnImages.addLinesToImage(originalImage, jobDefinition.BoundaryLines, Color.Red, true);

                List<Rectangle> rectangles = new List<Rectangle>();
                List<Color> colors = new List<Color>();
                List<string> ids = new List<string>();
                List<bool> dashed = new List<bool>();
                for (int j = 0; j < res.boxesAndCategories.objects.Count; j++)
                {
                    MultiObjectLocalizationAndLabelingResultSingleEntry box = res.boxesAndCategories.objects[j];
                    int x = box.boundingBox.tlx;
                    int y = box.boundingBox.tly;
                    int width = box.boundingBox.brx - box.boundingBox.tlx;
                    int height = box.boundingBox.bry - box.boundingBox.tly;
                    Rectangle r = new Rectangle(x, y, width, height);
                    rectangles.Add(r);

                    string category = box.Category;
                    int colorIndex = jobDefinition.Categories.IndexOf(category);
                    colors.Add(DrawingBoxesAndLinesOnImages.Colors[colorIndex]);

                    string id = j + "-" + category;
                    ids.Add(id);

                    dashed.Add(false);
                }
                Image imageWithBoxesAndBoundary = DrawingBoxesAndLinesOnImages.addRectanglesToImage(imageWithBoundary, rectangles, colors, ids, dashed);

                fileName = fileName + "-AggregatedResult";
                
                ImageUtilities.saveImage(imageWithBoxesAndBoundary, directoryName, fileName);
            }
        }

        public static void SaveAggregatedProgressiveResultImagesLocally(List<SatyamAggregatedProgressiveResultsTableEntry> entries, string directoryName)
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
                SatyamAggregatedProgressiveResultsTableEntry entry = entries[i];
                SatyamAggregatedResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParameters);
                SatyamJob job = task.jobEntry;



                MultiObjectLocalizationAndLabelingAggregatedResult res = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingAggregatedResult>(satyamResult.AggregatedResultString);

                string ofilename = URIUtilities.filenameFromURI(task.SatyamURI);
                string[] fields = ofilename.Split('.');
                string fileName = fields[0];

                Image originalImage = ImageUtilities.getImageFromURI(task.SatyamURI);

                MultiObjectLocalizationAndLabelingSubmittedJob jobDefinition = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingSubmittedJob>(job.JobParameters);

                Image imageWithBoundary = DrawingBoxesAndLinesOnImages.addLinesToImage(originalImage, jobDefinition.BoundaryLines, Color.Red, true);

                List<Rectangle> rectangles = new List<Rectangle>();
                List<Color> colors = new List<Color>();
                List<string> ids = new List<string>();
                List<bool> dashed = new List<bool>();
                for (int j = 0; j < res.boxesAndCategories.objects.Count; j++)
                {
                    MultiObjectLocalizationAndLabelingResultSingleEntry box = res.boxesAndCategories.objects[j];
                    int x = box.boundingBox.tlx;
                    int y = box.boundingBox.tly;
                    int width = box.boundingBox.brx - box.boundingBox.tlx;
                    int height = box.boundingBox.bry - box.boundingBox.tly;
                    Rectangle r = new Rectangle(x, y, width, height);
                    rectangles.Add(r);

                    string category = box.Category;
                    int colorIndex = jobDefinition.Categories.IndexOf(category);
                    colors.Add(DrawingBoxesAndLinesOnImages.Colors[colorIndex]);

                    string id = j + "-" + category;
                    ids.Add(id);

                    dashed.Add(false);
                }
                Image imageWithBoxesAndBoundary = DrawingBoxesAndLinesOnImages.addRectanglesToImage(imageWithBoundary, rectangles, colors, ids, dashed);

                fileName = fileName + "-AggregatedResult";

                ImageUtilities.saveImage(imageWithBoxesAndBoundary, directoryName, fileName);
            }
        }


        public static Image DrawImageDetectionResult(MultiObjectLocalizationAndLabelingResult res, string image_url, List<string> Categories)
        {
            

            Image originalImage = ImageUtilities.getImageFromURI(image_url);
            List<Rectangle> rectangles = new List<Rectangle>();
            List<Color> colors = new List<Color>();
            List<string> ids = new List<string>();
            List<bool> dashed = new List<bool>();
            for (int j = 0; j < res.objects.Count; j++)
            {
                MultiObjectLocalizationAndLabelingResultSingleEntry box = res.objects[j];
                int x = box.boundingBox.tlx;
                int y = box.boundingBox.tly;
                int width = box.boundingBox.brx - box.boundingBox.tlx;
                int height = box.boundingBox.bry - box.boundingBox.tly;
                Rectangle r = new Rectangle(x, y, width, height);
                rectangles.Add(r);
                string category = box.Category;
                int colorIndex = Categories.IndexOf(category);
                colors.Add(DrawingBoxesAndLinesOnImages.Colors[colorIndex]);
                string id = j + "-" + category;
                ids.Add(id);
                dashed.Add(false);
            }
            Image imageWithBoxes = DrawingBoxesAndLinesOnImages.addRectanglesToImage(originalImage, rectangles, colors, ids, dashed);

            return imageWithBoxes;
        }

    }

}
