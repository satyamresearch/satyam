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
    public partial class SingleObjectLabelingInVideo : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                getNewRandomJob();
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
                amazonInfo.AssignmentID = "";
                amazonInfo.WorkerID = "";
                amazonInfo.HITID = "";

                result.amazonInfo = amazonInfo;

                SingleObjectLabelingResult sresult = new SingleObjectLabelingResult();
                sresult.Category = CategorySelection_RadioButtonList.Items[CategorySelection_RadioButtonList.SelectedIndex].Text;
                string sresultString = JSonUtils.ConvertObjectToJSon<SingleObjectLabelingResult>(sresult);
                result.TaskResult = sresultString;

                string resultString = JSonUtils.ConvertObjectToJSon<SatyamResult>(result);

                SatyamResultsTableAccess resultdb = new SatyamResultsTableAccess();
                resultdb.AddEntry(taskEntry.JobTemplateType, taskEntry.UserID, taskEntry.JobGUID, resultString, taskEntry.ID, PageLoadTime, SubmitTime);
                resultdb.close();
                //SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
                //taskDB.IncrementDoneScore(taskEntry.ID);

                bool status = getNewRandomJob();
                if (!status)
                {
                    Response.Redirect("AllJobsDone.aspx");
                }
            }
        }

        private bool getNewRandomJob()
        {
            SatyamTaskTableAccess taskTableDB = new SatyamTaskTableAccess();
            SatyamTaskTableEntry entry = taskTableDB.getMinimumTriedEntryByTemplate(TaskConstants.Classification_Video);
            if (entry != null)
            {
                taskTableDB.IncrementDoneScore(entry.ID);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(entry.TaskParametersString);
                string uri = task.SatyamURI;
                Hidden_VideoURL.Value = uri;

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