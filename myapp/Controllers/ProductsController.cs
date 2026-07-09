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
    private readonly ICloudinaryService _cloudinaryService;

    public ProductsController(IProductService productService, IInteractionService interactionService, ICloudinaryService cloudinaryService)
    {
        _productService = productService;
        _interactionService = interactionService;
        _cloudinaryService = cloudinaryService;
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

    [Authorize(Roles = "2")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var response = await _productService.CreateProductAsync(request);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return StatusCode(201, response);
    }

    [Authorize(Roles = "2")]
    [HttpPut("{product_id}")]
    public async Task<IActionResult> UpdateProduct([FromRoute(Name = "product_id")] long productId, [FromBody] UpdateProductRequest request)
    {
        var response = await _productService.UpdateProductAsync(productId, request);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpDelete("{product_id}")]
    public async Task<IActionResult> DeleteProduct([FromRoute(Name = "product_id")] long productId)
    {
        var response = await _productService.DeleteProductAsync(productId);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpPost("image")]
    public async Task<IActionResult> UploadImage(Microsoft.AspNetCore.Http.IFormFile file)
    {
        try
        {
            var url = await _cloudinaryService.UploadImageAsync(file);
            return Ok(new ApiResponse<string>
            {
                Success = true,
                Message = "Tải ảnh thành công",
                Data = url
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = ex.Message
            });
        }
    }

    [Authorize(Roles = "2")]
    [HttpPost("{product_id}/variants")]
    public async Task<IActionResult> AddProductVariants([FromRoute(Name = "product_id")] long productId, [FromBody] List<CreateProductVariantDto> variants)
    {
        var response = await _productService.AddProductVariantsAsync(productId, variants);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpDelete("{product_id}/variants/{variant_id}")]
    public async Task<IActionResult> DeleteProductVariant([FromRoute(Name = "product_id")] long productId, [FromRoute(Name = "variant_id")] long variantId)
    {
        var response = await _productService.DeleteProductVariantAsync(productId, variantId);
        if (!response.Success)
        {
            return BadRequest(response);
        }
        return Ok(response);
    }
}
