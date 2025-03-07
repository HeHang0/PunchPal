using PunchPal.Core.Models;
using PunchPal.Tools;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PunchPal.Core.Services
{
    public class PunchRecordService : IService<PunchRecord>
    {
        public static readonly PunchRecordService Instance;

        static PunchRecordService()
        {
            Instance = new PunchRecordService();
        }

        public async Task<bool> Add(PunchRecord entity)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    context.PunchRecords.AddOrUpdate(m => new
                    {
                        m.PunchTime,
                        m.UserId
                    }, entity);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> Add(IEnumerable<PunchRecord> entities)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var count = 0;
                    foreach (var entity in entities)
                    {
                        var existingEntity = context.PunchRecords.FirstOrDefaultAsync(m => m.UserId == entity.UserId && m.PunchTime == entity.PunchTime);
                        if (existingEntity != null)
                        {
                            context.Entry(existingEntity).CurrentValues.SetValues(existingEntity);
                        }
                        else
                        {
                            context.PunchRecords.Add(entity);
                        }
                        count++;
                    }
                    await context.SaveChangesAsync();
                    return count;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> Remove(PunchRecord entity)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var record = await context.PunchRecords.FirstOrDefaultAsync(m => m.UserId == entity.UserId && m.PunchTime == entity.PunchTime);
                    context.PunchRecords.Remove(record);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<PunchRecord>> List(Expression<Func<PunchRecord, bool>> predicate)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    return await context.PunchRecords.Where(predicate).OrderByDescending(m => m.PunchTime).ToListAsync();
                }
            }
            catch (Exception)
            {
                return new List<PunchRecord>();
            }
        }

        public async Task<PunchRecord> TodayFirst(int dayStartHour = 6)
        {
            try
            {
                var now = DateTime.Now;
                var dayStart = new DateTime(now.Year, now.Month, now.Day, dayStartHour, 0, 0);
                var dayStartUnix = dayStart.TimestampUnix();
                using (var context = new PunchDbContext())
                {
                    return await context.PunchRecords.Where(m => m.PunchTime > dayStartUnix).OrderByDescending(m => m.PunchTime).FirstOrDefaultAsync();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task ImportFromFile(string fileName, string userId)
        {
            if (string.IsNullOrWhiteSpace(fileName) || File.Exists(fileName))
            {
                return;
            }
            var text = File.ReadAllText(fileName);
            var matches = Regex.Matches(text,
                "([\\d]{4}-[\\d]{2}-[\\d]{2} [\\d]{2}:[\\d]{2}:[\\d]{2})[\\s]+[\\n]([\\S]+)[\\s]+");
            var records = new List<PunchRecord>();
            foreach (Match match in matches)
            {
                var dateText = match.Groups[1].Value;
                var remark = match.Groups[2].Value;
                if (!DateTime.TryParse(dateText, out var date))
                {
                    continue;
                }

                var record = new PunchRecord
                {
                    PunchTime = date.TimestampUnix(),
                    Remark = remark,
                    UserId = userId,
                    PunchType = PunchRecord.PunchTypeFile
                };
                records.Add(record);
            }
            await Add(records);
        }
    }
}
