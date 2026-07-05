using BLL.Interfaces;
using POS.Contracts.Receipts;
using Microsoft.Extensions.Logging;
using System.Windows;
using UI.ViewModels;
using UI.Views;

namespace UI.Services
{
    /// <summary>
    /// Independent service for displaying receipts as modal dialogs.
    /// Completely decoupled from MainViewModel - any ViewModel can use it directly.
    /// </summary>
    public class ReceiptDisplayService : IReceiptDisplayService
    {
        private readonly IReceiptService _receiptService;
        private readonly ReceiptPrinterService _receiptPrinterService;
        private readonly ILogger<ReceiptDisplayService> _logger;

        public ReceiptDisplayService(
            IReceiptService receiptService,
            ReceiptPrinterService receiptPrinterService,
            ILogger<ReceiptDisplayService> logger)
        {
            _receiptService = receiptService;
            _receiptPrinterService = receiptPrinterService;
            _logger = logger;
        }

        public void ShowReceipt(int transactionId)
        {
            if (transactionId <= 0)
                return;

            // Fetch receipt data synchronously (modal dialog context)
            var receipt = Task.Run(
                () => _receiptService.GetReceiptByTransactionIdAsync(transactionId))
                .GetAwaiter().GetResult();

            if (receipt == null)
            {
                _logger.LogWarning("Receipt could not be loaded for transaction {TransactionId}", transactionId);
                MessageBox.Show(
                    "Transaction completed, but receipt could not be loaded.",
                    "Receipt Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var receiptViewModel = new ReceiptViewModel(receipt, _receiptPrinterService, _logger);
            var receiptWindow = new ReceiptWindow(receiptViewModel);

            // Set owner to the active window, but avoid setting to itself
            if (Application.Current.MainWindow != null &&
                Application.Current.MainWindow != receiptWindow)
            {
                receiptWindow.Owner = Application.Current.MainWindow;
            }

            receiptWindow.ShowDialog();
        }

        public void PrintReceipt(int transactionId)
        {
            if (transactionId <= 0)
                return;

            var receipt = Task.Run(
                () => _receiptService.GetReceiptByTransactionIdAsync(transactionId))
                .GetAwaiter().GetResult();

            if (receipt == null)
            {
                _logger.LogWarning("Receipt could not be loaded for printing transaction {TransactionId}", transactionId);
                MessageBox.Show(
                    "No receipt is available to print.",
                    "Print Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                _receiptPrinterService.Print(receipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to print receipt for transaction {TransactionId}", transactionId);
                MessageBox.Show(
                    "Printing failed. Please check the printer and try again.",
                    "Print Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
