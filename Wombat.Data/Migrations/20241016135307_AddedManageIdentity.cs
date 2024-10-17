using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedManageIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d313726a-157a-4d76-bc76-f691c41a78b1", "AQAAAAIAAYagAAAAECby9Ecce1H+SjDDO7RMG3OFvxB19IwKR47eD2UXzVydir7bw76yYLGztOAtUpqkcw==", "5a97fc8f-2234-4185-b399-bac7b1a9280b" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9eefc6ac-fac9-4cd5-913f-568b309f7671", "AQAAAAIAAYagAAAAEGCsbx/hqrc8Wb2xtspGwaLRAopIHDMMc8QLE7FlcPMkhK/aG2DNrs1PiO3Us7rYEA==", "01f2cf86-1629-4257-9298-162d64885433" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f861aa8b-3ca3-4e6b-93ca-cfc95679ee8e", "AQAAAAIAAYagAAAAEOAMchGj81hW0FL168PrdTJnG0+Iiempx898tK98l75PZbSBd7q0vGcK5wxZ0nj+vA==", "15f4d996-ab44-4bdc-9fe7-5b124ef65780" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8dbedd7b-e575-418d-abaa-5708fb9d482c", "AQAAAAIAAYagAAAAEHrp81grzUV5AS6YLsDKMtkqZTndQlEaFlw/dBwoBROx7c9iPNuqMsN6c68yfwpAnA==", "dc8a03bd-bc52-4852-999c-e19d177d9783" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fabaf2c3-d858-40c7-a1f2-de609d8c40e0", "AQAAAAIAAYagAAAAEHbxM3mvzVqG6XLN5sSXs60Av3Vhu02jxvNnUSqrtbBdXDXh91KdkxKeU9wLwNjFjQ==", "50e11a70-507b-4bb4-8e88-f0838ebb8965" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e1c46101-bbf7-445b-bd65-98c909761ea7", "AQAAAAIAAYagAAAAEHZ/NU9dYmsh4DxLusJpKXFHgcZY74JFHMhuqwtPcwX9vPz2P7dOM4gBe9HJLX1X6w==", "9c4c8eb2-d114-4163-9333-32085612c436" });
        }
    }
}
