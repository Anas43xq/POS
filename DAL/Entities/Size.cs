using System;
using System.Collections.Generic;

namespace DAL.Entities;

public class Size
{
    public int SizeId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ICollection<SizeTranslation> SizeTranslations { get; set; } = [];

    public ICollection<ProductVariant> ProductVariants { get; set; } = [];
}
