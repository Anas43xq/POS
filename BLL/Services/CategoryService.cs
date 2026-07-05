using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Result<List<Category>>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryrepo.GetAllAsync();
                return Result<List<Category>>.Success(categories.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories");
                return Result<List<Category>>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<Category>>> GetAllCategoriesWithChildrenAsync()
        {
            try
            {
                var categories = await _categoryrepo.GetAllWithChildrenAsync();
                return Result<List<Category>>.Success(categories.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load categories with children");
                return Result<List<Category>>.Failure(ex.Message);
            }
        }

        public async Task<Result<List<Category>>> GetChildCategoriesAsync(int parentCategoryId)
        {
            try
            {
                var categories = await _categoryrepo.GetChildrenAsync(parentCategoryId);
                return Result<List<Category>>.Success(categories.ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load child categories for parent {ParentCategoryId}", parentCategoryId);
                return Result<List<Category>>.Failure(ex.Message);
            }
        }

        public async Task<Category?> GetCategoryByIdAsync(int id) =>
            await _categoryrepo.GetByIdAsync(id);

        public async Task AddCategoryAsync(Category Category) =>
            await _categoryrepo.AddAsync(Category);

        public async Task UpdateCategoryAsync(Category Category) =>
            await _categoryrepo.UpdateAsync(Category);

        public async Task DeleteCategoryAsync(int id) =>
            await _categoryrepo.DeleteAsync(id);
    }
}
