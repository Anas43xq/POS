using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class User
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }

        public Role? Role { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }


        public ICollection<Session> Sessions { get; set; } = new List<Session>();

        public ICollection<Shift> Shifts { get; set; } = new List<Shift>();

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();

        public ICollection<AuditLog> AuditLogs { get; set; }
            = new List<AuditLog>();

    }
}
