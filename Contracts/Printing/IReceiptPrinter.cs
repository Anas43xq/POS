using POS.Contracts.Receipts;

namespace POS.Contracts.Printing;

public interface IReceiptPrinter : IPrinter
{
    Task PrintReceiptAsync(ReceiptDetailsDto receipt, PrinterSettings settings, bool showDialog, CancellationToken cancellationToken = default);
}
