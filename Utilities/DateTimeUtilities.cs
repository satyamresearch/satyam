using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class DateTimeUtilities
    {
        public static TimeSpan getTimeSpanFromTotalMilliSeconds(int ms)
        {
            int left_ms = ms;
            int hours = (int)Math.Floor((double)left_ms / (60 * 60 * 1000));
            left_ms -= hours * 60 * 60 * 1000;
            int minutes = (int)Math.Floor((double)left_ms / (60 * 1000));
            left_ms -= minutes * 60 * 1000;
            int seconds = (int)Math.Floor((double)left_ms / 1000);
            left_ms -= seconds * 1000;
            TimeSpan dt = new TimeSpan(0, hours, minutes, seconds, left_ms);
            return dt;
        }

        public static DateTime getDateTimeFromString(String l_dateString)
        {
            string[] fields = l_dateString.Split('-');
            int year = Convert.ToInt32(fields[0]);
            int month = Convert.ToInt32(fields[1]);
            int day = Convert.ToInt32(fields[2]);
            int hour = Convert.ToInt32(fields[3]);
            int min = Convert.ToInt32(fields[4]);
            int sec = Convert.ToInt32(fields[5]);
            int ms = Convert.ToInt32(fields[6]);
            DateTime t = new DateTime(year, month, day, hour, min, sec, ms);
            return t;
        }

        public static string convertDateTimeToString(DateTime t)
        {
            string s = t.Year + "-" + t.Month.ToString("00") + "-" + t.Day.ToString("00") + "-" + t.Hour.ToString("00") + "-" + t.Minute.ToString("00") + "-" + t.Second.ToString("00") + "-" + t.Millisecond.ToString("000");
            return s;
        }

        public static string formatDateTimeString(string t)
        {
            DateTime time = Convert.ToDateTime(t);
            return convertDateTimeToString(time);
        }
    }
}
