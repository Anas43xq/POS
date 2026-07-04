using BLL.Models;
using DAL.Entities;

namespace BLL.Interfaces
{
    public interface ICategoryService
    {
        Task<Result<List<Category>>> GetAllCategoriesAsync();

        Task<Result<List<Category>>> GetAllCategoriesWithChildrenAsync();

        Task<Result<List<Category>>> GetChildCategoriesAsync(int parentCategoryId);

        Task<Category?> GetCategoryByIdAsync(int id);

        Task AddCategoryAsync(Category Category);

        Task UpdateCategoryAsync(Category Category);

        Task DeleteCategoryAsync(int id);
    }
}