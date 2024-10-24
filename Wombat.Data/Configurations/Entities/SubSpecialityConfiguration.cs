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

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data.Configurations.Entities
{
    public class SubSpecialityConfiguration : IEntityTypeConfiguration<SubSpeciality>
    {
        public void Configure(EntityTypeBuilder<SubSpeciality> builder)
        {
            builder.HasData(
                new SubSpeciality
                {
                    Id = 1,
                    Name = "General",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 2,
                    Name = "Common",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 3,
                    Name = "Adolescent Medicine",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 4,
                    Name = "Cardiology",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 5,
                    Name = "Child Abuse Pediatrics",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 6,
                    Name = "Critical Care Medicine",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 7,
                    Name = "Developmental-Behavioral Pediatrics",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 8,
                    Name = "Emergency Medicine",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 9,
                    Name = "Endocrinology",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 10,
                    Name = "Gastroenterology",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 11,
                    Name = "Hematology-Oncology",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 12,
                    Name = "Hospital Medicine",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 13,
                    Name = "Infectious Diseases",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 14,
                    Name = "Neonatal-Perinatal Medicine",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 15,
                    Name = "Nephrology",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 16,
                    Name = "Pulmonology",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 17,
                    Name = "Rheumatology",
                    SpecialityId = 1
                },
                new SubSpeciality
                {
                    Id = 18,
                    Name = "Pediatric Transplant Hepatology",
                    SpecialityId = 1
                }
             );
        }
    }
}
