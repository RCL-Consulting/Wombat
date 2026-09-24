using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// An MSF invitation keeps the link a reminder replaced, as a selector under a unique index of its own and a hash, so
    /// that link still takes the respondent's one response until their last day to respond. (T214)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A check constraint holds the two columns together: both set, or neither. Every invitation already stored gets
    /// neither, so a link a reminder replaced before this migration stays retired; only a reminder sent after it keeps
    /// the link it replaces. Nothing is live (W-007), and nothing could be backfilled: the replaced link's hash was
    /// overwritten when the reminder stored its own.
    /// </para>
    /// <para>
    /// A second keeps a previous link off an answered invitation, whichever write comes last (T214 review): the reminder
    /// job stores the link it replaced after it has mailed the respondent, and an answer may commit in between. Every
    /// invitation already stored holds no previous link, so every one passes it.
    /// </para>
    /// <para>
    /// <c>Down</c> drops both columns and both checks, which retires every previous link again; the current links are
    /// untouched.
    /// </para>
    /// </remarks>
    public partial class T214_MsfInvitationPreviousLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousTokenHash",
                table: "MsfInvitations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousTokenSelector",
                table: "MsfInvitations",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MsfInvitations_PreviousTokenSelector",
                table: "MsfInvitations",
                column: "PreviousTokenSelector",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_MsfInvitations_PreviousLink",
                table: "MsfInvitations",
                sql: "(\"PreviousTokenSelector\" IS NULL) = (\"PreviousTokenHash\" IS NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_MsfInvitations_PreviousLinkUnanswered",
                table: "MsfInvitations",
                sql: "\"PreviousTokenSelector\" IS NULL OR \"RespondedOn\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MsfInvitations_PreviousTokenSelector",
                table: "MsfInvitations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MsfInvitations_PreviousLink",
                table: "MsfInvitations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MsfInvitations_PreviousLinkUnanswered",
                table: "MsfInvitations");

            migrationBuilder.DropColumn(
                name: "PreviousTokenHash",
                table: "MsfInvitations");

            migrationBuilder.DropColumn(
                name: "PreviousTokenSelector",
                table: "MsfInvitations");
        }
    }
}
