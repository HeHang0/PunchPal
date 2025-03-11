using PunchPal.Core.Models;
using PunchPal.Core.ViewModels;
using PunchPal.Tools;
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

        public async Task<int> Add(IEnumerable<WorkingTimeRange> entities)
        {
            try
            {
                entities = entities.GroupBy(m => new { m.Type, m.Date }).Select(m => m.First());
                using (var context = new PunchDbContext())
                {
                    var count = 0;
                    foreach (var entity in entities)
                    {
                        context.WorkingTimeRanges.AddOrUpdate(entity);
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

        public async Task<WorkingTimeRangeItems> CurrentItems()
        {
            var date = DateTime.Now;
            var startDate = new DateTime(date.Year, date.Month, 1);
            var endDate = startDate.AddMonths(1);
            var result = await Items(startDate.TimestampUnix(), endDate.TimestampUnix());
            return result[date.Date.TimestampUnix()];
        }

        public async Task<Dictionary<long, WorkingTimeRangeItems>> Items(long startDate, long endDate)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    InitData(context);
                    var data = await context.WorkingTimeRanges.Where(m => m.Date == 0 || (m.Date >= startDate && m.Date < endDate)).OrderBy(m => m.Date).ThenBy(m => m.Type).ToListAsync();
                    var result = new Dictionary<long, WorkingTimeRangeItems>();
                    var startDateTime = startDate.Unix2DateTime().Date;
                    var endDateTime = endDate.Unix2DateTime().Date;
                    var commonData = data.Where(m => m.Date == 0);
                    var commonWork = commonData.FirstOrDefault(m => m.Type == WorkingTimeRangeType.Work);
                    var commonLunch = commonData.FirstOrDefault(m => m.Type == WorkingTimeRangeType.Lunch);
                    var commonDinner = commonData.FirstOrDefault(m => m.Type == WorkingTimeRangeType.Dinner);
                    for (var date = startDateTime; date <= endDateTime; date = date.AddDays(1))
                    {
                        var currentUnix = date.TimestampUnix();
                        var current = data.Where(m => m.Date == currentUnix);
                        var item = new WorkingTimeRangeItems();
                        item.Work = current.FirstOrDefault(m => m.Type == WorkingTimeRangeType.Work) ?? commonWork;
                        item.Lunch = current.FirstOrDefault(m => m.Type == WorkingTimeRangeType.Lunch) ?? commonLunch;
                        item.Dinner = current.FirstOrDefault(m => m.Type == WorkingTimeRangeType.Dinner) ?? commonDinner;
                        result[currentUnix] = item;
                    }
                    return result;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
