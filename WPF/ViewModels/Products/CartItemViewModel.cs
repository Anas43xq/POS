using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace UI.ViewModels
{
    public class CartItemViewModel : BaseViewModel
    {
        public Product Product { get; }

        private int _quantity;

        public CartItemViewModel(Product product)
        {
            Product = product;
            _quantity = 1;
        }

        public string ProductName => Product.Name;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;

                _quantity = value;
                OnPropertyChanged();
            }
        }

        // NOTE: Unused legacy class. Product-level pricing was removed as
        // part of the variant-pricing migration; this class references
        // DAL.Entities.Product directly and has no ProductVariant context
        // to source a price from, so UnitPrice/LineTotal (never called
        // anywhere) were removed rather than reintroducing product-level
        // pricing to keep them compiling.
    }
}
