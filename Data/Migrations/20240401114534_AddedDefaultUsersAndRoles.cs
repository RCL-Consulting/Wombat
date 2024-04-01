using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultUsersAndRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "3FAA94D6-23C2-4365-9951-796673F48402", null, "Trainee", "TRAINEE" },
                    { "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E", null, "Assessor", "ASSESSOR" },
                    { "8DDBAFD6-4044-4AF0-BED8-D77B16F75404", null, "Administrator", "ADMINISTRATOR" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DateJoined", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "Surname", "TwoFactorEnabled", "UserName" },
                values: new object[,]
                {
                    { "409696F3-CA82-4381-A734-38A5EF6AA445", 0, "e4a3848d-f420-4bc2-8499-54141a611177", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "user@localhost.com", false, false, null, "System", "USER@LOCALHOST.COM", null, "AQAAAAIAAYagAAAAEFQ+IrQBu3iqW48nw9k5dfyZMsgaOmQZIxsYPgiroTFzSua2i+v8FNLjaJT9aqM/xg==", null, false, "74eafa17-e4a9-494e-b666-532019a94665", "User", false, null },
                    { "D68AC189-5BB6-4511-B96F-0F8BD55569AC", 0, "0c394c9b-e89a-4441-a4ca-d400569fbfc0", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@localhost.com", false, false, null, "System", "ADMIN@LOCALHOST.COM", null, "AQAAAAIAAYagAAAAEE8oK6Q7tWl/Y50AAsZRCF/3BEqWz/gVzHWwvi0o6GafTS8Hjpq1LDHzB8xC/+SAPw==", null, false, "6f0b9959-c236-4575-b7da-85c79484ccf7", "Admin", false, null }
                });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E", "409696F3-CA82-4381-A734-38A5EF6AA445" },
                    { "8DDBAFD6-4044-4AF0-BED8-D77B16F75404", "D68AC189-5BB6-4511-B96F-0F8BD55569AC" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3FAA94D6-23C2-4365-9951-796673F48402");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E", "409696F3-CA82-4381-A734-38A5EF6AA445" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "8DDBAFD6-4044-4AF0-BED8-D77B16F75404", "D68AC189-5BB6-4511-B96F-0F8BD55569AC" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "50BC176C-BD18-49A8-8DF7-9FC6FE9E7B9E");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8DDBAFD6-4044-4AF0-BED8-D77B16F75404");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC");
        }
    }
}
