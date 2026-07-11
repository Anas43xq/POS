namespace POS.Contracts.Printing;

/// <summary>
/// Base abstraction for all printer types.
/// Every printer (receipt, kitchen, label, report) implements this.
/// </summary>
public interface IPrinter
{
    /// <summary>
    /// Unique name identifying the printer type, used for settings lookup.
    /// </summary>
    string PrinterType { get; }

    /// <summary>
    /// Prints the specified content asynchronously.
    /// </summary>
    /// <param name="content">The printable content.</param>
    /// <param name="showDialog">If true, shows the Windows Print Dialog; otherwise prints silently.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task PrintAsync(object content, bool showDialog, CancellationToken cancellationToken = default);
}