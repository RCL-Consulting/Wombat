using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7f9eaac9-32ae-4d81-a126-a7bfa6746ba7", "AQAAAAIAAYagAAAAEHg287hdN+J/Ky8TLmjv2ZNEQfLJHTSCRtOiAuPLspoF1G2W89LZLd6XioFtccOxMA==", "ab58a55c-2a3c-44a8-84ce-ecd4d4672367" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "39b4b340-5d0f-4807-9077-0b77980b1543", "AQAAAAIAAYagAAAAEA5YqfnzsCFxKAR4TGxp2QFNt5gPEK/Sf2gxz4ozlueTUBF7zMtjNdBgzn8/j9qKPw==", "2881a754-d206-47cc-b9ba-bb868fa3830a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "50ac9168-9393-445a-a755-d9844bcd28e8", "AQAAAAIAAYagAAAAEArugqglp8YjJLyW38WQk8QOb0sBiIbh/ke3njgHm9RBijfA8Vg8luLtPTRWyR4gTQ==", "da93a81e-7274-4d77-b619-0d9d96ef1027" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "42fa96f5-a733-4e46-9266-cf17680609f2", "AQAAAAIAAYagAAAAEJaV7yXY06ZE7dvvBo6RDfLOLhuwIeeDF6468e9p8Q4dOw+uha3sRTdQcfGe/B9L/A==", "279ea27a-5732-42dc-a93f-eb8ac1be5de6" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "79d857d2-35f4-422b-be17-5cd1dc050e7b", "AQAAAAIAAYagAAAAEIvH+eNJ6mnchdoFXeRsFO/JniikP3NMb4exhYPORLpuJVUVHsQfqBwtfH/SRj4XFg==", "663a078b-90eb-4241-915a-29bcd24f07c8" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "961561d6-04d5-455e-9731-b8b8b3334522", "AQAAAAIAAYagAAAAEI7jamR2T8bimOSSK7NFLKEs6Cv62VyVVkbJltbmR1jyC8eDahuqrUbAh5RBFHzPfA==", "a040f234-53c7-49f4-b799-f66b66c3d7bd" });
        }
    }
}
