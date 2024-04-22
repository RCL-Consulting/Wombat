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

        public string Role() 
        {
            string Result = "";
            if (Assessor) Result = AddRole(Roles.Assessor, Result);
            if (Administrator) Result = AddRole(Roles.Administrator, Result);
            if (Coordinator) Result = AddRole(Roles.Coordinator, Result);
            if (Trainee) Result = AddRole(Roles.Trainee, Result);
            return Result;
        }

        public bool Assessor { get; set; }
        public bool Administrator { get; set; }
        public bool Coordinator { get; set; }
        public bool Trainee { get; set; }

    }
}
