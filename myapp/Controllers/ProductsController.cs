using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.BM;
using Services.Interface;

namespace myapp.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IInteractionService _interactionService;

    public ProductsController(IProductService productService, IInteractionService interactionService)
    {
        _productService = productService;
        _interactionService = interactionService;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim) : Guid.Empty;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery(Name = "category_id")] int? categoryId,
        [FromQuery(Name = "brand_id")] int? brandId,
        [FromQuery(Name = "min_price")] decimal? minPrice,
        [FromQuery(Name = "max_price")] decimal? maxPrice,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 8)
    {
        var response = await _productService.GetProductsAsync(search, categoryId, brandId, minPrice, maxPrice, page, limit);
        return Ok(response);
    }


    [HttpGet("{product_id}")]
    public async Task<IActionResult> GetQuickView([FromRoute(Name = "product_id")] long productId)
    {
        var response = await _productService.GetQuickViewAsync(productId);
        if (!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [HttpGet("{product_id}/reviews")]
    public async Task<IActionResult> GetReviews([FromRoute(Name = "product_id")] long productId)
    {
        var response = await _interactionService.GetReviewsAsync(productId);
        if (!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    [Authorize]
    [HttpPost("{product_id}/reviews")]
    public async Task<IActionResult> CreateReview([FromRoute(Name = "product_id")] long productId, [FromBody] CreateReviewRequest request)
    {
        var response = await _interactionService.CreateReviewAsync(GetUserId(), productId, request);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return StatusCode(201, response);
    }
}
