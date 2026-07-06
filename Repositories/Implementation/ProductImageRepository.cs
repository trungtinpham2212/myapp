using Repositories.DBContext;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementation;

public class ProductImageRepository : GenericRepository<ProductImage>, IProductImageRepository
{
    public ProductImageRepository(myappContext dbContext) : base(dbContext)
    {
    }
}
