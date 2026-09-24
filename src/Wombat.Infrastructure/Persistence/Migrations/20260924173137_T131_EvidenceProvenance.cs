using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T131 slice 1 (D38): every staged entrustment decision names the lines of its review's frozen evidence snapshot it
    /// rests on, and each STAR's evidence links say which snapshot row they were copied from.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item>
    /// <c>PendingEntrustmentDecisions.EvidenceLinksJson</c> (free-text links the chair typed, which the page always sent
    /// empty) becomes <c>EvidenceItemIdsJson</c> (snapshot line ids). Every staged row is deleted first (W-007: no real
    /// data anywhere): a staged row names no evidence, so it could never be ratified, and nothing could fill it in. A
    /// chair stages again, naming the evidence.
    /// </item>
    /// <item>One staged decision per (review, EPA): the index becomes unique. The delete above leaves no duplicates.</item>
    /// <item>
    /// <c>EntrustmentEvidenceLinks.CommitteeEvidenceId</c>, with no foreign key (T167's snapshot rule), and <c>Summary</c>
    /// widened to the snapshot's 4000 so a copied summary is whole.
    /// </item>
    /// <item>
    /// <c>EntrustmentDecisions.SupersededByDecisionId</c> becomes a foreign key, so ratify supersedes through the
    /// navigation in the same save that issues the new STAR. Every stored value was written from a stored decision's id.
    /// </item>
    /// <item>
    /// <c>CommitteeEvidenceItems.SourceFinished</c>: whether a snapshot line was finished work when its review started
    /// (D44). Null on every line frozen before this migration; no backfill, because which states finish depends on each
    /// activity's pinned workflow, and the committee page only says something when it is known (T131 review).
    /// </item>
    /// <item>
    /// <c>CommitteeReviews</c> gains an <c>xmin</c> concurrency token in the model and nothing in the database: see Up
    /// (T131 review, CommitteeReviewConfiguration).
    /// </item>
    /// </list>
    /// </remarks>
    public partial class T131_EvidenceProvenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "PendingEntrustmentDecisions";""");

            migrationBuilder.DropIndex(
                name: "IX_PendingEntrustmentDecisions_ReviewId_EpaId",
                table: "PendingEntrustmentDecisions");

            migrationBuilder.RenameColumn(
                name: "EvidenceLinksJson",
                table: "PendingEntrustmentDecisions",
                newName: "EvidenceItemIdsJson");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "EntrustmentEvidenceLinks",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<int>(
                name: "CommitteeEvidenceId",
                table: "EntrustmentEvidenceLinks",
                type: "integer",
                nullable: true);

            // EF generated an AddColumn for "xmin" here and it has been REMOVED by hand, deliberately, as in T121's
            // migration. xmin is a PostgreSQL SYSTEM column present on every table, so adding it fails with "column name
            // xmin conflicts with a system column name". The model maps it as a shadow concurrency token
            // (CommitteeReviewConfiguration) precisely because it already exists; there is no DDL to write.

            migrationBuilder.AddColumn<bool>(
                name: "SourceFinished",
                table: "CommitteeEvidenceItems",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingEntrustmentDecisions_ReviewId_EpaId",
                table: "PendingEntrustmentDecisions",
                columns: new[] { "ReviewId", "EpaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EntrustmentDecisions_SupersededByDecisionId",
                table: "EntrustmentDecisions",
                column: "SupersededByDecisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_EntrustmentDecisions_EntrustmentDecisions_SupersededByDecis~",
                table: "EntrustmentDecisions",
                column: "SupersededByDecisionId",
                principalTable: "EntrustmentDecisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A staged row now holds ids, which the old shape cannot read as links.
            migrationBuilder.Sql("""DELETE FROM "PendingEntrustmentDecisions";""");

            migrationBuilder.DropForeignKey(
                name: "FK_EntrustmentDecisions_EntrustmentDecisions_SupersededByDecis~",
                table: "EntrustmentDecisions");

            migrationBuilder.DropIndex(
                name: "IX_PendingEntrustmentDecisions_ReviewId_EpaId",
                table: "PendingEntrustmentDecisions");

            migrationBuilder.DropIndex(
                name: "IX_EntrustmentDecisions_SupersededByDecisionId",
                table: "EntrustmentDecisions");

            migrationBuilder.DropColumn(
                name: "CommitteeEvidenceId",
                table: "EntrustmentEvidenceLinks");

            // No DropColumn for "xmin": see Up. It is a system column and was never created here.

            migrationBuilder.DropColumn(
                name: "SourceFinished",
                table: "CommitteeEvidenceItems");

            migrationBuilder.RenameColumn(
                name: "EvidenceItemIdsJson",
                table: "PendingEntrustmentDecisions",
                newName: "EvidenceLinksJson");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "EntrustmentEvidenceLinks",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(4000)",
                oldMaxLength: 4000);

            migrationBuilder.CreateIndex(
                name: "IX_PendingEntrustmentDecisions_ReviewId_EpaId",
                table: "PendingEntrustmentDecisions",
                columns: new[] { "ReviewId", "EpaId" });
        }
    }
}
