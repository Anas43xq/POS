using BLL.Interfaces;
using DAL.Entities;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Commands;
using UI.Services;

namespace UI.ViewModels
{
    public enum ReportFilterMode
    {
        Today,
        Week,
        Month,
        Period
    }

    public class ReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IProductService _productService;
        private readonly ExcelReportExporter _excelExporter;

        private ReportFilterMode _selectedPeriodType = ReportFilterMode.Today;
        private DateTime? _fromDate;
        private DateTime? _toDate;
        private object? _selectedProduct;
        private bool _isSalesMode = true;
        private bool _isProductMode;
        private bool _isPeriodFilterVisible;
        private bool _isLoading;
        private string _totalOrders = "0";
        private string _totalSales = "AED 0.00";
        private string _cashTotal = "AED 0.00";
        private string _cardTotal = "AED 0.00";
        private string _totalQuantitySold = "0";
        private string _totalRevenue = "AED 0.00";
        private string _averagePrice = "AED 0.00";

        public ReportViewModel(IReportService reportService, IProductService productService, ExcelReportExporter excelExporter)
        {
            _reportService = reportService;
            _productService = productService;
            _excelExporter = excelExporter;

            ReportCommand = new RelayCommand<string>(OnReportAction, _ => !_isLoading);

            TransactionReports = new ObservableCollection<TransactionReportEntity>();
            ProductReports = new ObservableCollection<ProductReportEntity>();
            Products = new ObservableCollection<object>();

            IsSalesMode = true;
            _ = LoadProductsAsync();
        }

        private async Task LoadProductsAsync()
        {
            var result = await _productService.GetAllProductsAsync();
            if (result.IsSuccess && result.Value != null)
            {
                Products.Clear();
                foreach (var p in result.Value)
                {
                    Products.Add(new { p.ProductId, p.Name });
                }
            }
        }

        // ================================================================
        // REPORT COMMAND (single command for all actions)
        // ================================================================
        public ICommand ReportCommand { get; }

        private async void OnReportAction(string? action)
        {
            switch (action)
            {
                case "GenerateAndExport":
                    await LoadReportAsync();
                    ExportToExcel();
                    break;
            }
        }

        // ================================================================
        // REPORT LOADING
        // ================================================================
        private async Task LoadReportAsync()
        {
            if (_isLoading) return; // Guard: prevent double-click

            IsLoading = true;
            try
            {
                string periodType = _selectedPeriodType switch
                {
                    ReportFilterMode.Today => "Today",
                    ReportFilterMode.Week => "Week",
                    ReportFilterMode.Month => "Month",
                    ReportFilterMode.Period => "Custom",
                    _ => "Today"
                };

                if (IsSalesMode)
                {
                    var data = await _reportService.GetTransactionReportAsync(
                        periodType, _fromDate, _toDate);

                    TransactionReports.Clear();
                    foreach (var item in data)
                        TransactionReports.Add(item);

                    // Calculate KPIs
                    TotalOrders = TransactionReports.Count.ToString();
                    TotalSales = TransactionReports.Sum(t => t.GrandTotal).ToString("AED 0.00");
                    CashTotal = TransactionReports.Where(t => t.PaymentMethod == "Cash").Sum(t => t.GrandTotal).ToString("AED 0.00");
                    CardTotal = TransactionReports.Where(t => t.PaymentMethod == "Card").Sum(t => t.GrandTotal).ToString("AED 0.00");
                }
                else
                {
                    int? productId = _selectedProduct != null
                        ? (int?)_selectedProduct.GetType().GetProperty("ProductId")?.GetValue(_selectedProduct)
                        : null;

                    var data = await _reportService.GetProductSalesReportAsync(
                        periodType, _fromDate, _toDate, productId);

                    ProductReports.Clear();
                    foreach (var item in data)
                        ProductReports.Add(item);

                    // Calculate KPIs
                    int totalQty = ProductReports.Sum(p => p.Quantity);
                    decimal totalRev = ProductReports.Sum(p => p.LineTotal);
                    TotalQuantitySold = totalQty.ToString();
                    TotalRevenue = totalRev.ToString("AED 0.00");
                    AveragePrice = totalQty > 0 ? (totalRev / totalQty).ToString("AED 0.00") : "AED 0.00";
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ================================================================
        // EXPORT TO EXCEL
        // ================================================================
        private void ExportToExcel()
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            DateTime from = _fromDate ?? DateTime.Today;
            DateTime to = _toDate ?? DateTime.Today;

            if (IsSalesMode)
            {
                var request = new ExcelReportRequest
                {
                    ReportType = ReportType.Transactions,
                    Title = "SALES SUMMARY REPORT",
                    FromDate = from,
                    ToDate = to,
                    Summary = new TransactionsReportSummary
                    {
                        TotalOrders = _totalOrders,
                        TotalSales = _totalSales,
                        CashTotal = _cashTotal,
                        CardTotal = _cardTotal
                    },
                    Data = TransactionReports.ToList()
                };

                byte[] bytes = _excelExporter.Export(request);
                File.WriteAllBytes(saveDialog.FileName, bytes);
            }
            else
            {
                var request = new ExcelReportRequest
                {
                    ReportType = ReportType.Product,
                    Title = "PRODUCT SALES SUMMARY REPORT",
                    FromDate = from,
                    ToDate = to,
                    Summary = new ProductReportSummary
                    {
                        TotalQuantitySold = _totalQuantitySold,
                        TotalRevenue = _totalRevenue,
                        AveragePrice = _averagePrice
                    },
                    Data = ProductReports.ToList()
                };

                byte[] bytes = _excelExporter.Export(request);
                File.WriteAllBytes(saveDialog.FileName, bytes);
            }
        }

        // ================================================================
        // LOADING STATE (prevents double-click)
        // ================================================================
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                ((RelayCommand<string>)ReportCommand).RaiseCanExecuteChanged();
            }
        }

        // ================================================================
        // FILTER MODE
        // ================================================================
        public ReportFilterMode SelectedPeriodType
        {
            get => _selectedPeriodType;
            set
            {
                if (_selectedPeriodType == value) return;
                _selectedPeriodType = value;
                OnPropertyChanged();
            }
        }

        public bool IsPeriodFilterVisible
        {
            get => _isPeriodFilterVisible;
            set
            {
                _isPeriodFilterVisible = value;
                OnPropertyChanged();
            }
        }

        public DateTime? FromDate
        {
            get => _fromDate;
            set
            {
                _fromDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime? ToDate
        {
            get => _toDate;
            set
            {
                _toDate = value;
                OnPropertyChanged();
            }
        }

        public object? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged();
            }
        }

        // ================================================================
        // FILTER COMMANDS
        // ================================================================
        public ICommand FilterTodayCommand =>
            new RelayCommand(async _ => { SelectedPeriodType = ReportFilterMode.Today; IsPeriodFilterVisible = false; await LoadReportAsync(); }, _ => !_isLoading);
        public ICommand FilterThisWeekCommand =>
            new RelayCommand(async _ => { SelectedPeriodType = ReportFilterMode.Week; IsPeriodFilterVisible = false; await LoadReportAsync(); }, _ => !_isLoading);
        public ICommand FilterMonthCommand =>
            new RelayCommand(async _ => { SelectedPeriodType = ReportFilterMode.Month; IsPeriodFilterVisible = false; await LoadReportAsync(); }, _ => !_isLoading);
        public ICommand ShowPeriodFilterCommand =>
            new RelayCommand(_ => { SelectedPeriodType = ReportFilterMode.Period; IsPeriodFilterVisible = true; });
        public ICommand ApplyPeriodFilterCommand =>
            new RelayCommand(async _ => { SelectedPeriodType = ReportFilterMode.Period; IsPeriodFilterVisible = true; await LoadReportAsync(); }, _ => !_isLoading);

        // ================================================================
        // MODE PROPERTIES
        // ================================================================
        public bool IsSalesMode
        {
            get => _isSalesMode;
            set
            {
                _isSalesMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProductMode));
                OnPropertyChanged(nameof(SalesSummaryVisibility));
                OnPropertyChanged(nameof(ProductSummaryVisibility));
                OnPropertyChanged(nameof(SalesGridVisibility));
                OnPropertyChanged(nameof(ProductGridVisibility));
            }
        }

        public bool IsProductMode
        {
            get => _isProductMode;
            set
            {
                _isProductMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSalesMode));
                OnPropertyChanged(nameof(SalesSummaryVisibility));
                OnPropertyChanged(nameof(ProductSummaryVisibility));
                OnPropertyChanged(nameof(SalesGridVisibility));
                OnPropertyChanged(nameof(ProductGridVisibility));
            }
        }

        // ================================================================
        // SALES SUMMARY
        // ================================================================
        public string TotalOrders
        {
            get => _totalOrders;
            set { _totalOrders = value; OnPropertyChanged(); }
        }

        public string TotalSales
        {
            get => _totalSales;
            set { _totalSales = value; OnPropertyChanged(); }
        }

        public string CashTotal
        {
            get => _cashTotal;
            set { _cashTotal = value; OnPropertyChanged(); }
        }

        public string CardTotal
        {
            get => _cardTotal;
            set { _cardTotal = value; OnPropertyChanged(); }
        }

        // ================================================================
        // PRODUCT SUMMARY
        // ================================================================
        public string TotalQuantitySold
        {
            get => _totalQuantitySold;
            set { _totalQuantitySold = value; OnPropertyChanged(); }
        }

        public string TotalRevenue
        {
            get => _totalRevenue;
            set { _totalRevenue = value; OnPropertyChanged(); }
        }

        public string AveragePrice
        {
            get => _averagePrice;
            set { _averagePrice = value; OnPropertyChanged(); }
        }

        // ================================================================
        // VISIBILITY (MODE TOGGLING)
        // ================================================================
        public System.Windows.Visibility SalesSummaryVisibility =>
            IsSalesMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility ProductSummaryVisibility =>
            IsProductMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility SalesGridVisibility =>
            IsSalesMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public System.Windows.Visibility ProductGridVisibility =>
            IsProductMode ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        // ================================================================
        // COLLECTIONS
        // ================================================================
        public ObservableCollection<TransactionReportEntity> TransactionReports { get; }
        public ObservableCollection<ProductReportEntity> ProductReports { get; }
        public ObservableCollection<object> Products { get; }
    }
}