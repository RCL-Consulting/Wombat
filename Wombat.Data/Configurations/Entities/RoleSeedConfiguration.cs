using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
                    Id = "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E",
                    Name = Roles.Assessor,
                    NormalizedName = Roles.Assessor.ToUpper()
                },
                new IdentityRole
                {
                    Id = "3FAA94D6-23C2-4365-9951-796673F48402",
                    Name = Roles.Trainee,
                    NormalizedName = Roles.Trainee.ToUpper()
                },
                new IdentityRole
                {
                    Id = "5653650A-167F-42D5-A67F-2C0AE818EB84",
                    Name = Roles.Coordinator,
                    NormalizedName = Roles.Coordinator.ToUpper()
                },
                new IdentityRole
                {
                    Id = "0EC1BA72-D475-4B61-9A06-E9F85CF2CCB8",
                    Name = Roles.Unassigned,
                    NormalizedName = Roles.Unassigned.ToUpper()
                }
            );
        }
    }
}