using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CommitteeReviewType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewType",
                table: "CommitteeReviews",
                type: "integer",
                nullable: false,
                // Existing reviews predate the type field; backfill them to AnnualProgression (1),
                // the default and a valid enum value (the enum has no 0 member).
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewType",
                table: "CommitteeReviews");
        }
    }
}
