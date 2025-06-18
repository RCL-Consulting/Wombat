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

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Wombat.Data
{
    public class WombatUser: IdentityUser
    {
        public enum eApprovalStatus
        {
            Pending,
            Approved,
            Rejected
        }

        public eApprovalStatus ApprovalStatus { get; set; } = eApprovalStatus.Pending;

        public string Name { get; set; }
        public string Surname{ get; set; }

        public string? IdNumber { get; set; } = "";

        public string HPCSANumber { get; set; } = "";

        public int? InstitutionId { get; set; } = 1;
        [ForeignKey("InstitutionId")]
        public Institution? Institution { get; set; }

        public int? SubSpecialityId { get; set; }
        [ForeignKey("SubSpecialityId")]
        public SubSpeciality? SubSpeciality { get; set; }

        public DateTime DateJoined { get; set; }

        public DateTime StartDate { get; set; }
    }
}
