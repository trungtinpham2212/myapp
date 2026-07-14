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
[Route("api/wishlists")]
public class WishlistsController : ControllerBase
{
    private readonly IInteractionService _interactionService;

    public WishlistsController(IInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleWishlist([FromBody] ToggleWishlistRequest request)
    {
        var response = await _interactionService.ToggleWishlistAsync(User.GetUserId(), request);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var response = await _interactionService.GetWishlistAsync(User.GetUserId());
        return Ok(response);
    }
}
