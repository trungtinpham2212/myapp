using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<List<CustomerDto>>> GetAllCustomersAsync(int page = 1, int limit = 10)
    {
        var (items, total) = await _unitOfWork.UserRepository.GetCustomersAsync(page, limit);

        var dtos = items.Select(u => new CustomerDto
        {
            UserId = u.UserId,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        }).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)limit);

        return new ApiResponse<List<CustomerDto>>
        {
            Success = true,
            Data = dtos,
            Pagination = new PaginationMeta
            {
                CurrentPage = page,
                Limit = limit,
                TotalItems = total,
                TotalPages = totalPages
            }
        };
    }

    public async Task<ApiResponse<CustomerDetailDto>> GetCustomerDetailsAsync(Guid userId)
    {
        var user = await _unitOfWork.UserRepository.GetCustomerDetailsAsync(userId);
        if (user == null)
        {
            return new ApiResponse<CustomerDetailDto> { Success = false, Message = "Không tìm thấy khách hàng" };
        }

        var totalSpent = user.Orders
            .Where(o => o.PaymentStatus == "Success")
            .Sum(o => o.FinalAmount);

        var reviews = user.Reviews.Select(r => new CustomerReviewDto
        {
            ReviewId = r.ReviewId,
            ProductId = r.ProductId,
            ProductName = r.Product?.Name,
            RatingStars = r.RatingStars,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        }).ToList();

        var detailDto = new CustomerDetailDto
        {
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            TotalSpent = totalSpent,
            Reviews = reviews
        };

        return new ApiResponse<CustomerDetailDto> { Success = true, Data = detailDto };
    }

    public async Task<ApiResponse<string>> ToggleCustomerStatusAsync(Guid userId, bool isActive)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
        if (user == null || user.Role != 1)
        {
            return new ApiResponse<string> { Success = false, Message = "Không tìm thấy khách hàng" };
        }

        if (user.IsActive == isActive)
        {
            return new ApiResponse<string> { Success = true, Message = "Trạng thái không thay đổi" };
        }

        if (!isActive)
        {
            var incompleteOrders = await _unitOfWork.OrderRepository.CountIncompleteOrdersByUserIdAsync(userId);
            if (incompleteOrders > 0)
            {
                return new ApiResponse<string> { Success = false, Message = "Không thể vô hiệu hóa khách hàng đang có đơn hàng chưa hoàn thành" };
            }
        }

        user.IsActive = isActive;
        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<string> { Success = true, Message = isActive ? "Đã kích hoạt khách hàng" : "Đã vô hiệu hóa khách hàng" };
    }
}
