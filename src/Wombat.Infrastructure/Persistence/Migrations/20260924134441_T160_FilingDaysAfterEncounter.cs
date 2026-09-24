using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T160, D15: the filing's history row records how many days after its stated encounter it was filed. Nullable and
    /// not backfilled: null is the honest value for every move that is not a filing, and for every filing recorded
    /// before this column existed.
    /// </summary>
    public partial class T160_FilingDaysAfterEncounter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DaysAfterEncounter",
                table: "ActivityTransitions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysAfterEncounter",
                table: "ActivityTransitions");
        }
    }
}
