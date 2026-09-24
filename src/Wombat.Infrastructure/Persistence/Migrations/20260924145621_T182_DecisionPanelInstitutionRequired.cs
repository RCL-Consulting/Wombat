using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class T182_DecisionPanelInstitutionRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Every panel runs at one institution (T182). Until now a panel was stamped only when an InstitutionalAdmin
            // created it; the committee rules now compare the panel's institution with the trainee's, so an unstamped
            // panel could review nobody. Which institution such a panel belongs to cannot be read off the row, and
            // guessing would hand its reviews to someone, so this stops rather than backfills: stamp or delete each one
            // it names, then restart. The scaffolded default of 0 is gone for the same reason. Dev's one panel was
            // stamped when read on 2026-09-24; production was not read, and if it holds an unstamped panel the deploy
            // stops here and names it.
            migrationBuilder.Sql(
                """
                DO $$
                DECLARE unstamped text;
                BEGIN
                    SELECT string_agg("Id"::text, ', ' ORDER BY "Id") INTO unstamped
                    FROM "DecisionPanels" WHERE "InstitutionId" IS NULL;
                    IF unstamped IS NOT NULL THEN
                        RAISE EXCEPTION 'T182: decision panel(s) % have no institution. Set "DecisionPanels"."InstitutionId" on each, or delete them, before this migration runs.', unstamped;
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "InstitutionId",
                table: "DecisionPanels",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DecisionPanels_InstitutionId",
                table: "DecisionPanels",
                column: "InstitutionId");

            migrationBuilder.AddForeignKey(
                name: "FK_DecisionPanels_Institutions_InstitutionId",
                table: "DecisionPanels",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DecisionPanels_Institutions_InstitutionId",
                table: "DecisionPanels");

            migrationBuilder.DropIndex(
                name: "IX_DecisionPanels_InstitutionId",
                table: "DecisionPanels");

            migrationBuilder.AlterColumn<int>(
                name: "InstitutionId",
                table: "DecisionPanels",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
