using System;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces;

public interface IWishlistRepository : IGenericRepository<Wishlist>
{
    Task<Wishlist?> GetUserWishlistForProductAsync(Guid userId, long productId);
    Task<System.Collections.Generic.IReadOnlyList<Wishlist>> GetUserWishlistAsync(Guid userId);
}
