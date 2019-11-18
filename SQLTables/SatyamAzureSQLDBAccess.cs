using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Constants;

namespace SQLTables
{
    public class SatyamAzureSQLDBAccess 
    {
        static string connection_string = ConfigConstants.AZURE_SQL_DATABASE_CONNECTION_STRING;
        AzureSQLDBAccess azureSQlDBAccess;

        public SatyamAzureSQLDBAccess()
        {
            azureSQlDBAccess = new AzureSQLDBAccess(connection_string);
        }

        public SqlConnection getSQLConnection()
        {
            return azureSQlDBAccess.getSQLConnection();
        }

        public void close()
        {
            azureSQlDBAccess.close();
        }
    }
}
