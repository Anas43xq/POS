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
    private readonly ILocalizationService _localizationService;

    public ReceiptDisplayService(
        IReceiptService receiptService,
        IPrintingService printingService,
        ILogger<ReceiptDisplayService> logger,
        INotificationService notifications,
        ILocalizationService localizationService)
    {
        _receiptService = receiptService;
        _printingService = printingService;
        _logger = logger;
        _notifications = notifications;
        _localizationService = localizationService;
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

        var receipt = await _receiptService.GetReceiptByTransactionIdAsync(transactionId);

        if (receipt != null)
        {
            var msg = _localizationService.GetString("Receipt.PaymentSuccess");
            _notifications.ShowSuccess(msg);
        }

        _ = PrintReceiptCoreAsync(receipt, transactionId);

        ShowReceiptWindow(receipt, transactionId);
    }

    private void ShowReceiptWindow(POS.Contracts.Receipts.ReceiptDetailsDto? receipt, int transactionId)
    {
        if (receipt == null)
        {
            _logger.LogWarning("Receipt could not be loaded for transaction {TransactionId}", transactionId);
            _notifications.ShowWarning(_localizationService.GetString("Receipt.CouldNotLoadReceipt"));
            return;
        }

        var receiptViewModel = new ReceiptViewModel(receipt, _printingService, _logger, _notifications, _localizationService);
        var receiptWindow = new ReceiptWindow(receiptViewModel);

        if (Application.Current.MainWindow != null &&
            Application.Current.MainWindow != receiptWindow)
        {
            receiptWindow.Owner = Application.Current.MainWindow;
        }

        // Non-modal so the cashier can continue working (e.g. start the
        // next sale, refresh recent sales) while the customer is shown
        // the receipt. ShowDialog() would block the UI thread until the
        // window is closed, which is no longer desired now that the
        // success toast carries the user-facing feedback.
        receiptWindow.Show();
    }

    private async Task PrintReceiptCoreAsync(POS.Contracts.Receipts.ReceiptDetailsDto? receipt, int transactionId)
    {
        if (receipt == null)
        {
            _logger.LogWarning("Receipt could not be loaded for printing transaction {TransactionId}", transactionId);
            _notifications.ShowWarning(_localizationService.GetString("Receipt.NoReceiptToPrint"));
            return;
        }

        try
        {
            await _printingService.PrintReceiptDirectAsync(receipt, showDialog: false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to print receipt for transaction {TransactionId}", transactionId);
            _notifications.ShowError(_localizationService.GetString("Receipt.PrintingFailed"));
        }
    }
}
