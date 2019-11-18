using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobTemplateClasses
{
    public class AzureInformation
    {
        public string AzureBlobStorageConnectionString;
        public string AzureBlobStorageContainerName;
        public string AzureBlobStorageContainerDirectoryName;
    }

    public class AmazonMTurkHITInformation
    {
        public string AmazonAccessKeyID;
        public string AmazonSecretAccessKeyID;
        public double Price;
        public string AmazonMTurkTaskTitle;
        public string AmazonMTurkTaskDescription;
        public string AmazonMTurkTaskKeywords;

        public AmazonMTurkHITInformation()
        {
            AmazonAccessKeyID = "";
            AmazonMTurkTaskDescription = "";
            AmazonMTurkTaskKeywords = "";
            AmazonMTurkTaskTitle = "";
            AmazonSecretAccessKeyID = "";
            Price = 0;
        }
    }

    public class SatyamJob
    {
        public AzureInformation azureInformation;
        public AmazonMTurkHITInformation amazonHITInformation;
        public string JobGUIDString;
        public string JobTemplateType;
        public string UserID;
        public DateTime JobSubmitTime;
        public string JobParameters;//this is a JsonString  
        public int TasksPerJob;
        public bool AdaptivePricing;
        public double TargetPricePerTask;
        public double TotalBudget;

    }
}
