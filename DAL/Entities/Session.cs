using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Session
    {
        public int SessionId { get; set; }

        public int UserId { get; set; }

        public User? User { get; set; }

        public DateTime LoginAt { get; set; } = DateTime.Now;

        public DateTime? LogoutAt { get; set; }
    }
}
