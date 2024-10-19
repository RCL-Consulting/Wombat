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
