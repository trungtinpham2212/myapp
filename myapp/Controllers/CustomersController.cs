using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace API.Controllers;

[Route("api/customers")]
[ApiController]
public class CustomersController : ControllerBase
{
    private readonly IUserService _userService;

    public CustomersController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(Roles = "2")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var response = await _userService.GetAllCustomersAsync(page, limit);
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetDetails(Guid userId)
    {
        var response = await _userService.GetCustomerDetailsAsync(userId);
        if (!response.Success)
            return NotFound(response);
            
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpPut("{userId:guid}/activate")]
    public async Task<IActionResult> Activate(Guid userId)
    {
        var response = await _userService.ToggleCustomerStatusAsync(userId, true);
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpPut("{userId:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid userId)
    {
        var response = await _userService.ToggleCustomerStatusAsync(userId, false);
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }
}
