using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Services.Interface;

public interface ICloudinaryService
{
    Task<string> UploadImageAsync(IFormFile file);
}
