using System;
using System.Collections.Generic;
using System.Linq;
using AmazonMechanicalTurkAPI;
using AzureBlobStorage;
using Constants;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using SatyamTaskGenerators;
using SatyamTaskResultClasses;
using SQLTableManagement;
using SQLTables;
using Utilities;

namespace SatyamDispatch
{
    public static class PaymentDispatch
    {
        [FunctionName("PaymentDispatch")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            DateTime start = DateTime.Now;
            bool logging = false;
            if (logging) log.Info($"Payment Dispatch executed at: {DateTime.Now}");
            SatyamDispatchStorageAccountAccess satyamQueue = new SatyamDispatchStorageAccountAccess();

            //get IDs of all accepted and rejected results related to MTurk
            Dictionary<string, string> AssignmentIDToHITIDMap = new Dictionary<string, string>();
            List<SatyamResultsTableEntry> acceptedEntries = new List<SatyamResultsTableEntry>();
            foreach (string taskTemplate in TaskConstants.MTurkTaskTemplates)
            {
                SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
                List<SatyamResultsTableEntry> entries = resultsDB.getAllEntriesByJobtemplateTypeAndStatus(taskTemplate, ResultStatus.accepted);
                resultsDB.close();
                acceptedEntries.AddRange(entries);
            }

            List<SatyamResultsTableEntry> rejectedEntries = new List<SatyamResultsTableEntry>();
            foreach (string taskTemplate in TaskConstants.MTurkTaskTemplates)
            {
                SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
                List<SatyamResultsTableEntry> entries = resultsDB.getAllEntriesByJobtemplateTypeAndStatus(taskTemplate, ResultStatus.rejected);
                resultsDB.close();
                rejectedEntries.AddRange(entries);
            }
            

            if (acceptedEntries.Count == 0 && rejectedEntries.Count == 0) //nothing to do
            {
                return;
            }

            // a single assignment may have multiple accepted results
            Dictionary<string, List<SatyamResultsTableEntry>> acceptedResultsByAssignmentID = new Dictionary<string, List<SatyamResultsTableEntry>>();
            foreach (SatyamResultsTableEntry entry in acceptedEntries)
            {
                SatyamResult sr = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);

                string AssignmentID = sr.amazonInfo.AssignmentID;
                if (!AssignmentIDToHITIDMap.ContainsKey(AssignmentID))
                {
                    string HITID = sr.amazonInfo.HITID;
                    AssignmentIDToHITIDMap.Add(AssignmentID, HITID);
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
                    if (!assignmentIDList.Contains(AssignmentID))
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
                    if (r == null)
                    {
                        entry = rejectedResultsByAssignmentID[assignmentID][0];
                        r = JSonUtils.ConvertJSonToObject<SatyamResult>(entry.ResultString);
                    }
                }
                double ratio = (double)noAccepted / ((double)noAccepted + (double)noRejected);

                SatyamTask task = JSonUtils.ConvertJSonToObject<SatyamTask>(r.TaskParametersString);

                string AmazonAccessKeyID = task.jobEntry.amazonHITInformation.AmazonAccessKeyID;
                string AmazonSecretAccessKeyID = task.jobEntry.amazonHITInformation.AmazonSecretAccessKeyID;

                string m = AmazonAccessKeyID + "_" + AmazonSecretAccessKeyID + "_" + assignmentID;

                SatyamResultsTableAccess resultsDB = new SatyamResultsTableAccess();
                
                if (ratio >= AmazonMTurkPayments.assignement_acceptance_threshold) //this is acceptable
                {

                    if (logging) log.Info($"Dispatching Payment for {assignmentID}");
                    string queueName = "payment";                    
                    satyamQueue.Enqueue(queueName, m);

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
                            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();
                            hitDB.UpdateStatusByHITID(AssignmentIDToHITIDMap[assignmentID], HitStatus.accepted);
                            hitDB.close();
                        }
                    }

                }
                else
                {
                    if (logging) log.Info($"Dispatching NoPayment for {assignmentID}");
                    string queueName = "nopayment";
                    satyamQueue.Enqueue(queueName, m);

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
                            SatyamAmazonHITTableAccess hitDB = new SatyamAmazonHITTableAccess();
                            hitDB.UpdateStatusByHITID(AssignmentIDToHITIDMap[assignmentID], HitStatus.rejected);
                            hitDB.close();
                        }
                    }
                }

                if ((DateTime.Now - start).TotalSeconds > 280) break;
            }
        }
    }
}
