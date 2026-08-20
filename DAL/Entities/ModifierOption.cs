using System;
using System.Collections.Generic;

namespace DAL.Entities
{
    public class ModifierOption
    {
        public int ModifierOptionId { get; set; }

        public int ModifierGroupId { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal PriceAdd { get; set; }

        public bool IsActive { get; set; }

    public bool AllowQuantity { get; set; }

    public bool IsDefault { get; set; }

    public int SortOrder { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public ModifierGroup? ModifierGroup { get; set; }

        public ICollection<ModifierOptionTranslation> ModifierOptionTranslations { get; set; } = new List<ModifierOptionTranslation>();
    }
}