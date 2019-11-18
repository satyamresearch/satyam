using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Text;

using Utilities;
using JobTemplateClasses;
using SQLTables;
using AmazonMechanicalTurkAPI;


namespace SatyamPortal
{
    public partial class WebServiceHelpers : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        [WebMethod]
        public static string getNoFilesInAzureLocation(string request)
        {
            string[] fields = request.Split(',');
            AzureConnectionInfo connectionInfo = new AzureConnectionInfo(fields[0], fields[1], fields[2]);
            int noFiles = connectionInfo.getNoFiles();
            return noFiles.ToString();
        }

        [WebMethod]
        public static string checkMoneyInAmazonAccount(string request)
        {
            string[] fields = request.Split(',');
            AmazonMTurkHIT hit = new AmazonMTurkHIT();
            bool success = hit.setAccount(fields[0], fields[1], false);
            if (!success)
            {                
                return "-1";
            }
            double balance = hit.getAccountBalance();
            if (balance < 0)
            {
                return "-1";
            }
            return balance.ToString();
        }


        [WebMethod]
        public static string getJobStatus(string request)
        {
            SatyamJobSubmissionsTableAccess dbaccess = new SatyamJobSubmissionsTableAccess();
            List<SatyamJobSubmissionsTableAccessEntry> entries = dbaccess.getAllEntriesByUserID(request);
            string JsonString = JSonUtils.ConvertObjectToJSon(entries);
            dbaccess.close();
            return JsonString;
        }

    }
}