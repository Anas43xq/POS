namespace POS.Contracts.Printing;

/// <summary>
/// Printer configuration settings. Persisted through ISettingsService.
/// </summary>
public sealed class PrinterSettings
{
    /// <summary>
    /// Name of the Windows-installed printer to use (e.g. "Epson TM-T20").
    /// </summary>
    public string ReceiptPrinterName { get; set; } = string.Empty;

    /// <summary>
    /// Paper width in mm (58 or 80).
    /// </summary>
    public int PaperWidth { get; set; } = 80;

    /// <summary>
    /// If true, automatically prints the receipt after payment is confirmed.
    /// </summary>
    public bool AutoPrint { get; set; } = true;

    /// <summary>
    /// If true, shows the Windows Print Dialog before printing.
    /// If false, prints silently to the configured printer.
    /// </summary>
    public bool ShowPrintDialog { get; set; }

    /// <summary>
    /// Number of copies to print.
    /// </summary>
    public int Copies { get; set; } = 1;

    /// <summary>
    /// What the "Print Test Receipt" button does.
    /// </summary>
    public TestPrintAction TestPrintAction { get; set; } = TestPrintAction.Print;
}

/// <summary>
/// Action performed when the user clicks "Print Test Receipt" on the
/// Settings page. The two options are mutually exclusive — the test
/// print either sends the receipt to the configured thermal printer
/// (no dialog) or saves a PDF copy of it on disk. They cannot both
/// run from a single click.
/// </summary>
public enum TestPrintAction
{
    /// <summary>
    /// Send the test receipt directly to the printer selected in the
    /// "Receipt Printer" combo box. Matches the production receipt's
    /// printing path (no Windows dialog).
    /// </summary>
    Print = 0,

    /// <summary>
    /// Save the test receipt as a PDF file under
    /// %USERPROFILE%\Documents\Hawa Receipts\ with file name
    /// "hawa-receipt-{yyyy-MM-dd}-{4-digit}.pdf".
    /// </summary>
    SaveToFile = 1,
}
