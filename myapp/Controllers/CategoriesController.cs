using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.BM;
using Services.Interface;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CategoriesController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var response = await _catalogService.GetAllCategoriesAsync(page, limit);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _catalogService.GetCategoryByIdAsync(id);
        if (!response.Success) return NotFound(response);
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUpdateCategoryDto request)
    {
        var response = await _catalogService.CreateCategoryAsync(request);
        if (!response.Success) return BadRequest(response);
        return CreatedAtAction(nameof(GetById), new { id = response.Data?.CategoryId }, response);
    }

    [Authorize(Roles = "2")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateCategoryDto request)
    {
        var response = await _catalogService.UpdateCategoryAsync(id, request);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [Authorize(Roles = "2")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var response = await _catalogService.DeleteCategoryAsync(id);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }
}
