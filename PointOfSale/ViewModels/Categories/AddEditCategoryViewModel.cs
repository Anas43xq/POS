using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.Extensions.DependencyInjection;
using UI.Commands;
using UI.Views;

namespace UI.ViewModels
{
    public class AddEditCategoryViewModel : BaseViewModel
    {
        private readonly ICategoryService? _categoryService;
        private readonly ILocalizationService? _localization;
        private string _name = string.Empty;
        private string _icon = "📦";
        private ParentCategoryOption? _selectedParent;
        private bool _hasError;
        private string _errorMessage = string.Empty;

        public AddEditCategoryViewModel() : this(null, null)
        {
        }

        public AddEditCategoryViewModel(ICategoryService? categoryService, ILocalizationService? localization = null)
        {
            _categoryService = categoryService;
            _localization = localization;
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            TranslationsCommand = new RelayCommand(OpenTranslations, () => CategoryId > 0);

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

            var languageCode = _localization?.CurrentLanguage.FilePrefix ?? "en";
            var result = await _categoryService.GetAllCategoriesWithChildrenAsync(languageCode);
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

        public int CategoryId { get; set; }

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

                    // Ensure the selected option is in the list (for async loading scenarios)
                    if (value != null && value.CategoryId != null &&
                        !ParentCategoryOptions.Any(o => o.CategoryId == value.CategoryId))
                    {
                        ParentCategoryOptions.Add(value);
                    }
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

        public ICommand TranslationsCommand { get; }

        public Action? RequestClose { get; set; }

        private bool CanSave() => !string.IsNullOrWhiteSpace(Name);

        private async void Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                HasError = true;
                ErrorMessage = "Category name is required.";
                return;
            }

            if (_categoryService == null)
            {
                HasError = true;
                ErrorMessage = "Category service is not available.";
                return;
            }

            try
            {
                var dto = new CategoryDto
                {
                    CategoryId = CategoryId,
                    Name = Name.Trim(),
                    ParentCategoryId = SelectedParent?.CategoryId,
                    Description = Icon
                };

                if (CategoryId > 0)
                    await _categoryService.UpdateCategoryAsync(dto);
                else
                    await _categoryService.AddCategoryAsync(dto);

                HasError = false;
                ErrorMessage = string.Empty;
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Failed to save: {ex.Message}";
            }
        }

        private void Cancel() => RequestClose?.Invoke();

        private void OpenTranslations()
        {
            if (CategoryId <= 0) return;

            var vm = new TranslationDialogViewModel(
                TranslationDialogViewModel.EntityType.Category,
                CategoryId,
                Name,
                App.ServiceProvider.GetRequiredService<IProductTranslationService>(),
                App.ServiceProvider.GetRequiredService<ICategoryTranslationService>(),
                App.ServiceProvider.GetRequiredService<ISizeTranslationService>());

            var dialog = new TranslationDialogView { DataContext = vm, Owner = Application.Current.MainWindow };
            vm.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
        }

        public sealed class ParentCategoryOption
        {
            public int? CategoryId { get; set; }

            public string DisplayName { get; set; } = string.Empty;
        }
    }
}
