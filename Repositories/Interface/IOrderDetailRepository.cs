using Repositories.Models;

namespace Repositories.Interface;

public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
{
    Task<int> CountIncompleteOrdersByVariantIdsAsync(List<long> variantIds);
}
