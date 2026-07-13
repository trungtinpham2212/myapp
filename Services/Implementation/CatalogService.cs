using Repositories.Models;
using Repositories.UnitOfWork;
using Services.BM;
using Services.Interface;

namespace Services.Implementation;

public class CatalogService : ICatalogService
{
    private readonly IUnitOfWork _unitOfWork;

    public CatalogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ================= CATEGORY =================

    public async Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync(int page = 1, int limit = 10)
    {
        var (items, total) = await _unitOfWork.CategoryRepository.GetPagedWithDetailsAsync(null, page, limit);
        var dtos = items.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            Name = c.Name
        }).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)limit);

        return new ApiResponse<List<CategoryDto>> 
        { 
            Success = true, 
            Data = dtos,
            Pagination = new PaginationMeta
            {
                CurrentPage = page,
                Limit = limit,
                TotalItems = total,
                TotalPages = totalPages
            }
        };
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
        if (category == null) return new ApiResponse<CategoryDto> { Success = false, Message = "Không tìm thấy danh mục" };

        return new ApiResponse<CategoryDto>
        {
            Success = true,
            Data = new CategoryDto { CategoryId = category.CategoryId, Name = category.Name }
        };
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateUpdateCategoryDto request)
    {
        var category = new Category
        {
            Name = request.Name,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.CategoryRepository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<CategoryDto>
        {
            Success = true,
            Message = "Thêm danh mục thành công",
            Data = new CategoryDto { CategoryId = category.CategoryId, Name = category.Name }
        };
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, CreateUpdateCategoryDto request)
    {
        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
        if (category == null) return new ApiResponse<CategoryDto> { Success = false, Message = "Không tìm thấy danh mục" };

        category.Name = request.Name;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.CategoryRepository.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<CategoryDto>
        {
            Success = true,
            Message = "Cập nhật danh mục thành công",
            Data = new CategoryDto { CategoryId = category.CategoryId, Name = category.Name }
        };
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.CategoryRepository.GetByIdAsync(id);
        if (category == null) return new ApiResponse<bool> { Success = false, Message = "Không tìm thấy danh mục" };

        var productCount = await _unitOfWork.ProductRepository.CountByCategoryIdAsync(id);
        if (productCount > 0)
        {
            return new ApiResponse<bool> { Success = false, Message = "Không thể xóa danh mục đang có sản phẩm", Data = false };
        }

        _unitOfWork.CategoryRepository.Delete(category);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<bool> { Success = true, Message = "Xóa danh mục thành công", Data = true };
    }

    // ================= BRAND =================

    public async Task<ApiResponse<List<BrandDto>>> GetAllBrandsAsync(int page = 1, int limit = 10)
    {
        var (items, total) = await _unitOfWork.BrandRepository.GetPagedWithDetailsAsync(null, page, limit);
        var dtos = items.Select(b => new BrandDto
        {
            BrandId = b.BrandId,
            Name = b.Name,
            LogoUrl = b.LogoUrl
        }).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)limit);

        return new ApiResponse<List<BrandDto>> 
        { 
            Success = true, 
            Data = dtos,
            Pagination = new PaginationMeta
            {
                CurrentPage = page,
                Limit = limit,
                TotalItems = total,
                TotalPages = totalPages
            }
        };
    }

    public async Task<ApiResponse<BrandDto>> GetBrandByIdAsync(int id)
    {
        var brand = await _unitOfWork.BrandRepository.GetByIdAsync(id);
        if (brand == null) return new ApiResponse<BrandDto> { Success = false, Message = "Không tìm thấy thương hiệu" };

        return new ApiResponse<BrandDto>
        {
            Success = true,
            Data = new BrandDto { BrandId = brand.BrandId, Name = brand.Name, LogoUrl = brand.LogoUrl }
        };
    }

    public async Task<ApiResponse<BrandDto>> CreateBrandAsync(CreateUpdateBrandDto request)
    {
        var brand = new Brand
        {
            Name = request.Name,
            LogoUrl = request.LogoUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.BrandRepository.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<BrandDto>
        {
            Success = true,
            Message = "Thêm thương hiệu thành công",
            Data = new BrandDto { BrandId = brand.BrandId, Name = brand.Name, LogoUrl = brand.LogoUrl }
        };
    }

    public async Task<ApiResponse<BrandDto>> UpdateBrandAsync(int id, CreateUpdateBrandDto request)
    {
        var brand = await _unitOfWork.BrandRepository.GetByIdAsync(id);
        if (brand == null) return new ApiResponse<BrandDto> { Success = false, Message = "Không tìm thấy thương hiệu" };

        brand.Name = request.Name;
        brand.LogoUrl = request.LogoUrl;
        brand.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.BrandRepository.Update(brand);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<BrandDto>
        {
            Success = true,
            Message = "Cập nhật thương hiệu thành công",
            Data = new BrandDto { BrandId = brand.BrandId, Name = brand.Name, LogoUrl = brand.LogoUrl }
        };
    }

    public async Task<ApiResponse<bool>> DeleteBrandAsync(int id)
    {
        var brand = await _unitOfWork.BrandRepository.GetByIdAsync(id);
        if (brand == null) return new ApiResponse<bool> { Success = false, Message = "Không tìm thấy thương hiệu" };

        var productCount = await _unitOfWork.ProductRepository.CountByBrandIdAsync(id);
        if (productCount > 0)
        {
            return new ApiResponse<bool> { Success = false, Message = "Không thể xóa thương hiệu đang có sản phẩm", Data = false };
        }

        _unitOfWork.BrandRepository.Delete(brand);
        await _unitOfWork.SaveChangesAsync();

        return new ApiResponse<bool> { Success = true, Message = "Xóa thương hiệu thành công", Data = true };
    }
}
