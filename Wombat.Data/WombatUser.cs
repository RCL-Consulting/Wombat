using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class WombatUser: IdentityUser
    {
        public string Name { get; set; }
        public string Surname{ get; set; }

        public string IdNumber { get; set; } = "";

        public string HPCSANumber { get; set; } = "";

        public int InstitutionId { get; set; } = 1;
        [ForeignKey("InstitutionId")]
        public Institution? Institution { get; set; }

        public int? SubSpecialityId { get; set; }
        [ForeignKey("SubSpecialityId")]
        public SubSpeciality? SubSpeciality { get; set; }

        public DateTime DateJoined { get; set; }
    }
}
