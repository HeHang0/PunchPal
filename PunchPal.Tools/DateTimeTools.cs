using System;

namespace PunchPal.Tools
{
    public static class DateTimeTools
    {
        public static DateTime Unix2DateTime(this long totalSecond)
        {
            return new DateTime(1970, 1, 1).AddSeconds(totalSecond);
        }

        public static DateTime Unix2DateTime(this int totalSecond)
        {
            return new DateTime(1970, 1, 1).AddSeconds(totalSecond);
        }

        public static int TimestampUnix(this DateTime dateTime)
        {
            return (int)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string ToDateTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ToDateString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }

        public static string ToMonthString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM");
        }
    }
}
