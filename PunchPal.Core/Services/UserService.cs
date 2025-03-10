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
    public class UserService : IService<User>
    {
        public static readonly UserService Instance;

        public async Task<bool> Add(User entity)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    context.Users.AddOrUpdate(entity);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> Remove(User entity)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var count = await context.Users.CountAsync();
                    if (count <= 1)
                    {
                        return false;
                    }
                    var user = await context.Users.FirstOrDefaultAsync(m => m.UserId == entity.UserId);
                    context.Users.Remove(user);
                    await context.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<User> FirstOrDefaultAsync()
        {
            return await FirstOrDefaultAsync(m => true);
        }

        public User FirstOrDefault()
        {
            return FirstOrDefault(m => true);
        }

        public async Task<User> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate)
        {
            using (var context = new PunchDbContext())
            {
                return await context.Users.FirstOrDefaultAsync(predicate);
            }
        }

        public User FirstOrDefault(Expression<Func<User, bool>> predicate)
        {
            using (var context = new PunchDbContext())
            {
                return context.Users.FirstOrDefault(predicate);
            }
        }

        public async Task<int> Add(IEnumerable<User> entities)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    var count = 0;
                    foreach (var entity in entities)
                    {
                        var existingEntity = await context.Users.FirstOrDefaultAsync(m => m.UserId == entity.UserId);
                        if (existingEntity != null)
                        {
                            context.Entry(existingEntity).CurrentValues.SetValues(existingEntity);
                        }
                        else
                        {
                            context.Users.Add(entity);
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

        public async Task<List<User>> List(Expression<Func<User, bool>> predicate = null)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    if (predicate == null)
                    {
                        return await context.Users.ToListAsync();
                    }
                    return await context.Users.Where(predicate).ToListAsync();
                }
            }
            catch (Exception)
            {
                return new List<User>();
            }
        }

        public static List<string> RecordUsers(PunchDbContext context)
        {
            try
            {
                return context.PunchRecords.Select(m => m.UserId).GroupBy(m => m).Select(m => m.Key).ToList();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }

        static UserService()
        {
            Instance = new UserService();
            using (var context = new PunchDbContext())
            {
                if (context.Users.Any())
                {
                    return;
                }
                if (!context.PunchRecords.Any())
                {
                    OldDataService.ImportFromOldDatabase(context);
                }
                var existsUsers = RecordUsers(context);
                if (!existsUsers.Any())
                {
                    existsUsers.Add(Guid.NewGuid().ToString("N").Substring(0, 8));
                }
                foreach (var id in existsUsers)
                {
                    var entity = new User
                    {
                        UserId = id,
                        Name = "用户" + id,
                        Remark = "初始用户"
                    };
                    var existingEntity = context.Users.FirstOrDefault(m => m.UserId == entity.UserId);
                    if (existingEntity != null)
                    {
                        context.Entry(existingEntity).CurrentValues.SetValues(existingEntity);
                    }
                    else
                    {
                        context.Users.Add(entity);
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
