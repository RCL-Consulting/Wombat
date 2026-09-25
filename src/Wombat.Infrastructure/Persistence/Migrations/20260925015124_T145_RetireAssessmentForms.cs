using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T145: retires the legacy assessment-forms feature. Its EPA links restricted nothing: no activity, credit rule or
    /// picker ever read <c>AssessmentForms</c>, <c>FormCriteria</c> or <c>FormEpaLinks</c>. Which instrument may assess an
    /// EPA is the curriculum item's permitted-tools list against the activity type's WBA tool key (T122). Down restores
    /// the three tables empty.
    /// </summary>
    public partial class T145_RetireAssessmentForms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormCriteria");

            migrationBuilder.DropTable(
                name: "FormEpaLinks");

            migrationBuilder.DropTable(
                name: "AssessmentForms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessmentForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstitutionId = table.Column<int>(type: "integer", nullable: true),
                    ScaleId = table.Column<int>(type: "integer", nullable: false),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true),
                    SubSpecialityId = table.Column<int>(type: "integer", nullable: true),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
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
                name: "FormCriteria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FormId = table.Column<int>(type: "integer", nullable: false),
                    HelpText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Prompt = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
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
                    EpaId = table.Column<int>(type: "integer", nullable: false),
                    FormId = table.Column<int>(type: "integer", nullable: false)
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
    }
}
