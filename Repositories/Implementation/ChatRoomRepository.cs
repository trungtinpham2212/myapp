using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Interface;
using Repositories.Models;

namespace Repositories.Implementation;

public class ChatRoomRepository : GenericRepository<ChatRoom>, IChatRoomRepository
{
    public ChatRoomRepository(myappContext context) : base(context)
    {
    }

    public async Task<ChatRoom?> GetChatRoomByUserIdAsync(Guid userId)
    {
        return await _dbContext.ChatRooms.FirstOrDefaultAsync(r => r.UserId == userId);
    }

    public async Task<System.Collections.Generic.List<ChatRoom>> GetAllRoomsWithUserAsync()
    {
        return await _dbContext.ChatRooms
            .Include(r => r.User)
            .OrderByDescending(r => r.UpdatedAt)
            .ToListAsync();
    }
}
