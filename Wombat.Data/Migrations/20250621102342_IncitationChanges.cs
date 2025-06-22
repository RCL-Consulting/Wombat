using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class IncitationChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Institutions_InstitutionId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationInvitations_Institutions_InstitutionId",
                table: "RegistrationInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationInvitations_Specialities_SpecialityId",
                table: "RegistrationInvitations");

            migrationBuilder.AlterColumn<int>(
                name: "SpecialityId",
                table: "RegistrationInvitations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "InstitutionId",
                table: "RegistrationInvitations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "InstitutionId",
                table: "AspNetUsers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "88c7f5e9-becb-4439-a311-4d36075a970d", "AQAAAAIAAYagAAAAELbYvUzENkpKKoqRSm/dZBjz6R9/1nW8pR5AO1dTqBsxUgQfONIdU4KS8xpGWj72tg==", "34fa1195-81a0-42b8-8392-6379502076ae" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "02aae754-f627-4867-9ca8-9465c23dd3bc", "AQAAAAIAAYagAAAAEFkBdrNtHrmZOz0x7ZDKAPAFDA2V2aU1ls5rKSjzk1AKn+ANGeMggISJzBJm8UcqbA==", "d6e06eda-7a2a-48d7-a9cd-46c96f7b1edc" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "834b35a6-0703-4a98-af67-6e7bffa5ec30", "AQAAAAIAAYagAAAAEMPtvI4P4Ldv3J30OZTA6SXQgKHihERpbBqL2aGaMR1oPyrdXiVbZhVJKDxTfCcIRg==", "39bedfc3-0c27-46fa-9ad9-c7a385a6e7ad" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7d9c4968-fd05-476a-bc8a-aa7a6c9fbcee", "AQAAAAIAAYagAAAAEMxNkctiq6mybwGwqyZiX65iHzhQfb6jwfUuR4e6DWzneoLgW3s1hDkh3vxGKIvVvg==", "8ed3f052-ad14-409a-b5c5-e3c37a8a78d1" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Institutions_InstitutionId",
                table: "AspNetUsers",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationInvitations_Institutions_InstitutionId",
                table: "RegistrationInvitations",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationInvitations_Specialities_SpecialityId",
                table: "RegistrationInvitations",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Institutions_InstitutionId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationInvitations_Institutions_InstitutionId",
                table: "RegistrationInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationInvitations_Specialities_SpecialityId",
                table: "RegistrationInvitations");

            migrationBuilder.AlterColumn<int>(
                name: "SpecialityId",
                table: "RegistrationInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InstitutionId",
                table: "RegistrationInvitations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InstitutionId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f4a9815d-35a6-432a-a6e8-84d9d5ccbb23", "AQAAAAIAAYagAAAAEMJvj8yw6hE/Yo0nrVLMoi6kzgtc7kEd8TjJK1vDK0EMLcn1dU2CDNGcBrHO4uoTiw==", "718029fd-f355-42e9-8136-8be7230ec166" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "26fb6b96-f2ef-47d5-91d1-b1da151c620b", "AQAAAAIAAYagAAAAEBxdDPYHO3Im5TB2CHARKeZY8Gmbs6+ACmuYcvQau/LRrxjcwvyDbxR7weaHPRgXhg==", "6c9be54b-f79f-4954-9f44-16c578a6761c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "add7ca81-f467-4829-9a17-6cab3f9c9e42", "AQAAAAIAAYagAAAAEAH91rQvzmjQ7m6r/HzpzfHG4+cyDzPZU/KMgYsnT1L40aiYuXmPAgdBDGLF+9jYTQ==", "64829fe8-29e4-4776-9e3b-0c6eba0b0e27" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d3847961-0552-40e5-91fe-0aea570727e9", "AQAAAAIAAYagAAAAEPqznPo9nGYoRkAdfHzxV6A86C/sEnj4O3Y7zVrnUVK01vS3DXn+xipXb0IsQqFTlQ==", "34f649f4-1855-4a6c-9fcf-e2eb03305f32" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Institutions_InstitutionId",
                table: "AspNetUsers",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationInvitations_Institutions_InstitutionId",
                table: "RegistrationInvitations",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationInvitations_Specialities_SpecialityId",
                table: "RegistrationInvitations",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
