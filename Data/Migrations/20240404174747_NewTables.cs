using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnumCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumCriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TextCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextCriteria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryEnumCriterion",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "int", nullable: false),
                    EnumCriteriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryEnumCriterion", x => new { x.CategoriesId, x.EnumCriteriaId });
                    table.ForeignKey(
                        name: "FK_CategoryEnumCriterion_AssessmentCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "AssessmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryEnumCriterion_EnumCriteria_EnumCriteriaId",
                        column: x => x.EnumCriteriaId,
                        principalTable: "EnumCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnumOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    EnumCriterionId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnumOptions_EnumCriteria_EnumCriterionId",
                        column: x => x.EnumCriterionId,
                        principalTable: "EnumCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryTextCriterion",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "int", nullable: false),
                    TextCriteriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTextCriterion", x => new { x.CategoriesId, x.TextCriteriaId });
                    table.ForeignKey(
                        name: "FK_CategoryTextCriterion_AssessmentCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "AssessmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryTextCriterion_TextCriteria_TextCriteriaId",
                        column: x => x.TextCriteriaId,
                        principalTable: "TextCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "dcd70065-4437-4985-8f4f-0287a70aca6a", "AQAAAAIAAYagAAAAEK2nS4p43d5GXsiZ12kVhCIS8NRSjoFevicwWz9NXRCo62QLlCqBhIc8hbuGZWivoQ==", "53df84f4-d9dd-490d-a227-90612a819c1f" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "5cfd8cce-b0d2-41f2-9cc8-e451761acc2c", "AQAAAAIAAYagAAAAENFITKq0hl+SEf6UfYwz5pB4zsAKXTBO4mWcZPdIL5dZ2TY5EDqqYc+8fPm7gAdSXw==", "17b9ea81-dbe8-426c-995f-26d7bcc18fb5" });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryEnumCriterion_EnumCriteriaId",
                table: "CategoryEnumCriterion",
                column: "EnumCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTextCriterion_TextCriteriaId",
                table: "CategoryTextCriterion",
                column: "TextCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_EnumOptions_EnumCriterionId",
                table: "EnumOptions",
                column: "EnumCriterionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryEnumCriterion");

            migrationBuilder.DropTable(
                name: "CategoryTextCriterion");

            migrationBuilder.DropTable(
                name: "EnumOptions");

            migrationBuilder.DropTable(
                name: "TextCriteria");

            migrationBuilder.DropTable(
                name: "EnumCriteria");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "decda88c-26b8-4f48-9083-6341d628b5c1", "AQAAAAIAAYagAAAAECv4Z40b1IM8q4BI7+M7uHxbDzIgXBxukg11SuL1SCE8WslVQ+Oc5gTH4RqDc/s89Q==", "0861f3c5-8315-4bfc-b89e-12f6e6cbc09e" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "825585fc-737b-448e-b46d-c1b7cc569c5a", "AQAAAAIAAYagAAAAEPZWOpuwmdBVrQI3CBjY0huQpM33i/v9JsvqXNb518lCW66Hkb8IjVCRYGS7UdRn+A==", "475b4ebf-5527-42f5-acd5-a8a403e5a382" });
        }
    }
}
