using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data.Configurations.Entities
{
    public class AssessmentFormConfiguration : IEntityTypeConfiguration<AssessmentForm>
    {
        public void Configure(EntityTypeBuilder<AssessmentForm> builder)
        {
            builder.HasData(
                new AssessmentForm
                {
                    Id = 1,
                    Name = "Default Template",
                    CanDelete = false
                },
                new AssessmentForm
                {
                    Id = 2,
                    Name = "Peadiatrics-General EPA1",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 3,
                    Name = "Peadiatrics-General EPA2",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 4,
                    Name = "Peadiatrics-General EPA3",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 5,
                    Name = "Peadiatrics-General EPA4",
                    CanDelete = true
                }
            );
        }
    }
}
