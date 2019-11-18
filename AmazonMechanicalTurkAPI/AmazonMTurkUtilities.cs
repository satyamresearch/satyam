using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Utilities;

namespace AmazonMechanicalTurkAPI
{
    public static class AmazonMTurkUtilities
    {
        public static void getAmazonParametersFromURI(string OriginalUri, out string AssignmentID, out string HITID, out string workerID, out string reward)
        {
            OriginalUri = HttpUtility.UrlDecode(OriginalUri);
            string uri = HttpUtility.HtmlDecode(OriginalUri);
            Dictionary<string,string> dict = URIUtilities.getURIAttributes(uri);
            if(dict.ContainsKey("assignmentId"))
            {
                AssignmentID = dict["assignmentId"];
            }
            else
            {
                AssignmentID = "";
            }
            if (dict.ContainsKey("hitId"))
            {
                HITID = dict["hitId"];
            }
            else
            {
                HITID = "";
            }
            if (dict.ContainsKey("workerId"))
            {
                workerID = dict["workerId"];
            }
            else
            {
                workerID = "";
            }
            if (dict.ContainsKey("reward"))
            {
                reward = dict["reward"];
            }
            else
            {
                reward = "";
            }
        }
    }
}
