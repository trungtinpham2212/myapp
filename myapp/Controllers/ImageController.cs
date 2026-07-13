using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.BM;
using Services.Interface;

namespace myapp.Controllers;

[ApiController]
[Route("api/image")]
public class ImageController : ControllerBase
{
    private readonly ICloudinaryService _cloudinaryService;

    public ImageController(ICloudinaryService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [Authorize(Roles = "2")]
    [HttpPost]
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
}
