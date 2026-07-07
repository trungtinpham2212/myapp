using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync()
    {
        // Total Users
        int totalCustomers = await _unitOfWork.DbContext.Users.CountAsync();

        // Orders stats
        var successfulOrdersQuery = _unitOfWork.DbContext.Orders.Where(o => o.PaymentStatus == "Success");
        
        int totalSuccessfulOrders = await successfulOrdersQuery.CountAsync();
        decimal totalRevenue = await successfulOrdersQuery.SumAsync(o => o.FinalAmount);

        var stats = new DashboardStatsDto
        {
            TotalNewCustomers = totalCustomers,
            TotalSuccessfulOrders = totalSuccessfulOrders,
            TotalRevenue = totalRevenue
        };

        return new ApiResponse<DashboardStatsDto>
        {
            Success = true,
            Data = stats
        };
    }

    public async Task<ApiResponse<List<TopSellingProductDto>>> GetTopSellingProductsAsync(int top = 5)
    {
        var variantStats = await _unitOfWork.ProductRepository.GetTopSellingVariantStatsAsync();

        var topProductStats = variantStats
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalSold = g.Sum(x => x.TotalSold),
                TotalRevenue = g.Sum(x => x.TotalRevenue)
            })
            .OrderByDescending(x => x.TotalSold)
            .Take(top)
            .ToList();

        if (!topProductStats.Any())
        {
            return new ApiResponse<List<TopSellingProductDto>>
            {
                Success = true,
                Data = new List<TopSellingProductDto>()
            };
        }

        var productIds = topProductStats.Select(x => x.ProductId).ToList();

        var products = await _unitOfWork.ProductRepository.GetProductsByIdsAsync(productIds);

        var result = new List<TopSellingProductDto>();
        foreach (var stat in topProductStats)
        {
            var product = products.FirstOrDefault(p => p.ProductId == stat.ProductId);
            if (product == null) continue;

            result.Add(new TopSellingProductDto
            {
                ProductId = product.ProductId,
                ProductName = product.Name,
                CategoryName = product.Category?.Name ?? "Không xác định",
                TotalSold = stat.TotalSold,
                TotalRevenue = stat.TotalRevenue,
                Variants = product.ProductVariants
                    .Where(v => variantStats.Any(vs => vs.VariantId == v.ProductVariantId && vs.TotalSold > 0))
                    .Select(v => new TopSellingVariantDto
                    {
                        ProductVariantId = v.ProductVariantId,
                        Color = v.Color,
                        Storage = v.Storage,
                        StockQuantity = v.StockQuantity,
                        Price = v.SalePrice,
                        TotalSold = variantStats.First(vs => vs.VariantId == v.ProductVariantId).TotalSold
                    })
                    .OrderByDescending(v => v.TotalSold)
                    .ToList()
            });
        }

        return new ApiResponse<List<TopSellingProductDto>>
        {
            Success = true,
            Data = result
        };
    }

    public async Task<ApiResponse<List<RevenueByDayDto>>> GetRevenueLast7DaysAsync()
    {
        var endDate = DateTime.UtcNow.Date;
        var startDate = endDate.AddDays(-6); // 7 days including today

        var orders = await _unitOfWork.DbContext.Orders
            .Where(o => o.PaymentStatus == "Success" && o.CreatedAt != null && o.CreatedAt >= startDate && o.CreatedAt < endDate.AddDays(1))
            .Select(o => new { o.CreatedAt, o.FinalAmount })
            .ToListAsync();

        var result = new List<RevenueByDayDto>();

        for (int i = 0; i < 7; i++)
        {
            var currentDate = startDate.AddDays(i);
            
            var dailyRevenue = orders
                .Where(o => o.CreatedAt?.Date == currentDate)
                .Sum(o => o.FinalAmount);

            result.Add(new RevenueByDayDto
            {
                Date = currentDate.ToString("yyyy-MM-dd"),
                Revenue = dailyRevenue
            });
        }

        return new ApiResponse<List<RevenueByDayDto>>
        {
            Success = true,
            Data = result
        };
    }
}
