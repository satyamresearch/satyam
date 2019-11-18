using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using Utilities;
using SatyamTaskResultClasses;
using Constants;

namespace SQLTables
{

    /*
    CREATE TABLE [dbo].[SatyamResultsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[UserID] VARCHAR(100) NOT NULL,
	[JobGUID] VARCHAR(100) NOT NULL, 
    [JobTemplateType] VARCHAR(100) NOT NULL, 
    [ResultString] VARCHAR(MAX) NOT NULL, 
    [SatyamTaskTableEntryID] INT NOT NULL, 
    [PageLoadTime] DATETIME2 NOT NULL, 
    [SubmitTime] DATETIME2 NOT NULL,
    [Status] VARCHAR(100) NOT NULL
) 
   */

    public class SatyamResultsTableEntry
    {
        public int ID;
        public string JobTemplateType;
        public string UserID;
        public string JobGUID;
        public string ResultString;
        public int SatyamTaskTableEntryID;
        public DateTime PageLoadTime;
        public DateTime SubmitTime;
        public string Status; // "incconclusive","accepted/rejected","paid/rejected" for mturk 

        public SatyamResultsTableEntry()
        {

        }

        public SatyamResultsTableEntry(int _ID, String _JobType, String _UserID, String _JoBGUID, String _JsonString, int _SatyamTaskTableEntryID, DateTime _PageLoadTime, DateTime _SubmitTime, string _Status)
        {
            ID = _ID;
            JobTemplateType = _JobType;
            UserID = _UserID;
            JobGUID = _JoBGUID;
            ResultString = _JsonString;
            SatyamTaskTableEntryID = _SatyamTaskTableEntryID;
            PageLoadTime = _PageLoadTime;
            SubmitTime = _SubmitTime;
            Status = _Status;
        }

       
    }
    public class SatyamResultsTableAccess
    {
        string TableName = "SatyamResultsTable";
        SatyamAzureSQLDBAccess dbAccess;

        public SatyamResultsTableAccess()
        {
            dbAccess = new SatyamAzureSQLDBAccess();
        }

        public void close()
        {
            dbAccess.close();
        }

        public List<SatyamResultsTableEntry> getEntries(string SQLCommandString)
        {
            List<SatyamResultsTableEntry> ret = new List<SatyamResultsTableEntry>();
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int ID = (int)reader["ID"];
                    string UserID = (string)reader["UserID"];
                    string JobGUID = (string)reader["JobGUID"];
                    string JobTemplateType = (string)reader["JobTemplateType"];
                    string JSonString = (string)reader["ResultString"];
                    int SatyamTaskTableEntryID = (int)reader["SatyamTaskTableEntryID"];
                    DateTime PageLoadTime = (DateTime)reader["PageLoadTime"];
                    DateTime SubmitTime = (DateTime)reader["SubmitTime"];
                    string Status = (string)reader["Status"];
                    SatyamResultsTableEntry entry = new SatyamResultsTableEntry(ID, JobTemplateType, UserID, JobGUID, JSonString, SatyamTaskTableEntryID,PageLoadTime,SubmitTime,Status);
                    ret.Add(entry);
                }
                reader.Close();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
            }
            return ret;
        }

        public SatyamResultsTableEntry getEntry(string SQLCommandString)
        {
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            if (entries.Count > 0)
            {
                return entries[0];
            }
            else
            {
                return null;
            }
        }

        public List<SatyamResultsTableEntry> getEntriesID(string ID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE ID = '" + ID + "'";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getEntriesByGUID(string GUID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + GUID + "'";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getEntriesByGUIDAndTaskID(string GUID, int taskID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + GUID + "' AND SatyamTaskTableEntryID = '" + taskID.ToString() + "' ORDER BY ID";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getEntriesByTaskID(int taskID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE SatyamTaskTableEntryID = '" + taskID.ToString() + "'";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getEntriesByGUIDOrderByID(string GUID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + GUID + "' order by id";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        

        public List<SatyamResultsTableEntry> getEntriesByStatus(string status)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE Status = '" + status + "'";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getEntriesByStatus()
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE Status = 'inconclusive'";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getAllEntries()
        {
            string SQLCommandString = "SELECT * FROM " + TableName;
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getAllEntriesByJobtemplateType(string jobTemplateType)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobTemplateType = '" + jobTemplateType + "'";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getAllEntriesByJobtemplateTypeAndStatus(string jobTemplateType, string status)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE Status = '" + status + "' AND JobTemplateType = '" + jobTemplateType + "'";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamResultsTableEntry> getAllEntriesByStatus(string status)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE Status = '" + status + "'";
            List<SatyamResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        /***** Adding Entries ********************************************************************************************/

        public bool AddEntry(String JobTemplateType, String UserID, String JobGUID, String JsonString, int SatyamTaskTableEntryID, DateTime PageLoadTime, DateTime SubmitTime)
        {
            int noTries = 0;
            bool ret = true;
            bool done = true;
            do
            {
                String SQLCommandString = "INSERT INTO " + TableName + " (JobTemplateType,UserID, JobGUID, ResultString,SatyamTaskTableEntryID,PageLoadTime,SubmitTime,Status) VALUES(@JobTemplateType,@UserID, @JobGUID, @ResultString,@SatyamTaskTableEntryID,@PageLoadTime,@SubmitTime,@Status)";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", JobTemplateType);
                sqlCommand.Parameters.AddWithValue("@UserId", UserID);
                sqlCommand.Parameters.AddWithValue("@JobGUID", JobGUID);
                sqlCommand.Parameters.AddWithValue("@ResultString", JsonString);
                sqlCommand.Parameters.AddWithValue("@SatyamTaskTableEntryID", SatyamTaskTableEntryID.ToString());
                sqlCommand.Parameters.AddWithValue("@PageLoadTime", PageLoadTime.ToString());
                sqlCommand.Parameters.AddWithValue("@SubmitTime", SubmitTime.ToString());
                sqlCommand.Parameters.AddWithValue("@Status", ResultStatus.inconclusive);
                try
                {
                    sqlCommand.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (ex.Message.Contains("Timeout"))
                    {
                        done = false;
                        noTries++;
                        if (noTries > 3)
                        {
                            done = true;
                            ret = false;
                        }
                    }
                    Console.Error.WriteLine(ex.ToString());
                    throw ex;
                }
            } while (!done);
            return ret;
        }

        public void AddEntry(SatyamResultsTableEntry entry)
        {
            AddEntry(entry.JobTemplateType, entry.UserID, entry.JobGUID, entry.ResultString, entry.SatyamTaskTableEntryID,entry.PageLoadTime,entry.SubmitTime);
        }

        public void AddEntries(List<SatyamResultsTableEntry> entries)
        {
            foreach (SatyamResultsTableEntry entry in entries)
            {
                AddEntry(entry);
            }
        }

        /***** Generic Command ********************************************************************************************/


        public bool ExecuteNoQuerySQLCommand(string SQLCommandString)
        {
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                return false;
                // the queries that reopen a guid might time out, 
                // need to execute manually or set a longer time out.
            }
            return true;
        }

        /****** Deleting Entries ********************************************************************************************/
        private bool DeleteEntry(int id)
        {
            String SQLCommandString = "DELETE FROM " + TableName + " WHERE Id = '" + id + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool Clear()
        {
            String SQLCommandString = "DELETE FROM " + TableName;
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool ClearByJobGUD(string GUID)
        {
            String SQLCommandString = "DELETE FROM " + TableName + " WHERE JobGUID = '" + GUID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool UpdateStatusByTaskID(int TaskID, string status)
        {
            String SQLCommandString = "UPDATE " + TableName + " SET Status = '" + status + "' WHERE SatyamTaskTableEntryID = '" + TaskID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool UpdateStatusByID(int ID, string status)
        {
            String SQLCommandString = "UPDATE " + TableName + " SET Status = '" + status + "' WHERE ID = '" + ID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool UpdateStatusByGUID(string guid, string status)
        {
            String SQLCommandString = "UPDATE " + TableName + " SET Status = '" + status + "' WHERE JobGUID = '" + guid + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }


        /******Getting all IDs*********************************************************************************/


        public List<int> getIDsBySQLCommand(string SQLCommandString)
        {
            List<int> IDList = new List<int>();
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int ID = (int)reader["ID"];
                    IDList.Add(ID);
                }
                reader.Close();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
            }
            return IDList;
        }


        public List<int> getAllIDsByGUID(string JobGUID)
        {

            String SQLCommandString = "SELECT ID FROM " + TableName + " WHERE JobGUID = '" + JobGUID + "'";

            return getIDsBySQLCommand(SQLCommandString);
        }

        public List<int> getAllIDsByJobTemplateTypeAndStatus(string JobTemplateType, string status)
        {

            String SQLCommandString = "SELECT ID FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "' AND Status = '" + status + "'";

            return getIDsBySQLCommand(SQLCommandString);
        }

        public List<int> getAllIDsByJobTemplateByStatus(string status)
        {

            String SQLCommandString = "SELECT ID FROM " + TableName + " WHERE Status = '" + status + "'";

            return getIDsBySQLCommand(SQLCommandString);
        }



        public List<int> getAllTaskIDsAWorkerHasDoneForAGUID(string WorkerID, string guid)
        {
            List<int> ret = new List<int>();

            List<SatyamResultsTableEntry> entries = getEntriesByGUID(guid);

            foreach(SatyamResultsTableEntry entry in entries)
            {
                SatyamResult result = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                if (result.amazonInfo.WorkerID == WorkerID)
                {
                    ret.Add(result.TaskTableEntryID);
                }
            }

            return ret;
        }
    }
}
