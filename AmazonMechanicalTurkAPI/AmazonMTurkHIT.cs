using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.WebServices.MechanicalTurk;
using Amazon.WebServices.MechanicalTurk.Domain;
using Constants;

namespace AmazonMechanicalTurkAPI
{

    //API link
    //http://docs.aws.amazon.com/AWSMechTurk/latest/AWSMturkAPI/amt-API.pdf
    //Working With HITS "http://docs.aws.amazon.com/AWSMechTurk/latest/AWSMechanicalTurkRequester/Concepts_HITsArticle.html"

    /*
    The HIT properties that define a HIT type are the following: 
    •Title
    •Description
    •Keywords
    •Reward
    •AssignmentDurationInSeconds
    •AutoApprovalDelayInSeconds
    •A set of zero or more QualificationRequirements
    */

    /*  HITs of the same HIT type can have differing values for the following properties: 
       •Question
       •MaxAssignments
       •LifetimeInSeconds
       •RequesterAnnotation
     */


    public class AmazonMTurkHIT
   {

        public static string MechanicalTurk_ServiceEndPointProduction = "https://mechanicalturk.amazonaws.com?Service=AWSMechanicalTurkRequester";
        public static string MechanicalTurk_ServiceEndPointSandBox = "https://mechanicalturk.sandbox.amazonaws.com?Service=AWSMechanicalTurkRequester";


        //defualt values
        public static int Default_FrameHeight = 900;
        public static int Default_MaxAssignments = 1;
        public static string Default_HitType = null;
        public static string Default_KeyWords = "";
        public static decimal Default_Reward = 0;
        public static long Default_assignmentDurationInSeconds = 3600; //60 mins
        public static long Default_autoApprovalDelayInSeconds = 864000*3; //30 days
        public static long Default_lifetimeInSeconds = 86400*5; //5 days
        public static string Default_requesterAnnotation = "DefaultRequesterAnnotation";
        public static string[] Default_ResponseGroup = null;

        public bool sandBox=false;
        SimpleClient client;

        public AmazonMTurkHIT()
        {
            //MTurkConfig config = null;            
        }

        public bool setAccount(string accessKeyId, string secretAccessKey, bool l_sandbox)
        {
            sandBox = l_sandbox;
            MTurkConfig config = null;
            if (sandBox)
            {
                try
                {
                    config = new MTurkConfig(MechanicalTurk_ServiceEndPointSandBox, accessKeyId, secretAccessKey);
                }catch(Exception)
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    config = new MTurkConfig(MechanicalTurk_ServiceEndPointProduction, accessKeyId, secretAccessKey);
                }catch(Exception)
                {
                    return false;
                }
            }

            try
            {
                client = new SimpleClient(config);
            }catch(Exception)
            {
                return false;
            }
            return true;
        }

        public HIT CreateExternalHIT(string url, Dictionary<string,object> HitParams)
        {
            
            String HitType = Default_HitType;

            String title = (string)HitParams["Title"];
            String description = (string)HitParams["TaskDescription"];

            String keywords = Default_KeyWords;
            if (HitParams.ContainsKey("KeyWords"))
            {
                keywords = (string)HitParams["KeyWords"];
            }

            /**************************External Question******************/
            ExternalQuestion myExtQuestion = new ExternalQuestion();
            myExtQuestion.ExternalURL = url;
            if (HitParams.ContainsKey("FrameHeight"))
            {
                myExtQuestion.FrameHeight = HitParams["FrameHeight"].ToString();
            }
            else
            {
                myExtQuestion.FrameHeight = Default_FrameHeight.ToString();
            }
            /*********************************************************/

            decimal reward = Default_Reward;
            if(HitParams.ContainsKey("Reward"))
            {
                reward = Convert.ToDecimal((double)HitParams["Reward"]);
            }

            long assignmentDuration = Default_assignmentDurationInSeconds;
            if(HitParams.ContainsKey("AssignmentDuration"))
            {
                assignmentDuration = (long)Convert.ToInt64(HitParams["AssignmentDuration"]);
            }

            long autoApporvalTime = Default_autoApprovalDelayInSeconds;
            if (HitParams.ContainsKey("AutoApprovalTime"))
            {
                autoApporvalTime = (long)Convert.ToInt64(HitParams["AutoApprovalTime"]);
            }

            long lifeTime = Default_lifetimeInSeconds;
            if (HitParams.ContainsKey("AssignmentLifeTime"))
            {
                lifeTime = (long)Convert.ToInt64(HitParams["AssignmentLifeTime"]);
            }


            int maxAssignments = Default_MaxAssignments;
            if (HitParams.ContainsKey("MaxHITs"))
            {
                maxAssignments = (int)HitParams["MaxHITs"];
            }

            String requesterAnnotation = Default_requesterAnnotation;
            if (HitParams.ContainsKey("RequesterAnnotation"))
            {
                requesterAnnotation = (string)HitParams["RequesterAnnotation"];
            }

            /***************Qualifications***************************************/

            List<QualificationRequirement> qualReqs = new List<QualificationRequirement>();


            if (HitParams.ContainsKey("Country"))
            {
                //setting the qualification requirements
                QualificationRequirement qualReq = new QualificationRequirement();
                qualReq.QualificationTypeId = MTurkSystemQualificationTypes.LocaleQualification; // locale system qual identifier                
                Locale country = new Locale();
                country.Country = "US";
                qualReq.LocaleValue = country;
                qualReq.Comparator = Comparator.EqualTo;
                qualReqs.Add(qualReq);
            }

            if (HitParams.ContainsKey("Masters"))
            {
                QualificationRequirement qualReq1 = new QualificationRequirement();
                if (sandBox)
                {
                    qualReq1.QualificationTypeId = "2ARFPLSP75KLA8M8DH1HTEQVJT3SY6"; // masters 
                }
                else
                {
                    qualReq1.QualificationTypeId = "2F1QJWKUDD8XADTFD2Q0G6UTO95ALH"; // masters 
                }
                qualReq1.Comparator = Comparator.Exists;
                qualReqs.Add(qualReq1);
            }
                /*if ((string)HitParams["PercentApproved"] == "Masters")
                {
                    QualificationRequirement qualReq1 = new QualificationRequirement();
                    qualReq1.QualificationTypeId = "2F1QJWKUDD8XADTFD2Q0G6UTO95ALH"; // masters                   
                    qualReq1.Comparator = Comparator.Exists;
                    qualReqs.Add(qualReq1);
                }*/
            
            /****************************************************************/

            String[] responseGroup = Default_ResponseGroup;
            if (HitParams.ContainsKey("ResponseGroup"))
            {
                responseGroup = (string[])HitParams["ResponseGroup"];
            }

            /************************Now create a HIT *****************************/

            HIT h = client.CreateHIT(HitType,
                title,
                description,
                keywords,
                myExtQuestion,
                reward,
                assignmentDuration,                // 1 hour
                autoApporvalTime,      // 15 days
                lifeTime,       // 3 days,
                maxAssignments,
                requesterAnnotation,
                qualReqs,
                responseGroup);

            Console.WriteLine("Created HIT: {0} ({1})", h.HITId, client.GetPreviewURL(h.HITTypeId));

            return h;
        }

        public void ApproveAssignment(string AssignmentId, string feedback)
        {
            bool redo = false;
            int limit = 10;
            int count = 0;
            do
            {
                try
                {
                client.ApproveAssignment(AssignmentId, feedback);
                }
                catch(Amazon.WebServices.MechanicalTurk.Exceptions.InsufficientFundsException)
                {
                    Console.WriteLine("NoT Enough Funds!!!");
                }
                catch(Amazon.WebServices.MechanicalTurk.Exceptions.InvalidParameterValueException)
                {
                    Console.WriteLine("Invalid Parameter Value!!!");
                }
                catch(Amazon.WebServices.MechanicalTurk.Exceptions.AccessKeyException)
                {
                    Console.WriteLine("Invalid Access Key Exception!!!");
                }
                catch (Amazon.WebServices.MechanicalTurk.Exceptions.InvalidTransportEndpointException)
                {
                    Console.WriteLine("Invalid Transport end point!!!");
                }
                catch (Amazon.WebServices.MechanicalTurk.Exceptions.ObjectDoesNotExistException)
                {
                    //Console.WriteLine(e.ToString());
                    Console.WriteLine(AssignmentId + " does not exist in Amazon");
                    return;
                }
                catch (Amazon.WebServices.MechanicalTurk.Exceptions.InvalidStateException)
                {
                    //Console.WriteLine(e.ToString());
                    Console.WriteLine(AssignmentId + "already approved");
                    return;
                }
                catch (Amazon.WebServices.MechanicalTurk.Exceptions.ServiceException)
                {
                    redo = true;
                    count++;
                }
            } while (redo && count < limit);
        }

        public void RejectAssignment(string AssignmentId, string feedback)
        {
            bool redo = false;
            do
            {
                try
                {
                    client.RejectAssignment(AssignmentId, feedback);
                }
                catch (Amazon.WebServices.MechanicalTurk.Exceptions.ObjectDoesNotExistException)
                {
                    //Console.WriteLine(e.ToString());
                    Console.WriteLine(AssignmentId + "rejected and removed from Amazon");
                    return;
                }
                catch (Amazon.WebServices.MechanicalTurk.Exceptions.InvalidStateException)
                {
                    //                Console.WriteLine(e.ToString());
                    Console.WriteLine(AssignmentId + "already rejected");
                    return;
                }                
            } while (redo);
        }

        public int getNumTasksRemaining(string HITId)
        {
            HIT h = client.GetHIT(HITId);
            int available = h.NumberOfAssignmentsAvailable;
            int pending = h.NumberOfAssignmentsPending;
            int remaining = available + pending;
            return remaining;
        }

        public void extendHITTExpiryime(string HITId, long seconds)
        {
            client.ExtendHIT(HITId, null, seconds);
        }

        public bool expireHIT(string HITId)
        {
            try
            {
                client.ForceExpireHIT(HITId);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        //public bool TaskExpired(string HITId)
        //{
        //    HIT h = client.GetHIT(HITId);
        //    long lifeTimeInSeconds = h.AssignmentDurationInSeconds;
        //    //Console.WriteLine(lifeTimeInSeconds);
        //    if(lifeTimeInSeconds >0)
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}

        public double getAccountBalance()
        {
            try
            {
                GetAccountBalanceResult result = client.GetAccountBalance();
                Price p = result.AvailableBalance;
                Console.WriteLine("Account Balance: {0}", (double)p.Amount);
                return (double)p.Amount;
            }
            catch(Exception)
            {
                return -1;
            }
            
        }

        public bool DeleteHIT(string hitID)
        {
            try { 
            client.DisposeHIT(hitID);
            }
            catch(Amazon.WebServices.MechanicalTurk.Exceptions.ObjectDoesNotExistException)
            {
                return true;
            }
            catch(Amazon.WebServices.MechanicalTurk.Exceptions.InvalidStateException e)
            {
                if (e.Message.Contains("'Disposed'"))
                {
                    return true;
                }
                if (e.Message.Contains("'Reviewable'"))
                {
                    return false;
                }
                //  should never reach here
                return false; 
            }
            catch (Exception)
            {
                //  should never reach here
                return false;
            }
            return true;
        }
    }

}
