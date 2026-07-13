using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class ReviewRepository : GenericRepository<Review>, IReviewRepository
{
    public ReviewRepository(myappContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<Review>> GetReviewsByProductIdAsync(long productId)
    {
        return await _dbContext.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<(IReadOnlyList<Review> items, int total)> GetFilteredReviewsAsync(long? productId, Guid? userId, int? star, int page, int limit)
    {
        var query = _dbContext.Reviews.AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (star.HasValue)
        {
            query = query.Where(r => r.RatingStars == star.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (items, total);
    }
}
