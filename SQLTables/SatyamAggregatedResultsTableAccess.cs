using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace SQLTables
{
    /*
     CREATE TABLE [dbo].[SatyamAggregatedResultsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[UserID] VARCHAR(200) NOT NULL,
	[JobGUID] VARCHAR(200) NOT NULL, 
    [JobTemplateType] VARCHAR(100) NOT NULL, 
    [ResultString] VARCHAR(MAX) NOT NULL, 
    [SatyamTaskTableEntryID] INT NOT NULL, 
)
     */
    public class SatyamAggregatedResultsTableEntry
    {
        public int ID;
        public string JobTemplateType;
        public string UserID;
        public string JobGUID;
        public string ResultString;
        public int SatyamTaskTableEntryID;

        public SatyamAggregatedResultsTableEntry()
        {

        }

        public SatyamAggregatedResultsTableEntry(int _ID, String _JobType, String _UserID, String _JoBGUID, String _JsonString, int _SatyamTaskTableEntryID)
        {
            ID = _ID;
            JobTemplateType = _JobType;
            UserID = _UserID;
            JobGUID = _JoBGUID;
            ResultString = _JsonString;
            SatyamTaskTableEntryID = _SatyamTaskTableEntryID;
        }
    }

    public class SatyamAggregatedResultsTableAccess
    {
        string TableName = "SatyamAggregatedResultsTable";
        SatyamAzureSQLDBAccess dbAccess;

        public SatyamAggregatedResultsTableAccess()
        {
            dbAccess = new SatyamAzureSQLDBAccess();
        }

        public void close()
        {
            dbAccess.close();
        }

        public List<SatyamAggregatedResultsTableEntry> getEntries(string SQLCommandString)
        {
            List<SatyamAggregatedResultsTableEntry> ret = new List<SatyamAggregatedResultsTableEntry>();
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

                    SatyamAggregatedResultsTableEntry entry = new SatyamAggregatedResultsTableEntry(ID, JobTemplateType, UserID, JobGUID, JSonString, SatyamTaskTableEntryID);
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

        public SatyamAggregatedResultsTableEntry getEntry(string SQLCommandString)
        {
            List<SatyamAggregatedResultsTableEntry> entries = getEntries(SQLCommandString);
            if (entries.Count > 0)
            {
                return entries[0];
            }
            else
            {
                return null;
            }
        }

        public List<SatyamAggregatedResultsTableEntry> getEntriesByGUID(string GUID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + GUID + "'";
            List<SatyamAggregatedResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamAggregatedResultsTableEntry> getEntriesByGUIDOrderByResultsAggregatedDesc(string GUID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + GUID + "' ORDER BY RESULTSAGGREGATED DESC";
            List<SatyamAggregatedResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamAggregatedResultsTableEntry> getEntriesByNOTSaved()
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE Saved = 0";
            List<SatyamAggregatedResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public SatyamAggregatedResultsTableEntry getEntryByTaskID(int taskID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE SatyamTaskTableEntryID = '" + taskID.ToString() + "'";
            SatyamAggregatedResultsTableEntry entry = getEntry(SQLCommandString);
            return entry;
        }

        public List<SatyamAggregatedResultsTableEntry> getEntriesByTaskID(int taskID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE SatyamTaskTableEntryID = '" + taskID.ToString() + "' ORDER BY RESULTSAGGREGATED DESC";
            List<SatyamAggregatedResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public SatyamAggregatedResultsTableEntry getLatestEntryByTaskID(int taskID)
        {
            string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE SatyamTaskTableEntryID = '" + taskID.ToString() + "' ORDER BY RESULTSAGGREGATED DESC";
            SatyamAggregatedResultsTableEntry entries = getEntry(SQLCommandString);
            return entries;
        }

        public int getLatestNoResultsAggregatedByTaskID(int taskID)
        {
            string SQLCommandString = "SELECT TOP 1 RESULTSAGGREGATED FROM " + TableName + " WHERE SatyamTaskTableEntryID = '" + taskID.ToString() + "' ORDER BY RESULTSAGGREGATED DESC";
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            int ResultsAggregated = -1;
            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    ResultsAggregated = (int)reader["ResultsAggregated"];
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
            return ResultsAggregated;
        }

        public List<SatyamAggregatedResultsTableEntry> getAllEntries()
        {
            string SQLCommandString = "SELECT * FROM " + TableName;
            List<SatyamAggregatedResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        /***** Adding Entries ********************************************************************************************/

        public bool AddEntry(String JobTemplateType, String UserID, String JobGUID, String JsonString, int SatyamTaskTableEntryID )
        {

            ////first see if this task has already been aggregated and 
            //SatyamAggregatedResultsTableEntry entry = getEntryByTaskID(SatyamTaskTableEntryID);

            //if(entry!=null)
            //{
            //    if(JsonString != entry.ResultString) //if there is nothing to be updated
            //    {
            //        updateResultString(entry.ID,JsonString);                    
            //    }
            //    return true;
            //}

            int noTries = 0;
            bool ret = true;
            bool done = true;
            //int zero = 0;
            do
            {
                String SQLCommandString = "INSERT INTO " + TableName + " (JobTemplateType,UserID, JobGUID, ResultString,SatyamTaskTableEntryID) VALUES(@JobTemplateType,@UserID, @JobGUID, @ResultString,@SatyamTaskTableEntryID)";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", JobTemplateType);
                sqlCommand.Parameters.AddWithValue("@UserId", UserID);
                sqlCommand.Parameters.AddWithValue("@JobGUID", JobGUID);
                sqlCommand.Parameters.AddWithValue("@ResultString", JsonString);
                sqlCommand.Parameters.AddWithValue("@SatyamTaskTableEntryID", SatyamTaskTableEntryID.ToString());

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

        public void AddEntry(SatyamAggregatedResultsTableEntry entry, int ResultsAggregated = 0)
        {
            AddEntry(entry.JobTemplateType, entry.UserID, entry.JobGUID, entry.ResultString, entry.SatyamTaskTableEntryID);
            UpdateResultsAggregated(entry, ResultsAggregated);
        }

        public void AddEntries(List<SatyamAggregatedResultsTableEntry> entries)
        {
            foreach (SatyamAggregatedResultsTableEntry entry in entries)
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
            }
            return true;
        }

        /***** Updating entries ***********************************************************************************************/

        public bool updateResultString(int ID, string resultString)
        {
                String SQLCommandString = "UPDATE " + TableName + " SET ResultString = '" + resultString + "' WHERE ID = '" + ID + "'";
                return ExecuteNoQuerySQLCommand(SQLCommandString);
          
        }

        /// <summary>
        /// Results Numbers
        /// </summary>
        /// <param name="_ID"></param>
        /// <param name="_value"></param>
        /// <returns></returns>

        private bool UpdateResultsAggregated(SatyamAggregatedResultsTableEntry entry, int _value)
        {
            int value = _value;
            String SQLCommandString = "UPDATE " + TableName + " SET ResultsAggregated = '" + value + "' WHERE SatyamTaskTableEntryID = '" + entry.SatyamTaskTableEntryID + "' AND ResultsAggregated IS NULL";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        /****** Deleting Entries ********************************************************************************************/
        public bool DeleteEntriesByGUID(string guid)
        {
            String SQLCommandString = "DELETE FROM " + TableName + " WHERE JobGUID = '" + guid + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

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


        public List<int> getAllTaskIDs()
        {
            List<int> IDList = new List<int>();
            String SQLCommandString = "SELECT DISTINCT SatyamTaskTableEntryID FROM " + TableName;

            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int taskID = (int)reader["SatyamTaskTableEntryID"];
                    IDList.Add(taskID);
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

    }
}
