using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLTables;
using SatyamTaskResultClasses;
using Utilities;
using SatyamTaskGenerators;
using JobTemplateClasses;
using AzureBlobStorage;
using SatyamResultAggregators;
using Constants;

namespace SatyamResultsSaving
{


    public class SatyamSaveAggregatedDataRequester
    {
        public string AggregatedResultString;
        //public string OrignialURI;
        public string JobGUID;
        public string JobTemplateType;
        public string UserID;
        public DateTime JobSubmitTime;
        public DateTime JobCompletionTime; //time of saving the result
        public string JobParameters;//this is a JsonString  

        public SatyamSaveAggregatedDataRequester()
        {

        }

        public SatyamSaveAggregatedDataRequester(SatyamAggregatedResultsTableEntry entry)
        {
            SatyamAggregatedResult result = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(result.TaskParameters);
            SatyamJob job = task.jobEntry;
            //OrignialURI = task.OriginalURI;
            JobGUID = entry.JobGUID;
            JobTemplateType = entry.JobTemplateType;
            UserID = entry.UserID;
            JobSubmitTime = job.JobSubmitTime;
            JobParameters = job.JobParameters;
            JobCompletionTime = DateTime.Now;
            AggregatedResultString = result.AggregatedResultString;
        }
    }

    public class SatyamSaveAggregatedDataSatyam
    {
        public string AggregatedResultString;
        //public string OrignialURI;
        public string SatyamURI;
        public string JobGUID;
        public string JobTemplateType;
        public string UserID;
        public DateTime JobSubmitTime;
        public DateTime JobCompletionTime;//time of saving the result
        public string JobParameters;//this is a JsonString 

        public SatyamSaveAggregatedDataSatyam()
        {

        }

        public SatyamSaveAggregatedDataSatyam(SatyamAggregatedResultsTableEntry entry)
        {
            SatyamAggregatedResult result = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(entry.ResultString);
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(result.TaskParameters);
            SatyamJob job = task.jobEntry;
            //OrignialURI = task.OriginalURI;
            SatyamURI = task.SatyamURI;
            JobGUID = entry.JobGUID;
            JobTemplateType = entry.JobTemplateType;
            UserID = entry.UserID;
            JobSubmitTime = job.JobSubmitTime;
            JobParameters = job.JobParameters;
            JobCompletionTime = DateTime.Now;
            AggregatedResultString = result.AggregatedResultString;
        }
    }

    public static class SatyamSaveAggregatedResult
    {
        public static void SaveByGUIDRequester(string guid)
        {
            //get all aggregated results
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();
            if (results.Count == 0) return;
            //get the saving location info
            SatyamAggregatedResult result = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(results[0].ResultString);
            SingleObjectLabelingAggregatedResult sresult = JSonUtils.ConvertJSonToObject<SingleObjectLabelingAggregatedResult>(result.AggregatedResultString);
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(result.TaskParameters);
            SatyamJob job = task.jobEntry;
            string ConnectionString = job.azureInformation.AzureBlobStorageConnectionString;
            string ContainerName = job.azureInformation.AzureBlobStorageContainerName;
            string DirectoryName = job.azureInformation.AzureBlobStorageContainerDirectoryName;

            StringBuilder s = new StringBuilder();

            for (int i = 0; i < results.Count; i++)
            {
                SatyamSaveAggregatedDataRequester data = new SatyamSaveAggregatedDataRequester(results[i]);
                String jsonString = JSonUtils.ConvertObjectToJSon<SatyamSaveAggregatedDataRequester>(data);
                s.Append(jsonString);
                if (i == results.Count - 1)
                {
                    s.Append("\n");
                }
            }

            string dataToBeSaved = s.ToString();

            BlobContainerManager bcm = new BlobContainerManager();
            bcm.Connect(ConnectionString);
            string FileName = "AggregatedResults-" + results[0].JobGUID + ".txt";
            bcm.SaveATextFile(ContainerName, DirectoryName, FileName, dataToBeSaved);
        }

        public static void SaveByGUIDSatyam(string guid)
        {
            //get all aggregated results
            SatyamAggregatedResultsTableAccess resultsDB = new SatyamAggregatedResultsTableAccess();
            List<SatyamAggregatedResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);
            resultsDB.close();
            if (results.Count == 0) return;

            StringBuilder s = new StringBuilder();

            for (int i = 0; i < results.Count; i++)
            {
                SatyamSaveAggregatedDataSatyam data = new SatyamSaveAggregatedDataSatyam(results[i]);
                String jsonString = JSonUtils.ConvertObjectToJSon<SatyamSaveAggregatedDataSatyam>(data);
                s.Append(jsonString);
                if (i == results.Count - 1)
                {
                    s.Append("\n");
                }
            }

            string dataToBeSaved = s.ToString();
            SatyamJobStorageAccountAccess storage = new SatyamJobStorageAccountAccess();
            string FileName = "AggregatedResults-" + results[0].JobGUID + ".txt";

            string satyamDirectoryName = SatyamTaskGenerator.JobTemplateToSatyamContainerNameMap[results[0].JobTemplateType];
            //switch(results[0].JobTemplateType)
            //{
            //    case TaskConstants.Classification_Image:
            //    case TaskConstants.Classification_Image_MTurk:
            //        satyamDirectoryName = "singleobjectlabeling";
            //        break;
            //    case TaskConstants.Classification_Video:
            //    case TaskConstants.Classification_Video_MTurk:
            //        satyamDirectoryName = "singleobjectlabelinginvideo";
            //        break;
            //    case TaskConstants.Counting_Image:
            //    case TaskConstants.Counting_Image_MTurk:
            //        satyamDirectoryName = "objectcounting";
            //        break;
            //    case TaskConstants.Counting_Video:
            //    case TaskConstants.Counting_Video_MTurk:
            //        satyamDirectoryName = "objectcountinginvideo";
            //        break;
            //    case TaskConstants.Detection_Image:
            //    case TaskConstants.Detection_Image_MTurk:
            //        satyamDirectoryName = "multiobjectlocalizationandlabeling";
            //        break;
            //    case TaskConstants.Tracking:
            //    case TaskConstants.Tracking_MTurk:
            //        satyamDirectoryName = "multiobjecttracking";
            //        break;
            //}
            storage.SaveATextFile(satyamDirectoryName, results[0].JobGUID, FileName, dataToBeSaved);
        }
    }
}
