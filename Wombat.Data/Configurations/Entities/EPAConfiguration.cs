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
