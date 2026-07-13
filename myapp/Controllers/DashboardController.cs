using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Services.Interface;

namespace myapp.Controllers;

[ApiController]
[Authorize(Roles = "2")]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats([FromQuery] System.DateTime? fromDate = null, [FromQuery] System.DateTime? toDate = null)
    {
        var response = await _dashboardService.GetDashboardStatsAsync(fromDate, toDate);
        return Ok(response);
    }

    [HttpGet("top-selling")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int top = 5, [FromQuery] System.DateTime? fromDate = null, [FromQuery] System.DateTime? toDate = null)
    {
        var response = await _dashboardService.GetTopSellingProductsAsync(top, fromDate, toDate);
        return Ok(response);
    }

    [HttpGet("top-selling/export")]
    public async Task<IActionResult> ExportTopSellingProducts([FromQuery] int top = 5, [FromQuery] System.DateTime? fromDate = null, [FromQuery] System.DateTime? toDate = null)
    {
        var excelBytes = await _dashboardService.ExportTopSellingProductsToExcelAsync(top, fromDate, toDate);
        var fileName = $"TopSellingProducts_{System.DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("revenue-last-7-days")]
    public async Task<IActionResult> GetRevenueLast7Days([FromQuery] System.DateTime? fromDate = null, [FromQuery] System.DateTime? toDate = null)
    {
        var response = await _dashboardService.GetRevenueByDayAsync(fromDate, toDate);
        return Ok(response);
    }

    [HttpGet("revenue-by-category")]
    public async Task<IActionResult> GetRevenueByCategory([FromQuery] System.DateTime? fromDate = null, [FromQuery] System.DateTime? toDate = null)
    {
        var response = await _dashboardService.GetRevenueByCategoryAsync(fromDate, toDate);
        return Ok(response);
    }
}
