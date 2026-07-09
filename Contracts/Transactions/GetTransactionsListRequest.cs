namespace Contracts.Transactions
{
    public class GetTransactionsListRequest
    {
        public string PeriodType { get; set; } = "Today";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;

        /// <summary>
        /// Optional status filter: "Completed" | "Voided" | "Pending" | null (all).
        /// </summary>
        public string? StatusFilter { get; set; }
    }
}
