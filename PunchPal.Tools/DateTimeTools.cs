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



        public static DateTime[] GetTimeRange(DateTime dateTime, bool isMonth, bool isWeekRecent = true)
        {
            if (isMonth)
            {
                // 获取当前月的时间范围
                DateTime start = new DateTime(dateTime.Year, dateTime.Month, 1); // 当月第一天
                DateTime end = start.AddMonths(1).AddTicks(-1);                 // 当月最后一天的最后时刻
                return new DateTime[] { start, end };
            }
            else if (!isWeekRecent)
            {
                // 获取当前周的时间范围（假设一周从周一开始）
                int diffToMonday = dateTime.DayOfWeek == 0 ? 6 : (int)dateTime.DayOfWeek - 1; // 计算距周一的天数
                DateTime start = dateTime.Date.AddDays(-diffToMonday);                             // 本周周一
                DateTime end = start.AddDays(7).AddTicks(-1);                                     // 本周周日的最后时刻
                return new DateTime[] { start, end };
            }
            else
            {
                // 获取最近一周的时间范围
                DateTime end = dateTime.Date; // 今天
                DateTime start = end.AddDays(-6); // 最近一周的开始时间
                return new DateTime[] { start, end };
            }
        }
    }
}
