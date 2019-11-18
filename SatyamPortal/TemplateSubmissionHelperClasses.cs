using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AzureBlobStorage;

namespace SatyamPortal
{
    public class AzureConnectionInfo
    {
        string ConnectionString;
        string ContainerName;
        string DirectoryName;

        public AzureConnectionInfo(string connectionString, string containerName)
        {
            ConnectionString = connectionString;
            ContainerName = containerName;
            DirectoryName = "";
        }
        public AzureConnectionInfo(string connectionString, string containerName, string directoryName)
        {
            ConnectionString = connectionString;
            ContainerName = containerName;
            DirectoryName = directoryName;
        }

        public int getNoFiles()
        {
            int noFiles = -1;
            BlobContainerManager bcm = new BlobContainerManager();
            string status = bcm.Connect(ConnectionString);
            if (status == "SUCCESS")
            {
                List<string> names = new List<string>();
                if (DirectoryName != "" && DirectoryName != null)
                {
                    names = bcm.getURLList(ContainerName, DirectoryName);
                }
                else
                {
                    names = bcm.getURLList(ContainerName);
                }
                if (names != null)
                {
                    noFiles = names.Count;
                }
            }
            return noFiles;
        }
    }
}