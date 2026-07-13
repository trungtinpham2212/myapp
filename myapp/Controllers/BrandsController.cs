using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.BM;
using Services.Interface;

namespace API.Controllers;

[Route("api/brands")]
[ApiController]
public class BrandsController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public BrandsController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var response = await _catalogService.GetAllBrandsAsync(page, limit);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _catalogService.GetBrandByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUpdateBrandDto request)
    {
        var response = await _catalogService.CreateBrandAsync(request);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.BrandId }, response);
    }

    [Authorize(Roles = "2")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateBrandDto request)
    {
        var response = await _catalogService.UpdateBrandAsync(id, request);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _catalogService.DeleteBrandAsync(id);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}
