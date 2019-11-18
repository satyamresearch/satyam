using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using SQLTables;
using SatyamTaskGenerators;
using Utilities;
using JobTemplateClasses;
using SatyamTaskResultClasses;
using AmazonMechanicalTurkAPI;
using Constants;
using SQLTableManagement;

namespace SatyamTaskPages
{
    public partial class SingleObjectLabelingMTurk : System.Web.UI.Page
    {
        //bool Testing = true;
        bool Testing = false;
        int DoneScore = -1;

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
                PreacceptancePanel.Visible = false;
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
                Hidden_Price.Value = "0.02";
            }

            if (!IsPostBack)
            {
                bool res = getNewRandomJob();
                if (!res)
                {
                    Response.Redirect("AllJobsDone.aspx");
                }
            }
        }

        protected void SubmitButton_Click(object sender, EventArgs e)
        {
            DateTime SubmitTime = DateTime.Now;
            DateTime PageLoadTime = Convert.ToDateTime(Hidden_PageLoadTime.Value);


            if (CategorySelection_RadioButtonList.SelectedIndex != -1)
            {
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

                SingleObjectLabelingResult sresult = new SingleObjectLabelingResult();
                sresult.Category = CategorySelection_RadioButtonList.Items[CategorySelection_RadioButtonList.SelectedIndex].Text;
                string sresultString = JSonUtils.ConvertObjectToJSon<SingleObjectLabelingResult>(sresult);
                result.TaskResult = sresultString;

                string resultString = JSonUtils.ConvertObjectToJSon<SatyamResult>(result);

                SatyamResultsTableAccess resultdb = new SatyamResultsTableAccess();
                resultdb.AddEntry(taskEntry.JobTemplateType, taskEntry.UserID, taskEntry.JobGUID, resultString, taskEntry.ID, PageLoadTime, SubmitTime);
                resultdb.close();
                SatyamTaskTableManagement.UpdateResultNumber(taskEntry.ID);

                //SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
                //taskDB.IncrementDoneScore(taskEntry.ID);

                int noDone = Convert.ToInt32(Hidden_NoImagesDone.Value);
                noDone++;
                Hidden_NoImagesDone.Value = noDone.ToString();

                int noTasksPerJob = Convert.ToInt32(Hidden_TasksPerJob.Value);

                bool NotDone = false;

                if (noDone < noTasksPerJob)
                {
                    NotDone = getNewRandomJob();
                }
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
            if (SubmitButton.Enabled == true)
            {
                //entry = taskTableDB.getMinimumTriedEntryByTemplateAndPrice(TaskConstants.Classification_Image_MTurk, price, DoneScore);
                entry = taskTableDB.getMinimumTriedEntryByTemplateAndPrice(TaskConstants.Classification_Image_MTurk, price);

                //entry = taskTableDB.getMinimumTriedNewEntryForWorkerIDByTemplateAndPrice(Hidden_AmazonWorkerID.Value,
                //    TaskConstants.Classification_Image_MTurk, price, DoneScore);                


            }
            else
            {
                entry = taskTableDB.getMinimumTriedEntryByTemplate(TaskConstants.Classification_Image_MTurk);
            }
            if (entry != null)
            {
                //DoneScore = entry.DoneScore;//next time you don't have to start from -1
                taskTableDB.IncrementDoneScore(entry.ID);


                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(entry.TaskParametersString);
                string uri = task.SatyamURI;
                DisplayImage.ImageUrl = uri;

                SatyamJob jobDefinitionEntry = task.jobEntry;
                SingleObjectLabelingSubmittedJob job = JSonUtils.ConvertJSonToObject<SingleObjectLabelingSubmittedJob>(jobDefinitionEntry.JobParameters);
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
                    //DescriptionLabel.Text = job.Description;
                }

                Hidden_TasksPerJob.Value = jobDefinitionEntry.TasksPerJob.ToString();

                Hidden_TaskEntryString.Value = JSonUtils.ConvertObjectToJSon<SatyamTaskTableEntry>(entry);
                Hidden_PageLoadTime.Value = DateTime.Now.ToString();
                NoLabeled.Text = Hidden_NoImagesDone.Value;
                taskTableDB.close();
                return true;
            }
            else
            {
                taskTableDB.close();
                return false;
            }
        }
    }
}