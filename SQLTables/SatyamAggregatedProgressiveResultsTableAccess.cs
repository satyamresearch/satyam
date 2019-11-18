using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLTables
{
    /*
     CREATE TABLE [dbo].[SatyamAggregatedProgressiveResultsTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[UserID] VARCHAR(200) NOT NULL,
	[JobGUID] VARCHAR(200) NOT NULL, 
    [JobTemplateType] VARCHAR(100) NOT NULL, 
    [ResultString] VARCHAR(MAX) NOT NULL, 
    [SatyamTaskTableEntryID] INT NOT NULL, 
    [ResultsAggregated] INT NOT NULL,
)
     */


    public class SatyamAggregatedProgressiveResultsTableEntry
    {
        public int ID;
        public string JobTemplateType;
        public string UserID;
        public string JobGUID;
        public string ResultString;
        public int SatyamTaskTableEntryID;
        public int ResultsAggregated;

        public SatyamAggregatedProgressiveResultsTableEntry()
        {

        }

        public SatyamAggregatedProgressiveResultsTableEntry(int _ID, String _JobType, String _UserID, String _JoBGUID, String _JsonString, int _SatyamTaskTableEntryID, int _ResultsAggregated)
        {
            ID = _ID;
            JobTemplateType = _JobType;
            UserID = _UserID;
            JobGUID = _JoBGUID;
            ResultString = _JsonString;
            SatyamTaskTableEntryID = _SatyamTaskTableEntryID;
            ResultsAggregated = _ResultsAggregated;
        }
    }


    public class SatyamAggregatedProgressiveResultsTableAccess
    {
        string TableName = "SatyamAggregatedProgressiveResultsTable";
        SatyamAzureSQLDBAccess dbAccess;

        public SatyamAggregatedProgressiveResultsTableAccess()
        {
            dbAccess = new SatyamAzureSQLDBAccess();
        }

        public void close()
        {
            dbAccess.close();
        }

        public List<SatyamAggregatedProgressiveResultsTableEntry> getEntries(string SQLCommandString)
        {
            List<SatyamAggregatedProgressiveResultsTableEntry> ret = new List<SatyamAggregatedProgressiveResultsTableEntry>();
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
                    int ResultsAggregated = (int)reader["ResultsAggregated"];

                    SatyamAggregatedProgressiveResultsTableEntry entry = new SatyamAggregatedProgressiveResultsTableEntry(ID, JobTemplateType, UserID, JobGUID, JSonString, SatyamTaskTableEntryID, ResultsAggregated);
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

        public SatyamAggregatedProgressiveResultsTableEntry getEntry(string SQLCommandString)
        {
            List<SatyamAggregatedProgressiveResultsTableEntry> entries = getEntries(SQLCommandString);
            if (entries.Count > 0)
            {
                return entries[0];
            }
            else
            {
                return null;
            }
        }

        public List<SatyamAggregatedProgressiveResultsTableEntry> getEntriesByGUID(string GUID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + GUID + "'";
            List<SatyamAggregatedProgressiveResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public List<SatyamAggregatedProgressiveResultsTableEntry> getEntriesByGUIDOrderByResultsAggregatedDesc(string GUID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + GUID + "' ORDER BY RESULTSAGGREGATED DESC";
            List<SatyamAggregatedProgressiveResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public SatyamAggregatedProgressiveResultsTableEntry getEntryByTaskID(int taskID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE SatyamTaskTableEntryID = '" + taskID.ToString() + "'";
            SatyamAggregatedProgressiveResultsTableEntry entry = getEntry(SQLCommandString);
            return entry;
        }

        public List<SatyamAggregatedProgressiveResultsTableEntry> getEntriesByTaskID(int taskID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE SatyamTaskTableEntryID = '" + taskID.ToString() + "' ORDER BY RESULTSAGGREGATED DESC";
            List<SatyamAggregatedProgressiveResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        public SatyamAggregatedProgressiveResultsTableEntry getLatestEntryWithMostResultsAggregatedByTaskID(int taskID)
        {
            string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE SatyamTaskTableEntryID = '" + taskID.ToString() + "' ORDER BY RESULTSAGGREGATED DESC";
            SatyamAggregatedProgressiveResultsTableEntry entries = getEntry(SQLCommandString);
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

        public List<SatyamAggregatedProgressiveResultsTableEntry> getAllEntries()
        {
            string SQLCommandString = "SELECT * FROM " + TableName;
            List<SatyamAggregatedProgressiveResultsTableEntry> entries = getEntries(SQLCommandString);
            return entries;
        }

        /***** Adding Entries ********************************************************************************************/

        public bool AddEntry(String JobTemplateType, String UserID, String JobGUID, String JsonString, int SatyamTaskTableEntryID, int ResultsAggregated)
        {

            ////first see if this task has already been aggregated and 
            //SatyamAggregatedProgressiveResultsTableEntry entry = getEntryByTaskID(SatyamTaskTableEntryID);

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
                String SQLCommandString = "INSERT INTO " + TableName + " (JobTemplateType,UserID, JobGUID, ResultString,SatyamTaskTableEntryID, ResultsAggregated) VALUES(@JobTemplateType,@UserID, @JobGUID, @ResultString,@SatyamTaskTableEntryID, @ResultsAggregated)";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", JobTemplateType);
                sqlCommand.Parameters.AddWithValue("@UserId", UserID);
                sqlCommand.Parameters.AddWithValue("@JobGUID", JobGUID);
                sqlCommand.Parameters.AddWithValue("@ResultString", JsonString);
                sqlCommand.Parameters.AddWithValue("@SatyamTaskTableEntryID", SatyamTaskTableEntryID.ToString());
                sqlCommand.Parameters.AddWithValue("@ResultsAggregated", ResultsAggregated.ToString());

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

        public void AddEntry(SatyamAggregatedProgressiveResultsTableEntry entry)
        {
            AddEntry(entry.JobTemplateType, entry.UserID, entry.JobGUID, entry.ResultString, entry.SatyamTaskTableEntryID, entry.ResultsAggregated);
        }

        public void AddEntries(List<SatyamAggregatedProgressiveResultsTableEntry> entries)
        {
            foreach (SatyamAggregatedProgressiveResultsTableEntry entry in entries)
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

        //private bool UpdateResultsAggregated(SatyamAggregatedProgressiveResultsTableEntry entry, int _value)
        //{
        //    int value = _value;
        //    String SQLCommandString = "UPDATE " + TableName + " SET ResultsAggregated = '" + value + "' WHERE SatyamTaskTableEntryID = '" + entry.SatyamTaskTableEntryID + "' AND ResultsAggregated IS NULL";
        //    return ExecuteNoQuerySQLCommand(SQLCommandString);
        //}

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
