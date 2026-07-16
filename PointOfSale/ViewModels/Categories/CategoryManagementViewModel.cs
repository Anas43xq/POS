using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using BLL.DTOs;
using BLL.Interfaces;
using Contracts.Enum;
using UI.Commands;
using UI.Services;
using UI.Views;

namespace UI.ViewModels
{
    public class CategoryManagementViewModel : BaseViewModel
    {
        private readonly ICategoryService _categoryService;
        private readonly ILocalizationService _localization;
        private readonly ObservableCollection<CategoryCardViewModel> _allCategories = new();
        private string _searchText = string.Empty;
        private CategoryCardViewModel? _selectedCategory;
        private SubcategoryCardViewModel? _selectedSubcategory;


        public CategoryManagementViewModel(
            ICategoryService categoryService,
            ILocalizationService localization)
        {
            _categoryService = categoryService;
            _localization = localization;

            _localization.LanguageChanged += OnLanguageChanged;

            AddCommand = new RelayCommand(OpenAddDialog);
            EditCommand = new RelayCommand(OpenEditDialog, CanEdit);
            DeleteCommand = new RelayCommand(DeleteSelected, CanDelete);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync);
            SelectCategoryCommand = new RelayCommand<object?>(SelectCategory);
            SelectSubcategoryCommand = new RelayCommand<object?>(SelectSubcategory);
            AddSubcategoryCommand = new RelayCommand(OpenAddSubcategoryDialog, () => SelectedCategory != null);

            _ = LoadDataAsync();
        }

        public ObservableCollection<CategoryCardViewModel> FilteredCategories { get; } = new();

        public ObservableCollection<SubcategoryCardViewModel> Subcategories { get; } = new();

        public CategoryCardViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasSelection));
                    OnPropertyChanged(nameof(IsSubcategoriesPanelVisible));
                    OnPropertyChanged(nameof(IsSubcategoriesEmpty));
                    RefreshSubcategories();
                    UpdateSelectionState();
                }
            }
        }

        public SubcategoryCardViewModel? SelectedSubcategory
        {
            get => _selectedSubcategory;
            set
            {
                if (_selectedSubcategory != value)
                {
                    _selectedSubcategory = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasSelection => SelectedCategory != null;

        public bool IsSubcategoriesPanelVisible => SelectedCategory != null && SelectedCategory.HasSubcategories;

        public bool IsSubcategoriesEmpty => SelectedCategory?.HasSubcategories == true && Subcategories.Count == 0;

        public bool IsEmptySearch => !string.IsNullOrWhiteSpace(SearchText) && FilteredCategories.Count == 0;

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsEmptySearch));
                    ApplyFilters();
                }
            }
        }

        public ICommand AddCommand { get; }

        public ICommand EditCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand RefreshCommand { get; }

        public ICommand SelectCategoryCommand { get; }

        public ICommand SelectSubcategoryCommand { get; }

        public ICommand AddSubcategoryCommand { get; }

        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            var languageCode = _localization.CurrentLanguage.FilePrefix;
            var result = await _categoryService.GetAllCategoriesWithChildrenAsync(languageCode);

            _allCategories.Clear();

            if (result.IsSuccess && result.Value != null)
            {
                foreach (var category in result.Value)
                {
                    _allCategories.Add(ToCategoryCard(category));
                }
            }

            ApplyFilters();
        }

        private CategoryCardViewModel ToCategoryCard(CategoryDto category)
        {
            var card = new CategoryCardViewModel
            {
                Id = category.CategoryId,
                Name = category.Name,
                Icon = string.IsNullOrWhiteSpace(category.Description) ? "📦" : (category.Description.StartsWith("http") || category.Description.Length <= 2 ? category.Description : "📦"),
                HasSubcategories = category.ChildCategories != null && category.ChildCategories.Any(),
                IsExpanded = false
            };

            int totalProducts = category.ProductCount;

            if (category.ChildCategories != null)
            {
                foreach (var child in category.ChildCategories.OrderBy(c => c.Name))
                {
                    card.Subcategories.Add(new SubcategoryCardViewModel
                    {
                        Name = child.Name,
                        CountLabel = $"{child.ProductCount} products"
                    });
                    totalProducts += child.ProductCount;
                }
            }

            card.CountLabel = totalProducts == 0 ? "No products" : $"{totalProducts} products";

            return card;
        }

        private void ApplyFilters()
        {
            FilteredCategories.Clear();

            var query = SearchText.Trim();
            var filtered = string.IsNullOrWhiteSpace(query)
                ? _allCategories.OrderByDescending(c => c.HasSubcategories).ThenBy(c => c.Name).ToList()
                : _allCategories.Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).OrderByDescending(c => c.HasSubcategories).ThenBy(c => c.Name).ToList();

            foreach (var category in filtered)
            {
                FilteredCategories.Add(category);
            }

            if (SelectedCategory != null && !FilteredCategories.Contains(SelectedCategory))
            {
                SelectedCategory = null;
            }

            if (SelectedCategory == null && FilteredCategories.Count > 0)
            {
                SelectedCategory = FilteredCategories[0];
            }

            OnPropertyChanged(nameof(IsEmptySearch));
        }

        private void RefreshSubcategories()
        {
            Subcategories.Clear();
            if (SelectedCategory == null)
            {
                SelectedSubcategory = null;
                OnPropertyChanged(nameof(IsSubcategoriesEmpty));
                return;
            }

            foreach (var child in SelectedCategory.Subcategories)
            {
                Subcategories.Add(child);
            }

            SelectedSubcategory = null;
            OnPropertyChanged(nameof(IsSubcategoriesEmpty));
        }

        private void UpdateSelectionState()
        {
            foreach (var category in _allCategories)
            {
                category.IsSelected = category == SelectedCategory;
            }

            foreach (var category in FilteredCategories)
            {
                category.IsSelected = category == SelectedCategory;
            }
        }

        private void SelectCategory(object? parameter)
        {
            if (parameter is not CategoryCardViewModel category)
            {
                return;
            }

            if (SelectedCategory == category)
            {
                category.IsExpanded = !category.IsExpanded;
            }

            SelectedCategory = category;
            SelectedCategory.IsExpanded = true;
            RefreshSubcategories();
            OnPropertyChanged(nameof(IsSubcategoriesPanelVisible));
        }

        private void SelectSubcategory(object? parameter)
        {
            if (parameter is not SubcategoryCardViewModel subcategory)
            {
                return;
            }

            foreach (var item in Subcategories)
            {
                item.IsSelected = item == subcategory;
            }

            SelectedSubcategory = subcategory;
        }

        private void OpenAddDialog()
        {
            var viewModel = new AddEditCategoryViewModel(_categoryService, _localization) { DialogTitle = "Add Category" };
            var dialog = new AddEditCategoryDialog { DataContext = viewModel };
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, dialog))
                dialog.Owner = owner;
            viewModel.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
            _ = RefreshAsync();
        }

        private void OpenEditDialog()
        {
            if (SelectedCategory == null)
            {
                return;
            }

            var viewModel = new AddEditCategoryViewModel(_categoryService, _localization)
            {
                CategoryId = SelectedCategory.Id,
                DialogTitle = "Edit Category",
                Name = SelectedCategory.Name,
                Icon = SelectedCategory.Icon,
                SelectedParent = new AddEditCategoryViewModel.ParentCategoryOption { DisplayName = "— None —" }
            };

            var dialog = new AddEditCategoryDialog { DataContext = viewModel };
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, dialog))
                dialog.Owner = owner;
            viewModel.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
            _ = RefreshAsync();
        }

        private void OpenAddSubcategoryDialog()
        {
            var parentOption = new AddEditCategoryViewModel.ParentCategoryOption
            {
                CategoryId = SelectedCategory?.Id,
                DisplayName = SelectedCategory?.Name ?? "— None —"
            };

            var viewModel = new AddEditCategoryViewModel(_categoryService, _localization)
            {
                DialogTitle = "Add Subcategory",
                SelectedParent = parentOption
            };

            var dialog = new AddEditCategoryDialog { DataContext = viewModel };
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, dialog))
                dialog.Owner = owner;
            viewModel.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
            _ = RefreshAsync();
        }

        private void DeleteSelected()
        {
            if (SelectedCategory == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete \"{SelectedCategory.Name}\"? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            _allCategories.Remove(SelectedCategory);
            FilteredCategories.Remove(SelectedCategory);
            SelectedCategory = FilteredCategories.FirstOrDefault();
            RefreshSubcategories();
            OnPropertyChanged(nameof(IsEmptySearch));
        }

        private async Task RefreshAsync()
        {
            SearchText = string.Empty;
            await LoadDataAsync();
            if (FilteredCategories.Count > 0 && SelectedCategory == null)
            {
                SelectedCategory = FilteredCategories[0];
            }
            else if (SelectedCategory != null)
            {
                SelectedCategory = FilteredCategories.FirstOrDefault(c => c.Id == SelectedCategory.Id) ?? FilteredCategories.FirstOrDefault();
            }
        }

        private bool CanEdit() => SelectedCategory != null;

        private bool CanDelete() => SelectedCategory != null;

    }
}
