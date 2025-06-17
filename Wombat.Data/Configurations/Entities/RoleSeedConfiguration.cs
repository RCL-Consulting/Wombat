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
                    Name = Roles.Administrator,
                    NormalizedName = Roles.Administrator.ToUpper()
                },
                new IdentityRole
                {
                    Id = "86708DA5-1688-4617-B6FF-B64E78D9A032",
                    Name = Roles.InstitutionalAdmin,
                    NormalizedName = Roles.InstitutionalAdmin.ToUpper()
                },
                new IdentityRole
                {
                    Id = "725E680D-9DCD-4C9D-B8F9-2415E12F0FA5",
                    Name = Roles.SpecialityAdmin,
                    NormalizedName = Roles.SpecialityAdmin.ToUpper()
                },
                new IdentityRole
                {
                    Id = "{32EFCBBA-5B31-4248-82C9-4E3A053DEA86}",
                    Name = Roles.SubSpecialityAdmin,
                    NormalizedName = Roles.SubSpecialityAdmin.ToUpper()
                },
                new IdentityRole
                {
                    Id = "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E",
                    Name = Roles.Assessor,
                    NormalizedName = Roles.Assessor.ToUpper()
                },
                new IdentityRole
                {
                    Id = "5653650A-167F-42D5-A67F-2C0AE818EB84",
                    Name = Roles.Coordinator,
                    NormalizedName = Roles.Coordinator.ToUpper()
                },
                new IdentityRole
                {
                    Id = "616FDCDE-67A1-4C3F-8153-ACC4809FCAE8",
                    Name = Roles.CommitteeMember,
                    NormalizedName = Roles.CommitteeMember.ToUpper()
                },
                new IdentityRole
                {
                    Id = "3FAA94D6-23C2-4365-9951-796673F48402",
                    Name = Roles.Trainee,
                    NormalizedName = Roles.Trainee.ToUpper()
                },
                new IdentityRole
                {
                    Id = "7F94F90B-44AA-4A93-846A-F16581B487F6",
                    Name = Roles.PendingTrainee,
                    NormalizedName = Roles.PendingTrainee.ToUpper()
                }
            );
        }
    }
}
