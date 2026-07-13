using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;
using ClosedXML.Excel;
using System.IO;

namespace Services.Implementation;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        // Total Users
        int totalCustomers = await _unitOfWork.UserRepository.CountUsersAsync(fromDate, toDate);

        // Orders stats
        var orderStats = await _unitOfWork.OrderRepository.GetOrderStatsAsync(fromDate, toDate);
        
        var stats = new DashboardStatsDto
        {
            TotalNewCustomers = totalCustomers,
            TotalSuccessfulOrders = orderStats.TotalSuccessfulOrders,
            TotalRevenue = orderStats.TotalRevenue
        };

        return new ApiResponse<DashboardStatsDto>
        {
            Success = true,
            Data = stats
        };
    }

    public async Task<ApiResponse<List<TopSellingProductDto>>> GetTopSellingProductsAsync(int top = 5, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var variantStats = await _unitOfWork.ProductRepository.GetTopSellingVariantStatsAsync(fromDate, toDate);

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

    public async Task<ApiResponse<List<RevenueByDayDto>>> GetRevenueByDayAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var endDate = toDate?.Date ?? DateTime.UtcNow.Date;
        var startDate = fromDate?.Date ?? endDate.AddDays(-6); // default to 7 days including today

        var orders = await _unitOfWork.OrderRepository.GetSuccessfulOrdersRevenueAsync(startDate, endDate.AddDays(1));

        var result = new List<RevenueByDayDto>();
        int days = (endDate - startDate).Days + 1;
        if (days > 100) days = 100; // safety limit

        for (int i = 0; i < days; i++)
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

    public async Task<ApiResponse<List<RevenueByCategoryDto>>> GetRevenueByCategoryAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var categoryStatsRaw = await _unitOfWork.OrderRepository.GetRevenueByCategoryAsync(fromDate, toDate);

        var categoryStats = categoryStatsRaw.Select(c => new RevenueByCategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            Revenue = c.Revenue
        }).ToList();

        decimal totalRevenue = categoryStats.Sum(c => c.Revenue);
        if (totalRevenue > 0)
        {
            foreach (var stat in categoryStats)
            {
                stat.Percentage = Math.Round((stat.Revenue / totalRevenue) * 100, 2);
            }
        }

        return new ApiResponse<List<RevenueByCategoryDto>>
        {
            Success = true,
            Data = categoryStats
        };
    }

    public async Task<byte[]> ExportTopSellingProductsToExcelAsync(int top = 5, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var response = await GetTopSellingProductsAsync(top, fromDate, toDate);
        var data = response.Data;

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Top Selling Products");

        // Headers
        worksheet.Cell(1, 1).Value = "Mã SP";
        worksheet.Cell(1, 2).Value = "Tên Sản Phẩm";
        worksheet.Cell(1, 3).Value = "Danh Mục";
        worksheet.Cell(1, 4).Value = "Tổng Đã Bán";
        worksheet.Cell(1, 5).Value = "Tổng Doanh Thu";

        // Format headers
        var headerRow = worksheet.Range("A1:E1");
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        int row = 2;
        if (data != null)
        {
            foreach (var item in data)
            {
                worksheet.Cell(row, 1).Value = item.ProductId;
                worksheet.Cell(row, 2).Value = item.ProductName;
                worksheet.Cell(row, 3).Value = item.CategoryName;
                worksheet.Cell(row, 4).Value = item.TotalSold;
                worksheet.Cell(row, 5).Value = item.TotalRevenue;
                
                // Format revenue column
                worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
                
                row++;
            }
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
