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
}