using System.Collections.Generic;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface IDashboardService
{
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync();
    Task<ApiResponse<List<TopSellingProductDto>>> GetTopSellingProductsAsync(int top = 5);
    Task<ApiResponse<List<RevenueByDayDto>>> GetRevenueLast7DaysAsync();
}
