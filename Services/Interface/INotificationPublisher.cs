using System;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface INotificationPublisher
{
    Task PushToAdminAsync(NotificationDto noti);
    Task PushToUserAsync(Guid userId, NotificationDto noti);
}
