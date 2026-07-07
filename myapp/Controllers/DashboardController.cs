using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace myapp.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var response = await _dashboardService.GetDashboardStatsAsync();
        return Ok(response);
    }

    [HttpGet("top-selling")]
    public async Task<IActionResult> GetTopSellingProducts([FromQuery] int top = 5)
    {
        var response = await _dashboardService.GetTopSellingProductsAsync(top);
        return Ok(response);
    }

    [HttpGet("revenue-last-7-days")]
    public async Task<IActionResult> GetRevenueLast7Days()
    {
        var response = await _dashboardService.GetRevenueLast7DaysAsync();
        return Ok(response);
    }
}
