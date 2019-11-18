using AmazonMechanicalTurkAPI;
using AzureBlobStorage;
using Constants;
using JobTemplateClasses;
using SatyamResultClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTableManagement;
using SQLTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Utilities;

namespace SatyamTaskPages
{
    public partial class TraceletLabeling_MTurk : System.Web.UI.Page
    {

        //bool Testing = true;
        bool Testing = false;
        int doneScore = -1;

        protected void Page_Load(object sender, EventArgs e)
        {
            string uri = Request.Url.AbsoluteUri;
            String AssignmentID = Request.QueryString["assignmentId"]; ;
            String WorkerID = Request.QueryString["workerId"]; ;
            String HITID = Request.QueryString["hitId"]; ;
            string reward_string = Request.QueryString["reward"];

            AmazonMTurkUtilities.getAmazonParametersFromURI(uri, out AssignmentID, out HITID, out WorkerID, out reward_string);

            if (Testing == true || (AssignmentID != "" && AssignmentID != "ASSIGNMENT_ID_NOT_AVAILABLE"))
            {
                //PreacceptancePanel.Visible = false;
                Submit_Button.Enabled = true;
            }

            if (!Testing)
            {
                Hidden_AmazonAssignmentID.Value = AssignmentID;
                Hidden_AmazonWorkerID.Value = WorkerID;
                Hidden_HITID.Value = HITID;
                Hidden_Price.Value = reward_string;

                SatyamAmazonHITTableAccess HITdb = new SatyamAmazonHITTableAccess();
                HITdb.UpdateStatusByHITID(HITID, HitStatus.taken);
                HITdb.close();
            }
            else
            {
                Hidden_AmazonAssignmentID.Value = "Testing";
                Hidden_AmazonWorkerID.Value = "Testing";
                Hidden_HITID.Value = "Testing";
                Hidden_Price.Value = "0.02";
            }

            Page.GetPostBackEventReference(Submit_Button);
            if (IsPostBack)
            {
                if (Skipped_Hidden.Value == "false")
                {
                    storeResult();
                }
            }
            if (FinalDone_Hidden.Value == "false")
            {
                bool status = getNewRandomJob();
                if (!status)
                {
                    Response.Redirect("AllJobsDone.aspx");
                }
            }
        }

        private void storeResult()
        {
            DateTime SubmitTime = DateTime.Now;
            DateTime PageLoadTime = Convert.ToDateTime(Hidden_PageLoadTime.Value);

            SatyamTaskTableEntry taskEntry = JSonUtils.ConvertJSonToObject<SatyamTaskTableEntry>(Hidden_TaskEntryString.Value);

            string tracksString = TracksOutput_Hidden.Value;
            Console.WriteLine(tracksString);
            string urlList = Hidden_ImageURLList.Value;
            string[] fields = urlList.Split(',');

            DateTime start = DateTime.MinValue;
            List<DateTime> frameTimes = new List<DateTime>();
            //double frameTimeSpanInMiliseconds = (Convert.ToDouble(Hidden_ChunkDuration.Value) / (double)fields.Length) * 1000;
            double frameTimeSpanInMiliseconds = (double)(1000) / Convert.ToDouble(fps_Hidden.Value);
            for (int i = 0; i < fields.Length; i++)
            {
                DateTime t;
                t = start.AddMilliseconds(frameTimeSpanInMiliseconds * i);
                frameTimes.Add(t);
            }
            string s = Raw_VATIC_DVA_Crowdsourced_Track_Collection.Raw_VATIC_DVA_Crowdsourced_Track_Collection_ToTrackStrings(tracksString, frameTimes);

            SatyamResult result = new SatyamResult();

            result.TaskParametersString = taskEntry.TaskParametersString;
            result.TaskStartTime = PageLoadTime;
            result.TaskEndTime = SubmitTime;
            result.TaskTableEntryID = taskEntry.ID;

            AmazonTaskResultInfo amazonInfo = new AmazonTaskResultInfo();
            amazonInfo.AssignmentID = Hidden_AmazonAssignmentID.Value;
            amazonInfo.WorkerID = Hidden_AmazonWorkerID.Value;
            amazonInfo.HITID = Hidden_HITID.Value;
            amazonInfo.PricePerHIT = Convert.ToDouble(Hidden_Price.Value);

            result.amazonInfo = amazonInfo;
            result.TaskResult = s;

            string resultString = JSonUtils.ConvertObjectToJSon<SatyamResult>(result);

            SatyamResultsTableAccess resultdb = new SatyamResultsTableAccess();
            resultdb.AddEntry(taskEntry.JobTemplateType, taskEntry.UserID, taskEntry.JobGUID, resultString, taskEntry.ID, PageLoadTime, SubmitTime);
            resultdb.close();
            SatyamTaskTableManagement.UpdateResultNumber(taskEntry.ID);

            if (!Testing)
            {
                SatyamAmazonHITTableAccess HITdb = new SatyamAmazonHITTableAccess();
                string HITID = result.amazonInfo.HITID;
                HITdb.UpdateStatusByHITID(HITID, HitStatus.submitted);
                HITdb.close();
                AmazonMTurkNotification.submitAmazonTurkHit(result.amazonInfo.AssignmentID, result.amazonInfo.WorkerID, false);
            }
            Response.Redirect("MTurkTaskDonePage.aspx");
        }

        private bool getNewRandomJob()
        {
            double price = 0;
            bool success = Double.TryParse(Hidden_Price.Value, out price);
            if (!success)
            {
                price = 0;
            }
            SatyamTaskTableAccess taskTableDB = new SatyamTaskTableAccess();
            SatyamTaskTableEntry entry = null;
            if (Submit_Button.Enabled == true)
            {
                //entry = taskTableDB.getMinimumTriedNewEntryForWorkerIDByTemplateAndPrice(Hidden_AmazonWorkerID.Value,
                //    TaskConstants.TrackletLabeling_MTurk, price);
                entry = taskTableDB.getTopKNewEntryForWorkerIDByTemplateAndPrice(50, Hidden_AmazonWorkerID.Value,
                    TaskConstants.TrackletLabeling_MTurk, price);
            }
            else
            {
                entry = taskTableDB.getMinimumTriedEntryByTemplate(TaskConstants.TrackletLabeling_MTurk);
            }

            taskTableDB.close();


            if (entry == null)
            {
                return false;
            }
            taskTableDB = new SatyamTaskTableAccess();
            taskTableDB.IncrementDoneScore(entry.ID);
            taskTableDB.close();

            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(entry.TaskParametersString);

            SatyamJobStorageAccountAccess satyamStorage = new SatyamJobStorageAccountAccess();

            string videoDir = URIUtilities.localDirectoryFullPathFromURI(task.SatyamURI);
            List<string> ImageURLs = satyamStorage.getURLListOfSpecificExtensionUnderSubDirectoryByURI(videoDir, new List<string>() { "jpg" });

            string annotationFilePath = task.SatyamURI;

            //string urls = "";
            //for (int i=0;i<ImageURLs.Count;i++)
            //{
            //    urls += ImageURLs[i];
            //    if (i == ImageURLs.Count - 1) break;
            //    urls += ',';
            //}
            Hidden_ImageURLList.Value = ObjectsToStrings.ListString(ImageURLs, ',');

            SatyamJob jobDefinitionEntry = task.jobEntry;
            MultiObjectTrackingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectTrackingSubmittedJob>(jobDefinitionEntry.JobParameters);

            Dictionary<string, List<string>> subcategories = job.Categories;
            List<string> categories = subcategories.Keys.ToList();
            //CategorySelection_RakdioButtonList.Items.Clear();
            for (int i = 0; i < categories.Count; i++)
            {
                ListItem l = new ListItem(categories[i]);
                //CategorySelection_RadioButtonList.Items.Add(l);
            }

            if (job.Description != "")
            {
                //DescriptionPanel.Visible = true;
                //DescriptionTextPanel.Controls.Add(new LiteralControl(job.Description));
            }
            //Hidden_BoundaryLines.Value = JSonUtils.ConvertObjectToJSon(job.BoundaryLines);


            Hidden_TaskEntryString.Value = JSonUtils.ConvertObjectToJSon<SatyamTaskTableEntry>(entry);
            Hidden_PageLoadTime.Value = DateTime.Now.ToString();


            // pass parameters from old template
            Slug_Hidden.Value = "null";
            Start_Hidden.Value = "0";
            Stop_Hidden.Value = (ImageURLs.Count - 1).ToString();
            Skip_Hidden.Value = "0";
            PerObject_Hidden.Value = "0.1";
            Completion_Hidden.Value = "0.5";
            BlowRadius_Hidden.Value = "0";
            JobId_Hidden.Value = "1";
            LabelString_Hidden.Value = ObjectsToStrings.ListString(categories.ToList(), ',');
            Attributes_Hidden.Value = ObjectsToStrings.DictionaryStringListString(subcategories, ',', ':', '_');
            Training_Hidden.Value = "0";
            fps_Hidden.Value = job.FrameRate.ToString();
            Hidden_ChunkDuration.Value = job.ChunkDuration.ToString();

            var web = new WebClient();
            System.Drawing.Image x = System.Drawing.Image.FromStream(web.OpenRead(ImageURLs[0]));
            ImageWidth_Hidden.Value = x.Width.ToString();
            ImageHeight_Hidden.Value = x.Height.ToString();

            // image boundary for now
            //string[] region = new string[] { "0-0-1242-0-1242-375-0-375-0-0" };
            string[] region = new string[] { "0-0-" + x.Width + "-0-" + x.Width + "-" + x.Height + "-0-" + x.Height + "-0-0" };
            RegionString_Hidden.Value = ObjectsToStrings.ListString(region, ',');

            // temp test
            List<VATIC_Tracklet> prevTracesTemp = new List<VATIC_Tracklet>();

            WebClient client = new WebClient();
            Stream stream = client.OpenRead(annotationFilePath);
            StreamReader reader = new StreamReader(stream);
            List<string> trace = new List<string>();
            while (reader.Peek() >= 0)
            {
                string content = reader.ReadLine();
                trace.Add(content);
            }


            Dictionary<string, VATIC_Tracklet> tracklets = VATIC_Tracklet.ReadTrackletsFromVIRAT(trace);

            foreach (string id in tracklets.Keys)
            {
                //string output = JSonUtils.ConvertObjectToJSon(tracklets[id]);
                prevTracesTemp.Add(tracklets[id]);
            }
            string output = JSonUtils.ConvertObjectToJSon(prevTracesTemp);
            PreviousTrackString_Hidden.Value = output;

            return true;
            
        }
    }
}