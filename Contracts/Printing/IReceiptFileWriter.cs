using POS.Contracts.Receipts;

namespace POS.Contracts.Printing;

public interface IReceiptFileWriter
{
    /// <summary>
    /// Renders <paramref name="receipt"/> as a PDF and writes it to
    /// <paramref name="filePath"/>. The parent directory is created
    /// if it does not exist. Overwrites any existing file at that
    /// path.
    /// </summary>
    /// <param name="receipt">The receipt data to render.</param>
    /// <param name="filePath">Absolute path of the target .pdf file.</param>
    /// <param name="paperWidthMm">Paper width in millimetres (58 or 80).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveReceiptAsPdfAsync(
        ReceiptDetailsDto receipt,
        string filePath,
        int paperWidthMm,
        CancellationToken cancellationToken = default);
}
