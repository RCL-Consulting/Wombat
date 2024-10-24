/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

namespace Wombat.Common.Models
{
    public class WombatUserVM
    {
        public WombatUserVM()
        {
            Roles = new List<CheckBoxListItem>();
            Email = "";
        }

        public string DisplayName
        {
            get
            {
                return Name + " " + Surname + " ("+Email+ ")";
            }
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

        public int InstitutionId { get; set; }
        public InstitutionVM Institution { get; set; }

        public SpecialityVM? Speciality { get; set; }

        public SubSpecialityVM? SubSpeciality { get; set; }

        public string IdNumber { get; set; } = "";
        public string HPCSANumber { get; set; } = "";

        public bool EmailConfirmed { get; set; }
    }
}
