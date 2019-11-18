using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Utilities;
using JobTemplateClasses;
using SQLTables;
using AmazonMechanicalTurkAPI;
using Constants;

namespace SatyamPortal
{
    public partial class Template_SingleObjectLabelingInVideo : System.Web.UI.Page
    {
        //temporary to be removed
        string userName = TaskConstants.AdminName;

        //permanent
        List<string> categories;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //first update all the masterpage related entities:
                ContentPlaceHolder mpContentPlaceHolder = (ContentPlaceHolder)this.Master.Master.FindControl("MainTemplatePlaceHolder");
                DropDownList ddlist1 = (DropDownList)mpContentPlaceHolder.FindControl("TemplatesDropDownList");
                ddlist1.Items.FindByText("Single Object Labeling In Video").Selected = true;

                Label usernameLabel = (Label)this.Master.Master.FindControl("UsernameLabel");
                usernameLabel.Text = userName;

                //Create a new Job GUID
                NewJobGUID.Text = Guid.NewGuid().ToString();

            }

            if (IsPostBack)
            {
                categories = new List<string>();
                for (int i = 0; i < CategoryListBox.Items.Count; i++)
                {
                    categories.Add(CategoryListBox.Items[i].Text);
                }
                categories.Add("None of the Above");
            }
        }

        protected void Template_SingleObjectLabeling_AddCategoryTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        protected void Template_SingleObjectLabeling_AddNewCategoryButton_Click(object sender, EventArgs e)
        {
            string newCategory = AddCategoryTextBox.Text;
            if (newCategory == "")
            {
                return;
            }
            else if (categories.Contains(newCategory))
            {
                return;
            }
            ListItem item = new ListItem(newCategory);
            CategoryListBox.Items.Add(item);
            AddCategoryTextBox.Text = "";
        }

        protected void DeleteCategoryButton_Click(object sender, EventArgs e)
        {
            List<ListItem> categoriesToBeDeleted = new List<ListItem>();
            for (int i = 0; i < CategoryListBox.Items.Count; i++)
            {
                if (CategoryListBox.Items[i].Selected)
                {
                    categoriesToBeDeleted.Add(CategoryListBox.Items[i]);
                }
            }
            for (int i = 0; i < categoriesToBeDeleted.Count; i++)
            {
                CategoryListBox.Items.Remove(categoriesToBeDeleted[i]);
            }
        }


        protected void JobSubmitButton_Click(object sender, EventArgs e)
        {
            string AzureBlobStorageConnectionString = AzureBlobStorageConnectionStringTextBox.Text;
            string AzureContainer = AzureBlobStorageContainerNameTextBox.Text;
            string AzureContainerDirectory = AzureBlobStorageContainerDirectoryNameTextBox.Text;
            string AmazonAccessKeyIDValue = AmazonAccessKeyID.Text;
            string AmazonSecretAccessKeyIDValue = AmazonSecretAccessKeyID.Text;
            string Description = CategoryDescription.Text;
            double pricePerImage = 0;
            double price = 0;
            string jobGuid = NewJobGUID.Text;
            string AmazonTaskTitle = AmazonTaskTitleTextBox.Text;
            string AmazonTaskDescription = AmazonTaskDescriptionTextBox.Text;
            string AmazonTaskKeywords = AmazonTaskKeywordsTextBox.Text;

            if (AzureBlobStorageConnectionString == "" || AzureContainer == "")
            {
                Template_ErrorMessageLabel.Text = "Error : Azure Connection String and Container are mandatory fields.";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }

            AzureConnectionInfo connectionInfo = new AzureConnectionInfo(AzureBlobStorageConnectionString, AzureContainer, AzureContainerDirectory);
            int noFiles = connectionInfo.getNoFiles();
            if (noFiles == -1)
            {
                Template_ErrorMessageLabel.Text = "Error :  Invalid Azure Location";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }
            else if (noFiles == 0)
            {
                Template_ErrorMessageLabel.Text = "Error :  There are 0 files at the Azure Location";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }

            if (categories.Count == 0)
            {
                Template_ErrorMessageLabel.Text = "Error :  There are no Categories";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }


            if (Hidden_MechanicalTurk.Value == "true")
            {
                bool success = Double.TryParse(PriceTextBox.Text, out pricePerImage);
                if (!success)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Non-numerical price entered";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
                pricePerImage = pricePerImage / 100; //convert to dollars
                price = pricePerImage * TaskConstants.SINGLE_OBJECT_LABLING_IN_VIDEO_MTURK_MAX_VIDEOS_PER_TASK;

                //need to round to cents
                price = Math.Floor(price * 100) / 100;

                if (price <= 0)
                {
                    Template_ErrorMessageLabel.Text = "Error :  The Price is Zero";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
                AmazonMTurkHIT hit = new AmazonMTurkHIT();
                success = hit.setAccount(AmazonAccessKeyIDValue, AmazonSecretAccessKeyIDValue, false);
                if (!success)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Invalid Amazon Turk Account";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
                double balance = hit.getAccountBalance();
                if (balance < 0)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Invalid Amazon Turk Account";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
                double moneyNeeded = 4 * price * noFiles / TaskConstants.SINGLE_OBJECT_LABLING_MTURK_MAX_IMAGES_PER_TASK;
                if (balance < moneyNeeded)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Insufficient money in Amazon Turk Account. You will need at least " + moneyNeeded + "$.";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }


                if (AmazonTaskTitle == "")
                {
                    Template_ErrorMessageLabel.Text = "Video Classification Task";
                }

                if (AmazonTaskTitle == "")
                {
                    Template_ErrorMessageLabel.Text = "Quickly earn money by simply selecting what a video looks like!";
                }



                if (AmazonTaskKeywords == "Videos, Classify, Categorize")
                {
                    Template_ErrorMessageLabel.Text = "";
                }
            }



            AzureInformation AzureInfo = new AzureInformation();
            AzureInfo.AzureBlobStorageConnectionString = AzureBlobStorageConnectionString;
            AzureInfo.AzureBlobStorageContainerName = AzureContainer;
            AzureInfo.AzureBlobStorageContainerDirectoryName = AzureContainerDirectory;

            AmazonMTurkHITInformation AmazonInfo = new AmazonMTurkHITInformation();
            if (Hidden_MechanicalTurk.Value == "true")
            {
                AmazonInfo.AmazonAccessKeyID = AmazonAccessKeyIDValue;
                AmazonInfo.AmazonSecretAccessKeyID = AmazonSecretAccessKeyIDValue;
                AmazonInfo.Price = price; //in dollars
                AmazonInfo.AmazonMTurkTaskTitle = AmazonTaskTitle;
                AmazonInfo.AmazonMTurkTaskDescription = AmazonTaskDescription;
                AmazonInfo.AmazonMTurkTaskKeywords = AmazonTaskKeywords;
            }
            else
            {
                AmazonInfo.AmazonAccessKeyID = "";
                AmazonInfo.AmazonSecretAccessKeyID = "";
                AmazonInfo.Price = 0;
                AmazonInfo.AmazonMTurkTaskTitle = "";
                AmazonInfo.AmazonMTurkTaskDescription = "";
                AmazonInfo.AmazonMTurkTaskKeywords = "";
            }

            SingleObjectLabelingSubmittedJob jobParameters = new SingleObjectLabelingSubmittedJob();
            jobParameters.Categories = categories;
            jobParameters.Description = Description;

            SatyamJob job = new SatyamJob();
            job.UserID = userName;
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobGUIDString = jobGuid;
            if (Hidden_MechanicalTurk.Value == "true")
            {
                job.JobTemplateType = TaskConstants.Classification_Video_MTurk;
                job.TasksPerJob = TaskConstants.SINGLE_OBJECT_LABLING_IN_VIDEO_MTURK_MAX_VIDEOS_PER_TASK;
            }
            else
            {
                job.JobTemplateType = TaskConstants.Classification_Video;
                job.TasksPerJob = 0; //does not matter

            }
            job.JobSubmitTime = DateTime.Now;
            job.JobParameters = JSonUtils.ConvertObjectToJSon<SingleObjectLabelingSubmittedJob>(jobParameters);
            string jobDefinition = JSonUtils.ConvertObjectToJSon<SatyamJob>(job);

            SatyamJobSubmissionsTableAccess dbAccess = new SatyamJobSubmissionsTableAccess();
            dbAccess.AddEntry(job.JobTemplateType, job.UserID, job.JobGUIDString, jobDefinition, job.JobSubmitTime);
            dbAccess.close();
            Response.Redirect("NewJobSubmitted.aspx");
        }

        protected void UseAmazonMTurkCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (UseAmazonMTurkCheckBox.Checked == true)
            {
                AmazonInformationPanel.Visible = true;
                PricePanel.Visible = true;
                TemplateInfo.Visible = false;
                TemplateInfoMturk.Visible = true;
                Hidden_MechanicalTurk.Value = "true";
            }
            else
            {
                AmazonInformationPanel.Visible = false;
                PricePanel.Visible = false;
                TemplateInfo.Visible = true;
                TemplateInfoMturk.Visible = false;
                Hidden_MechanicalTurk.Value = "false";
            }
        }
    }
}