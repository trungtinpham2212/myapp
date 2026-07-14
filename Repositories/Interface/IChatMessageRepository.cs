using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
    Task<List<ChatMessage>> GetMessagesByRoomIdAsync(long chatRoomId);
    Task<int> GetUnreadMessagesCountAsync(long chatRoomId, Guid receiverId);
    Task<List<ChatMessage>> GetUnreadMessagesAsync(long chatRoomId, Guid receiverId);
}
