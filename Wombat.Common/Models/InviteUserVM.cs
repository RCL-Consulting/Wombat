using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Common.Models
{
    public class InviteUserVM
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public List<string> Roles { get; set; } = new();

        [Required]
        public int SpecialityId { get; set; }

        public int? SubSpecialityId { get; set; }

        public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(7);

        // For dropdowns
        public List<SelectListItem> AvailableRoles { get; set; } = new();
        public List<SelectListItem> Specialities { get; set; } = new();
        public List<SelectListItem> SubSpecialities { get; set; } = new();

        public List<SubSpecialityOption> AllSubSpecialities { get; set; } = new();

        [Required]
        public int InstitutionId { get; set; }

        public List<SelectListItem> Institutions { get; set; } = new();
    }

}
