using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Migrations
{
    /// <inheritdoc />
    public partial class ChangedFieldName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OptionCriterionResponses_LoggedAssessments_LoggedAssessmentId",
                table: "OptionCriterionResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionCriterionResponses_OptionCriteria_OptionCriterionId",
                table: "OptionCriterionResponses");

            migrationBuilder.DropIndex(
                name: "IX_OptionCriterionResponses_LoggedAssessmentId",
                table: "OptionCriterionResponses");

            migrationBuilder.DropColumn(
                name: "LoggedAssessmentId",
                table: "OptionCriterionResponses");

            migrationBuilder.RenameColumn(
                name: "OptionCriterionId",
                table: "OptionCriterionResponses",
                newName: "CriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_OptionCriterionResponses_OptionCriterionId",
                table: "OptionCriterionResponses",
                newName: "IX_OptionCriterionResponses_CriterionId");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "61290848-8a4b-4f0a-a8bc-e794a589913a", "AQAAAAIAAYagAAAAENmAIy6ur637jbRLf7FM1+XmHfW4zU3dv6zO/fzwKSFoi0Y1J7FRr9zAYVuampJQ5Q==", "d49d792f-4aee-400d-a105-2d9a1a4f5b9e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "387356c2-cac0-438b-b867-b470f5f74fc0", "AQAAAAIAAYagAAAAEEa+et1cLs6xpxO/2I0N20A5npl7tnhH3biUSjhJtfPVUdihctV6lwym361U48LenQ==", "b1a83868-b442-4473-bf39-88af64c1cdf1" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "c229b23c-d7e8-4bdf-9ec9-15068f40bde4", "AQAAAAIAAYagAAAAEP8hi5ccaZb80LFtJ2ldW5DT/huXcgw53xuC5PxVI0kipTdyNLVmiHZg/RvfyppKqw==", "5b06156f-561c-465a-b66e-5205a2e2da1d" });

            migrationBuilder.CreateIndex(
                name: "IX_OptionCriterionResponses_AssessmentId",
                table: "OptionCriterionResponses",
                column: "AssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_OptionCriterionResponses_LoggedAssessments_AssessmentId",
                table: "OptionCriterionResponses",
                column: "AssessmentId",
                principalTable: "LoggedAssessments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OptionCriterionResponses_OptionCriteria_CriterionId",
                table: "OptionCriterionResponses",
                column: "CriterionId",
                principalTable: "OptionCriteria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OptionCriterionResponses_LoggedAssessments_AssessmentId",
                table: "OptionCriterionResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_OptionCriterionResponses_OptionCriteria_CriterionId",
                table: "OptionCriterionResponses");

            migrationBuilder.DropIndex(
                name: "IX_OptionCriterionResponses_AssessmentId",
                table: "OptionCriterionResponses");

            migrationBuilder.RenameColumn(
                name: "CriterionId",
                table: "OptionCriterionResponses",
                newName: "OptionCriterionId");

            migrationBuilder.RenameIndex(
                name: "IX_OptionCriterionResponses_CriterionId",
                table: "OptionCriterionResponses",
                newName: "IX_OptionCriterionResponses_OptionCriterionId");

            migrationBuilder.AddColumn<int>(
                name: "LoggedAssessmentId",
                table: "OptionCriterionResponses",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b56cc21b-5706-4443-b3f5-b8a552761355", "AQAAAAIAAYagAAAAEAv3WRapfPj5kO09x31UquJYZUuHpIF1aJ//myB4MDeeYI1aBjxfjnw+9KXQhofx3g==", "c6991d8c-9cb1-4726-8cb2-efd9450822cd" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "06a213ed-005c-4edd-ac12-b639b6c83b0a", "AQAAAAIAAYagAAAAEMNWyjPsnhkT1E5BZ1K2URj2OWLWSLhg5/OY7d5BoO+P45qJj9G/S20OkkGjBF59SQ==", "33bcf595-e514-4f7c-addf-a5000c1f63f8" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "91513e2b-bef6-4ed1-8359-872b5225bd11", "AQAAAAIAAYagAAAAEK7SpQhbscH4JsRAeQOftp0Ed6/xwQvhnwecQ056suqm6AS6zQuv3JonjP46knOt/g==", "33926269-2b28-482a-aa94-637bf3bafb9f" });

            migrationBuilder.CreateIndex(
                name: "IX_OptionCriterionResponses_LoggedAssessmentId",
                table: "OptionCriterionResponses",
                column: "LoggedAssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_OptionCriterionResponses_LoggedAssessments_LoggedAssessmentId",
                table: "OptionCriterionResponses",
                column: "LoggedAssessmentId",
                principalTable: "LoggedAssessments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OptionCriterionResponses_OptionCriteria_OptionCriterionId",
                table: "OptionCriterionResponses",
                column: "OptionCriterionId",
                principalTable: "OptionCriteria",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
