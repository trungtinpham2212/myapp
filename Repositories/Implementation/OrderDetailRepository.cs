using Repositories.DBContext;
using Repositories.Interface;
using Repositories.Models;

namespace Repositories.Implementation;

public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
{
    public OrderDetailRepository(myappContext dbContext) : base(dbContext)
    {
    }
}
