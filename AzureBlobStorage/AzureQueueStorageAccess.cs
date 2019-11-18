using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStorage
{
    
    public class QueueManager
    {

        CloudStorageAccount storageAccount = null;
        CloudQueueClient queueClient = null;

        //public List<String> urlList = new List<String>();
        Random R = new Random((int)DateTime.Now.Ticks);

        public QueueManager()
        {

        }

        public string Connect(String connectionString)
        {
            // Retrieve storage account from connection string.
            try
            {
                storageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch (System.ArgumentNullException)
            {
                return "CONNECTION_STRING_NULL_EMPTY";
            }
            catch (System.FormatException)
            {
                return "CONNECTION_STRING_FORMAT_INVALID";
            }
            catch (System.ArgumentException)
            {
                return "CONNECTION_STRING_NOT_PARSE";
            }
            catch (Exception)
            {
                return "FAILURE ";
            }

            queueClient = storageAccount.CreateCloudQueueClient();
            return "SUCCESS";
        }


        public void Enqueue(string QueueName, string m)
        {
            CloudQueue queue = queueClient.GetQueueReference(QueueName);
            queue.CreateIfNotExists();
            CloudQueueMessage message = new CloudQueueMessage(m);
            queue.AddMessage(message);

        }

        public string Peek(string QueueName)
        {
            // Retrieve a reference to a queue
            CloudQueue queue = queueClient.GetQueueReference(QueueName);

            // Peek at the next message
            CloudQueueMessage peekedMessage = queue.PeekMessage();

            // Display message.
            return(peekedMessage.AsString);

        }

        public string Dequeue(string QueueName)
        {
            // Retrieve a reference to a queue
            CloudQueue queue = queueClient.GetQueueReference(QueueName);

            // Get the next message
            CloudQueueMessage retrievedMessage = queue.GetMessage();

            //Process the message in less than 30 seconds, and then delete the message
            queue.DeleteMessage(retrievedMessage);

            return retrievedMessage.AsString;
        }
    }
}
