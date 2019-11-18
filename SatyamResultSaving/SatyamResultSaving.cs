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
using Constants;

namespace SatyamResultsSaving
{

    public class AmazonSavingInfo
    {
        public double pricePerHIT;
        public string AssignmentID;
        public string WorkerID;
        public int TasksPerHIT;
    }
    public class SatyamResultSaveDataRequester
    {
        public bool resultAccepted;
        public bool resultPaid;
        public string ResultString;
        public DateTime TaskStartTime;
        public DateTime TaskEndTime;
        //public string OrignialURI;
        public AmazonSavingInfo amazonInfo;
        public string JobGUID;
        public string JobTemplateType;
        public string UserID;
        public DateTime JobSubmitTime;
        public string JobParameters;//this is a JsonString  


        public SatyamResultSaveDataRequester()
        {

        }
        public SatyamResultSaveDataRequester(SatyamResultsTableEntry entry)
        {
            string status = entry.Status;
            if(status == ResultStatus.accepted || status == ResultStatus.accepted_Paid || status == ResultStatus.accepted_NotPaid)
            {
                resultAccepted = true;
            }
            else
            {
                resultAccepted = false;
            }
            if (status == ResultStatus.rejected_Paid || status == ResultStatus.accepted_Paid)
            {
                resultPaid = true;
            }
            else
            {
                resultPaid = false;
            }
            SatyamResult result = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);    
            TaskStartTime = result.TaskStartTime;
            TaskEndTime = result.TaskEndTime;
            JobGUID = entry.JobGUID;
            JobTemplateType = entry.JobTemplateType;
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(result.TaskParametersString);
            //OrignialURI = task.OriginalURI;
            SatyamJob job = task.jobEntry;
            UserID = job.UserID;
            JobSubmitTime = job.JobSubmitTime;
            JobParameters = job.JobParameters;
            amazonInfo = new AmazonSavingInfo();
            amazonInfo.AssignmentID = result.amazonInfo.AssignmentID;
            amazonInfo.pricePerHIT = result.amazonInfo.PricePerHIT;
            amazonInfo.TasksPerHIT = job.TasksPerJob;
            amazonInfo.WorkerID = result.amazonInfo.WorkerID;
            ResultString = result.TaskResult;
        }

    }

    public class SatyamResultSaveDataSatyam
    {
        public bool resultAccepted;
        public bool resultPaid;
        public string ResultString;
        public DateTime TaskStartTime;
        public DateTime TaskEndTime;
        //public string OrignialURI;
        public string SatyamURI;
        public AmazonSavingInfo amazonInfo;
        public string JobGUID;
        public string JobTemplateType;
        public string UserID;
        public DateTime JobSubmitTime;
        public string JobParameters;//this is a JsonString  

        public SatyamResultSaveDataSatyam()
        {

        }
        public SatyamResultSaveDataSatyam(SatyamResultsTableEntry entry)
        {
            string status = entry.Status;
            if (status == ResultStatus.accepted || status == ResultStatus.accepted_Paid || status == ResultStatus.accepted_NotPaid)
            {
                resultAccepted = true;
            }
            else
            {
                resultAccepted = false;
            }
            if (status == ResultStatus.rejected_Paid || status == ResultStatus.accepted_Paid)
            {
                resultPaid = true;
            }
            else
            {
                resultPaid = false;
            }
            SatyamResult result = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
            TaskStartTime = result.TaskStartTime;
            TaskEndTime = result.TaskEndTime;
            JobGUID = entry.JobGUID;
            JobTemplateType = entry.JobTemplateType;
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(result.TaskParametersString);
            //OrignialURI = task.OriginalURI;
            SatyamJob job = task.jobEntry;
            UserID = job.UserID;
            JobSubmitTime = job.JobSubmitTime;
            JobParameters = job.JobParameters;
            amazonInfo = new AmazonSavingInfo();
            amazonInfo.AssignmentID = result.amazonInfo.AssignmentID;
            amazonInfo.pricePerHIT = result.amazonInfo.PricePerHIT;
            amazonInfo.TasksPerHIT = job.TasksPerJob;
            amazonInfo.WorkerID = result.amazonInfo.WorkerID;
            ResultString = result.TaskResult;
        }

    }

    public static class SatyamSaveResults
    {

        public static void SaveByGUIDRequester(string guid)
        {
            //get all the results of a GUID
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);

            if (results.Count == 0)
            {
                resultsDB.close();
                return;
            }
            //get the azure info
            SatyamResult result = JSonUtils.ConvertJSonToObject<SatyamResult>(results[0].ResultString);
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(result.TaskParametersString);
            SatyamJob job = task.jobEntry;
            string ConnectionString = job.azureInformation.AzureBlobStorageConnectionString;
            string ContainerName = job.azureInformation.AzureBlobStorageContainerName;
            string DirectoryName = job.azureInformation.AzureBlobStorageContainerDirectoryName;

            //create the data to be saved
            List<SatyamResultSaveDataRequester> savingDataList = new List<SatyamResultSaveDataRequester>();
            StringBuilder s = new StringBuilder();

            for (int i = 0; i < results.Count; i++)
            {
                SatyamResultSaveDataRequester data = new SatyamResultSaveDataRequester(results[i]);
                String jsonString = JSonUtils.ConvertObjectToJSon(data);
                s.Append(jsonString);
                if (i == results.Count - 1)
                {
                    s.Append("\n");
                }
            }
            string dataToBeSaved = s.ToString();

            BlobContainerManager bcm = new BlobContainerManager();
            bcm.Connect(ConnectionString);
            string FileName = "Results-" + results[0].JobGUID + ".txt";
            bcm.SaveATextFile(ContainerName, DirectoryName, FileName, dataToBeSaved);
            resultsDB.close();
        }
        
        public static void SaveByGUIDSatyam(string guid)
        {

            //get all the results of a GUID
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> results = resultsDB.getEntriesByGUID(guid);

            if (results.Count == 0)
            {
                resultsDB.close();
                return;
            }

            //create the data to be saved
            List<SatyamResultSaveDataSatyam> savingDataList = new List<SatyamResultSaveDataSatyam>();
            StringBuilder s = new StringBuilder();

            for (int i = 0; i < results.Count; i++)
            {
                SatyamResultSaveDataSatyam data = new SatyamResultSaveDataSatyam(results[i]);
                String jsonString = JSonUtils.ConvertObjectToJSon(data);
                s.Append(jsonString);
                if (i == results.Count - 1)
                {
                    s.Append("\n");
                }
            }
            string dataToBeSaved = s.ToString();

            SatyamJobStorageAccountAccess storage = new SatyamJobStorageAccountAccess();
            string FileName = "Results-" + results[0].JobGUID + ".txt";

            string satyamDirectoryName = SatyamTaskGenerator.JobTemplateToSatyamContainerNameMap[results[0].JobTemplateType];

            storage.SaveATextFile(satyamDirectoryName, results[0].JobGUID, FileName, dataToBeSaved);
            resultsDB.close();
        }
    }


}
