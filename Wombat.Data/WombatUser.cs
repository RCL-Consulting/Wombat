using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class WombatUser: IdentityUser
    {
        public string Name { get; set; }
        public string Surname{ get; set; }

        public int InstitutionId { get; set; } = 1;

        [ForeignKey("InstitutionId")]
        public Institution? Institution { get; set; }

        public DateTime DateJoined { get; set; }
    }
}
