namespace UI.Services
{
    /// <summary>
    /// Independent service for displaying receipts as modal dialogs.
    /// Decouples receipt display from MainViewModel.
    /// </summary>
    public interface IReceiptDisplayService
    {
        /// <summary>
        /// Shows a receipt for the given transaction ID as a modal dialog.
        /// </summary>
        void ShowReceipt(int transactionId);

        /// <summary>
        /// Shows a receipt for a printer.
        /// </summary>
        void PrintReceipt(int transactionId);
    }
}