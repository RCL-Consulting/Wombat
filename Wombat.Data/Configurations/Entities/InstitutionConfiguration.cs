using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wombat.Data.Configurations.Entities
{
    class InstitutionConfiguration : IEntityTypeConfiguration<Institution>
    {
        public void Configure(EntityTypeBuilder<Institution> builder)
        {
            builder.HasData(
                new Institution
                {
                    Id = 1,
                    Name = "Owning institution",
                    CanDelete = false
                },
                new Institution
                {
                    Id = 2,
                    Name = "University of Pretoria",
                    CanDelete = false
                },
                new Institution
                {
                    Id = 3,
                    Name = "University of Cape Town",
                    CanDelete = false
                }
            );
        }
    }
}
