using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Application.Scheduling;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.Persistence;
using Wombat.Infrastructure.Scheduling.Jobs;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.MultiSourceFeedback;

/// <summary>
/// T163 on a real PostgreSQL server: a respondent's link is looked up by one read of one row, by the selector's unique
/// index, and the migration leaves every invitation stored before it findable by no link. T214: the link a reminder
/// replaced is found the same way, by an index of its own, and takes the invitation's one response.
/// </summary>
/// <remarks>
/// <para>
/// Until T163 <see cref="MsfCampaignRules.GetActiveInvitationByTokenAsync" /> loaded every invitation there was, with its
/// campaign and questionnaire, and hashed the token against each, on a public page, for every load and every submit.
/// The unit tests run on the in-memory provider, which has no query plan; this runs the lookup's own statement, captured
/// as EF sent it, through <c>EXPLAIN</c> with the parameters it was sent with.
/// </para>
/// <para>
/// Isolated as <c>MsfRespondentHashPostgresTests</c> is: a schema of its own (<c>it_&lt;guid&gt;</c>), registered before it
/// is created and dropped in a finally and again on dispose.
/// </para>
/// </remarks>
public sealed class MsfLinkSelectorPostgresTests : IAsyncLifetime
{
    private const string T163MigrationSuffix = "_T163_MsfInvitationTokenSelector";
    private const string SelectorIndex = "IX_MsfInvitations_TokenSelector";
    private const string PreviousSelectorIndex = "IX_MsfInvitations_PreviousTokenSelector";
    private const string PreviousLinkCheck = "CK_MsfInvitations_PreviousLink";
    private const string PreviousLinkUnansweredCheck = "CK_MsfInvitations_PreviousLinkUnanswered";
    private const string T214MigrationSuffix = "_T214_MsfInvitationPreviousLink";
    private const string RespondUrl = "https://wombat.example/msf/respond";
    private const string OldHashIndex = "IX_MsfInvitations_TokenHash";

    /// <summary>Invitations other than the one the link names: enough that reading them all is not what a planner picks.</summary>
    private const int OtherInvitations = 2000;

    /// <summary>Invitations with no link issued, as a draft's invitees and every invitation stored before T163 are.</summary>
    private const int InvitationsWithNoLink = 500;

    private readonly InvitationTokenService _tokens = new();
    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    /// <summary>
    /// One statement, which asks both selector columns (T214) and reads each by its own index: never a scan of the table.
    /// </summary>
    [Fact]
    public async Task ALink_IsLookedUpInOneStatement_ThatReadsOneRowByTheSelectorsIndex()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);

            var link = _tokens.GenerateSelectorToken();
            int named;
            await using (var arrange = NewContext(schema))
            {
                var campaign = OpenCampaign();
                var invitation = Invitation(campaign, "named@example.test", link.Selector, link.Hash);
                arrange.MsfInvitations.Add(invitation);
                await arrange.SaveChangesAsync();
                named = invitation.Id;

                await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
                await ExecuteAsync(connection,
                    """
                    INSERT INTO "MsfInvitations"
                        ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenSelector", "TokenHash", "IssuedOn", "ExpiresOn")
                    SELECT $1, 'respondent-' || g || '@example.test', $2,
                           CASE WHEN g <= $3 THEN lpad(g::text, 16, '0') END, md5(g::text), now(), CURRENT_DATE + 10
                    FROM generate_series(1, $3 + $4) AS g
                    """,
                    campaign.Id, (int)MsfRespondentCategory.Nurse, OtherInvitations, InvitationsWithNoLink);
                await ExecuteAsync(connection, """ANALYZE "MsfInvitations" """);
            }

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfInvitations" WHERE "TokenSelector" IS NULL"""))
                .Should().Be(InvitationsWithNoLink, "guard: the unique index holds any number of invitations with no link");

            var commands = new CommandLog();
            await using var db = NewContext(schema, commands);

            var found = await MsfCampaignRules.GetActiveInvitationByTokenAsync(db, link.Token, _tokens, CancellationToken.None);

            found.Id.Should().Be(named);
            found.Campaign.Template.Questions.Should().HaveCount(2, "the questionnaire comes in the same statement");

            var statement = commands.Statements.Should().ContainSingle("the invitation, its campaign and questionnaire are one read").Subject;
            statement.Text.Should().Contain("\"TokenSelector\" =", "the row is chosen on the server, by its selector")
                .And.Contain("\"PreviousTokenSelector\" =", "as the current link or the one a reminder replaced (T214)");

            // Each column by its own index: an index scan of each, or the two combined in a bitmap.
            var plan = await ExplainAsync(schema, statement);
            plan.Should().MatchRegex($"Index Scan (using|on) \"{SelectorIndex}\"", plan)
                .And.MatchRegex($"Index Scan (using|on) \"{PreviousSelectorIndex}\"", plan)
                .And.NotContain("Seq Scan on \"MsfInvitations\"", plan);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task OnPostgres_AValidLinkOpens_AndItsSelectorWithAnotherSecretIsRefused()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);

            var link = _tokens.GenerateSelectorToken();
            await using (var arrange = NewContext(schema))
            {
                arrange.MsfInvitations.Add(Invitation(OpenCampaign(), "named@example.test", link.Selector, link.Hash));
                await arrange.SaveChangesAsync();
            }

            await using var db = NewContext(schema);
            (await MsfCampaignRules.GetActiveInvitationByTokenAsync(db, link.Token, _tokens, CancellationToken.None))
                .TokenSelector.Should().Be(link.Selector);

            var forged = link.Selector + _tokens.GenerateSelectorToken().Token[InvitationTokenService.SelectorLength..];
            var open = () => MsfCampaignRules.GetActiveInvitationByTokenAsync(db, forged, _tokens, CancellationToken.None);
            (await open.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The lookup reads the one row a selector names (<c>SingleOrDefaultAsync</c>), so the migration's index must be
    /// unique: two rows sharing a selector would turn a respondent's page into a server error. The server refuses the
    /// second, by that index and no other, while any number of rows hold no selector. The column is sized to the
    /// selector. (T163 review: the model snapshot said unique, and nothing checked that the migration did.)
    /// </summary>
    [Fact]
    public async Task TheSelectorsIndex_RefusesASecondRowWithASelectorAlreadyHeld_AndHoldsAnyNumberWithNone()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);

            int campaignId;
            await using (var arrange = NewContext(schema))
            {
                var campaign = OpenCampaign();
                arrange.MsfCampaigns.Add(campaign);
                await arrange.SaveChangesAsync();
                campaignId = campaign.Id;
            }

            (await ScalarAsync<string>(schema,
                    """
                    SELECT data_type || '(' || character_maximum_length || ')' FROM information_schema.columns
                    WHERE table_schema = current_schema() AND table_name = 'MsfInvitations' AND column_name = 'TokenSelector'
                    """))
                .Should().Be($"character varying({InvitationTokenService.SelectorLength})");
            (await Catalog.IndexIsUniqueAsync(schema, SelectorIndex)).Should().BeTrue();

            const string withSelector =
                """
                INSERT INTO "MsfInvitations"
                    ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenSelector", "TokenHash", "IssuedOn", "ExpiresOn")
                VALUES ($1, $2, $3, $4, $5, now(), CURRENT_DATE + 10)
                """;
            const string withNoSelector =
                """
                INSERT INTO "MsfInvitations"
                    ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenHash", "IssuedOn", "ExpiresOn")
                VALUES ($1, $2, $3, $4, now(), CURRENT_DATE + 10)
                """;

            var link = _tokens.GenerateSelectorToken();
            var nurse = (int)MsfRespondentCategory.Nurse;
            await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
            await ExecuteAsync(connection, withSelector, campaignId, "first@example.test", nurse, link.Selector, link.Hash);
            await ExecuteAsync(connection, withNoSelector, campaignId, "draft-1@example.test", nurse, _tokens.GenerateSelectorToken().Hash);
            await ExecuteAsync(connection, withNoSelector, campaignId, "draft-2@example.test", nurse, _tokens.GenerateSelectorToken().Hash);

            // Everything but the selector differs, so only the selector's index can refuse it.
            var second = () => ExecuteAsync(
                connection, withSelector, campaignId, "second@example.test", nurse, link.Selector, _tokens.GenerateSelectorToken().Hash);
            var refused = (await second.Should().ThrowAsync<PostgresException>()).Which;
            refused.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
            refused.ConstraintName.Should().Be(SelectorIndex);

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfInvitations" WHERE "TokenSelector" IS NULL"""))
                .Should().Be(2);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Every link mailed before T163 stops working (W-007): the migration gives no invitation already stored a selector,
    /// so no link finds it. The rest of the invitation is left, and <c>Down</c> puts the hash's unique index back.
    /// </summary>
    [Fact]
    public async Task Migration_LeavesEveryInvitationStoredBeforeIt_FoundByNoLink_AndDownRestoresTheHashIndex()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            string predecessor;
            await using (var db = NewContext(schema))
            {
                var migrations = db.Database.GetMigrations().ToList();
                var t163 = migrations.FindIndex(migration => migration.EndsWith(T163MigrationSuffix, StringComparison.Ordinal));
                t163.Should().BePositive("guard: the T163 migration is in the assembly, after at least one other");
                predecessor = migrations[t163 - 1];
                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            // A link as the product mailed it before T163: a 43-character token, only its hash stored.
            var oldToken = _tokens.GenerateToken();
            int invitationId;
            await using (var connection = await TestDatabase.OpenSchemaConnectionAsync(schema))
            {
                var template = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfTemplates" ("Name", "IsActive", "AllowPatientResponses", "Kind")
                    VALUES ('T163 MSF', TRUE, FALSE, 0) RETURNING "Id"
                    """);
                var campaign = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfCampaigns"
                        ("SubjectUserId", "TemplateId", "CreatedByUserId", "CreatedOn", "OpensOn", "ClosesOn", "State",
                         "MinimumResponses", "MinimumRespondentCategories", "MinimumCategoryResponses")
                    VALUES ('trainee-1', $1, 'coordinator-1', now(), CURRENT_DATE - 1, CURRENT_DATE + 3, $2, 0, 0, 0)
                    RETURNING "Id"
                    """,
                    template, (int)MsfCampaignState.Open);
                invitationId = await InsertAsync(connection,
                    """
                    INSERT INTO "MsfInvitations"
                        ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenHash", "IssuedOn", "ExpiresOn")
                    VALUES ($1, 'nurse@example.test', $2, $3, now(), CURRENT_DATE + 10)
                    RETURNING "Id"
                    """,
                    campaign, (int)MsfRespondentCategory.Nurse, _tokens.HashToken(oldToken));
            }

            (await Catalog.IndexNamesAsync(schema, "MsfInvitations")).Should().Contain(OldHashIndex, "guard: the pre-T163 schema");

            await MigrateToLatestAsync(schema);

            (await Catalog.IndexNamesAsync(schema, "MsfInvitations")).Should().Contain(SelectorIndex).And.NotContain(OldHashIndex);
            (await Catalog.IndexIsUniqueAsync(schema, SelectorIndex)).Should().BeTrue();
            await using (var db = NewContext(schema))
            {
                var stored = await db.MsfInvitations.AsNoTracking().SingleAsync();
                stored.Id.Should().Be(invitationId, "no invitation is deleted");
                stored.TokenSelector.Should().BeNull();
                stored.TokenHash.Should().Be(_tokens.HashToken(oldToken));
                stored.RespondentEmail.Should().Be("nurse@example.test");

                var oldLink = () => MsfCampaignRules.GetActiveInvitationByTokenAsync(db, oldToken, _tokens, CancellationToken.None);
                (await oldLink.Should().ThrowAsync<MsfResponseRefusedException>())
                    .Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);

                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            (await Catalog.IndexNamesAsync(schema, "MsfInvitations")).Should().Contain(OldHashIndex).And.NotContain(SelectorIndex);
            (await Catalog.IndexIsUniqueAsync(schema, OldHashIndex)).Should().BeTrue("Down puts the pre-T163 index back as it was");
            (await ScalarAsync<bool>(schema,
                    """
                    SELECT EXISTS (SELECT 1 FROM information_schema.columns
                                   WHERE table_schema = current_schema() AND table_name = 'MsfInvitations'
                                     AND column_name = 'TokenSelector')
                    """))
                .Should().BeFalse();
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T214's verification. A respondent opens the questionnaire through their invitation's link; before they submit, the
    /// reminder job, run as the scheduler runs it, mails them a new link. Their answer, submitted through the link they
    /// opened, is accepted, once; the reminder's link is then used, and the link they answered through is retired.
    /// </summary>
    [Fact]
    public async Task OnPostgres_AResponseThroughTheLinkAReminderReplaced_IsAcceptedOnce_AndTheReminderLinkIsThenUsed()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);

            // The window closes in two days, so today is the first reminder day; the link was mailed three days ago, so it
            // is due to be replaced (MsfInvitation.IsReminderDue).
            var opening = _tokens.GenerateSelectorToken();
            int invitationId;
            int questionId;
            await using (var arrange = NewContext(schema))
            {
                var campaign = OpenCampaign(closesInDays: 2);
                var invitation = Invitation(campaign, "nurse@example.test", opening.Selector, opening.Hash);
                invitation.IssuedOn = DateTime.UtcNow.AddDays(-3);
                arrange.MsfInvitations.Add(invitation);
                await arrange.SaveChangesAsync();
                invitationId = invitation.Id;
                questionId = campaign.Template.Questions.Single(question => question.Required).Id;
            }

            await using (var db = NewContext(schema))
            {
                (await MsfCampaignRules.GetActiveInvitationByTokenAsync(db, opening.Token, _tokens, CancellationToken.None))
                    .Id.Should().Be(invitationId, "the respondent opens the questionnaire and starts typing");
            }

            var reminder = TokenIn((await RunReminderAsync(schema)).Should().ContainSingle("the reminder is due").Subject);

            await using (var db = NewContext(schema))
            {
                var stored = await db.MsfInvitations.AsNoTracking().SingleAsync();
                stored.TokenSelector.Should().Be(_tokens.SelectorOf(reminder));
                _tokens.VerifyToken(reminder, stored.TokenHash).Should().BeTrue();
                stored.PreviousTokenSelector.Should().Be(opening.Selector);
                stored.PreviousTokenHash.Should().Be(opening.Hash);
            }

            // The answer, through the link the reminder replaced.
            await SubmitAsync(schema, opening.Token, questionId, "Calm with parents.");

            await using (var db = NewContext(schema))
            {
                var stored = await db.MsfInvitations.AsNoTracking().SingleAsync();
                stored.RespondedOn.Should().NotBeNull();
                stored.PreviousTokenSelector.Should().BeNull("the answer retires the link it came through");
                stored.PreviousTokenHash.Should().BeNull();
                (await db.MsfResponses.AsNoTracking().Include(response => response.Answers).SingleAsync())
                    .Should().Match<MsfResponse>(response =>
                        response.InvitationId == invitationId &&
                        response.Answers.Single().LongText == "Calm with parents.");

                // The reminder's link is then used: the page and the submit both say so.
                var load = () => MsfCampaignRules.GetActiveInvitationByTokenAsync(db, reminder, _tokens, CancellationToken.None);
                (await load.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkUsed);
            }

            var throughReminder = () => SubmitAsync(schema, reminder, questionId, "A second opinion.");
            (await throughReminder.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkUsed);

            var again = () => SubmitAsync(schema, opening.Token, questionId, "Again.");
            (await again.Should().ThrowAsync<MsfResponseRefusedException>()).Which.Reason.Should().Be(MsfResponseRefusal.LinkNotRecognised);

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfResponses" """)).Should().Be(1, "one response per invitation");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Two answers racing through both links, the reminder's and the one it replaced, each through the respondent's own
    /// submit: the one through the replaced link reads the invitation unanswered, the one through the reminder's link
    /// then commits, and the first is refused at its save by the unique index on <c>MsfResponses.InvitationId</c>, as a
    /// used link, not a fault. One response stands, the winner's, and the invitation holds no previous link. (T214 review:
    /// the winner used to be a bare insert, which left the invitation unanswered, a state no answer leaves.)
    /// </summary>
    [Fact]
    public async Task OnPostgres_AnAnswerThroughThePreviousLink_ThatLosesTheRaceToTheReminderLinks_IsRefusedAsUsed()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);

            var opening = _tokens.GenerateSelectorToken();
            var reminder = _tokens.GenerateSelectorToken();
            int questionId;
            await using (var arrange = NewContext(schema))
            {
                var campaign = OpenCampaign(closesInDays: 2);
                var invitation = Invitation(campaign, "nurse@example.test", opening.Selector, opening.Hash);
                invitation.ReplaceLink(reminder.Selector, reminder.Hash, DateTime.UtcNow.AddHours(-1));
                arrange.MsfInvitations.Add(invitation);
                await arrange.SaveChangesAsync();
                questionId = campaign.Template.Questions.Single(question => question.Required).Id;
            }

            // The loser has read the invitation and checked its link; the winner commits before the loser saves.
            await using (var loser = NewContext(schema, new BeforeSave(() => SubmitAsync(schema, reminder.Token, questionId, "The winner."))))
            {
                var submit = () => new SubmitMsfResponseCommandHandler(loser, _tokens).Handle(
                    new SubmitMsfResponseCommand(opening.Token, [new SubmitMsfResponseAnswerItem(questionId, null, "The loser.")]),
                    CancellationToken.None);
                var refusal = (await submit.Should().ThrowAsync<MsfResponseRefusedException>()).Which;
                refusal.Reason.Should().Be(MsfResponseRefusal.LinkUsed);
                refusal.InnerException.Should().BeAssignableTo<DbUpdateException>("refused by the server's index, not the check");
            }

            (await ScalarAsync<long>(schema, """SELECT COUNT(*) FROM "MsfResponses" """)).Should().Be(1);
            (await ScalarAsync<string>(schema, """SELECT "LongText" FROM "MsfResponseAnswers" """)).Should().Be("The winner.");
            await using (var db = NewContext(schema))
            {
                var stored = await db.MsfInvitations.AsNoTracking().SingleAsync();
                stored.RespondedOn.Should().NotBeNull();
                stored.TokenSelector.Should().Be(reminder.Selector);
                stored.PreviousTokenSelector.Should().BeNull("the winner's answer retired it");
                stored.PreviousTokenHash.Should().BeNull();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The reminder job reads two respondents unanswered and mails each a new link; the first answers, through the link
    /// they hold, after the job has read them and before it stores their new link. That store is refused by the server
    /// (<c>CK_MsfInvitations_PreviousLinkUnanswered</c>): an answered invitation keeps no link a reminder replaced. The
    /// answer stands, the link it came through stays the invitation's, and the job goes on to the next respondent.
    /// (T214 review, finding 1: the job's save used to put the replaced link back on the answered row.)
    /// </summary>
    [Fact]
    public async Task OnPostgres_ARespondentWhoAnswersWhileTheirReminderIsBeingSent_KeepsTheirAnswer_AndTheReminderStoresNothing()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);
            var (links, questionId) = await SeedDueRespondentsAsync(schema, "nurse@example.test", "peer@example.test");

            string? answered = null;
            var mailed = await RunReminderAsync(schema, async message =>
            {
                if (answered is null)
                {
                    answered = message.To;
                    await SubmitAsync(schema, links[message.To].Token, questionId, "Answered while the reminder was sent.");
                }
            });

            mailed.Should().HaveCount(2, "both were due, and each was mailed before the job stored anything for them");

            await using var db = NewContext(schema);
            var stored = await db.MsfInvitations.AsNoTracking().ToDictionaryAsync(invitation => invitation.RespondentEmail!);

            var first = stored[answered!];
            first.RespondedOn.Should().NotBeNull();
            first.TokenSelector.Should().Be(links[answered!].Selector, "the reminder's link was not stored");
            first.PreviousTokenSelector.Should().BeNull("an answered invitation keeps no previous link");
            first.PreviousTokenHash.Should().BeNull();

            var other = stored.Values.Single(invitation => invitation.RespondentEmail != answered);
            other.RespondedOn.Should().BeNull();
            other.PreviousTokenSelector.Should().Be(links[other.RespondentEmail!].Selector, "the job went on to the next respondent");
            other.TokenSelector.Should().Be(_tokens.SelectorOf(TokenIn(mailed.Single(message => message.To == other.RespondentEmail))));

            (await db.MsfResponses.CountAsync()).Should().Be(1);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The coordinator withdraws the campaign after the reminder job has read it and mailed its first respondent, and before
    /// the job stores that respondent's new link. The job's store is refused by the campaign's xmin token, which it
    /// writes for that purpose; it does not reopen the campaign or give an anonymised invitation a previous link, and it
    /// mails nobody else of the withdrawn campaign. (T214 review, finding 1.)
    /// </summary>
    [Fact]
    public async Task OnPostgres_ACampaignWithdrawnWhileAReminderIsBeingSent_KeepsNoPreviousLink_AndNoOneElseIsMailed()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);
            var (links, _) = await SeedDueRespondentsAsync(schema, "nurse@example.test", "peer@example.test");

            var withdrawn = false;
            var mailed = await RunReminderAsync(schema, async _ =>
            {
                if (!withdrawn)
                {
                    withdrawn = true;
                    await WithdrawAsync(schema);
                }
            });

            mailed.Should().ContainSingle("the job asks again before each reminder, and sees the campaign withdrawn");

            await using var db = NewContext(schema);
            (await db.MsfCampaigns.AsNoTracking().SingleAsync()).State.Should().Be(MsfCampaignState.Withdrawn);
            var stored = await db.MsfInvitations.AsNoTracking().ToListAsync();
            stored.Should().HaveCount(2).And.OnlyContain(invitation =>
                invitation.AnonymizedOn != null &&
                invitation.RespondentEmail == null &&
                invitation.PreviousTokenSelector == null &&
                invitation.PreviousTokenHash == null);
            stored.Select(invitation => invitation.TokenSelector).Should().BeEquivalentTo(
                links.Values.Select(link => link.Selector), "no reminder's link was stored");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// A store refused for any other reason, one the respondent is still due a reminder after, is a fault: the run stops
    /// there, as any failed store did before T214, so no later respondent is mailed a link that may not be stored. The
    /// respondent keeps the link they had, and is still due at the next run.
    /// </summary>
    [Fact]
    public async Task OnPostgres_AStoreRefusedForAnyOtherReason_StopsTheRun_AndLeavesTheRespondentDue()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);
            var (links, _) = await SeedDueRespondentsAsync(schema, "nurse@example.test", "peer@example.test");

            var mailed = new List<EmailMessage>();
            var run = () => RunReminderAsync(
                schema,
                message =>
                {
                    mailed.Add(message);
                    return Task.CompletedTask;
                },
                new RefuseSaves());

            await run.Should().ThrowAsync<DbUpdateException>();
            mailed.Should().ContainSingle("the run stopped at the first refused store");

            await using var db = NewContext(schema);
            var stored = await db.MsfInvitations.AsNoTracking().ToListAsync();
            stored.Select(invitation => invitation.TokenSelector).Should().BeEquivalentTo(links.Values.Select(link => link.Selector));
            stored.Should().OnlyContain(invitation => invitation.PreviousTokenSelector == null);

            var campaign = await db.MsfCampaigns.AsNoTracking().SingleAsync();
            stored.Should().OnlyContain(invitation => invitation.IsReminderDue(campaign, DateTime.UtcNow));
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The other order: the respondent's submit has read their invitation, and the reminder job replaces its link before
    /// the submit saves. The answer is stored, and it retires the link the reminder kept, though the submit never saw it:
    /// the answer writes that the invitation holds no previous link, whatever it read.
    /// </summary>
    [Fact]
    public async Task OnPostgres_AnAnswerSavedJustAfterAReminderReplacedItsLink_IsStored_AndLeavesNoPreviousLink()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);
            var (links, questionId) = await SeedDueRespondentsAsync(schema, "nurse@example.test");
            var opening = links["nurse@example.test"];

            List<EmailMessage> mailed = [];
            await using (var respondent = NewContext(schema, new BeforeSave(async () => mailed = await RunReminderAsync(schema))))
            {
                await new SubmitMsfResponseCommandHandler(respondent, _tokens).Handle(
                    new SubmitMsfResponseCommand(opening.Token, [new SubmitMsfResponseAnswerItem(questionId, null, "Calm with parents.")]),
                    CancellationToken.None);
            }

            var reminder = TokenIn(mailed.Should().ContainSingle("the reminder ran between the answer's read and its save").Subject);

            await using var db = NewContext(schema);
            var stored = await db.MsfInvitations.AsNoTracking().SingleAsync();
            stored.RespondedOn.Should().NotBeNull();
            stored.TokenSelector.Should().Be(_tokens.SelectorOf(reminder), "the reminder's store committed first");
            stored.PreviousTokenSelector.Should().BeNull("the answer retires the link the reminder kept");
            stored.PreviousTokenHash.Should().BeNull();
            (await db.MsfResponses.CountAsync()).Should().Be(1);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The other order for a close: the coordinator's close has read the campaign, and the reminder job replaces a link
    /// before it saves. The close is refused by the campaign's xmin token, which the reminder's store moved, and says so;
    /// nothing it did is stored. Closing again retires the link the reminder kept.
    /// </summary>
    [Fact]
    public async Task OnPostgres_ACloseSavedJustAfterAReminderReplacedALink_IsRefused_AndClosingAgainRetiresIt()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);
            var (links, _) = await SeedDueRespondentsAsync(schema, "nurse@example.test");
            var campaignId = await ScalarAsync<int>(schema, """SELECT "Id" FROM "MsfCampaigns" """);

            await using (var coordinator = NewContext(schema, new BeforeSave(() => RunReminderAsync(schema))))
            {
                var close = () => CloseAsync(coordinator, campaignId);
                (await close.Should().ThrowAsync<InvalidOperationException>())
                    .WithMessage(CloseMsfCampaignCommandHandler.CampaignChanged)
                    .WithInnerException<DbUpdateConcurrencyException>();
            }

            await using (var db = NewContext(schema))
            {
                (await db.MsfCampaigns.AsNoTracking().SingleAsync()).State.Should().Be(MsfCampaignState.Open);
                var stored = await db.MsfInvitations.AsNoTracking().SingleAsync();
                stored.RespondentEmail.Should().NotBeNull("the refused close anonymised nobody");
                stored.PreviousTokenSelector.Should().Be(links["nurse@example.test"].Selector, "the reminder's store stands");
            }

            await using (var coordinator = NewContext(schema))
            {
                await CloseAsync(coordinator, campaignId);
            }

            await using (var db = NewContext(schema))
            {
                (await db.MsfCampaigns.AsNoTracking().SingleAsync()).State.Should().Be(MsfCampaignState.UnderReview);
                var stored = await db.MsfInvitations.AsNoTracking().SingleAsync();
                stored.AnonymizedOn.Should().NotBeNull();
                stored.PreviousTokenSelector.Should().BeNull();
                stored.PreviousTokenHash.Should().BeNull();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// The T214 migration adds the previous link's columns, sized as the current link's, its selector under a unique
    /// index, a check that the selector and hash are set together, and a check that an answered invitation holds none
    /// (T214 review); <c>Down</c> drops them and leaves the current link as it was.
    /// </summary>
    [Fact]
    public async Task TheT214Migration_KeepsThePreviousLinkUnderAUniqueIndexAndACheck_AndDownDropsItAlone()
    {
        try
        {
            var schema = await _schemas.CreateAsync();
            await MigrateToLatestAsync(schema);

            int campaignId;
            await using (var arrange = NewContext(schema))
            {
                var campaign = OpenCampaign();
                arrange.MsfCampaigns.Add(campaign);
                await arrange.SaveChangesAsync();
                campaignId = campaign.Id;
            }

            (await Catalog.ColumnTypeAsync(schema, "MsfInvitations", "PreviousTokenSelector")).Should().Be($"character varying({InvitationTokenService.SelectorLength})");
            (await Catalog.ColumnTypeAsync(schema, "MsfInvitations", "PreviousTokenHash")).Should().Be("character varying(64)");
            (await Catalog.IndexIsUniqueAsync(schema, PreviousSelectorIndex)).Should().BeTrue();

            const string insert =
                """
                INSERT INTO "MsfInvitations"
                    ("CampaignId", "RespondentEmail", "RespondentCategory", "TokenSelector", "TokenHash",
                     "PreviousTokenSelector", "PreviousTokenHash", "IssuedOn", "ExpiresOn")
                VALUES ($1, $2, $3, $4, $5, $6, $7, now(), CURRENT_DATE + 10)
                """;
            var nurse = (int)MsfRespondentCategory.Nurse;
            var current = _tokens.GenerateSelectorToken();
            var previous = _tokens.GenerateSelectorToken();
            await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
            await ExecuteAsync(connection, insert, campaignId, "first@example.test", nurse, current.Selector, current.Hash, previous.Selector, previous.Hash);

            // Everything but the previous selector differs, so only its index can refuse the row.
            var another = _tokens.GenerateSelectorToken();
            var shared = () => ExecuteAsync(
                connection, insert, campaignId, "second@example.test", nurse, another.Selector, another.Hash, previous.Selector,
                _tokens.GenerateSelectorToken().Hash);
            var duplicate = (await shared.Should().ThrowAsync<PostgresException>()).Which;
            duplicate.SqlState.Should().Be(PostgresErrorCodes.UniqueViolation);
            duplicate.ConstraintName.Should().Be(PreviousSelectorIndex);

            foreach (var (selector, hash) in new (object, object)[]
                     {
                         (_tokens.GenerateSelectorToken().Selector, DBNull.Value),
                         (DBNull.Value, _tokens.GenerateSelectorToken().Hash)
                     })
            {
                var fresh = _tokens.GenerateSelectorToken();
                var halfSet = () => ExecuteAsync(
                    connection, insert, campaignId, "third@example.test", nurse, fresh.Selector, fresh.Hash, selector, hash);
                var refused = (await halfSet.Should().ThrowAsync<PostgresException>()).Which;
                refused.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);
                refused.ConstraintName.Should().Be(PreviousLinkCheck);
            }

            // An answered invitation keeps no previous link, whichever write comes last (T214 review): answering the row
            // above is refused while it holds one, and so is giving one to an answered row.
            var answer = () => ExecuteAsync(
                connection, """UPDATE "MsfInvitations" SET "RespondedOn" = now() WHERE "PreviousTokenSelector" = $1""", previous.Selector);
            var unanswered = (await answer.Should().ThrowAsync<PostgresException>()).Which;
            unanswered.SqlState.Should().Be(PostgresErrorCodes.CheckViolation);
            unanswered.ConstraintName.Should().Be(PreviousLinkUnansweredCheck);

            var answered = _tokens.GenerateSelectorToken();
            await ExecuteAsync(connection, insert, campaignId, "fourth@example.test", nurse, answered.Selector, answered.Hash, DBNull.Value, DBNull.Value);
            await ExecuteAsync(connection, """UPDATE "MsfInvitations" SET "RespondedOn" = now() WHERE "TokenSelector" = $1""", answered.Selector);
            var kept = _tokens.GenerateSelectorToken();
            var keep = () => ExecuteAsync(
                connection,
                """UPDATE "MsfInvitations" SET "PreviousTokenSelector" = $1, "PreviousTokenHash" = $2 WHERE "TokenSelector" = $3""",
                kept.Selector, kept.Hash, answered.Selector);
            (await keep.Should().ThrowAsync<PostgresException>()).Which.ConstraintName.Should().Be(PreviousLinkUnansweredCheck);

            string predecessor;
            await using (var db = NewContext(schema))
            {
                var migrations = db.Database.GetMigrations().ToList();
                var t214 = migrations.FindIndex(migration => migration.EndsWith(T214MigrationSuffix, StringComparison.Ordinal));
                t214.Should().BePositive("guard: the T214 migration is in the assembly, after at least one other");
                predecessor = migrations[t214 - 1];
                await db.GetService<IMigrator>().MigrateAsync(predecessor);
            }

            (await Catalog.IndexNamesAsync(schema, "MsfInvitations")).Should().Contain(SelectorIndex).And.NotContain(PreviousSelectorIndex);
            (await ScalarAsync<long>(schema,
                    """
                    SELECT COUNT(*) FROM information_schema.columns
                    WHERE table_schema = current_schema() AND table_name = 'MsfInvitations'
                      AND column_name IN ('PreviousTokenSelector', 'PreviousTokenHash')
                    """))
                .Should().Be(0);
            (await ScalarAsync<string>(schema, """SELECT "TokenSelector" FROM "MsfInvitations" """))
                .Should().Be(current.Selector, "Down leaves the current link as it was");
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    // ---- the data ----------------------------------------------------------------------------------------------------

    private static MsfCampaign OpenCampaign(int closesInDays = 3)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new MsfCampaign
        {
            SubjectUserId = "trainee-1",
            CreatedByUserId = "coordinator-1",
            CreatedOn = DateTime.UtcNow,
            OpensOn = today.AddDays(-1),
            ClosesOn = today.AddDays(closesInDays),
            State = MsfCampaignState.Open,
            Template = new MsfTemplate
            {
                Name = "Annual MSF",
                IsActive = true,
                Questions =
                [
                    new MsfQuestion { Order = 1, Prompt = "Communication", Type = MsfQuestionType.LongText, Required = true },
                    new MsfQuestion { Order = 2, Prompt = "Anything else", Type = MsfQuestionType.LongText, Required = false }
                ]
            }
        };
    }

    private static MsfInvitation Invitation(MsfCampaign campaign, string email, string selector, string hash)
        => new()
        {
            Campaign = campaign,
            RespondentEmail = email,
            RespondentCategory = MsfRespondentCategory.Nurse,
            TokenSelector = selector,
            TokenHash = hash,
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(10)
        };

    /// <summary>
    /// An open campaign whose window closes in two days, so today is the first reminder day, and one invitation per
    /// address, each with its own link mailed three days ago: every one of them is due a reminder today
    /// (<see cref="MsfInvitation.IsReminderDue" />). The links by address, and the questionnaire's required question.
    /// </summary>
    private async Task<(Dictionary<string, SelectorToken> Links, int QuestionId)> SeedDueRespondentsAsync(
        string schema, params string[] addresses)
    {
        var links = addresses.ToDictionary(address => address, _ => _tokens.GenerateSelectorToken(), StringComparer.Ordinal);

        await using var arrange = NewContext(schema);
        var campaign = OpenCampaign(closesInDays: 2);
        foreach (var (address, link) in links)
        {
            var invitation = Invitation(campaign, address, link.Selector, link.Hash);
            invitation.IssuedOn = DateTime.UtcNow.AddDays(-3);
            arrange.MsfInvitations.Add(invitation);
        }

        await arrange.SaveChangesAsync();
        return (links, campaign.Template.Questions.Single(question => question.Required).Id);
    }

    // ---- the respondent, the coordinator and the reminder, as the product runs them ----------------------------------

    /// <summary>The coordinator's close, through its handler, as an administrator: the campaign read, closed and saved.</summary>
    private static Task<MsfCampaignAggregateReportDto> CloseAsync(ApplicationDbContext db, int campaignId)
        => new CloseMsfCampaignCommandHandler(db, new MsfAggregationService()).Handle(
            new CloseMsfCampaignCommand(
                campaignId,
                new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, "admin-1"), new Claim(ClaimTypes.Role, WombatRoles.Administrator)],
                    "IntegrationTest",
                    ClaimTypes.Name,
                    ClaimTypes.Role))),
            CancellationToken.None);

    /// <summary>
    /// The coordinator's withdrawal, in a context of its own, as <c>WithdrawMsfCampaignCommandHandler</c> makes it: the
    /// campaign's graph read, withdrawn, and saved under its xmin token.
    /// </summary>
    private async Task WithdrawAsync(string schema)
    {
        await using var db = NewContext(schema);
        var campaign = await db.MsfCampaigns.Include(candidate => candidate.Invitations).SingleAsync();
        campaign.Withdraw(DateTime.UtcNow);
        await db.SaveChangesAsync();
    }

    /// <summary>A response through a link, as the respondent page submits it, in a context of its own.</summary>
    private async Task SubmitAsync(string schema, string token, int questionId, string comment)
    {
        await using var db = NewContext(schema);
        await new SubmitMsfResponseCommandHandler(db, _tokens).Handle(
            new SubmitMsfResponseCommand(token, [new SubmitMsfResponseAnswerItem(questionId, null, comment)]),
            CancellationToken.None);
    }

    /// <summary>
    /// The reminder job, run now as the scheduler runs it, on this schema; the reminders it mailed. <paramref name="onSend" />
    /// runs as each reminder is handed over, after the job has read the invitation and before it stores the new link.
    /// </summary>
    private async Task<List<EmailMessage>> RunReminderAsync(
        string schema, Func<EmailMessage, Task>? onSend = null, params IInterceptor[] interceptors)
    {
        var mailed = new CapturingEmailSender(onSend);
        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(TestDatabase.SchemaConnectionString(schema));
            if (interceptors.Length > 0)
            {
                options.AddInterceptors(interceptors);
            }
        });
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IEmailSender>(mailed);
        services.AddSingleton<IInvitationTokenService>(_tokens);
        services.AddSingleton<IUserAdministrationService>(new FakeUserDirectory(("trainee-1", "Thandi Nkosi")));
        services.AddSingleton(Options.Create(new WombatOptions { MsfRespondUrl = RespondUrl }));

        await using var provider = services.BuildServiceProvider();
        await new MsfInvitationExpiryReminderJob(provider.GetRequiredService<IServiceScopeFactory>()).ExecuteAsync(
            new ScheduledJobContext(DateTime.UtcNow, NullLogger.Instance),
            CancellationToken.None);

        return mailed.Messages;
    }

    /// <summary>The token of the one link a reminder carries.</summary>
    private static string TokenIn(EmailMessage message)
    {
        var link = message.TextBody
            .Split('\n', StringSplitOptions.TrimEntries)
            .Single(line => line.StartsWith(RespondUrl + "?token=", StringComparison.Ordinal));
        return Uri.UnescapeDataString(link[(RespondUrl.Length + "?token=".Length)..]);
    }

    private sealed class CapturingEmailSender(Func<EmailMessage, Task>? onSend) : IEmailSender
    {
        public List<EmailMessage> Messages { get; } = [];

        public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            if (onSend is not null)
            {
                await onSend(message);
            }
        }
    }

    /// <summary>Refuses every save, as the server would for a reason no respondent's state explains.</summary>
    private sealed class RefuseSaves : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
            => throw new DbUpdateException("Refused by the test.");
    }

    /// <summary>
    /// Runs <paramref name="action" /> once, as the context it is added to starts to save: after that context has read
    /// what it saves, and before its statements reach the server. Another context's work committed in
    /// <paramref name="action" /> is what a racing request would have committed in that gap.
    /// </summary>
    private sealed class BeforeSave(Func<Task> action) : SaveChangesInterceptor
    {
        private bool _ran;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_ran)
            {
                _ran = true;
                await action();
            }

            return result;
        }
    }

    // ---- the plan ----------------------------------------------------------------------------------------------------

    /// <summary>The server's plan for a statement EF sent, with the parameter values it was sent with.</summary>
    private async Task<string> ExplainAsync(string schema, CapturedStatement statement)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand("EXPLAIN " + statement.Text, connection);
        command.Parameters.AddRange(statement.Parameters.ToArray());

        var lines = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lines.Add(reader.GetString(0));
        }

        return string.Join('\n', lines);
    }

    // ---- schema lifecycle (as MsfRespondentHashPostgresTests) ---------------------------------------------------------

    private async Task MigrateToLatestAsync(string schema)
    {
        await using var db = NewContext(schema);
        await db.Database.MigrateAsync();
        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    private ApplicationDbContext NewContext(string schema, params IInterceptor[] interceptors)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(TestDatabase.SchemaConnectionString(schema));
        if (interceptors.Length > 0)
        {
            options.AddInterceptors(interceptors);
        }

        return new ApplicationDbContext(options.Options);
    }

    private static async Task<int> InsertAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        return Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql, params object[] values)
    {
        await using var command = Command(connection, sql, values);
        await command.ExecuteNonQueryAsync();
    }

    private async Task<T> ScalarAsync<T>(string schema, string sql)
    {
        await using var connection = await TestDatabase.OpenSchemaConnectionAsync(schema);
        await using var command = new NpgsqlCommand(sql, connection);
        var value = await command.ExecuteScalarAsync();
        return (T)Convert.ChangeType(value!, typeof(T), CultureInfo.InvariantCulture);
    }

    /// <summary>Positional parameters ($1, $2, …), so no value is ever spliced into SQL text.</summary>
    private static NpgsqlCommand Command(NpgsqlConnection connection, string sql, object[] values)
    {
        var command = new NpgsqlCommand(sql, connection);
        foreach (var value in values)
        {
            command.Parameters.Add(new NpgsqlParameter { Value = value });
        }

        return command;
    }

    private sealed record CapturedStatement(string Text, IReadOnlyList<NpgsqlParameter> Parameters);

    /// <summary>Every query that reached the server, with a copy of the parameters it was sent with.</summary>
    private sealed class CommandLog : DbCommandInterceptor
    {
        public List<CapturedStatement> Statements { get; } = [];

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            Capture(command);
            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Capture(command);
            return ValueTask.FromResult(result);
        }

        public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<object> result, CancellationToken cancellationToken = default)
        {
            Capture(command);
            return ValueTask.FromResult(result);
        }

        private void Capture(DbCommand command)
            => Statements.Add(new CapturedStatement(
                command.CommandText,
                command.Parameters.Cast<NpgsqlParameter>().Select(parameter => parameter.Clone()).ToList()));
    }
}
