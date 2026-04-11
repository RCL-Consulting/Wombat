using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Profiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssessorProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    Qualifications = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    InstitutionId = table.Column<int>(type: "integer", nullable: false),
                    SpecialityId = table.Column<int>(type: "integer", nullable: true),
                    SubSpecialityId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessorProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssessorProfiles_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessorProfiles_Specialities_SpecialityId",
                        column: x => x.SpecialityId,
                        principalTable: "Specialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AssessorProfiles_SubSpecialities_SubSpecialityId",
                        column: x => x.SubSpecialityId,
                        principalTable: "SubSpecialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TraineeProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CurriculumId = table.Column<int>(type: "integer", nullable: false),
                    ProgrammeStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpectedCompletionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraineeProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraineeProfiles_Curricula_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curricula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessorProfiles_InstitutionId",
                table: "AssessorProfiles",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessorProfiles_SpecialityId",
                table: "AssessorProfiles",
                column: "SpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessorProfiles_SubSpecialityId",
                table: "AssessorProfiles",
                column: "SubSpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessorProfiles_UserId",
                table: "AssessorProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProfiles_CurriculumId",
                table: "TraineeProfiles",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProfiles_UserId",
                table: "TraineeProfiles",
                column: "UserId",
                unique: true,
                filter: "\"IsActive\" = TRUE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessorProfiles");

            migrationBuilder.DropTable(
                name: "TraineeProfiles");
        }
    }
}
