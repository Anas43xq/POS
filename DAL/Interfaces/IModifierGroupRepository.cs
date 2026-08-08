using DAL.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL.Interfaces
{
    public interface IModifierGroupRepository : IRepository<ModifierGroup>
    {
        Task<IEnumerable<ModifierGroup>> GetAllWithOptionsAndTranslationsAsync();

        /// <summary>
        /// Same shape as <see cref="GetAllWithOptionsAndTranslationsAsync"/>
        /// (options + translations included), but filtered to the given
        /// group IDs at the database level instead of loading the full
        /// catalog and filtering in memory. Used on the cashier hot path
        /// (add-to-cart / modifier panel) where only a product's own
        /// modifier groups are needed.
        /// </summary>
        Task<IEnumerable<ModifierGroup>> GetByIdsWithOptionsAndTranslationsAsync(IEnumerable<int> groupIds);

        /// <summary>
        /// Loads a single group with its category/product assignment
        /// collections populated (CategoryModifierGroups,
        /// ProductModifierGroups) — the data DeleteGroupAsync's "is this
        /// group still assigned to anything?" guard actually needs.
        /// Does NOT include options/translations; callers that need those
        /// too should use GetByIdsWithOptionsAndTranslationsAsync instead.
        /// </summary>
        Task<ModifierGroup?> GetByIdWithAssignmentsAsync(int groupId);
    }
}