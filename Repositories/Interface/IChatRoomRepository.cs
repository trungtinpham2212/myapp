using System;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface IChatRoomRepository : IGenericRepository<ChatRoom>
{
    Task<ChatRoom?> GetChatRoomByUserIdAsync(Guid userId);
    Task<System.Collections.Generic.List<ChatRoom>> GetAllRoomsWithUserAsync();
}
