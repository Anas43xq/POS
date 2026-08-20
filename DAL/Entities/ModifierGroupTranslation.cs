using System;

namespace DAL.Entities
{
    public class ModifierGroupTranslation
    {
        public int ModifierGroupTranslationId { get; set; }

        public int ModifierGroupId { get; set; }

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ModifierGroup? ModifierGroup { get; set; }
    }
}