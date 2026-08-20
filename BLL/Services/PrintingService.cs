using BLL.Interfaces;
using POS.Contracts.Printing;
using POS.Contracts.Receipts;

namespace BLL.Services;

public sealed class PrintingService : IPrintingService
{
    private readonly ISettingsService _settingsService;
    private readonly IReceiptPrinter _receiptPrinter;

    public PrintingService(ISettingsService settingsService, IReceiptPrinter receiptPrinter)
    {
        _settingsService = settingsService;
        _receiptPrinter = receiptPrinter;
    }

    public async Task PrintReceiptAsync(ReceiptDetailsDto receipt, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.GetPrinterSettingsAsync();

        if (!settings.AutoPrint)
            return;

        await PrintCopiesAsync(receipt, settings, settings.ShowPrintDialog, cancellationToken);
    }

    public async Task PrintReceiptDirectAsync(ReceiptDetailsDto receipt, bool showDialog, CancellationToken cancellationToken = default)
    {
        var settings = await _settingsService.GetPrinterSettingsAsync();

        bool effectiveShowDialog = showDialog || settings.ShowPrintDialog;

        await PrintCopiesAsync(receipt, settings, effectiveShowDialog, cancellationToken);
    }

    private async Task PrintCopiesAsync(ReceiptDetailsDto receipt, PrinterSettings settings, bool showDialog, CancellationToken cancellationToken)
    {
        int copies = Math.Max(1, settings.Copies);

        for (int i = 0; i < copies; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _receiptPrinter.PrintReceiptAsync(receipt, settings, showDialog, cancellationToken);
        }
    }
}
