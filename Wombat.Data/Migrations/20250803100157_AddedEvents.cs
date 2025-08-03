using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    AssessmentRequestId = table.Column<int>(type: "int", nullable: true),
                    LoggedAssessmentId = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentEvents_AspNetUsers_ActorId",
                        column: x => x.ActorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentEvents_AssessmentRequests_AssessmentRequestId",
                        column: x => x.AssessmentRequestId,
                        principalTable: "AssessmentRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssessmentEvents_LoggedAssessments_LoggedAssessmentId",
                        column: x => x.LoggedAssessmentId,
                        principalTable: "LoggedAssessments",
                        principalColumn: "Id");
                });

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
                name: "IX_AssessmentEvents_ActorId",
                table: "AssessmentEvents",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEvents_AssessmentRequestId",
                table: "AssessmentEvents",
                column: "AssessmentRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentEvents_LoggedAssessmentId",
                table: "AssessmentEvents",
                column: "LoggedAssessmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentEvents");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "19A3D40C-9852-43B9-9BEC-B2552FA715F7",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "4e4c0f55-03ca-4536-b589-501d6489951c", "AQAAAAIAAYagAAAAEB+8YDkOMeAv7GfH2j1FebpNZ9Dt+hj1fWl5myK6gyS+8nbQNsfDbbpgD9c7/fWGPQ==", "34a13b2a-f69d-4139-b36c-08e71db6fe57" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5bb85bea-dbe2-4801-a910-001d8a8c94fc", "AQAAAAIAAYagAAAAEOd2SdykiAv/CqRlmhnAQqokIogYRITdY/LpXgnubKfIOAqisHRP/5dEixEjme0r3Q==", "67385b68-f57b-480f-9aa7-3b1e1a4131e0" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "BD92BFFF-A88E-4FDB-9F7D-54E57AB58237",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "97c115b5-b885-45a3-85fa-7351fc290e33", "AQAAAAIAAYagAAAAENdLVA9VLcEBcnNl0YiH5TbI+su2DyLaS2X8r1PVB0DEYC2FJB6OoXWCBVAms10NIA==", "18888262-7513-4895-ae87-0355a8e6342d" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "568b2a48-2414-4535-8017-a68c2db61253", "AQAAAAIAAYagAAAAEI0bBE4xsq8wkexKgaqsUqtKYoYNJDXv/AHPneGw/YwaNb7DpTopr2pU0Zfa9yNlYA==", "bf986c72-5494-41c2-ae5f-8974b55f84a3" });
        }
    }
}
