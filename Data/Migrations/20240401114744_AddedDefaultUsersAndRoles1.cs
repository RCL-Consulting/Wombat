using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultUsersAndRoles1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d3898e47-f97c-4587-aa0d-ecf662a7deec", "AQAAAAIAAYagAAAAENj5vbfq/5oXzeiMC6JP6kIZSYR0GETNKjTs9Rt4TeV6HgEbmrm7nIVKIMltbf6syw==", "25a75105-e93f-42cc-ab14-c793f3ba5e78" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a0a1eb71-1eff-4cf5-a3ee-f177f5ebd02b", "AQAAAAIAAYagAAAAEAQtJg/H98uJgS0Gp6c90ELc5fsjnwW2L3FSVnezd09FbYSf5FXhH4oK3AqlpvUXiQ==", "848d61cd-5139-40d9-a811-75d0e3545493" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e4a3848d-f420-4bc2-8499-54141a611177", "AQAAAAIAAYagAAAAEFQ+IrQBu3iqW48nw9k5dfyZMsgaOmQZIxsYPgiroTFzSua2i+v8FNLjaJT9aqM/xg==", "74eafa17-e4a9-494e-b666-532019a94665" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0c394c9b-e89a-4441-a4ca-d400569fbfc0", "AQAAAAIAAYagAAAAEE8oK6Q7tWl/Y50AAsZRCF/3BEqWz/gVzHWwvi0o6GafTS8Hjpq1LDHzB8xC/+SAPw==", "6f0b9959-c236-4575-b7da-85c79484ccf7" });
        }
    }
}
