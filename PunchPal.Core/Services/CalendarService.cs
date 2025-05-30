﻿using PunchPal.Core.Apis;
using PunchPal.Core.Models;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PunchPal.Core.Services
{
    public class CalendarService
    {
        public static readonly CalendarService Instance;

        static CalendarService()
        {
            Instance = new CalendarService();
        }

        public async Task Add(IEnumerable<CalendarRecord> calendarRecords)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    foreach (var record in calendarRecords)
                    {
                        context.CalendarRecords.AddOrUpdate(record);
                    }
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task<bool> Any(Expression<Func<CalendarRecord, bool>> predicate)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    return await context.CalendarRecords.AnyAsync(predicate);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task Sync(DateTime date)
        {
            var startValue = date.TimestampUnix();
            var endValue = date.AddMonths(1).TimestampUnix();
            var result = await Any(m => m.Date >= startValue && m.Date < endValue);
            if (!result)
            {
                var syncResult = await SyncCalendar(date);
            }
            return;
        }

        public async Task<List<CalendarRecord>> ListOrSync(Expression<Func<CalendarRecord, bool>> predicate, DateTime date)
        {
            var startValue = date.TimestampUnix();
            var endValue = date.AddMonths(1).TimestampUnix();
            var result = await List(predicate);
            if (!result.Any(m => m.Date >= startValue && m.Date < endValue))
            {
                var syncResult = await SyncCalendar(date);
                result.AddRange(syncResult.Where(m => m.Date >= startValue && m.Date < endValue));
            }
            return result;
        }

        public async Task<List<CalendarRecord>> List(Expression<Func<CalendarRecord, bool>> predicate)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    return await context.CalendarRecords.Where(predicate).ToListAsync();
                }
            }
            catch (Exception)
            {
                return new List<CalendarRecord>();
            }
        }

        public static Dictionary<string, CalendarRecord> GetCalendarMap(IEnumerable<CalendarRecord> records)
        {
            Dictionary<string, CalendarRecord> recordMap = new Dictionary<string, CalendarRecord>();
            foreach (var item in records)
            {
                recordMap[item.Date.Unix2DateTime().ToDateString()] = item;
            }
            return recordMap;
        }

        public async Task<List<CalendarRecord>> ListAll(Expression<Func<CalendarRecord, bool>> predicate)
        {
            var result = new List<CalendarRecord>();
            try
            {
                using (var context = new PunchDbContext())
                {
                    result = await context.CalendarRecords.Where(predicate).ToListAsync();
                }
            }
            catch (Exception)
            {
                return new List<CalendarRecord>();
            }
            var calendars = result.Where(m => m.Type == CalendarType.Baidu).ToList();
            var otherCalendars = result.Where(m => m.Type != CalendarType.Baidu);
            var calendarMap = GetCalendarMap(calendars);
            var otherCalendarMap = GetCalendarMap(otherCalendars);
            foreach (var item in calendars)
            {
                var date = item.DateTime.ToDateString();
                var otherCalendarData = otherCalendarMap.ContainsKey(date) ? otherCalendarMap[date] : null;
                if (otherCalendarData != null)
                {
                    if (string.IsNullOrWhiteSpace(otherCalendarData.LunarDate) &&
                        string.IsNullOrWhiteSpace(otherCalendarData.LunarMonth) &&
                        string.IsNullOrWhiteSpace(otherCalendarData.LunarYear) &&
                        string.IsNullOrWhiteSpace(otherCalendarData.Remark) &&
                        !string.IsNullOrWhiteSpace(otherCalendarData.Festival))
                    {
                        item.Remark = otherCalendarData.Festival;
                    }
                    item.IsHoliday = otherCalendarData.IsHoliday;
                    item.IsWorkday = otherCalendarData.IsWorkday;
                    var festivals = new List<string>();
                    if (!string.IsNullOrWhiteSpace(otherCalendarData.Festival))
                    {
                        festivals.Add(otherCalendarData.Festival);
                    }
                    if (!string.IsNullOrWhiteSpace(item.Festival))
                    {
                        festivals.Add(item.Festival);
                    }
                    item.Festival = string.Join(" ", festivals);
                }
            }
            return calendars;
        }

        public async Task<List<CalendarRecord>> SyncCalendar(DateTime date)
        {
            var result = new List<CalendarRecord>();
            try
            {
                using (var context = new PunchDbContext())
                {
                    for (int i = 1; i < 12; i++)
                    {
                        var syncDate = new DateTime(date.Year, i, 1);
                        var startValue = syncDate.TimestampUnix();
                        var endValue = syncDate.AddMonths(1).TimestampUnix();
                        var exists = context.CalendarRecords.Any(m => m.Date >= startValue && m.Date < endValue);
                        if (exists) continue;
                        var records = await BaiduAPI.GetCalendars(syncDate);
                        if (records.Count > 0)
                        {
                            await SaveRecords(context, records);
                            if (i == date.Month)
                            {
                                result.AddRange(records.Where(m => m.Date >= startValue && m.Date < endValue));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("sync calendar error", e.Message);
            }
            return result;
        }

        public static HashSet<string> ChineseHolidays = new HashSet<string>()
        {
            "元旦", "除夕", "春节", "清明节", "清明", "劳动节", "端午节", "国庆节", "中秋节"
        };

        public static HashSet<string> ChineseSolarTermHolidays = new HashSet<string>()
        {
            "除夕", "春节", "清明节", "清明", "端午节", "中秋节"
        };
        public static HashSet<string> SolarTerms = new HashSet<string>()
        {
            "小寒", "大寒", "立春", "雨水", "惊蛰", "春分",
            "清明", "谷雨", "立夏", "小满", "芒种", "夏至",
            "小暑", "大暑", "立秋", "处暑", "白露", "秋分",
            "寒露", "霜降", "立冬", "小雪", "大雪", "冬至"
        };
        public async Task<(CalendarRecord Record, int Distance)> GetRecentHolidays()
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var today = DateTime.Now;
                    var startValue = new DateTime(today.Year, today.Month, today.Day).AddDays(1).TimestampUnix();
                    var result = await context.CalendarRecords.OrderBy(m => m.Date).FirstOrDefaultAsync(m => m.Date >= startValue && m.SolarTerm != "除夕" && ChineseHolidays.Contains(m.SolarTerm));
                    if (result == null)
                    {
                        return (null, 0);
                    }
                    var distance = 1 + (int)(result.Date - startValue) / DateTimeTools.DaySeconds;
                    startValue = (int)result.Date;
                    var lastDays = await context.CalendarRecords.OrderByDescending(m => m.Date).Where(m => m.Date < startValue).Take(10).ToListAsync();
                    for (var i = 0; i < lastDays.Count; i++)
                    {
                        if (lastDays[i].IsHoliday)
                        {
                            distance--;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (result.SolarTerm == "清明")
                    {
                        result.SolarTerm = "清明节";
                    }
                    return (result, distance);
                }
            }
            catch (Exception)
            {
                return (null, 0);
            }
        }

        private static readonly Dictionary<int, int> _yearDays = new Dictionary<int, int>
            {
                { 1, 31 },
                { 3, 31 },
                { 4, 30 },
                { 5, 31 },
                { 6, 30 },
                { 7, 31 },
                { 8, 31 },
                { 9, 30 },
                { 10, 31 },
                { 11, 30 },
                { 12, 31 }
            };

        private async Task SaveRecords(PunchDbContext context, List<CalendarRecord> records)
        {
            if (records.Count <= 0)
            {
                return;
            }
            var startDate = records[0].Date.Unix2DateTime();
            var endDate = records[records.Count - 1].Date.Unix2DateTime();
            for (; startDate <= endDate; startDate = startDate.AddMonths(1))
            {
                var monthStart = new DateTime(startDate.Year, startDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1);
                var monthStartValue = monthStart.TimestampUnix();
                var monthEndValue = monthEnd.TimestampUnix();
                var monthRecords = records.Where(m => m.Date >= monthStartValue && m.Date < monthEndValue).GroupBy(m => new { m.Date, m.Type }).Select(m => m.First()).ToList();
                if (monthRecords.Count != GetYearDays(monthStart))
                {
                    continue;
                }
                foreach (var record in monthRecords)
                {
                    context.CalendarRecords.AddOrUpdate(record);
                }
            }
            await context.SaveChangesAsync();
        }

        private int GetYearDays(DateTime date)
        {
            if (date.Month == 2)
            {
                return DateTime.IsLeapYear(date.Year) ? 29 : 28;
            }
            return _yearDays[date.Month];
        }
    }
}
