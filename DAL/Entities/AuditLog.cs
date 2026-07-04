using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }

        public int? UserId { get; set; }

        public string ActionType { get; set; } = string.Empty;

        public string? EntityName { get; set; }

        public int? EntityId { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime OccurredAt { get; set; }


        // Navigation

        public User? User { get; set; }
    }
}
