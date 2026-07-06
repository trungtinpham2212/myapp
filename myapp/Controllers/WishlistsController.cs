using System;
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

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }

    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleWishlist([FromBody] ToggleWishlistRequest request)
    {
        var response = await _interactionService.ToggleWishlistAsync(GetUserId(), request);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var response = await _interactionService.GetWishlistAsync(GetUserId());
        return Ok(response);
    }
}
