using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wombat.Common.Constants;

namespace Wombat.Data.Configurations.Entities
{
    public class RoleSeedConfiguration : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole
                {
                    Id = "8DDBAFD6-4044-4AF0-BED8-D77B16F75404",
                    Name = Role.Administrator.ToStringValue(),
                    NormalizedName = Role.Administrator.ToStringValue().ToUpper()
                },
                new IdentityRole
                {
                    Id = "86708DA5-1688-4617-B6FF-B64E78D9A032",
                    Name = Role.InstitutionalAdmin.ToStringValue(),
                    NormalizedName = Role.InstitutionalAdmin.ToStringValue().ToUpper()
                },
                new IdentityRole
                {
                    Id = "725E680D-9DCD-4C9D-B8F9-2415E12F0FA5",
                    Name = Role.SpecialityAdmin.ToStringValue(),
                    NormalizedName = Role.SpecialityAdmin.ToStringValue().ToUpper()
                },
                new IdentityRole
                {
                    Id = "{32EFCBBA-5B31-4248-82C9-4E3A053DEA86}",
                    Name = Role.SubSpecialityAdmin.ToStringValue(),
                    NormalizedName = Role.SubSpecialityAdmin.ToStringValue().ToUpper()
                },
                new IdentityRole
                {
                    Id = "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E",
                    Name = Role.Assessor.ToStringValue(),
                    NormalizedName = Role.Assessor.ToStringValue().ToUpper()
                },
                new IdentityRole
                {
                    Id = "5653650A-167F-42D5-A67F-2C0AE818EB84",
                    Name = Role.Coordinator.ToStringValue(),
                    NormalizedName = Role.Coordinator.ToStringValue().ToUpper()
                },
                new IdentityRole
                {
                    Id = "616FDCDE-67A1-4C3F-8153-ACC4809FCAE8",
                    Name = Role.CommitteeMember.ToStringValue(),
                    NormalizedName = Role.CommitteeMember.ToStringValue().ToUpper()
                },
                new IdentityRole
                {
                    Id = "3FAA94D6-23C2-4365-9951-796673F48402",
                    Name = Role.Trainee.ToStringValue(),
                    NormalizedName = Role.Trainee.ToStringValue().ToUpper()
                },
                new IdentityRole
                {
                    Id = "7F94F90B-44AA-4A93-846A-F16581B487F6",
                    Name = Role.PendingTrainee.ToStringValue(),
                    NormalizedName = Role.PendingTrainee.ToStringValue().ToUpper()
                }
            );
        }
    }
}
