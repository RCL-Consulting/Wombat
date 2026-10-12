using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T361, flow 07: the one migration of the build. D53 (the operator's, 2026-10-12) removes agenda deferral.
    /// <list type="bullet">
    /// <item>An agenda line stored Deferred (<c>State</c> 2) on a review still open to decision, Scheduled (1) or In progress
    /// (2), becomes Due (1): the sitting has not closed, so the line is still to decide, and a STAR staged on it issues at
    /// ratification as any due line's does (T361, review R2). Every other Deferred line becomes Not decided (4): what a
    /// deferral on a closed review recorded, that the sitting did not decide the EPA, is what Not decided now says. A later
    /// sitting in the window plans it again either way. The deferral's reason goes with its column: the sentence about
    /// undecided EPAs now lives once, on the decision, and no stored reason is guessed into one.</item>
    /// <item><c>CK_CommitteeAgendaLines_DeferredHasAReason</c> and <c>DeferralReason</c> are dropped. The constraint goes
    /// first, so the rows can change state while it no longer holds them.</item>
    /// <item><c>CommitteeDecisions.UndecidedNote</c>: the decision's one optional sentence about the EPAs it did not decide,
    /// as long as <c>Conditions</c> may be.</item>
    /// </list>
    /// Down restores the column and the constraint, but not the deferrals: a Not decided line cannot say it was one.
    /// </summary>
    public partial class T361_NoDeferral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_CommitteeAgendaLines_DeferredHasAReason",
                table: "CommitteeAgendaLines");

            // Deferred (2) on a Scheduled (1) or In progress (2) review to Due (1); every other Deferred line to NotDecided
            // (4) (T361, review R2). A deferred line names no STAR, so CK_..._DecidedNamesItsStar holds either way.
            migrationBuilder.Sql("""
                UPDATE "CommitteeAgendaLines" AS line SET "State" = 1
                FROM "CommitteeReviews" AS review
                WHERE line."ReviewId" = review."Id" AND line."State" = 2 AND review."State" IN (1, 2);
                """);
            migrationBuilder.Sql("""UPDATE "CommitteeAgendaLines" SET "State" = 4 WHERE "State" = 2;""");

            migrationBuilder.DropColumn(
                name: "DeferralReason",
                table: "CommitteeAgendaLines");

            migrationBuilder.AddColumn<string>(
                name: "UndecidedNote",
                table: "CommitteeDecisions",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UndecidedNote",
                table: "CommitteeDecisions");

            migrationBuilder.AddColumn<string>(
                name: "DeferralReason",
                table: "CommitteeAgendaLines",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommitteeAgendaLines_DeferredHasAReason",
                table: "CommitteeAgendaLines",
                sql: "(\"State\" = 2) = (\"DeferralReason\" IS NOT NULL)");
        }
    }
}
