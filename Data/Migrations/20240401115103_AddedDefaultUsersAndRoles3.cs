using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedDefaultUsersAndRoles3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b5c6f30b-d372-46ac-a92c-b9626d5b0440", "AQAAAAIAAYagAAAAEN2XAACjpTebYJS03zwYKa4mBasfa8VGo7/HmOlBc90rEIxRAg5fycaGinBb/VVhVg==", "b8dbd90c-c0bd-41eb-ae9f-3e3f39471de3" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "22156bf6-dc96-4299-a453-62406e33ae81", "AQAAAAIAAYagAAAAEMlSfTVcB/abzCOLfUcysg2DYROFug8PErYEKgMbSbM/Yh0S+2tPfS7ogawmLA0G1A==", "6dd0f563-cf95-4e41-9827-2ddc2c43cff8" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
