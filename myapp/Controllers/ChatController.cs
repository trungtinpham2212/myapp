using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using API.Extensions;
using Services.Interface;
using Services.BM;

namespace myapp.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ChatHub> _hubContext;

    public ChatController(IChatService chatService, Microsoft.AspNetCore.SignalR.IHubContext<Hubs.ChatHub> hubContext)
    {
        _chatService = chatService;
        _hubContext = hubContext;
    }

    // For Admin: get all rooms
    [HttpGet("rooms")]
    [Authorize(Roles = "2")]
    public async Task<IActionResult> GetAllRooms()
    {
        var adminId = User.GetUserId();
        var response = await _chatService.GetAllChatRoomsAsync(adminId);
        return Ok(response);
    }

    // For Customer: get their room
    [HttpGet("my-room")]
    [Authorize(Roles = "1")]
    public async Task<IActionResult> GetMyRoom()
    {
        var userId = User.GetUserId();
        var response = await _chatService.GetOrCreateChatRoomAsync(userId);
        return Ok(response);
    }

    // For Both: Get messages in a room
    [HttpGet("{roomId}/messages")]
    public async Task<IActionResult> GetMessages([FromRoute] long roomId)
    {
        var userId = User.GetUserId();
        var response = await _chatService.GetMessagesAsync(roomId, userId);
        
        // As a side-effect, when they fetch messages, mark them as read
        if (response.Success)
        {
            await _chatService.MarkMessagesAsReadAsync(roomId, userId);
        }
        
        return Ok(response);
    }

    // For Both: Mark messages as read manually
    [HttpPut("{roomId}/read")]
    public async Task<IActionResult> MarkAsRead([FromRoute] long roomId)
    {
        var userId = User.GetUserId();
        var response = await _chatService.MarkMessagesAsReadAsync(roomId, userId);
        return Ok(response);
    }

    // For Both: Send a message via HTTP
    [HttpPost("{roomId}/messages")]
    public async Task<IActionResult> SendMessage([FromRoute] long roomId, [FromBody] Services.BM.SendMessageRequestDto request)
    {
        var userId = User.GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var response = await _chatService.SendMessageAsync(roomId, userId, request);
        if (response.Success && response.Data != null)
        {
            // Broadcast via SignalR to connected clients
            var msg = response.Data;
            await _hubContext.Clients.Group($"Room_{roomId}").SendAsync("ReceiveMessage", msg);
            await _hubContext.Clients.Group("Admins").SendAsync("RoomUpdated", roomId);
            return Ok(response);
        }
        
        return BadRequest(response);
    }
}
