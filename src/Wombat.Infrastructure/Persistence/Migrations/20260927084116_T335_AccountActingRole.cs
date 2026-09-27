using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T335, flow 01 (D1, W-010): the acting role is stored with the account, so it follows the person across sign-ins and
    /// the next person on the same browser lands by the role precedence (T317). Null until the person first switches. It
    /// replaces the <c>wombat_preferred_dashboard_role</c> browser cookie, which is no longer read; nothing is carried over
    /// from it, since a browser's cookie names no account (CLAUDE.md § Nothing is live).
    /// </summary>
    public partial class T335_AccountActingRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActingRole",
                table: "AspNetUsers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActingRole",
                table: "AspNetUsers");
        }
    }
}
