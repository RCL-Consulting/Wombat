using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class RegistrationInvitationVM
    {
        public int Id { get; set; }
        public string Email { get; set; } = "";
        public string Roles { get; set; } = "";
        public string Institution { get; set; } = "";
        public string? Speciality { get; set; }
        public string? SubSpeciality { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }

        public string Status =>
            IsUsed ? "Used" :
            ExpiryDate < DateTime.UtcNow ? "Expired" :
            "Active";
    }
}
