using System.Collections.ObjectModel;

namespace UI.ViewModels
{
    public class CategoryCardViewModel : BaseViewModel
    {
        private bool _isExpanded;
        private bool _isSelected;

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Icon { get; set; } = "📦";

        public string CountLabel { get; set; } = string.Empty;

        public bool HasSubcategories { get; set; }

        public ObservableCollection<SubcategoryCardViewModel> Subcategories { get; } = new();

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public CategoryCardViewModel()
        {
        }
    }
}
