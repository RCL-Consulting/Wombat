using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// An MSF respondent's link names its invitation by a selector, under a unique index, so the respondent page reads
    /// one row instead of every invitation there is. (T163)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The unique index on <c>TokenHash</c> goes: nothing looks a row up by the hash any more, only checks it once the
    /// selector has found the row.
    /// </para>
    /// <para>
    /// <b>Every link mailed before this migration stops working</b>, and is answered "not recognised". Such a link
    /// carries no selector, and every invitation already stored is left without one, so no link finds it. A link is
    /// issued anew only by opening a campaign or by the reminder, and the reminder replaces only a link issued before
    /// its first day (<c>MsfInvitation.IsReminderDue</c>). So a respondent of a campaign open when this is applied can
    /// answer only once the reminder reaches them, and one whose link was issued on or after that day (a campaign opened
    /// late, or a reminder already sent) not at all, unless the campaign is withdrawn and a new one opened.
    /// <c>INFRASTRUCTURE.md</c> § After T163 lists both groups. Nothing is live (W-007): accepted rather than preserved
    /// by keeping the scan for old links.
    /// </para>
    /// <para>
    /// <c>Down</c> drops every selector, so it retires every link issued after this migration too. So does the planned
    /// rollback, restoring the dump taken before the deploy.
    /// </para>
    /// </remarks>
    public partial class T163_MsfInvitationTokenSelector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MsfInvitations_TokenHash",
                table: "MsfInvitations");

            migrationBuilder.AddColumn<string>(
                name: "TokenSelector",
                table: "MsfInvitations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MsfInvitations_TokenSelector",
                table: "MsfInvitations",
                column: "TokenSelector",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MsfInvitations_TokenSelector",
                table: "MsfInvitations");

            migrationBuilder.DropColumn(
                name: "TokenSelector",
                table: "MsfInvitations");

            migrationBuilder.CreateIndex(
                name: "IX_MsfInvitations_TokenHash",
                table: "MsfInvitations",
                column: "TokenHash",
                unique: true);
        }
    }
}
