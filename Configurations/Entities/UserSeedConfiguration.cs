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
                    Email = "assessor@localhost.com",
                    NormalizedEmail = "ASSESSOR@LOCALHOST.COM",
                    UserName = "assessor@localhost.com",
                    NormalizedUserName = "ASSESSOR@LOCALHOST.COM",
                    Name = "System",
                    Surname = "Assessor",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                },
                new WombatUser
                {
                    Id = "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                    Email = "trainee@localhost.com",
                    NormalizedEmail = "TRAINEE@LOCALHOST.COM",
                    UserName = "trainee@localhost.com",
                    NormalizedUserName = "TRAINEE@LOCALHOST.COM",
                    Name = "System",
                    Surname = "Trainee",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                }
            );
        }
    }
}