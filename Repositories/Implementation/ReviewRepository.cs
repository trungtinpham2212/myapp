using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interfaces;

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
            .ToListAsync();
    }
}
