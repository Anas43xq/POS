using System;

namespace DAL.Entities
{
    public class ModifierOptionTranslation
    {
        public int ModifierOptionTranslationId { get; set; }

        public int ModifierOptionId { get; set; }

        public string LanguageCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public ModifierOption? ModifierOption { get; set; }
    }
}