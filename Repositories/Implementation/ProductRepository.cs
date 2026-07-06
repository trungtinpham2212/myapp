using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.DBContext;
using Repositories.Models;
using Repositories.Interfaces;

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
}
