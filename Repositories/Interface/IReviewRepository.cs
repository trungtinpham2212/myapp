using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<(IReadOnlyList<Review> items, int total)> GetFilteredReviewsAsync(long? productId, Guid? userId, int? star, int page, int limit);
    Task<IReadOnlyList<Review>> GetReviewsByProductIdAsync(long productId);
}
