using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BLL.Interfaces;
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
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync, CanDeleteProduct);
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
