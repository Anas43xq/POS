using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IProductModifierGroupRepository : IRepository<ProductModifierGroup>
    {
        Task<IEnumerable<ProductModifierGroup>> GetByProductIdAsync(int productId);
    }
}