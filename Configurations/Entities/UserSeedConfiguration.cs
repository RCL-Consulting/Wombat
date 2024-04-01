using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Data;

namespace Wombat.Configurations.Entities
{
    public class UserSeedConfiguration : IEntityTypeConfiguration<WombatUser>
    {
        public void Configure(EntityTypeBuilder<WombatUser> builder)
        {
            var hasher = new PasswordHasher<WombatUser>();

            builder.HasData(
                new WombatUser
                {
                    Id = "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                    Email = "admin@localhost.com",
                    NormalizedEmail = "ADMIN@LOCALHOST.COM",
                    UserName = "admin@localhost.com",
                    NormalizedUserName = "ADMIN@LOCALHOST.COM",
                    Name = "System",
                    Surname = "Admin",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                },
                new WombatUser
                {
                    Id = "409696F3-CA82-4381-A734-38A5EF6AA445",
                    Email = "user@localhost.com",
                    NormalizedEmail = "USER@LOCALHOST.COM",
                    UserName = "user@localhost.com",
                    NormalizedUserName = "USER@LOCALHOST.COM",
                    Name = "System",
                    Surname = "User",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                }
            );
        }
    }
}