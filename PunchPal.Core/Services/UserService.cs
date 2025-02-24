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

        public async Task<int> Add(IList<User> entities)
        {
            try
            {
                using (var context = new PunchDbContext())
                {
                    foreach (var entity in entities)
                    {
                        context.Users.AddOrUpdate(entity);
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
                if (existsUsers.Any())
                {
                    foreach (var id in existsUsers)
                    {
                        context.Users.AddOrUpdate(new User
                        {
                            UserId = id,
                            Name = "用户" + id,
                            Remark = "初始用户"
                        });
                    }
                    context.SaveChanges();
                    return;
                }
                var userId = Guid.NewGuid().ToString("N").Substring(0, 8);
                var user = new User
                {
                    UserId = userId,
                    Name = "User",
                    Remark = "初始用户"
                };
                context.Users.Add(user);
                context.SaveChanges();
            }
        }
    }
}
