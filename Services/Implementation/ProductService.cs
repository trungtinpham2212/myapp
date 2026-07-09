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
    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request)
    {
        if (request.Variants == null || !request.Variants.Any())
        {
            return new ApiResponse<ProductDto> { Success = false, Message = "Product must have at least 1 variant" };
        }

        var product = new Product
        {
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            Name = request.Name,
            ThumbnailUrl = request.ThumbnailUrl,
            IsFeatured = request.IsFeatured,
            Status = "active",
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
        };

        foreach (var vDto in request.Variants)
        {
            decimal salePrice = vDto.OriginalPrice * (1 - (vDto.DiscountPercentage ?? 0) / 100m);
            product.ProductVariants.Add(new ProductVariant
            {
                Color = vDto.Color,
                Storage = vDto.Storage,
                OriginalPrice = vDto.OriginalPrice,
                DiscountPercentage = vDto.DiscountPercentage,
                SalePrice = salePrice,
                StockQuantity = vDto.StockQuantity,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            });
        }

        product.MinPrice = product.ProductVariants.Min(v => v.SalePrice);
        product.MaxPrice = product.ProductVariants.Max(v => v.SalePrice);
        product.DiscountPercentage = product.ProductVariants.Max(v => v.DiscountPercentage);

        await _unitOfWork.ProductRepository.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<ProductDto>
        {
            Success = true,
            Message = "Thêm sản phẩm thành công",
            Data = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                ThumbnailUrl = product.ThumbnailUrl,
                DiscountPercentage = product.DiscountPercentage,
                MinPrice = product.MinPrice,
                MaxPrice = product.MaxPrice,
                IsFeatured = product.IsFeatured,
                Status = product.Status
            }
        };
    }
    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(long productId, UpdateProductRequest request)
    {
        var product = await _unitOfWork.ProductRepository.GetProductWithDetailsAsync(productId);
        if (product == null)
        {
            return new ApiResponse<ProductDto> { Success = false, Message = "Product not found" };
        }

        if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;
        if (request.BrandId.HasValue) product.BrandId = request.BrandId.Value;
        if (!string.IsNullOrEmpty(request.Name)) product.Name = request.Name;
        if (request.ThumbnailUrl != null) product.ThumbnailUrl = request.ThumbnailUrl;
        if (request.IsFeatured.HasValue) product.IsFeatured = request.IsFeatured.Value;
        
        product.UpdatedAt = System.DateTime.UtcNow;

        if (request.Variants != null && request.Variants.Any())
        {
            foreach (var vDto in request.Variants)
            {
                var existingVariant = product.ProductVariants.FirstOrDefault(v => v.ProductVariantId == vDto.ProductVariantId);
                if (existingVariant != null)
                {
                    if (vDto.Color != null) existingVariant.Color = vDto.Color;
                    if (vDto.Storage != null) existingVariant.Storage = vDto.Storage;
                    if (vDto.OriginalPrice.HasValue) existingVariant.OriginalPrice = vDto.OriginalPrice.Value;
                    if (vDto.DiscountPercentage.HasValue) existingVariant.DiscountPercentage = vDto.DiscountPercentage;
                    if (vDto.StockQuantity.HasValue) existingVariant.StockQuantity = vDto.StockQuantity.Value;
                    
                    existingVariant.SalePrice = existingVariant.OriginalPrice * (1 - (existingVariant.DiscountPercentage ?? 0) / 100m);
                    existingVariant.UpdatedAt = System.DateTime.UtcNow;
                }
            }

            if (product.ProductVariants.Any())
            {
                product.MinPrice = product.ProductVariants.Min(v => v.SalePrice);
                product.MaxPrice = product.ProductVariants.Max(v => v.SalePrice);
                product.DiscountPercentage = product.ProductVariants.Max(v => v.DiscountPercentage);
            }
        }

        _unitOfWork.ProductRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<ProductDto>
        {
            Success = true,
            Message = "Cập nhật sản phẩm thành công",
            Data = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                ThumbnailUrl = product.ThumbnailUrl,
                DiscountPercentage = product.DiscountPercentage,
                MinPrice = product.MinPrice,
                MaxPrice = product.MaxPrice,
                IsFeatured = product.IsFeatured,
                Status = product.Status
            }
        };
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(long productId)
    {
        var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return new ApiResponse<bool> { Success = false, Message = "Product not found", Data = false };
        }

        _unitOfWork.ProductRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<bool> { Success = true, Message = "Xóa sản phẩm thành công", Data = true };
    }

    public async Task<ApiResponse<ProductDto>> AddProductVariantsAsync(long productId, List<CreateProductVariantDto> variants)
    {
        if (variants == null || !variants.Any())
        {
            return new ApiResponse<ProductDto> { Success = false, Message = "Danh sách variants trống" };
        }

        var product = await _unitOfWork.ProductRepository.GetProductWithDetailsAsync(productId);
        if (product == null)
        {
            return new ApiResponse<ProductDto> { Success = false, Message = "Không tìm thấy sản phẩm" };
        }

        foreach (var vDto in variants)
        {
            decimal salePrice = vDto.OriginalPrice * (1 - (vDto.DiscountPercentage ?? 0) / 100m);
            product.ProductVariants.Add(new ProductVariant
            {
                Color = vDto.Color,
                Storage = vDto.Storage,
                OriginalPrice = vDto.OriginalPrice,
                DiscountPercentage = vDto.DiscountPercentage,
                SalePrice = salePrice,
                StockQuantity = vDto.StockQuantity,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            });
        }

        product.MinPrice = product.ProductVariants.Min(v => v.SalePrice);
        product.MaxPrice = product.ProductVariants.Max(v => v.SalePrice);
        product.DiscountPercentage = product.ProductVariants.Max(v => v.DiscountPercentage);
        product.UpdatedAt = System.DateTime.UtcNow;

        _unitOfWork.ProductRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<ProductDto>
        {
            Success = true,
            Message = "Thêm biến thể (variants) thành công",
            Data = new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                ThumbnailUrl = product.ThumbnailUrl,
                DiscountPercentage = product.DiscountPercentage,
                MinPrice = product.MinPrice,
                MaxPrice = product.MaxPrice,
                IsFeatured = product.IsFeatured,
                Status = product.Status
            }
        };
    }

    public async Task<ApiResponse<bool>> DeleteProductVariantAsync(long productId, long variantId)
    {
        var product = await _unitOfWork.ProductRepository.GetProductWithDetailsAsync(productId);
        if (product == null)
        {
            return new ApiResponse<bool> { Success = false, Message = "Không tìm thấy sản phẩm", Data = false };
        }

        var variant = product.ProductVariants.FirstOrDefault(v => v.ProductVariantId == variantId);
        if (variant == null)
        {
            return new ApiResponse<bool> { Success = false, Message = "Không tìm thấy biến thể", Data = false };
        }

        if (product.ProductVariants.Count == 1)
        {
            return new ApiResponse<bool> { Success = false, Message = "Không thể xóa biến thể duy nhất của sản phẩm", Data = false };
        }

        // Ràng buộc: không được xóa khi nằm trong cart hay đã order
        var cartItemCount = await _unitOfWork.CartItemRepository.CountAsync(ci => ci.ProductVariantId == variantId);
        if (cartItemCount > 0)
        {
            return new ApiResponse<bool> { Success = false, Message = "Không thể xóa biến thể đang nằm trong giỏ hàng", Data = false };
        }

        var orderDetailCount = await _unitOfWork.OrderDetailRepository.CountAsync(od => od.ProductVariantId == variantId);
        if (orderDetailCount > 0)
        {
            return new ApiResponse<bool> { Success = false, Message = "Không thể xóa biến thể đã có đơn hàng", Data = false };
        }

        // Remove variant
        product.ProductVariants.Remove(variant);

        // Calculate fields again
        product.MinPrice = product.ProductVariants.Min(v => v.SalePrice);
        product.MaxPrice = product.ProductVariants.Max(v => v.SalePrice);
        product.DiscountPercentage = product.ProductVariants.Max(v => v.DiscountPercentage);
        product.UpdatedAt = System.DateTime.UtcNow;

        _unitOfWork.ProductRepository.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<bool> { Success = true, Message = "Xóa biến thể thành công", Data = true };
    }
}
