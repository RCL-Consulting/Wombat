using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Constants;

namespace Wombat.Configurations.Entities
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
                }
            );
        }
    }
}