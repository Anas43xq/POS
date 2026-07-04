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
                OnPropertyChanged(nameof(LineTotal));
            }
        }

        public decimal UnitPrice => Product.UnitPrice;

        public decimal LineTotal => UnitPrice * Quantity;
    }
}
