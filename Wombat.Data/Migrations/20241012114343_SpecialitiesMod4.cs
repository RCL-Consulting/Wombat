using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class SpecialitiesMod4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs");

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

            migrationBuilder.AddForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs",
                column: "SubSpecialityId",
                principalTable: "SubSpecialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
