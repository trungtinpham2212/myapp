using System;
using API.Extensions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.BM;
using Services.Interface;

namespace myapp.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var response = await _orderService.CreateOrderAsync(User.GetUserId(), request);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return StatusCode(201, response);
    }

    [HttpGet("{order_id}")]
    public async Task<IActionResult> GetOrderDetails([FromRoute(Name = "order_id")] long orderId)
    {
        var response = await _orderService.GetOrderDetailsAsync(orderId);
        if (!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }
}
