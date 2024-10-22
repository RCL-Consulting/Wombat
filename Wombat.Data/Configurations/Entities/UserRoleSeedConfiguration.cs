using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Wombat.Data.Configurations.Entities
{
    internal class UserRoleSeedConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.HasData(
                new IdentityUserRole<string>
                {
                    //admin@localhost.com is administrator
                    RoleId = "8DDBAFD6-4044-4AF0-BED8-D77B16F75404",
                    UserId = "D68AC189-5BB6-4511-B96F-0F8BD55569AC"
                },
                new IdentityUserRole<string>
                {
                    //"upassessor@localhost.com" is assessor
                    RoleId = "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E", 
                    UserId = "409696F3-CA82-4381-A734-38A5EF6AA445"
                },
                new IdentityUserRole<string>
                {
                    //"uctassessor@localhost.com" is assessor
                    RoleId = "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E",
                    UserId = "965631FD-F55B-4AAE-85B4-81561A5CD78F"
                },
                new IdentityUserRole<string>
                {
                    //"uptrainee@localhost.com" is trainee
                    RoleId = "3FAA94D6-23C2-4365-9951-796673F48402",
                    UserId = "19A3D40C-9852-43B9-9BEC-B2552FA715F7"
                },
                new IdentityUserRole<string>
                {
                    //"ucttrainee@localhost.com" is trainee
                    RoleId = "3FAA94D6-23C2-4365-9951-796673F48402",
                    UserId = "343ABA27-DDC0-40E0-AD5C-C4E918965876"
                },
                new IdentityUserRole<string>
                {
                    //"upcoordinator@localhost.com" is trainee
                    RoleId = "5653650A-167F-42D5-A67F-2C0AE818EB84",
                    UserId = "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237"
                },
                new IdentityUserRole<string>
                {
                    //"uctcoordinator@localhost.com" is trainee
                    RoleId = "5653650A-167F-42D5-A67F-2C0AE818EB84",
                    UserId = "9AF99F0F-868F-41A8-8121-758647507D92"
                }
            );
        }
    }
}