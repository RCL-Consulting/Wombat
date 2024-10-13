using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class Specialities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OptionCriteria_AssessmentTemplates_AssessmentTemplateId",
                table: "OptionCriteria");

            migrationBuilder.DropForeignKey(
                name: "FK_SubSpecialities_Specialities_SpecialityId",
                table: "SubSpecialities");

            migrationBuilder.AddColumn<bool>(
                name: "CanDeleteFromList",
                table: "SubSpecialities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SubSpecialityId",
                table: "EPAs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "d16724aa-16b8-4f1a-abb3-fb20e913a3ac", "AQAAAAIAAYagAAAAEEWfbIIKBkO0LJcEcNEoDIONI7TLkdGaPM8sYfXjETa9LPWJl4cbvu8A4GanvRu+0w==", "ed40950f-f22b-4e96-86a0-bcc5416bdd1c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c900983f-657f-4f2a-b1d0-88d24b3f140e", "AQAAAAIAAYagAAAAEJ6auOukzlQK7tl4Da8ahzNDJiJDPIrb/2XxRP1eYedGVcDMO/fqxq6ht6hagksNmQ==", "0af9f90b-ef6d-4731-942c-4f0a6f5da4b4" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "0f87f11b-63c1-4c95-b940-d8d131f77e93", "AQAAAAIAAYagAAAAEJ/CCdUq8lMEOELYhv9VnpngXfOWI8Ff/pby214Vf7XRSjCZy/5Fnb1IO6ktB29nTQ==", "10ee6ad3-2d4d-4c56-8f4e-3dcc60e0b89b" });

            migrationBuilder.InsertData(
                table: "Specialities",
                columns: new[] { "Id", "DateCreated", "DateModified", "Name" },
                values: new object[] { 1, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Paediatrics" });

            migrationBuilder.CreateIndex(
                name: "IX_EPAs_SubSpecialityId",
                table: "EPAs",
                column: "SubSpecialityId");

            migrationBuilder.AddForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs",
                column: "SubSpecialityId",
                principalTable: "SubSpecialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OptionCriteria_AssessmentTemplates_AssessmentTemplateId",
                table: "OptionCriteria",
                column: "AssessmentTemplateId",
                principalTable: "AssessmentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubSpecialities_Specialities_SpecialityId",
                table: "SubSpecialities",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EPAs_SubSpecialities_SubSpecialityId",
                table: "EPAs");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionCriteria_AssessmentTemplates_AssessmentTemplateId",
                table: "OptionCriteria");

            migrationBuilder.DropForeignKey(
                name: "FK_SubSpecialities_Specialities_SpecialityId",
                table: "SubSpecialities");

            migrationBuilder.DropIndex(
                name: "IX_EPAs_SubSpecialityId",
                table: "EPAs");

            migrationBuilder.DeleteData(
                table: "Specialities",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "CanDeleteFromList",
                table: "SubSpecialities");

            migrationBuilder.DropColumn(
                name: "SubSpecialityId",
                table: "EPAs");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "7553e95c-49f1-4d1d-bbeb-741d42e2f97e", "AQAAAAIAAYagAAAAEFk5OHD+r48WsxsR+bFoHriPL+kXqw0adurkZlpyc2ewjgBiuQmnsAhl8uu7HQonvQ==", "b1b550a2-d898-4faa-aca5-1e87a94eb995" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "72150f2c-f7ed-41bd-a61c-08833157438d", "AQAAAAIAAYagAAAAEMm4+xyaNyyRMaPAWlMPd9/XSttJHQH+VBsNc7alzrxbUa8Je2/qq8ow86onEWRPsw==", "3d13e857-144f-48d3-b979-63ddba52a8d0" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "485c83ea-6d42-477b-b45b-d68a664fb754", "AQAAAAIAAYagAAAAEKaFzJm4busW73vjtHC/Ceyf//zZd3SmlnUoaCBpXVKqmr2vcWY5E6g6UypfdfCsWw==", "3663d175-0c3c-4f90-9f9a-d35f1a9fedf9" });

            migrationBuilder.AddForeignKey(
                name: "FK_OptionCriteria_AssessmentTemplates_AssessmentTemplateId",
                table: "OptionCriteria",
                column: "AssessmentTemplateId",
                principalTable: "AssessmentTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubSpecialities_Specialities_SpecialityId",
                table: "SubSpecialities",
                column: "SpecialityId",
                principalTable: "Specialities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
