using System;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface IOrderService
{
    Task<ApiResponse<CreateOrderResponseDto>> CreateOrderAsync(Guid userId, CreateOrderRequest request);
    Task<ApiResponse<OrderDto>> GetOrderDetailsAsync(long orderId);
}
