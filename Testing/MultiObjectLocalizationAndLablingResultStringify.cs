using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HelperClasses;
using Utilities;
using SatyamTaskResultClasses;

namespace Testing
{
    public static class MultiObjectLocalizationAndLabelingResultStringify
    {
        public static void testJSONString()
        {
            MultiObjectLocalizationAndLabelingResult res = new MultiObjectLocalizationAndLabelingResult();
            res.objects = new List<MultiObjectLocalizationAndLabelingResultSingleEntry>();

            MultiObjectLocalizationAndLabelingResultSingleEntry entry = new MultiObjectLocalizationAndLabelingResultSingleEntry();
            entry.boundingBox = new BoundingBox(5, 15, 25, 35);
            entry.Category = "Car";
            res.objects.Add(entry);
            entry = new MultiObjectLocalizationAndLabelingResultSingleEntry();
            entry.boundingBox = new BoundingBox(45, 55, 65, 75);
            entry.Category = "Bus";
            res.objects.Add(entry);

            string jsonString = JSonUtils.ConvertObjectToJSon<MultiObjectLocalizationAndLabelingResult>(res);
        }
    }
}
