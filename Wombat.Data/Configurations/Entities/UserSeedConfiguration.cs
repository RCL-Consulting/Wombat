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
                    Email = "upassessor@localhost.com",
                    NormalizedEmail = "UPASSESSOR@LOCALHOST.COM",
                    UserName = "upassessor@localhost.com",
                    NormalizedUserName = "UPASSESSOR@LOCALHOST.COM",
                    Name = "System",
                    Surname = "UPAssessor",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                    InstitutionId = 2,
                    SubSpecialityId = null
                },
                new WombatUser
                {
                    Id = "965631FD-F55B-4AAE-85B4-81561A5CD78F",
                    Email = "uctassessor@localhost.com",
                    NormalizedEmail = "UCTASSESSOR@LOCALHOST.COM",
                    UserName = "uctassessor@localhost.com",
                    NormalizedUserName = "UCTASSESSOR@LOCALHOST.COM",
                    Name = "System",
                    Surname = "UCTAssessor",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                    InstitutionId = 3,
                    SubSpecialityId = null
                },
                new WombatUser
                {
                    Id = "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                    Email = "uptrainee@localhost.com",
                    NormalizedEmail = "UPTRAINEE@LOCALHOST.COM",
                    UserName = "uptrainee@localhost.com",
                    NormalizedUserName = "UPTRAINEE@LOCALHOST.COM",
                    Name = "System",
                    Surname = "UPTrainee",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                    InstitutionId = 2,
                    SubSpecialityId = 1
                },
                new WombatUser
                {
                    Id = "343ABA27-DDC0-40E0-AD5C-C4E918965876",
                    Email = "ucttrainee@localhost.com",
                    NormalizedEmail = "UCTTRAINEE@LOCALHOST.COM",
                    UserName = "ucttrainee@localhost.com",
                    NormalizedUserName = "UCTTRAINEE@LOCALHOST.COM",
                    Name = "System",
                    Surname = "UCTTrainee",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                    InstitutionId = 3,
                    SubSpecialityId = 1
                },
                new WombatUser
                {
                    Id = "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                    Email = "upcoordinator@localhost.com",
                    NormalizedEmail = "UPCOORDINATOR@LOCALHOST.COM",
                    UserName = "upcoordinator@localhost.com",
                    NormalizedUserName = "UPCOORDINATOR@LOCALHOST.COM",
                    Name = "System",
                    Surname = "UPCoordinator",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                    InstitutionId = 2
                },
                new WombatUser
                {
                    Id = "9AF99F0F-868F-41A8-8121-758647507D92",
                    Email = "uctcoordinator@localhost.com",
                    NormalizedEmail = "UCTCOORDINATOR@LOCALHOST.COM",
                    UserName = "uctcoordinator@localhost.com",
                    NormalizedUserName = "UCTCOORDINATOR@LOCALHOST.COM",
                    Name = "System",
                    Surname = "UCTCoordinator",
                    PasswordHash = hasher.HashPassword(null, "P@ssw0rd"),
                    EmailConfirmed = true,
                    InstitutionId = 2
                }
            );
        }
    }
}