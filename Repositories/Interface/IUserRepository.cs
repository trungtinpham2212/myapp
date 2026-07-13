using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
    Task<int> CountUsersAsync(System.DateTime? fromDate = null, System.DateTime? toDate = null);
    Task<(System.Collections.Generic.IReadOnlyList<User> items, int total)> GetCustomersAsync(int page, int limit);
    Task<User?> GetCustomerDetailsAsync(System.Guid userId);
}
