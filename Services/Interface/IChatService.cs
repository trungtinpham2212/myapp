using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface IChatService
{
    // For Admin: Get all chat rooms
    Task<ApiResponse<List<ChatRoomDto>>> GetAllChatRoomsAsync(Guid adminId);
    
    // For Customer: Get or create their chat room
    Task<ApiResponse<ChatRoomDto>> GetOrCreateChatRoomAsync(Guid userId);
    
    // For both: Get messages in a room
    Task<ApiResponse<List<ChatMessageDto>>> GetMessagesAsync(long chatRoomId, Guid currentUserId);
    
    // For both: Send a message
    Task<ApiResponse<ChatMessageDto>> SendMessageAsync(long chatRoomId, Guid senderId, SendMessageRequestDto request);
    
    // Mark messages as read
    Task<ApiResponse<bool>> MarkMessagesAsReadAsync(long chatRoomId, Guid currentUserId);
}
