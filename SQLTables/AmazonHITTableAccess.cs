using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Constants;
using Utilities;
using AmazonMechanicalTurkAPI;

namespace SQLTables
{
    public class SatyamAmazonHITTableAccessEntry
    {
        public int ID;
        public string JobTemplateType;
        public string UserID;
        public string HITID;
        public string JobGUID;
        public DateTime CreationTime;
        public String HITParameters;
        public string AmazonAccessKeyID;
        public string AmazonSecretAccessKeyID;
        public string Status;

        public SatyamAmazonHITTableAccessEntry()
        {

        }

        public SatyamAmazonHITTableAccessEntry(int _ID, String _JobType, String _UserID, String _HITID, String _JoBGUID, DateTime _CreationTime, String _HITParameters, string _AccessKey, string _SecretAccessKey, string _Status)
        {
            ID = _ID;
            JobTemplateType = _JobType;
            UserID = _UserID;
            HITID = _HITID;
            JobGUID = _JoBGUID;
            CreationTime = _CreationTime;
            HITParameters = _HITParameters;
            AmazonAccessKeyID = _AccessKey;
            AmazonSecretAccessKeyID = _SecretAccessKey;
            Status = _Status; //pending,expired,unpaid,paid
        }

        public bool EntryExpired()
        {
            Dictionary<string, object> HitParams = JSonUtils.ConvertJSonToObject<Dictionary<string, object>>(HITParameters);
            long lifeTime = AmazonMTurkHIT.Default_lifetimeInSeconds;
            if (HitParams.ContainsKey("AssignmentLifeTime"))
            {
                lifeTime = (long)Convert.ToInt64(HitParams["AssignmentLifeTime"]);
            }

            long LiveSeconds = (long)(DateTime.Now - CreationTime).TotalSeconds;
            if (LiveSeconds > lifeTime)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
    public class SatyamAmazonHITTableAccess
    {
        string TableName = "SatyamAmazonHITTable";
        SatyamAzureSQLDBAccess dbAccess;

        public SatyamAmazonHITTableAccess()
        {
            dbAccess = new SatyamAzureSQLDBAccess();
        }
        public void close()
        {
            dbAccess.close();
        }

        public List<SatyamAmazonHITTableAccessEntry> getEntries(string SQLCommandString)
        {
            List<SatyamAmazonHITTableAccessEntry> ret = new List<SatyamAmazonHITTableAccessEntry>();
            SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
            sqlCommand.CommandTimeout = 200;

            try
            {
                SqlDataReader reader = sqlCommand.ExecuteReader();
                while (reader.Read())
                {
                    int ID = (int)reader["ID"];
                    string JobTemplateType = (string)reader["JobTemplateType"];
                    string UserID = (string)reader["UserID"];
                    string HITID = (string)reader["HITID"];
                    string JobGUID = (string)reader["JobGUID"];
                    DateTime CreationTime = (DateTime)reader["CreationTime"];
                    string HITParameters = (string)reader["HITParameters"];
                    string AmazonAccessKeyID = (string)reader["AmazonAccessKeyID"];
                    string AmazonSecretAccessKeyID = (string)reader["AmazonSecretAccessKeyID"];
                    string Status = (string)reader["Status"];
                    SatyamAmazonHITTableAccessEntry entry = new SatyamAmazonHITTableAccessEntry(ID, JobTemplateType, UserID, HITID, JobGUID, CreationTime, HITParameters,AmazonAccessKeyID,AmazonSecretAccessKeyID,Status);
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

        public List<SatyamAmazonHITTableAccessEntry> getAllEntries()
        {
            string SQLCommandString = "SELECT * FROM " + TableName;
            return getEntries(SQLCommandString);
        }

        public List<SatyamAmazonHITTableAccessEntry> getAllEntriesByJobGUID(string guid)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + guid + "'";
            return getEntries(SQLCommandString);
        }

        public List<SatyamAmazonHITTableAccessEntry> getAllEntriesByID(string id)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE Id = '" + id + "'";
            return getEntries(SQLCommandString);
        }

        public bool EntryExistByID(string id)
        {
            List<SatyamAmazonHITTableAccessEntry> entries = getAllEntriesByID(id);
            if (entries.Count == 0) return false;
            return true;
        }

        public List<SatyamAmazonHITTableAccessEntry> getAllEntriesByStatus(string status)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE Status = '" +status + "'";
            return getEntries(SQLCommandString);
        }

        public List<SatyamAmazonHITTableAccessEntry> getAllEntriesByJobGUIDAndStatus(string guid, string status)
        {
            string SQLCommandString = "SELECT * FROM " + TableName + " WHERE JobGUID = '" + guid + "' AND Status = '" + status + "'";
            return getEntries(SQLCommandString);
        }

        public bool AddEntry(String JobType, String UserID, String HITID, String JobGUID, DateTime CreationTime, String HITParameters, string AmazonAccessKeyID, string AmazonSecretAccessKeyID)
        {
            int noTries = 0;
            bool ret = true;
            bool done = true;
            do
            {
                String SQLCommandString = "INSERT INTO " + TableName + " (JobTemplateType,UserID, HITID,JobGUID,CreationTime, HITParameters,AmazonAccessKeyID,AmazonSecretAccessKeyID,Status) VALUES(@JobTemplateType,@UserID, @HITID,@JobGUID,@CreationTime, @HITParameters,@AmazonAccessKeyID,@AmazonSecretAccessKeyID,@Status)";
                SqlCommand sqlCommand = new SqlCommand(SQLCommandString, dbAccess.getSQLConnection());
                sqlCommand.CommandTimeout = 500;
                sqlCommand.Parameters.AddWithValue("@JobTemplateType", JobType);
                sqlCommand.Parameters.AddWithValue("@UserId", UserID);
                sqlCommand.Parameters.AddWithValue("@HITID", HITID);
                sqlCommand.Parameters.AddWithValue("@JobGUID", JobGUID);
                sqlCommand.Parameters.AddWithValue("@CreationTime", CreationTime.ToString());
                sqlCommand.Parameters.AddWithValue("@HITParameters", HITParameters);
                sqlCommand.Parameters.AddWithValue("@AmazonAccessKeyID", AmazonAccessKeyID);
                sqlCommand.Parameters.AddWithValue("@AmazonSecretAccessKeyID", AmazonSecretAccessKeyID);
                sqlCommand.Parameters.AddWithValue("@Status", HitStatus.pending);
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

        public bool UpdateStatus(int id, string status)
        {
            String SQLCommandString = "UPDATE " + TableName + " SET Status = '" + status + "' WHERE  ID = '" + id + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }

        public bool UpdateStatusByHITID(string hitid, string status)
        {
            String SQLCommandString = "UPDATE " + TableName + " SET Status = '" + status + "' WHERE  HITID = '" + hitid + "'";
            return ExecuteNoQuerySQLCommand(SQLCommandString);
        }
    }
}
