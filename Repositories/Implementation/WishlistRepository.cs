using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementation;

public class WishlistRepository : GenericRepository<Wishlist>, IWishlistRepository
{
    public WishlistRepository(myappContext dbContext) : base(dbContext)
    {
    }

    public async Task<Wishlist?> GetUserWishlistForProductAsync(Guid userId, long productId)
    {
        return await _dbContext.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
    }

    public async Task<System.Collections.Generic.IReadOnlyList<Wishlist>> GetUserWishlistAsync(Guid userId)
    {
        return await _dbContext.Wishlists
            .Include(w => w.Product)
            .Where(w => w.UserId == userId)
            .ToListAsync();
    }
}
