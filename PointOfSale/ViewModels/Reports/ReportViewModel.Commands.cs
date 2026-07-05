using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UI.Services;
using UI.Commands;

namespace UI.ViewModels
{
    public partial class ReportViewModel
    {
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

        private async Task LoadReportAsync()
        {
            if (_isLoading)
                return;

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
                    var data = await _reportService.GetTransactionReportAsync(periodType, _fromDate, _toDate);
                    TransactionReports.Clear();
                    foreach (var item in data)
                        TransactionReports.Add(item);

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

                    var data = await _reportService.GetProductSalesReportAsync(periodType, _fromDate, _toDate, productId);
                    ProductReports.Clear();
                    foreach (var item in data)
                        ProductReports.Add(item);

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
    }
}
