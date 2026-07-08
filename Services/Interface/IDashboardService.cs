using System.Collections.Generic;
using System.Threading.Tasks;
using Services.BM;

namespace Services.Interface;

public interface IDashboardService
{
    Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<ApiResponse<List<TopSellingProductDto>>> GetTopSellingProductsAsync(int top = 5, DateTime? fromDate = null, DateTime? toDate = null);
    Task<ApiResponse<List<RevenueByDayDto>>> GetRevenueByDayAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<ApiResponse<List<RevenueByCategoryDto>>> GetRevenueByCategoryAsync(DateTime? fromDate = null, DateTime? toDate = null);
}
