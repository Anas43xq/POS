using System;

namespace DAL.Entities;

public class CategoryTranslation
{
    public int CategoryTranslationId { get; set; }

    public int CategoryId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Category Category { get; set; } = null!;
}
