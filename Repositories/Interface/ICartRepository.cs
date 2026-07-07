using System;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetCartByUserIdAsync(Guid userId);
}
