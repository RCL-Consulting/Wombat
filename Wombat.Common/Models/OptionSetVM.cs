﻿/*Copyright (C) 2024 RCL Consulting
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
    public class OptionSetVM
    {
        public OptionSetVM()
        {
            Description = "";
            Options = new List<OptionVM>();
        }
        public int Id { get; set; }
        public string Description { get; set; }

        [Display(Name = "Display Rank")]
        public bool DisplayRank { get; set; }

        public List<OptionVM>? Options { get; set; }

        public bool CanDelete { get; set; } = true;

        public bool CanEdit { get; set; } = true;

        public List<SelectListItem>? Institutions { get; set; }
        public List<SelectListItem>? Specialities { get; set; }
        public List<SelectListItem>? SubSpecialities { get; set; }

        public List<SubSpecialityOption> AllSubSpecialities { get; set; } = new();
        
        public int? InstitutionId { get; set; }
        public string? InstitutionName { get; set; }

        public int? SpecialityId { get; set; }
        public string? SpecialityName { get; set; }

        public int? SubSpecialityId { get; set; }
        public string? SubSpecialityName { get; set; }

        public bool IsEditableByCurrentUser { get; set; } = false;

    }
}
