using POS.Contracts.Receipts;

namespace POS.Contracts.Printing;

/// <summary>
/// Receipt-specific printer.
/// Extends the generic <see cref="IPrinter"/> with receipt-typed convenience overloads.
/// </summary>
public interface IReceiptPrinter : IPrinter
{
    Task PrintReceiptAsync(ReceiptDetailsDto receipt, PrinterSettings settings, bool showDialog, CancellationToken cancellationToken = default);
}