using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace SQLTables
{

    public class WorkerInfoTableEntry
    {
        public int Id;
        public string WorkerId;
        public int Age;
        public string Sex;
        public string Ethnicity;
        public string Employment;
        public string Income;
        public string Home;
        public string HighestDegree;
        public string TaskSpecificInfo;
        

        public WorkerInfoTableEntry(
            int l_Id,
            string l_WorkerId,
            int l_Age,
            string l_Sex,
            string l_Ethnicity,
            string l_Employment,
            string l_Income,
            string l_Home,
            string l_HighestDegree,
            string l_TaskSpecificInfo
            )
        {
            Id = l_Id;
            WorkerId = l_WorkerId;
            Age = l_Age;
            Sex = l_Sex;
            Ethnicity = l_Ethnicity;
            Employment = l_Employment;
            Income = l_Income;
            Home = l_Home;
            HighestDegree = l_HighestDegree;
            TaskSpecificInfo = l_TaskSpecificInfo;
        }
        
    }


    public class WorkerInfoTableAccess
    {
        string TableName = "WorkerInfoTable";
        SatyamAzureSQLDBAccess dbAccess;

        public WorkerInfoTableAccess()
        {
            dbAccess = new SatyamAzureSQLDBAccess();
        }

        public void close()
        {
            dbAccess.close();
        }

        public List<WorkerInfoTableEntry> getEntries(string SQLCommandString)
        {
            List<WorkerInfoTableEntry> ret = new List<WorkerInfoTableEntry>();
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int ID = (int)reader["id"];
                    string WorkerId = (string)reader["WorkerId"];
                    int Age = (int)reader["Age"];
                    string Sex = (string)reader["Sex"];
                    string Ethnicity = (string)reader["Ethnicity"];
                    string Employment = (string)reader["Employment"];
                    string Income = (string)reader["Income"];
                    string Home = (string)reader["Home"];
                    string HighestDegree = (string)reader["HighestDegree"];
                    string TaskSpecificInfo = (string)reader["TaskSpecificInfo"];

                    WorkerInfoTableEntry entry = new WorkerInfoTableEntry(ID, WorkerId, Age, Sex, Ethnicity, Employment,Income, Home, HighestDegree, TaskSpecificInfo);
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

        public List<WorkerInfoTableEntry> getAllEntries()
        {
            string SQLCommandString = "SELECT * FROM " + TableName;
            return getEntries(SQLCommandString);
        }


        public List<WorkerInfoTableEntry> getEntryByWorkerID(string workerID)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE WorkerId = '" + workerID + "'";
            return getEntries(SQLCommandString);
        }


        /***** Adding Entries ********************************************************************************************/

        public bool AddEntry(string WorkerId,
            int Age,
            string Sex,
            string Ethnicity,
            string Employment,
            string Income,
            string Home,
            string HighestDegree,
            string TaskSpecificInfo)
        {
            int noTries = 0;
            bool ret = true;
            bool done = true;
            //int zero = 0;
            do
            {
                String SQLCommandString = "INSERT INTO " + TableName + " (WorkerId,Age,Sex,Ethnicity,Employment,Income,Home,HighestDegree,TaskSpecificInfo) " +
                    "VALUES(@WorkerId,@Age,@Sex,@Ethnicity,@Employment,@Income,@Home,@HighestDegree,@TaskSpecificInfo)";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@WorkerId", WorkerId);
                sqlCommand.Parameters.AddWithValue("@Age", Age.ToString());
                sqlCommand.Parameters.AddWithValue("@Sex", Sex);
                sqlCommand.Parameters.AddWithValue("@Ethnicity", Ethnicity);
                sqlCommand.Parameters.AddWithValue("@Employment", Employment);
                sqlCommand.Parameters.AddWithValue("@Income", Income);
                sqlCommand.Parameters.AddWithValue("@Home", Home);
                sqlCommand.Parameters.AddWithValue("@HighestDegree", HighestDegree);
                sqlCommand.Parameters.AddWithValue("@TaskSpecificInfo", TaskSpecificInfo);
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
    }
}
