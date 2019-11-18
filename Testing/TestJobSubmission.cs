using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLTables;
using JobTemplateClasses;
using AzureBlobStorage;
using Utilities;
using HelperClasses;
using Constants;

namespace Testing
{
    public static class TestJobSubmission
    {

        public static void CreateDemoJob_SINGLE_OBJECT_LABLING()
        {
            SatyamJob job = new SatyamJob();

            Console.WriteLine("here 1");

            AzureInformation AzureInfo = new AzureInformation();
            AzureInfo.AzureBlobStorageConnectionString = SatyamJobStorageAccountAccess.connection_string;
            AzureInfo.AzureBlobStorageContainerName = "demo";
            AzureInfo.AzureBlobStorageContainerDirectoryName = "testSingleObjectLabelingImages";

            AmazonMTurkHITInformation AmazonInfo = new AmazonMTurkHITInformation();


            SingleObjectLabelingSubmittedJob template = new SingleObjectLabelingSubmittedJob();
            List<string> categories = new List<string>() { "Car", "Bus" };
            template.Categories = categories;
            template.Description = "Car: Includes SUV's, Vans with upto 6 passengers, Jeeps and Pickcups. Click <a href=\"http://satyamtaskpages.azurewebsites.net/CategoryExamplesPageForDemo.aspx\"> here</a> to see examples of the various categories.";

            job.JobGUIDString = "SINGLE_OBJECT_LABLING_DEMO";
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobSubmitTime = DateTime.Now;
            job.JobTemplateType = "SINGLE_OBJECT_LABLING_DEMO";
            job.UserID = TaskConstants.AdminName;
            

            job.JobParameters = JSonUtils.ConvertObjectToJSon<SingleObjectLabelingSubmittedJob>(template);

            string JobParametersString = JSonUtils.ConvertObjectToJSon<SatyamJob>(job);

            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            jobDB.AddEntry(job.JobTemplateType, job.UserID, job.JobGUIDString, JobParametersString, job.JobSubmitTime);
            jobDB.close();
        }


        public static void CreateDemoJob_OBJECT_COUNTING_IN_IMAGE()
        {
            SatyamJob job = new SatyamJob();

            Console.WriteLine("here 1");

            AzureInformation AzureInfo = new AzureInformation();
            AzureInfo.AzureBlobStorageConnectionString = SatyamJobStorageAccountAccess.connection_string;
            AzureInfo.AzureBlobStorageContainerName = "demo";
            AzureInfo.AzureBlobStorageContainerDirectoryName = "catsCount";

            AmazonMTurkHITInformation AmazonInfo = new AmazonMTurkHITInformation();


            ObjectCountingSubmittedJob template = new ObjectCountingSubmittedJob();
            template.ObjectName = "cat";
            template.Description = "By cat we mean house cat and not the general category of cat family like lions or tigers.";

            job.JobGUIDString = "OBJECT_COUNTING_IN_IMAGE_DEMO";
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobSubmitTime = DateTime.Now;
            job.JobTemplateType = "OBJECT_COUNTING_IN_IMAGE_DEMO";
            job.UserID = TaskConstants.AdminName;

            job.JobParameters = JSonUtils.ConvertObjectToJSon<ObjectCountingSubmittedJob>(template);

            string JobParametersString = JSonUtils.ConvertObjectToJSon<SatyamJob>(job);

            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            jobDB.AddEntry(job.JobTemplateType, job.UserID, job.JobGUIDString, JobParametersString, job.JobSubmitTime);
            jobDB.close();
        }

        public static void CreateDemoJob_OBJECT_COUNTING_IN_VIDEO()
        {
            SatyamJob job = new SatyamJob();

            AzureInformation AzureInfo = new AzureInformation();
            AzureInfo.AzureBlobStorageConnectionString = SatyamJobStorageAccountAccess.connection_string;
            AzureInfo.AzureBlobStorageContainerName = "demo";
            AzureInfo.AzureBlobStorageContainerDirectoryName = "PedestrianCountingDemo";

            AmazonMTurkHITInformation AmazonInfo = new AmazonMTurkHITInformation();


            ObjectCountingSubmittedJob template = new ObjectCountingSubmittedJob();
            template.ObjectName = "pedestrian";
            template.Description = "By pedestrians we mean people walking or on wheel chairs not cyclists.";

            job.JobGUIDString = "OBJECT_COUNTING_IN_VIDEO_DEMO";
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobSubmitTime = DateTime.Now;
            job.JobTemplateType = "OBJECT_COUNTING_IN_VIDEO_DEMO";
            job.UserID = TaskConstants.AdminName;

            job.JobParameters = JSonUtils.ConvertObjectToJSon<ObjectCountingSubmittedJob>(template);

            string JobParametersString = JSonUtils.ConvertObjectToJSon<SatyamJob>(job);

            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            jobDB.AddEntry(job.JobTemplateType, job.UserID, job.JobGUIDString, JobParametersString, job.JobSubmitTime);
            jobDB.close();
        }

        public static void CreateDemoJob_MULTI_OBJECT_LOCALIZATION_AND_LABLING()
        {

            //first Job
            SatyamJob job = new SatyamJob();

            AzureInformation AzureInfo = new AzureInformation();
            AzureInfo.AzureBlobStorageConnectionString = SatyamJobStorageAccountAccess.connection_string;
            AzureInfo.AzureBlobStorageContainerName = "demo";
            AzureInfo.AzureBlobStorageContainerDirectoryName = "catsAndDogs";

            AmazonMTurkHITInformation AmazonInfo = new AmazonMTurkHITInformation();


            MultiObjectLocalizationAndLabelingSubmittedJob template = new MultiObjectLocalizationAndLabelingSubmittedJob();
            List<string> categories = new List<string>() { "Cat", "Dog" };
            template.Categories = categories;
            template.Description = "";
            template.BoundaryLines = new List<LineSegment>();
            

            job.JobGUIDString = "MULTI_OBJECT_LOCALIZATION_AND_LABLING_DEMO";
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobSubmitTime = DateTime.Now;
            job.JobTemplateType = "MULTI_OBJECT_LOCALIZATION_AND_LABLING_DEMO";
            job.UserID = TaskConstants.AdminName;


            job.JobParameters = JSonUtils.ConvertObjectToJSon<MultiObjectLocalizationAndLabelingSubmittedJob>(template);

            string JobParametersString = JSonUtils.ConvertObjectToJSon<SatyamJob>(job);

            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            jobDB.AddEntry(job.JobTemplateType, job.UserID, job.JobGUIDString, JobParametersString, job.JobSubmitTime);
            

            //second job
            SatyamJob job1 = new SatyamJob();

            AzureInformation AzureInfo1 = new AzureInformation();
            AzureInfo1.AzureBlobStorageConnectionString = SatyamJobStorageAccountAccess.connection_string;
            AzureInfo1.AzureBlobStorageContainerName = "demo";
            AzureInfo1.AzureBlobStorageContainerDirectoryName = "TrafficExample-EastGate";

            AmazonMTurkHITInformation AmazonInfo1 = new AmazonMTurkHITInformation();


            MultiObjectLocalizationAndLabelingSubmittedJob template1 = new MultiObjectLocalizationAndLabelingSubmittedJob();
            List<string> categories1 = new List<string>() { "Vehicle", "Person" };
            template1.Categories = categories1;
            template1.Description = "Vehicles can be buses, cars, trucks, motorbikes but not bi-cylces or wheelchairs. Person can be pedestrian or a cylist.";
            template1.BoundaryLines = new List<LineSegment>();

            template1.BoundaryLines.Add(new LineSegment(1, 489, 291, 191));
            template1.BoundaryLines.Add(new LineSegment(291, 191, 435, 103));
            template1.BoundaryLines.Add(new LineSegment(435, 103, 664, 68));
            template1.BoundaryLines.Add(new LineSegment(664, 68, 883, 127));
            template1.BoundaryLines.Add(new LineSegment(883, 127, 915, 205));
            template1.BoundaryLines.Add(new LineSegment(915, 205, 1276, 497));
            template1.BoundaryLines.Add(new LineSegment(1276, 497, 1276, 694));
            template1.BoundaryLines.Add(new LineSegment(1276, 694, 1, 696));
            template1.BoundaryLines.Add(new LineSegment(1, 696, 1, 489));

            job1.JobGUIDString = "MULTI_OBJECT_LOCALIZATION_AND_LABLING_DEMO";
            job1.amazonHITInformation = AmazonInfo1;
            job1.azureInformation = AzureInfo1;
            job1.JobSubmitTime = DateTime.Now;
            job1.JobTemplateType = "MULTI_OBJECT_LOCALIZATION_AND_LABLING_DEMO";
            job1.UserID = TaskConstants.AdminName;

            job1.JobParameters = JSonUtils.ConvertObjectToJSon<MultiObjectLocalizationAndLabelingSubmittedJob>(template1);

            string JobParametersString1 = JSonUtils.ConvertObjectToJSon<SatyamJob>(job1);

            jobDB.AddEntry(job1.JobTemplateType, job1.UserID, job1.JobGUIDString, JobParametersString1, job1.JobSubmitTime);
            jobDB.close();
        }


        public static void CreateDemoJob_SINGLE_OBJECT_LABLING_IN_VIDEO()
        {
            SatyamJob job = new SatyamJob();

            AzureInformation AzureInfo = new AzureInformation();
            AzureInfo.AzureBlobStorageConnectionString = SatyamJobStorageAccountAccess.connection_string;
            AzureInfo.AzureBlobStorageContainerName = "demo";
            AzureInfo.AzureBlobStorageContainerDirectoryName = "TestVideoClassificationMP4";

            AmazonMTurkHITInformation AmazonInfo = new AmazonMTurkHITInformation();


            SingleObjectLabelingSubmittedJob template = new SingleObjectLabelingSubmittedJob();
            List<string> categories = new List<string>() { "Biking", "HorseRiding", "SkateBoarding" };
            template.Categories = categories;
            template.Description = "";

            job.JobGUIDString = "SINGLE_OBJECT_LABLING_IN_VIDEO_DEMO";
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobSubmitTime = DateTime.Now;
            job.JobTemplateType = "SINGLE_OBJECT_LABLING_IN_VIDEO_DEMO";
            job.UserID = TaskConstants.AdminName;


            job.JobParameters = JSonUtils.ConvertObjectToJSon<SingleObjectLabelingSubmittedJob>(template);

            string JobParametersString = JSonUtils.ConvertObjectToJSon<SatyamJob>(job);

            SatyamJobSubmissionsTableAccess jobDB = new SatyamJobSubmissionsTableAccess();
            jobDB.AddEntry(job.JobTemplateType, job.UserID, job.JobGUIDString, JobParametersString, job.JobSubmitTime);
            jobDB.close();
        }


        public static void RunTest()
        {
            //CreateDemoJob_SINGLE_OBJECT_LABLING();
            //CreateDemoJob_OBJECT_COUNTING_IN_IMAGE();
            //CreateDemoJob_MULTI_OBJECT_LOCALIZATION_AND_LABLING();
            //CreateDemoJob_SINGLE_OBJECT_LABLING_IN_VIDEO();
            CreateDemoJob_OBJECT_COUNTING_IN_VIDEO();
        }

    }
}
