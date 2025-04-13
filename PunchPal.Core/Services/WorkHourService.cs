using PunchPal.Core.Models;
using PunchPal.Core.ViewModels;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<WorkingHours>> List(int dayStartHour, DateTime start, DateTime end, IEnumerable<PunchRecord> punchRecordEnumerables, IEnumerable<AttendanceRecord> attendancesAll)
        {
            var result = new List<WorkingHours>();
            if (punchRecordEnumerables == null || punchRecordEnumerables.Count() == 0)
            {
                return result;
            }
            var punchRecords = punchRecordEnumerables.ToList();
            var now = DateTime.Now;
            var startUnix = start.TimestampUnix();
            var endUnix = end.TimestampUnix();
            var workingTimeRanges = await WorkingTimeRangeService.Instance.Items(startUnix, endUnix);
            var attendances = attendancesAll.Where(m => AttendanceTypeService.AskForLeaveIds.Contains(m.AttendanceTypeId));
            var punchInRecords = attendancesAll.Where(m => AttendanceTypeService.PunchInRecordIds.Contains(m.AttendanceTypeId));
            var attendanceRecords = ParseAttendance(attendances, workingTimeRanges);
            var calendars = await CalendarService.Instance.ListAll(m => m.Date >= startUnix && m.Date < endUnix);
            foreach (var item in punchInRecords)
            {
                if (item.StartTime <= 0 && item.EndTime <= 0) continue;
                punchRecords.Add(new PunchRecord()
                {
                    PunchTime = item.StartTime > 0 ? item.StartTime : item.EndTime,
                });
            }
            for (; start <= end; start = start.AddDays(1))
            {
                if (start.Year == now.Year && start.Month == now.Month && start.Day > now.Day)
                {
                    break;
                }
                var date = new DateTime(start.Year, start.Month, start.Day, dayStartHour, 0, 0);
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

        private List<AttendanceRecord> ParseAttendance(IEnumerable<AttendanceRecord> attendances, Dictionary<long, WorkingTimeRangeItems> workingTimeRanges)
        {
            var result = new List<AttendanceRecord>();
            foreach (var item in attendances)
            {
                var startDate = item.StartDateTime;
                var endDate = item.EndDateTime;
                var startDateUnix = startDate?.TimestampUnix() ?? 0;
                var endDateUnix = endDate?.TimestampUnix() ?? 0;
                var diff = endDateUnix - startDateUnix;
                if (diff > DateTimeTools.DaySeconds)
                {
                    var ranges = GetDateRanges(startDate.Value, endDate.Value, workingTimeRanges);
                    foreach (var range in ranges)
                    {
                        result.Add(new AttendanceRecord()
                        {
                            AttendanceId = item.AttendanceId,
                            AttendanceTypeId = item.AttendanceTypeId,
                            AttendanceType = item.AttendanceType,
                            UserId = item.UserId,
                            StartTime = range.Item1,
                            EndTime = range.Item2,
                            AttendanceTime = item.AttendanceTime,
                            Remark = item.Remark
                        });
                    }
                }
                else
                {
                    result.Add(item);
                }
            }
            return result;
        }

        private List<(int, int)> GetDateRanges(DateTime startTime, DateTime endTime, Dictionary<long, WorkingTimeRangeItems> workingTimeRanges)
        {
            var result = new List<(int, int)>();
            var endTimeUnix = endTime.TimestampUnix();
            for (var start = startTime; start < endTime; start = start.AddDays(1))
            {
                var dateUnix = start.Date.TimestampUnix();
                if (!workingTimeRanges.TryGetValue(dateUnix, out WorkingTimeRangeItems work))
                {
                    continue;
                }
                var startWorkDate = new DateTime(start.Year, start.Month, start.Day, work.Work.StartHour, work.Work.StartMinute, 0);
                var endWorkDate = new DateTime(start.Year, start.Month, start.Day, work.Work.EndHour, work.Work.EndMinute, 0);
                result.Add((Math.Max(startWorkDate.TimestampUnix(), start.TimestampUnix()), Math.Min(endWorkDate.TimestampUnix(), endTimeUnix)));
            }
            return result;
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
                var currentCrossRecords = attendanceRecords.Where(m => m.StartTime <= timeStart && m.EndTime >= timeStart).ToList();
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
            if (minTimePure <= 0)
            {
                minTimePure = minTime;
            }
            if (maxTimePure <= 0)
            {
                maxTimePure = maxTime;
            }
            if (workingTime != null && workingTime.Work != null)
            {
                var dateStart = timeStart.Unix2DateTime();
                var startWorkTime = new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, workingTime.Work.StartHour, workingTime.Work.StartMinute, 0);
                var endWorkTime = new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, workingTime.Work.EndHour, workingTime.Work.EndMinute, 0);
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
            if (isTody)
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
