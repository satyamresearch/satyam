using Constants;
using JobTemplateClasses;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SatyamAnalysis
{
    public class MiscTaskAnalyzer
    {
        public static void SaveWholeSatyamResultText(string jobGUID)
        {
            string directoryName = DirectoryConstants.defaultTempDirectory + "\\" + jobGUID + "\\";
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            resultsDB.close();

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            directoryName = directoryName + "\\Raw\\";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                SatyamResultsTableEntry entry = entries[i];
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                SatyamJob job = task.jobEntry;

                string result = satyamResult.TaskResult;


                string ofilename = URIUtilities.filenameFromURI(task.SatyamURI);
                string[] fields = ofilename.Split('.');
                string fileName = "";
                for (int j = 0; j < fields.Count(); j++)
                {
                    fileName += fields[j];
                    if (j == fields.Count() - 1) break;
                    fileName += ".";
                }


                //string fileName = ofilename;
                fileName = fileName + "-Result";

                fileName = fileName + "-" + entry.ID;

                Console.WriteLine("Saving " + fileName);

                string resultFile = directoryName + fileName + ".txt";
                StreamWriter f = new System.IO.StreamWriter(resultFile);
                f.WriteLine(entry.ResultString);
                f.Close();
            }

        }

        public static void SaveResultOnlyText(string jobGUID)
        {
            string directoryName = DirectoryConstants.defaultTempDirectory + "\\" + jobGUID + "\\";
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
            List<SatyamResultsTableEntry> entries = resultsDB.getEntriesByGUID(jobGUID);
            resultsDB.close();

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            directoryName = directoryName + "\\Raw\\";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                SatyamResultsTableEntry entry = entries[i];
                SatyamResult satyamResult = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(satyamResult.TaskParametersString);
                SatyamJob job = task.jobEntry;

                string result = satyamResult.TaskResult;
                

                string ofilename = URIUtilities.filenameFromURI(task.SatyamURI);
                string[] fields = ofilename.Split('.');
                string fileName = "";
                for (int j = 0; j < fields.Count(); j++)
                {
                    fileName += fields[j];
                    if (j == fields.Count() - 1) break;
                    fileName += ".";
                }


                //string fileName = ofilename;
                fileName = fileName + "-Result";

                fileName = fileName + "-" + entry.ID;

                Console.WriteLine("Saving " + fileName);

                string resultFile = directoryName + fileName + ".txt";
                StreamWriter f = new System.IO.StreamWriter(resultFile);
                f.WriteLine(result);
                f.Close();
            }
            
        }
    }
}
