using UnityEngine;
using System.Collections;

namespace Fresvii.AppSteroid.UI
{
    public class AUIUtility : MonoBehaviour
    {
        public static string CurrentTimeSpan(System.DateTime dt)
        {
            System.TimeSpan ts = System.DateTime.Now.Subtract(dt);

            string timespan = "";

            if (ts.Days > 1)
                timespan = ts.Days + " " + FASText.Get("days") + FASText.Get("ago");
            else if (ts.Days > 0)
                timespan = ts.Days + " " + FASText.Get("day") + FASText.Get("ago");
            else if (ts.Hours > 1)
                timespan = ts.Hours + " " + FASText.Get("hours") + FASText.Get("ago");
            else if (ts.Hours > 0)
                timespan = ts.Hours + " " + FASText.Get("hour") + FASText.Get("ago");
            else if (ts.Minutes > 1)
                timespan = ts.Minutes + " " + FASText.Get("minutes") + FASText.Get("ago");
            else if (ts.Minutes > 0)
                timespan = ts.Minutes + " " + FASText.Get("minute") + FASText.Get("ago");
            else
                timespan = FASText.Get("now");

            return timespan;
        }

        public static string CurrentTimeSpanShort(System.DateTime dt)
        {
            System.TimeSpan ts = System.DateTime.Now.Subtract(dt);

            string timespan = "";

            if (ts.Days > 1)
                timespan = ts.Days + " " + FASText.Get("days");
            else if (ts.Days > 0)
                timespan = ts.Days + " " + FASText.Get("day");
            else if (ts.Hours > 1)
                timespan = ts.Hours + " " + FASText.Get("hours");
            else if (ts.Hours > 0)
                timespan = ts.Hours + " " + FASText.Get("hour");
            else if (ts.Minutes > 1)
                timespan = ts.Minutes + " " + FASText.Get("min");
            else if (ts.Minutes > 0)
                timespan = ts.Minutes + " " + FASText.Get("min");
            else
                timespan = FASText.Get("now");

            return timespan;
        }

        public static string TimeSpanWatch(System.DateTime dt)
        {
            System.TimeSpan ts = System.DateTime.Now.Subtract(dt);

            string timespan = "";

            if (ts.Hours > 1)
                timespan += ts.Hours + ":";

            timespan += ts.Minutes.ToString("00") + ":";

            timespan += ts.Seconds.ToString("00");

            return timespan;
        }

        public static string TimeSpan(long elapsedTime)
        {
            string timespan = "";

            long sec = elapsedTime / 1000;

            long min = sec / 60;

            long hour = min / 60;

            if (hour > 1)
                timespan += hour + ":";

            timespan += min.ToString("00") + ":";

            timespan += sec.ToString("00");

            return timespan;
        }
    }
}
