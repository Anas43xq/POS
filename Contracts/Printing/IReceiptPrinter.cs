using POS.Contracts.Receipts;

namespace POS.Contracts.Printing
{
    public interface IReceiptPrinter
    {
        public void Print(ReceiptDetailsDto receipt);
    }
}