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
    public class EPACurriculumConfiguration : IEntityTypeConfiguration<EPACurriculum>
    {
        public void Configure(EntityTypeBuilder<EPACurriculum> builder)
        {
            builder.HasData(
                new EPACurriculum
                {
                    Id = 1,
                    NumberOfMonths = 6,
                    EPAId = 1,
                    EPAScaleId = 2
                },
                new EPACurriculum
                {
                    Id = 2,
                    NumberOfMonths = 12,
                    EPAId = 1,
                    EPAScaleId = 3
                },
                new EPACurriculum
                {
                    Id = 3,
                    NumberOfMonths = 24,
                    EPAId = 1,
                    EPAScaleId = 4
                },
                new EPACurriculum
                {
                    Id = 4,
                    NumberOfMonths = 36,
                    EPAId = 1,
                    EPAScaleId = 5
                },
                new EPACurriculum
                {
                    Id = 5,
                    NumberOfMonths = 6,
                    EPAId = 2,
                    EPAScaleId = 2
                },
                new EPACurriculum
                {
                    Id = 6,
                    NumberOfMonths = 12,
                    EPAId = 2,
                    EPAScaleId = 3
                },
                new EPACurriculum
                {
                    Id = 7,
                    NumberOfMonths = 24,
                    EPAId = 2,
                    EPAScaleId = 5
                },
                new EPACurriculum
                {
                    Id = 8,
                    NumberOfMonths = 24,
                    EPAId = 3,
                    EPAScaleId = 3
                },
                new EPACurriculum
                {
                    Id = 9,
                    NumberOfMonths = 36,
                    EPAId = 3,
                    EPAScaleId = 4
                },
                new EPACurriculum
                {
                    Id = 10,
                    NumberOfMonths = 42,
                    EPAId = 3,
                    EPAScaleId = 5
                },
                new EPACurriculum
                {
                    Id = 11,
                    NumberOfMonths = 24,
                    EPAId = 4,
                    EPAScaleId = 3
                },
                new EPACurriculum
                {
                    Id = 12,
                    NumberOfMonths = 42,
                    EPAId = 4,
                    EPAScaleId = 4
                },
                new EPACurriculum
                {
                    Id = 13,
                    NumberOfMonths = 48,
                    EPAId = 4,
                    EPAScaleId = 5
                }
            );
        }
    }
}
