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
using UI.Services;
using UI.Views;

namespace UI.ViewModels
{
    /// <summary>
    /// One Size/Price row on the Product form's variant editor.
    /// VariantId is 0 for a row that hasn't been saved yet.
    /// </summary>
    public class ProductVariantRowViewModel : BaseViewModel
    {
        public int VariantId { get; set; }

        private SizeDto? _selectedSize;
        public SizeDto? SelectedSize
        {
            get => _selectedSize;
            set { _selectedSize = value; OnPropertyChanged(); }
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set { _unitPrice = value; OnPropertyChanged(); }
        }

        public bool IsActive { get; set; } = true;
    }

    public class ProductFormViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ITaxRateService _taxRateService;
        private readonly ISizeService _sizeService;
        private readonly ILocalizationService _localization;
        private readonly IViewModelFactory _viewModelFactory;
        private readonly ProductManagementViewModel _parentVm;

        private ProductRowViewModel? _existingProduct;
        private string _productName = string.Empty;
        private CategoryNodeViewModel? _selectedCategory;
        private TaxRateDto? _selectedTax;
        private bool _isActive = true;
        private bool _hasAdditionalTax;
        private string _additionalTaxAmount = string.Empty;
        private string _description = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _productExistsWarning;
        private bool _isBusy;

        public string FormTitle => _existingProduct == null
            ? _localization.GetString("Manager.ProductForm.AddProduct")
            : _localization.GetString("Manager.ProductForm.EditProduct");
        public string SaveButtonLabel => _existingProduct == null
            ? _localization.GetString("Manager.ProductForm.SaveProduct")
            : _localization.GetString("Manager.ProductForm.UpdateProduct");

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The product's Size/Price rows. Every product — single or
        /// multi-size — is edited here; a single-size product simply has
        /// one row (its size defaulting to "Regular" when available).
        /// This is the only source of selling price.
        /// </summary>
        public ObservableCollection<ProductVariantRowViewModel> Variants { get; } = new();

        public ObservableCollection<SizeDto> SizeOptions { get; } = new();

        public ICommand AddVariantCommand { get; }
        public ICommand RemoveVariantCommand { get; }

        public ObservableCollection<CategoryNodeViewModel> CategoryOptions { get; } = new();

        public CategoryNodeViewModel? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (_isBusy == value) return;

                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotBusy));
                RaiseCommandStatesChanged();
            }
        }

        public bool IsNotBusy => !IsBusy;

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand TranslationsCommand { get; }

        public event Action? DialogClosed;

        public ProductFormViewModel(
            IProductService productService,
            ICategoryService categoryService,
            ITaxRateService taxRateService,
            ISizeService sizeService,
            ILocalizationService localization,
            IViewModelFactory viewModelFactory,
            ProductManagementViewModel parentVm,
            ProductRowViewModel? existingProduct = null)
        {
            _productService = productService;
            _categoryService = categoryService;
            _taxRateService = taxRateService;
            _sizeService = sizeService;
            _localization = localization;
            _viewModelFactory = viewModelFactory;
            _parentVm = parentVm;
            _existingProduct = existingProduct;

            SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy);
            CancelCommand = new RelayCommand(Cancel);
            TranslationsCommand = new RelayCommand(_ => OpenTranslations(), _ => _existingProduct != null);
            AddVariantCommand = new RelayCommand(_ => AddVariantRow(), _ => !IsBusy);
            RemoveVariantCommand = new RelayCommand(RemoveVariantRow, row => !IsBusy && Variants.Count > 1);

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            IsBusy = true;

            try
            {
                var languageCode = _localization.CurrentLanguage.FilePrefix;
                var categoriesResult = await _categoryService.GetAllCategoriesWithChildrenAsync(languageCode);
                if (categoriesResult.IsSuccess && categoriesResult.Value != null)
                {
                    foreach (var cat in categoriesResult.Value)
                    {
                        AddCategoryToOptions(cat);
                    }
                }

                var taxResult = await _taxRateService.GetAllTaxRatesAsync();
                if (taxResult.IsSuccess && taxResult.Value != null)
                {
                    foreach (var tax in taxResult.Value)
                    {
                        TaxOptions.Add(tax);
                    }
                }

                var sizeResult = await _sizeService.GetAllSizesAsync();
                if (sizeResult.IsSuccess && sizeResult.Value != null)
                {
                    foreach (var size in sizeResult.Value.Where(s => s.IsActive))
                    {
                        SizeOptions.Add(size);
                    }
                }

                if (_existingProduct != null)
                {
                    _productName = _existingProduct.Name;
                    _isActive = _existingProduct.Status == "Active";
                    OnPropertyChanged(nameof(ProductName));
                    OnPropertyChanged(nameof(IsActive));
                    OnPropertyChanged(nameof(FormTitle));
                    OnPropertyChanged(nameof(SaveButtonLabel));

                    SelectedCategory = CategoryOptions.FirstOrDefault(c => c.Id == _existingProduct.CategoryId);
                    SelectedTax = TaxOptions.FirstOrDefault(t => t.TaxRateId == _existingProduct.TaxRateId);

                    var full = await _productService.GetProductByIdAsync(_existingProduct.Id);
                    if (full != null)
                    {
                        Description = full.Description ?? string.Empty;

                        foreach (var v in full.Variants)
                        {
                            Variants.Add(new ProductVariantRowViewModel
                            {
                                VariantId = v.VariantId,
                                SelectedSize = SizeOptions.FirstOrDefault(s => s.SizeId == v.SizeId)
                                    ?? new SizeDto { SizeId = v.SizeId, Name = v.SizeName },
                                UnitPrice = v.UnitPrice,
                                IsActive = v.IsActive
                            });
                        }
                    }
                }

                if (Variants.Count == 0)
                    AddVariantRow();
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddVariantRow()
        {
            var regular = SizeOptions.FirstOrDefault(s => s.Name.Equals("Regular", StringComparison.OrdinalIgnoreCase));
            var alreadyUsed = Variants.Select(v => v.SelectedSize?.SizeId).ToHashSet();
            var defaultSize = SizeOptions.FirstOrDefault(s => !alreadyUsed.Contains(s.SizeId)) ?? regular;

            Variants.Add(new ProductVariantRowViewModel { SelectedSize = defaultSize });
            RaiseCommandStatesChanged();
        }

        private void RemoveVariantRow(object? parameter)
        {
            if (parameter is ProductVariantRowViewModel row && Variants.Count > 1)
            {
                Variants.Remove(row);
                RaiseCommandStatesChanged();
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

            bool exists = _parentVm.Products.Cast<ProductRowViewModel>().Any(p =>
                p.Name.Equals(ProductName.Trim(), StringComparison.OrdinalIgnoreCase) &&
                p.CategoryId == SelectedCategory.Id &&
                (_existingProduct == null || p.Id != _existingProduct.Id));

            ProductExistsWarning = exists;
        }

        private async Task SaveAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(ProductName))
                {
                    ErrorMessage = _localization.GetString("Manager.ProductForm.NameRequired");
                    return;
                }

                if (SelectedCategory == null)
                {
                    ErrorMessage = _localization.GetString("Manager.ProductForm.CategoryRequired");
                    return;
                }

                if (SelectedTax == null)
                {
                    ErrorMessage = _localization.GetString("Manager.ProductForm.TaxRequired");
                    return;
                }

                if (Variants.Count == 0)
                {
                    ErrorMessage = _localization.GetString("Manager.ProductForm.AtLeastOneSize");
                    return;
                }

                if (Variants.Any(v => v.SelectedSize == null))
                {
                    ErrorMessage = _localization.GetString("Manager.ProductForm.RowSizeRequired");
                    return;
                }

                if (Variants.Any(v => v.UnitPrice <= 0))
                {
                    ErrorMessage = _localization.GetString("Manager.ProductForm.PriceGreaterThanZero");
                    return;
                }

                if (Variants.Select(v => v.SelectedSize!.SizeId).Distinct().Count() != Variants.Count)
                {
                    ErrorMessage = _localization.GetString("Manager.ProductForm.DuplicateSize");
                    return;
                }

                CheckProductExists();

                if (ProductExistsWarning)
                {
                    ErrorMessage = _localization.GetString("Manager.ProductForm.DuplicateWarning");
                    return;
                }

                var variantDtos = Variants.Select(v => new ProductVariantWriteDto
                {
                    VariantId = v.VariantId,
                    SizeId = v.SelectedSize!.SizeId,
                    SizeName = v.SelectedSize.Name,
                    UnitPrice = v.UnitPrice,
                    IsActive = v.IsActive
                }).ToList();

                if (_existingProduct == null)
                {
                    var product = new ProductWriteDto
                    {
                        Name = ProductName.Trim(),
                        CategoryId = SelectedCategory.Id,
                        TaxRateId = SelectedTax.TaxRateId,
                        IsActive = IsActive,
                        Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                        Variants = variantDtos
                    };

                    await _productService.AddProductAsync(product);
                }
                else
                {
                    var existing = await _productService.GetProductByIdAsync(_existingProduct.Id);
                    if (existing != null)
                    {
                        var updated = new ProductWriteDto
                        {
                            ProductId = _existingProduct.Id,
                            Name = ProductName.Trim(),
                            CategoryId = SelectedCategory.Id,
                            TaxRateId = SelectedTax.TaxRateId,
                            IsActive = IsActive,
                            Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                            Variants = variantDtos
                        };

                        var warnings = await _productService.UpdateProductAsync(updated);
                        if (warnings.Count > 0)
                        {
                            MessageBox.Show(
                                string.Join(Environment.NewLine, warnings),
                                "Some sizes were deactivated instead of removed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }

                await _parentVm.RefreshDataAsync();
                CloseDialog();
            }
            catch (Exception ex)
            {
                ErrorMessage = _localization.GetString("Manager.ProductForm.SaveFailed", ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OpenTranslations()
        {
            if (_existingProduct == null) return;

            var vm = _viewModelFactory.Create<TranslationDialogViewModel>(
                TranslationDialogViewModel.EntityType.Product,
                _existingProduct.Id,
                _existingProduct.Name);

            var dialog = new Views.TranslationDialogView { DataContext = vm };
            var owner = Application.Current?.MainWindow;
            if (owner != null && !ReferenceEquals(owner, dialog))
                dialog.Owner = owner;
            vm.RequestClose = () => dialog.Close();
            dialog.ShowDialog();
        }

        private void Cancel(object? obj)
        {
            CloseDialog();
        }

        private void CloseDialog()
        {
            DialogClosed?.Invoke();
        }

        private void RaiseCommandStatesChanged()
        {
            if (SaveCommand is AsyncRelayCommand save) save.RaiseCanExecuteChanged();
            if (AddVariantCommand is RelayCommand add) add.RaiseCanExecuteChanged();
            if (RemoveVariantCommand is RelayCommand remove) remove.RaiseCanExecuteChanged();
        }
    }
}
