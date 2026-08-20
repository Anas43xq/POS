using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IModifierOptionRepository : IRepository<ModifierOption>
    {
        Task<IEnumerable<ModifierOption>> GetByGroupIdAsync(int modifierGroupId);
    }
}