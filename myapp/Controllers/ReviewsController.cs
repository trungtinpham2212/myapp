using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interface;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReviewsController : ControllerBase
{
    private readonly IInteractionService _interactionService;

    public ReviewsController(IInteractionService interactionService)
    {
        _interactionService = interactionService;
    }

    [Authorize(Roles = "2")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] long? productId, [FromQuery] Guid? userId, [FromQuery] int? star, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var response = await _interactionService.GetAllReviewsFilteredAsync(productId, userId, star, page, limit);
        return Ok(response);
    }
}
