using SQLTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace SQLTableManagement
{
    public class WorkerInfoManagement
    {
        public static void SaveAllWorkerInfo(string filepath)
        {
            StreamWriter sw = new StreamWriter(filepath);

            WorkerInfoTableAccess wita = new WorkerInfoTableAccess();
            List<WorkerInfoTableEntry> infos = wita.getAllEntries();
            //List<string> workerinfos = new List<string>();
            foreach(WorkerInfoTableEntry i in infos)
            {
                string info = JSonUtils.ConvertObjectToJSon(i);
                //workerinfos.Add(info);
                sw.WriteLine(info);
            }
            sw.Close();
        }
    }
}
