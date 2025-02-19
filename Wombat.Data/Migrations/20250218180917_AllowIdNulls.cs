using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowIdNulls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IdNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "99f9429c-8d27-45bd-802c-e7d8df89f1ac", "AQAAAAIAAYagAAAAEEaGC3tGASaGjhTv9/58X3Mo/8IOoe3v7cnllNLnHRbQo8krUvbc0QN33Yt86AITXA==", "fa7ae374-d62e-451b-8cea-57ef30bb94e5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b77abed7-77dc-4628-954e-e9a22db566e7", "AQAAAAIAAYagAAAAEDD9nKLfB5q9ZstO4blykugdUsAbaIcAHMkW6+FrVWsrr9B8kBKi+uFhFXwn5IYT8A==", "5dc812a3-7463-4fd0-a7cd-3b26eab47fb2" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "272f208b-8485-47aa-bbcc-f48a9234a1e7", "AQAAAAIAAYagAAAAENX2l2T4u3PKydtJ0UKG/TIn/mxeO5Auq4LBp3LEtHQ4FfnglhC/ModcLV/VO8aw/g==", "bf1910f7-b180-4561-a481-93c44a65b96e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d9db0a63-77b1-496e-9708-0511dfaf1359", "AQAAAAIAAYagAAAAECG0gCc7fLk7pOU/d1Ug1HtkYRfPUB8P5TuZGXW5GbEKZSmZxhOL7C5ujw1Ov8wsJQ==", "3ec966a1-3afb-4e3d-a0e5-6052d270ffed" });
        }
    }
}
