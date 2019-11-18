using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Net;

using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types

using Utilities;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureBlobStorage
{
    public class BlobContainerManager
    {

        CloudStorageAccount storageAccount = null;
        CloudBlobClient blobClient = null;

        //public List<String> urlList = new List<String>();
        Random R = new Random((int)DateTime.Now.Ticks);

        public BlobContainerManager()
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
            catch(Exception)
            {
                return "FAILURE ";
            }           

            blobClient = storageAccount.CreateCloudBlobClient();
            return "SUCCESS";
        }

        public string ConnectDebug(String connectionString)
        {
            // Retrieve storage account from connection string.
            //storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(connectionString));

            try
            {
                string debugString = "About to parse connection string : " + connectionString + "\n";
                debugString += typeof(CloudStorageAccount).ToString();
                return debugString;
                //storageAccount = CloudStorageAccount.Parse(connectionString);
                //return debugString += "About to parse connection string : " + connectionString + "\n";
            }
            catch (SystemException ex)
            {
                return "SYSTEM EXCEPTION " + ex.ToString();
            }
            /*catch (System.ArgumentNullException)
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
            catch(Exception e)
            {
                return "FAILURE ";
            }*/
            //return "succeeded in parsing the string it is : " + storageAccount + "\n";
            //// Create the blob client.
            //blobClient = storageAccount.CreateCloudBlobClient();
            //return "SUCCESS";
        }

        public string getContainerUriString(string containerName)
        {
            // Retrieve a reference to a container.
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);  
            return blobContainer.Uri.ToString();
        }


        public List<String> getURLList(string containerName)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            List<String> urlList = new List<String>();
            //List of urls of all files in the blob
            var listOfBlobs = blobContainer.ListBlobs(useFlatBlobListing: true);
            foreach (var blob in listOfBlobs)
            {
                urlList.Add(blob.Uri.ToString());
            }
            return urlList;
        }

        public List<String> getImmediateNextLevelURLList(string containerName, string DirectoryName)
        {
            return getURLList(containerName, DirectoryName, false);
        }
        public List<String> getURLList(string containerName, string DirectoryName, bool flatList = true)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            CloudBlobDirectory dir = blobContainer.GetDirectoryReference(DirectoryName);
            List<String> urlList = new List<String>();
            try
            {
                List<IListBlobItem> blobs = dir.ListBlobs(useFlatBlobListing: flatList).ToList();
                foreach (var blob in blobs)
                {
                    urlList.Add(blob.Uri.ToString());
                }
            }
            catch(Exception)
            {
                return null;
            }
            
            return urlList;
        }

        public List<String> getURLListOfSpecificExtension(string containerName, string DirectoryName, List<string> extensions)
        {
            List<String> urlList = getURLList(containerName, DirectoryName, flatList: true);
            List<String> returnList = new List<string>();
            foreach (String url in urlList)
            {
                string ext = URIUtilities.fileExtensionFromURI(url);
                foreach(string e in extensions)
                {
                    if (ext.Equals(e, StringComparison.InvariantCultureIgnoreCase))
                    {
                        returnList.Add(url);
                    }
                }
                
            }
            return returnList;
        }

        public List<String> getURLListOfSubDirectoryByURL(string url)
        {
            string[] urlparts = url.Split('/');
            if (urlparts.Length < 4) return null;
            //string containerString = urlparts[2];
            //string containerName = containerString.Split('.')[0];
            string containerName = urlparts[3];
            string directoryName = "";
            for (int i = 4; i < urlparts.Length; i++)
            {
                if (urlparts[i].Length == 0) continue;
                directoryName += urlparts[i];
                if (i == urlparts.Length - 1) break;
                directoryName += '/';
            }
            return getURLList(containerName, directoryName);
        }


        public List<String> getURLListOfSpecificExtensionUnderSubDirectoryByURI(string url, List<string> extensions)
        {
            string[] urlparts = url.Split('/');
            if (urlparts.Length < 4) return null;
            //string containerString = urlparts[2];
            //string containerName = containerString.Split('.')[0];
            string containerName = urlparts[3];
            string directoryName = "";
            for (int i = 4; i < urlparts.Length; i++)
            {
                if (urlparts[i].Length == 0) continue;
                directoryName += urlparts[i];
                if (i == urlparts.Length - 1) break;
                directoryName += '/';
            }
            return getURLListOfSpecificExtension(containerName, directoryName, extensions);
        }


        public void UploadALocalFile(string localPath,string containerName , string blobDirectoryPath)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            var fileStream = System.IO.File.OpenRead(localPath);
            string filename = System.IO.Path.GetFileName(localPath);
            //blobDirectoryPath = "148th and NE 29th avi";
            filename = blobDirectoryPath + "\\" + filename;
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);
            using (fileStream)
            {
                blockBlob.UploadFromStream(fileStream, null, null, null);
            }
        }

        public void UploadALocalFileAsync(string localPath, string containerName, string blobDirectoryPath)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            var fileStream = System.IO.File.OpenRead(localPath);
            string filename = System.IO.Path.GetFileName(localPath);
            //blobDirectoryPath = "148th and NE 29th avi";
            filename = blobDirectoryPath + "\\" + filename;
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(filename);
            using (fileStream)
            {
                blockBlob.UploadFromStreamAsync(fileStream, null, null, null);
            }
        }

        public void UploadFromURI(string uri, string containerName, string directoryName, string filename)
        {
            var webRequest = WebRequest.Create(uri);

            byte[] buffer = new byte[4096];
            byte[] result;

            using (WebResponse response = webRequest.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            using (MemoryStream memoryStream = new MemoryStream())
            {
                int count = 0;
                do
                {
                    count = responseStream.Read(buffer, 0, buffer.Length);
                    memoryStream.Write(buffer, 0, count);

                } while (count != 0);

                result = memoryStream.ToArray();
            }

            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);

            string fullfilename = directoryName + "\\" + filename;
            
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fullfilename);
            blockBlob.UploadFromByteArray(result,0,result.Length);
        }

              
        public void UploadALocalFolder(string localDirectoryPath, string containerName,string blobDirectoryPath)
        {

            //List<String> files = getURLList(containerName);
            //List<String> filenames = new List<String>();
            //foreach (String file in files)
            //{
            //    Uri uri = new Uri(file);
            //    string fname = System.IO.Path.GetFileName(uri.LocalPath);
            //    filenames.Add(fname);
            //}
            //Console.WriteLine("no files = " + files.Count);


            List<string> filesToBeUploaded = Directory.GetFiles(localDirectoryPath, "*").ToList<string>();
            List<string> fileNamesToBeUploaded = new List<string>();
            foreach (String file in filesToBeUploaded)
            {
                string fname = System.IO.Path.GetFileName(file);
                fileNamesToBeUploaded.Add(fname);
            }

            Console.WriteLine("no files = " + fileNamesToBeUploaded.Count);
            Console.WriteLine("firstFile = " + fileNamesToBeUploaded[0]);
            Console.WriteLine("lastFile = " + fileNamesToBeUploaded[fileNamesToBeUploaded.Count - 1]);



            int cnt = 0;
            for (int i = 0; i < filesToBeUploaded.Count; i++)
            {
                //if (!filenames.Contains(fileNamesToBeUploaded[i]))
                //{
                Console.WriteLine("Uploading  " + fileNamesToBeUploaded[i]);
                UploadALocalFile(filesToBeUploaded[i], containerName, blobDirectoryPath);
                cnt++;
                Console.WriteLine(fileNamesToBeUploaded[i] + " uploaded");
                //Environment.Exit(0);
                //}
            }
        }

        public bool ContainsDirectoryName(string directory, string containerName, out string blobDirectory)
        {
            bool ret = false;
            blobDirectory = "";
            //List<string> directories = blobManager.getURLList();

            CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
            var folders = blobContainer.ListBlobs().Where(b => b as CloudBlobDirectory != null).ToList();
            List<string> names = new List<string>();
            foreach (var folder in folders)
            {
                string[] layers = folder.Uri.LocalPath.Split('/');
                string prefix = layers[layers.Length - 2];
                //Console.WriteLine(prefix);
                string[] fields = prefix.Split(new[] { "--" }, StringSplitOptions.None);
                string name = "";
                for (int i = 1; i < fields.Length - 1; i++)
                {
                    name += fields[i] + "--";
                }
                name += fields[fields.Length - 1];
                //Console.WriteLine(name);
                if (name == directory)
                {
                    ret = true;
                    int idx = names.IndexOf(directory);
                    blobDirectory = prefix;
                    break;
                }
            }
            return ret;
        }


        public static void UploadFileFromRemoteFolder(BlobContainerManager source, string sourceContainerName, string sourceDirectoryName, BlobContainerManager destination, string destinationContainerName, string destinationDirectoryName)
        {
            List<String> filesToBeCopied = source.getURLList(sourceContainerName, sourceDirectoryName);

            foreach(string uri in filesToBeCopied)
            {
               
                string fileName = URIUtilities.filenameFromURI(uri);
                destination.UploadFromURI(uri, destinationContainerName, destinationDirectoryName, fileName);
                //destination.UploadFromURI(uri, destinationContainerName, destinationDirectoryName, fileName);
            }
        }

        public static void UploadFileFromRemoteFolder(BlobContainerManager source, string sourceContainerName, string sourceDirectoryName, BlobContainerManager destination, string destinationContainerName, string destinationDirectoryName, List<string> fileTypes)
        {
            List<String> filesToBeCopied = source.getURLList(sourceContainerName, sourceDirectoryName);

            foreach (string uri in filesToBeCopied)
            {

                string fileName = URIUtilities.filenameFromURI(uri);
                if (CheckFileExtensions.IsAllowedType(fileName,fileTypes))
                {
                    destination.UploadFromURI(uri, destinationContainerName, destinationDirectoryName, fileName);
                }
            }
        }


        public void SaveATextFile(string ContainerName, string DirectoryName, string FileName, string dataToBeSaved)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(ContainerName);
            string fileName = "";
            if(DirectoryName != null && DirectoryName != "")
            {
                fileName = DirectoryName + "\\" + FileName;
            }
            else
            {
                fileName = FileName;
            }
            
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(fileName);
            blockBlob.UploadText(dataToBeSaved);
        }

    }
}
