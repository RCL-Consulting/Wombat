using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class SpecialitiesMod3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "37d4fc4d-256f-4e6e-8772-83375a2cf7c5", "AQAAAAIAAYagAAAAEChViGtmzgHgficCIUDtY1IVzYuCG89JzZnSZwQ9SIpzNJB6TU0wvXwGy0JHRDus0Q==", "e6b451f9-7736-4470-8f90-3b57cb893416" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "29854e13-7fd5-4c00-809d-bff3a343aff9", "AQAAAAIAAYagAAAAEGtX9nvA6FvrDCU6b1xhU9mqC0xqz2uU04ErHUphJ4qLY4A0aeb73OW+eFzE1kBDsA==", "9ee20f89-0f8f-4462-85a3-d18d128baeac" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2454760b-97c5-4881-a332-34346c2fa53c", "AQAAAAIAAYagAAAAEJ7Ay0rP1xxsOGiDGpV034c4svH1T9zP/HJJ0iCAb7qnLUozxLRweoPiT3i6tjHJHw==", "9e7a70b1-a4b9-41cb-b507-420437d71219" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "aa38f72d-8790-484c-a0fe-adced0ce4897", "AQAAAAIAAYagAAAAELxapYfmclQv4m5sKD4bynnVCfN+Ru2Zzou+NcSIzNvoZMgUqJlgi6WDH6hlmDeqSw==", "b9d4a99b-c4ad-4e1f-8ef8-241773f23280" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "64348419-1486-40bf-97cf-139211630c31", "AQAAAAIAAYagAAAAEI4nv/kwXDmuT3n+FapFk3xek9osUEEz4s5bklI7e3j0BzR88AzhYL8L7bcaGznU/A==", "99c9c11a-1094-48f0-9b30-101855b403fa" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ec28083f-4ea5-4f61-af17-4466625e8c2e", "AQAAAAIAAYagAAAAEK1BPLxDFUgmcrDPY40yAI5xdX4i/pWVQk2hYWEpX/mFLzf+fODm9xgFxvp/IgG/qg==", "55e08371-0289-48de-9a58-fdc34342a534" });
        }
    }
}
