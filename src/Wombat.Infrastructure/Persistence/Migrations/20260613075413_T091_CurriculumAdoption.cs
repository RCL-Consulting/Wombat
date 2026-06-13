using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T091_CurriculumAdoption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdoptionId",
                table: "TraineeProfiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InstitutionCurriculumAdoptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InstitutionId = table.Column<int>(type: "integer", nullable: false),
                    CurriculumId = table.Column<int>(type: "integer", nullable: false),
                    SubSpecialityId = table.Column<int>(type: "integer", nullable: false),
                    AdoptedOn = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionCurriculumAdoptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionCurriculumAdoptions_Curricula_CurriculumId",
                        column: x => x.CurriculumId,
                        principalTable: "Curricula",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstitutionCurriculumAdoptions_Institutions_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Institutions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstitutionCurriculumAdoptions_SubSpecialities_SubSpecialit~",
                        column: x => x.SubSpecialityId,
                        principalTable: "SubSpecialities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraineeProfiles_AdoptionId",
                table: "TraineeProfiles",
                column: "AdoptionId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionCurriculumAdoptions_CurriculumId",
                table: "InstitutionCurriculumAdoptions",
                column: "CurriculumId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionCurriculumAdoptions_InstitutionId_SubSpecialityId",
                table: "InstitutionCurriculumAdoptions",
                columns: new[] { "InstitutionId", "SubSpecialityId" },
                unique: true,
                filter: "\"IsActive\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionCurriculumAdoptions_SubSpecialityId",
                table: "InstitutionCurriculumAdoptions",
                column: "SubSpecialityId");

            migrationBuilder.AddForeignKey(
                name: "FK_TraineeProfiles_InstitutionCurriculumAdoptions_AdoptionId",
                table: "TraineeProfiles",
                column: "AdoptionId",
                principalTable: "InstitutionCurriculumAdoptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraineeProfiles_InstitutionCurriculumAdoptions_AdoptionId",
                table: "TraineeProfiles");

            migrationBuilder.DropTable(
                name: "InstitutionCurriculumAdoptions");

            migrationBuilder.DropIndex(
                name: "IX_TraineeProfiles_AdoptionId",
                table: "TraineeProfiles");

            migrationBuilder.DropColumn(
                name: "AdoptionId",
                table: "TraineeProfiles");
        }
    }
}
