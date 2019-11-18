using Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobStorage
{
    public class SatyamJobStorageAccountAccess
    {
        public static string connection_string = ConfigConstants.AZURE_STORAGE_CONNECTION_STRING;
        BlobContainerManager containerManager;

        public SatyamJobStorageAccountAccess()
        {
            containerManager = new BlobContainerManager();
            string status = containerManager.Connect(connection_string);
        }

        public List<String> getURLList(string containerName)
        {
            return containerManager.getURLList(containerName);
        }

        public List<String> getURLList(string containerName, string directoryName, bool flat = true)
        {
            return containerManager.getURLList(containerName, directoryName, flat);
        }

        public List<String> getImmediateNextLevelURLList(string containerName, string directoryName)
        {
            return containerManager.getURLList(containerName, directoryName, false);
        }

        public List<String> getURLListOfSpecificExtension(string containerName, string directoryName, List<string> extension)
        {
            return containerManager.getURLListOfSpecificExtension(containerName, directoryName, extension);
        }

        public List<String> getURLListOfSubDirectoryByURL(string url)
        {
            return containerManager.getURLListOfSubDirectoryByURL(url);
        }

        public List<String> getURLListOfSpecificExtensionUnderSubDirectoryByURI(string url, List<string> extensions)
        {
            return containerManager.getURLListOfSpecificExtensionUnderSubDirectoryByURI(url, extensions);
        }

        public void copyFilesFromAnotherAzureBlob(BlobContainerManager b, string sourceContainerName, string sourceDirectoryName, string destinationContainerName, string destinationDirectoryName)
        {
            BlobContainerManager.UploadFileFromRemoteFolder(b, sourceContainerName, sourceDirectoryName, containerManager, destinationContainerName, destinationDirectoryName);
        }

        public void copyFilesFromAnotherAzureBlob(BlobContainerManager b, string sourceContainerName, string sourceDirectoryName, string destinationContainerName, string destinationDirectoryName,List<string> fileTypes)
        {
            BlobContainerManager.UploadFileFromRemoteFolder(b, sourceContainerName, sourceDirectoryName, containerManager, destinationContainerName, destinationDirectoryName,fileTypes);
        }

        public void copyFileFromAnotherAzureBlob(string uri, string containerName, string directoryName, string filename)
        {
            containerManager.UploadFromURI(uri, containerName, directoryName, filename);
        }

        public void SaveATextFile(string ContainerName, string DirectoryName, string FileName, string dataToBeSaved)
        {
            containerManager.SaveATextFile(ContainerName, DirectoryName, FileName, dataToBeSaved);
        }

        public void uploadALocalFolder(string localFolder, string containerName, string directoryName)
        {
            containerManager.UploadALocalFolder(localFolder, containerName, directoryName);
        }

        public void UploadALocalFile(string localPath, string containerName, string blobDirectoryPath)
        {
            containerManager.UploadALocalFile(localPath, containerName, blobDirectoryPath);
        }
    }
}
