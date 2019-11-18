using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using Constants;


/*
 CREATE TABLE [dbo].[SatyamJobSubmissionsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserID] VARCHAR(200) NOT NULL, 
    [JobGUID] VARCHAR(200) NOT NULL, 
    [JobTemplateType] VARCHAR(100) NOT NULL, 
    [JobParametersString] VARCHAR(MAX) NOT NULL, 
    [JobSubmitTime] DATETIME2 NOT NULL, 
    [JobStatus] VARCHAR(100) NOT NULL,
    [JobProgress] VARCHAR(100) NOT NULL,
)
 */
namespace SQLTables
{

    public class SatyamJobSubmissionsTableAccessEntry
    {
        public int ID;
        public string JobTemplateType;
        public string UserID;
        public string JobGUID;
        public string JobParametersString;
        public DateTime JobSubmitTime;
        public string JobStatus;
        public string JobProgress;

        public SatyamJobSubmissionsTableAccessEntry()
        {

        }

        public SatyamJobSubmissionsTableAccessEntry(int _ID, String _JobType, String _UserID, String _JoBGUID, String _JsonString, DateTime _SubmitTime)
        {
            ID = _ID;
            JobTemplateType = _JobType;
            UserID = _UserID;
            JobGUID = _JoBGUID;
            JobParametersString = _JsonString;
            JobSubmitTime = _SubmitTime;
            JobStatus = Constants.JobStatus.submitted;
            JobProgress = "";
        }

        public SatyamJobSubmissionsTableAccessEntry(int _ID, String _JobType, String _UserID, String _JoBGUID, String _JsonString, DateTime _SubmitTime, string _JobStatus, string _JobProgress)
        {
            ID = _ID;
            JobTemplateType = _JobType;
            UserID = _UserID;
            JobGUID = _JoBGUID;
            JobParametersString = _JsonString;
            JobSubmitTime = _SubmitTime;
            JobStatus = _JobStatus;
            JobProgress = _JobProgress;
        }

    }

    public class SatyamJobSubmissionsTableAccess
    {
        string TableName = "SatyamJobSubmissionsTable";
        SatyamAzureSQLDBAccess dbAccess;

        public SatyamJobSubmissionsTableAccess()
        {
            dbAccess = new SatyamAzureSQLDBAccess();
        }

        public void close()
        {
            dbAccess.close();
        }

        public List<SatyamJobSubmissionsTableAccessEntry> getEntries(string SQLCommandString)
        {
            List<SatyamJobSubmissionsTableAccessEntry> ret = new List<SatyamJobSubmissionsTableAccessEntry>();
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            try {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int ID = (int)reader["ID"];
                    string UserID = (string)reader["UserID"];
                    string JobGUID = (string)reader["JobGUID"];
                    string JobTemplateType = (string)reader["JobTemplateType"];
                    string JSonString = (string)reader["JobParametersString"];
                    DateTime SubmitTime = (DateTime)reader["JobSubmitTime"];
                    string Status = (string)reader["JobStatus"];
                    string Progress = (string)reader["JobProgress"];
                    SatyamJobSubmissionsTableAccessEntry entry = new SatyamJobSubmissionsTableAccessEntry(ID, JobTemplateType, UserID, JobGUID, JSonString, SubmitTime,Status,Progress);
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

        public bool AddEntry(String JobTemplateType, String UserID, String JobGUID, String JobParametersString, DateTime JobSubmitTime)
        {
            int noTries = 0;
            bool ret = true;
            bool done = true;
            do
            {
                String SQLCommandString = "INSERT INTO " + TableName + " (JobTemplateType,UserID, JobGUID, JobParametersString,JobSubmitTime,JobStatus,JobProgress) VALUES(@JobTemplateType,@UserID, @JobGUID, @JobParametersString,@JobSubmitTime,@JobStatus,@JobProgress)";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", JobTemplateType);
                sqlCommand.Parameters.AddWithValue("@UserID", UserID);
                sqlCommand.Parameters.AddWithValue("@JobGUID", JobGUID);
                sqlCommand.Parameters.AddWithValue("@JobParametersString", JobParametersString);
                sqlCommand.Parameters.AddWithValue("@JobSubmitTime", JobSubmitTime.ToString());
                sqlCommand.Parameters.AddWithValue("@JobStatus", JobStatus.submitted);
                sqlCommand.Parameters.AddWithValue("@JobProgress", "");
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
            }
            return true;
        }

        /****** Deletes **************************************/
       public bool DeleteEntry(string jobGUID)
        {
            String SQLCommandString = "DELETE FROM " + TableName + " WHERE JobGUID = '" + jobGUID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool Clear()
        {
            String SQLCommandString = "DELETE FROM " + TableName;
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool UpdateEntryStatus(string GUID, string Status)
        {
            SatyamJobSubmissionsTableAccessEntry entry = getEntryByJobGIUD(GUID);
            if(entry!=null)
            {
                String SQLCommandString = "UPDATE " + TableName + " SET JobStatus = '" + Status + "' WHERE  JobGUID = '" + GUID + "'";
                return ExecuteNoQuerySQLCommand(SQLCommandString);
            }
            return true;
        }

        public bool UpdateEntryProgress(string GUID, string Progress)
        {
            SatyamJobSubmissionsTableAccessEntry entry = getEntryByJobGIUD(GUID);
            if (entry != null)
            {
                String SQLCommandString = "UPDATE " + TableName + " SET JobProgress = '" + Progress + "' WHERE  JobGUID = '" + GUID + "'";
                return ExecuteNoQuerySQLCommand(SQLCommandString);
            }
            return true;
        }
        public List<SatyamJobSubmissionsTableAccessEntry> getAllEntriesByUserID(string userID)
        {
            String SQLCommandString = "SELECT * FROM " + TableName + " WHERE UserID = '" + userID + "'";
            return getEntries(SQLCommandString);
        }

        public List<SatyamJobSubmissionsTableAccessEntry> getAllEntriesByStatus(string Status)
        {
            String SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobStatus = '" + Status + "'";
            return getEntries(SQLCommandString);
        }

        public SatyamJobSubmissionsTableAccessEntry getEntryByJobGIUD(string JobGUID)
        {
            String SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + JobGUID + "'";
            List<SatyamJobSubmissionsTableAccessEntry> entries = getEntries(SQLCommandString);
            if(entries.Count > 0)
            {
                return entries[0];
            }
            return null;
        }

        public List<string> getGUIDListFromSQLCommand(string SQLCommandString)
        {
            List<string> IDList = new List<string>();
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    string guid = (string)reader["JobGUID"];
                    IDList.Add(guid);
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

        public List<string> getAllJobGUIDs()
        {            
            String SQLCommandString = "SELECT JobGUID FROM " + TableName;           
            return getGUIDListFromSQLCommand(SQLCommandString);
        }

        public List<string> getAllJobGUIDSByStatus(string Status)
        {
            String SQLCommandString = "SELECT JobGUID FROM " + TableName + " WHERE JobStatus = '" + Status + "'";
            return getGUIDListFromSQLCommand(SQLCommandString);
        }
        
    }

}
