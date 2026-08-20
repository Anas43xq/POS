namespace Contracts.Transactions
{
    public class GetTransactionKpisRequest
    {
        public string PeriodType { get; set; } = "Today"; // "Today" | "Week" | "Month" | "Custom"
        public DateTime? FromDate { get; set; }            // required when PeriodType == "Custom"
        public DateTime? ToDate { get; set; }               // optional when PeriodType == "Custom"
    }
}