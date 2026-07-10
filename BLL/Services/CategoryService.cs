using BLL.DTOs;
using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryrepo;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository CategoryRepo, ILogger<CategoryService> logger)
        {
            _categoryrepo = CategoryRepo;
            _logger = logger;
        }

        public async Task<Result<List<CategoryDto>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryrepo.GetAllAsync();
                return Result<List<CategoryDto>>.Success(categories.Select(MapToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories");
                return Result<List<CategoryDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<CategoryDto>>> GetAllCategoriesWithChildrenAsync()
        {
            try
            {
                var categories = await _categoryrepo.GetAllWithChildrenAsync();
                return Result<List<CategoryDto>>.Success(categories.Select(MapToDtoRecursive).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories with children");
                return Result<List<CategoryDto>>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<CategoryDto>>> GetChildCategoriesAsync(int parentCategoryId)
        {
            try
            {
                var categories = await _categoryrepo.GetChildrenAsync(parentCategoryId);
                return Result<List<CategoryDto>>.Success(categories.Select(MapToDto).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load child categories for parent {ParentCategoryId}", parentCategoryId);
                return Result<List<CategoryDto>>.Failure(ex.Message);
            }
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var entity = await _categoryrepo.GetByIdAsync(id);
            return entity is null ? null : MapToDto(entity);
        }

        public async Task AddCategoryAsync(CategoryDto category) =>
            await _categoryrepo.AddAsync(MapToEntity(category));

        public async Task UpdateCategoryAsync(CategoryDto category) =>
            await _categoryrepo.UpdateAsync(MapToEntity(category));

        public async Task DeleteCategoryAsync(int id) =>
            await _categoryrepo.DeleteAsync(id);

        private static CategoryDto MapToDto(Category e) => new()
        {
            CategoryId = e.CategoryId,
            Name = e.Name,
            ParentCategoryId = e.ParentCategoryId,
            Description = e.Description,
            ProductCount = e.Products?.Count ?? 0
        };

        private static CategoryDto MapToDtoRecursive(Category e) => new()
        {
            CategoryId = e.CategoryId,
            Name = e.Name,
            ParentCategoryId = e.ParentCategoryId,
            Description = e.Description,
            ProductCount = (e.Products?.Count ?? 0) + (e.ChildCategories?.Sum(c => c.Products?.Count ?? 0) ?? 0),
            ChildCategories = e.ChildCategories?.Select(MapToDto).ToList() ?? new List<CategoryDto>()
        };

        private static Category MapToEntity(CategoryDto d) => new()
        {
            CategoryId = d.CategoryId,
            Name = d.Name,
            ParentCategoryId = d.ParentCategoryId,
            Description = d.Description
        };
    }
}