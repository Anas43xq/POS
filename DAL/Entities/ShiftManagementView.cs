using System;

namespace DAL.Entities
{
    public class ShiftManagementView
    {
        public int ShiftId { get; set; }
        public int UserId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public decimal OpeningCash { get; set; }
        public decimal? ClosingCash { get; set; }
        public decimal? ExpectedCash { get; set; }
        public decimal? CashDifference { get; set; }
        public ShiftStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public int DurationHours { get; set; }
    }
}
