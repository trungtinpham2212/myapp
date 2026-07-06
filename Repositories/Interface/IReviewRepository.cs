using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces;

public interface IReviewRepository : IGenericRepository<Review>
{
    Task<IReadOnlyList<Review>> GetReviewsByProductIdAsync(long productId);
}
