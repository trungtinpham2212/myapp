using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Interface;
using Repositories.Models;

namespace Repositories.Implementation;

public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
{
    public ChatMessageRepository(myappContext context) : base(context)
    {
    }

    public async Task<List<ChatMessage>> GetMessagesByRoomIdAsync(long chatRoomId)
    {
        return await _dbContext.ChatMessages
            .Where(m => m.ChatRoomId == chatRoomId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetUnreadMessagesCountAsync(long chatRoomId, Guid receiverId)
    {
        // Tin nhắn chưa đọc là tin nhắn mà SenderId != receiverId và IsRead = false
        // receiverId ở đây là ID của người đang mở chat (ví dụ User đang mở chat thì Sender là Admin, Admin mở chat thì Sender là User)
        return await _dbContext.ChatMessages
            .Where(m => m.ChatRoomId == chatRoomId && m.SenderId != receiverId && (m.IsRead == false || m.IsRead == null))
            .CountAsync();
    }

    public async Task<List<ChatMessage>> GetUnreadMessagesAsync(long chatRoomId, Guid receiverId)
    {
        return await _dbContext.ChatMessages
            .Where(m => m.ChatRoomId == chatRoomId && m.SenderId != receiverId && (m.IsRead == false || m.IsRead == null))
            .ToListAsync();
    }
}
