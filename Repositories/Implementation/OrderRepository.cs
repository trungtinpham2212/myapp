using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interfaces;

namespace Repositories.Implementation;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(myappContext dbContext) : base(dbContext)
    {
    }

    public async Task<Order?> GetOrderWithDetailsAsync(long orderId)
    {
        return await _dbContext.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }
}
