using System;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface ICartService
{
    Task<ApiResponse<CartDto>> GetCartAsync(Guid userId);
    Task<ApiResponse<CartDto>> AddToCartAsync(Guid userId, AddToCartRequest request);
    Task<ApiResponse<object>> RemoveFromCartAsync(Guid userId, long cartItemId);
}
