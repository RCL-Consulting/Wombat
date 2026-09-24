using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Wombat.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// T131 slice 4: a committee review sits for an academic period and carries an agenda of the EPAs it is there to
    /// decide.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    /// <item><c>CommitteeReviews.AcademicYear</c> and <c>Semester</c>, NOT NULL, with checks that the semester is 1 or 2
    /// and the year one <c>AcademicPeriod</c> can represent with its neighbours (2 to 9998), and a partial unique index:
    /// one open (Scheduled, InProgress or Decided) binding review per trainee, panel and period. The schedule handler holds
    /// the wider rule, one per seat (every general panel at an institution, or every panel sitting as one College
    /// committee); the index holds the same panel against a race.</item>
    /// <item><c>CommitteeAgendaLines</c>: one line per EPA per review, cascading with the review; the EPA's code and title
    /// frozen; the curriculum item and EPA as ids with no foreign key (the snapshot rule, T167); the STAR a Decided line
    /// names as a restricting foreign key. Checks: the window's semester is 1, 2 or null (a year); a line is Decided
    /// exactly when it names a STAR, and Deferred exactly when it has a reason. Each line carries PostgreSQL's
    /// <c>xmin</c> as a concurrency token in the model and nothing in the database (see Up).</item>
    /// </list>
    /// <para>
    /// <b>Rows that predate the columns</b> are stamped once, here, with the semester holding the last day of their
    /// evidence window. Semester 2 starts on 1 July: <c>AcademicPeriod</c>'s boundary as it stands today, written out
    /// rather than read from its constants, so that this migration does what it did when it was written whatever the
    /// calendar later becomes. That is a one-time stamp of scenario rows (W-007), not a rule: from now on the period is
    /// chosen when a review is scheduled and never derived from its window. Such a review has no agenda lines, since it was
    /// scheduled and started before agendas existed; it ratifies with none outstanding, and staging a decision on it adds
    /// the chair's line as on any review. The stamp runs before the index and the checks, which it must satisfy: two open
    /// binding reviews of one trainee before one panel whose windows end in the same semester would stop this migration.
    /// Neither database holds such a pair (dev: reviews 1 and 2, one of them ratified; production: none).
    /// </para>
    /// </remarks>
    public partial class T131_ReviewAgenda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AcademicYear",
                table: "CommitteeReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Semester",
                table: "CommitteeReviews",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // The one-time stamp of existing rows (see the remarks), before the index and the checks that read it. The
            // 7 and 1 are semester 2's first month and day, frozen here.
            migrationBuilder.Sql(
                "UPDATE \"CommitteeReviews\" SET " +
                "\"AcademicYear\" = EXTRACT(YEAR FROM \"ReviewPeriodTo\")::integer, " +
                "\"Semester\" = CASE WHEN \"ReviewPeriodTo\" < make_date(EXTRACT(YEAR FROM \"ReviewPeriodTo\")::integer, 7, 1) " +
                "THEN 1 ELSE 2 END;");

            migrationBuilder.CreateTable(
                name: "CommitteeAgendaLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReviewId = table.Column<int>(type: "integer", nullable: false),
                    CurriculumItemId = table.Column<int>(type: "integer", nullable: false),
                    EpaId = table.Column<int>(type: "integer", nullable: false),
                    EpaCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EpaTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Origin = table.Column<int>(type: "integer", nullable: false),
                    WindowYear = table.Column<int>(type: "integer", nullable: false),
                    WindowSemester = table.Column<int>(type: "integer", nullable: true),
                    IsClosing = table.Column<bool>(type: "boolean", nullable: false),
                    IsPartialPeriod = table.Column<bool>(type: "boolean", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    DeferralReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    EntrustmentDecisionId = table.Column<int>(type: "integer", nullable: true)

                    // EF generated an "xmin" column here and it has been REMOVED by hand, deliberately, as in T121's and
                    // T131 slice 1's migrations. xmin is a PostgreSQL SYSTEM column present on every table, so creating it
                    // fails with "column name xmin conflicts with a system column name". The model maps it as a shadow
                    // concurrency token (CommitteeAgendaLineConfiguration) precisely because it already exists.
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitteeAgendaLines", x => x.Id);
                    table.CheckConstraint("CK_CommitteeAgendaLines_DecidedNamesItsStar", "(\"State\" = 3) = (\"EntrustmentDecisionId\" IS NOT NULL)");
                    table.CheckConstraint("CK_CommitteeAgendaLines_DeferredHasAReason", "(\"State\" = 2) = (\"DeferralReason\" IS NOT NULL)");
                    table.CheckConstraint("CK_CommitteeAgendaLines_WindowSemester", "\"WindowSemester\" IS NULL OR \"WindowSemester\" IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_CommitteeAgendaLines_CommitteeReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "CommitteeReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CommitteeAgendaLines_EntrustmentDecisions_EntrustmentDecisi~",
                        column: x => x.EntrustmentDecisionId,
                        principalTable: "EntrustmentDecisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeReviews_OneOpenBindingReviewPerPeriod",
                table: "CommitteeReviews",
                columns: new[] { "TraineeUserId", "PanelId", "AcademicYear", "Semester" },
                unique: true,
                filter: "\"IsFormative\" = FALSE AND \"State\" IN (1, 2, 3)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommitteeReviews_AcademicYear",
                table: "CommitteeReviews",
                sql: "\"AcademicYear\" BETWEEN 2 AND 9998");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CommitteeReviews_Semester",
                table: "CommitteeReviews",
                sql: "\"Semester\" IN (1, 2)");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeAgendaLines_EntrustmentDecisionId",
                table: "CommitteeAgendaLines",
                column: "EntrustmentDecisionId");

            migrationBuilder.CreateIndex(
                name: "IX_CommitteeAgendaLines_ReviewId_EpaId",
                table: "CommitteeAgendaLines",
                columns: new[] { "ReviewId", "EpaId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommitteeAgendaLines");

            migrationBuilder.DropIndex(
                name: "IX_CommitteeReviews_OneOpenBindingReviewPerPeriod",
                table: "CommitteeReviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CommitteeReviews_AcademicYear",
                table: "CommitteeReviews");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CommitteeReviews_Semester",
                table: "CommitteeReviews");

            migrationBuilder.DropColumn(
                name: "AcademicYear",
                table: "CommitteeReviews");

            migrationBuilder.DropColumn(
                name: "Semester",
                table: "CommitteeReviews");
        }
    }
}
