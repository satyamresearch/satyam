using AzureBlobStorage;
using Constants;
using JobTemplateClasses;
using SatyamResultClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
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
    public partial class TrackletLabeling : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
            amazonInfo.AssignmentID = "";
            amazonInfo.WorkerID = "";
            amazonInfo.HITID = "";

            result.amazonInfo = amazonInfo;
            result.TaskResult = s;

            string resultString = JSonUtils.ConvertObjectToJSon<SatyamResult>(result);

            SatyamResultsTableAccess resultdb = new SatyamResultsTableAccess();
            resultdb.AddEntry(taskEntry.JobTemplateType, taskEntry.UserID, taskEntry.JobGUID, resultString, taskEntry.ID, PageLoadTime, SubmitTime);
            resultdb.close();

            //SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
            //taskDB.IncrementDoneScore(taskEntry.ID);

            bool NotDone = getNewRandomJob();
            if (NotDone == false)
            {
                Response.Redirect("AllJobsDone.aspx");
            }
        }

        private bool getNewRandomJob()
        {
            SatyamTaskTableAccess taskTableDB = new SatyamTaskTableAccess();
            SatyamTaskTableEntry entry = taskTableDB.getMinimumTriedEntryByTemplate(TaskConstants.TrackletLabeling);
            if (entry != null) {
                taskTableDB.IncrementDoneScore(entry.ID);
            }
            taskTableDB.close();


            if (entry != null)
            {
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
            else
            {
                return false;
            }
        }
    }
}