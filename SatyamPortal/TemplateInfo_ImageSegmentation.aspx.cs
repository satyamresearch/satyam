using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SatyamPortal
{
    public partial class TemplateInfo_ImageSegmentation : System.Web.UI.Page
    {
        //temporary to be removed
        string userName = TaskConstants.AdminName;
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
            }
        }
    }
}