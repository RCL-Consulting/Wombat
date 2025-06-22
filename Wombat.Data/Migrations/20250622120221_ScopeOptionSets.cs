using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class ScopeOptionSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InstitutionId",
                table: "OptionSets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialityId",
                table: "OptionSets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SubSpecialityId",
                table: "OptionSets",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c41715ce-52c0-4921-a66d-f324bd39c693", "AQAAAAIAAYagAAAAEJ/L+/IiQmHxr0AJdzShCUiFUoFMO26fD8lzE9njDrkFjZr9EYvrofZHnJvp2jy6iw==", "f11951fd-479c-4257-a0bb-9f795484b4cd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d3906fd8-34ab-4f29-b87e-f9c66ac87236", "AQAAAAIAAYagAAAAEBS7GgiKuB85omvhKCpyDmDJqXxlG00nfOiPHJ0OxNw/L4YMEdMnST/gqP27uU6Y4Q==", "bd487c9c-d986-4a59-acab-9693807e7484" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "89b2b0dd-e0eb-424e-9e6e-a90b720fa624", "AQAAAAIAAYagAAAAEOum+STBl5xMUWaJ/YkTQ98lIbt00ri4erJsw2QYJ4Q65aDL+OG5xItNhUQ0aKjaSA==", "4b0a8c80-dc47-4eca-9cc2-a95672a43672" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c41f0a36-e464-47dc-9ea6-3b6fe4f86dab", "AQAAAAIAAYagAAAAEHAB9FdOdALFAcCQrhq4h1VU/kEW8ffokKJI5p57/uKHqWJnwQ2T0fEQQsWJw0tKcg==", "77dae253-29d4-43a8-84b2-c3e309750368" });

            migrationBuilder.UpdateData(
                table: "OptionSets",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "InstitutionId", "SpecialityId", "SubSpecialityId" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "OptionSets",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "InstitutionId", "SpecialityId", "SubSpecialityId" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "OptionSets",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "InstitutionId", "SpecialityId", "SubSpecialityId" },
                values: new object[] { null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_OptionSets_InstitutionId",
                table: "OptionSets",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionSets_SpecialityId",
                table: "OptionSets",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionSets_SubSpecialityId",
                table: "OptionSets",
                column: "SubSpecialityId");

            migrationBuilder.AddForeignKey(
                name: "FK_OptionSets_Institutions_InstitutionId",
                table: "OptionSets",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OptionSets_Specialities_SpecialityId",
                table: "OptionSets",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OptionSets_SubSpecialities_SubSpecialityId",
                table: "OptionSets",
                column: "SubSpecialityId",
                principalTable: "SubSpecialities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OptionSets_Institutions_InstitutionId",
                table: "OptionSets");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionSets_Specialities_SpecialityId",
                table: "OptionSets");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionSets_SubSpecialities_SubSpecialityId",
                table: "OptionSets");

            migrationBuilder.DropIndex(
                name: "IX_OptionSets_InstitutionId",
                table: "OptionSets");

            migrationBuilder.DropIndex(
                name: "IX_OptionSets_SpecialityId",
                table: "OptionSets");

            migrationBuilder.DropIndex(
                name: "IX_OptionSets_SubSpecialityId",
                table: "OptionSets");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "OptionSets");

            migrationBuilder.DropColumn(
                name: "SpecialityId",
                table: "OptionSets");

            migrationBuilder.DropColumn(
                name: "SubSpecialityId",
                table: "OptionSets");

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
        }
    }
}
