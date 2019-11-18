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
    public partial class Template_ObjectCounting : System.Web.UI.Page
    {
        //temporary to be removed
        string userName = TaskConstants.AdminName;

        //permanent
        string objectName = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //first update all the masterpage related entities:
                ContentPlaceHolder mpContentPlaceHolder = (ContentPlaceHolder)this.Master.Master.FindControl("MainTemplatePlaceHolder");
                DropDownList ddlist1 = (DropDownList)mpContentPlaceHolder.FindControl("TemplatesDropDownList");
                ddlist1.Items.FindByText("Object Counting In Image").Selected = true;

                Label usernameLabel = (Label)this.Master.Master.FindControl("UsernameLabel");
                usernameLabel.Text = userName;

                //Create a new Job GUID
                NewJobGUID.Text = Guid.NewGuid().ToString();

            }

            if (IsPostBack)
            {
                objectName = ObjectNameTextBox.Text;
            }
        }

        protected void Object_Counting_JobSubmitButton_Click(object sender, EventArgs e)
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
                price = pricePerImage * TaskConstants.OBJECT_COUNTING_MTURK_MAX_IMAGES_PER_TASK;

                //need to round to cents
                price = Math.Floor(price * 100) / 100;

                if (price <= 0.0)
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
                double moneyNeeded = 4 * price * noFiles / TaskConstants.OBJECT_COUNTING_MTURK_MAX_IMAGES_PER_TASK;

                if (balance < moneyNeeded)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Insufficient money in Amazon Turk Account. You will need at least " + moneyNeeded + "$.";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }

                if (AmazonTaskTitle == "")
                {
                    Template_ErrorMessageLabel.Text = "Counting Objects in an Image";
                }

                if (AmazonTaskTitle == "")
                {
                    Template_ErrorMessageLabel.Text = "Earn money quickly by simply counting the number of objects of interest in an image!";
                }

                if (AmazonTaskKeywords == "Object Counting")
                {
                    Template_ErrorMessageLabel.Text = "";
                }

            }

            string objectName = ObjectNameTextBox.Text;
            if (objectName == "")
            {
                Template_ErrorMessageLabel.Text = "Error :  No Object Name Entered";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
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

            ObjectCountingSubmittedJob jobParameters = new ObjectCountingSubmittedJob();
            jobParameters.ObjectName = objectName;
            jobParameters.Description = Description;

            SatyamJob job = new SatyamJob();
            job.UserID = userName;
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobGUIDString = jobGuid;
            if (Hidden_MechanicalTurk.Value == "true")
            {
                job.JobTemplateType = TaskConstants.Counting_Image_MTurk;
                job.TasksPerJob = TaskConstants.OBJECT_COUNTING_MTURK_MAX_IMAGES_PER_TASK;
            }
            else
            {
                job.JobTemplateType = TaskConstants.Counting_Image;
                job.TasksPerJob = 0;
            }
            job.JobSubmitTime = DateTime.Now;
            job.JobParameters = JSonUtils.ConvertObjectToJSon<ObjectCountingSubmittedJob>(jobParameters);

            if (Hidden_AdaptivePricing.Value == "true")
            {
                job.AdaptivePricing = true;
                bool success = Double.TryParse(TargetPricePerHourTextBox.Text, out job.TargetPricePerTask);
                if (!success) job.TargetPricePerTask = 13.56; // default for now
                success = Double.TryParse(BudgetTextBox.Text, out job.TotalBudget);
                if (!success) job.TotalBudget = -1;//default
            }
            else
            {
                job.AdaptivePricing = false;
                job.TargetPricePerTask = -1;
                job.TotalBudget = -1;
            }

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

        protected void AdaptivePricingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (AdaptivePricingCheckBox.Checked == true)
            {
                AdaptivePricePanel.Visible = true;
                FixedPricePanel.Visible = false;
                Hidden_AdaptivePricing.Value = "true";
            }
            else
            {
                AdaptivePricePanel.Visible = false;
                FixedPricePanel.Visible = true;
                Hidden_AdaptivePricing.Value = "false";
            }
        }
    }
}