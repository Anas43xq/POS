using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Shift
    {
        public int ShiftId { get; set; }

        public int UserId { get; set; }

        public DateTime OpenedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public decimal OpeningCash { get; set; }

        public decimal? ClosingCash { get; set; }

        public decimal? ExpectedCash { get; set; }

        public decimal? CashDifference { get; set; }

        public ShiftStatus Status { get; set; } = ShiftStatus.Open;


        // Navigation

        public User? User { get; set; }

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}