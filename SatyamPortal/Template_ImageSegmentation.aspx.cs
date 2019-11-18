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
using HelperClasses;

namespace SatyamPortal
{
    public partial class Template_ImageSegmentation : System.Web.UI.Page
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
                ddlist1.Items.FindByText("Image Segmentation").Selected = true;

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
            }
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
            string jobGuid = NewJobGUID.Text;
            string AmazonTaskTitle = AmazonTaskTitleTextBox.Text;
            string AmazonTaskDescription = AmazonTaskDescriptionTextBox.Text;
            string AmazonTaskKeywords = AmazonTaskKeywordsTextBox.Text;
            double pricePerObject = 0;//default
            int numObjectPerImage = 0;//default
            double price = 0;//default
            int tasksPerHIT = 1;//defuault



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
                AmazonMTurkHIT hit = new AmazonMTurkHIT();
                bool success = hit.setAccount(AmazonAccessKeyIDValue, AmazonSecretAccessKeyIDValue, false);
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
                if (balance < 0.1)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Insufficient money in Amazon Turk Account";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
            }

            if (categories.Count == 0)
            {
                Template_ErrorMessageLabel.Text = "Error :  There are no Categories";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }

            List<LineSegment> BoundaryLines = new List<LineSegment>();
            if (BoundaryTextBox.Text != "")
            {
                bool error = false;
                string errorMessage = "";
                string[] boundaryFields = BoundaryTextBox.Text.Split(',');
                for (int i = 0; i < boundaryFields.Length; i++)
                {
                    string[] coords = boundaryFields[i].Split('-');
                    if (coords.Length != 4)
                    {
                        errorMessage = "In Boundary String, Line No " + (i + 1) + " " + boundaryFields[i] + " : does not have 4 values.";
                        error = true;
                        break;
                    }
                    int x1, y1, x2, y2;
                    bool success = int.TryParse(coords[0], out x1);
                    if (!success)
                    {
                        errorMessage = "In Boundary String, Line No " + (i + 1) + " " + boundaryFields[i] + " : " + coords[0] + " must be an integer";
                        error = true;
                        break;
                    }
                    success = int.TryParse(coords[1], out y1);
                    if (!success)
                    {
                        errorMessage = "In Boundary String, Line No " + (i + 1) + " " + boundaryFields[i] + " : " + coords[1] + " must be an integer";
                        error = true;
                        break;
                    }
                    success = int.TryParse(coords[2], out x2);
                    if (!success)
                    {
                        errorMessage = "In Boundary String, Line No " + (i + 1) + " " + boundaryFields[i] + " : " + coords[2] + " must be an integer";
                        error = true;
                        break;
                    }
                    success = int.TryParse(coords[3], out y2);
                    if (!success)
                    {
                        errorMessage = "In Boundary String, Line No " + (i + 1) + " " + boundaryFields[i] + " : " + coords[3] + " must be an integer";
                        error = true;
                        break;
                    }
                    LineSegment ls = new LineSegment(x1, y1, x2, y2);
                    BoundaryLines.Add(ls);
                }
                if (error)
                {
                    Template_ErrorMessageLabel.Text = errorMessage;
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
            }

            if (Hidden_MechanicalTurk.Value == "true")
            {
                bool success = Double.TryParse(PriceTextBox.Text, out pricePerObject);
                if (!success)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Non-numerical price entered";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
                pricePerObject = pricePerObject / 100; //covert to dollars
                success = Int32.TryParse(NoObjectsPerImageTextBox.Text, out numObjectPerImage);
                if (!success)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Non-numerical number of objects per image entered";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }

                price = pricePerObject * numObjectPerImage;
                if (price <= 0.0)
                {
                    Template_ErrorMessageLabel.Text = "Error :  The Price is Zero";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }

                //need to round to cents
                price = Math.Floor(price * 100) / 100;

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

                double moneyNeeded = 4 * price * noFiles / tasksPerHIT;
                if (balance < moneyNeeded)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Insufficient money in Amazon Turk Account, you will need atleast " + moneyNeeded + "$.";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }

                if (AmazonTaskTitle == "")
                {
                    Template_ErrorMessageLabel.Text = "Image Segmentation";
                }

                if (AmazonTaskTitle == "")
                {
                    Template_ErrorMessageLabel.Text = "Earn money quickly by drawing polygons around objects of interest in an image!";
                }

                if (AmazonTaskKeywords == "Image Segmentation")
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
                AmazonInfo.Price = price;
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

            ImageSegmentationSubmittedJob jobParameters = new ImageSegmentationSubmittedJob();
            jobParameters.Categories = categories;
            jobParameters.Description = Description;
            jobParameters.BoundaryLines = BoundaryLines;

            SatyamJob job = new SatyamJob();
            job.UserID = userName;
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobGUIDString = jobGuid;
            if (Hidden_MechanicalTurk.Value == "true")
            {
                job.JobTemplateType = TaskConstants.Segmentation_Image_MTurk;
                job.TasksPerJob = tasksPerHIT;
            }
            else
            {
                job.JobTemplateType = TaskConstants.Segmentation_Image;
                job.TasksPerJob = 0;
            }
            job.JobSubmitTime = DateTime.Now;
            job.JobParameters = JSonUtils.ConvertObjectToJSon<ImageSegmentationSubmittedJob>(jobParameters);
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

        protected void AddNewCategoryButton_Click(object sender, EventArgs e)
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
    }
}
