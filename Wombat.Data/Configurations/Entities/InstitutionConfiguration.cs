﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
                }
            );
        }
    }
}