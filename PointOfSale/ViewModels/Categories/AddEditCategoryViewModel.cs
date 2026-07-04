using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using BLL.Interfaces;
using DAL.Entities;
using UI.Commands;

namespace UI.ViewModels
{
    public class AddEditCategoryViewModel : BaseViewModel
    {
        private readonly ICategoryService? _categoryService;
        private string _name = string.Empty;
        private string _icon = "📦";
        private ParentCategoryOption? _selectedParent;
        private bool _hasError;
        private string _errorMessage = string.Empty;

        public AddEditCategoryViewModel() : this(null)
        {
        }

        public AddEditCategoryViewModel(ICategoryService? categoryService)
        {
            _categoryService = categoryService;
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            ParentCategoryOptions = new ObservableCollection<ParentCategoryOption>
            {
                new ParentCategoryOption { CategoryId = null, DisplayName = "— None —" }
            };

            if (_categoryService != null)
            {
                _ = LoadParentCategoriesAsync();
            }
        }

        private async System.Threading.Tasks.Task LoadParentCategoriesAsync()
        {
            if (_categoryService == null) return;

            var result = await _categoryService.GetAllCategoriesAsync();
            if (result.IsSuccess && result.Value != null)
            {
                foreach (var c in result.Value.Where(c => c.ParentCategoryId == null))
                {
                    ParentCategoryOptions.Add(new ParentCategoryOption
                    {
                        CategoryId = c.CategoryId,
                        DisplayName = c.Name
                    });
                }
            }
        }

        public string DialogTitle { get; set; } = "Add Category";

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                    if (_hasError)
                    {
                        HasError = false;
                        ErrorMessage = string.Empty;
                    }
                }
            }
        }

        public string Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged();
                }
            }
        }

        public ParentCategoryOption? SelectedParent
        {
            get => _selectedParent;
            set
            {
                if (_selectedParent != value)
                {
                    _selectedParent = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<ParentCategoryOption> ParentCategoryOptions { get; }

        public bool HasError
        {
            get => _hasError;
            set
            {
                if (_hasError != value)
                {
                    _hasError = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }

        public ICommand CancelCommand { get; }

        public Action? RequestClose { get; set; }

        private bool CanSave() => !string.IsNullOrWhiteSpace(Name);

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                HasError = true;
                ErrorMessage = "Category name is required.";
                return;
            }

            HasError = false;
            ErrorMessage = string.Empty;
            RequestClose?.Invoke();
        }

        private void Cancel() => RequestClose?.Invoke();

        public sealed class ParentCategoryOption
        {
            public int? CategoryId { get; set; }

            public string DisplayName { get; set; } = string.Empty;
        }
    }
}
