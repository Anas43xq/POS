using BLL.Interfaces;
using POS.Contracts.Printing;
using POS.Contracts.Receipts;
using Microsoft.Extensions.Logging;
using System.Windows;
using UI.ViewModels;
using UI.Views;

namespace UI.Services;

public class ReceiptDisplayService : IReceiptDisplayService
{
    private readonly IReceiptService _receiptService;
    private readonly IPrintingService _printingService;
    private readonly ILogger<ReceiptDisplayService> _logger;

    public ReceiptDisplayService(
        IReceiptService receiptService,
        IPrintingService printingService,
        ILogger<ReceiptDisplayService> logger)
    {
        _receiptService = receiptService;
        _printingService = printingService;
        _logger = logger;
    }

    public void ShowReceipt(int transactionId)
    {
        if (transactionId <= 0)
            return;

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

        var receiptViewModel = new ReceiptViewModel(receipt, _printingService, _logger);
        var receiptWindow = new ReceiptWindow(receiptViewModel);

        if (Application.Current.MainWindow != null &&
            Application.Current.MainWindow != receiptWindow)
        {
            receiptWindow.Owner = Application.Current.MainWindow;
        }

        receiptWindow.ShowDialog();
    }

    public async Task PrintReceiptAsync(int transactionId)
    {
        if (transactionId <= 0)
            return;

        var receipt = await _receiptService.GetReceiptByTransactionIdAsync(transactionId);

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
            await _printingService.PrintReceiptDirectAsync(receipt, showDialog: false);
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
