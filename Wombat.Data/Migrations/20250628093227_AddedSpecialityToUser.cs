using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedSpecialityToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SpecialityId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp", "SpecialityId" },
                values: new object[] { "4e4c0f55-03ca-4536-b589-501d6489951c", "AQAAAAIAAYagAAAAEB+8YDkOMeAv7GfH2j1FebpNZ9Dt+hj1fWl5myK6gyS+8nbQNsfDbbpgD9c7/fWGPQ==", "34a13b2a-f69d-4139-b36c-08e71db6fe57", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp", "SpecialityId" },
                values: new object[] { "5bb85bea-dbe2-4801-a910-001d8a8c94fc", "AQAAAAIAAYagAAAAEOd2SdykiAv/CqRlmhnAQqokIogYRITdY/LpXgnubKfIOAqisHRP/5dEixEjme0r3Q==", "67385b68-f57b-480f-9aa7-3b1e1a4131e0", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp", "SpecialityId" },
                values: new object[] { "97c115b5-b885-45a3-85fa-7351fc290e33", "AQAAAAIAAYagAAAAENdLVA9VLcEBcnNl0YiH5TbI+su2DyLaS2X8r1PVB0DEYC2FJB6OoXWCBVAms10NIA==", "18888262-7513-4895-ae87-0355a8e6342d", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp", "SpecialityId" },
                values: new object[] { "568b2a48-2414-4535-8017-a68c2db61253", "AQAAAAIAAYagAAAAEI0bBE4xsq8wkexKgaqsUqtKYoYNJDXv/AHPneGw/YwaNb7DpTopr2pU0Zfa9yNlYA==", "bf986c72-5494-41c2-ae5f-8974b55f84a3", null });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_SpecialityId",
                table: "AspNetUsers",
                column: "SpecialityId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Specialities_SpecialityId",
                table: "AspNetUsers",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Specialities_SpecialityId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_SpecialityId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SpecialityId",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c41715ce-52c0-4921-a66d-f324bd39c693", "AQAAAAIAAYagAAAAEJ/L+/IiQmHxr0AJdzShCUiFUoFMO26fD8lzE9njDrkFjZr9EYvrofZHnJvp2jy6iw==", "f11951fd-479c-4257-a0bb-9f795484b4cd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d3906fd8-34ab-4f29-b87e-f9c66ac87236", "AQAAAAIAAYagAAAAEBS7GgiKuB85omvhKCpyDmDJqXxlG00nfOiPHJ0OxNw/L4YMEdMnST/gqP27uU6Y4Q==", "bd487c9c-d986-4a59-acab-9693807e7484" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "89b2b0dd-e0eb-424e-9e6e-a90b720fa624", "AQAAAAIAAYagAAAAEOum+STBl5xMUWaJ/YkTQ98lIbt00ri4erJsw2QYJ4Q65aDL+OG5xItNhUQ0aKjaSA==", "4b0a8c80-dc47-4eca-9cc2-a95672a43672" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c41f0a36-e464-47dc-9ea6-3b6fe4f86dab", "AQAAAAIAAYagAAAAEHAB9FdOdALFAcCQrhq4h1VU/kEW8ffokKJI5p57/uKHqWJnwQ2T0fEQQsWJw0tKcg==", "77dae253-29d4-43a8-84b2-c3e309750368" });
        }
    }
}
