/*Copyright (C) 2024 RCL Consulting
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>
 */

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
                    //"assessor@localhost.com" is assessor
                    RoleId = "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E", 
                    UserId = "409696F3-CA82-4381-A734-38A5EF6AA445"
                },
                new IdentityUserRole<string>
                {
                    //"trainee@localhost.com" is trainee
                    RoleId = "3FAA94D6-23C2-4365-9951-796673F48402",
                    UserId = "19A3D40C-9852-43B9-9BEC-B2552FA715F7"
                },
                new IdentityUserRole<string>
                {
                    //"coordinator@localhost.com" is trainee
                    RoleId = "5653650A-167F-42D5-A67F-2C0AE818EB84",
                    UserId = "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237"
                }
            );
        }
    }
}