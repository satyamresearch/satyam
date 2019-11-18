using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class ObjectsToStrings
    {
        public static string ListString(List<string> ListOfStrings, char separator)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < ListOfStrings.Count; i++)
            {
                s.Append(ListOfStrings[i]);
                if (i != ListOfStrings.Count - 1)
                {
                    s.Append(separator); //make the filenames csv seperated
                }
            }
            return s.ToString();
        }

        public static string ListString(string[] ArrayOfStrings, char separator)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < ArrayOfStrings.Length; i++)
            {
                s.Append(ArrayOfStrings[i]);
                if (i != ArrayOfStrings.Length - 1)
                {
                    s.Append(separator); //make the filenames csv seperated
                }
            }
            return s.ToString();
        }

        public static string DictionaryStringListString(Dictionary<string, List<string>> Dict, char separator1, char keySeperator, char separator2)
        {
            StringBuilder s = new StringBuilder();
            bool first = true;
            foreach (KeyValuePair<string, List<string>> entry in Dict)
            {
                if (!first)
                {
                    s.Append(separator1);
                }
                else
                {
                    first = false;
                }
                s.Append(entry.Key);
                s.Append(keySeperator);
                if (entry.Value.Count == 0)
                {
                    s.Append("");
                }
                else
                {
                    s.Append(entry.Value[0]);
                    for (int i = 1; i < entry.Value.Count; i++)
                    {
                        s.Append(separator2);
                        s.Append(entry.Value[i]);
                    }
                }
            }
            return s.ToString();
        }
    }
}
