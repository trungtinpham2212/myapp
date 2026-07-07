using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<ProductDto>>> GetProductsAsync(
        string? search,
        int? categoryId,
        int? brandId,
        decimal? minPrice,
        decimal? maxPrice,
        int page = 1,
        int limit = 8)
    {
        if (page < 1) page = 1;
        if (limit < 1) limit = 8;

        var totalItems = await _unitOfWork.ProductRepository.CountFilteredProductsAsync(search, categoryId, brandId, minPrice, maxPrice);
        var totalPages = (int)Math.Ceiling((double)totalItems / limit);

        var products = await _unitOfWork.ProductRepository.GetFilteredProductsAsync(search, categoryId, brandId, minPrice, maxPrice, page, limit);

        var productDtos = products.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            Name = p.Name,
            ThumbnailUrl = p.ThumbnailUrl,
            DiscountPercentage = p.DiscountPercentage,
            MinPrice = p.MinPrice,
            MaxPrice = p.MaxPrice,
            AverageRating = p.AverageRating,
            TotalReviews = p.TotalReviews,
            IsFeatured = p.IsFeatured,
            Status = p.Status
        }).ToList();

        return new ApiResponse<List<ProductDto>>
        {
            Success = true,
            Pagination = new PaginationMeta
            {
                CurrentPage = page,
                Limit = limit,
                TotalItems = totalItems,
                TotalPages = totalPages
            },
            Data = productDtos
        };
    }

    public async Task<ApiResponse<ProductQuickViewDto>> GetQuickViewAsync(long productId)
    {
        var product = await _unitOfWork.ProductRepository.GetProductWithDetailsAsync(productId);

        if (product == null)
        {
            return new ApiResponse<ProductQuickViewDto>
            {
                Success = false,
                Message = "Không tìm thấy sản phẩm"
            };
        }

        var variants = product.ProductVariants.Select(v => new VariantDto
        {
            ProductVariantId = v.ProductVariantId,
            Color = v.Color,
            Storage = v.Storage,
            OriginalPrice = v.OriginalPrice,
            DiscountPercentage = v.DiscountPercentage,
            SalePrice = v.SalePrice,
            StockQuantity = v.StockQuantity
        }).ToList();

        var images = product.ProductImages
            .OrderBy(i => i.SortOrder ?? 0)
            .Select(i => new ImageDto
            {
                ProductImageId = i.ProductImageId,
                ImageUrl = i.ImageUrl,
                SortOrder = i.SortOrder
            }).ToList();

        var quickViewDto = new ProductQuickViewDto
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Variants = variants,
            Images = images
        };

        return new ApiResponse<ProductQuickViewDto>
        {
            Success = true,
            Data = quickViewDto
        };
    }

}
