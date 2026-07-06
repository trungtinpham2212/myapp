using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementation;

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    public CartRepository(myappContext dbContext) : base(dbContext)
    {
    }

    public async Task<Cart?> GetCartByUserIdAsync(Guid userId)
    {
        return await _dbContext.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
    }
}
