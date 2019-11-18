using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SatyamPortal
{
    public partial class JobTemplate : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void TemplatesDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (TemplatesDropDownList.SelectedIndex)
            {
                case 0:
                    Response.Redirect("Template_SingleObjectLabeling.aspx", true);
                    break;
                case 1:
                    Response.Redirect("Template_SingleObjectLabelingInVideo.aspx", true);
                    break;
                case 2:
                    Response.Redirect("Template_ObjectCounting.aspx", true);
                    break;
                case 3:
                    Response.Redirect("Template_ObjectCountingInVideo.aspx", true);
                    break;
                case 4:
                    Response.Redirect("Template_MultipleObjectsLocalizationAndLabeling.aspx", true);
                    break;
                case 5:
                    Response.Redirect("Template_MultipleObjectsTracking.aspx", true);
                    break;
                case 6:
                    Response.Redirect("Template_TrackletLabeling.aspx", true);
                    break;
                case 7:
                    Response.Redirect("Template_ImageSegmentation.aspx", true);
                    break;
                case 8:
                    Response.Redirect("Template_OpenEndedQuestionsImage.aspx", true);
                    break;
            }
        }
    }
}