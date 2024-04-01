using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wombat.Configurations.Entities
{
    internal class UserRoleSeedConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "8DDBAFD6-4044-4AF0-BED8-D77B16F75404",
                    UserId = "D68AC189-5BB6-4511-B96F-0F8BD55569AC"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E",
                    UserId = "409696F3-CA82-4381-A734-38A5EF6AA445"
                }
            );
        }
    }
}