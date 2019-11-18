using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using Constants;

namespace SQLTables
{

    /*
     CREATE TABLE [dbo].[SatyamTaskTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserID] VARCHAR(200) NOT NULL,
	[JobGUID] VARCHAR(200) NOT NULL, 
    [JobTemplateType] VARCHAR(100) NOT NULL, 
    [TaskParametersString] VARCHAR(MAX) NOT NULL, 
    [JobSubmitTime] DATETIME2 NOT NULL, 
	[DoneScore] INT NOT NULL,
    [PricePerTask] FLOAT NULL,
)
    */

    public class SatyamTaskTableEntry
    {
        public int ID;
        public string JobTemplateType;
        public string UserID;
        public string JobGUID;
        public string TaskParametersString;
        public DateTime JobSubmitTime;
        public int DoneScore;
        public double PricePerTask;

        public SatyamTaskTableEntry()
        {

        }

        public SatyamTaskTableEntry(int _ID, String _JobType, String _UserID, String _JoBGUID, String _JsonString, DateTime _SubmitTime, int _DoneScore)
        {
            ID = _ID;
            JobTemplateType = _JobType;
            UserID = _UserID;
            JobGUID = _JoBGUID;
            TaskParametersString = _JsonString;
            JobSubmitTime = _SubmitTime;
            DoneScore = _DoneScore;
            PricePerTask = 0;
        }

        public SatyamTaskTableEntry(int _ID, String _JobType, String _UserID, String _JoBGUID, String _JsonString, DateTime _SubmitTime, int _DoneScore, double _PricePerTask)
        {
            ID = _ID;
            JobTemplateType = _JobType;
            UserID = _UserID;
            JobGUID = _JoBGUID;
            TaskParametersString = _JsonString;
            JobSubmitTime = _SubmitTime;
            DoneScore = _DoneScore;
            PricePerTask = _PricePerTask;
        }


    }

    public class SatyamTaskTableAccess
    {
        string TableName = "SatyamTaskTable";
        SatyamAzureSQLDBAccess dbAccess;

        public SatyamTaskTableAccess()
        {
            dbAccess = new SatyamAzureSQLDBAccess();
        }

        public void close()
        {
            dbAccess.close();
        }

        public List<SatyamTaskTableEntry> getEntries(string SQLCommandString)
        {
            List<SatyamTaskTableEntry> ret = new List<SatyamTaskTableEntry>();
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
                    string JSonString = (string)reader["TaskParametersString"];
                    DateTime SubmitTime = (DateTime)reader["JobSubmitTime"];
                    int DoneScore = (int)reader["DoneScore"];
                    double PricePerTask = 0;
                    if(reader["PricePerTask"]!=null)
                    {
                        PricePerTask = (double)reader["PricePerTask"];
                    }
                    SatyamTaskTableEntry entry = new SatyamTaskTableEntry(ID, JobTemplateType, UserID, JobGUID, JSonString, SubmitTime, DoneScore,PricePerTask);
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

        public SatyamTaskTableEntry getEntry(string SQLCommandString)
        {
            List<SatyamTaskTableEntry> entries = getEntries(SQLCommandString);
            if (entries.Count > 0)
            {
                return entries[0];
            }
            else
            {
                return null;
            }
        }

        public List<SatyamTaskTableEntry> getAllEntries()
        {
            string SQLCommandString = "SELECT * FROM " + TableName;
            return getEntries(SQLCommandString);
        }

        public List<SatyamTaskTableEntry> getAllEntriesByJobtemplateType(string JobTemplateType)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "'";
            return getEntries(SQLCommandString);
        }

        public List<SatyamTaskTableEntry> getAllEntriesByGUID(string guid)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + guid + "'";
            return getEntries(SQLCommandString);
        }

        /***** Adding Entries ********************************************************************************************/

        public bool AddEntry(String JobTemplateType, String UserID, String JobGUID, String JsonString, DateTime SubmitTime, double PricePerTask)
        {
            int noTries = 0;
            bool ret = true;
            bool done = true;
            int zero = 0;
            do
            {
                String SQLCommandString = "INSERT INTO " + TableName + " (JobTemplateType,UserID, JobGUID, TaskParametersString,JobSubmitTime,DoneScore,PricePerTask) VALUES(@JobTemplateType,@UserID, @JobGUID, @TaskParametersString,@JobSubmitTime,@DoneScore,@PricePerTask)";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", JobTemplateType);
                sqlCommand.Parameters.AddWithValue("@UserId", UserID);
                sqlCommand.Parameters.AddWithValue("@JobGUID", JobGUID);
                sqlCommand.Parameters.AddWithValue("@TaskParametersString", JsonString);
                sqlCommand.Parameters.AddWithValue("@JobSubmitTime", SubmitTime.ToString());
                sqlCommand.Parameters.AddWithValue("@DoneScore", zero.ToString());
                sqlCommand.Parameters.AddWithValue("@PricePerTask", PricePerTask.ToString());
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

        public bool AddEntryWithSpecificID(int ID, String JobTemplateType, String UserID, String JobGUID, String JsonString, DateTime SubmitTime, double PricePerTask)
        {
            int noTries = 0;
            bool ret = true;
            bool done = true;
            int zero = 0;
            do
            {
                String SQLCommandString = "SET IDENTITY_INSERT dbo." + TableName + " ON;";
                SQLCommandString += "INSERT INTO " + TableName + " (Id, JobTemplateType,UserID, JobGUID, TaskParametersString,JobSubmitTime,DoneScore,PricePerTask) VALUES(@Id, @JobTemplateType,@UserID, @JobGUID, @TaskParametersString,@JobSubmitTime,@DoneScore,@PricePerTask)";
                SQLCommandString += "SET IDENTITY_INSERT dbo." + TableName + " OFF;";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@Id", ID.ToString());
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", JobTemplateType);
                sqlCommand.Parameters.AddWithValue("@UserId", UserID);
                sqlCommand.Parameters.AddWithValue("@JobGUID", JobGUID);
                sqlCommand.Parameters.AddWithValue("@TaskParametersString", JsonString);
                sqlCommand.Parameters.AddWithValue("@JobSubmitTime", SubmitTime.ToString());
                sqlCommand.Parameters.AddWithValue("@DoneScore", zero.ToString());
                sqlCommand.Parameters.AddWithValue("@PricePerTask", PricePerTask.ToString());
                //Console.WriteLine("INSERT INTO " + TableName + " (Id, JobTemplateType,UserID, JobGUID, TaskParametersString,JobSubmitTime,DoneScore,PricePerTask) " +
                //    "VALUES(" + ID.ToString() +
                //    "," + JobTemplateType +
                //    "," + UserID +
                //    "," + JobGUID +
                //    "," + JsonString +
                //    "," + SubmitTime.ToString() +
                //    "," + zero.ToString() +
                //    "," + PricePerTask.ToString() + ")");
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

        public bool AddEntry(String JobTemplateType, String UserID, String JobGUID, String JsonString, DateTime SubmitTime)
        {
            return AddEntry(JobTemplateType, UserID, JobGUID, JsonString, SubmitTime, 0);
        }

            public void AddEntry(SatyamTaskTableEntry entry)
        {
            AddEntry(entry.JobTemplateType, entry.UserID, entry.JobGUID, entry.TaskParametersString, entry.JobSubmitTime, entry.PricePerTask);
        }

        public void AddEntries(List<SatyamTaskTableEntry> taskList)
        {
            foreach (SatyamTaskTableEntry task in taskList)
            {
                AddEntry(task);
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

        /****** Deleting Entries ********************************************************************************************/
        public bool DeleteEntry(int id)
        {
            String SQLCommandString = "DELETE FROM " + TableName + " WHERE Id = '" + id + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }
        
        public bool Clear()
        {
            String SQLCommandString = "DELETE FROM " + TableName;
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool ClearByJobGUID(string GUID)
        {
            String SQLCommandString = "DELETE FROM " + TableName + " WHERE JobGUID = '" + GUID + "'"; 
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }


        /// <summary>
        /// Results Numbers
        /// </summary>
        /// <param name="_ID"></param>
        /// <param name="_value"></param>
        /// <returns></returns>

        public bool UpdateResultNumber(int _ID, int _value)
        {
            int value = _value;
            String SQLCommandString = "UPDATE " + TableName + " SET ResultNumber = '" + value + "' WHERE Id = '" + _ID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        /************Done Scores *****************************************************************************************/
        public bool UpdateDoneScore(int _ID, int _value)
        {            
            int value = _value;
            String SQLCommandString = "UPDATE " + TableName + " SET DoneScore = '" + value + "' WHERE Id = '" + _ID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool IncrementDoneScore(int _ID)
        {
            String SQLCommandString = "UPDATE " + TableName + " SET DoneScore = DoneScore + 1 WHERE Id = '" + _ID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool DecrementDoneScore(int _ID)
        {
            String SQLCommandString = "UPDATE " + TableName + " SET DoneScore = DoneScore - 1 WHERE Id = '" + _ID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool ResetDoneScore(int _ID)
        {
            int value = 0;
            String SQLCommandString = "UPDATE " + TableName + " SET DoneScore = '" + value + "' WHERE Id = '" + _ID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool ResetAllDoneScores()
        {
            int value = 0;
            String SQLCommandString = "UPDATE " + TableName + " SET DoneScore = '" + value + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool ResetDoneScoresByJobGUID(string GUID)
        {
            int value = 0;
            String SQLCommandString = "UPDATE " + TableName + " SET DoneScore = '" + value + "' WHERE JobGUID = '" + GUID + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        /***** Remaining Tasks **************************************************************/


        public int getCountsBySQLString(string SQLCommandString)
        {
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 500;
            int num = 0;
            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                reader.Read();
                num = (int)reader["REMAINING"];
                reader.Close();
            }
            catch (SqlException ex)
            {
                throw ex;
            }
            finally
            {
            }
            return num;
        }
        public int getUndoneTasksCountByGUID(string GUID)
        {
            String SQLCommandString = "SELECT COUNT(DoneScore) AS REMAINING FROM " + TableName + " WHERE DoneScore=0 AND WHERE JobGUID = '" + GUID + "'";
            return getCountsBySQLString(SQLCommandString);
        }

        public int getTasksCountByJobTemplateType(string JobTemplateType)
        {
            String SQLCommandString = "SELECT COUNT(DoneScore) AS REMAINING FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "'";
            return getCountsBySQLString(SQLCommandString);
        }

        public int getTasksCountByJobTemplateTypeAndPrice(string JobTemplateType, double Price)
        {
            String SQLCommandString = "SELECT COUNT(DoneScore) AS REMAINING FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "' AND PricePerTask='" + Price.ToString() + "'";
            return getCountsBySQLString(SQLCommandString);
        }

        public int getTasksCountByGUID(string guid)
        {
            String SQLCommandString = "SELECT COUNT(ID) AS REMAINING FROM " + TableName + " WHERE JobGUID = '" + guid + "'";
            return getCountsBySQLString(SQLCommandString);
        }

        public int getRemainingTasksLessThanADoneScoreCountByGUID(string GUID, int doneScore)
        {
            String SQLCommandString = "SELECT COUNT(DoneScore) AS REMAINING FROM " + TableName + " WHERE DoneScore <= '" + doneScore + "' AND JobGUID = '" + GUID + "'";
            return getCountsBySQLString(SQLCommandString);
        }

        public int getRemainingTasksLessThanADoneScoreCountByJobTemplateType(string JobTemplateType, int doneScore)
        {
            String SQLCommandString = "SELECT COUNT(DoneScore) AS REMAINING FROM " + TableName + " WHERE DoneScore <= '" + doneScore + "' AND JobTemplateType = '" + JobTemplateType + "'";
            return getCountsBySQLString(SQLCommandString);
        }

        public int getRemainingTasksLessThanADoneScoreCountByJobTemplateTypeAndPrice(string JobTemplateType, int doneScore, double Price)
        {
            String SQLCommandString = "SELECT COUNT(DoneScore) AS REMAINING FROM " + TableName + " WHERE DoneScore <= '" + doneScore + "' AND JobTemplateType = '" + JobTemplateType + "' AND PricePerTask='" + Price.ToString() + "'";
            return getCountsBySQLString(SQLCommandString);
        }

        public int getMinDoneScoreByJobTemplateTypeAndPrice(string JobTemplateType, double Price)
        {
            String SQLCommandString = "SELECT DoneScore AS REMAINING FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "' AND PricePerTask='" + Price.ToString() + "' ORDER BY DONESCORE";
            return getCountsBySQLString(SQLCommandString);
        }

        public int getRemainingTaskCount()
        {
            String SQLCommandString = "SELECT COUNT(DoneScore) AS REMAINING FROM " + TableName + " WHERE DoneScore=0";
            return getCountsBySQLString(SQLCommandString);
        }

        /*** Getting Randomized Entries **********************************************/
        /*SELECT TOP 1 column FROM table ORDER BY NEWID()*/
        /*
         Select a random row with MySQL:
         SELECT column FROM table
         ORDER BY RAND()
         LIMIT 1
         Select a random row with PostgreSQL:
         SELECT column FROM table
         ORDER BY RANDOM()
         LIMIT 1
         Select a random row with Microsoft SQL Server:
         SELECT TOP 1 column FROM table
         ORDER BY NEWID()
         */
        public SatyamTaskTableEntry getMinimumTriedEntryByTemplate(string JobTemplateType)
        {
            int noTasks = getTasksCountByJobTemplateType(JobTemplateType);
            if (noTasks == 0)
            {
                return null;
            }
            int doneScore = -1;
            int num = 0;
            do
            {
                doneScore++;
                num = getRemainingTasksLessThanADoneScoreCountByJobTemplateType(JobTemplateType, doneScore);
            } while (num == 0);

            //string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE DoneScore = '" + doneScore + "' AND JobTemplateType = '" + JobTemplateType + "' ORDER BY NEWID()";            
            //SatyamTaskTableEntry entry = getEntry(SQLCommandString);
            SatyamTaskTableEntry entry = getRandomEntryByJobTemplateAndMaxDoneScore(JobTemplateType, doneScore);
            return entry;
        }

        public SatyamTaskTableEntry getMinimumTriedEntryByTemplateAndMaxDoneScore(string JobTemplateType, int MaxDoneScore)
        {
            int noTasks = getTasksCountByJobTemplateType(JobTemplateType);
            if (noTasks == 0)
            {
                return null;
            }
            int doneScore = 0;
            int num = 0;
            while (num == 0 && doneScore < MaxDoneScore) { 
                num = getRemainingTasksLessThanADoneScoreCountByJobTemplateType(JobTemplateType, doneScore);
                doneScore++;
            }

            //string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE DoneScore = '" + doneScore + "' AND JobTemplateType = '" + JobTemplateType + "' ORDER BY NEWID()";            
            //SatyamTaskTableEntry entry = getEntry(SQLCommandString);
            SatyamTaskTableEntry entry = getRandomEntryByJobTemplateAndMaxDoneScore(JobTemplateType, doneScore-1);
            return entry;
        }


        public SatyamTaskTableEntry getMinimumTriedEntryByTemplateAndPrice(string JobTemplateType, double Price)
        {
            int noTasks = getTasksCountByJobTemplateTypeAndPrice(JobTemplateType,Price);
            if (noTasks == 0)
            {
                return null;
            }

            int doneScore = getMinDoneScoreByJobTemplateTypeAndPrice(JobTemplateType, Price);
            //string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE DoneScore = '" + doneScore + "' AND JobTemplateType = '" + JobTemplateType + "' ORDER BY NEWID()";
            //SatyamTaskTableEntry entry = getEntry(SQLCommandString);
            SatyamTaskTableEntry entry = getRandomEntryByJobTemplateAndPriceAndMaxDoneScore(JobTemplateType, doneScore, Price);
            return entry;
        }


        public SatyamTaskTableEntry getMinimumTriedNewEntryForWorkerIDByTemplateAndPrice(string WorkerID, string JobTemplateType, 
            double Price)
        {
            int noTasks = getTasksCountByJobTemplateTypeAndPrice(JobTemplateType, Price);
            if (noTasks == 0)
            {
                return null;
            }
            //int num = 0;
            //do
            //{
            //    doneScore++;
            //    num = getRemainingTasksLessThanADoneScoreCountByJobTemplateTypeAndPrice(JobTemplateType, doneScore, Price);
            //} while (num == 0);

            int doneScore = getMinDoneScoreByJobTemplateTypeAndPrice(JobTemplateType, Price);

            SatyamTaskTableEntry entry = new SatyamTaskTableEntry();
            SatyamResultsTableAccess resultDB = new SatyamResultsTableAccess();
            List<string> GUIDList = new List<string>();
            List<int> TriedEntries = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                // try at most 3 times
                //string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE DoneScore = '" + doneScore + "' AND JobTemplateType = '" + JobTemplateType + "' ORDER BY NEWID()";
                //entry = getEntry(SQLCommandString);


                //entry = getRandomEntry_FromTopK_ByJobTemplatePrice(JobTemplateType, Price, 10);

                entry = getRandomEntryByJobTemplateAndPriceAndMaxDoneScore(JobTemplateType, doneScore, Price);

                if (!GUIDList.Contains(entry.JobGUID))
                {
                    GUIDList.Add(entry.JobGUID);
                    TriedEntries.AddRange(resultDB.getAllTaskIDsAWorkerHasDoneForAGUID(WorkerID, entry.JobGUID));
                }
                // the guid might change here if multiple guid are in play with same price and job template
                // query by jobtemplate itself takes the whole table, too long.
                if (!TriedEntries.Contains(entry.ID) || WorkerID == "Testing" || WorkerID == TaskConstants.AdminID)//
                {
                    break;
                }
                else
                {
                    entry = null;
                }
            }

            //if (entry == null)
            //{
            //    entry = getRandomEntryByJobTemplateDoneScorePrice(JobTemplateType, doneScore, Price);
            //}

            resultDB.close();
            return entry;
        }

        public SatyamTaskTableEntry getMinimumTriedNewEntryForWorkerIDByTemplateAndPriceAndMaxDoneScore(string WorkerID, string JobTemplateType,
            double Price, int MaxDoneScore)
        {
            int noTasks = getTasksCountByJobTemplateTypeAndPrice(JobTemplateType, Price);
            if (noTasks == 0)
            {
                return null;
            }

            int doneScore = getMinDoneScoreByJobTemplateTypeAndPrice(JobTemplateType, Price);
            if (doneScore >= MaxDoneScore) return null;

            SatyamTaskTableEntry entry = new SatyamTaskTableEntry();
            SatyamResultsTableAccess resultDB = new SatyamResultsTableAccess();
            List<string> GUIDList = new List<string>();
            List<int> TriedEntries = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                entry = getRandomEntryByJobTemplateAndPriceAndMaxDoneScore(JobTemplateType, doneScore, Price);

                if (!GUIDList.Contains(entry.JobGUID))
                {
                    GUIDList.Add(entry.JobGUID);
                    TriedEntries.AddRange(resultDB.getAllTaskIDsAWorkerHasDoneForAGUID(WorkerID, entry.JobGUID));
                }
                // the guid might change here if multiple guid are in play with same price and job template
                // query by jobtemplate itself takes the whole table, too long.
                if (!TriedEntries.Contains(entry.ID) || WorkerID == "Testing" || WorkerID == TaskConstants.AdminID)//
                {
                    break;
                }
                else
                {
                    entry = null;
                }
            }

            resultDB.close();
            return entry;
        }


        public SatyamTaskTableEntry getTopKNewEntryForWorkerIDByTemplateAndPrice(int K, string WorkerID, string JobTemplateType,
            double Price)
        {
            int noTasks = getTasksCountByJobTemplateTypeAndPrice(JobTemplateType, Price);
            if (noTasks == 0)
            {
                return null;
            }
            //int num = 0;
            //do
            //{
            //    doneScore++;
            //    num = getRemainingTasksLessThanADoneScoreCountByJobTemplateTypeAndPrice(JobTemplateType, doneScore, Price);
            //} while (num == 0);

            int doneScore = getMinDoneScoreByJobTemplateTypeAndPrice(JobTemplateType, Price);

            SatyamTaskTableEntry entry = new SatyamTaskTableEntry();
            SatyamResultsTableAccess resultDB = new SatyamResultsTableAccess();
            List<string> GUIDList = new List<string>();
            List<int> TriedEntries = new List<int>();
            for (int i = 0; i < 3; i++)
            {
                // try at most 3 times
                entry = getRandomEntry_FromTopK_ByJobTemplatePrice(JobTemplateType, Price, K);

                if (!GUIDList.Contains(entry.JobGUID))
                {
                    GUIDList.Add(entry.JobGUID);
                    TriedEntries.AddRange(resultDB.getAllTaskIDsAWorkerHasDoneForAGUID(WorkerID, entry.JobGUID));
                }
                // the guid might change here if multiple guid are in play with same price and job template
                // query by jobtemplate itself takes the whole table, too long.
                if (!TriedEntries.Contains(entry.ID) || WorkerID == "Testing" || WorkerID == TaskConstants.AdminID)//
                {
                    break;
                }
                else
                {
                    entry = null;
                }
            }

            if (entry == null)
            {
                // fall back to the minimum done score approach
                entry = getMinimumTriedNewEntryForWorkerIDByTemplateAndPrice(WorkerID, JobTemplateType, Price);
            }

            resultDB.close();
            return entry;
        }


        public SatyamTaskTableEntry getRandomEntryByJobTemplateAndGUID(string JobTemplateType, string GUID)
        {
           
            string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "' AND JobGUID = '" + GUID + "' ORDER BY NEWID()";
            SatyamTaskTableEntry entry = getEntry(SQLCommandString);
            return entry;
        }

        public SatyamTaskTableEntry getRandomEntryByJobTemplateAndMaxDoneScore(string JobTemplateType, int doneScore)
        {
            string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE DoneScore <= '" + doneScore + "' AND JobTemplateType = '" + JobTemplateType + "' ORDER BY NEWID()";
            SatyamTaskTableEntry entry = getEntry(SQLCommandString);
            return entry;
        }

        public SatyamTaskTableEntry getRandomEntryByJobTemplateAndPriceAndMaxDoneScore(string JobTemplateType, int doneScore, double price)
        {

            string SQLCommandString = "SELECT TOP 1 * FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "' AND DoneScore <= '" + doneScore + "' AND PricePerTask = '" + price + "' ORDER BY NEWID()";
            SatyamTaskTableEntry entry = getEntry(SQLCommandString);
            return entry;
        }

        public SatyamTaskTableEntry getRandomEntry_FromTopK_ByJobTemplatePrice(string JobTemplateType, double price, int K)
        {

            string SQLCommandString = "SELECT TOP 1 * FROM (SELECT TOP " + K + " * FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "' AND PricePerTask = '" + price + "' ORDER BY ID) AS A ORDER BY NEWID()";
            SatyamTaskTableEntry entry = getEntry(SQLCommandString);
            return entry;
        }

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

        public List<int> getAllIDs()
        {
            
            String SQLCommandString = "SELECT ID FROM " + TableName;

            return getIDsBySQLCommand(SQLCommandString);
        }

        public List<int> getAllIDsByGUID(string JobGUID)
        {
            
            String SQLCommandString = "SELECT ID FROM " + TableName + " WHERE JobGUID = '" + JobGUID + "'";

            return getIDsBySQLCommand(SQLCommandString);
        }

        public List<string> getAllGUIDs()
        {
            string SQLCommandString = "SELECT DISTINCT JoBGUID FROM " + TableName;
            List<string> GUIDList = new List<string>();
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    string ID = (string)reader["JobGUID"];
                    GUIDList.Add(ID);
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
            return GUIDList;
        }
    }
}
