using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAssessmentName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "52ac863d-409f-481b-a654-9c068377d0cc", "AQAAAAIAAYagAAAAEKL2yostPhir2oBNCaR5dt6Cd8roKjNyPSf/rLZmAuCiekfNX6HZSQscWFm26i4xeA==", "efddbd20-4e6a-4273-b922-9aafc43a18c9" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "649ce986-ce19-484e-be56-dbaf2cd3daee", "AQAAAAIAAYagAAAAEEogWsbfFfrFB5NVCaIoe7HPkSYaAANVyPQ+J0Isnz2uUhPrvFadhfiqwIPaTxshqA==", "a698602a-6272-49f6-be30-4810f6fa3a24" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7361f54f-29e1-4be2-bd38-101034bd5c17", "AQAAAAIAAYagAAAAEPtqeBmKjavFWhvIzY13ttDp1XG0T0o51VcwNzrU9u5yvKZ7eNvJdXB59CsfBQ7Y2Q==", "5356149e-3388-4920-9f49-4aa628274cbe" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "fda66001-d85d-4e15-9829-237b937a0dd0", "AQAAAAIAAYagAAAAEA+4PCUfnfxT3uIJjdpL7FtSz04kqc5PViAO1Z6xI9uXj0mt5+nD/54BZP0K3Lm/Og==", "1685029d-4ebf-4395-a814-e2b2c7e4c2fd" });
        }
    }
}
