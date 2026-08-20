namespace UI.Services;

public interface IReceiptDisplayService
{
    /// <summary>
    /// Loads the receipt and displays it in a modal window. Fully asynchronous —
    /// does not block the calling (UI) thread while the receipt is loaded from the database.
    /// </summary>
    Task ShowReceiptAsync(int transactionId);

    Task PrintReceiptAsync(int transactionId);

    /// <summary>
    /// Prints and displays a receipt for a transaction that was just completed,
    /// fetching the receipt data only once and reusing it for both operations.
    /// </summary>
    Task PrintAndShowReceiptAsync(int transactionId);
}
