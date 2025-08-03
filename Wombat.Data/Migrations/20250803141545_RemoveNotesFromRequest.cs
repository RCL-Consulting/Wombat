using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotesFromRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssessorNotes",
                table: "AssessmentRequests");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "AssessmentRequests");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dd99099a-11a1-44fa-8fb2-0cd215282235", "AQAAAAIAAYagAAAAEMsRzQ8p/RKuXT7GviVPSE3C213QAU7d2466HQSayJIhJCFMkm2q6atXQXKp76+txw==", "e0cae147-504f-4a49-9305-07222df8d00f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "521a773c-4192-427e-9f9d-76523d978b0b", "AQAAAAIAAYagAAAAEOu80QgDrqXIeTI9ipqw+ePFgfYH1Nffd78ns6fYVb1WIqEjXfVhfHnYCe49b22u+A==", "bb027b62-8696-4d39-96e8-ff8f84310e4c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bdbea8f6-4865-4f78-8a83-75ee2150e6bb", "AQAAAAIAAYagAAAAELB3UizPpFyWJedXe4teeJ+YSiCEhwHDBPmdFHDhnL0a/O5C5TwOC3sPh8yEODNVkQ==", "105cbcff-4c12-4ea3-8a7a-801c48c4df7a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4e9ea5f1-a13e-4063-9b58-e48e27a52cb0", "AQAAAAIAAYagAAAAEKk7IzDDWkMsFOgk6S2vFY9uzhrbFkIetKm6UfdIqrg9qBkbEBnM1TSmMOtfKb1aFw==", "61f35e4c-07ec-4a3f-95d1-5fa67d566be5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssessorNotes",
                table: "AssessmentRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "AssessmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a419720d-f3f1-4333-a4f0-1344a0c78f2f", "AQAAAAIAAYagAAAAEI7lakUr7WygLEoJw1iSYofq+Y0nqRMFvIdB7S5EYI8dDkoK61jY/eOgTSKNefkJhQ==", "2a75e658-b5f5-48d1-bfa6-353d5f02de0f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "db3c2794-1df2-4fae-b39f-a0b3f6b62234", "AQAAAAIAAYagAAAAEMvdFLbIh3lm8p6uIfXuWbEV2Pte3rurirMDTgcPaJkkNDAUkEaFSCYduRgSh31ORg==", "6c6c6431-bd10-4e18-a8af-fd0ae0f5fd99" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b6c2745a-0841-413b-ac21-bb1f0915fe68", "AQAAAAIAAYagAAAAEPJdaXhekqKeL+wJUZEnEetDwpH4KqV8luVHbTQGZ5oBS2FF7LzqnX004gZrKSxZOQ==", "406eaf7d-4d2f-496f-ac27-dea9bcf07e62" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e1d9a5ba-5ac4-49bb-8225-1dd58c309881", "AQAAAAIAAYagAAAAEPl2RDbJv8qRM6CFAhpNp6OD6AEYoH4cvBdw3safjDR/SYswVoWeRX+GZBzqhKupEg==", "d49fe532-9223-4d9c-90ae-83741c0ae294" });
        }
    }
}
