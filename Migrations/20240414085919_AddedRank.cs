using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Migrations
{
    /// <inheritdoc />
    public partial class AddedRank : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rank",
                table: "OptionCriteria",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3387d3bb-a996-4940-a2f4-f8ab67b17aba", "AQAAAAIAAYagAAAAEDWsFx57ZnrLbcM7G/70fyGngCHmvB7pHvbqkhazavoXmm5+21lJFm0dJVygg5DUFA==", "e1ce16c5-76c9-4e89-97e8-1a279b65e10d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "04cdaf60-a613-4003-9a2b-ae077b52c269", "AQAAAAIAAYagAAAAEJQ09hKsLHIHDLt4x2+sk4vUCTnNfwTO7RrgXOC+gCmgEDB3/B63fJrgXNdmgRd+Rw==", "6a6aec25-ff36-4430-9b0e-ddab018628c9" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "OptionCriteria");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3cd1edf9-85b9-49b0-a111-92aaeab76a0d", "AQAAAAIAAYagAAAAECicCyT1dyHJXmkNyAemojJO09bak0oAMgy6XKAU0qU7E1xiXVkL+q+Gn3t5nToxlA==", "471fee56-b2b8-4376-9dd3-d3e566ce4567" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "07aa16b9-d540-4b28-9b50-370bfefcce53", "AQAAAAIAAYagAAAAEAjreF4TeZNIhxhoX6mGSg60wo8xt8V+6TCuA5W9fagkbt5x5/GIkWG6VP2zLmgyxA==", "e398ef56-51c2-4ae1-98be-f5d91a0fc890" });
        }
    }
}
