using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Who closed an MSF campaign, or null when the hourly auto-close job did (T369, F5). Nullable with no foreign key, as
    /// <c>ReviewedByUserId</c>. Existing rows stay null: nothing is live, and a closed scenario row reads "Closed by itself"
    /// until the replay creates it again.
    /// </summary>
    public partial class T369_MsfCampaignClosedBy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClosedByUserId",
                table: "MsfCampaigns",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedByUserId",
                table: "MsfCampaigns");
        }
    }
}
