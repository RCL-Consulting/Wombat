using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Migrations
{
    /// <inheritdoc />
    public partial class ChangedOptionCriterion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OptionCriteria_AssessmentCategories_AssessmentCategoryId",
                table: "OptionCriteria");

            migrationBuilder.AlterColumn<int>(
                name: "AssessmentCategoryId",
                table: "OptionCriteria",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3cd1edf9-85b9-49b0-a111-92aaeab76a0d", "AQAAAAIAAYagAAAAECicCyT1dyHJXmkNyAemojJO09bak0oAMgy6XKAU0qU7E1xiXVkL+q+Gn3t5nToxlA==", "471fee56-b2b8-4376-9dd3-d3e566ce4567" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "07aa16b9-d540-4b28-9b50-370bfefcce53", "AQAAAAIAAYagAAAAEAjreF4TeZNIhxhoX6mGSg60wo8xt8V+6TCuA5W9fagkbt5x5/GIkWG6VP2zLmgyxA==", "e398ef56-51c2-4ae1-98be-f5d91a0fc890" });

            migrationBuilder.AddForeignKey(
                name: "FK_OptionCriteria_AssessmentCategories_AssessmentCategoryId",
                table: "OptionCriteria",
                column: "AssessmentCategoryId",
                principalTable: "AssessmentCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OptionCriteria_AssessmentCategories_AssessmentCategoryId",
                table: "OptionCriteria");

            migrationBuilder.AlterColumn<int>(
                name: "AssessmentCategoryId",
                table: "OptionCriteria",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e7bcc6b3-3fa9-4883-9a1f-6009d7d415df", "AQAAAAIAAYagAAAAEJT6pnZ3kOhR3GkOy5USX3EOjWnRNUTqc6qbJCvlvYzZ8KwaQ7ctha0CqUwm2Xt9mg==", "29634868-9ca3-49f9-93af-28765e0228fe" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "171cdbe1-163b-4c08-a6e1-a590a23662e3", "AQAAAAIAAYagAAAAEGoLKZGZAu5Jj2R/84qxZluGeKYdFJL7ClAfKB5C3IDplhUyX1fuOW2b3Xf+QSNtCA==", "1ddc2c15-cd0c-468e-ba94-64ba3cebe89d" });

            migrationBuilder.AddForeignKey(
                name: "FK_OptionCriteria_AssessmentCategories_AssessmentCategoryId",
                table: "OptionCriteria",
                column: "AssessmentCategoryId",
                principalTable: "AssessmentCategories",
                principalColumn: "Id");
        }
    }
}
