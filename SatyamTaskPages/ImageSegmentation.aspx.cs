using Constants;
using JobTemplateClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
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
    public partial class ImageSegementation : System.Web.UI.Page
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
            result.TaskResult = Hidden_Result.Value;

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
            SatyamTaskTableEntry entry = taskTableDB.getMinimumTriedEntryByTemplate(TaskConstants.Segmentation_Image);
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