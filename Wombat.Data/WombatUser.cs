using Microsoft.AspNetCore.Identity;

namespace Wombat.Data
{
    public class WombatUser: IdentityUser
    {
        public string Name { get; set; }
        public string Surname{ get; set; }
        public DateTime DateJoined { get; set; }
    }
}
