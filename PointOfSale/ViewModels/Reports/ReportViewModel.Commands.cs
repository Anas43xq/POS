using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BLL.DTOs;
using UI.Services;
using UI.Commands;

namespace UI.ViewModels
{
    public partial class ReportViewModel
    {
        public ICommand ReportCommand { get; }
        public ICommand OpenReceiptCommand { get; }

        private async void OnReportAction(string? action)
        {
            if (_isLoading)
                return;

            switch (action)
            {
                case "GenerateAndExport":
                    IsLoading = true;
                    try
                    {
                        await LoadReportDataAsync();
                        await ExportToExcelAsync();
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                    break;
            }
        }

        // Called by filter commands (Today/Week/Month/Period) — manages
        // its own IsLoading guard since those commands don't have an outer
        // IsLoading scope.
        private async Task LoadReportAsync()
        {
            if (_isLoading)
                return;

            IsLoading = true;
            try
            {
                await LoadReportDataAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Core data-loading logic without IsLoading management.
        // OnReportAction calls this directly after setting IsLoading=true
        // so that the loading state spans both Load + Export.
        private async Task LoadReportDataAsync()
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
            else if (IsSalesAnalysisMode)
            {
                var data = await _reportService.GetSalesAnalysisReportAsync(periodType, _fromDate, _toDate);
                SalesAnalysisReports.Clear();
                foreach (var item in data)
                    SalesAnalysisReports.Add(item);

                int categoriesSold = SalesAnalysisReports.Select(r => r.CategoryId).Distinct().Count();
                int productsSold = SalesAnalysisReports.Select(r => r.ProductId).Distinct().Count();
                int variantsSold = SalesAnalysisReports.Count;
                int totalQty = SalesAnalysisReports.Sum(r => r.Quantity);
                decimal totalSales = SalesAnalysisReports.Sum(r => r.LineTotal);

                CategoriesSold = categoriesSold.ToString();
                ProductsSold = productsSold.ToString();
                VariantsSold = variantsSold.ToString();
                SalesAnalysisTotalQuantitySold = totalQty.ToString();
                SalesAnalysisTotalSales = totalSales.ToString("AED 0.00");
            }
        }

        private async Task ExportToExcelAsync()
        {
            // WPF dialogs must run on the STA/UI thread — capture everything
            // needed before offloading CPU-bound work to the thread pool.
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (saveDialog.ShowDialog() != true)
                return;

            DateTime from = _fromDate ?? DateTime.Today;
            DateTime to = _toDate ?? DateTime.Today;

            try
            {
                if (IsSalesMode)
                {
                    // Build the request on the UI thread (reads VM properties)
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

                    string filePath = saveDialog.FileName;

                    // Offload CPU-bound export + write to background thread
                    byte[] bytes = await Task.Run(() => _excelExporter.Export(request));
                    await File.WriteAllBytesAsync(filePath, bytes);
                    StatusMessage = string.Empty;

                    ShowExportSuccess(filePath);
                }
                else if (IsSalesAnalysisMode)
                {
                    if (SalesAnalysisReports.Count == 0)
                    {
                        StatusMessage = "No sales analysis data to export for the selected period.";
                        return;
                    }

                    // Build the request on the UI thread (reads VM properties)
                    var request = new ExcelReportRequest
                    {
                        ReportType = ReportType.SalesAnalysis,
                        Title = "SALES ANALYSIS REPORT",
                        FromDate = from,
                        ToDate = to,
                        Summary = new SalesAnalysisReportSummary
                        {
                            CategoriesSold = CategoriesSold,
                            ProductsSold = ProductsSold,
                            VariantsSold = VariantsSold,
                            TotalQuantitySold = SalesAnalysisTotalQuantitySold,
                            TotalSales = SalesAnalysisTotalSales
                        },
                        Data = SalesAnalysisReports.ToList()
                    };

                    string filePath = saveDialog.FileName;

                    // Offload CPU-bound export + write to background thread
                    byte[] bytes = await Task.Run(() => _excelExporter.Export(request));
                    await File.WriteAllBytesAsync(filePath, bytes);
                    StatusMessage = string.Empty;

                    ShowExportSuccess(filePath);
                }
            }
            catch (Exception ex)
            {
                // Never fail silently: surface the error to the UI.
                StatusMessage = $"Export failed: {ex.Message}";
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

        private static void ShowExportSuccess(string filePath)
        {
            var result = MessageBox.Show(
                "Report exported successfully.",
                "Export Complete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
                catch
                {
                    // Opening the file is best-effort; no error shown if it fails.
                }
            }
        }

        private void OpenReceipt(TransactionReportDto? transaction)
        {
            if (transaction == null)
                return;

            _receiptDisplayService.ShowReceipt(transaction.TransactionId);
        }
    }
}