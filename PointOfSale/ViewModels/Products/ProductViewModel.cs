using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace UI.ViewModels
{
    public class ProductViewModel
    {
        public Product Model { get; }

        public ProductViewModel(Product model)
        {
            Model = model;
        }

        public string Name => Model.Name;
        public decimal UnitPrice => Model.UnitPrice;
    }
}
