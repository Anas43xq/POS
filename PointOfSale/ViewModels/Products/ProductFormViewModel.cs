using BLL.Interfaces;
using BLL.Models;
using BLL.DTOs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UI.Commands;

namespace UI.ViewModels
{
    public class ProductFormViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ITaxRateService _taxRateService;
        private readonly ILocalizationService _localization;
        private readonly ProductManagementViewModel _parentVm;

        private ProductRowViewModel? _existingProduct;
        private string _productName = string.Empty;
        private decimal _price;
        private CategoryNodeViewModel? _selectedCategory;
        private TaxRateDto? _selectedTax;
        private bool _isActive = true;
        private bool _hasAdditionalTax;
        private string _additionalTaxAmount = string.Empty;
        private string _description = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _productExistsWarning;

        public string FormTitle => _existingProduct == null ? "Add Product" : "Edit Product";
        public string SaveButtonLabel => _existingProduct == null ? "Save Product" : "Update Product";

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged();
                CheckProductExists();
            }
        }

        public decimal Price
        {
            get => _price;
            set
            {
                _price = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<CategoryNodeViewModel> CategoryOptions { get; } = new();

        public CategoryNodeViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                CheckProductExists();
            }
        }

        public ObservableCollection<TaxRateDto> TaxOptions { get; } = new();

        public TaxRateDto? SelectedTax
        {
            get => _selectedTax;
            set
            {
                _selectedTax = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public bool HasAdditionalTax
        {
            get => _hasAdditionalTax;
            set
            {
                _hasAdditionalTax = value;
                OnPropertyChanged();
            }
        }

        public string AdditionalTaxAmount
        {
            get => _additionalTaxAmount;
            set
            {
                _additionalTaxAmount = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

        public bool ProductExistsWarning
        {
            get => _productExistsWarning;
            set
            {
                _productExistsWarning = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action? DialogClosed;

        public ProductFormViewModel(
            IProductService productService,
            ICategoryService categoryService,
            ITaxRateService taxRateService,
            ILocalizationService localization,
            ProductManagementViewModel parentVm,
            ProductRowViewModel? existingProduct = null)
        {
            _productService = productService;
            _categoryService = categoryService;
            _taxRateService = taxRateService;
            _localization = localization;
            _parentVm = parentVm;
            _existingProduct = existingProduct;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            CancelCommand = new RelayCommand(Cancel);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            // Load categories (localized)
            var languageCode = _localization.CurrentLanguage.FilePrefix;
            var categoriesResult = await _categoryService.GetAllCategoriesWithChildrenAsync(languageCode);
            if (categoriesResult.IsSuccess && categoriesResult.Value != null)
            {
                foreach (var cat in categoriesResult.Value)
                {
                    AddCategoryToOptions(cat);
                }
            }

            // Load tax rates
            var taxResult = await _taxRateService.GetAllTaxRatesAsync();
            if (taxResult.IsSuccess && taxResult.Value != null)
            {
                foreach (var tax in taxResult.Value)
                {
                    TaxOptions.Add(tax);
                }
            }

            // If editing, populate fields
            if (_existingProduct != null)
            {
                _productName = _existingProduct.Name;
                _price = _existingProduct.Price;
                _isActive = _existingProduct.Status == "Active";
                OnPropertyChanged(nameof(ProductName));
                OnPropertyChanged(nameof(Price));
                OnPropertyChanged(nameof(IsActive));
                OnPropertyChanged(nameof(FormTitle));
                OnPropertyChanged(nameof(SaveButtonLabel));

                // Try to pre-select the category
                SelectedCategory = CategoryOptions.FirstOrDefault(c => c.Id == _existingProduct.CategoryId);

                // Pre-select the tax rate
                SelectedTax = TaxOptions.FirstOrDefault(t => t.TaxRateId == _existingProduct.TaxRateId);
            }
        }

        private void AddCategoryToOptions(CategoryDto category)
        {
            var node = new CategoryNodeViewModel
            {
                Id = category.CategoryId,
                Name = category.Name,
                ParentId = category.ParentCategoryId
            };
            CategoryOptions.Add(node);

            if (category.ChildCategories != null)
            {
                foreach (var child in category.ChildCategories)
                {
                    AddCategoryToOptions(child);
                }
            }
        }

        private void CheckProductExists()
        {
            if (string.IsNullOrWhiteSpace(ProductName) || SelectedCategory == null)
            {
                ProductExistsWarning = false;
                return;
            }

            bool exists = _parentVm.Products.Any(p =>
                p.Name.Equals(ProductName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                p.CategoryId == SelectedCategory.Id &&
                (_existingProduct == null || p.Id != _existingProduct.Id));

            ProductExistsWarning = exists;
        }

        private async Task SaveAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(ProductName))
            {
                ErrorMessage = "Product name is required.";
                return;
            }

            if (SelectedCategory == null)
            {
                ErrorMessage = "Please select a category.";
                return;
            }

            if (SelectedTax == null)
            {
                ErrorMessage = "Please select a tax rate.";
                return;
            }

            if (Price < 0)
            {
                ErrorMessage = "Price cannot be negative.";
                return;
            }

            if (ProductExistsWarning)
            {
                ErrorMessage = "A product with this name already exists in the selected category.";
                return;
            }

            try
            {
                if (_existingProduct == null)
                {
                    // Add new product
                    var product = new ProductWriteDto
                    {
                        Name = ProductName.Trim(),
                        CategoryId = SelectedCategory.Id,
                        UnitPrice = Price,
                        TaxRateId = SelectedTax.TaxRateId,
                        IsActive = IsActive,
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim()
                    };

                    await _productService.AddProductAsync(product);
                }
                else
                {
                    // Update existing product
                    var existing = await _productService.GetProductByIdAsync(_existingProduct.Id);
                    if (existing != null)
                    {
                        var updated = new ProductWriteDto
                        {
                            ProductId = _existingProduct.Id,
                            Name = ProductName.Trim(),
                            CategoryId = SelectedCategory.Id,
                            UnitPrice = Price,
                            TaxRateId = SelectedTax.TaxRateId,
                            IsActive = IsActive,
                            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim()
                        };

                        await _productService.UpdateProductAsync(updated);
                    }
                }

                // Refresh parent list
                await _parentVm.RefreshDataAsync();
                CloseDialog();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save product: {ex.Message}";
            }
        }

        private void Cancel(object? obj)
        {
            CloseDialog();
        }

        private void CloseDialog()
        {
            DialogClosed?.Invoke();
        }
    }
}