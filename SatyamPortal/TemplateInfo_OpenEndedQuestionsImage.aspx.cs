using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SatyamPortal
{
    public partial class TemplateInfo_OpenEndedQuestionsImage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string userName = TaskConstants.AdminName;
            //first update all the masterpage related entities:
            ContentPlaceHolder mpContentPlaceHolder = (ContentPlaceHolder)this.Master.Master.FindControl("MainTemplatePlaceHolder");
            DropDownList ddlist1 = (DropDownList)mpContentPlaceHolder.FindControl("TemplatesDropDownList");
            ddlist1.Items.FindByText("Open-ended Questions for Image").Selected = true;

            Label usernameLabel = (Label)this.Master.Master.FindControl("UsernameLabel");
            usernameLabel.Text = userName;
        }
    }
}