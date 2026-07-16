using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class ModifierGroup
    {
        public int ModifierGroupId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 1 = SingleSelect, 2 = MultiSelect, 3 = Quantity
        /// </summary>
        public byte GroupType { get; set; }

        public bool IsRequired { get; set; }

        public int MinSelections { get; set; }

        public int MaxSelections { get; set; }

        public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ICollection<ModifierOption> ModifierOptions { get; set; } = new List<ModifierOption>();

        public ICollection<ModifierGroupTranslation> ModifierGroupTranslations { get; set; } = new List<ModifierGroupTranslation>();

        public ICollection<CategoryModifierGroup> CategoryModifierGroups { get; set; } = new List<CategoryModifierGroup>();

        public ICollection<ProductModifierGroup> ProductModifierGroups { get; set; } = new List<ProductModifierGroup>();
    }
}