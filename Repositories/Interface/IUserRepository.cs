using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
}
