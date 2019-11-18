using System;
using System.Collections.Generic;
using System.Linq;


namespace Utilities
{
    public static class URIUtilities
    {
        public static string localDirectoryNameFromURI(string uri)
        {
            string[] parts = uri.Split('/');
            string fileName = "";

            if (parts.Length > 1)
            {   
                fileName = parts[parts.Length - 2];                
            }                
            else
                fileName = uri;

            return fileName;
        }

        public static string localDirectoryNameFromDirectory(string uri)
        {
            string[] parts = uri.Split('\\');
            string fileName = "";

            if (parts.Length > 1)
            {
                fileName = parts[parts.Length - 2];
            }
            else
                fileName = uri;

            return fileName;
        }


        public static string localDirectoryFullPathFromURI(string uri)
        {
            string[] parts = uri.Split('/');
            string fileName = "";

            if (parts.Length > 1)
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    fileName += parts[i];
                    fileName += '/';
                }
            }

            return fileName;
        }

        public static string localDirectoryFullPathFromDirectory(string uri)
        {
            string[] parts = uri.Split('\\');
            string fileName = "";

            if (parts.Length > 1)
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    fileName += parts[i];
                    fileName += '/';
                }
            }

            return fileName;
        }

        public static string filenameFromURI(string uri)
        {
            string[] parts = uri.Split('/');
            string fileName = "";

            if (parts.Length > 0)
                fileName = parts[parts.Length - 1];
            else
                fileName = uri;

            return fileName;
        }

        public static string filenameFromDirectory(string uri)
        {
            string[] parts = uri.Split('\\');
            string fileName = "";

            if (parts.Length > 0)
                fileName = parts[parts.Length - 1];
            else
                fileName = uri;

            return fileName;
        }

        public static string filenameFromURINoExtension(string uri)
        {
            string fileName = filenameFromURI(uri);
            string[] parts = fileName.Split('.');
            string fileNameNoExtension = "";
            if (parts.Length > 0)
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    fileNameNoExtension += parts[i];
                    if (i + 1 == parts.Length - 1) break;
                    fileNameNoExtension += ".";
                }
                return fileNameNoExtension;
            }
            else
            {
                return fileName;
            }
        }

        public static string filenameFromDirectoryNoExtension(string uri)
        {
            string fileName = filenameFromDirectory(uri);
            string[] parts = fileName.Split('.');
            string fileNameNoExtension = "";
            if (parts.Length > 0)
            {
                for (int i = 0; i < parts.Length - 1; i++)
                {
                    fileNameNoExtension += parts[i];
                    if (i+1 == parts.Length - 1) break;
                    fileNameNoExtension += ".";
                }
                return fileNameNoExtension;
            }
            else
            {
                return fileName;
            }            
        }

        public static string fileExtensionFromURI(string uri)
        {
            string fileName = filenameFromURI(uri);
            string[] parts = fileName.Split('.');
            string fileExtension = "";
            if (parts.Length > 1)
            {
                fileExtension = parts[parts.Length - 1];
            }
            return fileExtension;
        }

        public static string fileExtensionFromDirectory(string uri)
        {
            string fileName = filenameFromDirectory(uri);
            string[] parts = fileName.Split('.');
            string fileExtension = "";
            if (parts.Length > 1)
            {
                fileExtension = parts[parts.Length - 1];
            }
            return fileExtension;
        }


        public static Dictionary<string, string> getURIAttributes(string uri)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Uri bUri = new Uri(uri);
            string query = bUri.Query.Replace("?", "");
            if (query != "")
            {
                dict = query.Split('&').Select(q => q.Split('=')).ToDictionary(k => k[0], v => v[1]);
            }
            return dict;
        }

        

    }
}
