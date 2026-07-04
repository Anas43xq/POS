using BLL.Interfaces;
using BLL.Models;
using DAL.Entities;
using DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryrepo;

        public CategoryService(ICategoryRepository CategoryRepo)
        {
            _categoryrepo = CategoryRepo;
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
