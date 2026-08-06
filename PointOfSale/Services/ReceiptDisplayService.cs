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
    private readonly INotificationService _notifications;

    public ReceiptDisplayService(
        IReceiptService receiptService,
        IPrintingService printingService,
        ILogger<ReceiptDisplayService> logger,
        INotificationService notifications)
    {
        _receiptService = receiptService;
        _printingService = printingService;
        _logger = logger;
        _notifications = notifications;
    }

    public async Task ShowReceiptAsync(int transactionId)
    {
        if (transactionId <= 0)
            return;

        var receipt = await _receiptService.GetReceiptByTransactionIdAsync(transactionId);
        ShowReceiptWindow(receipt, transactionId);
    }

    public async Task PrintReceiptAsync(int transactionId)
    {
        if (transactionId <= 0)
            return;

        var receipt = await _receiptService.GetReceiptByTransactionIdAsync(transactionId);
        await PrintReceiptCoreAsync(receipt, transactionId);
    }

    public async Task PrintAndShowReceiptAsync(int transactionId)
    {
        if (transactionId <= 0)
            return;

        // Fetch the receipt once and reuse it for both printing and display,
        // instead of issuing the same (joined) query twice.
        var receipt = await _receiptService.GetReceiptByTransactionIdAsync(transactionId);

        // Printing can continue in the background; it doesn't need to block
        // showing the receipt to the cashier.
        _ = PrintReceiptCoreAsync(receipt, transactionId);

        ShowReceiptWindow(receipt, transactionId);
    }

    private void ShowReceiptWindow(POS.Contracts.Receipts.ReceiptDetailsDto? receipt, int transactionId)
    {
        if (receipt == null)
        {
            _logger.LogWarning("Receipt could not be loaded for transaction {TransactionId}", transactionId);
            _notifications.ShowWarning("Transaction completed, but receipt could not be loaded.");
            return;
        }

        var receiptViewModel = new ReceiptViewModel(receipt, _printingService, _logger, _notifications);
        var receiptWindow = new ReceiptWindow(receiptViewModel);

        if (Application.Current.MainWindow != null &&
            Application.Current.MainWindow != receiptWindow)
        {
            receiptWindow.Owner = Application.Current.MainWindow;
        }

        receiptWindow.ShowDialog();
    }

    private async Task PrintReceiptCoreAsync(POS.Contracts.Receipts.ReceiptDetailsDto? receipt, int transactionId)
    {
        if (receipt == null)
        {
            _logger.LogWarning("Receipt could not be loaded for printing transaction {TransactionId}", transactionId);
            _notifications.ShowWarning("No receipt is available to print.");
            return;
        }

        try
        {
            await _printingService.PrintReceiptDirectAsync(receipt, showDialog: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to print receipt for transaction {TransactionId}", transactionId);
            _notifications.ShowError("Printing failed. Please check the printer and try again.");
        }
    }
}
