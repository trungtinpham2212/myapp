using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Interface;
using Repositories.Models;

namespace Repositories.Implementation;

public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
{
    public OrderDetailRepository(myappContext dbContext) : base(dbContext)
    {
    }

    public async Task<int> CountIncompleteOrdersByVariantIdsAsync(List<long> variantIds)
    {
        return await _dbContext.OrderDetails.CountAsync(od => 
            variantIds.Contains(od.ProductVariantId) && 
            (od.Order.OrderStatus == "Processing" || od.Order.OrderStatus == "Pending"));
    }
}
