using PunchPal.Core.Models;
using PunchPal.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace PunchPal.Core.Services
{
    public class WorkingTimeRangeService : IService<WorkingTimeRange>
    {
        public static readonly WorkingTimeRangeService Instance;

        static WorkingTimeRangeService()
        {
            Instance = new WorkingTimeRangeService();
        }

        private void InitData(PunchDbContext context)
        {
            if (context.WorkingTimeRanges.Any())
            {
                return;
            }
            var userId = SettingsModel.Load().Common.CurrentUser?.UserId;
            var record = new WorkingTimeRange
            {
                Date = 0,
                Type = WorkingTimeRangeType.Work,
                StartHour = 9,
                StartMinute = 0,
                EndHour = 17,
                EndMinute = 0,
                UserId = userId
            };
            context.WorkingTimeRanges.Add(record);
            context.SaveChanges();
        }

        public async Task<bool> Add(WorkingTimeRange entity)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    context.WorkingTimeRanges.AddOrUpdate(entity);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> Add(IList<WorkingTimeRange> entities)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    foreach (var entity in entities)
                    {
                        context.WorkingTimeRanges.AddOrUpdate(entity);
                    }
                    await context.SaveChangesAsync();
                }
                return entities.Count;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<bool> Remove(WorkingTimeRange entity)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var record = await context.WorkingTimeRanges.FirstOrDefaultAsync(m => m.Date == entity.Date && m.Type == entity.Type);
                    context.WorkingTimeRanges.Remove(record);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<WorkingTimeRange>> List(Expression<Func<WorkingTimeRange, bool>> predicate)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    InitData(context);
                    var result = await context.WorkingTimeRanges.Where(predicate).OrderBy(m => m.Date).ThenBy(m => m.Type).ToListAsync();
                    return result;
                }
            }
            catch (Exception)
            {
                return new List<WorkingTimeRange>();
            }
        }
    }
}
