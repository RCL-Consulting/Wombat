using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemovedRequestDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateAccepted",
                table: "AssessmentRequests");

            migrationBuilder.DropColumn(
                name: "DateCancelled",
                table: "AssessmentRequests");

            migrationBuilder.DropColumn(
                name: "DateDeclined",
                table: "AssessmentRequests");

            migrationBuilder.DropColumn(
                name: "DateRequested",
                table: "AssessmentRequests");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "AssessmentRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedAt",
                table: "AssessmentRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "AssessmentRequests");

            migrationBuilder.DropColumn(
                name: "StatusChangedAt",
                table: "AssessmentRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAccepted",
                table: "AssessmentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCancelled",
                table: "AssessmentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDeclined",
                table: "AssessmentRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateRequested",
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
    }
}
