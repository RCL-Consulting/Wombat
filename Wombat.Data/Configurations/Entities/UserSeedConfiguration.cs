using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wombat.Common.Models;

namespace Wombat.Data.Configurations.Entities
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
                    InstitutionId = 1,
                    SubSpecialityId = null
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
                    InstitutionId = 2,
                    SubSpecialityId = null
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
                    InstitutionId = 2,
                    SubSpecialityId = 1
                },
                new WombatUser
                {
                    Id = "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                    Email = "coordinator@localhost.com",
                    NormalizedEmail = "COORDINATOR@LOCALHOST.COM",
                    UserName = "coordinator@localhost.com",
                    NormalizedUserName = "COORDINATOR@LOCALHOST.COM",
                    Name = "System",
                    Surname = "Coordinator",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                    InstitutionId = 2
                }
            );
        }
    }
}