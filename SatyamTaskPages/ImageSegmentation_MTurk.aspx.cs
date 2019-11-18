using AmazonMechanicalTurkAPI;
using Constants;
using JobTemplateClasses;
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
using Utilities;

namespace SatyamTaskPages
{
    public partial class ImageSegmentation_MTurk : System.Web.UI.Page
    {
        //bool Testing = true;
        bool Testing = false;
        //int doneScore = -1;

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
                SubmitButton.Enabled = true;
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
                Hidden_Price.Value = "0.50";
            }

            Page.GetPostBackEventReference(SubmitButton);
            if (!IsPostBack)
            {
                bool status = getNewRandomJob();
                if (!status)
                {
                    Response.Redirect("AllJobsDone.aspx");
                }
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
            Response.Redirect("AllJobsDone.aspx");
            //}
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

            if (SubmitButton.Enabled == true)
            {
                //entry = taskTableDB.getMinimumTriedEntryByTemplateAndPrice(TaskConstants.Detection_Image_MTurk, price);
                //entry = taskTableDB.getMinimumTriedNewEntryForWorkerIDByTemplateAndPrice(Hidden_AmazonWorkerID.Value,
                //    TaskConstants.Detection_Image_MTurk, price, doneScore);
                entry = taskTableDB.getMinimumTriedNewEntryForWorkerIDByTemplateAndPrice(Hidden_AmazonWorkerID.Value,
                    TaskConstants.Segmentation_Image_MTurk, price);

            }
            else
            {
                entry = taskTableDB.getMinimumTriedEntryByTemplate(TaskConstants.Segmentation_Image_MTurk);
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
            string uri = task.SatyamURI;
            TheImage.ImageUrl = uri;

            SatyamJob jobDefinitionEntry = task.jobEntry;
            ImageSegmentationSubmittedJob job = JSonUtils.ConvertJSonToObject<ImageSegmentationSubmittedJob>(jobDefinitionEntry.JobParameters);

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

            return true;

        }
    }
}