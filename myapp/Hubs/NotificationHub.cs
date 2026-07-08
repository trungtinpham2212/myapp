using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace myapp.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value 
                ?? Context.User?.FindFirst("role")?.Value
                ?? Context.User?.FindFirst("Role")?.Value;
        
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? Context.User?.FindFirst("id")?.Value;

        // Nếu là Admin, add vào group "Admin"
        if (role == "2" || role == "3" || role == "Admin") 
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Admin");
        }

        // Add user vào group theo UserId để push cá nhân
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value 
                ?? Context.User?.FindFirst("role")?.Value
                ?? Context.User?.FindFirst("Role")?.Value;
        
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? Context.User?.FindFirst("id")?.Value;

        if (role == "2" || role == "3" || role == "Admin")
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admin");
        }

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
