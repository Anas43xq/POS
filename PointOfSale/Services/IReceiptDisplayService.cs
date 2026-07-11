namespace UI.Services;

public interface IReceiptDisplayService
{
    void ShowReceipt(int transactionId);
    Task PrintReceiptAsync(int transactionId);
}
