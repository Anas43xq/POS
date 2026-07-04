using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllWithChildrenAsync();

        Task<IEnumerable<Category>> GetChildrenAsync(int parentCategoryId);
    }
}
