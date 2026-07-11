using System;

namespace Contracts.Shifts
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
        public string Status { get; set; } = string.Empty;

        public string OpenedAt => OpenTime.ToString("HH:mm");
        public string ClosedAtDisplay => CloseTime.HasValue ? CloseTime.Value.ToString("HH:mm") : "--";
        public bool IsShortfall => CashDifference.HasValue && CashDifference.Value < 0;
    }
}
