using SQLTables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLTableManagement
{
    public class SatyamAggregatedResultManagement
    {
        public static Dictionary<string, Dictionary<int, SatyamAggregatedResultsTableEntry>> getAggregatedEntriesPerTaskByGuidList(List<string> guids)
        {
            SatyamAggregatedResultsTableAccess aggDB = new SatyamAggregatedResultsTableAccess();
            Dictionary<string, Dictionary<int, SatyamAggregatedResultsTableEntry>> aggEntriesPerTaskPerGUID = new Dictionary<string, Dictionary<int, SatyamAggregatedResultsTableEntry>>();
            foreach (string guid in guids)
            {
                if (!aggEntriesPerTaskPerGUID.ContainsKey(guid))
                {
                    aggEntriesPerTaskPerGUID.Add(guid, new Dictionary<int, SatyamAggregatedResultsTableEntry>());
                }
                //List<SatyamAggregatedResultsTableEntry> aggEntriesPerGUID = aggDB.getEntriesByGUID(guid);
                List<SatyamAggregatedResultsTableEntry> aggEntriesPerGUID = aggDB.getEntriesByGUIDOrderByResultsAggregatedDesc(guid);
                
                foreach (SatyamAggregatedResultsTableEntry aggEntry in aggEntriesPerGUID)
                {
                    if (!aggEntriesPerTaskPerGUID[guid].ContainsKey(aggEntry.SatyamTaskTableEntryID))
                    {
                        aggEntriesPerTaskPerGUID[guid].Add(aggEntry.SatyamTaskTableEntryID, aggEntry);
                    }
                    
                }
            }
            aggDB.close();
            return aggEntriesPerTaskPerGUID;
        }
    }
}
