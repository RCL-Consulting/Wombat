using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class RegisterFromInviteVM
    {
        public RegisterFromInviteVM()
        {
            Email = string.Empty;
            Roles = new List<string>();
            SpecialityId = 0;
            SubSpecialityId = 0;
            FirstName = string.Empty;
            LastName = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }

        public string Token { get; set; } = string.Empty;
        public string Email { get; set; }

        public List<string> Roles { get; set; }

        public int SpecialityId { get; set; }

        public int? SubSpecialityId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public string? SpecialityName { get; set; } = string.Empty;
        public string? SubSpecialityName { get; set; } = string.Empty;

        public string? InstitutionName { get; set; } = string.Empty;

    }
}
