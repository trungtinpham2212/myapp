using Repositories.DBContext;
using Repositories.Interfaces;
using Repositories.Models;

namespace Repositories.Implementation;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(myappContext dbContext) : base(dbContext)
    {
    }
}
