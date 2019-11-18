using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AmazonMechanicalTurkAPI
{
    public static class AmazonMTurkNotification
    {
        public static string amazonSandboxNotificationURL = "https://workersandbox.mturk.com/mturk/externalSubmit";
        public static string amazonNotificationURL = "https://www.mturk.com/mturk/externalSubmit";


        public static void submitAmazonTurkHit(string assignmentID, string workerID, bool sandBox)
        {
            string notificationURL = amazonNotificationURL;
            if (sandBox)
            {
                notificationURL = amazonSandboxNotificationURL;
            }
            string formId = "amazonNotificationForm";
            StringBuilder htmlForm = new StringBuilder();
            htmlForm.AppendLine("<html>");
            htmlForm.AppendLine(String.Format("<body onload='document.forms[\"{0}\"].submit()'>", formId));
            htmlForm.AppendLine(String.Format("<form id='{0}' method='POST' action='{1}'>", formId, notificationURL));
            htmlForm.AppendLine(String.Format("<input type='hidden' id='assignmentId' name='assignmentId' value='{0}' />", assignmentID));
            htmlForm.AppendLine(String.Format("<input type='hidden' id='workerId' name='workerId' value='{0}' />", workerID));
            htmlForm.AppendLine("</form>");
            htmlForm.AppendLine("</body>");
            htmlForm.AppendLine("</html>");

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(htmlForm.ToString());
            HttpContext.Current.Response.End();
        }


        public static void returnAmazonTurkHit(string assignmentID, string workerID, HttpContext httpContext, bool sandBox)
        {
            string notificationURL = amazonNotificationURL;
            if (sandBox)
            {
                notificationURL = amazonSandboxNotificationURL;
            }

            string formId = "amazonNotificationForm";
            StringBuilder htmlForm = new StringBuilder();
            htmlForm.AppendLine("<html>");
            htmlForm.AppendLine(String.Format("<body onload='document.forms[\"{0}\"].submit()'>", formId));
            htmlForm.AppendLine(String.Format("<form id='{0}' method='POST' action='{1}'>", formId, notificationURL));
            htmlForm.AppendLine(String.Format("<input type='hidden' id='assignmentId' name='assignmentId' value='{0}' />", assignmentID));
            htmlForm.AppendLine(String.Format("<input type='hidden' id='workerId' name='workerId' value='{0}' />", workerID));
            htmlForm.AppendLine("</form>");
            htmlForm.AppendLine("</body>");
            htmlForm.AppendLine("</html>");

            HttpContext.Current.Response.Clear();
            HttpContext.Current.Response.Write(htmlForm.ToString());
            HttpContext.Current.Response.End();
        }
    }
}
