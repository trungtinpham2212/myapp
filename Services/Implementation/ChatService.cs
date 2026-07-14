using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChatService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<ChatRoomDto>>> GetAllChatRoomsAsync(Guid adminId)
    {
        // Get all chat rooms, order by UpdatedAt desc
        var rooms = await _unitOfWork.ChatRoomRepository.GetAllRoomsWithUserAsync();

        var result = new List<ChatRoomDto>();
        foreach (var r in rooms)
        {
            var unreadCount = await _unitOfWork.ChatMessageRepository.GetUnreadMessagesCountAsync(r.ChatRoomId, adminId);
            result.Add(new ChatRoomDto
            {
                ChatRoomId = r.ChatRoomId,
                UserId = r.UserId,
                CustomerName = r.User?.FullName ?? "Unknown",
                CustomerAvatarUrl = r.User?.AvatarUrl ?? "",
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                UnreadCount = unreadCount
            });
        }

        return new ApiResponse<List<ChatRoomDto>> { Success = true, Data = result };
    }

    public async Task<ApiResponse<ChatRoomDto>> GetOrCreateChatRoomAsync(Guid userId)
    {
        var room = await _unitOfWork.ChatRoomRepository.GetChatRoomByUserIdAsync(userId);
        if (room == null)
        {
            room = new ChatRoom
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.ChatRoomRepository.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();
        }

        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        var unreadCount = await _unitOfWork.ChatMessageRepository.GetUnreadMessagesCountAsync(room.ChatRoomId, userId);

        var dto = new ChatRoomDto
        {
            ChatRoomId = room.ChatRoomId,
            UserId = room.UserId,
            CustomerName = user?.FullName ?? "Unknown",
            CustomerAvatarUrl = user?.AvatarUrl ?? "",
            CreatedAt = room.CreatedAt,
            UpdatedAt = room.UpdatedAt,
            UnreadCount = unreadCount
        };

        return new ApiResponse<ChatRoomDto> { Success = true, Data = dto };
    }

    public async Task<ApiResponse<List<ChatMessageDto>>> GetMessagesAsync(long chatRoomId, Guid currentUserId)
    {
        var room = await _unitOfWork.ChatRoomRepository.GetByIdAsync(chatRoomId);
        if (room == null) return new ApiResponse<List<ChatMessageDto>> { Success = false, Message = "Phòng chat không tồn tại" };

        var messages = await _unitOfWork.ChatMessageRepository.GetMessagesByRoomIdAsync(chatRoomId);
        
        var dtos = messages.Select(m => new ChatMessageDto
        {
            ChatMessageId = m.ChatMessageId,
            ChatRoomId = m.ChatRoomId,
            SenderId = m.SenderId,
            MessageText = m.MessageText,
            IsRead = m.IsRead ?? false,
            CreatedAt = m.CreatedAt
        }).ToList();

        return new ApiResponse<List<ChatMessageDto>> { Success = true, Data = dtos };
    }

    public async Task<ApiResponse<ChatMessageDto>> SendMessageAsync(long chatRoomId, Guid senderId, SendMessageRequestDto request)
    {
        var room = await _unitOfWork.ChatRoomRepository.GetByIdAsync(chatRoomId);
        if (room == null) return new ApiResponse<ChatMessageDto> { Success = false, Message = "Phòng chat không tồn tại" };

        var message = new ChatMessage
        {
            ChatRoomId = chatRoomId,
            SenderId = senderId,
            MessageText = request.MessageText,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ChatMessageRepository.AddAsync(message);
        
        room.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.ChatRoomRepository.Update(room);
        
        await _unitOfWork.SaveChangesAsync();

        var dto = new ChatMessageDto
        {
            ChatMessageId = message.ChatMessageId,
            ChatRoomId = message.ChatRoomId,
            SenderId = message.SenderId,
            MessageText = message.MessageText,
            IsRead = false,
            CreatedAt = message.CreatedAt
        };

        return new ApiResponse<ChatMessageDto> { Success = true, Data = dto };
    }

    public async Task<ApiResponse<bool>> MarkMessagesAsReadAsync(long chatRoomId, Guid currentUserId)
    {
        var room = await _unitOfWork.ChatRoomRepository.GetByIdAsync(chatRoomId);
        if (room == null) return new ApiResponse<bool> { Success = false, Message = "Phòng chat không tồn tại" };

        var unreadMessages = await _unitOfWork.ChatMessageRepository.GetUnreadMessagesAsync(chatRoomId, currentUserId);

        if (unreadMessages.Any())
        {
            foreach (var m in unreadMessages)
            {
                m.IsRead = true;
                // m is already tracked by EF Core DbContext, so we just need to save changes
            }
            await _unitOfWork.SaveChangesAsync();
        }

        return new ApiResponse<bool> { Success = true, Data = true };
    }
}
