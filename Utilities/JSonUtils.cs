using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;



namespace Utilities
{
    public static class JSonUtils
    {
        public static string ConvertObjectToJSon<T>(T obj)
        {
            var jsonSerialiser = new JavaScriptSerializer();
            string json = jsonSerialiser.Serialize(obj);
            /*DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, obj);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();*/
            return json;
        }

        public static T ConvertJSonToObject<T>(string jsonString)
        {
            /*DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)serializer.ReadObject(ms);*/
            var jasonSerializer = new JavaScriptSerializer();
            //T obj = (T)jasonSerializer.DeserializeObject(jsonString);
            //T obj = (T)jasonSerializer.DeserializeObject(jsonString);
                        T obj = jasonSerializer.Deserialize<T>(jsonString);
            return obj;
        }
        
    }
}
