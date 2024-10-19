namespace Wombat.Common.Models
{
    public class WombatUserVM
    {
        public WombatUserVM()
        {
            Roles = new List<CheckBoxListItem>();
            Email = "";
        }

        public string Name { get; set; }
        public string Surname { get; set; }

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
                if(role.IsChecked) Result = AddRole(role.Display, Result);
            }
            return Result;
        }

        public bool UserIsInRole(string role)
        {
            bool result = false;
            foreach (var item in Roles)
            {
                if (item.Display == role)
                {
                    if (item.IsChecked)
                        result = true;
                    break;
                }
            }
            return result;
        }

        public List<CheckBoxListItem> Roles { get; set; }

        public InstitutionVM Institution { get; set; }

        public SpecialityVM? Speciality { get; set; }

        public SubSpecialityVM? SubSpeciality { get; set; }

        public string IdNumber { get; set; } = "";
        public string HPCSANumber { get; set; } = "";

        public bool EmailConfirmed { get; set; }
    }
}
