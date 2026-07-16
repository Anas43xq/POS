using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface ICategoryModifierGroupRepository : IRepository<CategoryModifierGroup>
    {
        Task<IEnumerable<CategoryModifierGroup>> GetByCategoryIdAsync(int categoryId);
    }
}