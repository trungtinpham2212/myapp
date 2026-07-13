using System.Collections.Generic;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface IUserService
{
    Task<ApiResponse<List<CustomerDto>>> GetAllCustomersAsync(int page = 1, int limit = 10);
    Task<ApiResponse<CustomerDetailDto>> GetCustomerDetailsAsync(System.Guid userId);
    Task<ApiResponse<string>> ToggleCustomerStatusAsync(System.Guid userId, bool isActive);
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(System.Guid userId);
    Task<ApiResponse<UserProfileDto>> UpdateProfileAsync(System.Guid userId, UpdateProfileRequestDto request);
}
