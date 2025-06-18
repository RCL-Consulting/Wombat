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

using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Wombat.Common.Models;

namespace Wombat.Data
{
    public class RegistrationInvitation : BaseEntity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = String.Empty;

        [Required]
        public string Token { get; set; } = String.Empty;

        [Required]
        public string Roles { get; set; } = String.Empty;// Comma-separated for now. Can be split on load.

        public int? InstitutionId { get; set; }

        [ForeignKey("InstitutionId")]
        public Institution? Institution { get; set; }

        public int? SpecialityId { get; set; }

        [ForeignKey("SpecialityId")]
        public Speciality? Speciality { get; set; }

        public int? SubSpecialityId { get; set; }

        [ForeignKey("SubSpecialityId")]
        public SubSpeciality? SubSpeciality { get; set; }

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}