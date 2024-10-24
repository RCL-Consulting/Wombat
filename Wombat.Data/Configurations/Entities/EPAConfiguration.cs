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
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data.Configurations.Entities
{
    class EPAConfiguration : IEntityTypeConfiguration<EPA>
    {
        public void Configure(EntityTypeBuilder<EPA> builder)
        {
            builder.HasData(
                new EPA
                {
                    Id = 1,
                    Name = "EPA1",
                    Description = "Provide Consultation to Other Health Care Providers Caring for Children",
                    SubSpecialityId = 1,
                },
                new EPA
                {
                    Id = 2,
                    Name = "EPA2",
                    Description = " Provide Recommended Pediatric Health Screening",
                    SubSpecialityId = 1,
                },
                new EPA
                {
                    Id = 3,
                    Name = "EPA3",
                    Description = "Care for the Well Newborn",
                    SubSpecialityId = 1,
                },
                new EPA
                {
                    Id = 4,
                    Name = "EPA4",
                    Description = "Manage Patients with Acute, Common Diagnoses in an Ambulatory,\r\nEmergency, or Inpatient Setting",
                    SubSpecialityId = 1,
                }
            );
        }
    }
}
