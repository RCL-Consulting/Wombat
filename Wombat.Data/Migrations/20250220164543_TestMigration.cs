using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class TestMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e55cf8c0-1be1-4411-b0ee-fef7c603f9ab", "AQAAAAIAAYagAAAAEGNxLkJsFCWCUCcJa+8ILRcv6cJkUTqwa3y1qlyQT/0xHf9Jr0tKtUnvGDTW4hcBSA==", "709a2fb2-0fd6-430e-ae40-69c36274a9a6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e613f9b8-d22c-4fbc-b704-b2f6e61536a0", "AQAAAAIAAYagAAAAEIHBsnhaog3ASHi+0/7cZu4/MGoGAL1CY9kRnt5/QXFHLdevi1Mz7oQGdtDobB2dCw==", "efb87b85-e647-4b16-a410-34b65f766fed" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8579213a-633c-448e-9e01-7abac8254871", "AQAAAAIAAYagAAAAELtn6++m8E6AL6MjGtO5bDHaOMZochx2BbQ7eqUheJ6AG9EGY8hbrPd8Coow1HBsag==", "35b5298c-4c31-41d5-84b3-a7e76a639e05" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a10faef5-fde7-4f09-8448-ce23f9dce824", "AQAAAAIAAYagAAAAEFNdDhsTAjP5figU2OiTYA9Glfo4ohbsWaXcH0KKR2U5XWNqBEhU+769n7KpYHVG4A==", "4db7cc75-44dd-4c05-9ad0-d4d8a93d4c74" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "031c0410-2d51-4340-8dee-9e0e660924b7", "AQAAAAIAAYagAAAAEGSjKcfzpYuEihC5vScsTFS7IMi01ikoiqj7qgNKuY8DK8fOQOyO1lSN05OS6sEmnQ==", "82f279bd-28bc-433b-9127-22dc7c4e0dac" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "96e05335-43ea-48d9-bc4a-31e89b3b4099", "AQAAAAIAAYagAAAAEAFc/CBgkZeZEi+MaXbT9xegI5VHIgLkHZ4T0yzmGWzIzUc3XqjHQ2f9LII46tsV6Q==", "358b0b74-1eb2-4a7e-bf5b-f4fd6de8694b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "71fd5b6d-0654-47a5-b776-b9070dd61d3f", "AQAAAAIAAYagAAAAEI9qBDzEM0dcsZLagqvZKPvrmwFhFbAChyaW/M29FiO8PLdYErSmJf252cE0tHtIyg==", "3397f4ad-60ac-45af-ae12-6e270cb852d7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ce28021f-6757-42a8-95a5-7d9095fe131b", "AQAAAAIAAYagAAAAEE7qpUf7PNXyJ0sPMRrs12bX4tDFWX8RECdjpagoY4cToRYhtaLAJ+p8OxKeuPUHMg==", "1504ec85-ae7d-477f-8fb1-e34f7a2ac476" });
        }
    }
}
