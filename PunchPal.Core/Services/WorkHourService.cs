using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System;
using PunchPal.Core.Models;
using PunchPal.Tools;
using System.Runtime;
using System.Linq;

namespace PunchPal.Core.Services
{
    public class WorkHourService
    {
        public static readonly WorkHourService Instance;

        static WorkHourService()
        {
            Instance = new WorkHourService();
        }

        public async Task<List<WorkingHours>> List(int dayStartHour, Expression<Func<PunchRecord, bool>> predicate)
        {
            var records = await PunchRecordService.Instance.List(predicate);
            return await List(dayStartHour, records);
        }
        public Task<List<WorkingHours>> List(int dayStartHour, IList<PunchRecord> punchRecords)
        {
            var result = new List<WorkingHours>();
            if (punchRecords == null || punchRecords.Count == 0)
            {
                return Task.FromResult(result);
            }
            var lastRecord = punchRecords[0];
            var lastDate = lastRecord.PunchDateTime;
            var monthStartDay = new DateTime(lastDate.Year, lastDate.Month, 1);
            var monthEndDay = monthStartDay.AddMonths(1).AddDays(-1);
            var now = DateTime.Now;
            for (var i = 1; i <= monthEndDay.Day; i++)
            {
                if (monthEndDay.Year == now.Year && monthEndDay.Month == now.Month && i > now.Day)
                {
                    break;
                }
                var date = new DateTime(monthEndDay.Year, monthEndDay.Month, i, dayStartHour, 0, 0);
                var timeStart = date.TimestampUnix();
                var timeEnd = timeStart + DateTimeTools.DaySeconds;
                var currentRecords = punchRecords.Where(m => m.PunchTime >= timeStart && m.PunchTime < timeEnd).ToList();
                if (currentRecords.Count < 2)
                {
                    continue;
                }
                var item = ParseWorkingHours(currentRecords, timeStart);
                result.Add(item);
            }
            return Task.FromResult(result);
        }

        private WorkingHours ParseWorkingHours(List<PunchRecord> punchRecords, int timeStart)
        {
            var item = new WorkingHours()
            {
                WorkingDate = timeStart
            };
            var (minTime, maxTime) = GetMinMaxTime(punchRecords);
            var totalMinutes = CalcTotalMinutes(minTime, maxTime, item.IsHoliday);
            item.TotalMinutes = totalMinutes;
            item.StartTime = minTime;
            item.EndTime = maxTime;
            item.TotalRealMinutes = item.TotalMinutes;
            var date = item.WorkingDateTime;
            var now = DateTime.Now;
            item.IsToday = date.Year == now.Year && date.Month == now.Month && date.Day == now.Day;
            return item;
        }

        private static (long, long) GetMinMaxTime(List<PunchRecord> record)
        {
            if (record.Count <= 0)
            {
                return (0, 0);
            }
            var maxTime = record.Max(m => m.PunchTime);
            var minTime = record.Min(m => m.PunchTime);
            return (minTime, maxTime);
        }

        public static int CalcTotalMinutes(long startTime, long endTime, bool ignoreDinner = false)
        {
            return (int)((endTime - startTime) / 60);
        }
    }
}
