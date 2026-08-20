using System;

namespace DAL.Entities;

public class SizeTranslation
{
    public int SizeTranslationId { get; set; }

    public int SizeId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public Size Size { get; set; } = null!;
}
