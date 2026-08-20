using POS.Contracts.Receipts;

namespace POS.Contracts.Printing;

public interface IPrintingService
{
    const string ReceiptPrinterType = "Receipt";

    /// <summary>
    /// Prints a receipt. Respects AutoPrint and ShowPrintDialog settings.
    /// </summary>
    Task PrintReceiptAsync(ReceiptDetailsDto receipt, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prints a receipt bypassing AutoPrint check — always prints.
    /// Used when user explicitly clicks Print from the receipt preview.
    /// </summary>
    Task PrintReceiptDirectAsync(ReceiptDetailsDto receipt, bool showDialog, CancellationToken cancellationToken = default);
}
