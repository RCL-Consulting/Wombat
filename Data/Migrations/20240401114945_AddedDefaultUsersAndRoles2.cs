using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultUsersAndRoles2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "258975c0-32ed-4ab4-8da1-1bbc091cec69", "AQAAAAIAAYagAAAAEKWEfGwjQnyGpkALOOB7zLX59wU3PMT44QUgEDkmcgnoyDfMIgoFYsWj5qgbHqdJUA==", "843149fd-04a7-4f74-9247-f92f4aa89341" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c991781d-825c-43a4-bd95-7d0ef4559c19", "AQAAAAIAAYagAAAAEFMpuVNKrOtW9iMOhgtEKESdP/ny7JAYt1l0LcRniqUvcQK74G2YX71l1XD281RciA==", "6f2ed86a-c1cc-450f-b4ba-6271e28ad5f3" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
