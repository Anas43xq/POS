using System;

namespace DAL.Entities
{
    public class ShiftSummaryView
    {
        public int ShiftId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public DateTime OpenTime { get; set; }
        public decimal OpeningCash { get; set; }
        public DateTime? CloseTime { get; set; }
        public decimal? ClosingCash { get; set; }
        public decimal? ExpectedCash { get; set; }
        public decimal? CashDifference { get; set; }
        public ShiftStatus Status { get; set; } = ShiftStatus.Open;
    }
}
