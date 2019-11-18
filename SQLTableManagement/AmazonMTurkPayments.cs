using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLTables;
using Constants;
using AmazonMechanicalTurkAPI;
using SatyamTaskResultClasses;
using Utilities;
using SatyamTaskGenerators;

namespace SQLTableManagement
{
    public static class AmazonMTurkPayments
    {
        //when there are multiple results in each assignment
        //it should only be paid if over 90% have been done correctly
        public static double assignement_acceptance_threshold = 0.9;

        //go through all unpaid results
        //check if aggregated results has an entry for this
        //check if it satisfies acceptance criterion
        //pay if passes else fail!
        public static void MakePayments()
        {

            //get IDs of all accepted and rejected results related to MTurk
            SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();

            Dictionary<string, string> AssignmentIDToHITIDMap = new Dictionary<string, string>();

            List<SatyamResultsTableEntry> acceptedEntries = new List<SatyamResultsTableEntry>();
            foreach (string taskTemplate in TaskConstants.MTurkTaskTemplates)
            {
                List<SatyamResultsTableEntry> entries = resultsDB.getAllEntriesByJobtemplateTypeAndStatus(taskTemplate,ResultStatus.accepted);
                acceptedEntries.AddRange(entries);
            }

            List<SatyamResultsTableEntry> rejectedEntries = new List<SatyamResultsTableEntry>();
            foreach (string taskTemplate in TaskConstants.MTurkTaskTemplates)
            {
                List<SatyamResultsTableEntry> entries = resultsDB.getAllEntriesByJobtemplateTypeAndStatus(taskTemplate, ResultStatus.rejected);
                rejectedEntries.AddRange(entries);
            }
            resultsDB.close();

            if (acceptedEntries.Count ==0 && rejectedEntries.Count == 0) //nothing to do
            {    
                return;
            }

            // a single assignment may have multiple accepted results
            Dictionary<string, List<SatyamResultsTableEntry>> acceptedResultsByAssignmentID = new Dictionary<string, List<SatyamResultsTableEntry>>();
            foreach (SatyamResultsTableEntry entry in acceptedEntries)
            {
                SatyamResult sr = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                
                string AssignmentID = sr.amazonInfo.AssignmentID;
                if(!AssignmentIDToHITIDMap.ContainsKey(AssignmentID))
                {
                    string HITID = sr.amazonInfo.HITID;
                    AssignmentIDToHITIDMap.Add(AssignmentID,HITID);
                }

                if (!acceptedResultsByAssignmentID.ContainsKey(AssignmentID))
                {
                    acceptedResultsByAssignmentID.Add(AssignmentID, new List<SatyamResultsTableEntry>());
                }
                acceptedResultsByAssignmentID[AssignmentID].Add(entry);
            }

            List<string> assignmentIDList = acceptedResultsByAssignmentID.Keys.ToList();

            // a single assignment may have multiple rejected results
            Dictionary<string, List<SatyamResultsTableEntry>> rejectedResultsByAssignmentID = new Dictionary<string, List<SatyamResultsTableEntry>>();
            foreach (SatyamResultsTableEntry entry in rejectedEntries)
            {
                SatyamResult sr = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                string AssignmentID = sr.amazonInfo.AssignmentID;

                if (!AssignmentIDToHITIDMap.ContainsKey(AssignmentID))
                {
                    string HITID = sr.amazonInfo.HITID;
                    AssignmentIDToHITIDMap.Add(AssignmentID, HITID);
                }

                if (!rejectedResultsByAssignmentID.ContainsKey(AssignmentID))
                {
                    rejectedResultsByAssignmentID.Add(AssignmentID, new List<SatyamResultsTableEntry>());
                    if(!assignmentIDList.Contains(AssignmentID))
                    {
                        assignmentIDList.Add(AssignmentID);
                    }
                }
                rejectedResultsByAssignmentID[AssignmentID].Add(entry);
                
            }

            List<string> rejectedHITs = new List<string>();
            List<string> acceptedHITs = new List<string>();

            //for each assignment ID we check if it should be paid or not and then pay or reject them.
            //pay all accepted entries
            AmazonMTurkHIT hit = new AmazonMTurkHIT();
            foreach (string assignmentID in assignmentIDList)
            {
                int noAccepted = 0;
                SatyamResult r = null;
                SatyamResultsTableEntry entry = null;
                if (acceptedResultsByAssignmentID.ContainsKey(assignmentID))
                {
                    noAccepted = acceptedResultsByAssignmentID[assignmentID].Count;
                    entry = acceptedResultsByAssignmentID[assignmentID][0];
                    r = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                }
                int noRejected = 0;
                if (rejectedResultsByAssignmentID.ContainsKey(assignmentID))
                {
                    noRejected = rejectedResultsByAssignmentID[assignmentID].Count;
                    if(r==null)
                    {
                        entry = rejectedResultsByAssignmentID[assignmentID][0];
                        r = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    }
                }
                double ratio = (double)noAccepted / ((double)noAccepted + (double)noRejected);
                
                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(r.TaskParametersString);

                string AmazonAccessKeyID = task.jobEntry.amazonHITInformation.AmazonAccessKeyID;
                string AmazonSecretAccessKeyID = task.jobEntry.amazonHITInformation.AmazonSecretAccessKeyID;

                hit.setAccount(AmazonAccessKeyID, AmazonSecretAccessKeyID, false);

                resultsDB = new SatyamResultsTableAccess();
                if (ratio >= assignement_acceptance_threshold) //this is acceptable
                {

                    hit.ApproveAssignment(assignmentID, "Great Job! Your work was within acceptable parameters!");
                    if (acceptedResultsByAssignmentID.ContainsKey(assignmentID))
                    {
                        foreach (SatyamResultsTableEntry result in acceptedResultsByAssignmentID[assignmentID])
                        {
                            resultsDB.UpdateStatusByID(result.ID, ResultStatus.accepted_Paid);
                        }
                    }
                    if (rejectedResultsByAssignmentID.ContainsKey(assignmentID))
                    {
                        foreach (SatyamResultsTableEntry result in rejectedResultsByAssignmentID[assignmentID])
                        {
                            resultsDB.UpdateStatusByID(result.ID, ResultStatus.rejected_Paid);
                        }
                    }
                    if (AssignmentIDToHITIDMap.ContainsKey(assignmentID))
                    {
                        if (!acceptedHITs.Contains(AssignmentIDToHITIDMap[assignmentID]))
                        {
                            acceptedHITs.Add(AssignmentIDToHITIDMap[assignmentID]);
                        }
                    }
                }
                else
                {
                    hit.RejectAssignment(assignmentID, "Sorry! Your work was not within acceptable parameters!");
                    if (acceptedResultsByAssignmentID.ContainsKey(assignmentID))
                    {
                        foreach (SatyamResultsTableEntry result in acceptedResultsByAssignmentID[assignmentID])
                        {
                            resultsDB.UpdateStatusByID(result.ID, ResultStatus.accepted_NotPaid);
                        }
                    }
                    if (rejectedResultsByAssignmentID.ContainsKey(assignmentID))
                    {
                        foreach (SatyamResultsTableEntry result in rejectedResultsByAssignmentID[assignmentID])
                        {
                            resultsDB.UpdateStatusByID(result.ID, ResultStatus.rejected_NotPaid);
                        }
                    }
                    if (AssignmentIDToHITIDMap.ContainsKey(assignmentID))
                    {
                        if (!rejectedHITs.Contains(AssignmentIDToHITIDMap[assignmentID]))
                        {
                            rejectedHITs.Add(AssignmentIDToHITIDMap[assignmentID]);
                        }
                    }
                }
                resultsDB.close();
            }

            //update the status of all finished HITs in the hittable
            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();
            foreach(string HITID in acceptedHITs)
            {
                hitDB.UpdateStatusByHITID(HITID, HitStatus.accepted);
            }

            foreach (string HITID in rejectedHITs)
            {
                hitDB.UpdateStatusByHITID(HITID, HitStatus.rejected);
            }
            hitDB.close();
        }
    }
}
