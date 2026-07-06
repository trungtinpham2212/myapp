using System;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetCartByUserIdAsync(Guid userId);
}
