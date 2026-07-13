using Services.BM;

namespace Services.Interface;

public interface ICatalogService
{
    // Category
    Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync(int page = 1, int limit = 10);
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateUpdateCategoryDto request);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, CreateUpdateCategoryDto request);
    Task<ApiResponse<bool>> DeleteCategoryAsync(int id);

    // Brand
    Task<ApiResponse<List<BrandDto>>> GetAllBrandsAsync(int page = 1, int limit = 10);
    Task<ApiResponse<BrandDto>> GetBrandByIdAsync(int id);
    Task<ApiResponse<BrandDto>> CreateBrandAsync(CreateUpdateBrandDto request);
    Task<ApiResponse<BrandDto>> UpdateBrandAsync(int id, CreateUpdateBrandDto request);
    Task<ApiResponse<bool>> DeleteBrandAsync(int id);
}
