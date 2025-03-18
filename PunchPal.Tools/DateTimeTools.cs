using System;
using System.Collections.Generic;

namespace PunchPal.Tools
{
    public static class DateTimeTools
    {
        public const int HourSeconds = 60 * 60;
        public const int DaySeconds = 24 * 60 * 60;
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

        public static string ToTimeString(this DateTime dateTime)
        {
            return dateTime.ToString("HH:mm:ss");
        }

        public static string ToMonthString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM");
        }

        public readonly static List<int> DayList = new List<int>();
        public readonly static List<int> HoursList = new List<int>();
        public readonly static List<int> HourTotalList = new List<int>();
        public readonly static List<int> MinutesList = new List<int>();
        public static List<int> SecondsList => MinutesList;

        static DateTimeTools()
        {
            for (var i = 0; i < 60; i++)
            {
                if (i < 24)
                {
                    HoursList.Add(i);
                    HourTotalList.Add(i + 1);
                }
                if (i < 31)
                {
                    DayList.Add(i + 1);
                }
                MinutesList.Add(i);
            }
        }
    }
}
