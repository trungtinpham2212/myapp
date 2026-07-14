using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Services.Interface;
using Services.BM;

namespace myapp.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue("UserId") ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (!string.IsNullOrEmpty(userId))
        {
            // Join a group for their own user id to receive personal messages
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

            if (role == "2") // Admin
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
        }

        await base.OnConnectedAsync();
    }

    // Client calls this to send a message
    public async Task SendMessage(long chatRoomId, string messageText)
    {
        var userIdStr = Context.User?.FindFirstValue("UserId") ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid senderId)) return;

        var request = new SendMessageRequestDto { MessageText = messageText };
        var response = await _chatService.SendMessageAsync(chatRoomId, senderId, request);

        if (response.Success && response.Data != null)
        {
            var msg = response.Data;
            // Broadcast to everyone in the room
            await Clients.Group($"Room_{chatRoomId}").SendAsync("ReceiveMessage", msg);
            // Also notify all admins that there's a new message (for the room list)
            await Clients.Group("Admins").SendAsync("RoomUpdated", chatRoomId);
        }
    }
    
    // Client calls this when they open a chat room
    public async Task JoinRoom(long chatRoomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{chatRoomId}");
    }

    public async Task LeaveRoom(long chatRoomId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Room_{chatRoomId}");
    }
}
