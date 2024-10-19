using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data.Configurations.Entities
{
    public class EPAFormConfiguration : IEntityTypeConfiguration<EPAForm>
    {
        public void Configure(EntityTypeBuilder<EPAForm> builder)
        {
            builder.HasData(
                new EPAForm
                {
                    Id = 1,
                    EPAId = 1,
                    FormId = 2
                },
                new EPAForm
                {
                    Id = 2,
                    EPAId = 2,
                    FormId = 3
                },
                new EPAForm
                {
                    Id = 3,
                    EPAId = 3,
                    FormId = 4
                },
                new EPAForm
                {
                    Id = 4,
                    EPAId = 4,
                    FormId = 5
                }
            );
        }
    }
}
