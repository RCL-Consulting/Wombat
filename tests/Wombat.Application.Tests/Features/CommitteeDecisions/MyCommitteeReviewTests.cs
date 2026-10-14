using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Common.Security;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// The trainee's own committee reviews (T366, flow 09): My committee reviews newest first (F1), her review's page at
/// <c>/committee/my-reviews/{id}</c> read only by its own trainee once it is ratified or later, for every role (F8), with
/// the appeal body named while it is still Ratified, the panel's institution by name, and who lodged and resolved each
/// appeal by name (T308); the lodge's refusals in Wombat's words (M3).
/// </summary>
public sealed class MyCommitteeReviewTests
{
    private const int Institution = 1;
    private const string Trainee = "trainee-1";

    private static readonly (string UserId, string Name)[] People =
    [
        ("chair-1", "Thandi Zulu"),
        ("member-1", "Sarah Botha"),
        ("external-1", "John van Rensburg"),
        (Trainee, "Nomsa Mahlangu"),
    ];

    // ─── My committee reviews (F1) ───────────────────────────────────────────

    [Fact]
    public async Task TheList_IsNewestFirst_ByScheduledDateThenReviewNumber_BothDescending()
    {
        await using var db = SeededDb();
        var first = await RatifiedReviewAsync(db, new DateOnly(2026, 10, 13));
        var second = await RatifiedReviewAsync(db, new DateOnly(2026, 10, 13));
        var earlier = await RatifiedReviewAsync(db, new DateOnly(2026, 4, 1));

        var reviews = await new ListReviewsForTraineeQueryHandler(db).Handle(
            new ListReviewsForTraineeQuery(Trainee, TraineePrincipal()), CancellationToken.None);

        reviews.Select(review => review.Id).Should().Equal(
            [second, first, earlier], "one day's ties read the later review first, as Dr Molefe's #7 above #1 (F1)");
    }

    // ─── Her review's page (F8) ──────────────────────────────────────────────

    [Fact]
    public async Task HerRatifiedReview_NamesTheAppealBody_AndThePanelsInstitution()
    {
        await using var db = SeededDb();
        var reviewId = await RatifiedReviewAsync(db);

        var review = await ReadMineAsync(db, reviewId, TraineePrincipal());

        review.Should().NotBeNull();
        review!.State.Should().Be(CommitteeReviewState.Ratified);
        review.AppealBody.Select(person => person.Label).Should().Equal(
            ["Thandi Zulu (chair)", "John van Rensburg (external)"],
            "What happens next names who would hear her appeal before she lodges it (round 2's data row)");
        review.PanelInstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        review.Decisions.Single().Attendees.Select(person => person.Label).Should().Equal("Thandi Zulu (chair)", "Sarah Botha");
        review.TraineeName.Should().Be("Nomsa Mahlangu");
    }

    [Theory]
    [InlineData(WombatRoles.Trainee)]
    [InlineData(WombatRoles.Administrator)]
    public async Task AnotherTraineesReview_ReadsAsNotFound_ForEveryRole(string role)
    {
        await using var db = SeededDb();
        var reviewId = await RatifiedReviewAsync(db);

        (await ReadMineAsync(db, reviewId, Principal("trainee-2", role))).Should().BeNull(
            "an Administrator reads a review at the committee's address, never here (F8)");
    }

    [Fact]
    public async Task AnUnknownId_ReadsAsNotFound()
    {
        await using var db = SeededDb();

        (await ReadMineAsync(db, 999, TraineePrincipal())).Should().BeNull();
    }

    [Theory]
    [InlineData(CommitteeReviewState.Scheduled)]
    [InlineData(CommitteeReviewState.InProgress)]
    [InlineData(CommitteeReviewState.Decided)]
    public async Task HerOwnReview_BeforeItIsRatified_ReadsAsNotFound(CommitteeReviewState state)
    {
        await using var db = SeededDb();
        var reviewId = await ReviewInStateAsync(db, state);

        (await ReadMineAsync(db, reviewId, TraineePrincipal())).Should().BeNull(
            "before ratification a review is the panel's working record, not yet hers");
    }

    [Fact]
    public async Task HerOwnReview_IsHers_WhateverRoleSheActsIn()
    {
        await using var db = SeededDb();
        var reviewId = await RatifiedReviewAsync(db);

        (await ReadMineAsync(db, reviewId, Principal(Trainee, WombatRoles.Administrator, WombatRoles.Trainee))).Should().NotBeNull(
            "the check is whose review it is, not the role (F8)");
    }

    [Fact]
    public async Task AfterLodging_TheReviewReadAgain_IsUnderAppeal_WithTheOpenEntryAndWhoLodgedIt()
    {
        await using var db = SeededDb();
        var reviewId = await RatifiedReviewAsync(db);

        await new LodgeAppealCommandHandler(db).Handle(
            new LodgeAppealCommand(reviewId, "The Mini-CEX waited on the assessor.", TraineePrincipal()), CancellationToken.None);
        var review = await ReadMineAsync(db, reviewId, TraineePrincipal());

        review!.State.Should().Be(CommitteeReviewState.UnderAppeal);
        var appeal = review.Appeals.Should().ContainSingle().Subject;
        appeal.Outcome.Should().BeNull();
        appeal.LodgedByName.Should().Be("Nomsa Mahlangu");
        appeal.ResolvedByName.Should().BeNull();
        review.AppealBody.Select(person => person.Label).Should().Equal(
            "Thandi Zulu (chair)", "John van Rensburg (external)");
    }

    [Fact]
    public async Task AResolvedAppeal_NamesWhoLodgedAndWhoResolvedIt_ForTheTraineeAndTheCommittee()
    {
        await using var db = SeededDb();
        var reviewId = await RatifiedReviewAsync(db);
        await new LodgeAppealCommandHandler(db).Handle(
            new LodgeAppealCommand(reviewId, "The Mini-CEX waited on the assessor.", TraineePrincipal()), CancellationToken.None);
        await new ResolveAppealCommandHandler(db, Directory(db)).Handle(
            new ResolveAppealCommand(reviewId, CommitteeAppealOutcome.Dismissed, null, null, null, null,
                Principal("chair-1", WombatRoles.CommitteeMember)),
            CancellationToken.None);

        var hers = await ReadMineAsync(db, reviewId, TraineePrincipal());
        var committees = await new GetCommitteeReviewByIdQueryHandler(db, Directory(db)).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, Principal("member-1", WombatRoles.CommitteeMember)), CancellationToken.None);

        foreach (var review in new[] { hers!, committees })
        {
            var appeal = review.Appeals.Should().ContainSingle().Subject;
            appeal.LodgedByName.Should().Be("Nomsa Mahlangu");
            appeal.ResolvedByName.Should().Be("Thandi Zulu", "T308: resolved by name, not by id");
        }

        hers!.State.Should().Be(CommitteeReviewState.Final);
    }

    [Fact]
    public async Task TheCommitteesPage_NamesNoAppealBody_WhileTheReviewIsRatified()
    {
        await using var db = SeededDb();
        var reviewId = await RatifiedReviewAsync(db);

        var review = await new GetCommitteeReviewByIdQueryHandler(db, Directory(db)).Handle(
            new GetCommitteeReviewByIdQuery(reviewId, Principal("member-1", WombatRoles.CommitteeMember)), CancellationToken.None);

        review.AppealBody.Should().BeEmpty("only her own page reads it before an appeal is lodged");
    }

    // ─── Lodging (M3) ────────────────────────────────────────────────────────

    [Fact]
    public async Task ASecondLodge_IsRefused_InTheReviewsOwnWords()
    {
        await using var db = SeededDb();
        var reviewId = await RatifiedReviewAsync(db);
        var handler = new LodgeAppealCommandHandler(db);
        await handler.Handle(new LodgeAppealCommand(reviewId, "First tab.", TraineePrincipal()), CancellationToken.None);

        var second = () => handler.Handle(new LodgeAppealCommand(reviewId, "Second tab.", TraineePrincipal()), CancellationToken.None);

        (await second.Should().ThrowAsync<CommitteeRefusedException>())
            .WithMessage("Only ratified reviews can be appealed.");
    }

    [Fact]
    public async Task ALodgeThatLosesTheRaceAtTheSave_IsRefusedAsTheSecondTabsLodgeIs()
    {
        // T366, R6: two tabs read the review Ratified and both lodge; the loser's save trips the review's xmin. The appeal
        // exists, so the loser is told the review's own rule, not "could not be lodged".
        var save = new FailArmedSave();
        await using var db = SeededDb(save);
        var reviewId = await RatifiedReviewAsync(db);
        save.Arm(new DbUpdateConcurrencyException("induced: the review changed since it was read"));

        var lodge = () => new LodgeAppealCommandHandler(db).Handle(
            new LodgeAppealCommand(reviewId, "Second tab.", TraineePrincipal()), CancellationToken.None);

        var refusal = (await lodge.Should().ThrowAsync<CommitteeRefusedException>())
            .WithMessage(CommitteeReview.OnlyRatifiedAppealed).Which;
        refusal.InnerException.Should().BeOfType<DbUpdateConcurrencyException>("the audit pipeline still sees a refused save (T201)");
    }

    [Fact]
    public async Task AResolveTheReviewsOwnRulesRefuse_IsACommitteeRefusal_InItsWords()
    {
        // T366, R1: the committee's page shows a CommitteeRefusedException's words, and anything else as a fixed sentence.
        await using var db = SeededDb();
        var reviewId = await RatifiedReviewAsync(db);

        var resolve = () => new ResolveAppealCommandHandler(db, Directory(db)).Handle(
            new ResolveAppealCommand(reviewId, CommitteeAppealOutcome.Dismissed, null, null, null, null,
                Principal("chair-1", WombatRoles.CommitteeMember)),
            CancellationToken.None);

        (await resolve.Should().ThrowAsync<CommitteeRefusedException>())
            .WithMessage("Only reviews under appeal can resolve an appeal.");
    }

    [Fact]
    public void ABlankReason_IsRefusedInWombatsWords()
    {
        var result = new LodgeAppealCommandValidator().Validate(new LodgeAppealCommand(4, "  ", TraineePrincipal()));

        result.Errors.Select(error => error.ErrorMessage).Should().Equal("Give your reason for appealing.");
    }

    [Fact]
    public void AReasonOverTheLimit_IsRefusedInWombatsWords()
    {
        var result = new LodgeAppealCommandValidator().Validate(
            new LodgeAppealCommand(4, new string('a', LodgeAppealCommand.MaxReasonLength + 1), TraineePrincipal()));

        result.Errors.Select(error => error.ErrorMessage).Should().Equal("Give your reason in 4,000 characters or fewer.");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static Task<CommitteeReviewDetailDto?> ReadMineAsync(ApplicationDbContext db, int reviewId, ClaimsPrincipal principal)
        => new GetMyCommitteeReviewQueryHandler(db, Directory(db)).Handle(
            new GetMyCommitteeReviewQuery(reviewId, principal), CancellationToken.None);

    private static FakeUserDirectory Directory(ApplicationDbContext db)
        => new FakeUserDirectory(People).WithPanelMembersOf(db);

    private static async Task<int> RatifiedReviewAsync(ApplicationDbContext db, DateOnly? scheduledOn = null)
        => await ReviewInStateAsync(db, CommitteeReviewState.Ratified, scheduledOn);

    private static async Task<int> ReviewInStateAsync(ApplicationDbContext db, CommitteeReviewState state, DateOnly? scheduledOn = null)
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            PanelId = 20,
            TraineeUserId = Trainee,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = scheduledOn ?? new DateOnly(2026, 10, 13)
        };
        db.CommitteeReviews.Add(review);
        await db.SaveChangesAsync();

        var chair = Principal("chair-1", WombatRoles.CommitteeMember);
        if (state == CommitteeReviewState.Scheduled)
        {
            return review.Id;
        }

        await new StartCommitteeReviewCommandHandler(db, Directory(db)).Handle(
            new StartCommitteeReviewCommand(review.Id, chair), CancellationToken.None);
        if (state == CommitteeReviewState.InProgress)
        {
            return review.Id;
        }

        await new RecordCommitteeDecisionCommandHandler(db, Directory(db)).Handle(
            new RecordCommitteeDecisionCommand(
                review.Id,
                CommitteeDecisionCategory.InadequateProgressAdditionalTraining,
                "One DOPS in the year and a Mini-CEX that stalled.",
                null,
                "Not enough observed evidence yet this year.",
                ["chair-1", "member-1"],
                chair),
            CancellationToken.None);
        if (state == CommitteeReviewState.Decided)
        {
            return review.Id;
        }

        await new RatifyCommitteeDecisionCommandHandler(db, Directory(db)).Handle(
            new RatifyCommitteeDecisionCommand(review.Id, chair), CancellationToken.None);
        return review.Id;
    }

    private static ApplicationDbContext SeededDb(SaveChangesInterceptor? interceptor = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString());
        if (interceptor is not null)
        {
            options.AddInterceptors(interceptor);
        }

        var db = new ApplicationDbContext(options.Options);
        db.Set<Institution>().Add(new Institution
        {
            Id = Institution, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow
        });
        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = 20,
            Name = "Paed Annual Review Panel",
            Scope = DecisionPanelScope.Speciality,
            InstitutionId = Institution,
            SpecialityId = 5,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member },
                new DecisionPanelMember { UserId = "external-1", Role = DecisionPanelMemberRole.External }
            ]
        });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = Trainee, InstitutionId = Institution, CurriculumId = 1, IsActive = true,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1)
        });
        db.SaveChanges();
        return db;
    }

    private static ClaimsPrincipal TraineePrincipal() => Principal(Trainee, WombatRoles.Trainee);

    private static ClaimsPrincipal Principal(string userId, params string[] roles)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId), new(WombatClaimTypes.SpecialityId, "5") };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    /// <summary>Makes the next SaveChanges throw once it is armed, as a lost race or a dropped connection would.</summary>
    private sealed class FailArmedSave : SaveChangesInterceptor
    {
        private Exception? _armed;

        public void Arm(Exception exception) => _armed = exception;

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (_armed is { } exception)
            {
                _armed = null;
                throw exception;
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
