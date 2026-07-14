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
[Route("api/carts")]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var response = await _cartService.GetCartAsync(User.GetUserId());
        return Ok(response);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var response = await _cartService.AddToCartAsync(User.GetUserId(), request);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [HttpDelete("items/{cart_item_id}")]
    public async Task<IActionResult> RemoveFromCart([FromRoute(Name = "cart_item_id")] long cartItemId)
    {
        var response = await _cartService.RemoveFromCartAsync(User.GetUserId(), cartItemId);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}
