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
    public partial class Template_MultipleObjectsTracking : System.Web.UI.Page
    {
        //temporary to be removed
        string userName = TaskConstants.AdminName;

        //permanent
        Dictionary<string, List<string>> subCategories;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //first update all the masterpage related entities:
                ContentPlaceHolder mpContentPlaceHolder = (ContentPlaceHolder)this.Master.Master.FindControl("MainTemplatePlaceHolder");
                DropDownList ddlist1 = (DropDownList)mpContentPlaceHolder.FindControl("TemplatesDropDownList");
                ddlist1.Items.FindByText("Multi-Object Tracking").Selected = true;

                Label usernameLabel = (Label)this.Master.Master.FindControl("UsernameLabel");
                usernameLabel.Text = userName;

                //Create a new Job GUID
                NewJobGUID.Text = Guid.NewGuid().ToString();


            }
            else
            {
                string jsonString = Hidden_SubCategories.Value;
                if (jsonString != "")
                {
                    subCategories = JSonUtils.ConvertJSonToObject<Dictionary<String, List<String>>>(jsonString);
                }
                else
                {
                    subCategories = new Dictionary<string, List<string>>();
                }
            }
        }

        protected void AddCategoryButton_Click(object sender, EventArgs e)
        {
            string newCategory = AddCategoryTextBox.Text;
            if (newCategory == "")
            {
                return;
            }
            else if (subCategories.Keys.ToList().Contains(newCategory))
            {
                return;
            }
            ListItem item = new ListItem(newCategory);
            CategoryListBox.Items.Add(item);
            AddCategoryTextBox.Text = "";
            subCategories.Add(newCategory, new List<string>());
            Hidden_SubCategories.Value = JSonUtils.ConvertObjectToJSon<Dictionary<String, List<String>>>(subCategories);
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
                subCategories.Remove(categoriesToBeDeleted[i].Text);
                CategoryListBox.Items.Remove(categoriesToBeDeleted[i]);
            }
            //SubCategoryListBox.Items.Clear();
            //Hidden_SubCategories.Value = JSonUtils.ConvertObjectToJSon<Dictionary<String, List<String>>>(subCategories);
        }

        //protected void CategoryListBox_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    SubCategoryListBox.Items.Clear();
        //    string selectedItem = CategoryListBox.SelectedValue;
        //    List<string> items = subCategories[selectedItem];
        //    for (int i = 0; i < items.Count; i++)
        //    {
        //        ListItem item = new ListItem(items[i]);
        //        SubCategoryListBox.Items.Add(item);
        //    }
        //}

        //protected void AddSubCategoryButton_Click(object sender, EventArgs e)
        //{
        //    string category = CategoryListBox.SelectedValue;
        //    string newSubCategory = AddSubCategoryTextBox.Text;
        //    if (newSubCategory == "")
        //    {
        //        return;
        //    }
        //    else if (subCategories[category].Contains(newSubCategory))
        //    {
        //        return;
        //    }
        //    ListItem item = new ListItem(newSubCategory);
        //    SubCategoryListBox.Items.Add(item);
        //    AddSubCategoryTextBox.Text = "";
        //    subCategories[category].Add(newSubCategory);
        //    Hidden_SubCategories.Value = JSonUtils.ConvertObjectToJSon<Dictionary<String, List<String>>>(subCategories);
        //}

        //protected void DeleteSubCategoryButton_Click(object sender, EventArgs e)
        //{
        //    string category = CategoryListBox.SelectedValue;
        //    List<ListItem> subcategoriesToBeDeleted = new List<ListItem>();
        //    for (int i = 0; i < SubCategoryListBox.Items.Count; i++)
        //    {
        //        if (SubCategoryListBox.Items[i].Selected)
        //        {
        //            subcategoriesToBeDeleted.Add(SubCategoryListBox.Items[i]);
        //        }
        //    }
        //    for (int i = 0; i < subcategoriesToBeDeleted.Count; i++)
        //    {
        //        subCategories[category].Remove(subcategoriesToBeDeleted[i].Text);
        //        SubCategoryListBox.Items.Remove(subcategoriesToBeDeleted[i]);
        //    }
        //    Hidden_SubCategories.Value = JSonUtils.ConvertObjectToJSon<Dictionary<String, List<String>>>(subCategories);
        //}

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

            double pricePerObjectPerChunk = 0;
            int noObjectsPerChunk = 0;
            string FrameRate = TargetFrameRate.Text;
            string chunkLength = ChunkDuration.Text;
            string chunkOverlap = ChunkOverlap.Text;


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

            if (subCategories.Count == 0)
            {
                Template_ErrorMessageLabel.Text = "Error :  There are no Categories";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }

            List<LineSegment> BoundaryLines = new List<LineSegment>();
            if (BoundaryString.Text != "")
            {
                bool error = false;
                string errorMessage = "";
                string[] boundaryFields = BoundaryString.Text.Split(',');
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

                bool success = Double.TryParse(PriceTextBox.Text, out pricePerObjectPerChunk);
                if (!success)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Non-numerical price entered";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
                pricePerObjectPerChunk = pricePerObjectPerChunk / 100; //covert to dollars
                success = Int32.TryParse(NoObjectsPerChunkTextBox.Text, out noObjectsPerChunk);
                if (!success)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Non-numerical number of objects per image entered";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }

                if (pricePerObjectPerChunk <= 0.0)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Illegal Price";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
                if (noObjectsPerChunk <= 0.0)
                {
                    Template_ErrorMessageLabel.Text = "Error :  Illegal number of objects";
                    Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                    Template_ErrorMessageLabel.Font.Bold = true;
                    return;
                }
            }

            if (!InputFormatImage.Checked && !InputFormatVideo.Checked)
            {
                Template_ErrorMessageLabel.Text = "Error :  Please select input format";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }

            if (Convert.ToDouble(FrameRate) <= 0)
            {
                Template_ErrorMessageLabel.Text = "Error :  Invalid Target Frame Rate";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }

            if(Convert.ToInt32(chunkLength) <= 0)
            {
                Template_ErrorMessageLabel.Text = "Error :  Invalid Chunk Duration";
                Template_ErrorMessageLabel.ForeColor = System.Drawing.Color.Red;
                Template_ErrorMessageLabel.Font.Bold = true;
                return;
            }
            if (Convert.ToDouble(chunkOverlap) <= 0 || Convert.ToDouble(chunkOverlap) > Convert.ToInt32(chunkLength) / 2)
            {
                Template_ErrorMessageLabel.Text = "Error :  Invalid Chunk Overlap, must be in (0,Chunk/2]";
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
                AmazonInfo.Price = Math.Floor(pricePerObjectPerChunk * (double)noObjectsPerChunk * 100) / 100;
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

            MultiObjectTrackingSubmittedJob jobParameters = new MultiObjectTrackingSubmittedJob();
            jobParameters.Categories = subCategories;
            jobParameters.Description = Description;
            jobParameters.BoundaryLines = BoundaryLines;

            jobParameters.FrameRate = Convert.ToInt32(FrameRate);
            jobParameters.ChunkDuration = Convert.ToInt32(chunkLength);
            jobParameters.ChunkOverlap = Convert.ToDouble(chunkOverlap);


            if (InputFormatVideo.Checked)
            {
                jobParameters.DataSrcFormat = Constants.DataFormat.Video;
            }else if (InputFormatImage.Checked)
            {
                jobParameters.DataSrcFormat = Constants.DataFormat.VideoFrame;
            }


            SatyamJob job = new SatyamJob();
            job.UserID = userName;
            job.amazonHITInformation = AmazonInfo;
            job.azureInformation = AzureInfo;
            job.JobGUIDString = jobGuid;
            if (Hidden_MechanicalTurk.Value == "true")
            {
                job.JobTemplateType = TaskConstants.Tracking_MTurk;
                job.TasksPerJob = 1;
            }
            else
            {
                job.JobTemplateType = TaskConstants.Tracking;
                job.TasksPerJob = 0;
            }
            job.JobSubmitTime = DateTime.Now;
            job.JobParameters = JSonUtils.ConvertObjectToJSon<MultiObjectTrackingSubmittedJob>(jobParameters);
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
                Hidden_MechanicalTurk.Value = "true";
            }
            else
            {
                AmazonInformationPanel.Visible = false;
                PricePanel.Visible = true;
                Hidden_MechanicalTurk.Value = "false";
            }
        }
    }
}