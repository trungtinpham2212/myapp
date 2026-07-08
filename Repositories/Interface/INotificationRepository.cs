using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<List<Notification>> GetUserNotificationsAsync(Guid? userId, int skip, int take);
    Task<int> CountUnreadNotificationsAsync(Guid? userId);
    Task MarkAllAsReadAsync(Guid? userId);
}
