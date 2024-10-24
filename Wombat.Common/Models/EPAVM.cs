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

using Microsoft.AspNetCore.Mvc.Rendering;

namespace Wombat.Common.Models
{
    public class EPAVM
    {
        public EPAVM()
        {
            Name = "";
            Description = "";
            Forms = new List<EPAFormVM>();
            EPACurricula = new List<EPACurriculumVM>();
        }
        public int Id { get; set; }

        public int SpecialityId { get; set; }
        public int SubSpecialityId { get; set; }

        public SpecialityVM? Speciality { get; set; }
        public SubSpecialityVM? SubSpeciality { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public List<EPAFormVM>? Forms { get; set; }

        public static List<AssessmentFormVM> AvailableForms { get; set; } = new List<AssessmentFormVM>();
        
        public List<SpecialitySelectVM>? Specialities { get; set; }
        public List<SubSpecialitySelectVM>? SubSpecialities { get; set; }

        public List<EPACurriculumVM> EPACurricula { get; set; }

    }
}
