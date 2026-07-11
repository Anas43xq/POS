using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;

namespace BLL.Models
{
    [SupportedOSPlatform("windows")]
    public class CartItem : INotifyPropertyChanged
    {
        public int VariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        private string _localizedProductName = string.Empty;

        /// <summary>
        /// Localized display name shown in the cart UI.
        /// <see cref="ProductName"/> remains English-only for receipt snapshots.
        /// </summary>
        public string LocalizedProductName
        {
            get => _localizedProductName;
            set
            {
                if (_localizedProductName == value)
                    return;

                _localizedProductName = value;
                OnPropertyChanged();
            }
        }

        private int _quantity = 1;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 1)
                    value = 1;

                if (_quantity == value)
                    return;

                _quantity = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(LineSubtotal));
                OnPropertyChanged(nameof(LineTax));
                OnPropertyChanged(nameof(LineTotal));
            }
        }

        public decimal UnitPrice { get; set; }

        public decimal TaxRate { get; set; }

        public decimal LineSubtotal => Math.Round(UnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);

        public decimal LineTax => Math.Round(LineSubtotal * TaxRate, 2, MidpointRounding.AwayFromZero);

        public decimal LineTotal => Math.Round(LineSubtotal + LineTax, 2, MidpointRounding.AwayFromZero);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
