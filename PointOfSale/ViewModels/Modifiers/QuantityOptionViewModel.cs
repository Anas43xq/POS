using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels.Modifiers
{
    public class QuantityOptionViewModel : BaseViewModel
    {
        public int ModifierOptionId { get; init; }

        public string Name { get; set; } = string.Empty;

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value || value < 0) return;
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UnitPriceDisplay));
            }
        }

        public decimal UnitPrice { get; init; }

        public string UnitPriceDisplay => $"{UnitPrice:N2} AED";

        public ICommand IncrementCommand { get; }

        public ICommand DecrementCommand { get; }

        public QuantityOptionViewModel(System.Action onChanged)
        {
            IncrementCommand = new RelayCommand(() =>
            {
                Quantity++;
                onChanged();
            });

            DecrementCommand = new RelayCommand(() =>
            {
                if (Quantity > 0)
                {
                    Quantity--;
                    onChanged();
                }
            });
        }
    }
}