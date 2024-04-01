using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultUsersAndRoles4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "EmailConfirmed", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "decda88c-26b8-4f48-9083-6341d628b5c1", true, "USER@LOCALHOST.COM", "AQAAAAIAAYagAAAAECv4Z40b1IM8q4BI7+M7uHxbDzIgXBxukg11SuL1SCE8WslVQ+Oc5gTH4RqDc/s89Q==", "0861f3c5-8315-4bfc-b89e-12f6e6cbc09e", "user@localhost.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "EmailConfirmed", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "825585fc-737b-448e-b46d-c1b7cc569c5a", true, "ADMIN@LOCALHOST.COM", "AQAAAAIAAYagAAAAEPZWOpuwmdBVrQI3CBjY0huQpM33i/v9JsvqXNb518lCW66Hkb8IjVCRYGS7UdRn+A==", "475b4ebf-5527-42f5-acd5-a8a403e5a382", "admin@localhost.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "EmailConfirmed", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "b5c6f30b-d372-46ac-a92c-b9626d5b0440", false, null, "AQAAAAIAAYagAAAAEN2XAACjpTebYJS03zwYKa4mBasfa8VGo7/HmOlBc90rEIxRAg5fycaGinBb/VVhVg==", "b8dbd90c-c0bd-41eb-ae9f-3e3f39471de3", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "EmailConfirmed", "NormalizedUserName", "PasswordHash", "SecurityStamp", "UserName" },
                values: new object[] { "22156bf6-dc96-4299-a453-62406e33ae81", false, null, "AQAAAAIAAYagAAAAEMlSfTVcB/abzCOLfUcysg2DYROFug8PErYEKgMbSbM/Yh0S+2tPfS7ogawmLA0G1A==", "6dd0f563-cf95-4e41-9827-2ddc2c43cff8", null });
        }
    }
}
