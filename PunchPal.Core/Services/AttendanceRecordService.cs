using PunchPal.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PunchPal.Core.Services
{
    public class AttendanceRecordService : IService<AttendanceRecord>
    {
        public static readonly AttendanceRecordService Instance;

        static AttendanceRecordService()
        {
            Instance = new AttendanceRecordService();
        }

        public async Task<bool> Add(AttendanceRecord entity)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    context.AttendanceRecords.AddOrUpdate(entity);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> Add(IEnumerable<AttendanceRecord> entities)
        {
            try
            {
                var count = 0;
                using (var context = new PunchDbContext())
                {
                    foreach (var entity in entities)
                    {
                        var existingEntity = context.AttendanceRecords.FirstOrDefaultAsync(m => m.AttendanceId == entity.AttendanceId);
                        if (existingEntity != null)
                        {
                            context.Entry(existingEntity).CurrentValues.SetValues(existingEntity);
                        }
                        else
                        {
                            context.AttendanceRecords.Add(entity);
                        }
                        count++;
                    }
                    await context.SaveChangesAsync();
                }
                return count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> Remove(AttendanceRecord entity)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var record = await context.AttendanceRecords.FirstOrDefaultAsync(m => m.UserId == entity.UserId && m.AttendanceId == entity.AttendanceId);
                    context.AttendanceRecords.Remove(record);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<AttendanceRecord>> List(Expression<Func<AttendanceRecord, bool>> predicate)
        {
            try
            {
                var attendanceTypes = await AttendanceTypeService.Instance.ToDictionory();
                using (var context = new PunchDbContext())
                {
                    var result = await context.AttendanceRecords.Where(predicate).OrderByDescending(m => m.StartTime).ToListAsync();
                    foreach (var item in result)
                    {
                        item.AttendanceType = attendanceTypes.ContainsKey(item.AttendanceTypeId) ? attendanceTypes[item.AttendanceTypeId] : string.Empty;
                    }
                    return result;
                }
            }
            catch (Exception)
            {
                return new List<AttendanceRecord>();
            }
        }
    }
}
