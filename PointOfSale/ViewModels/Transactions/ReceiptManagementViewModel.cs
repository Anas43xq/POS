using BLL.Interfaces;
using Contracts.Transactions;
using POS.Contracts.Receipts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;
using Microsoft.Win32;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public class ReceiptManagementViewModel : BaseViewModel
    {
        private readonly ITransactionService _transactionService;
        private readonly IReceiptDisplayService _receiptDisplayService;
        private readonly IPurchaseReceiptService _purchaseReceiptService;
        private readonly ISupplierService _supplierService;
        private readonly ExcelReportExporter _excelExporter;

        private string _searchText = string.Empty;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private string _categoryFilter = string.Empty;
        private SupplierDto? _selectedSupplier;
        private string _selectedReceiptTypeFilter = "All";
        private string _statusMessage = string.Empty;
        private bool _isFormVisible;
        private bool _isFormEditable = true;
        private string _formTitle = "Add Purchase Receipt";

        private string _invoiceNumber = string.Empty;
        private string _category = string.Empty;
        private string _description = string.Empty;
        private string _notes = string.Empty;
        private string _imagePath = string.Empty;
        private DateTime _invoiceDate = DateTime.Today;
        private decimal _subtotal;
        private decimal _vatRate;
        private decimal _vatAmount;
        private decimal _grandTotal;
        private int? _editingReceiptId;
        private byte _activeReceiptTypeId = 1;

        public ReceiptManagementViewModel(
            ITransactionService transactionService,
            IReceiptDisplayService receiptDisplayService,
            IPurchaseReceiptService purchaseReceiptService,
            ISupplierService supplierService,
            ExcelReportExporter excelExporter)
        {
            _transactionService = transactionService;
            _receiptDisplayService = receiptDisplayService;
            _purchaseReceiptService = purchaseReceiptService;
            _supplierService = supplierService;
            _excelExporter = excelExporter;

            SalesReceipts = new ObservableCollection<TransactionListItemDto>();
            VatReceipts = new ObservableCollection<PurchaseReceiptDto>();
            NonVatReceipts = new ObservableCollection<PurchaseReceiptDto>();
            Suppliers = new ObservableCollection<SupplierDto>();

            RefreshCommand = new RelayCommand(_ => { _ = LoadAllAsync(); });
            ApplyFiltersCommand = new RelayCommand(_ => { _ = LoadAllAsync(); });
            ResetFiltersCommand = new RelayCommand(_ => ResetFilters());
            OpenSalesReceiptCommand = new RelayCommand(_ => OpenSelectedSalesReceipt());
            AddVatReceiptCommand = new RelayCommand(_ => ShowReceiptForm(1, false));
            EditVatReceiptCommand = new RelayCommand(_ => EditSelectedReceipt(1));
            DeleteVatReceiptCommand = new RelayCommand(_ => { _ = DeleteSelectedReceipt(1); });
            ViewVatReceiptCommand = new RelayCommand(_ => ViewSelectedReceipt(1));
            AddNonVatReceiptCommand = new RelayCommand(_ => ShowReceiptForm(2, false));
            EditNonVatReceiptCommand = new RelayCommand(_ => EditSelectedReceipt(2));
            DeleteNonVatReceiptCommand = new RelayCommand(_ => { _ = DeleteSelectedReceipt(2); });
            ViewNonVatReceiptCommand = new RelayCommand(_ => ViewSelectedReceipt(2));
            SaveReceiptCommand = new RelayCommand(_ => { _ = SaveReceiptAsync(); });
            CancelReceiptCommand = new RelayCommand(_ => CancelReceiptForm());
            ExportVatPurchasesCommand = new RelayCommand(_ => { _ = ExportVatPurchasesAsync(); });
            ExportNonVatPurchasesCommand = new RelayCommand(_ => { _ = ExportNonVatPurchasesAsync(); });

            _ = LoadAllAsync();
        }

        public ObservableCollection<TransactionListItemDto> SalesReceipts { get; }
        public ObservableCollection<PurchaseReceiptDto> VatReceipts { get; }
        public ObservableCollection<PurchaseReceiptDto> NonVatReceipts { get; }
        public ObservableCollection<SupplierDto> Suppliers { get; }

        public ICommand RefreshCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand OpenSalesReceiptCommand { get; }
        public ICommand AddVatReceiptCommand { get; }
        public ICommand EditVatReceiptCommand { get; }
        public ICommand DeleteVatReceiptCommand { get; }
        public ICommand ViewVatReceiptCommand { get; }
        public ICommand AddNonVatReceiptCommand { get; }
        public ICommand EditNonVatReceiptCommand { get; }
        public ICommand DeleteNonVatReceiptCommand { get; }
        public ICommand ViewNonVatReceiptCommand { get; }
        public ICommand SaveReceiptCommand { get; }
        public ICommand CancelReceiptCommand { get; }
        public ICommand ExportVatPurchasesCommand { get; }
        public ICommand ExportNonVatPurchasesCommand { get; }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? string.Empty; OnPropertyChanged(); }
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set { _dateFrom = value; OnPropertyChanged(); }
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set { _dateTo = value; OnPropertyChanged(); }
        }

        public string CategoryFilter
        {
            get => _categoryFilter;
            set { _categoryFilter = value ?? string.Empty; OnPropertyChanged(); }
        }

        public SupplierDto? SelectedSupplier
        {
            get => _selectedSupplier;
            set { _selectedSupplier = value; OnPropertyChanged(); }
        }

        public string SelectedReceiptTypeFilter
        {
            get => _selectedReceiptTypeFilter;
            set { _selectedReceiptTypeFilter = value ?? "All"; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ReceiptTypeFilterOptions { get; } = new() { "All", "Sales", "VAT", "Non-VAT" };

        public TransactionListItemDto? SelectedSalesReceipt { get; set; }
        public PurchaseReceiptDto? SelectedVatReceipt { get; set; }
        public PurchaseReceiptDto? SelectedNonVatReceipt { get; set; }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value ?? string.Empty; OnPropertyChanged(); }
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            private set { _isFormVisible = value; OnPropertyChanged(); }
        }

        public bool IsFormEditable
        {
            get => _isFormEditable;
            private set { _isFormEditable = value; OnPropertyChanged(); }
        }

        public string FormTitle
        {
            get => _formTitle;
            private set { _formTitle = value; OnPropertyChanged(); }
        }

        public bool IsVatReceiptMode => _activeReceiptTypeId == 1;
        public bool IsNonVatReceiptMode => _activeReceiptTypeId == 2;

        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set { _invoiceNumber = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string Category
        {
            get => _category;
            set { _category = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string Description
        {
            get => _description;
            set { _description = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string Notes
        {
            get => _notes;
            set { _notes = value ?? string.Empty; OnPropertyChanged(); }
        }

        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value ?? string.Empty; OnPropertyChanged(); }
        }

        public DateTime InvoiceDate
        {
            get => _invoiceDate;
            set { _invoiceDate = value; OnPropertyChanged(); }
        }

        public decimal Subtotal
        {
            get => _subtotal;
            set { _subtotal = value; OnPropertyChanged(); RecalculateTotals(); }
        }

        public decimal VatRate
        {
            get => _vatRate;
            set { _vatRate = value; OnPropertyChanged(); RecalculateTotals(); }
        }

        public decimal VatAmount
        {
            get => _vatAmount;
            private set { _vatAmount = value; OnPropertyChanged(); }
        }

        public decimal GrandTotal
        {
            get => _grandTotal;
            private set { _grandTotal = value; OnPropertyChanged(); }
        }

        private async Task LoadAllAsync()
        {
            StatusMessage = string.Empty;
            await LoadSuppliersAsync();
            await LoadSalesReceiptsAsync();
            await LoadPurchaseReceiptsAsync();
        }

        private async Task LoadSalesReceiptsAsync()
        {
            if (!ShouldShowReceiptType("Sales"))
            {
                SalesReceipts.Clear();
                return;
            }

            try
            {
                if (DateFrom is null && DateTo is null)
                {
                    // No dates selected, use default period (Today) instead of Custom
                    var defaultRequest = new GetTransactionsListRequest
                    {
                        PageNumber = 1,
                        PageSize = 100
                    };
                    var defaultResult = await _transactionService.GetTransactionsListAsync(defaultRequest);
                    SalesReceipts.Clear();
                    foreach (var item in defaultResult.Items.Where(MatchesFilters))
                    {
                        SalesReceipts.Add(item);
                    }
                }
                else if (DateFrom is not null && DateTo is not null)
                {
                    // Both dates selected, use Custom period
                    var request = new GetTransactionsListRequest
                    {
                        PeriodType = "Custom",
                        FromDate = DateFrom,
                        ToDate = DateTo,
                        PageNumber = 1,
                        PageSize = 100
                    };
                    var result = await _transactionService.GetTransactionsListAsync(request);
                    SalesReceipts.Clear();
                    foreach (var item in result.Items.Where(MatchesFilters))
                    {
                        SalesReceipts.Add(item);
                    }
                }
                else
                {
                    // One date is null but not both - request both dates to use Custom period
                    StatusMessage = "Please select both From and To dates, or clear both to use default period.";
                    SalesReceipts.Clear();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task LoadPurchaseReceiptsAsync()
        {
            if (!ShouldShowReceiptType("VAT") && !ShouldShowReceiptType("Non-VAT"))
            {
                VatReceipts.Clear();
                NonVatReceipts.Clear();
                return;
            }

            try
            {
                if (ShouldShowReceiptType("VAT"))
                {
                    var request = BuildPurchaseReceiptSearchRequest(1);
                    var vatResult = await _purchaseReceiptService.GetAllAsync(request);
                    VatReceipts.Clear();
                    if (vatResult.IsSuccess && vatResult.Value is not null)
                    {
                        foreach (var receipt in vatResult.Value.Where(MatchesFilters))
                        {
                            VatReceipts.Add(receipt);
                        }
                    }
                }
                else
                {
                    VatReceipts.Clear();
                }

                if (ShouldShowReceiptType("Non-VAT"))
                {
                    var request = BuildPurchaseReceiptSearchRequest(2);
                    var nonVatResult = await _purchaseReceiptService.GetAllAsync(request);
                    NonVatReceipts.Clear();
                    if (nonVatResult.IsSuccess && nonVatResult.Value is not null)
                    {
                        foreach (var receipt in nonVatResult.Value.Where(MatchesFilters))
                        {
                            NonVatReceipts.Add(receipt);
                        }
                    }
                }
                else
                {
                    NonVatReceipts.Clear();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task LoadSuppliersAsync()
        {
            try
            {
                var result = await _supplierService.GetAllAsync();
                Suppliers.Clear();
                if (result.IsSuccess && result.Value is not null)
                {
                    foreach (var supplier in result.Value)
                    {
                        Suppliers.Add(supplier);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void ResetFilters()
        {
            SearchText = string.Empty;
            DateFrom = null;
            DateTo = null;
            CategoryFilter = string.Empty;
            SelectedSupplier = null;
            SelectedReceiptTypeFilter = "All";
            _ = LoadAllAsync();
        }

        private void OpenSelectedSalesReceipt()
        {
            if (SelectedSalesReceipt is null)
            {
                StatusMessage = "Select a sales receipt to open.";
                return;
            }

            _receiptDisplayService.ShowReceipt(SelectedSalesReceipt.TransactionId);
        }

        private void ShowReceiptForm(byte receiptTypeId, bool isEditing)
        {
            _activeReceiptTypeId = receiptTypeId;
            OnPropertyChanged(nameof(IsVatReceiptMode));
            OnPropertyChanged(nameof(IsNonVatReceiptMode));
            IsFormVisible = true;
            IsFormEditable = true;
            FormTitle = isEditing ? $"Edit {(receiptTypeId == 1 ? "VAT" : "Non-VAT")} Purchase Receipt" : $"Add {(receiptTypeId == 1 ? "VAT" : "Non-VAT")} Purchase Receipt";
            StatusMessage = string.Empty;
            _editingReceiptId = null;

            if (!isEditing)
            {
                PopulateForm(null, receiptTypeId);
                RecalculateTotals();
                return;
            }

            var selectedReceipt = receiptTypeId == 1 ? SelectedVatReceipt : SelectedNonVatReceipt;
            if (selectedReceipt is null)
            {
                StatusMessage = "Select a receipt to edit.";
                IsFormVisible = false;
                return;
            }

            PopulateForm(selectedReceipt, receiptTypeId);
            _editingReceiptId = selectedReceipt.ReceiptId;
            RecalculateTotals();
        }

        private void EditSelectedReceipt(byte receiptTypeId)
        {
            ShowReceiptForm(receiptTypeId, true);
        }

        private void ViewSelectedReceipt(byte receiptTypeId)
        {
            _activeReceiptTypeId = receiptTypeId;
            var selectedReceipt = receiptTypeId == 1 ? SelectedVatReceipt : SelectedNonVatReceipt;
            if (selectedReceipt is null)
            {
                StatusMessage = "Select a receipt to view.";
                return;
            }

            PopulateForm(selectedReceipt, receiptTypeId);
            IsFormVisible = true;
            IsFormEditable = false;
            FormTitle = $"View {(receiptTypeId == 1 ? "VAT" : "Non-VAT")} Purchase Receipt";
            StatusMessage = string.Empty;
            RecalculateTotals();
        }

        private void PopulateForm(PurchaseReceiptDto? receipt, byte receiptTypeId)
        {
            if (receipt is null)
            {
                InvoiceNumber = string.Empty;
                Category = string.Empty;
                Description = string.Empty;
                Notes = string.Empty;
                ImagePath = string.Empty;
                InvoiceDate = DateTime.Today;
                Subtotal = 0m;
                VatRate = 0m;
                VatAmount = 0m;
                GrandTotal = 0m;
                SelectedSupplier = null;
                return;
            }

            InvoiceNumber = receipt.InvoiceNumber;
            Category = receipt.Category;
            Description = receipt.Description ?? string.Empty;
            Notes = receipt.Notes ?? string.Empty;
            ImagePath = receipt.ImagePath ?? string.Empty;
            InvoiceDate = receipt.InvoiceDate;
            Subtotal = receipt.Subtotal;
            VatRate = receipt.VatRate;
            VatAmount = receipt.VatAmount;
            GrandTotal = receipt.GrandTotal;
            SelectedSupplier = receipt.SupplierId is null ? null : Suppliers.FirstOrDefault(s => s.SupplierId == receipt.SupplierId);
        }

        private async Task SaveReceiptAsync()
        {
            if (!ValidateForm())
                return;

            try
            {
                var vatRate = IsVatReceiptMode ? VatRate : 0m;
                var vatAmount = IsVatReceiptMode ? VatAmount : 0m;
                var grandTotal = IsVatReceiptMode ? GrandTotal : Subtotal;

                if (_editingReceiptId.HasValue)
                {
                    var request = new UpdatePurchaseReceiptRequest
                    {
                        ReceiptId = _editingReceiptId.Value,
                        ReceiptTypeId = _activeReceiptTypeId,
                        SupplierId = SelectedSupplier?.SupplierId,
                        InvoiceNumber = InvoiceNumber,
                        InvoiceDate = InvoiceDate,
                        Category = Category,
                        Description = Description,
                        Subtotal = Subtotal,
                        VatRate = vatRate,
                        VatAmount = vatAmount,
                        GrandTotal = grandTotal,
                        Notes = Notes,
                        ImagePath = ImagePath
                    };

                    var result = await _purchaseReceiptService.UpdateAsync(_editingReceiptId.Value, request);
                    if (!result.IsSuccess)
                    {
                        StatusMessage = result.Error ?? "Unable to update receipt.";
                        return;
                    }
                }
                else
                {
                    var request = new CreatePurchaseReceiptRequest
                    {
                        ReceiptTypeId = _activeReceiptTypeId,
                        SupplierId = SelectedSupplier?.SupplierId,
                        InvoiceNumber = InvoiceNumber,
                        InvoiceDate = InvoiceDate,
                        Category = Category,
                        Description = Description,
                        Subtotal = Subtotal,
                        VatRate = VatRate,
                        VatAmount = VatAmount,
                        GrandTotal = GrandTotal,
                        Notes = Notes,
                        ImagePath = ImagePath,
                        CreatedBy = 1
                    };

                    var result = await _purchaseReceiptService.CreateAsync(request);
                    if (!result.IsSuccess)
                    {
                        StatusMessage = result.Error ?? "Unable to create receipt.";
                        return;
                    }
                }

                StatusMessage = string.Empty;
                CancelReceiptForm();
                await LoadPurchaseReceiptsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(InvoiceNumber))
            {
                StatusMessage = "Receipt number is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Category))
            {
                StatusMessage = "Category is required.";
                return false;
            }

            if (Subtotal <= 0)
            {
                StatusMessage = "Amount must be greater than zero.";
                return false;
            }

            if (IsVatReceiptMode && VatRate < 0)
            {
                StatusMessage = "VAT rate cannot be negative.";
                return false;
            }

            if (IsVatReceiptMode && GrandTotal < Subtotal)
            {
                StatusMessage = "Grand total must be greater than or equal to subtotal.";
                return false;
            }

            StatusMessage = string.Empty;
            return true;
        }

        private async Task DeleteSelectedReceipt(byte receiptTypeId)
        {
            var selectedReceipt = receiptTypeId == 1 ? SelectedVatReceipt : SelectedNonVatReceipt;
            if (selectedReceipt is null)
            {
                StatusMessage = "Select a receipt to delete.";
                return;
            }

            var confirm = MessageBox.Show($"Delete receipt {selectedReceipt.InvoiceNumber}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                var result = await _purchaseReceiptService.DeleteAsync(selectedReceipt.ReceiptId);
                if (!result.IsSuccess)
                {
                    StatusMessage = result.Error ?? "Unable to delete receipt.";
                    return;
                }

                StatusMessage = string.Empty;
                await LoadPurchaseReceiptsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void CancelReceiptForm()
        {
            IsFormVisible = false;
            IsFormEditable = true;
            FormTitle = "Add Purchase Receipt";
            StatusMessage = string.Empty;
            _editingReceiptId = null;
            OnPropertyChanged(nameof(IsVatReceiptMode));
            OnPropertyChanged(nameof(IsNonVatReceiptMode));
        }

        private void RecalculateTotals()
        {
            VatAmount = IsVatReceiptMode ? Subtotal * VatRate / 100m : 0m;
            GrandTotal = IsVatReceiptMode ? Subtotal + VatAmount : Subtotal;
        }

        private async Task ExportVatPurchasesAsync()
        {
            if (VatReceipts.Count == 0)
            {
                StatusMessage = "No VAT purchase receipts match the current filters.";
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"VAT_Purchase_Register_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            var request = new ExcelReportRequest
            {
                ReportType = ReportType.VatPurchaseRegister,
                Title = "UAE PURCHASE VAT REGISTER",
                FromDate = DateFrom ?? DateTime.Today,
                ToDate = DateTo ?? DateTime.Today,
                Data = VatReceipts.ToList()
            };

            var bytes = _excelExporter.Export(request);
            await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
            StatusMessage = string.Empty;
        }

        private async Task ExportNonVatPurchasesAsync()
        {
            if (NonVatReceipts.Count == 0)
            {
                StatusMessage = "No non-VAT purchase receipts match the current filters.";
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Business_Expense_Register_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            var request = new ExcelReportRequest
            {
                ReportType = ReportType.NonVatPurchaseRegister,
                Title = "BUSINESS EXPENSE REGISTER",
                FromDate = DateFrom ?? DateTime.Today,
                ToDate = DateTo ?? DateTime.Today,
                Data = NonVatReceipts.ToList()
            };

            var bytes = _excelExporter.Export(request);
            await File.WriteAllBytesAsync(saveDialog.FileName, bytes);
            StatusMessage = string.Empty;
        }

        private bool ShouldShowReceiptType(string receiptType)
        {
            return SelectedReceiptTypeFilter is "All" or "Sales" or "VAT" or "Non-VAT"
                && (SelectedReceiptTypeFilter == "All" || string.Equals(SelectedReceiptTypeFilter, receiptType, StringComparison.OrdinalIgnoreCase));
        }

        private PurchaseReceiptSearchRequest BuildPurchaseReceiptSearchRequest(byte receiptTypeId)
        {
            return new PurchaseReceiptSearchRequest
            {
                SearchText = SearchText,
                FromDate = DateFrom,
                ToDate = DateTo,
                SupplierId = SelectedSupplier?.SupplierId,
                Category = string.IsNullOrWhiteSpace(CategoryFilter) ? null : CategoryFilter.Trim(),
                ReceiptTypeId = receiptTypeId
            };
        }

        private bool MatchesFilters(TransactionListItemDto transaction)
        {
            return MatchesFilters(
                transaction.ReceiptNumber,
                transaction.TransactionDate,
                category: null,
                supplierName: null,
                searchText: transaction.ReceiptNumber + " " + transaction.PaymentMethod + " " + (transaction.Status ?? string.Empty) + " " + (transaction.Notes ?? string.Empty));
        }

        private bool MatchesFilters(PurchaseReceiptDto receipt)
        {
            return MatchesFilters(
                receipt.InvoiceNumber,
                receipt.InvoiceDate,
                receipt.Category,
                receipt.SupplierName,
                receipt.InvoiceNumber + " " + receipt.Category + " " + receipt.Description + " " + receipt.Notes + " " + receipt.SupplierName);
        }

        private bool MatchesFilters(string? receiptNumber, DateTime? receiptDate, string? category, string? supplierName, string? searchText)
        {
            var normalizedSearch = SearchText?.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                var search = searchText ?? string.Empty;
                if (!search.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (DateFrom.HasValue && receiptDate is not null && receiptDate.Value.Date < DateFrom.Value.Date)
            {
                return false;
            }

            if (DateTo.HasValue && receiptDate is not null && receiptDate.Value.Date > DateTo.Value.Date)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                if (string.IsNullOrWhiteSpace(category) || !category.Contains(CategoryFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (SelectedSupplier is not null)
            {
                if (string.IsNullOrWhiteSpace(supplierName) || !string.Equals(supplierName, SelectedSupplier.CompanyName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
