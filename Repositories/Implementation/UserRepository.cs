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

    public async Task<(System.Collections.Generic.IReadOnlyList<User> items, int total)> GetCustomersAsync(int page, int limit)
    {
        var query = _dbContext.Users.Where(u => u.Role == 1);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
            
        return (items, total);
    }

    public async Task<User?> GetCustomerDetailsAsync(System.Guid userId)
    {
        return await _dbContext.Users
            .Include(u => u.Orders)
            .Include(u => u.Reviews)
                .ThenInclude(r => r.Product)
            .FirstOrDefaultAsync(u => u.UserId == userId && u.Role == 1);
    }
}
