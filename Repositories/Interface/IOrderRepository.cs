using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetOrderWithDetailsAsync(long orderId);
    Task<(int TotalSuccessfulOrders, decimal TotalRevenue)> GetOrderStatsAsync(System.DateTime? fromDate = null, System.DateTime? toDate = null);
    Task<System.Collections.Generic.List<(System.DateTime? CreatedAt, decimal FinalAmount)>> GetSuccessfulOrdersRevenueAsync(System.DateTime startDate, System.DateTime endDate);
    Task<System.Collections.Generic.List<(int CategoryId, string CategoryName, decimal Revenue)>> GetRevenueByCategoryAsync(System.DateTime? fromDate = null, System.DateTime? toDate = null);
}
