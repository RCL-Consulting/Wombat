using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class NothingChanged : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6fb2caf2-dd96-4a4a-8c28-007113c169f3", "AQAAAAIAAYagAAAAEOYg8kq3l+bgjuK12qKovYJ3bgJ69fs41eXUEjttX5jpLxp0pp7RgzG2/diUc7JJhQ==", "d48f07e9-f757-4f11-b19b-a6993d403f8c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "13851607-1e6d-4738-92d1-2d8042a0c114", "AQAAAAIAAYagAAAAEI7GDhCmg34dVYqS/C1hRyylaOkmxGIyywWtAT88dq8Pjz3HBy2+Rftgylv94zIBVQ==", "51c487d8-0ec3-4c00-9d0b-88abebb61a07" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fdacab3f-dcbd-418f-a661-2b821330bf84", "AQAAAAIAAYagAAAAEJbrz0m+KbgRZEwzlxlG4MdpK9ix5+kPq8YHnR9G5m4c1lIZx4dnqqoY9gLZ8lyD3w==", "699b4b6d-d0a2-415c-befc-66d9dad5eebf" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "926b790e-d6c7-4467-a9e4-50525ecf3719", "AQAAAAIAAYagAAAAECKTEWDMfVIYGd2m5H4sEr5TkE798lEleZ2bnnakh+G0gQyKptkISikzE2Xj8gKZ0w==", "f250e0c0-beb9-425c-85e2-7614541c062b" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "418f6316-7307-4ac4-b135-818d64dc5f8c", "AQAAAAIAAYagAAAAEGa6HybGPORKQxnVmc3uaJyL8PmKMIGDM95ZRP9b8Yt5dQEhJ6G6/CvxY7Fq1j9Zmw==", "5cc07841-27ff-4689-91c0-90272bd43229" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "77b5386a-6a95-4d1a-923a-7b977319cde6", "AQAAAAIAAYagAAAAEMZJpo+HczwSqyQ5XqJW0r+qdgywIsNEeN/3JfYZrjDPoCYexUAhJ0G867dgAZZHOg==", "5c76dedd-6847-48ca-a711-528c872b0024" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ca1d0f30-0ba1-4a18-81cf-f0191261e9af", "AQAAAAIAAYagAAAAEDssE3H1ExWjpuSPGr+kKsdPk6/6xVYTGvoxeBSACPIkARwU+AiyYMbNfTkHeJyEng==", "144255db-32e5-4dee-8913-162ae35970d6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "68360ad8-c4aa-4987-8123-2290f492166b", "AQAAAAIAAYagAAAAELnD1/RCaO0zib2xF5wb1ncqXQtnkr/P0i6KiAgnjD2zFVUrv32NhkWDO/JwLdtNoQ==", "07d64ee7-0be4-4fce-a78d-a82deb1bac24" });
        }
    }
}
