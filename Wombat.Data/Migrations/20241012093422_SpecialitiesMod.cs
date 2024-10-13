using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class SpecialitiesMod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9e697d4e-74f8-4803-8a1e-3dff49461fbb", "AQAAAAIAAYagAAAAEAZgvfZnQzpJ8EDMMYnHCJM0MMo1nTdiSawOpmg1GEDAuPJ7SGRoIcVdVqAuF7KtmA==", "d2c33ae6-89f9-4365-a237-facbf5fed131" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fd56aae3-a5fe-4e8f-a0ce-06b68e17e8f3", "AQAAAAIAAYagAAAAEOGrx9nsiUqEgK4lE24u9W3G2/MAFpiqdWNe8kNzfjOvHcHUgGvsztIs+2zC4nKQRg==", "16776fe2-5868-47fa-b19a-24d2572c54d6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4dc1956d-9ddb-44cb-82b1-49f3fae3522b", "AQAAAAIAAYagAAAAEIK3J7Yt4ZLZXv54+x71VdpUC09qNocW6hu+M2Y6P048rwf1RGnzEkGZ7FbmQQ45iw==", "d5ff6159-bf14-46bb-8bba-09dcbcb14974" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d16724aa-16b8-4f1a-abb3-fb20e913a3ac", "AQAAAAIAAYagAAAAEEWfbIIKBkO0LJcEcNEoDIONI7TLkdGaPM8sYfXjETa9LPWJl4cbvu8A4GanvRu+0w==", "ed40950f-f22b-4e96-86a0-bcc5416bdd1c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c900983f-657f-4f2a-b1d0-88d24b3f140e", "AQAAAAIAAYagAAAAEJ6auOukzlQK7tl4Da8ahzNDJiJDPIrb/2XxRP1eYedGVcDMO/fqxq6ht6hagksNmQ==", "0af9f90b-ef6d-4731-942c-4f0a6f5da4b4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0f87f11b-63c1-4c95-b940-d8d131f77e93", "AQAAAAIAAYagAAAAEJ/CCdUq8lMEOELYhv9VnpngXfOWI8Ff/pby214Vf7XRSjCZy/5Fnb1IO6ktB29nTQ==", "10ee6ad3-2d4d-4c56-8f4e-3dcc60e0b89b" });
        }
    }
}
