using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetOrderWithDetailsAsync(long orderId);
}
