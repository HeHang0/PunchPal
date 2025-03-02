using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PunchPal.Core.Models;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

        private static bool usedDefaltData = false;

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
                            var festivalSet = new HashSet<string>((record["festivalList"]?.ToString() ?? string.Empty).Trim().Split(','));
                            var termSet = new HashSet<string>((record["term"]?.ToString() ?? string.Empty).Trim().Split(' '));
                            var festivalList = festivalSet.Where(m => !termSet.Contains(m)).ToList();
                            for (var i = 0; i < festivalList.Count; i++)
                            {
                                if (festivalList[i].EndsWith("九天"))
                                {
                                    termSet.Add(festivalList[i]);
                                    festivalList.RemoveAt(i);
                                    i--;
                                }
                            }
                            var calendarRecord = new CalendarRecord
                            {
                                Date = cDate,
                                Festival = string.Join(" ", festivalList).Trim(),
                                LunarMonth = record["lMonth"]?.ToString() ?? string.Empty,
                                LunarDate = record["lDate"]?.ToString() ?? string.Empty,
                                LunarYear = record["gzYear"]?.ToString() ?? string.Empty,
                                SolarTerm = string.Join(" ", termSet).Trim(),
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
                if (usedDefaltData)
                {
                    return calendarRecords;
                }
                usedDefaltData = true;
                try
                {
                    calendarRecords = JsonConvert.DeserializeObject<List<CalendarRecord>>(Encoding.UTF8.GetString(Properties.Resources.CalendarsText));
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            return calendarRecords;
        }
    }
}
