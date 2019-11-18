using AmazonMechanicalTurkAPI;
using Constants;
using JobTemplateClasses;
using SatyamResultAggregators;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTableManagement;
using SQLTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TFServingClient;
using Utilities;

namespace SatyamTaskPages
{
    public partial class MultiObjectDetectionRevisionTaskMTurk : System.Web.UI.Page
    {
        //bool Testing = true;
        bool Testing = false;
        //int doneScore = -1;
        //static int PrevResultID = 0; // default to 0, if it's done for the first time.
        bool TFServingBackend = true;

        protected void Page_Load(object sender, EventArgs e)
        {

            string uri = Request.Url.AbsoluteUri;
            String AssignmentID = Request.QueryString["assignmentId"]; ;
            String WorkerID = Request.QueryString["workerId"]; ;
            String HITID = Request.QueryString["hitId"]; ;
            string reward_string = Request.QueryString["reward"];

            AmazonMTurkUtilities.getAmazonParametersFromURI(uri, out AssignmentID, out HITID, out WorkerID, out reward_string);

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
                Hidden_Price.Value = "0.06";
            }


            if (Testing == true || (AssignmentID != "" && AssignmentID != "ASSIGNMENT_ID_NOT_AVAILABLE"))
            {
                PreacceptancePanel.Visible = false;
                SubmitButton.Enabled = true;
                if (!IsPostBack)
                {
                    bool status = getNewRandomJob();
                    if (!status)
                    {
                        Response.Redirect("AllJobsDone.aspx");
                    }
                }
            }
            else
            {
                // preaccept
                DisplayImage.ImageUrl = "~/Images/DetectionExample.jpg";
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {

            DateTime SubmitTime = DateTime.Now;
            DateTime PageLoadTime = Convert.ToDateTime(Hidden_PageLoadTime.Value);

            SatyamTaskTableEntry taskEntry = JSonUtils.ConvertJSonToObject<SatyamTaskTableEntry>(Hidden_TaskEntryString.Value);

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
            result.TaskResult = Hidden_Result.Value;
            result.PrevResultID = Convert.ToInt32(Hidden_PrevResultID.Value);

            string resultString = JSonUtils.ConvertObjectToJSon<SatyamResult>(result);

            SatyamResultsTableAccess resultdb = new SatyamResultsTableAccess();
            resultdb.AddEntry(taskEntry.JobTemplateType, taskEntry.UserID, taskEntry.JobGUID, resultString, taskEntry.ID, PageLoadTime, SubmitTime);
            resultdb.close();

            SatyamTaskTableManagement.UpdateResultNumber(taskEntry.ID);

            // Decrement the DoneScore, releasing token after latest results are saved.
            SatyamTaskTableAccess taskTableDB = new SatyamTaskTableAccess();
            //taskTableDB.DecrementDoneScore(taskEntry.ID);
            taskTableDB.UpdateDoneScore(taskEntry.ID, 0);
            taskTableDB.close();

            int noDone = Convert.ToInt32(Hidden_NoImagesDone.Value);
            noDone++;
            Hidden_NoImagesDone.Value = noDone.ToString();

            int noTasksPerJob = Convert.ToInt32(Hidden_TasksPerJob.Value);

            bool NotDone = false;

            //if (noDone < noTasksPerJob)
            //{
            //    NotDone = getNewRandomJob();
            //}
            if (NotDone == false)
            {
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

            //Thread.Sleep(5000);
            int maxDoneScore = TaskConstants.MULTI_OBJECT_LOCALIZATION_AND_LABLING_MAX_DONE_SCORE;
            if (SubmitButton.Enabled == true)
            {
                //entry = taskTableDB.getMinimumTriedNewEntryForWorkerIDByTemplateAndPrice(Hidden_AmazonWorkerID.Value,
                //    TaskConstants.Detection_Image_MTurk, price);
                entry = taskTableDB.getMinimumTriedNewEntryForWorkerIDByTemplateAndPriceAndMaxDoneScore(Hidden_AmazonWorkerID.Value,
                    TaskConstants.Detection_Image_MTurk, price, MaxDoneScore: maxDoneScore);
            }
            else
            {
                //entry = taskTableDB.getMinimumTriedEntryByTemplate(TaskConstants.Detection_Image_MTurk);
                entry = taskTableDB.getMinimumTriedEntryByTemplateAndMaxDoneScore(TaskConstants.Detection_Image_MTurk, MaxDoneScore: maxDoneScore);
            }

            if (entry == null)
            {
                taskTableDB.close();
                return false;
            }
            //doneScore = entry.DoneScore;
            //taskTableDB.IncrementDoneScore(entry.ID);
            taskTableDB.close();

            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(entry.TaskParametersString);
            string uri = task.SatyamURI;
            DisplayImage.ImageUrl = uri;

            SatyamJob jobDefinitionEntry = task.jobEntry;
            MultiObjectLocalizationAndLabelingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingSubmittedJob>(jobDefinitionEntry.JobParameters);

            List<string> categories = job.Categories;
            CategorySelection_RadioButtonList.Items.Clear();
            for (int i = 0; i < categories.Count; i++)
            {
                ListItem l = new ListItem(categories[i]);
                CategorySelection_RadioButtonList.Items.Add(l);
            }

            if (job.Description != "")
            {
                DescriptionPanel.Visible = true;
                DescriptionTextPanel.Controls.Add(new LiteralControl(job.Description));
            }

            Hidden_BoundaryLines.Value = JSonUtils.ConvertObjectToJSon(job.BoundaryLines);

            Hidden_TaskEntryString.Value = JSonUtils.ConvertObjectToJSon<SatyamTaskTableEntry>(entry);
            Hidden_PageLoadTime.Value = DateTime.Now.ToString();
            NoLabeled.Text = Hidden_NoImagesDone.Value;
            Hidden_TasksPerJob.Value = jobDefinitionEntry.TasksPerJob.ToString();

            /////////////////////////////////Load Previous Turker Results
            //MultiObjectLocalizationAndLabelingResult res = LoadLatestTurkerResult(entry);

            ///////////////////////////////Load Previous Aggregation Results
            MultiObjectLocalizationAndLabelingResult res = LoadLatestProgressiveAggregationResult(entry);

            //TFServing Backend
            if (res == null && TFServingBackend)
            {
                res = LoadTFServingResult(entry);
                Hidden_PrevResultID.Value = "-1"; // means TF
            }

            if (res == null)
            {
                Hidden_PrevResults.Value = "[]";
                Hidden_PrevResultID.Value = 0.ToString();
            }
            else
            {
                string prevBoxes = JSonUtils.ConvertObjectToJSon(res.objects);
                Hidden_ImageHeight.Value = res.imageHeight.ToString();
                Hidden_ImageWidth.Value = res.imageWidth.ToString();
                Hidden_PrevResults.Value = prevBoxes;
            }
            return true;

        }

        private MultiObjectLocalizationAndLabelingResult LoadLatestTurkerResult(SatyamTaskTableEntry entry)
        {
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUIDAndTaskID(entry.JobGUID, entry.ID);
            if (entries.Count == 0)
            {
                return null;
            }
            //organized sequentially
            SatyamResultsTableEntry prevResult = entries[entries.Count - 1];
            Hidden_PrevResultID.Value = prevResult.ID.ToString();
            SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(prevResult.ResultString);
            MultiObjectLocalizationAndLabelingResult res = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingResult>(satyamResult.TaskResult);
            return res;
        }

        private MultiObjectLocalizationAndLabelingResult LoadLatestProgressiveAggregationResult(SatyamTaskTableEntry entry)
        {
            SatyamAggregatedProgressiveResultsTableAccess aggDB = new SatyamAggregatedProgressiveResultsTableAccess();
            SatyamAggregatedProgressiveResultsTableEntry aggEntry = aggDB.getLatestEntryWithMostResultsAggregatedByTaskID(entry.ID);
            if (aggEntry == null)
            {
                return null;
            }
            Hidden_PrevResultID.Value = aggEntry.ID.ToString();
            SatyamAggregatedResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamAggregatedResult>(aggEntry.ResultString);
            MultiObjectLocalizationAndLabelingAggregatedResult aggRes = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingAggregatedResult>(satyamResult.AggregatedResultString);
            MultiObjectLocalizationAndLabelingResult res = aggRes.boxesAndCategories;
            return res;
        }

        private MultiObjectLocalizationAndLabelingResult LoadTFServingResult(SatyamTaskTableEntry entry)
        {
            SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(entry.TaskParametersString);
            string image_url = task.SatyamURI;

            SatyamJob jobDefinitionEntry = task.jobEntry;
            MultiObjectLocalizationAndLabelingSubmittedJob job = JSonUtils.ConvertJSonToObject<MultiObjectLocalizationAndLabelingSubmittedJob>(jobDefinitionEntry.JobParameters);
            List<string> categories = job.Categories;

            MultiObjectLocalizationAndLabelingResult res = TensorflowServingClient.GetImageDetectionResult(image_url, categories);
            return res;
        }
    }
}