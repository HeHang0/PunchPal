using PunchPal.Core.Apis;
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

        private static readonly string[] ChineseHolidays = { "元旦", "春节", "清明节", "清明", "劳动节", "端午节", "国庆节", "中秋节" };
        public async Task<(CalendarRecord Record, int Distance)> GetRecentHolidays()
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var today = DateTime.Now;
                    var startValue = new DateTime(today.Year, today.Month, today.Day).AddDays(1).TimestampUnix();
                    var result = await context.CalendarRecords.OrderBy(m => m.Date).FirstOrDefaultAsync(m => m.Date >= startValue && ChineseHolidays.Contains(m.SolarTerm));
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
                var monthRecords = records.Where(m => m.Date >= monthStartValue && m.Date < monthEndValue).ToList();
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
