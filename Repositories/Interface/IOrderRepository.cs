using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetOrderWithDetailsAsync(long orderId);
}
