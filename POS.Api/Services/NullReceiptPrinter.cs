using POS.Contracts.Printing;
using POS.Contracts.Receipts;

namespace POS.Api.Services;

/// <summary>
/// No-op implementation of <see cref="IReceiptPrinter"/> for the API host.
/// The API does not print receipts; this satisfies DI so the singleton
/// <see cref="BLL.Services.PrintingService"/> can be resolved.
/// </summary>
public sealed class NullReceiptPrinter : IReceiptPrinter
{
    public string PrinterType => IPrintingService.ReceiptPrinterType;

    public Task PrintAsync(object content, bool showDialog, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task PrintReceiptAsync(ReceiptDetailsDto receipt, PrinterSettings settings, bool showDialog, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}