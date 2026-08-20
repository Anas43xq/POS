using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IViewModelFactory _viewModelFactory;
        private readonly ObservableCollection<CategoryCardViewModel> _allCategories = new();
        private string _searchText = string.Empty;
        private CategoryCardViewModel? _selectedCategory;
        private SubcategoryCardViewModel? _selectedSubcategory;
        private CancellationTokenSource? _loadCts;
        private bool _isBusy;


        public CategoryManagementViewModel(
            ICategoryService categoryService,
            ILocalizationService localization,
            IViewModelFactory viewModelFactory)
        {
            _categoryService = categoryService;
            _localization = localization;
            _viewModelFactory = viewModelFactory;

            _localization.LanguageChanged += OnLanguageChanged;

            AddCommand = new AsyncRelayCommand(OpenAddDialogAsync, () => !IsBusy);
            EditCommand = new AsyncRelayCommand(OpenEditDialogAsync, CanEdit);
            DeleteCommand = new AsyncRelayCommand(DeleteSelectedAsync, CanDelete);
            RefreshCommand = new AsyncRelayCommand(RefreshAsync, () => !IsBusy);
            SelectCategoryCommand = new RelayCommand<object?>(SelectCategory);
            SelectSubcategoryCommand = new RelayCommand<object?>(SelectSubcategory);
            AddSubcategoryCommand = new AsyncRelayCommand(OpenAddSubcategoryDialogAsync, () => !IsBusy && SelectedCategory != null);

            // NOTE: Data is intentionally NOT loaded here — see
            // ProductManagementViewModel for the rationale. Load is
            // triggered on first navigation via EnsureDataLoadedAsync(),
            // called from ManagerMainViewModel.NavigateToCategoryManagement().
        }

        private bool _hasLoadedOnce;

        /// <summary>
        /// Loads data the first time this page is navigated to; subsequent
        /// navigations are no-ops (use RefreshCommand to force a reload).
        /// </summary>
        public Task EnsureDataLoadedAsync()
        {
            if (_hasLoadedOnce)
                return Task.CompletedTask;

            _hasLoadedOnce = true;
            return LoadDataAsync();
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
                    if (_selectedSubcategory != null)
                    {
                        _selectedSubcategory.IsSelected = false;
                        _selectedSubcategory = null;
                        OnPropertyChanged(nameof(SelectedSubcategory));
                    }

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasSelection));
                    OnPropertyChanged(nameof(IsSubcategoriesPanelVisible));
                    OnPropertyChanged(nameof(IsSubcategoriesEmpty));
                    RefreshSubcategories();
                    UpdateSelectionState();
                    RaiseCommandStatesChanged();
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
                    OnPropertyChanged(nameof(HasSelection));
                    RaiseCommandStatesChanged();
                }
            }
        }

        public bool HasSelection => SelectedSubcategory != null || SelectedCategory != null;

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
            try
            {
                _loadCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                _loadCts = null;
            }

            var cts = new CancellationTokenSource();
            _loadCts = cts;
            IsBusy = true;

            var languageCode = _localization.CurrentLanguage.FilePrefix;

            try
            {
                var result = await _categoryService.GetAllCategoriesWithChildrenAsync(languageCode);
                if (cts.Token.IsCancellationRequested)
                    return;

                _allCategories.Clear();

                if (result.IsSuccess && result.Value != null)
                {
                    foreach (var category in result.Value.Where(category => category.ParentCategoryId == null))
                    {
                        _allCategories.Add(ToCategoryCard(category));
                    }
                }

                ApplyFilters();
            }
            finally
            {
                if (ReferenceEquals(_loadCts, cts))
                {
                    _loadCts = null;
                    IsBusy = false;
                }

                cts.Dispose();
            }
        }

        private CategoryCardViewModel ToCategoryCard(CategoryDto category)
        {
            var card = new CategoryCardViewModel
            {
                Id = category.CategoryId,
                ParentCategoryId = category.ParentCategoryId,
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
                        Id = child.CategoryId,
                        ParentCategoryId = category.CategoryId,
                        ParentCategoryName = category.Name,
                        Name = child.Name,
                        CountLabel = $"{child.ProductCount} products"
                    });
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
                : _allCategories
                    .Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                c.Subcategories.Any(sub => sub.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(c => c.HasSubcategories)
                    .ThenBy(c => c.Name)
                    .ToList();

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

            var query = SearchText.Trim();
            var showAllChildren = string.IsNullOrWhiteSpace(query) ||
                                  SelectedCategory.Name.Contains(query, StringComparison.OrdinalIgnoreCase);

            foreach (var child in SelectedCategory.Subcategories.Where(child =>
                         showAllChildren || child.Name.Contains(query, StringComparison.OrdinalIgnoreCase)))
            {
                Subcategories.Add(child);
            }

            if (SelectedSubcategory != null && !Subcategories.Contains(SelectedSubcategory))
            {
                SelectedSubcategory = null;
            }

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

            foreach (var subcategory in Subcategories)
            {
                subcategory.IsSelected = subcategory == SelectedSubcategory;
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
            UpdateSelectionState();
        }

        private async Task OpenAddDialogAsync()
        {
            var viewModel = _viewModelFactory.Create<AddEditCategoryViewModel>();
            viewModel.DialogTitle = _localization.GetString("Category.AddTitle");
            var dialog = new AddEditCategoryDialog { DataContext = viewModel };
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, dialog))
                dialog.Owner = owner;
            viewModel.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
            await RefreshAsync();
        }

        private async Task OpenEditDialogAsync()
        {
            var selectedTarget = GetSelectedTarget();
            if (selectedTarget == null)
            {
                return;
            }

            var viewModel = _viewModelFactory.Create<AddEditCategoryViewModel>();
            viewModel.CategoryId = selectedTarget.CategoryId;
            viewModel.DialogTitle = _localization.GetString("Category.EditTitle");
            viewModel.Name = selectedTarget.Name;
            viewModel.SelectedParent = selectedTarget.ParentCategoryId == null
                ? new AddEditCategoryViewModel.ParentCategoryOption { DisplayName = _localization.GetString("Category.NoParent") }
                : new AddEditCategoryViewModel.ParentCategoryOption
                {
                    CategoryId = selectedTarget.ParentCategoryId,
                    DisplayName = selectedTarget.ParentCategoryName ?? _localization.GetString("Category.NoParent")
                };

            var dialog = new AddEditCategoryDialog { DataContext = viewModel };
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, dialog))
                dialog.Owner = owner;
            viewModel.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
            await RefreshAsync();
        }

        private async Task OpenAddSubcategoryDialogAsync()
        {
            var parentOption = new AddEditCategoryViewModel.ParentCategoryOption
            {
                CategoryId = SelectedCategory?.Id,
                DisplayName = SelectedCategory?.Name ?? _localization.GetString("Category.NoParent")
            };

            var viewModel = _viewModelFactory.Create<AddEditCategoryViewModel>();
            viewModel.DialogTitle = _localization.GetString("Category.AddSubcategoryTitle");
            viewModel.SelectedParent = parentOption;

            var dialog = new AddEditCategoryDialog { DataContext = viewModel };
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, dialog))
                dialog.Owner = owner;
            viewModel.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
            await RefreshAsync();
        }

        private async Task DeleteSelectedAsync()
        {
            var selectedTarget = GetSelectedTarget();
            if (selectedTarget == null)
            {
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete \"{selectedTarget.Name}\"? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            await _categoryService.DeleteCategoryAsync(selectedTarget.CategoryId);
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            var selectedTarget = GetSelectedTarget();
            SearchText = string.Empty;
            await LoadDataAsync();

            if (selectedTarget != null)
            {
                if (selectedTarget.ParentCategoryId != null)
                {
                    SelectedCategory = FilteredCategories.FirstOrDefault(category => category.Id == selectedTarget.ParentCategoryId)
                        ?? FilteredCategories.FirstOrDefault();

                    if (SelectedCategory != null)
                    {
                        SelectedSubcategory = Subcategories.FirstOrDefault(subcategory => subcategory.Id == selectedTarget.CategoryId);
                        UpdateSelectionState();
                    }
                }
                else
                {
                    SelectedCategory = FilteredCategories.FirstOrDefault(category => category.Id == selectedTarget.CategoryId)
                        ?? FilteredCategories.FirstOrDefault();
                }
            }
            else if (FilteredCategories.Count > 0 && SelectedCategory == null)
            {
                SelectedCategory = FilteredCategories[0];
            }
        }

        private bool CanEdit() => !IsBusy && HasSelection;

        private bool CanDelete() => !IsBusy && HasSelection;

        private void RaiseCommandStatesChanged()
        {
            if (AddCommand is AsyncRelayCommand add) add.RaiseCanExecuteChanged();
            if (EditCommand is AsyncRelayCommand edit) edit.RaiseCanExecuteChanged();
            if (DeleteCommand is AsyncRelayCommand delete) delete.RaiseCanExecuteChanged();
            if (RefreshCommand is AsyncRelayCommand refresh) refresh.RaiseCanExecuteChanged();
            if (AddSubcategoryCommand is AsyncRelayCommand addSub) addSub.RaiseCanExecuteChanged();
        }

        private SelectedCategoryTarget? GetSelectedTarget()
        {
            if (SelectedSubcategory != null)
            {
                return new SelectedCategoryTarget(
                    SelectedSubcategory.Id,
                    SelectedSubcategory.Name,
                    SelectedSubcategory.ParentCategoryId,
                    SelectedSubcategory.ParentCategoryName);
            }

            if (SelectedCategory != null)
            {
                return new SelectedCategoryTarget(
                    SelectedCategory.Id,
                    SelectedCategory.Name,
                    SelectedCategory.ParentCategoryId,
                    null);
            }

            return null;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _localization.LanguageChanged -= OnLanguageChanged;
                _loadCts?.Cancel();
                _loadCts?.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed record SelectedCategoryTarget(
            int CategoryId,
            string Name,
            int? ParentCategoryId,
            string? ParentCategoryName);
    }
}
