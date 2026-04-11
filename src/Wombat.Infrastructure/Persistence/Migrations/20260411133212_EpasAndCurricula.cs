using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class EpasAndCurricula : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Curricula",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubSpecialityId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Curricula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Curricula_SubSpecialities_SubSpecialityId",
                        column: x => x.SubSpecialityId,
                        principalTable: "SubSpecialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntrustmentScales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrustmentScales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Epas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SubSpecialityId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    RequiredKnowledgeSkills = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Epas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Epas_SubSpecialities_SubSpecialityId",
                        column: x => x.SubSpecialityId,
                        principalTable: "SubSpecialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstitutionId = table.Column<int>(type: "integer", nullable: true),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true),
                    SubSpecialityId = table.Column<int>(type: "integer", nullable: true),
                    ScaleId = table.Column<int>(type: "integer", nullable: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessmentForms_EntrustmentScales_ScaleId",
                        column: x => x.ScaleId,
                        principalTable: "EntrustmentScales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentForms_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentForms_Specialities_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Specialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessmentForms_SubSpecialities_SubSpecialityId",
                        column: x => x.SubSpecialityId,
                        principalTable: "SubSpecialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntrustmentLevels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ScaleId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntrustmentLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntrustmentLevels_EntrustmentScales_ScaleId",
                        column: x => x.ScaleId,
                        principalTable: "EntrustmentScales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurriculumId = table.Column<int>(type: "integer", nullable: false),
                    EpaId = table.Column<int>(type: "integer", nullable: false),
                    RequiredCount = table.Column<int>(type: "integer", nullable: false),
                    MinimumLevelOrder = table.Column<int>(type: "integer", nullable: false),
                    WindowMonths = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumItems_Curricula_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curricula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurriculumItems_Epas_EpaId",
                        column: x => x.EpaId,
                        principalTable: "Epas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormId = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    HelpText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormCriteria_AssessmentForms_FormId",
                        column: x => x.FormId,
                        principalTable: "AssessmentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormEpaLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormId = table.Column<int>(type: "integer", nullable: false),
                    EpaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormEpaLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormEpaLinks_AssessmentForms_FormId",
                        column: x => x.FormId,
                        principalTable: "AssessmentForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormEpaLinks_Epas_EpaId",
                        column: x => x.EpaId,
                        principalTable: "Epas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentForms_InstitutionId",
                table: "AssessmentForms",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentForms_ScaleId",
                table: "AssessmentForms",
                column: "ScaleId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentForms_SpecialityId",
                table: "AssessmentForms",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentForms_SubSpecialityId_Name",
                table: "AssessmentForms",
                columns: new[] { "SubSpecialityId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_Curricula_SubSpecialityId_Name_Version",
                table: "Curricula",
                columns: new[] { "SubSpecialityId", "Name", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumItems_CurriculumId_EpaId",
                table: "CurriculumItems",
                columns: new[] { "CurriculumId", "EpaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumItems_EpaId",
                table: "CurriculumItems",
                column: "EpaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentLevels_ScaleId_Order",
                table: "EntrustmentLevels",
                columns: new[] { "ScaleId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentScales_Name",
                table: "EntrustmentScales",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Epas_SubSpecialityId_Code",
                table: "Epas",
                columns: new[] { "SubSpecialityId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormCriteria_FormId_Order",
                table: "FormCriteria",
                columns: new[] { "FormId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormEpaLinks_EpaId",
                table: "FormEpaLinks",
                column: "EpaId");

            migrationBuilder.CreateIndex(
                name: "IX_FormEpaLinks_FormId_EpaId",
                table: "FormEpaLinks",
                columns: new[] { "FormId", "EpaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurriculumItems");

            migrationBuilder.DropTable(
                name: "EntrustmentLevels");

            migrationBuilder.DropTable(
                name: "FormCriteria");

            migrationBuilder.DropTable(
                name: "FormEpaLinks");

            migrationBuilder.DropTable(
                name: "Curricula");

            migrationBuilder.DropTable(
                name: "AssessmentForms");

            migrationBuilder.DropTable(
                name: "Epas");

            migrationBuilder.DropTable(
                name: "EntrustmentScales");
        }
    }
}
