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
                    Name = "Paediatrics-General EPA1 CEX",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 3,
                    Name = "Paediatrics-General EPA2 CEX",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 4,
                    Name = "Paediatrics-General EPA3 CEX",
                    CanDelete = true
                },
                new AssessmentForm
                {
                    Id = 5,
                    Name = "Paediatrics-General EPA4 CEX",
                    CanDelete = true
                }
            );
        }
    }
}
