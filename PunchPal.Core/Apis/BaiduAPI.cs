using Newtonsoft.Json.Linq;
using PunchPal.Core.Models;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PunchPal.Core.Apis
{
    public class BaiduAPI
    {
        internal static async Task<List<CalendarRecord>> GetCalendars(DateTime date)
        {
            var dateStr = date.ToString("yyyy年M月");
            var url = $"https://opendata.baidu.com/data/inner?tn=reserved_all_res_tn&type=json&resource_id=52109&query={WebUtility.UrlEncode(dateStr)}&apiType=yearMonthData";
            var json = await NetworkUtils.Get(url);
            return ParseBaseCalendarJson(json, date);
        }


        public static List<CalendarRecord> ParseBaseCalendarJson(string json, DateTime date)
        {
            var calendarRecords = new List<CalendarRecord>();
            try
            {
                var jsonData = JObject.Parse(json);
                var records = jsonData["Result"]?[0]?["DisplayData"]?["resultData"]?["tplData"]?["data"]?["almanac"];
                if (records != null)
                {
                    foreach (var record in records)
                    {
                        try
                        {
                            var cDate = (long)record["timestamp"] + 8 * DateTimeTools.HourSeconds;
                            var festivalList = (record["festivalList"]?.ToString() ?? string.Empty).Split(',');
                            var termList = (record["term"]?.ToString() ?? string.Empty).Split(' ');

                            var calendarRecord = new CalendarRecord
                            {
                                Date = cDate,
                                Festival = string.Join(" ", festivalList.Where(m => !termList.Contains(m))),
                                LunarMonth = record["lMonth"]?.ToString() ?? string.Empty,
                                LunarDate = record["lDate"]?.ToString() ?? string.Empty,
                                LunarYear = record["gzYear"]?.ToString() ?? string.Empty,
                                SolarTerm = string.Join(" ", termList),
                                IsHoliday = record["status"]?.ToString() == "1",
                                IsWorkday = record["status"]?.ToString() == "2"
                            };

                            calendarRecords.Add(calendarRecord);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return calendarRecords;
        }
    }
}
