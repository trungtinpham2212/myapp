using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces;

public interface ICartItemRepository : IGenericRepository<CartItem>
{
    Task<IReadOnlyList<CartItem>> GetCartItemsWithDetailsAsync(long cartId);
    Task<CartItem?> GetCartItemByCartAndVariantAsync(long cartId, long productVariantId);
}
