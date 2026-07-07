using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interface;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IReadOnlyList<Product>> GetFilteredProductsAsync(string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, int page, int limit);
    Task<int> CountFilteredProductsAsync(string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice);
    Task<Product?> GetProductWithDetailsAsync(long productId);
    Task<List<(long ProductId, long VariantId, int TotalSold, decimal TotalRevenue)>> GetTopSellingVariantStatsAsync();
    Task<List<Product>> GetProductsByIdsAsync(List<long> productIds);
}
