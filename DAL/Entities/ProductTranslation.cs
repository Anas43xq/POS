using System;

namespace DAL.Entities;

public class ProductTranslation
{
    public int ProductTranslationId { get; set; }

    public int ProductId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public Product Product { get; set; } = null!;
}
