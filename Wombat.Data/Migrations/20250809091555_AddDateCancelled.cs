using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDateCancelled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCancelled",
                table: "AssessmentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1b8d6d77-a04a-4928-b65a-bc6b854e71a1", "AQAAAAIAAYagAAAAEC2NBuVpcmrF4g2mHtAbVLenM7qnGRza0C11JlHzkEDqsdLFNX1OK5RdezqTDbOAwQ==", "f28f4411-e22e-4ce7-ab5a-78473803f6e5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7e4559be-33dc-4927-9150-08b6c5e42ba6", "AQAAAAIAAYagAAAAEIXFwroUbJmr38qOcji32+eM0YtY+qEZR8u5AEqebtYyRdi9bJRNaLVue8cWER8ynw==", "35b16a2d-b435-42a8-94b6-5e97649e40ff" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0e90035c-8dd8-45aa-8223-15992d2a32de", "AQAAAAIAAYagAAAAEOIdKr70ZEaMm9c1UOI6FvS9waQqV8OoPdnuAsUPPFWw1s7yN6ik+j5RQ2kxklDgiQ==", "55579ff3-3b43-40ff-b10d-8c7f5659cea4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "219066e6-8a20-4abe-990b-45597baef2e8", "AQAAAAIAAYagAAAAEImA9/wGyR/Cxk1OOo6N58UEqyk+z6XvY5DPJoYShPBADxCJKOxrgiDms11j7XCtfA==", "13d6f300-9eae-4ae3-8d00-36e7ea159695" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCancelled",
                table: "AssessmentRequests");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dd99099a-11a1-44fa-8fb2-0cd215282235", "AQAAAAIAAYagAAAAEMsRzQ8p/RKuXT7GviVPSE3C213QAU7d2466HQSayJIhJCFMkm2q6atXQXKp76+txw==", "e0cae147-504f-4a49-9305-07222df8d00f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "521a773c-4192-427e-9f9d-76523d978b0b", "AQAAAAIAAYagAAAAEOu80QgDrqXIeTI9ipqw+ePFgfYH1Nffd78ns6fYVb1WIqEjXfVhfHnYCe49b22u+A==", "bb027b62-8696-4d39-96e8-ff8f84310e4c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "bdbea8f6-4865-4f78-8a83-75ee2150e6bb", "AQAAAAIAAYagAAAAELB3UizPpFyWJedXe4teeJ+YSiCEhwHDBPmdFHDhnL0a/O5C5TwOC3sPh8yEODNVkQ==", "105cbcff-4c12-4ea3-8a7a-801c48c4df7a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4e9ea5f1-a13e-4063-9b58-e48e27a52cb0", "AQAAAAIAAYagAAAAEKk7IzDDWkMsFOgk6S2vFY9uzhrbFkIetKm6UfdIqrg9qBkbEBnM1TSmMOtfKb1aFw==", "61f35e4c-07ec-4a3f-95d1-5fa67d566be5" });
        }
    }
}
