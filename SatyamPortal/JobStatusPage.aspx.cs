using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Constants;
using SatyamAnalysis;
using SQLTables;

namespace SatyamPortal
{
    public partial class JobStatusPage : System.Web.UI.Page
    {
        //temporary 
        const string UserId = TaskConstants.AdminName;
        //const string UserId = "Krishna";


        protected void Page_Load(object sender, EventArgs e)
        {
            populateRows();
        }

        public class TableRow
        {
            public string JobGUID { get; set; }
            public string JobTemplateType { get; set; }
            public string JobSubmitTime { get; set; }
            public string JobStatus { get; set; }
            public string JobProgress { get; set; }
            public string TaskPending { get; set; }
            public string TotalResults { get; set; }
            public string TotalAggregated { get; set; }
            public string ApprovalRate { get; set; }
        }


        private void populateRows()
        {
            SatyamJobSubmissionsTableAccess dbaccess = new SatyamJobSubmissionsTableAccess();
            List<SatyamJobSubmissionsTableAccessEntry> entries = dbaccess.getAllEntriesByUserID(UserId);
            dbaccess.close();
            List<TableRow> rows = new List<TableRow>();
            int maxShow = 10;

            for (int i = entries.Count - 1; i >= Math.Max(0, entries.Count - 1-maxShow); i--)
            {
                TableRow row = new TableRow();
                row.JobGUID = entries[i].JobGUID;
                row.JobTemplateType = entries[i].JobTemplateType;
                row.JobSubmitTime = entries[i].JobSubmitTime.ToLocalTime().ToString();
                row.JobStatus = entries[i].JobStatus;
                SatyamTaskTableAccess taskDB = new SatyamTaskTableAccess();
                int remainingTasks = taskDB.getTasksCountByGUID(row.JobGUID);
                taskDB.close();

                row.TaskPending = remainingTasks.ToString();
                if (entries[i].JobProgress == "")
                {
                    row.JobProgress = "1";
                }
                else
                {
                    row.JobProgress = (1-(double)remainingTasks / Convert.ToDouble(entries[i].JobProgress)).ToString("0.000");
                }


                int totalResults, totalAggregated;
                row.ApprovalRate = SatyamResultsAnalysis.getPaymentRateByGUID(row.JobGUID, out totalResults, out totalAggregated).ToString("0.000");
                row.TotalResults = totalResults.ToString();
                row.TotalAggregated = totalAggregated.ToString();

                rows.Add(row);
            }

            rpt.DataSource = rows;
            rpt.DataBind();

        }

        protected void JobStatusRefreshButton_Click(object sender, EventArgs e)
        {
            populateRows();
        }
    }
}