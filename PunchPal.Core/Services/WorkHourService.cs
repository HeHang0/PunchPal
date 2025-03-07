using PunchPal.Core.Models;
using PunchPal.Core.ViewModels;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
        public async Task<List<WorkingHours>> List(int dayStartHour, IEnumerable<PunchRecord> punchRecords)
        {
            var result = new List<WorkingHours>();
            if (punchRecords == null || punchRecords.Count() == 0)
            {
                return result;
            }
            var lastRecord = punchRecords.FirstOrDefault();
            var lastDate = lastRecord.PunchDateTime;
            var monthStartDay = new DateTime(lastDate.Year, lastDate.Month, 1);
            var monthEndDay = monthStartDay.AddMonths(1).AddDays(-1);
            var now = DateTime.Now;
            var startUnix = monthStartDay.TimestampUnix();
            var endUnix = monthEndDay.TimestampUnix();
            var workingTimeRanges = await WorkingTimeRangeService.Instance.Items(startUnix, endUnix);
            var attendanceRecords = await AttendanceRecordService.Instance.List(m => m.StartTime >= startUnix && m.StartTime < endUnix && AttendanceTypeService.AskForLeaveIds.Contains(m.AttendanceTypeId));
            var calendars = await CalendarService.Instance.ListAll(m => m.Date >= startUnix && m.Date < endUnix);
            for (var i = 1; i <= monthEndDay.Day; i++)
            {
                if (monthEndDay.Year == now.Year && monthEndDay.Month == now.Month && i > now.Day)
                {
                    break;
                }
                var date = new DateTime(monthEndDay.Year, monthEndDay.Month, i, dayStartHour, 0, 0);
                var dateUnix = date.Date.TimestampUnix();
                var timeStart = date.TimestampUnix();
                var timeEnd = timeStart + DateTimeTools.DaySeconds;
                var currentRecords = punchRecords.Where(m => m.PunchTime >= timeStart && m.PunchTime < timeEnd).ToList();
                var currentAttendanceRecords = GetAttendanceRecords(attendanceRecords, timeStart, timeEnd, workingTimeRanges[dateUnix]);
                var item = ParseWorkingHours(currentRecords, timeStart, workingTimeRanges[dateUnix], currentAttendanceRecords, calendars.FirstOrDefault(m => m.Date == dateUnix));
                if (item != null)
                {
                    result.Add(item);
                }
            }
            return result.OrderByDescending(m => m.WorkingDate).ToList();
        }

        private List<AttendanceRecord> GetAttendanceRecords(List<AttendanceRecord> attendanceRecords, long timeStart, long timeEnd, WorkingTimeRangeItems workingTime)
        {
            var currentRecords = attendanceRecords.Where(m => m.StartTime >= timeStart && m.StartTime < timeEnd).ToList();
            if (workingTime == null || workingTime.Work == null)
            {
                return currentRecords;
            }
            var date = timeStart.Unix2DateTime();
            var work = workingTime.Work;
            var startWorkDate = new DateTime(date.Year, date.Month, date.Day, workingTime.Work.StartHour, 0, 0);
            if (currentRecords.Count == 0)
            {
                var currentCrossRecords = attendanceRecords.Where(m => m.StartTime < timeStart && m.EndTime > timeStart).ToList();
                foreach (var item in currentCrossRecords)
                {
                    item.StartTime = Math.Max(item.StartTime, startWorkDate.TimestampUnix());
                    item.EndTime = Math.Min(item.EndTime, startWorkDate.TimestampUnix());
                    currentRecords.Add(item);
                }
            }
            return currentRecords;
        }

        private WorkingHours ParseWorkingHours(List<PunchRecord> punchRecords, int timeStart, WorkingTimeRangeItems workingTime, List<AttendanceRecord> attendanceRecords, CalendarRecord calendar)
        {
            var settings = SettingsModel.Load();
            var item = new WorkingHours()
            {
                WorkingDate = timeStart,
                IsHoliday = calendar != null && (calendar.IsHoliday || (calendar.IsWeekend && !calendar.IsWorkday)),
            };
            var (minTimePure, maxTimePure) = GetMinMaxTime(punchRecords);
            if (settings.Data.IsAttendanceTime)
            {
                foreach (var attendance in attendanceRecords)
                {
                    punchRecords.Add(new PunchRecord()
                    {
                        PunchTime = attendance.StartTime
                    });
                    if (attendance.EndTime > 0)
                    {
                        punchRecords.Add(new PunchRecord()
                        {
                            PunchTime = attendance.EndTime
                        });
                    }
                }
            }
            if (punchRecords.Count < 2)
            {
                return null;
            }
            var lateTimeFix = settings.WorkingTimeRange.FlexibleWorkingMinute * 60;
            var faultTolerance = settings.WorkingTimeRange.FaultToleranceMinute;
            var (minTime, maxTime) = GetMinMaxTime(punchRecords);
            if (workingTime != null && workingTime.Work != null)
            {
                var dateStart = timeStart.Unix2DateTime();
                var startWorkTime = new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, workingTime.Work.StartHour, 0, 0);
                var endWorkTime = new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, workingTime.Work.EndHour, 0, 0);
                var startWorkTimeUnix = startWorkTime.TimestampUnix();
                var endWorkTimeUnix = endWorkTime.TimestampUnix();
                if (settings.Data.IsIgnoreBeforeWorkTime && startWorkTimeUnix > minTime)
                {
                    minTime = startWorkTimeUnix;
                }
                if (startWorkTimeUnix + lateTimeFix < minTime)
                {
                    var lateMinutes = (int)((minTime - startWorkTimeUnix - lateTimeFix) / 60);
                    item.LateMinutes = lateMinutes > faultTolerance ? lateMinutes : 0;
                }
                if (endWorkTimeUnix > maxTime)
                {
                    item.LeaveEarlyMinutes = (int)((endWorkTimeUnix - maxTime) / 60);
                }
                item.StandardMinutes = CalcTotalMinutes(Math.Max(minTimePure, startWorkTimeUnix), Math.Min(maxTimePure, endWorkTimeUnix), workingTime, true);
                if (maxTimePure > endWorkTimeUnix)
                {
                    item.WorkOvertimeMinutes = CalcTotalMinutes(endWorkTimeUnix, maxTimePure, workingTime, false);
                }
            }
            var totalMinutes = CalcTotalMinutes(minTime, maxTime, workingTime, item.IsHoliday && settings.Data.IsIgnoreDinnerAtHoliday);
            item.TotalMinutes = totalMinutes;
            item.StartTime = minTime;
            item.EndTime = maxTime;
            if (minTime != minTimePure && maxTime != maxTimePure)
            {
                item.TotalRealMinutes = CalcTotalMinutes(minTimePure, maxTimePure, workingTime, item.IsHoliday && settings.Data.IsIgnoreDinnerAtHoliday);
            }
            else
            {
                item.TotalRealMinutes = item.TotalMinutes;
            }
            var date = item.WorkingDateTime;
            var now = DateTime.Now;
            var isTody = date.Year == now.Year && date.Month == now.Month && date.Day == now.Day;
            if(isTody)
            {
                var endWorkTime = new DateTime(date.Year, date.Month, date.Day, workingTime.Work.EndHour, workingTime.Work.EndMinute, 0);
                item.IsToday = endWorkTime >= now;
            }
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

        private static int CalcTotalMinutes(long startTime, long endTime, WorkingTimeRangeItems workingTime, bool ignoreDinner = false)
        {
            var totalMinute = endTime - startTime;
            if (workingTime == null)
            {
                return (int)(totalMinute / 60);
            }
            var startDateTime = startTime.Unix2DateTime();
            if (workingTime.Lunch != null)
            {
                var lunchStart = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, workingTime.Lunch.StartHour, workingTime.Lunch.StartMinute, 0).TimestampUnix();
                var lunchEnd = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, workingTime.Lunch.EndHour, workingTime.Lunch.EndMinute, 0).TimestampUnix();
                totalMinute -= CalculateOverlap(startTime, endTime, lunchStart, lunchEnd);
            }
            if (!ignoreDinner && workingTime.Dinner != null)
            {
                var dinnerStart = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, workingTime.Dinner.StartHour, workingTime.Dinner.StartMinute, 0).TimestampUnix();
                var dinnerEnd = new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, workingTime.Dinner.EndHour, workingTime.Dinner.EndMinute, 0).TimestampUnix();
                totalMinute -= CalculateOverlap(startTime, endTime, dinnerStart, dinnerEnd);
            }
            return (int)(totalMinute / 60);
        }

        private static int CalculateOverlap(long rangeStart1, long rangeEnd1, long rangeStart2, long rangeEnd2)
        {
            // 找到两个区间的交集
            long overlapStart = Math.Max(rangeStart1, rangeStart2);
            long overlapEnd = Math.Min(rangeEnd1, rangeEnd2);

            // 如果有交集，返回交集时长；否则返回 0
            return (int)Math.Max(0, overlapEnd - overlapStart);
        }
    }
}
