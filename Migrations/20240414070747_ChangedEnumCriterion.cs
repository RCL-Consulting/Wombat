using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Migrations
{
    /// <inheritdoc />
    public partial class ChangedEnumCriterion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryEnumCriterion");

            migrationBuilder.DropTable(
                name: "CategoryTextCriterion");

            migrationBuilder.DropTable(
                name: "EnumOptions");

            migrationBuilder.DropTable(
                name: "EnumCriteria");

            migrationBuilder.CreateTable(
                name: "AssessmentCategoryTextCriterion",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "int", nullable: false),
                    TextCriteriaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentCategoryTextCriterion", x => new { x.CategoriesId, x.TextCriteriaId });
                    table.ForeignKey(
                        name: "FK_AssessmentCategoryTextCriterion_AssessmentCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "AssessmentCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentCategoryTextCriterion_TextCriteria_TextCriteriaId",
                        column: x => x.TextCriteriaId,
                        principalTable: "TextCriteria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionSetId = table.Column<int>(type: "int", nullable: false),
                    AssessmentCategoryId = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionCriteria_AssessmentCategories_AssessmentCategoryId",
                        column: x => x.AssessmentCategoryId,
                        principalTable: "AssessmentCategories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OptionCriteria_OptionSets_OptionSetId",
                        column: x => x.OptionSetId,
                        principalTable: "OptionSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentCategoryTextCriterion_TextCriteriaId",
                table: "AssessmentCategoryTextCriterion",
                column: "TextCriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionCriteria_AssessmentCategoryId",
                table: "OptionCriteria",
                column: "AssessmentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionCriteria_OptionSetId",
                table: "OptionCriteria",
                column: "OptionSetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentCategoryTextCriterion");

            migrationBuilder.DropTable(
                name: "OptionCriteria");

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

            migrationBuilder.CreateTable(
                name: "EnumCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnumCriteria", x => x.Id);
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
                    EnumCriterionId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateModified = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "409696F3-CA82-4381-A734-38A5EF6AA445",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "101cdc65-be78-4f08-be26-e97f6803fb77", "AQAAAAIAAYagAAAAEMhb1WnijBL8WWIYsexR7Ou7aM2n1VgmbtogKM26Tbv8bfzjw/xk8En7E1H2tmfy6Q==", "3ac82d20-ce7e-4a8f-a754-e516271ca9d5" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "D68AC189-5BB6-4511-B96F-0F8BD55569AC",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "41425dd5-5574-4ef2-a763-9f21933eb53e", "AQAAAAIAAYagAAAAECnNuBaID1iUrd84ctk35j7W9zfv+gH+XLJGetG3R9k3AB6mBWFCM48wEqznlEQmTw==", "5dda0474-5b9b-4b4c-9661-cf089e449b65" });

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
    }
}
