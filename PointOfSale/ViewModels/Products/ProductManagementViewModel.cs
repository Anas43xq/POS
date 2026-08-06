using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using BLL.Interfaces;
using Contracts.Enum;
using DAL.Entities;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public partial class ProductManagementViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ITaxRateService _taxRateService;
        private readonly IDialogService _dialogService;
        private readonly ILocalizationService _localization;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly INotificationService _notifications;
        private readonly List<CategoryNodeViewModel> _allCategoryNodes = new();
        private readonly ObservableCollection<ProductRowViewModel> _allProducts = new();
        private readonly DispatcherTimer _searchDebounceTimer;
        private string _categorySearchText = string.Empty;
        private string _productSearchText = string.Empty;
        private CategoryNodeViewModel? _selectedCategory;
        private ProductRowViewModel? _selectedProduct;
        private HashSet<int>? _cachedCategoryIds;
        private CancellationTokenSource? _loadCts;


        public ProductManagementViewModel(
            IProductService productService,
            ICategoryService categoryService,
            ITaxRateService taxRateService,
            IDialogService dialogService,
            ILocalizationService localization,
            IViewModelFactory viewModelFactory,
            INotificationService notifications)
        {
            _productService = productService;
            _categoryService = categoryService;
            _taxRateService = taxRateService;
            _dialogService = dialogService;
            _localization = localization;
            _viewModelFactory = viewModelFactory;
            _notifications = notifications;

            Products = CollectionViewSource.GetDefaultView(_allProducts);
            Products.Filter = FilterProduct;
            Products.SortDescriptions.Add(
                new SortDescription(nameof(ProductRowViewModel.Name), ListSortDirection.Ascending));

            _localization.LanguageChanged += OnLanguageChanged;

            _searchDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _searchDebounceTimer.Tick += OnSearchDebounceTick;

            AddProductCommand = new RelayCommand(AddProduct);
            EditProductCommand = new RelayCommand(EditProduct, CanEditProduct);
            DeleteProductCommand = new AsyncRelayCommand(
                DeleteProductAsync,
                CanDeleteProduct,
                onError: ex => _notifications.ShowError($"Failed to delete product: {ex.Message}"));
            RefreshCommand = new AsyncRelayCommand(RefreshDataAsync);

            // NOTE: Data is intentionally NOT loaded here. This VM is a
            // mandatory constructor parameter of ManagerMainViewModel, so an
            // eager load here would fire on every manager login regardless
            // of whether the manager ever opens the Products page. Instead,
            // ManagerMainViewModel.NavigateToProductManagement() triggers
            // EnsureDataLoadedAsync() the first time this page is actually
            // navigated to, matching the existing lazy pattern already used
            // by TransactionsViewModel/ShiftManagementViewModel. See
            // login-performance-analysis.md §11.
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

        public ObservableCollection<CategoryNodeViewModel> CategoryRoots { get; } = new();

        // Products is an ICollectionView over the stable _allProducts
        // collection rather than a separately-maintained list that gets
        // Clear()'d and rebuilt with Add() on every keystroke. Filtering
        // now calls Products.Refresh(), which re-evaluates the Filter
        // predicate and raises a single CollectionChanged(Reset) instead
        // of one Add notification per matching row — the DataGrid
        // re-measures once per search instead of once per item.
        public ICollectionView Products { get; }

        public string CategorySearchText
        {
            get => _categorySearchText;
            set
            {
                if (_categorySearchText != value)
                {
                    _categorySearchText = value;
                    OnPropertyChanged();
                    RestartSearchDebounce();
                }
            }
        }

        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                if (_productSearchText != value)
                {
                    _productSearchText = value;
                    OnPropertyChanged();
                    RestartSearchDebounce();
                }
            }
        }

        public CategoryNodeViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory != value)
                {
                    _selectedCategory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CurrentCategoryLabel));
                    CacheCategoryIds();
                    ApplyProductFilter();
                }
            }
        }

        public ProductRowViewModel? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanDelete));
                    if (EditProductCommand is RelayCommand editCmd) editCmd.RaiseCanExecuteChanged();
                    if (DeleteProductCommand is AsyncRelayCommand deleteCmd) deleteCmd.RaiseCanExecuteChanged();
                }
            }
        }

        public string CurrentCategoryLabel => SelectedCategory == null ? "All Categories" : SelectedCategory.DisplayName;
        public bool CanEdit => SelectedProduct != null;
        public bool CanDelete => SelectedProduct != null;

        public ICommand AddProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand RefreshCommand { get; }

    }

    public class CategoryNodeViewModel : BaseViewModel
    {
        private bool _isExpanded = true;

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string Icon { get; set; } = "📁";
        public bool IsRoot { get; set; }
        public ObservableCollection<CategoryNodeViewModel> Children { get; set; } = new();
        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "Untitled" : Name;

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

        public override string ToString() => DisplayName;
    }

    public partial class ProductManagementViewModel
    {
        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _localization.LanguageChanged -= OnLanguageChanged;
                _searchDebounceTimer.Tick -= OnSearchDebounceTick;
                _searchDebounceTimer.Stop();
                _loadCts?.Cancel();
                _loadCts?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    public class ProductRowViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int TaxRateId { get; set; }
        public string TaxRateName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
