using PunchPal.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

namespace PunchPal.Core.Services
{
    public class AttendanceTypeService
    {
        public static readonly AttendanceTypeService Instance;

        static AttendanceTypeService()
        {
            Instance = new AttendanceTypeService();
            using (var context = new PunchDbContext())
            {
                if (context.AttendanceTypes.Any())
                {
                    return;
                }
                foreach (var type in initialTyps)
                {
                    context.AttendanceTypes.AddOrUpdate(new AttendanceType
                    {
                        TypeId = type.Key,
                        TypeName = type.Value
                    });
                }
                context.SaveChanges();
            }
        }

        public async Task<Dictionary<string, string>> ToDictionory()
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    return await context.AttendanceTypes.ToDictionaryAsync(m => m.TypeId, m => m.TypeName);
                }
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
        }

        public async Task<List<AttendanceType>> List()
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var result = await context.AttendanceTypes.ToListAsync();
                    result.Sort((x, y) => initialTypsSort[x.TypeId].CompareTo(initialTypsSort[y.TypeId]));
                    return result;
                }
            }
            catch (Exception)
            {
                return new List<AttendanceType>();
            }
        }
        public static readonly string[] AskForLeaveIds = new string[] { "AL", "SL", "PL", "ML", "PPL", "BL", "FL", "CL", "GL", "RL", "TL", "BD", "IW" };
        private static readonly KeyValuePair<string, string>[] initialTyps = new KeyValuePair<string, string>[]
        {
                // 考勤类型
                new KeyValuePair<string, string>("NA", "正常出勤"),
                new KeyValuePair<string, string>("LA", "迟到"),
                new KeyValuePair<string, string>("EA", "早退"),
                new KeyValuePair<string, string>("AB", "缺勤"),
                new KeyValuePair<string, string>("OT", "加班"),
                new KeyValuePair<string, string>("WF", "外勤/出差"),
                new KeyValuePair<string, string>("RS", "调休"),
                new KeyValuePair<string, string>("AW", "旷工"),
                new KeyValuePair<string, string>("TR", "培训"),
                new KeyValuePair<string, string>("IW", "工伤假"),
                new KeyValuePair<string, string>("CP", "补打卡"),

                // 请假类型
                new KeyValuePair<string, string>("AL", "年假"),
                new KeyValuePair<string, string>("SL", "病假"),
                new KeyValuePair<string, string>("PL", "事假"),
                new KeyValuePair<string, string>("ML", "婚假"),
                new KeyValuePair<string, string>("PPL", "陪产假"),
                new KeyValuePair<string, string>("BL", "产假"),
                new KeyValuePair<string, string>("FL", "丧假"),
                new KeyValuePair<string, string>("CL", "育儿假"),
                new KeyValuePair<string, string>("GL", "公假"),
                new KeyValuePair<string, string>("RL", "探亲假"),
                new KeyValuePair<string, string>("TL", "调休假"),
                new KeyValuePair<string, string>("MCL", "婚检假"),
                new KeyValuePair<string, string>("MNL", "产检假"),
                new KeyValuePair<string, string>("PEL", "体检假"),

                // 节假日和法定假期
                new KeyValuePair<string, string>("PH", "法定假期"),
                new KeyValuePair<string, string>("SH", "节日假期"),
                new KeyValuePair<string, string>("BD", "生日假"),
                new KeyValuePair<string, string>("RD", "调休日"),
                new KeyValuePair<string, string>("WD", "周末"),
                new KeyValuePair<string, string>("OD", "其他假期")
        };

        private static readonly Dictionary<string, int> initialTypsSort = new Dictionary<string, int>
        {
                // 考勤类型
                { "NA", 1 },
                { "LA", 2 },
                { "EA", 3 },
                { "AB", 4 },
                { "OT", 5 },
                { "WF", 6 },
                { "RS", 7 },
                { "AW", 8 },
                { "TR", 9 },
                { "IW", 10 },
                { "CP", 11 },

                // 请假类型
                { "AL", 12 },
                { "SL", 13 },
                { "PL", 14 },
                { "ML", 15 },
                { "PPL", 16 },
                { "BL", 17 },
                { "FL", 18 },
                { "CL", 19 },
                { "GL", 20 },
                { "RL", 21 },
                { "TL", 22 },
                { "MCL", 23 },
                { "MNL", 24 },
                { "PEL", 25 },

                // 节假日和法定假期
                { "PH", 26 },
                { "SH", 27 },
                { "BD", 28 },
                { "RD", 29 },
                { "WD", 30 },
                { "OD", 31 }
        };
    }
}
