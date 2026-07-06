using System;
using System.Linq;
using System.Threading.Tasks;
using Repositories.Models;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class InteractionService : IInteractionService
{
    private readonly IUnitOfWork _unitOfWork;

    public InteractionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> CreateReviewAsync(Guid userId, long productId, CreateReviewRequest request)
    {
        if (request.RatingStars < 1 || request.RatingStars > 5)
        {
            return new ApiResponse<object> { Success = false, Message = "Số sao đánh giá phải từ 1 đến 5" };
        }

        var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return new ApiResponse<object> { Success = false, Message = "Sản phẩm không tồn tại" };
        }

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            RatingStars = request.RatingStars,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ReviewRepository.AddAsync(review);

        var reviews = await _unitOfWork.ReviewRepository.GetReviewsByProductIdAsync(productId);
        int total = reviews.Count + 1;
        decimal sum = reviews.Sum(r => r.RatingStars ?? 0) + request.RatingStars;
        product.TotalReviews = total;
        product.AverageRating = Math.Round(sum / total, 1);
        _unitOfWork.ProductRepository.Update(product);

        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<object>
        {
            Success = true,
            Message = "Gửi đánh giá thành công"
        };
    }

    public async Task<ApiResponse<List<ReviewDto>>> GetReviewsAsync(long productId)
    {
        var product = await _unitOfWork.ProductRepository.GetByIdAsync(productId);
        if (product == null)
        {
            return new ApiResponse<List<ReviewDto>> { Success = false, Message = "Sản phẩm không tồn tại" };
        }

        var reviews = await _unitOfWork.ReviewRepository.GetReviewsByProductIdAsync(productId);
        var reviewDtos = reviews.Select(r => new ReviewDto
        {
            ReviewId = r.ReviewId,
            RatingStars = r.RatingStars,
            Comment = r.Comment,
            UserName = r.User?.FullName,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new ApiResponse<List<ReviewDto>>
        {
            Success = true,
            Data = reviewDtos
        };
    }

    public async Task<ToggleWishlistResponse> ToggleWishlistAsync(Guid userId, ToggleWishlistRequest request)
    {
        var wishlist = await _unitOfWork.WishlistRepository.GetUserWishlistForProductAsync(userId, request.ProductId);

        bool isFavorite;
        if (wishlist != null)
        {
            _unitOfWork.WishlistRepository.Delete(wishlist);
            isFavorite = false;
        }
        else
        {
            wishlist = new Wishlist
            {
                UserId = userId,
                ProductId = request.ProductId,
                CreatedAt = DateTime.UtcNow
            };
            await _unitOfWork.WishlistRepository.AddAsync(wishlist);
            isFavorite = true;
        }

        await _unitOfWork.SaveChangesAsync();

        return new ToggleWishlistResponse
        {
            Success = true,
            IsFavorite = isFavorite
        };
    }

    public async Task<ApiResponse<List<ProductDto>>> GetWishlistAsync(Guid userId)
    {
        var wishlists = await _unitOfWork.WishlistRepository.GetUserWishlistAsync(userId);
        
        var productDtos = wishlists.Where(w => w.Product != null).Select(w => new ProductDto
        {
            ProductId = w.Product.ProductId,
            Name = w.Product.Name,
            ThumbnailUrl = w.Product.ThumbnailUrl,
            DiscountPercentage = w.Product.DiscountPercentage,
            MinPrice = w.Product.MinPrice,
            MaxPrice = w.Product.MaxPrice,
            AverageRating = w.Product.AverageRating,
            TotalReviews = w.Product.TotalReviews,
            IsFeatured = w.Product.IsFeatured,
            Status = w.Product.Status
        }).ToList();

        return new ApiResponse<List<ProductDto>>
        {
            Success = true,
            Data = productDtos
        };
    }
}
