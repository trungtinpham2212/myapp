using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface INotificationService
{
    Task PushNotificationAsync(Guid? userId, string title, string content, string type, string targetType, string targetId);
    Task<ApiResponse<System.Collections.Generic.List<NotificationDto>>> GetUserNotificationsAsync(Guid? userId, int page = 1, int limit = 20);
    Task<ApiResponse<int>> GetUnreadCountAsync(Guid? userId);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid? userId);
}
