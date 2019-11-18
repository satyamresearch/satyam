using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStorage
{
    public class SatyamDispatchStorageAccountAccess
    {
        public static string connection_string = ConfigConstants.AZURE_STORAGE_CONNECTION_STRING; // using the same storage account
        QueueManager queueManager;

        public SatyamDispatchStorageAccountAccess()
        {
            queueManager = new QueueManager();
            string status = queueManager.Connect(connection_string);
        }

        public void Enqueue(string QueueName, string m)
        {
            queueManager.Enqueue(QueueName, m);
        }
        public string Peek(string QueueName)
        {
            return queueManager.Peek(QueueName);
        }
        public string Dequeue(string QueueName)
        {
            return queueManager.Dequeue(QueueName);
        }
    }
}
