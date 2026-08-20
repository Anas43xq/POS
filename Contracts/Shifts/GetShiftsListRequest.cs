namespace Contracts.Shifts
{
    public class GetShiftsListRequest
    {
        public string PeriodType { get; set; } = "Today";
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? StatusFilter { get; set; }
        public int? CashierId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
