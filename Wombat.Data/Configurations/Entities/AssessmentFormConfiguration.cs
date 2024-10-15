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
                }
            );
        }
    }
}
