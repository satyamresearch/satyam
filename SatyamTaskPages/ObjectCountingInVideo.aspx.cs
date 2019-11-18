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
using Constants;

namespace SatyamTaskPages
{
    public partial class ObjectCountingInVideo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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

            int count;
            bool isValidCount = int.TryParse(CountTextBox.Text, out count);
            if (!isValidCount)
            {
                ErrorLabel.Text = "Error : Ivalid Count.";
                ErrorLabel.ForeColor = System.Drawing.Color.Red;
                ErrorLabel.Font.Bold = true;
            }
            else if (count < 0)
            {
                ErrorLabel.Text = "Error : Count cannot be less than zero.";
                ErrorLabel.ForeColor = System.Drawing.Color.Red;
                ErrorLabel.Font.Bold = true;
            }
            else
            {
                ErrorLabel.Text = "";

                SatyamTaskTableEntry taskEntry = JSonUtils.ConvertJSonToObject<SatyamTaskTableEntry>(Hidden_TaskEntryString.Value);

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

                ObjectCountingResult sresult = new ObjectCountingResult();
                sresult.Count = count;
                string sresultString = JSonUtils.ConvertObjectToJSon<ObjectCountingResult>(sresult);
                result.TaskResult = sresultString;

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
        }

        private bool getNewRandomJob()
        {
            SatyamTaskTableAccess taskTableDB = new SatyamTaskTableAccess();
            SatyamTaskTableEntry entry = taskTableDB.getMinimumTriedEntryByTemplate(TaskConstants.Counting_Video);
            
            if (entry != null)
            {
                taskTableDB.IncrementDoneScore(entry.ID);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(entry.TaskParametersString);
                string uri = task.SatyamURI;
                Hidden_VideoURL.Value = uri;

                SatyamJob jobDefinitionEntry = task.jobEntry;
                ObjectCountingSubmittedJob job = JSonUtils.ConvertJSonToObject<ObjectCountingSubmittedJob>(jobDefinitionEntry.JobParameters);
                ObjectNameLabel.Text = job.ObjectName;
                ObjectNameLabel1.Text = job.ObjectName;

                if (job.Description != "")
                {
                    DescriptionPanel.Visible = true;
                    DescriptionTextPanel.Controls.Add(new LiteralControl(job.Description));
                }

                CountTextBox.Text = "";

                Hidden_TaskEntryString.Value = JSonUtils.ConvertObjectToJSon<SatyamTaskTableEntry>(entry);
                Hidden_PageLoadTime.Value = DateTime.Now.ToString();

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