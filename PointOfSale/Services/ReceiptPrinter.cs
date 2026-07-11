using POS.Contracts.Printing;
using POS.Contracts.Receipts;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using UI.Views;

namespace UI.Services;

public sealed class ReceiptPrinter : IReceiptPrinter
{
    private const double MmToDip = 1.0 / 25.4 * 96.0;

    public string PrinterType => IPrintingService.ReceiptPrinterType;

    public Task PrintAsync(object content, bool showDialog, CancellationToken cancellationToken = default)
    {
        if (content is not ReceiptDetailsDto receipt)
            throw new ArgumentException($"Expected ReceiptDetailsDto, got {content?.GetType().Name}", nameof(content));

        var settings = new PrinterSettings();
        return PrintReceiptAsync(receipt, settings, showDialog, cancellationToken);
    }

    public Task PrintReceiptAsync(ReceiptDetailsDto receipt, PrinterSettings settings, bool showDialog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        ArgumentNullException.ThrowIfNull(settings);

        var width = settings.PaperWidth > 0 ? settings.PaperWidth * MmToDip : 300;

        var view = new ReceiptPrintView
        {
            DataContext = receipt,
            Width = width
        };

        view.Measure(new Size(view.Width, double.PositiveInfinity));
        view.Arrange(new Rect(0, 0, view.Width, view.DesiredSize.Height));
        view.UpdateLayout();

        var printDialog = new PrintDialog();

        if (!showDialog && !string.IsNullOrWhiteSpace(settings.ReceiptPrinterName))
        {
            try
            {
                var localPrintServer = new LocalPrintServer();
                var printer = localPrintServer.GetPrintQueue(settings.ReceiptPrinterName);
                printDialog.PrintQueue = printer;
            }
            catch
            {
                // Fall back to default printer if configured name is missing.
            }
        }

        bool? printed = showDialog ? printDialog.ShowDialog() : true;
        if (printed == true)
        {
            printDialog.PrintVisual(view, "Receipt");
        }

        return Task.CompletedTask;
    }
}
