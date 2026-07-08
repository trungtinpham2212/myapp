using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Services.BM;
using Services.Interface;

namespace myapp.Hubs;

public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PushToAdminAsync(NotificationDto noti)
    {
        await _hubContext.Clients.Group("Admin").SendAsync("ReceiveNotification", noti);
    }

    public async Task PushToUserAsync(Guid userId, NotificationDto noti)
    {
        await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", noti);
    }
}
