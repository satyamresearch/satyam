using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace SQLTables
{
    public class AzureSQLDBAccess
    {
        public SqlConnection sql_connection = null;
        string Connection_String = null;

        public SqlConnection getSQLConnection()
        {
            return sql_connection;
        }

        public AzureSQLDBAccess(string _connection_string)
        {
            Connection_String = _connection_string;
            if (getSQLConnection() == null && Connection_String != null)
            {
                try
                {
                    sql_connection = new SqlConnection(Connection_String);
                    sql_connection.Open();
                }
                catch (SqlException ex)
                {
                    throw ex;
                }
            }
        }

        public void close()
        {
            sql_connection.Close();
        }
    }
}
