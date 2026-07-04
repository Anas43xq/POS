using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int? ParentCategoryId { get; set; }

        public Category? ParentCategory { get; set; }
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;


        public ICollection<Category> ChildCategories { get; set; } 
         = new List<Category>();

        public ICollection<Product> Products { get; set; }
         = new List<Product>();

    }
}
