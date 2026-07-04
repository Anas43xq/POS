namespace UI.ViewModels
{
    public class SubcategoryCardViewModel : BaseViewModel
    {
        private bool _isSelected;

        public string Name { get; set; } = string.Empty;

        public string CountLabel { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
