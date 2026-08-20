using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BLL.Interfaces;
using BLL.DTOs;
using UI.Commands;
using UI.Services;
using UI.Views;

namespace UI.ViewModels
{
    public class AddEditCategoryViewModel : BaseViewModel
    {
        private readonly ICategoryService? _categoryService;
        private readonly ILocalizationService? _localization;
        private readonly IViewModelFactory? _viewModelFactory;
        private string _name = string.Empty;
        private ParentCategoryOption? _selectedParent;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private bool _isBusy;

        public AddEditCategoryViewModel() : this(null, null, null)
        {
        }

        public AddEditCategoryViewModel(
            ICategoryService? categoryService,
            ILocalizationService? localization = null,
            IViewModelFactory? viewModelFactory = null)
        {
            _categoryService = categoryService;
            _localization = localization;
            _viewModelFactory = viewModelFactory;
            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            TranslationsCommand = new RelayCommand(OpenTranslations, () => CategoryId > 0);

            ParentCategoryOptions = new ObservableCollection<ParentCategoryOption>
            {
                new ParentCategoryOption { CategoryId = null, DisplayName = _localization?.GetString("Category.NoParent") ?? "— None —" }
            };

            if (_categoryService != null)
            {
                _ = LoadParentCategoriesAsync();
            }
        }

        private async System.Threading.Tasks.Task LoadParentCategoriesAsync()
        {
            if (_categoryService == null) return;

            IsBusy = true;

            try
            {
                var languageCode = _localization?.CurrentLanguage.FilePrefix ?? "en";
                var result = await _categoryService.GetAllCategoriesWithChildrenAsync(languageCode);
                if (result.IsSuccess && result.Value != null)
                {
                    var existingRootIds = ParentCategoryOptions
                        .Where(option => option.CategoryId != null)
                        .Select(option => option.CategoryId!.Value)
                        .ToHashSet();

                    foreach (var c in result.Value.Where(c => c.ParentCategoryId == null && c.CategoryId != CategoryId))
                    {
                        if (existingRootIds.Add(c.CategoryId))
                        {
                            ParentCategoryOptions.Add(new ParentCategoryOption
                            {
                                CategoryId = c.CategoryId,
                                DisplayName = c.Name
                            });
                        }
                    }
                }
            }
            finally
            {
                IsBusy = false;
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

                    RaiseCommandStatesChanged();
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

                    RaiseCommandStatesChanged();
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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value)
                    return;

                _isBusy = value;
                OnPropertyChanged();
                RaiseCommandStatesChanged();
            }
        }

        private bool CanSave() => !IsBusy && !string.IsNullOrWhiteSpace(Name);

        private async Task SaveAsync()
        {
            if (_categoryService == null)
            {
                HasError = true;
                ErrorMessage = "Category service is not available.";
                return;
            }

            IsBusy = true;

            try
            {
                var dto = new CategoryDto
                {
                    CategoryId = CategoryId,
                    Name = Name.Trim(),
                    ParentCategoryId = SelectedParent?.CategoryId,
                    Description = null
                };

                var result = CategoryId > 0
                    ? await _categoryService.UpdateCategoryAsync(dto)
                    : await _categoryService.AddCategoryAsync(dto);

                if (!result.IsSuccess)
                {
                    HasError = true;
                    ErrorMessage = result.Error ?? "Failed to save category.";
                    return;
                }

                HasError = false;
                ErrorMessage = string.Empty;
                RequestClose?.Invoke();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void Cancel() => RequestClose?.Invoke();

        private void OpenTranslations()
        {
            if (CategoryId <= 0 || _viewModelFactory == null) return;

            var vm = _viewModelFactory.Create<TranslationDialogViewModel>(
                TranslationDialogViewModel.EntityType.Category,
                CategoryId,
                Name);

            var dialog = new TranslationDialogView { DataContext = vm };
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, dialog))
                dialog.Owner = owner;
            vm.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
        }

        public sealed class ParentCategoryOption
        {
            public int? CategoryId { get; set; }

            public string DisplayName { get; set; } = string.Empty;
        }

        private void RaiseCommandStatesChanged()
        {
            if (SaveCommand is AsyncRelayCommand save) save.RaiseCanExecuteChanged();
        }
    }
}
