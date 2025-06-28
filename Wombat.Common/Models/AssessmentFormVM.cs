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
using System.ComponentModel.DataAnnotations;

namespace Wombat.Common.Models
{
    public class AssessmentFormVM
    {
        public AssessmentFormVM()
        {
            Name = "";
            OptionCriteria = new List<OptionCriterionVM>();
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        public List<OptionCriterionVM> OptionCriteria { get; set; }

        public EPAVM? EPAVM { get; set; }

        public bool CanDelete { get; set; } = true;

        public int? InstitutionId { get; set; }
        public string? InstitutionName { get; set; }

        public int? SpecialityId { get; set; }
        public string? SpecialityName { get; set; }

        public int? SubSpecialityId { get; set; }
        public string? SubSpecialityName { get; set; }

        public bool IsGlobal =>
            InstitutionId == null && SpecialityId == null && SubSpecialityId == null;

        public List<SelectListItem>? Institutions { get; set; }
        public List<SelectListItem>? Specialities { get; set; }
        public List<SelectListItem>? SubSpecialities { get; set; }

        public List<SubSpecialityOption> AllSubSpecialities { get; set; } = new();

        public class EPAOption
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public int SubSpecialityId { get; set; }
        }

        public List<EPAOption> AllEPAs { get; set; } = new();
        public List<int> SelectedEPAIds { get; set; } = new();

        public bool IsEditableByCurrentUser { get; set; } = false;

        public bool AutoPopulateSubSpecialities { get; set; } = true;
    }
}
