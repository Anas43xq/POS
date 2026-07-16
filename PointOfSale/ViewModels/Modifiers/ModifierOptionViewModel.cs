using System;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels.Modifiers
{
    public class ModifierOptionViewModel : BaseViewModel
    {
        public int ModifierOptionId { get; init; }

        public string Name { get; set; } = string.Empty;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                    return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public bool HasPriceAdd { get; init; }

        public string PriceAddDisplay { get; init; } = string.Empty;

        public ICommand SelectCommand { get; }

        public ModifierOptionViewModel(Action onSelected)
        {
            SelectCommand = new RelayCommand(() =>
            {
                IsSelected = !IsSelected;
                onSelected();
            });
        }
    }
}