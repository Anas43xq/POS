namespace Contracts.Shifts
{
    public class ShiftDetailDto
    {
        public int ShiftId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public double DurationHours { get; set; }
        public decimal OpeningCash { get; set; }
        public decimal? ClosingCash { get; set; }
        public decimal? ExpectedCash { get; set; }
        public decimal? CashDifference { get; set; }
        public string StatusLabel { get; set; } = string.Empty;

        public int TotalTransactions { get; set; }
        public int CashTransactions { get; set; }
        public int CardTransactions { get; set; }
        public decimal CashSales { get; set; }
        public decimal CardSales { get; set; }
        public decimal TotalSales { get; set; }
        public int RefundsCount { get; set; }

        public List<ShiftTransactionDto> Transactions { get; set; } = new();
    }

    public class ShiftTransactionDto
    {
        public int TransactionId { get; set; }
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
