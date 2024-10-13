using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutins : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs");

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Logo = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1ce88fc8-7017-47ad-983b-154914ac4130", "AQAAAAIAAYagAAAAELZrxlRsJEyz2s7y5BcPOD6erzlJ988flNML3CjvrxM2SpmeX3wKRAuvOn4bOtaOgw==", "6691f577-6fdb-41ac-b1f9-8385b403276a" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a6b12f15-6cbf-44f5-8cdc-dcd8a069a70b", "AQAAAAIAAYagAAAAEMfPXVd23wV/IETh6Voq4Xw1gm0g+Rfap6SMqiOK8r+Wft3I3FmyVQ5TrOQViKP1Qg==", "949af086-e454-4b7b-b897-0ed281d33860" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4f3c063a-f8cf-4b47-bbbb-88fc6b06dd10", "AQAAAAIAAYagAAAAEMFWotlUCtTomNP2Vz8M5rM5pAOV5E6QBC0OeaA1fAWCipVL+nrvXaVsQeYL7tIFjg==", "ba6307bd-3afb-4b4d-943e-6592fd9954e5" });

            migrationBuilder.AddForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs",
                column: "SubSpecialityId",
                principalTable: "SubSpecialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6dddd717-769e-413a-ad70-08f1295f2d52", "AQAAAAIAAYagAAAAENyqWEfImE9G1qlXvEiiPUQt4Sk8JtfYSAvUlm4fVDMn+VzvOv68MD/ne8NCqGD2SA==", "210f861e-c1f8-4884-92b9-b22f697cf938" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "335a3b4e-21ef-4bd8-af2b-f4a812dbe217", "AQAAAAIAAYagAAAAEFCisH5dXdmzLOiR3cK3y8zfdyJhATYw37+BpWZVnda9ihYoWJYLqwAMy0Xfjo07LQ==", "d867384c-cabf-4af5-bf0a-47c9e6fc2e82" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6607944e-2087-4f27-811c-f7b0eba6c4cf", "AQAAAAIAAYagAAAAEM2y9LXP4hh27+Di5taW/1jxn5RnUqgdZwl9ISHwWtlxqfJnFluSDOWBtLDHT8BCtg==", "25e29462-3467-4dfe-97dd-53c705c6da40" });

            migrationBuilder.AddForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs",
                column: "SubSpecialityId",
                principalTable: "SubSpecialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
