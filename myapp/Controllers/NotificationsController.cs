using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace myapp.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] Guid? userId, [FromQuery] int page = 1, [FromQuery] int limit = 20)
    {
        var response = await _notificationService.GetUserNotificationsAsync(userId, page, limit);
        return Ok(response);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount([FromQuery] Guid? userId)
    {
        var response = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(response);
    }

    [HttpPut("mark-all-read")]
    public async Task<IActionResult> MarkAllAsRead([FromQuery] Guid? userId)
    {
        var response = await _notificationService.MarkAllAsReadAsync(userId);
        return Ok(response);
    }
}
