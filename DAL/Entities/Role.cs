using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Role
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();

    }
}
