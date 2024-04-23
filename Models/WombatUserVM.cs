using Microsoft.AspNetCore.Identity;
using Wombat.Constants;

namespace Wombat.Models
{
    public class WombatUserVM
    {
        public string Id { get; set; }
        public string Email { get; set; }

        private string AddRole( string newrole, string currentroles )
        {
            string Result = "";
            if (currentroles == "") Result = newrole;
            else Result = currentroles + ", " + newrole;
            return Result;
        }

        public string RolesToString()
        {
            string Result = "";
            foreach (var role in Roles)
            {
                if (Result == "") Result = role;
                else Result = Result + ", " + role;
            }
            return Result;
        }

        public IList<string> Roles;

    }
}
