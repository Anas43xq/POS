using BLL.DTOs;
using BLL.Models;

namespace BLL.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<List<CategoryDto>>> GetAllCategoriesAsync();

        Task<Result<List<CategoryDto>>> GetAllCategoriesWithChildrenAsync();

        Task<Result<List<CategoryDto>>> GetChildCategoriesAsync(int parentCategoryId);

        Task<CategoryDto?> GetCategoryByIdAsync(int id);

        Task AddCategoryAsync(CategoryDto category);

        Task UpdateCategoryAsync(CategoryDto category);

        Task DeleteCategoryAsync(int id);
    }
}