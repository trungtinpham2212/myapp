using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interface;

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
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<(int TotalSuccessfulOrders, decimal TotalRevenue)> GetOrderStatsAsync(System.DateTime? fromDate = null, System.DateTime? toDate = null)
    {
        var successfulOrdersQuery = _dbContext.Orders.Where(o => o.PaymentStatus == "Success");
        if (fromDate.HasValue) successfulOrdersQuery = successfulOrdersQuery.Where(o => o.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) successfulOrdersQuery = successfulOrdersQuery.Where(o => o.CreatedAt <= toDate.Value);
        
        int count = await successfulOrdersQuery.CountAsync();
        decimal revenue = await successfulOrdersQuery.SumAsync(o => o.FinalAmount);
        return (count, revenue);
    }

    public async Task<int> CountIncompleteOrdersByUserIdAsync(System.Guid userId)
    {
        return await _dbContext.Orders
            .CountAsync(o => o.UserId == userId && o.OrderStatus != "Completed" && o.OrderStatus != "Cancelled");
    }

    public async Task<decimal> GetTotalSpentByUserIdAsync(System.Guid userId)
    {
        return await _dbContext.Orders
            .Where(o => o.UserId == userId && o.PaymentStatus == "Success")
            .SumAsync(o => o.FinalAmount);
    }

    public async Task<System.Collections.Generic.List<(System.DateTime? CreatedAt, decimal FinalAmount)>> GetSuccessfulOrdersRevenueAsync(System.DateTime startDate, System.DateTime endDate)
    {
        var query = _dbContext.Orders
            .Where(o => o.PaymentStatus == "Success" && o.CreatedAt != null && o.CreatedAt >= startDate && o.CreatedAt < endDate);

        var orders = await query
            .Select(o => new { o.CreatedAt, o.FinalAmount })
            .ToListAsync();
            
        return orders.Select(o => (o.CreatedAt, o.FinalAmount)).ToList();
    }

    public async Task<System.Collections.Generic.List<(int CategoryId, string CategoryName, decimal Revenue)>> GetRevenueByCategoryAsync(System.DateTime? fromDate = null, System.DateTime? toDate = null)
    {
        var query = _dbContext.OrderDetails
            .Where(od => od.Order.PaymentStatus == "Success");

        if (fromDate.HasValue) query = query.Where(od => od.Order.CreatedAt >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(od => od.Order.CreatedAt <= toDate.Value);

        var categoryStats = await query
            .GroupBy(od => new { od.ProductVariant.Product.CategoryId, CategoryName = od.ProductVariant.Product.Category.Name })
            .Select(g => new
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName ?? "Không xác định",
                Revenue = g.Sum(x => x.Quantity * x.PriceAtPurchase)
            })
            .OrderByDescending(c => c.Revenue)
            .ToListAsync();
            
        return categoryStats.Select(c => (c.CategoryId, c.CategoryName, c.Revenue)).ToList();
    }
}
