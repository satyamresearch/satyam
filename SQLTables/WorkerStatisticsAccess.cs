using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLTables
{

    public class WorkerStatisticsTableEntry
    {
        public int Id;
        public string WorkerId;
        public string JobTemplateType;
        public int TasksDone;
        public int TasksApproved;
        public double SuccessFraction;
        public DateTime LastUpdateTime;

        public WorkerStatisticsTableEntry(
            int l_Id,
            string l_WorkerId,
            string l_JobTemplateType,
            int l_TasksDone,
            int l_TasksApproved,
            double l_SuccessFraction,
            DateTime l_LastUpdateTime
            )
        {
            Id = l_Id;
            WorkerId = l_WorkerId;
            JobTemplateType = l_JobTemplateType;
            TasksDone = l_TasksDone;
            TasksApproved = l_TasksApproved;
            SuccessFraction = l_SuccessFraction;
            LastUpdateTime = l_LastUpdateTime;
        }

        public override string ToString()
        {
            return Id + "\t" + WorkerId + "\t" + JobTemplateType + "\t" + TasksDone + "\t" + TasksApproved + "\t" + SuccessFraction + "\t" + LastUpdateTime;
        }
    }


    public class WorkerStatisticsAccess
    {
        const string TableName = "WorkerStatisticsTable";
        SatyamAzureSQLDBAccess dbAccess;

        public WorkerStatisticsAccess()
        {
            dbAccess = new SatyamAzureSQLDBAccess();
        }

        public void close()
        {
            dbAccess.close();
        }


        public WorkerStatisticsTableEntry getWorkerStatistics(string l_WorkerId, string l_JobTemplateType)
        {
            String SQLCommandString = "SELECT * FROM " + TableName + " WHERE WorkerId = '" + l_WorkerId + "' AND JobTemplateType = '" + l_JobTemplateType + "'";
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;
            WorkerStatisticsTableEntry entry = null;
            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int Id = (int)reader["Id"];
                    string WorkerId = (string)reader["WorkerId"];
                    string JobTemplateType = (string)reader["JobTemplateType"];
                    int TasksDone = (int)reader["TasksDone"];
                    int TasksApproved = (int)reader["TasksApproved"];
                    double SuccessFraction = (double)reader["SuccessFraction"];
                    DateTime LastUpdateTime = (DateTime)reader["LastUpdateTime"];
                    entry = new WorkerStatisticsTableEntry(Id, WorkerId, JobTemplateType, TasksDone, TasksApproved, SuccessFraction, LastUpdateTime);
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
            return entry;
        }

        void UpdateNewSingleEntry(WorkerStatisticsTableEntry entry, bool approved, DateTime doneTime)
        {
            entry.TasksDone++;
            if (approved)
            {
                entry.TasksApproved++;
            }
            entry.SuccessFraction = ((float)entry.TasksApproved + 1.0) / ((float)entry.TasksDone + 2.0);

            if (doneTime > entry.LastUpdateTime)
            {
                entry.LastUpdateTime = doneTime;
            }

            String SQLCommandString = "UPDATE " + TableName + " SET TasksDone = '" + entry.TasksDone + "', TasksApproved = '" + entry.TasksApproved + "', SuccessFraction = '" + entry.SuccessFraction + "', LastUpdateTime = '" + entry.LastUpdateTime + "' WHERE WorkerId = '" + entry.WorkerId + "' AND JobTemplateType = '" + entry.JobTemplateType + "'";

            //Console.WriteLine(SQLCommandString);
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.ToString());
                throw ex;
            }
        }


        public void UpdateEntry(string l_WorkerId,
            string l_JobTemplateType,
            int l_TasksDone,
            int l_TasksApproved,
            DateTime l_LastUpdateTime)
        {

            WorkerStatisticsTableEntry entry = getWorkerStatistics(l_WorkerId, l_JobTemplateType);

            if (entry != null)
            {
                entry.TasksDone = l_TasksDone;
                entry.TasksApproved = l_TasksApproved;
                entry.SuccessFraction = (l_TasksApproved + 1) / (l_TasksDone + 2);
                entry.LastUpdateTime = l_LastUpdateTime;
                String SQLCommandString = "UPDATE " + TableName + " SET TasksDone = '" + entry.TasksDone + "', TasksApproved = '" + entry.TasksApproved + "', SuccessFraction = '" + entry.SuccessFraction + "', LastUpdateTime = '" + entry.LastUpdateTime + "' WHERE WorkerId = '" + entry.WorkerId + "' AND JobTemplateType = '" + entry.JobTemplateType + "'";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 200;
                try
                {
                    sqlCommand.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    throw ex;
                }
            }
            else
            {
                AddEntry(l_WorkerId, l_JobTemplateType, l_TasksDone, l_TasksApproved, l_LastUpdateTime);
            }
            //Console.WriteLine(SQLCommandString);
        }

        void AddNewSingleEntry(string l_WorkerId,
            string l_JobTemplateType,
            bool approved, DateTime doneTime)
        {
            int TasksDone = 1;
            int TasksApproved = 0;
            double SuccessFraction = 0.0;
            bool done = true;
            int noTries = 0;

            if (approved)
            {
                TasksApproved++;
                SuccessFraction = 2.0 / 3.0;
            }
            else
            {
                SuccessFraction = 1.0 / 3.0;
            }

            do
            {
                String rowNames = " (WorkerId," +
                                   "JobTemplateType," +
                                   "TasksDone," +
                                   "TasksApproved," +
                                   "SuccessFraction," +
                                   "LastUpdateTime)";

                String rowValueNames = "VALUES(@WorkerId," +
                               "@JobTemplateType," +
                               "@TasksDone," +
                               "@TasksApproved," +
                               "@SuccessFraction," +
                               "@LastUpdateTime)";

                String SQLCommandString = "INSERT INTO " + TableName + rowNames + rowValueNames;
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@WorkerId", l_WorkerId);
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", l_JobTemplateType);
                sqlCommand.Parameters.AddWithValue("@TasksDone", TasksDone.ToString());
                sqlCommand.Parameters.AddWithValue("@TasksApproved", TasksApproved.ToString());
                sqlCommand.Parameters.AddWithValue("@SuccessFraction", SuccessFraction.ToString());
                sqlCommand.Parameters.AddWithValue("@LastUpdateTime", doneTime.ToString());

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
                        }
                    }
                    Console.Error.WriteLine(ex.ToString());
                    throw ex;
                }
            } while (!done);
        }


        public void AddEntry(string l_WorkerId,
            string l_JobTemplateType,
            int l_TasksDone,
            int l_TasksApproved,
            DateTime l_LastUpdateTime
            )
        {
            double SuccessFraction = ((double)l_TasksApproved + 1) / ((double)l_TasksDone + 2);
            bool done = true;
            int noTries = 0;

            do
            {
                String rowNames = " (WorkerId," +
                                   "JobTemplateType," +
                                   "TasksDone," +
                                   "TasksApproved," +
                                   "SuccessFraction," +
                                   "LastUpdateTime)";

                String rowValueNames = "VALUES(@WorkerId," +
                               "@JobTemplateType," +
                               "@TasksDone," +
                               "@TasksApproved," +
                               "@SuccessFraction," +
                               "@LastUpdateTime)";

                String SQLCommandString = "INSERT INTO " + TableName + rowNames + rowValueNames;
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@WorkerId", l_WorkerId);
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", l_JobTemplateType);
                sqlCommand.Parameters.AddWithValue("@TasksDone", l_TasksDone.ToString());
                sqlCommand.Parameters.AddWithValue("@TasksApproved", l_TasksApproved.ToString());
                sqlCommand.Parameters.AddWithValue("@SuccessFraction", SuccessFraction.ToString());
                sqlCommand.Parameters.AddWithValue("@LastUpdateTime", l_LastUpdateTime.ToString());

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
                        }
                    }
                    Console.Error.WriteLine(ex.ToString());
                    throw ex;
                }
            } while (!done);
        }

        //adds or updates the entry
        public void UpdateSingleEntry(
            string l_WorkerId,
            string l_JobTemplateType,
            bool approved,
            DateTime lastUpdateTime
            )
        {
            WorkerStatisticsTableEntry entry = getWorkerStatistics(l_WorkerId, l_JobTemplateType);

            if (entry != null)
            {
                UpdateNewSingleEntry(entry, approved, lastUpdateTime);
            }
            else
            {
                AddNewSingleEntry(l_WorkerId, l_JobTemplateType, approved, lastUpdateTime);
            }
        }

        public void Flush()
        {
            String SQLCommandString = "DELETE FROM " + TableName;
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.ToString());
                throw ex;
            }
        }

        public void Flush(string JobTemplateType)
        {
            String SQLCommandString = "DELETE FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "'";
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            try
            {
                sqlCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                Console.Error.WriteLine(ex.ToString());
                throw ex;
            }
        }


        public SortedDictionary<string, WorkerStatisticsTableEntry> getAllEntries(string JobTemplateType)
        {
            SortedDictionary<string, WorkerStatisticsTableEntry> entries = new SortedDictionary<string, WorkerStatisticsTableEntry>();

            String SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobTemplateType = '" + JobTemplateType + "'";
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 500;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int Id = (int)reader["Id"];
                    string WorkerId = (string)reader["WorkerId"];
                    string l_JobTemplateType = (string)reader["JobTemplateType"];
                    int TasksDone = (int)reader["TasksDone"];
                    int TasksApproved = (int)reader["TasksApproved"];
                    double SuccessFraction = (double)reader["SuccessFraction"];
                    DateTime LastUpdateTime = (DateTime)reader["LastUpdateTime"];
                    WorkerStatisticsTableEntry entry = new WorkerStatisticsTableEntry(Id, WorkerId, l_JobTemplateType, TasksDone, TasksApproved, SuccessFraction, LastUpdateTime);
                    entries.Add(WorkerId, entry);
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

            return entries;
        }

        public SortedDictionary<string, SortedDictionary<string, WorkerStatisticsTableEntry>> getAllEntries()
        {
            SortedDictionary<string, SortedDictionary<string, WorkerStatisticsTableEntry>> entries = new SortedDictionary<string, SortedDictionary<string, WorkerStatisticsTableEntry>>();

            String SQLCommandString = "SELECT * FROM " + TableName;
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 500;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int Id = (int)reader["Id"];
                    string WorkerId = (string)reader["WorkerId"];
                    string JobTemplateType = (string)reader["JobTemplateType"];
                    int TasksDone = (int)reader["TasksDone"];
                    int TasksApproved = (int)reader["TasksApproved"];
                    double SuccessFraction = (double)reader["SuccessFraction"];
                    DateTime LastUpdateTime = (DateTime)reader["LastUpdateTime"];
                    WorkerStatisticsTableEntry entry = new WorkerStatisticsTableEntry(Id, WorkerId, JobTemplateType, TasksDone, TasksApproved, SuccessFraction, LastUpdateTime);
                    if (!entries.ContainsKey(WorkerId))
                    {
                        entries.Add(WorkerId, new SortedDictionary<string, WorkerStatisticsTableEntry>());
                    }
                    entries[WorkerId].Add(JobTemplateType, entry);
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

            return entries;
        }

    }
}
