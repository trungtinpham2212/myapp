using Repositories.DBContext;
using Repositories.Interface;
using Repositories.Models;

namespace Repositories.Implementation;

public class BrandRepository : GenericRepository<Brand>, IBrandRepository
{
    public BrandRepository(myappContext dbContext) : base(dbContext)
    {
    }
}
