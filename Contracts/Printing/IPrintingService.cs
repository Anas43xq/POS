using POS.Contracts.Receipts;

namespace POS.Contracts.Printing;

/// <summary>
/// Coordinator service that the UI communicates with for all printing needs.
/// Loads printer settings, selects the right printer, and dispatches to the
/// appropriate printer implementation.
/// The UI never interacts with Windows printing APIs directly.
/// </summary>
public interface IPrintingService
{
    /// <summary>
    /// Printer type string used for settings lookup.
    /// </summary>
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