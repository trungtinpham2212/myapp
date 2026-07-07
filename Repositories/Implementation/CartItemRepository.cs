using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{
    public CartItemRepository(myappContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<CartItem>> GetCartItemsWithDetailsAsync(long cartId)
    {
        return await _dbContext.CartItems
            .Where(ci => ci.CartId == cartId)
            .Include(ci => ci.ProductVariant)
            .ThenInclude(v => v.Product)
            .OrderByDescending(ci => ci.AddedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<CartItem?> GetCartItemByCartAndVariantAsync(long cartId, long productVariantId)
    {
        return await _dbContext.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.ProductVariantId == productVariantId);
    }
}
