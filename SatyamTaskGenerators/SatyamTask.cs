using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLTables;
using JobTemplateClasses;
using AzureBlobStorage;
using Utilities;
using Constants;

namespace SatyamTaskGenerators
{
    public class SatyamTask
    {
        public int SatyamJobSubmissionsTableEntryID; //link to the original job entry (just in case)
        public SatyamJob jobEntry;
        public String SatyamURI; //location of the copied image 
        //public String OriginalURI; //location of the original image       
    }

    

    public static class SatyamTaskGenerator
    {
        public static Dictionary<string, string> JobTemplateToSatyamContainerNameMap = new Dictionary<string, string>()
        {
            { "SINGLE_OBJECT_LABLING_DEMO","demo"},
            { TaskConstants.Classification_Image,"singleobjectlabeling"},
            { TaskConstants.Classification_Image_MTurk,"singleobjectlabeling"},
            { "SINGLE_OBJECT_LABLING_IN_VIDEO_DEMO","demo"},
            { TaskConstants.Classification_Video,"singleobjectlabelinginvideo"},
            { TaskConstants.Classification_Video_MTurk,"singleobjectlabelinginvideo"},
            { "OBJECT_COUNTING_IN_IMAGE_DEMO","demo"},
            { TaskConstants.Counting_Image,"objectcounting"},
            { TaskConstants.Counting_Image_MTurk,"objectcounting"},
            { "OBJECT_COUNTING_IN_VIDEO_DEMO","demo"},
            { TaskConstants.Counting_Video,"objectcountinginvideo"},
            { TaskConstants.Counting_Video_MTurk,"objectcountinginvideo"},
            { "MULTI_OBJECT_LOCALIZATION_AND_LABLING_DEMO","demo"},
            { TaskConstants.Detection_Image,"multiobjectlocalizationandlabeling"},
            { TaskConstants.Detection_Image_MTurk,"multiobjectlocalizationandlabeling"},

            { TaskConstants.Tracking,"multiobjecttracking"},
            { TaskConstants.Tracking_MTurk,"multiobjecttracking"},
            { TaskConstants.TrackletLabeling,"trackletlabeling"},
            { TaskConstants.TrackletLabeling_MTurk,"trackletlabeling"},
            { TaskConstants.Segmentation_Image, "segmentation" },
            { TaskConstants.Segmentation_Image_MTurk, "segmentation"},

            { TaskConstants.OpenEndedQuestion_Image,"miscellaneoustasks"},
            { TaskConstants.OpenEndedQuestion_Image_MTurk,"miscellaneoustasks"},
            { TaskConstants.CameraPoseEsitmation,"miscellaneoustasks"},
            { TaskConstants.CameraPoseEsitmation_MTurk,"miscellaneoustasks"},

        };

        public static Dictionary<string, List<string>> ValidFileTypesByTemplate = new Dictionary<string, List<string>>()
        {
            { "SINGLE_OBJECT_LABLING_DEMO",new List<string>() {DataFormat.Image} },
            { TaskConstants.Classification_Image,new List<string>() {DataFormat.Image}},
            { TaskConstants.Classification_Image_MTurk,new List<string>() {DataFormat.Image}},
            { "SINGLE_OBJECT_LABLING_IN_VIDEO_DEMO",new List<string>() {DataFormat.Video} },
            { TaskConstants.Classification_Video,new List<string>() {DataFormat.Video}},
            { TaskConstants.Classification_Video_MTurk,new List<string>() {DataFormat.Video}},
            { "OBJECT_COUNTING_IN_IMAGE_DEMO",new List<string>() {DataFormat.Image} },
            { TaskConstants.Counting_Image,new List<string>() {DataFormat.Image}},
            { TaskConstants.Counting_Image_MTurk,new List<string>() {DataFormat.Image}},
            { "OBJECT_COUNTING_IN_VIDEO_DEMO",new List<string>() {DataFormat.Video} },
            { TaskConstants.Counting_Video,new List<string>() {DataFormat.Video}},
            { TaskConstants.Counting_Video_MTurk,new List<string>() {DataFormat.Video}},
            { "MULTI_OBJECT_LOCALIZATION_AND_LABLING_DEMO",new List<string>() {DataFormat.Image}},
            { TaskConstants.Detection_Image,new List<string>() {DataFormat.Image}},
            { TaskConstants.Detection_Image_MTurk,new List<string>() {DataFormat.Image}},
            

            { TaskConstants.Tracking,new List<string>() {DataFormat.Video}},
            { TaskConstants.Tracking_MTurk,new List<string>() {DataFormat.Video}},
            { TaskConstants.TrackletLabeling,new List<string>() {DataFormat.Video}},
            { TaskConstants.TrackletLabeling_MTurk,new List<string>() {DataFormat.Video}},
            { TaskConstants.Segmentation_Image,new List<string>() {DataFormat.Image}},
            { TaskConstants.Segmentation_Image_MTurk,new List<string>() {DataFormat.Image}},

            { TaskConstants.OpenEndedQuestion_Image,new List<string>() {DataFormat.Image}},
            { TaskConstants.OpenEndedQuestion_Image_MTurk,new List<string>() {DataFormat.Image}},
            { TaskConstants.CameraPoseEsitmation,new List<string>() {DataFormat.Image}},
            { TaskConstants.CameraPoseEsitmation_MTurk,new List<string>() {DataFormat.Image}},
        };

        public static List<SatyamTask> generate(SatyamJobSubmissionsTableAccessEntry jobEntry)
        {

            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();
            string satyamContainerName = JobTemplateToSatyamContainerNameMap[jobEntry.JobTemplateType];
            string GUID = jobEntry.JobGUID;
            string satyamDirectoryName = GUID;

            SatyamJob job = JSonUtils.ConvertJSonToObject<SatyamJob>(jobEntry.JobParametersString);

            //BlobContainerManager bcm = new BlobContainerManager();
            //string status = bcm.Connect(job.azureInformation.AzureBlobStorageConnectionString);
            //if (status != "SUCCESS")
            //{
            //    return null;
            //}

            ////first copy all the images to satyam location
            //List<string> FileTypes = ValidFileTypesByTemplate[job.JobTemplateType];
            ////Now get all the URIs at the remote location that are relevant
            //List<string> FullURIList = bcm.getURLList(job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName);
            //List<string> URIList = new List<string>();
            //List<string> fileNameList = new List<string>();
            //foreach(string uri in FullURIList)
            //{
            //    string filename = URIUtilities.filenameFromURI(uri);
            //    if(CheckFileExtensions.IsAllowedType(filename,FileTypes))
            //    {
            //        URIList.Add(uri);
            //        fileNameList.Add(filename);
            //    }
            //}

            ////Now create a dictionary mapping filename to URI for both the locations
            //Dictionary<string, string> fileNameToURI = new Dictionary<string, string>();
            //for (int i = 0; i < URIList.Count; i++)
            //{
            //    string fileName = URIUtilities.filenameFromURI(URIList[i]);
            //    fileNameToURI.Add(fileName, URIList[i]);
            //}

            List<string> satyamURIList = new List<string>();
            switch (job.JobTemplateType)
            {
                case TaskConstants.Tracking:
                case TaskConstants.Tracking_MTurk:
                    satyamURIList = satyamStorage.getImmediateNextLevelURLList(satyamContainerName, satyamDirectoryName);// list only immediate next level subblobs, directories / files
                    break;
                case TaskConstants.TrackletLabeling:
                case TaskConstants.TrackletLabeling_MTurk:
                    satyamURIList = satyamStorage.getURLListOfSpecificExtension(satyamContainerName, satyamDirectoryName, new List<string>() { "txt" });  // list only the annotation files per chunk per object
                    break;
                case TaskConstants.Classification_Image:
                case TaskConstants.Classification_Image_MTurk:
                case TaskConstants.Classification_Video:
                case TaskConstants.Classification_Video_MTurk:
                case TaskConstants.Counting_Image:
                case TaskConstants.Counting_Image_MTurk:
                case TaskConstants.Counting_Video:
                case TaskConstants.Counting_Video_MTurk:
                case TaskConstants.Detection_Image:
                case TaskConstants.Detection_Image_MTurk:
                case TaskConstants.Segmentation_Image:
                case TaskConstants.Segmentation_Image_MTurk:
                case TaskConstants.OpenEndedQuestion_Image:
                case TaskConstants.OpenEndedQuestion_Image_MTurk:
                default:
                    // all the images to satyam location                    
                    satyamURIList = satyamStorage.getURLList(satyamContainerName, satyamDirectoryName);
                    break;
            }

            

            
            //Dictionary<string, string> satyamFileNameToURI = new Dictionary<string, string>();
            //for (int i = 0; i < satyamURIList.Count; i++)
            //{
            //    string satyamFileName = URIUtilities.filenameFromURI(satyamURIList[i]);
            //    satyamFileNameToURI.Add(satyamFileName, satyamURIList[i]);
            //}


            //List<string> fileNameList = fileNameToURI.Keys.ToList();

            //now create a task for each of these one by one
            List<SatyamTask> tasks = new List<SatyamTask>();
            foreach (string uri in satyamURIList)
            {
                SatyamTask task = new SatyamTask();
                task.jobEntry = job;
                //task.OriginalURI = fileNameToURI[filename];
                task.SatyamURI = uri;
                tasks.Add(task);
            }
            return tasks;
        }


        public static List<SatyamTask> generateDebug(SatyamJobSubmissionsTableAccessEntry jobEntry,string debugString ,out string newDebugString)
        {
            newDebugString = debugString + "In Generate \n";

            BlobContainerManager containerManager = new BlobContainerManager();

            newDebugString += "About to connect to Satayam\n";
            

            string statusString = containerManager.ConnectDebug(SatyamJobStorageAccountAccess.connection_string);

            newDebugString += "The connetion status is : " + statusString;
            return null;

            //SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();

            

            //newDebugString += "Got a new storage account\n";
            //return null;

            //string satyamContainerName = JobTemplateToSatyamContainerNameMap[jobEntry.JobTemplateType];
            //string GUID = jobEntry.JobGUID;
            //string satyamDirectoryName = GUID;

            //newDebugString += "About to get object from JSon\n";
            //return null;
            //SatyamJob job = JSonUtils.ConvertJSonToObject<SatyamJob>(jobEntry.JobParametersString);
            //newDebugString += "Got the job from JSon\n";
            //return null;


            //BlobContainerManager bcm = new BlobContainerManager();
            //string status = bcm.Connect(job.azureInformation.AzureBlobStorageConnectionString);
            //if (status != "SUCCESS")
            //{
            //    return null;
            //}

            ////first copy all the images to satyam location
            //List<string> FileTypes = ValidFileTypesByTemplate[job.JobTemplateType];
            //satyamStorage.copyFilesFromAnotherAzureBlob(bcm, job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName, satyamContainerName, satyamDirectoryName, FileTypes);

            ////Now get all the URIs at the remote location
            //List<string> FullURIList = bcm.getURLList(job.azureInformation.AzureBlobStorageContainerName, job.azureInformation.AzureBlobStorageContainerDirectoryName);
            //List<string> URIList = new List<string>();
            //foreach (string uri in FullURIList)
            //{
            //    string filename = URIUtilities.filenameFromURI(uri);
            //    if (CheckFileExtensions.IsAllowedType(filename, FileTypes))
            //    {
            //        URIList.Add(uri);
            //    }
            //}

            //List<string> satyamURIList = satyamStorage.getURLList(satyamContainerName, satyamDirectoryName);

            ////Now create a dictionary mapping filename to URI for both the locations
            //Dictionary<string, string> fileNameToURI = new Dictionary<string, string>();
            //Dictionary<string, string> satyamFileNameToURI = new Dictionary<string, string>();

            //for (int i = 0; i < URIList.Count; i++)
            //{
            //    string fileName = URIUtilities.filenameFromURI(URIList[i]);
            //    string satyamFileName = URIUtilities.filenameFromURI(satyamURIList[i]);
            //    fileNameToURI.Add(fileName, URIList[i]);
            //    satyamFileNameToURI.Add(satyamFileName, satyamURIList[i]);
            //}

            //List<string> fileNameList = fileNameToURI.Keys.ToList();

            ////now create a task for each of these one by one
            //List<SatyamTask> tasks = new List<SatyamTask>();
            //foreach (string filename in fileNameList)
            //{
            //    SatyamTask task = new SatyamTask();
            //    task.jobEntry = job;
            //    //task.OriginalURI = fileNameToURI[filename];
            //    task.SatyamURI = satyamFileNameToURI[filename];
            //    tasks.Add(task);
            //}
            //return tasks;
        }



        public static bool PreprocessSubmittedData(SatyamJobSubmissionsTableAccessEntry jobEntry)
        {
            string satyamContainerName = JobTemplateToSatyamContainerNameMap[jobEntry.JobTemplateType];
            string GUID = jobEntry.JobGUID;
            string satyamDirectoryName = GUID;
            SatyamJob job = JSonUtils.ConvertJSonToObject<SatyamJob>(jobEntry.JobParametersString);
            bool success = false;
            switch (job.JobTemplateType)
            {
                case TaskConstants.TrackletLabeling:
                case TaskConstants.TrackletLabeling_MTurk:
                    success = VATIC_Tracklet.ProcessAndUploadToAzureBlob(jobEntry);
                    break;
                case TaskConstants.Tracking:
                case TaskConstants.Tracking_MTurk:
                    success = TrackingDataPreprocessor.ProcessAndUploadToAzureBlob(jobEntry);
                    break;
                case TaskConstants.Classification_Video:
                case TaskConstants.Classification_Video_MTurk:
                    success = VideoClassficationPreprocessor.ProcessAndUploadToAzureBlob(jobEntry);
                    break;
                case TaskConstants.Classification_Image:
                case TaskConstants.Classification_Image_MTurk:
                case TaskConstants.Counting_Image:
                case TaskConstants.Counting_Image_MTurk:
                case TaskConstants.Counting_Video:
                case TaskConstants.Counting_Video_MTurk:
                case TaskConstants.Detection_Image:
                case TaskConstants.Detection_Image_MTurk:
                case TaskConstants.Segmentation_Image:
                case TaskConstants.Segmentation_Image_MTurk:
                case TaskConstants.OpenEndedQuestion_Image:
                case TaskConstants.OpenEndedQuestion_Image_MTurk:
                default:
                    // all the images to satyam location                    
                    success = DefaultDataPreprocessor.copyDataFromUserBlobToSatyamBlob(jobEntry);
                    break;
            }

            
            return success;
        }

        public static void PopulateTasksIntoTaskTable(SatyamJobSubmissionsTableAccessEntry job)
        {
            List<SatyamTask> tasks = generate(job);
            
            if(tasks == null)
            {
                return;
            }

            SatyamTaskTableAccess dbAccess = new SatyamTaskTableAccess();

            foreach (SatyamTask task in tasks)
            {
                String JobTemplateType = job.JobTemplateType;
                String UserID = job.UserID;
                String JobGUID = job.JobGUID;
                String JsonString = JSonUtils.ConvertObjectToJSon<SatyamTask>(task);
                DateTime SubmitTime = job.JobSubmitTime;
                double price = task.jobEntry.amazonHITInformation.Price;
                dbAccess.AddEntry(JobTemplateType, UserID, JobGUID, JsonString, SubmitTime,price);
            }
            dbAccess.close();

            // add the total task number to job submission table
            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            jobDB.UpdateEntryProgress(job.JobGUID, tasks.Count.ToString());
            jobDB.close();
        }

        public static string PopulateTasksIntoTaskTableDebug(SatyamJobSubmissionsTableAccessEntry job)
        {

            string debugString = "In PopulateTasksIntoTaskTable\n";
            
            
            List<SatyamTask> tasks = generateDebug(job,debugString,out debugString);

            return debugString;
            //SatyamTaskTableAccess dbAccess = new SatyamTaskTableAccess();

            //foreach (SatyamTask task in tasks)
            //{
            //    String JobTemplateType = job.JobTemplateType;
            //    String UserID = job.UserID;
            //    String JobGUID = job.JobGUID;
            //    String JsonString = JSonUtils.ConvertObjectToJSon<SatyamTask>(task);
            //    DateTime SubmitTime = job.JobSubmitTime;
            //    dbAccess.AddEntry(JobTemplateType, UserID, JobGUID, JsonString, SubmitTime);
            //}
            //dbAccess.close();
        }
    }


}
