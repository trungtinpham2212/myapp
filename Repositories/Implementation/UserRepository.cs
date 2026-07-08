using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Interface;
using Repositories.Models;

namespace Repositories.Implementation;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(myappContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }

    public async Task<int> CountUsersAsync(System.DateTime? fromDate = null, System.DateTime? toDate = null)
    {
        var usersQuery = _dbContext.Users.AsQueryable();
        if (fromDate.HasValue) usersQuery = usersQuery.Where(u => u.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) usersQuery = usersQuery.Where(u => u.CreatedAt <= toDate.Value);
        return await usersQuery.CountAsync();
    }
}
