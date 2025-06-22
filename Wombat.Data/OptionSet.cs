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

namespace Wombat.Data
{
    public class OptionSet : BaseEntity
    {
        public OptionSet()
        {
            Description = "";
            DisplayRank = true;
            Options = new List<Option>();
        }

        public string Description { get; set; }

        public bool DisplayRank { get; set; }

        public List<Option> Options { get; set; }

        public bool CanDelete { get; set; } = true;

        public bool CanEdit { get; set; } = true;

        public int? InstitutionId { get; set; }
        public Institution? Institution { get; set; }

        public int? SpecialityId { get; set; }
        public Speciality? Speciality { get; set; }

        public int? SubSpecialityId { get; set; }
        public SubSpeciality? SubSpeciality { get; set; }

        public static int kTextId = 1;
        public static int kEPAScaleId = 2;
    }
}
