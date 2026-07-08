using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(myappContext dbContext) : base(dbContext)
    {
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid? userId, int skip, int take)
    {
        var query = _dbContext.Notifications.AsQueryable();
        if (userId.HasValue)
            query = query.Where(n => n.UserId == userId.Value);
        else
            query = query.Where(n => n.UserId == null);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountUnreadNotificationsAsync(Guid? userId)
    {
        var query = _dbContext.Notifications.Where(n => n.IsRead == false);
        if (userId.HasValue)
            query = query.Where(n => n.UserId == userId.Value);
        else
            query = query.Where(n => n.UserId == null);

        return await query.CountAsync();
    }

    public async Task MarkAllAsReadAsync(Guid? userId)
    {
        var query = _dbContext.Notifications.Where(n => n.IsRead == false);
        if (userId.HasValue)
            query = query.Where(n => n.UserId == userId.Value);
        else
            query = query.Where(n => n.UserId == null);

        var unreadNotis = await query.ToListAsync();

        foreach (var noti in unreadNotis)
        {
            noti.IsRead = true;
            noti.UpdatedAt = DateTime.UtcNow;
        }
    }
}
