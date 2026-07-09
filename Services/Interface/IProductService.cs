using System.Collections.Generic;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface IProductService
{
    Task<ApiResponse<List<ProductDto>>> GetProductsAsync(string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, int page = 1, int limit = 8);
    Task<ApiResponse<ProductQuickViewDto>> GetQuickViewAsync(long productId);
    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request);
    Task<ApiResponse<ProductDto>> UpdateProductAsync(long productId, UpdateProductRequest request);
    Task<ApiResponse<bool>> DeleteProductAsync(long productId);
    Task<ApiResponse<ProductDto>> AddProductVariantsAsync(long productId, List<CreateProductVariantDto> variants);
    Task<ApiResponse<bool>> DeleteProductVariantAsync(long productId, long variantId);
}
