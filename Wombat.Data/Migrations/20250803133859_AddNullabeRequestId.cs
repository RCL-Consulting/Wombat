using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNullabeRequestId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoggedAssessments_AssessmentRequestId",
                table: "LoggedAssessments");

            migrationBuilder.AlterColumn<int>(
                name: "AssessmentRequestId",
                table: "LoggedAssessments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a419720d-f3f1-4333-a4f0-1344a0c78f2f", "AQAAAAIAAYagAAAAEI7lakUr7WygLEoJw1iSYofq+Y0nqRMFvIdB7S5EYI8dDkoK61jY/eOgTSKNefkJhQ==", "2a75e658-b5f5-48d1-bfa6-353d5f02de0f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "db3c2794-1df2-4fae-b39f-a0b3f6b62234", "AQAAAAIAAYagAAAAEMvdFLbIh3lm8p6uIfXuWbEV2Pte3rurirMDTgcPaJkkNDAUkEaFSCYduRgSh31ORg==", "6c6c6431-bd10-4e18-a8af-fd0ae0f5fd99" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b6c2745a-0841-413b-ac21-bb1f0915fe68", "AQAAAAIAAYagAAAAEPJdaXhekqKeL+wJUZEnEetDwpH4KqV8luVHbTQGZ5oBS2FF7LzqnX004gZrKSxZOQ==", "406eaf7d-4d2f-496f-ac27-dea9bcf07e62" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e1d9a5ba-5ac4-49bb-8225-1dd58c309881", "AQAAAAIAAYagAAAAEPl2RDbJv8qRM6CFAhpNp6OD6AEYoH4cvBdw3safjDR/SYswVoWeRX+GZBzqhKupEg==", "d49fe532-9223-4d9c-90ae-83741c0ae294" });

            migrationBuilder.CreateIndex(
                name: "IX_LoggedAssessments_AssessmentRequestId",
                table: "LoggedAssessments",
                column: "AssessmentRequestId",
                unique: true,
                filter: "[AssessmentRequestId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoggedAssessments_AssessmentRequestId",
                table: "LoggedAssessments");

            migrationBuilder.AlterColumn<int>(
                name: "AssessmentRequestId",
                table: "LoggedAssessments",
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
                values: new object[] { "dd117cb5-a4ef-4002-be25-c7014d1f7d9d", "AQAAAAIAAYagAAAAEDqlh4UUeXtEqeYfWwzQAOb6XjuooMoQYxCtogg1MJQ/VaS6321PL+kkfrtwViIE6g==", "41e63b43-394f-4e37-86ec-2f18e85c91b7" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "406c683b-936e-4611-a5ab-3ec2df48e507", "AQAAAAIAAYagAAAAEEVh+klmKnCVVZWbcrz4DiXskOhlK7cVxYBs4eLcf2kJmymw2p41JFg/YwEO9QzI3w==", "10371ac0-f72a-4d41-bb75-61e985aa80a5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6d49dd2b-1f89-47b7-a396-b0b0fb421322", "AQAAAAIAAYagAAAAEG+/koqaQR+cqd3c/0qiWvLmCFBa1+9HXW9n4H8cQug7Bt6ao7U/wGT7ddDdxmxydw==", "cd42d97b-8426-4a5b-9d08-d52caafe2f8e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "a586d738-1430-4324-b6f3-4dd4f45b1825", "AQAAAAIAAYagAAAAEDlsV6g+hei0zU0kGjJ02PHnWNWf6aWly0FTvTQew1QHEFPFbEV1vSSii1bDYToByg==", "a35f72fd-a7fb-4ae7-8da1-32a898580c20" });

            migrationBuilder.CreateIndex(
                name: "IX_LoggedAssessments_AssessmentRequestId",
                table: "LoggedAssessments",
                column: "AssessmentRequestId",
                unique: true);
        }
    }
}
