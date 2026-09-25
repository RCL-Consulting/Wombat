using System.Globalization;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.DataRights;
using Wombat.Application.Features.DataRights.Commands;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Infrastructure.DataRights;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Integration.Tests.Identity;

/// <summary>
/// T258 on a real PostgreSQL server, through the real approval handler and the real <see cref="ErasureExecutor" />: an
/// erasure hands every profile of the person to the pseudonym and ends it, ends what was open about them (their open
/// committee reviews, the decisions staged at them, their unreleased feedback campaigns), and keeps what was settled (a
/// ratified review, its STAR, a review under appeal that the appeal body can still answer, a released report) under the
/// pseudonym. The request is the trainee's own, submitted with those reviews open. A failed erasure changes nothing, the
/// request's completion included, a concurrent change is refused in words, and the request can be approved again.
/// </summary>
/// <remarks>
/// <para>
/// The erasure runs raw SQL beside its tracked changes, so only a real server shows what it leaves. Before T258 it moved
/// only the first profile and left it active, and left every open review and campaign running under the pseudonym.
/// </para>
/// <para>
/// Isolated the way <c>ErasedTraineeScopePostgresTests</c> is: a migrated schema of its own (<c>it_&lt;guid&gt;</c>),
/// registered through <c>TestSchemas</c> before it is created and dropped in a finally and again on dispose. The demo seed
/// gives the institution, curriculum, EPA and entrustment level the rows name.
/// </para>
/// </remarks>
public sealed class ErasureEndsOpenRecordsPostgresTests : IAsyncLifetime
{
    private const string ErasedUserId = "trainee-t258-erased";
    private const string ControlUserId = "trainee-t258-control";
    private const string ChairUserId = "chair-t258";
    private const string MemberUserId = "member-t258";
    private const string CoordinatorUserId = "coordinator-t258";
    private const string Salt = "salt-for-tests";

    /// <summary>
    /// 22:30 UTC on 24 September, which is already 25 September in South Africa: the erasure day is the South African one.
    /// </summary>
    private static readonly DateTime ErasedAt = new(2026, 9, 24, 22, 30, 0, DateTimeKind.Utc);

    private static readonly DateOnly ErasureDay = new(2026, 9, 25);

    private readonly TestSchemas _schemas = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _schemas.DropAllAsync();

    [Fact]
    public async Task OnPostgres_ErasingATrainee_EndsEveryProfile_WithdrawsWhatIsOpen_AndKeepsWhatIsSettled()
    {
        try
        {
            await using var root = await MigratedAndSeededServicesAsync();
            var seeded = await ArrangeAsync(root);

            await using (var act = root.CreateAsyncScope())
            {
                await Approver(act.ServiceProvider).Handle(
                    new ApproveDataRightsRequestCommand(seeded.RequestId, "Erased at the trainee's request.", Coordinator(seeded.HostId)),
                    CancellationToken.None);
            }

            await using var read = root.CreateAsyncScope();
            var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var pseudonym = (await db.DataRightsErasureRecords.SingleAsync(record => record.UserId == ErasedUserId)).Pseudonym;
            pseudonym.Should().StartWith("deleted_user_");

            (await db.DataRightsRequests.SingleAsync(request => request.Id == seeded.RequestId)).Status
                .Should().Be(DataRightsRequestStatus.Completed);

            // Every profile moved, none left under the id, and every one ended.
            (await db.TraineeProfiles.AnyAsync(profile => profile.UserId == ErasedUserId)).Should().BeFalse();
            (await db.AssessorProfiles.AnyAsync(profile => profile.UserId == ErasedUserId)).Should().BeFalse();

            var profiles = await db.TraineeProfiles.Where(profile => profile.UserId == pseudonym)
                .OrderBy(profile => profile.ProgrammeStartDate)
                .ToListAsync();
            profiles.Should().HaveCount(2, "the erased trainee held an earlier profile, completed, and a current one");
            profiles.Should().OnlyContain(profile => !profile.IsActive);
            profiles[0].CompletedOn.Should().Be(new DateOnly(2024, 1, 10), "a profile already ended keeps the end it recorded");
            profiles[0].DeactivatedOn.Should().BeNull();
            profiles[1].DeactivatedOn.Should().Be(ErasureDay, "the current profile ends on the erasure day, on the South African calendar");
            profiles[1].CompletedOn.Should().BeNull();

            (await db.AssessorProfiles.CountAsync(profile => profile.UserId == pseudonym)).Should().Be(1);

            // No open review is left, under either id: each was withdrawn with its reason.
            (await db.CommitteeReviews.AnyAsync(review => review.TraineeUserId == ErasedUserId)).Should().BeFalse();
            var reviews = await db.CommitteeReviews.Include(review => review.Decisions)
                .Where(review => review.TraineeUserId == pseudonym)
                .ToDictionaryAsync(review => review.Id);
            reviews.Values.Should().NotContain(review => CommitteeReview.OpenStates.Contains(review.State));

            foreach (var withdrawnId in new[] { seeded.ScheduledReviewId, seeded.InProgressReviewId, seeded.DecidedReviewId })
            {
                var review = reviews[withdrawnId];
                review.State.Should().Be(CommitteeReviewState.Withdrawn);
                review.WithdrawnOn.Should().Be(ErasedAt);
                review.WithdrawalReason.Should().Be(CommitteeReview.WithdrawnTraineeErased);
            }

            reviews[seeded.DecidedReviewId].Decisions.Should().ContainSingle("a decision recorded but never ratified stays as the record");

            // Nothing staged is left for a review nobody will ratify.
            (await db.PendingEntrustmentDecisions.AnyAsync(pending => pending.ReviewId == seeded.InProgressReviewId)).Should().BeFalse();

            // What was settled is kept, under the pseudonym.
            reviews[seeded.RatifiedReviewId].State.Should().Be(CommitteeReviewState.Ratified);
            reviews[seeded.RatifiedReviewId].WithdrawnOn.Should().BeNull();
            var star = await db.EntrustmentDecisions.SingleAsync(decision => decision.Id == seeded.StarId);
            star.TraineeUserId.Should().Be(pseudonym);
            star.Status.Should().Be(EntrustmentDecisionStatus.Active);

            // A review under appeal is settled too (its decision is ratified), and kept as it was, the appeal still open
            // and lodged under the pseudonym.
            reviews[seeded.UnderAppealReviewId].State.Should().Be(CommitteeReviewState.UnderAppeal);
            reviews[seeded.UnderAppealReviewId].WithdrawnOn.Should().BeNull();
            var appeal = await db.CommitteeAppeals.SingleAsync(entity => entity.ReviewId == seeded.UnderAppealReviewId);
            appeal.LodgedByUserId.Should().Be(pseudonym);
            appeal.ResolvedOn.Should().BeNull();

            // No campaign about the person is left to invite, open, close or release; a withdrawn one keeps no address.
            (await db.MsfCampaigns.AnyAsync(campaign => campaign.SubjectUserId == ErasedUserId)).Should().BeFalse();
            var campaigns = await db.MsfCampaigns.Include(campaign => campaign.Invitations)
                .Where(campaign => campaign.SubjectUserId == pseudonym)
                .ToDictionaryAsync(campaign => campaign.Id);

            foreach (var withdrawnId in new[]
                     {
                         seeded.DraftCampaignId, seeded.OpenCampaignId, seeded.ClosedCampaignId, seeded.UnderReviewCampaignId
                     })
            {
                var campaign = campaigns[withdrawnId];
                campaign.State.Should().Be(MsfCampaignState.Withdrawn);
                campaign.WithdrawnOn.Should().Be(ErasedAt);
                campaign.Invitations.Should().NotBeEmpty().And.OnlyContain(invitation => invitation.RespondentEmail == null);
            }

            campaigns.Values.Should().OnlyContain(campaign =>
                campaign.State == MsfCampaignState.Withdrawn || campaign.State == MsfCampaignState.Released);
            campaigns[seeded.ReleasedCampaignId].State.Should().Be(MsfCampaignState.Released, "a released report is settled");

            // Nobody else's records are touched.
            (await db.TraineeProfiles.SingleAsync(profile => profile.UserId == ControlUserId)).IsActive.Should().BeTrue();
            (await db.CommitteeReviews.SingleAsync(review => review.Id == seeded.ControlReviewId)).State
                .Should().Be(CommitteeReviewState.Scheduled);
            var control = await db.MsfCampaigns.Include(campaign => campaign.Invitations)
                .SingleAsync(campaign => campaign.Id == seeded.ControlCampaignId);
            control.State.Should().Be(MsfCampaignState.Open);
            control.Invitations.Should().OnlyContain(invitation => invitation.RespondentEmail != null);

            // And the appeal body can still answer the appeal the erased trainee lodged, through the real handler and user
            // store: resolving it reads no evidence and issues no STAR, and the review's trainee is not asked for (T182).
            await using (var resolve = root.CreateAsyncScope())
            {
                var context = resolve.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var users = new UserAdministrationService(
                    resolve.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>(), context);

                var resolved = await new ResolveAppealCommandHandler(context, users).Handle(
                    new ResolveAppealCommand(
                        seeded.UnderAppealReviewId, CommitteeAppealOutcome.Dismissed, null, null, null, null, Chair(seeded.HostId)),
                    CancellationToken.None);

                resolved.State.Should().Be(CommitteeReviewState.Final);
                resolved.TraineeUserId.Should().Be(pseudonym);
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    [Fact]
    public async Task OnPostgres_AnErasureThatFails_ChangesNothing_AndTheRequestCanBeApprovedAgain()
    {
        try
        {
            await using var root = await MigratedAndSeededServicesAsync();
            var seeded = await ArrangeAsync(root);

            // The database refuses the erasure's very last write, after every raw UPDATE and every save UserManager makes
            // on the way.
            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.ExecuteSqlRawAsync(
                    """
                    CREATE FUNCTION "t258_refuse_erasure_record"() RETURNS trigger LANGUAGE plpgsql AS
                    $$ BEGIN RAISE EXCEPTION 'T258: the erasure record is refused'; END $$;
                    CREATE TRIGGER "t258_refuse_erasure_record" BEFORE INSERT ON "DataRightsErasureRecords"
                    FOR EACH ROW EXECUTE FUNCTION "t258_refuse_erasure_record"();
                    """);
            }

            await using (var act = root.CreateAsyncScope())
            {
                var context = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var approve = () => Approver(act.ServiceProvider).Handle(
                    new ApproveDataRightsRequestCommand(seeded.RequestId, "Erased at the trainee's request.", Coordinator(seeded.HostId)),
                    CancellationToken.None);

                await approve.Should().ThrowAsync<DbUpdateException>();

                // As the audit pipeline would, from its catch: its ordinary write saves whatever is still tracked.
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
            }

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                (await db.DataRightsRequests.SingleAsync(request => request.Id == seeded.RequestId)).Status
                    .Should().Be(DataRightsRequestStatus.Submitted, "a refused erasure leaves the request to be approved again");
                (await db.DataRightsErasureRecords.AnyAsync()).Should().BeFalse();

                // The tracked changes, those UserManager saved on the way, and the raw UPDATEs: none of it stands.
                (await db.TraineeProfiles.CountAsync(profile => profile.UserId == ErasedUserId)).Should().Be(2);
                (await db.TraineeProfiles.CountAsync(profile => profile.UserId == ErasedUserId && profile.IsActive)).Should().Be(1);
                (await db.AssessorProfiles.CountAsync(profile => profile.UserId == ErasedUserId)).Should().Be(1);
                (await db.Users.SingleAsync(user => user.Id == ErasedUserId)).Email.Should().Be($"{ErasedUserId}@test.local");
                (await db.UserRoles.CountAsync(link => link.UserId == ErasedUserId)).Should().Be(2);
                (await db.EntrustmentDecisions.SingleAsync(decision => decision.Id == seeded.StarId)).TraineeUserId
                    .Should().Be(ErasedUserId, "the raw UPDATE of the STAR was rolled back with the rest");
                (await db.CommitteeReviews.CountAsync(review =>
                        review.TraineeUserId == ErasedUserId && CommitteeReview.OpenStates.Contains(review.State)))
                    .Should().Be(3);
                (await db.PendingEntrustmentDecisions.CountAsync(pending => pending.ReviewId == seeded.InProgressReviewId))
                    .Should().Be(1);
                (await db.MsfCampaigns.CountAsync(campaign =>
                        campaign.SubjectUserId == ErasedUserId && campaign.State != MsfCampaignState.Withdrawn))
                    .Should().Be(5);
                (await db.MsfInvitations.CountAsync(invitation =>
                        invitation.CampaignId == seeded.OpenCampaignId && invitation.RespondentEmail != null))
                    .Should().Be(1);

                await db.Database.ExecuteSqlRawAsync(
                    "DROP TRIGGER \"t258_refuse_erasure_record\" ON \"DataRightsErasureRecords\"");
            }

            // Approved again, it completes.
            await using (var retry = root.CreateAsyncScope())
            {
                await Approver(retry.ServiceProvider).Handle(
                    new ApproveDataRightsRequestCommand(seeded.RequestId, "Erased at the trainee's request.", Coordinator(seeded.HostId)),
                    CancellationToken.None);
            }

            await using (var read = root.CreateAsyncScope())
            {
                var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                (await db.DataRightsRequests.SingleAsync(request => request.Id == seeded.RequestId)).Status
                    .Should().Be(DataRightsRequestStatus.Completed);
                (await db.TraineeProfiles.AnyAsync(profile => profile.UserId == ErasedUserId)).Should().BeFalse();
                (await db.CommitteeReviews.AnyAsync(review => CommitteeReview.OpenStates.Contains(review.State) &&
                                                              review.Id != seeded.ControlReviewId))
                    .Should().BeFalse();
            }
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T258 review: the request's completion is written with the erasure, in its transaction. Written after it, by the
    /// handler's own save, a refused completion left the erasure standing under a request merely Approved, which cannot
    /// be approved again. Here the database refuses exactly that write, the request's move to Completed.
    /// </summary>
    [Fact]
    public async Task OnPostgres_ACompletionTheDatabaseRefuses_ErasesNothing_AndTheRequestCanBeApprovedAgain()
    {
        try
        {
            await using var root = await MigratedAndSeededServicesAsync();
            var seeded = await ArrangeAsync(root);

            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var completed = (int)DataRightsRequestStatus.Completed;
                var refuseCompletion =
                    $"""
                    CREATE FUNCTION "t258_refuse_completion"() RETURNS trigger LANGUAGE plpgsql AS
                    $$ BEGIN
                        IF NEW."Status" = {completed} THEN
                            RAISE EXCEPTION 'T258: the completion is refused';
                        END IF;
                        RETURN NEW;
                    END $$;
                    CREATE TRIGGER "t258_refuse_completion" BEFORE UPDATE ON "DataRightsRequests"
                    FOR EACH ROW EXECUTE FUNCTION "t258_refuse_completion"();
                    """;
                await db.Database.ExecuteSqlRawAsync(refuseCompletion);
            }

            await ApproveAndFailAsync<DbUpdateException>(root, seeded);
            await ShouldBeUnerasedAsync(root, seeded);

            await DropTriggerAsync(root, "t258_refuse_completion", "DataRightsRequests");
            await ApproveAgainAndCompleteAsync(root, seeded);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// T258 review: a row about the person that changes between the erasure reading it and saving (here every feedback
    /// campaign, as the auto-close job or a coordinator would) fails the save on its concurrency token. The approver is
    /// told so in words, not EF's message about row counts, with EF's exception inside for the audit pipeline, and nothing
    /// is erased. The database stands in for the concurrent writer: a trigger that skips the update is what a changed
    /// token looks like to EF, an update that touched no row.
    /// </summary>
    [Fact]
    public async Task OnPostgres_ACampaignChangedDuringTheErasure_IsRefusedInWords_AndErasesNothing()
    {
        try
        {
            await using var root = await MigratedAndSeededServicesAsync();
            var seeded = await ArrangeAsync(root);

            await using (var arrange = root.CreateAsyncScope())
            {
                var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await db.Database.ExecuteSqlRawAsync(
                    """
                    CREATE FUNCTION "t258_skip_campaign_update"() RETURNS trigger LANGUAGE plpgsql AS
                    $$ BEGIN RETURN NULL; END $$;
                    CREATE TRIGGER "t258_skip_campaign_update" BEFORE UPDATE ON "MsfCampaigns"
                    FOR EACH ROW EXECUTE FUNCTION "t258_skip_campaign_update"();
                    """);
            }

            var refusal = await ApproveAndFailAsync<InvalidOperationException>(root, seeded);
            refusal.Message.Should().Be(ErasureExecutor.PersonChanged);
            refusal.InnerException.Should().BeOfType<DbUpdateConcurrencyException>();
            await ShouldBeUnerasedAsync(root, seeded);

            await DropTriggerAsync(root, "t258_skip_campaign_update", "MsfCampaigns");
            await ApproveAgainAndCompleteAsync(root, seeded);
        }
        finally
        {
            await _schemas.DropAllAsync();
        }
    }

    /// <summary>
    /// Approves the request and expects the refusal; then, as the audit pipeline would from its catch, saves whatever is
    /// still tracked (its ordinary write flushes it) and clears.
    /// </summary>
    private static async Task<TException> ApproveAndFailAsync<TException>(ServiceProvider root, Seeded seeded)
        where TException : Exception
    {
        await using var act = root.CreateAsyncScope();
        var context = act.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var approve = () => Approver(act.ServiceProvider).Handle(
            new ApproveDataRightsRequestCommand(seeded.RequestId, "Erased at the trainee's request.", Coordinator(seeded.HostId)),
            CancellationToken.None);

        var thrown = (await approve.Should().ThrowAsync<TException>()).Which;

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return thrown;
    }

    /// <summary>Nothing of the erasure stands, and the request is as the trainee left it.</summary>
    private static async Task ShouldBeUnerasedAsync(ServiceProvider root, Seeded seeded)
    {
        await using var read = root.CreateAsyncScope();
        var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        (await db.DataRightsRequests.SingleAsync(request => request.Id == seeded.RequestId)).Status
            .Should().Be(DataRightsRequestStatus.Submitted, "a refused erasure leaves the request to be approved again");
        (await db.DataRightsErasureRecords.AnyAsync()).Should().BeFalse();
        (await db.TraineeProfiles.CountAsync(profile => profile.UserId == ErasedUserId && profile.IsActive)).Should().Be(1);
        (await db.Users.SingleAsync(user => user.Id == ErasedUserId)).Email.Should().Be($"{ErasedUserId}@test.local");
        (await db.EntrustmentDecisions.SingleAsync(decision => decision.Id == seeded.StarId)).TraineeUserId
            .Should().Be(ErasedUserId);
        (await db.CommitteeReviews.CountAsync(review =>
                review.TraineeUserId == ErasedUserId && CommitteeReview.OpenStates.Contains(review.State)))
            .Should().Be(3);
        (await db.MsfCampaigns.CountAsync(campaign =>
                campaign.SubjectUserId == ErasedUserId && campaign.State != MsfCampaignState.Withdrawn))
            .Should().Be(5);
    }

    private static async Task DropTriggerAsync(ServiceProvider root, string trigger, string table)
    {
        await using var scope = root.CreateAsyncScope();
        var drop = $"DROP TRIGGER \"{trigger}\" ON \"{table}\"";
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.ExecuteSqlRawAsync(drop);
    }

    private static async Task ApproveAgainAndCompleteAsync(ServiceProvider root, Seeded seeded)
    {
        await using (var retry = root.CreateAsyncScope())
        {
            await Approver(retry.ServiceProvider).Handle(
                new ApproveDataRightsRequestCommand(seeded.RequestId, "Erased at the trainee's request.", Coordinator(seeded.HostId)),
                CancellationToken.None);
        }

        await using var read = root.CreateAsyncScope();
        var db = read.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.DataRightsRequests.SingleAsync(request => request.Id == seeded.RequestId)).Status
            .Should().Be(DataRightsRequestStatus.Completed);
        (await db.TraineeProfiles.AnyAsync(profile => profile.UserId == ErasedUserId)).Should().BeFalse();
    }

    private sealed record Seeded(
        int HostId,
        Guid RequestId,
        int ScheduledReviewId,
        int InProgressReviewId,
        int DecidedReviewId,
        int RatifiedReviewId,
        int UnderAppealReviewId,
        int ControlReviewId,
        int StarId,
        int DraftCampaignId,
        int OpenCampaignId,
        int ClosedCampaignId,
        int UnderReviewCampaignId,
        int ReleasedCampaignId,
        int ControlCampaignId);

    /// <summary>
    /// The erased trainee (also an assessor) with an earlier profile completed and a current one; their reviews scheduled,
    /// in progress with a decision staged, decided, ratified with a STAR, and ratified and under their appeal; and their
    /// campaigns in draft, open, closed, under review and released. Beside them, a trainee who is not erased, with a review
    /// and a campaign of their own open. Last, the trainee's erasure request, submitted through the real command.
    /// </summary>
    private static async Task<Seeded> ArrangeAsync(ServiceProvider root)
    {
        await using var arrange = root.CreateAsyncScope();
        var db = arrange.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = new DateTime(2026, 9, 20, 9, 0, 0, DateTimeKind.Utc);

        var hostId = await db.Institutions.Where(entity => entity.ShortCode == "DEMO").Select(entity => entity.Id).SingleAsync();
        var curriculumId = await db.Curricula.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();
        var epaId = await db.Epas.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();
        var levelId = await db.EntrustmentLevels.OrderBy(entity => entity.Id).Select(entity => entity.Id).FirstAsync();

        NomineeSeed.AddUser(db, ErasedUserId, hostId, WombatRoles.Trainee, WombatRoles.Assessor);
        NomineeSeed.AddUser(db, ControlUserId, hostId, WombatRoles.Trainee);
        // The chair holds an account, so the appeal body can act at the review under appeal after the erasure (T237).
        NomineeSeed.AddUser(db, ChairUserId, hostId, WombatRoles.CommitteeMember);

        var earlier = Profile(ErasedUserId, hostId, curriculumId, new DateOnly(2020, 1, 15));
        earlier.Complete(new DateOnly(2024, 1, 10), ErasureDay);
        db.TraineeProfiles.AddRange(
            earlier,
            Profile(ErasedUserId, hostId, curriculumId, new DateOnly(2025, 1, 15)),
            Profile(ControlUserId, hostId, curriculumId, new DateOnly(2025, 1, 15)));
        db.AssessorProfiles.Add(new AssessorProfile { UserId = ErasedUserId, InstitutionId = hostId, Qualifications = "MBChB" });

        var panel = new DecisionPanel
        {
            Name = "T258 CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = hostId,
            CreatedOn = now,
            Members =
            [
                new DecisionPanelMember { UserId = ChairUserId, Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = MemberUserId, Role = DecisionPanelMemberRole.Member }
            ]
        };
        db.DecisionPanels.Add(panel);

        var template = new MsfTemplate
        {
            Name = "T258 MSF",
            Questions = [new MsfQuestion { Order = 1, Prompt = "Overall", Type = MsfQuestionType.Scale, Required = true }]
        };
        db.MsfTemplates.Add(template);
        await db.SaveChangesAsync();

        IReadOnlyCollection<DecisionPanelMember> present =
        [
            new DecisionPanelMember { UserId = ChairUserId, Role = DecisionPanelMemberRole.Chair },
            new DecisionPanelMember { UserId = MemberUserId, Role = DecisionPanelMemberRole.Member }
        ];

        var scheduled = Review(ErasedUserId, panel.Id, 2026, 2);
        var inProgress = Review(ErasedUserId, panel.Id, 2026, 1);
        inProgress.Start(
            [
                new CommitteeEvidence
                {
                    SourceType = CommitteeEvidenceSourceType.Activity,
                    ActivityId = 1,
                    SourceLabel = "Mini-CEX #1",
                    Summary = "State: completed.",
                    SourceRecordedOn = now
                }
            ],
            ChairUserId,
            now);
        var decided = Review(ErasedUserId, panel.Id, 2025, 2);
        decided.Start([], ChairUserId, now);
        decided.RecordDecision(CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, ChairUserId, now, present, [], []);
        var ratified = Review(ErasedUserId, panel.Id, 2025, 1);
        ratified.Start([], ChairUserId, now);
        ratified.RecordDecision(CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, ChairUserId, now, present, [], []);
        ratified.Ratify(ChairUserId, now);
        var underAppeal = Review(ErasedUserId, panel.Id, 2024, 2);
        underAppeal.Start([], ChairUserId, now);
        underAppeal.RecordDecision(CommitteeDecisionCategory.SatisfactoryProgress, "On track.", null, ChairUserId, now, present, [], []);
        underAppeal.Ratify(ChairUserId, now);
        underAppeal.LodgeAppeal("The panel did not weigh my last rotation.", ErasedUserId, now);
        var controlReview = Review(ControlUserId, panel.Id, 2026, 2);
        db.CommitteeReviews.AddRange(scheduled, inProgress, decided, ratified, underAppeal, controlReview);

        var draft = Campaign(ErasedUserId, template.Id, MsfCampaignState.Draft, now, "draft.nurse@host.test");
        var open = Campaign(ErasedUserId, template.Id, MsfCampaignState.Open, now, "open.nurse@host.test");
        // Nothing produces Closed today, and Withdraw accepts it: the erasure asks exactly what Withdraw accepts (T258 review).
        var closed = Campaign(ErasedUserId, template.Id, MsfCampaignState.Closed, now, respondent: null);
        var underReview = Campaign(ErasedUserId, template.Id, MsfCampaignState.UnderReview, now, respondent: null);
        var released = Campaign(ErasedUserId, template.Id, MsfCampaignState.Released, now, respondent: null);
        var controlCampaign = Campaign(ControlUserId, template.Id, MsfCampaignState.Open, now, "control.nurse@host.test");
        db.MsfCampaigns.AddRange(draft, open, closed, underReview, released, controlCampaign);
        await db.SaveChangesAsync();

        db.PendingEntrustmentDecisions.Add(PendingEntrustmentDecision.Stage(
            inProgress.Id, epaId, levelId, new DateOnly(2026, 9, 20), null, "Consistent across the window.",
            [inProgress.EvidenceItems.Single().Id], ChairUserId, now));
        var star = EntrustmentDecision.Issue(
            ErasedUserId, epaId, levelId, new DateOnly(2025, 7, 10), expiresOn: null, ratified.Id, ChairUserId,
            "Consistent across the period.", StarEvidence.One());
        db.EntrustmentDecisions.Add(star);
        await db.SaveChangesAsync();

        // The trainee asks through the real form's command, with their reviews open and one under appeal: no review
        // refuses the request (T258 review). Before, T026's guard refused it, and approval withdrew the same reviews for
        // a trainee who had asked before they were scheduled.
        var request = await new SubmitDataRightsRequestCommandHandler(db).Handle(
            new SubmitDataRightsRequestCommand(DataRightsRequestType.Erasure, "Leaving the programme.", Trainee(hostId)),
            CancellationToken.None);
        request.Status.Should().Be(DataRightsRequestStatus.Submitted);

        return new Seeded(
            hostId,
            request.Id,
            scheduled.Id,
            inProgress.Id,
            decided.Id,
            ratified.Id,
            underAppeal.Id,
            controlReview.Id,
            star.Id,
            draft.Id,
            open.Id,
            closed.Id,
            underReview.Id,
            released.Id,
            controlCampaign.Id);
    }

    /// <summary>The erased trainee, signed in, as the data-rights form sends their request.</summary>
    private static ClaimsPrincipal Trainee(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, ErasedUserId),
                new Claim(ClaimTypes.Name, "Erased Trainee"),
                new Claim(ClaimTypes.Role, WombatRoles.Trainee),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>The panel's chair, who sits on the appeal body (T165).</summary>
    private static ClaimsPrincipal Chair(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, ChairUserId),
                new Claim(ClaimTypes.Role, WombatRoles.CommitteeMember),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    private static TraineeProfile Profile(string userId, int institutionId, int curriculumId, DateOnly start) => new()
    {
        UserId = userId,
        InstitutionId = institutionId,
        CurriculumId = curriculumId,
        ProgrammeStartDate = start,
        ExpectedCompletionDate = start.AddYears(4).AddDays(-1)
    };

    private static CommitteeReview Review(string traineeUserId, int panelId, int year, int semester) => new()
    {
        AcademicYear = year,
        Semester = semester,
        PanelId = panelId,
        TraineeUserId = traineeUserId,
        ReviewPeriodFrom = new DateOnly(year, semester == 1 ? 1 : 7, 1),
        ReviewPeriodTo = semester == 1 ? new DateOnly(year, 6, 30) : new DateOnly(year, 12, 31),
        ScheduledOn = new DateOnly(2026, 9, 1)
    };

    /// <summary>
    /// A campaign in the state named, as that state leaves it: a draft's and an open campaign's invitee still has an
    /// address; closing anonymised the invitee of one under review or released.
    /// </summary>
    private static MsfCampaign Campaign(string subjectUserId, int templateId, MsfCampaignState state, DateTime now, string? respondent)
    {
        var closed = state is MsfCampaignState.Closed or MsfCampaignState.UnderReview or MsfCampaignState.Released;
        return new MsfCampaign
        {
            SubjectUserId = subjectUserId,
            TemplateId = templateId,
            CreatedByUserId = CoordinatorUserId,
            CreatedOn = now,
            OpensOn = new DateOnly(2026, 9, 1),
            ClosesOn = new DateOnly(2026, 10, 15),
            MinimumResponses = 1,
            MinimumCategoryResponses = 1,
            MinimumRespondentCategories = 1,
            State = state,
            OpenedOn = state == MsfCampaignState.Draft ? null : now,
            ClosedOn = closed ? now : null,
            ReleasedOn = state == MsfCampaignState.Released ? now : null,
            ReviewedByUserId = state == MsfCampaignState.Released ? CoordinatorUserId : null,
            Invitations =
            [
                new MsfInvitation
                {
                    RespondentEmail = respondent,
                    RespondentCategory = MsfRespondentCategory.Nurse,
                    TokenHash = $"hash-{Guid.NewGuid():N}",
                    IssuedOn = now,
                    ExpiresOn = new DateOnly(2026, 10, 22),
                    AnonymizedOn = closed ? now : null
                }
            ]
        };
    }

    private static ApproveDataRightsRequestCommandHandler Approver(IServiceProvider services)
        => new(
            services.GetRequiredService<ApplicationDbContext>(),
            services.GetRequiredService<ErasureExecutor>(),
            new NoAccessReports(),
            Options.Create(new WombatOptions { PseudonymSalt = Salt }));

    /// <summary>A coordinator of the request's institution: the role that approves a data-rights request (T112).</summary>
    private static ClaimsPrincipal Coordinator(int institutionId)
        => new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, CoordinatorUserId),
                new Claim(ClaimTypes.Role, WombatRoles.Coordinator),
                new Claim(WombatClaimTypes.InstitutionId, institutionId.ToString(CultureInfo.InvariantCulture))
            ],
            "IntegrationTest",
            ClaimTypes.Name,
            ClaimTypes.Role));

    /// <summary>An erasure builds no access report.</summary>
    private sealed class NoAccessReports : IAccessReportBuilder
    {
        public Task<AccessExportResult> BuildAsync(string userId, CancellationToken cancellationToken)
            => throw new NotSupportedException("An erasure builds no access report.");
    }

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow);
    }

    private async Task<ServiceProvider> MigratedAndSeededServicesAsync()
    {
        var schema = await _schemas.CreateAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(TestDatabase.SchemaConnectionString(schema)));
        services.AddIdentity<WombatIdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
        services.AddSingleton<TimeProvider>(new FixedClock(ErasedAt));
        services.AddScoped<ErasureExecutor>();

        var root = services.BuildServiceProvider();

        await using (var scope = root.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
        }

        await using (var scope = root.CreateAsyncScope())
        {
            await new DataSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
        }

        return root;
    }
}
