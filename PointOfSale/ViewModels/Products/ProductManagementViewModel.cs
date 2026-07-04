using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BLL.Interfaces;
using DAL.Entities;
using UI.Commands;
using UI.Services;
using UI.Views;

namespace UI.ViewModels
{
    public class ProductManagementViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ITaxRateService _taxRateService;
        private readonly IDialogService _dialogService;
        private readonly List<CategoryNodeViewModel> _allCategoryNodes = new();
        private readonly List<ProductRowViewModel> _allProducts = new();
        private string _categorySearchText = string.Empty;
        private string _productSearchText = string.Empty;
        private CategoryNodeViewModel? _selectedCategory;
        private ProductRowViewModel? _selectedProduct;

        public ProductManagementViewModel(
            IProductService productService,
            ICategoryService categoryService,
            ITaxRateService taxRateService,
            IDialogService dialogService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _taxRateService = taxRateService;
            _dialogService = dialogService;

            AddProductCommand = new RelayCommand(AddProduct);
            EditProductCommand = new RelayCommand(EditProduct, CanEditProduct);
            DeleteProductCommand = new RelayCommand(DeleteProduct, CanDeleteProduct);
            RefreshCommand = new AsyncRelayCommand(RefreshDataAsync);

            _ = LoadDataAsync();
        }

        public ObservableCollection<CategoryNodeViewModel> CategoryRoots { get; } = new();

        public ObservableCollection<ProductRowViewModel> Products { get; } = new();

        public string CategorySearchText
        {
            get => _categorySearchText;
            set
            {
                if (_categorySearchText != value)
                {
                    _categorySearchText = value;
                    OnPropertyChanged();
                    ApplyCategoryFilter();
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
                    ApplyProductFilter();
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
                    if (DeleteProductCommand is RelayCommand deleteCmd) deleteCmd.RaiseCanExecuteChanged();
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

        private async Task LoadDataAsync()
        {
            var categoriesResult = await _categoryService.GetAllCategoriesWithChildrenAsync();
            if (categoriesResult.IsSuccess && categoriesResult.Value != null)
            {
                _allCategoryNodes.Clear();
                foreach (var c in categoriesResult.Value)
                {
                    _allCategoryNodes.Add(ToCategoryNode(c));
                }
            }

            var productsResult = await _productService.GetAllProductsAsync();
            if (productsResult.IsSuccess && productsResult.Value != null)
            {
                _allProducts.Clear();
                foreach (var p in productsResult.Value)
                {
                    _allProducts.Add(ToProductRow(p));
                }
            }

            ApplyCategoryFilter();
            ApplyProductFilter();
            SelectedCategory = null;
        }

        private CategoryNodeViewModel ToCategoryNode(Category category)
        {
            var node = new CategoryNodeViewModel
            {
                Id = category.CategoryId,
                Name = category.Name,
                ParentId = category.ParentCategoryId,
                Icon = string.IsNullOrWhiteSpace(category.Description) ? "📁" : (category.Description.StartsWith("http") || category.Description.Length <= 2 ? category.Description : "📁"),
                IsRoot = category.ParentCategoryId == null
            };

            if (category.ChildCategories != null)
            {
                foreach (var child in category.ChildCategories)
                {
                    node.Children.Add(ToCategoryNode(child));
                }
            }

            return node;
        }

        private ProductRowViewModel ToProductRow(Product product)
        {
            return new ProductRowViewModel
            {
                Id = product.ProductId,
                Name = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.ParentCategoryId == null && product.Category != null
                    ? product.Category.Name
                    : (product.Category?.ParentCategory?.Name ?? string.Empty),
                Price = product.UnitPrice,
                TaxRateId = product.TaxRateId,
                TaxRateName = product.TaxRate?.Name ?? string.Empty,
                Status = product.IsActive ? "Active" : "Inactive"
            };
        }

        private void ApplyCategoryFilter()
        {
            CategoryRoots.Clear();

            var query = CategorySearchText.Trim();
            var sorted = _allCategoryNodes
                .Where(c => c.Children.Any())
                .OrderBy(c => c.Name)
                .ToList();

            if (string.IsNullOrWhiteSpace(query))
            {
                foreach (var root in sorted)
                {
                    CategoryRoots.Add(root);
                }
                return;
            }

            foreach (var root in sorted)
            {
                var filteredRoot = BuildFilteredBranch(root, query);
                if (filteredRoot != null)
                {
                    CategoryRoots.Add(filteredRoot);
                }
            }
        }

        private CategoryNodeViewModel? BuildFilteredBranch(CategoryNodeViewModel node, string query)
        {
            var children = node.Children
                .Select(child => BuildFilteredBranch(child, query))
                .Where(child => child != null)
                .Cast<CategoryNodeViewModel>()
                .ToList();

            var isMatch = node.Name.Contains(query, StringComparison.OrdinalIgnoreCase) || children.Count > 0;
            if (!isMatch)
            {
                return null;
            }

            // Auto-expand parent nodes when a child matches the search
            bool shouldExpand = children.Count > 0;

            return new CategoryNodeViewModel
            {
                Id = node.Id,
                Name = node.Name,
                ParentId = node.ParentId,
                Icon = node.Icon,
                IsRoot = node.IsRoot,
                IsExpanded = shouldExpand,
                Children = new ObservableCollection<CategoryNodeViewModel>(children)
            };
        }

        private void AddChildCategoryIds(CategoryNodeViewModel node, HashSet<int> ids)
        {
            foreach (var child in node.Children)
            {
                ids.Add(child.Id);
                AddChildCategoryIds(child, ids);
            }
        }

        private void ApplyProductFilter()
        {
            Products.Clear();

            var query = ProductSearchText.Trim();

            // Collect all category IDs (selected + its children)
            var selectedCategoryIds = new HashSet<int>();
            if (SelectedCategory != null && SelectedCategory.Id >= 0)
            {
                selectedCategoryIds.Add(SelectedCategory.Id);
                AddChildCategoryIds(SelectedCategory, selectedCategoryIds);
            }

            var filtered = _allProducts.Where(product =>
            {
                var matchesText = string.IsNullOrWhiteSpace(query) ||
                    product.Name.Contains(query, StringComparison.OrdinalIgnoreCase);

                var matchesCategory = SelectedCategory == null || SelectedCategory.Id < 0 || selectedCategoryIds.Contains(product.CategoryId);

                return matchesText && matchesCategory;
            });

            foreach (var product in filtered.OrderBy(p => p.Name))
            {
                Products.Add(product);
            }

            if (SelectedProduct != null && !Products.Contains(SelectedProduct))
            {
                SelectedProduct = null;
            }
        }

        private void AddProduct()
        {
            var formVm = new ProductFormViewModel(
                _productService,
                _categoryService,
                _taxRateService,
                this);

            _dialogService.ShowDialog<ProductFormView>(formVm);
        }

        private void EditProduct()
        {
            if (SelectedProduct == null)
                return;

            var formVm = new ProductFormViewModel(
                _productService,
                _categoryService,
                _taxRateService,
                this,
                SelectedProduct);

            _dialogService.ShowDialog<ProductFormView>(formVm);
        }

        private async void DeleteProduct()
        {
            if (SelectedProduct == null)
                return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete \"{SelectedProduct.Name}\"? This action cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _productService.DeleteProductAsync(SelectedProduct.Id);
                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to delete product: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        public async Task RefreshDataAsync()
        {
            CategorySearchText = string.Empty;
            ProductSearchText = string.Empty;
            SelectedCategory = null;
            SelectedProduct = null;
            await LoadDataAsync();
        }

        private bool CanEditProduct() => SelectedProduct != null;

        private bool CanDeleteProduct() => SelectedProduct != null;
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