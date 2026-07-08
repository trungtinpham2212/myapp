using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Models;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationPublisher _notificationPublisher;

    public NotificationService(IUnitOfWork unitOfWork, INotificationPublisher notificationPublisher)
    {
        _unitOfWork = unitOfWork;
        _notificationPublisher = notificationPublisher;
    }

    public async Task PushNotificationAsync(Guid? userId, string title, string content, string type, string targetType, string targetId)
    {
        var noti = new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = type,
            TargetType = targetType,
            TargetId = targetId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.NotificationRepository.AddAsync(noti);
        await _unitOfWork.SaveChangesAsync();

        var notiDto = new NotificationDto
        {
            NotificationId = noti.NotificationId,
            UserId = noti.UserId,
            Title = noti.Title,
            Content = noti.Content,
            Type = noti.Type,
            IsRead = false,
            TargetType = noti.TargetType,
            TargetId = noti.TargetId,
            CreatedAt = noti.CreatedAt
        };

        if (userId.HasValue)
        {
            await _notificationPublisher.PushToUserAsync(userId.Value, notiDto);
        }
        else
        {
            // Push to Admin
            await _notificationPublisher.PushToAdminAsync(notiDto);
        }
    }

    public async Task<ApiResponse<List<NotificationDto>>> GetUserNotificationsAsync(Guid? userId, int page = 1, int limit = 20)
    {
        int skip = (page - 1) * limit;
        var notis = await _unitOfWork.NotificationRepository.GetUserNotificationsAsync(userId, skip, limit);

        var result = notis.Select(n => new NotificationDto
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            Title = n.Title,
            Content = n.Content,
            Type = n.Type,
            IsRead = n.IsRead ?? false,
            TargetType = n.TargetType,
            TargetId = n.TargetId,
            CreatedAt = n.CreatedAt
        }).ToList();

        return new ApiResponse<List<NotificationDto>>
        {
            Success = true,
            Data = result
        };
    }

    public async Task<ApiResponse<int>> GetUnreadCountAsync(Guid? userId)
    {
        int count = await _unitOfWork.NotificationRepository.CountUnreadNotificationsAsync(userId);
        
        return new ApiResponse<int>
        {
            Success = true,
            Data = count
        };
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(Guid? userId)
    {
        await _unitOfWork.NotificationRepository.MarkAllAsReadAsync(userId);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<bool>
        {
            Success = true,
            Data = true
        };
    }
}
