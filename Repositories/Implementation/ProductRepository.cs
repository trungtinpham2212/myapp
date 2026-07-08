using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interface;

namespace Repositories.Implementation;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(myappContext dbContext) : base(dbContext)
    {
    }

    private IQueryable<Product> BuildFilterQuery(string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice)
    {
        var query = _dbContext.Products.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => p.Name.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (brandId.HasValue)
            query = query.Where(p => p.BrandId == brandId.Value);

        if (minPrice.HasValue)
            query = query.Where(p => p.MinPrice.HasValue && p.MinPrice.Value >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.MaxPrice.HasValue && p.MaxPrice.Value <= maxPrice.Value);

        query = query.Where(p => p.Status == "active");

        return query;
    }

    public async Task<IReadOnlyList<Product>> GetFilteredProductsAsync(string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice, int page, int limit)
    {
        var query = BuildFilterQuery(search, categoryId, brandId, minPrice, maxPrice);

        return await query
            .OrderByDescending(p => p.IsFeatured)
            .ThenByDescending(p => p.ProductId)
            .Skip((page - 1) * limit)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CountFilteredProductsAsync(string? search, int? categoryId, int? brandId, decimal? minPrice, decimal? maxPrice)
    {
        var query = BuildFilterQuery(search, categoryId, brandId, minPrice, maxPrice);
        return await query.CountAsync();
    }

    public async Task<Product?> GetProductWithDetailsAsync(long productId)
    {
        return await _dbContext.Products
            .Include(p => p.ProductVariants)
            .Include(p => p.ProductImages)
            .FirstOrDefaultAsync(p => p.ProductId == productId);
    }

    public async Task<List<(long ProductId, long VariantId, int TotalSold, decimal TotalRevenue)>> GetTopSellingVariantStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dbContext.OrderDetails
            .Where(od => od.Order.PaymentStatus == "Success");

        if (fromDate.HasValue)
        {
            query = query.Where(od => od.Order.CreatedAt >= fromDate.Value);
        }
        
        if (toDate.HasValue)
        {
            // toDate.Value.AddDays(1) can be used if they send date without time, but let's assume exact DateTime is passed or handled by service
            query = query.Where(od => od.Order.CreatedAt <= toDate.Value);
        }

        var stats = await query
            .GroupBy(od => new { od.ProductVariantId, od.ProductVariant.ProductId })
            .Select(g => new
            {
                ProductId = g.Key.ProductId,
                VariantId = g.Key.ProductVariantId,
                TotalSold = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Quantity * x.PriceAtPurchase)
            })
            .ToListAsync();

        return stats.Select(s => (s.ProductId, s.VariantId, s.TotalSold, s.TotalRevenue)).ToList();
    }

    public async Task<List<Product>> GetProductsByIdsAsync(List<long> productIds)
    {
        return await _dbContext.Products
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync();
    }
}
