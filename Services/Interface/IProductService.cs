using System.Collections.Generic;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface IProductService
{
    Task<ApiResponse<List<ProductDto>>> GetProductsAsync(string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, int page = 1, int limit = 8);
    Task<ApiResponse<ProductQuickViewDto>> GetQuickViewAsync(long productId);
}
