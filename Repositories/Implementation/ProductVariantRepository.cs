using Repositories.DBContext;
using Repositories.Interface;
using Repositories.Models;

namespace Repositories.Implementation;

public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
{
    public ProductVariantRepository(myappContext dbContext) : base(dbContext)
    {
    }
}
