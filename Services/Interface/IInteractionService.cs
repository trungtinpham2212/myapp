using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface IInteractionService
{
    Task<ApiResponse<object>> CreateReviewAsync(Guid userId, long productId, CreateReviewRequest request);
    Task<ApiResponse<List<ReviewDto>>> GetReviewsAsync(long productId);
    Task<ApiResponse<List<ReviewDto>>> GetAllReviewsFilteredAsync(long? productId, Guid? userId, int? star, int page = 1, int limit = 10);
    Task<ToggleWishlistResponse> ToggleWishlistAsync(Guid userId, ToggleWishlistRequest request);
    Task<ApiResponse<List<ProductDto>>> GetWishlistAsync(Guid userId);
}
