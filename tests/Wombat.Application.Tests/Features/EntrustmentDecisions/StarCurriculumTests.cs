using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

/// <summary>
/// A STAR is staged and issued only on an EPA of the trainee's curriculum, at a rung of that EPA's ladder, and the
/// committee page's picker offers exactly those EPAs (T167).
/// </summary>
/// <remarks>
/// <para>
/// The fixture's curriculum is shared the way a national one is: it holds two core items, a local extra of the
/// trainee's own institution, and a local extra of ANOTHER institution. That last one is the only row for its EPA
/// (<c>CurriculumItems</c> is unique on curriculum and EPA), so a query that forgot the owner predicate would offer it.
/// One more EPA sits on a different curriculum altogether.
/// </para>
/// <para>
/// Every refusal is followed by a save and a cleared tracker before anything is counted. The audit pipeline saves the
/// request's context even when the handler throws, so a check that ran after a mutation would still commit it; a
/// refusal that "writes nothing" has to survive that save.
/// </para>
/// </remarks>
public sealed class StarCurriculumTests
{
    private const int HostInstitution = 1;
    private const int OtherInstitution = 2;
    private const int PinnedLadder = 1;       // the scale PAED-001's item is pinned to
    private const int ProgrammeLadder = 2;    // the sub-speciality's default scale
    private const int CurriculumId = 20;
    private const int OtherCurriculumId = 21;
    private const int PinnedCoreEpa = 30;     // PAED-001: core, pinned to PinnedLadder
    private const int UnpinnedCoreEpa = 31;   // PAED-002: core, unpinned, so on ProgrammeLadder
    private const int OwnLocalEpa = 32;       // PAED-003: the host institution's local extra
    private const int ForeignLocalEpa = 33;   // PAED-004: another institution's local extra
    private const int OffCurriculumEpa = 34;  // PAED-099: on another curriculum only
    private const int ReviewId = 60;
    private const string TraineeUserId = "trainee-1";

    private static int RungOf(int ladder, int order) => (ladder * 10) + order;

    [Fact]
    public async Task ThePicker_ListsExactlyTheTraineesCurriculum_EachWithItsLadder()
    {
        await using var db = await SeededDbAsync();

        var options = await ListAsync(db, Chair());

        options.Select(option => option.Code).Should().Equal("PAED-001", "PAED-002", "PAED-003");
        options.Single(option => option.EpaId == PinnedCoreEpa).ScaleId.Should().Be(PinnedLadder, "the item's pin wins");
        options.Single(option => option.EpaId == UnpinnedCoreEpa).ScaleId.Should().Be(ProgrammeLadder);
        options.Single(option => option.EpaId == OwnLocalEpa).ScaleId.Should().Be(ProgrammeLadder);
        options.Single(option => option.EpaId == PinnedCoreEpa).ScaleName.Should().Be("Pinned ladder");
    }

    /// <summary>Picker = gate: every EPA the picker offers stages at a rung of its offered ladder.</summary>
    [Fact]
    public async Task EveryEpaThePickerOffers_StagesAtARungOfItsLadder()
    {
        await using var db = await SeededDbAsync();

        foreach (var option in await ListAsync(db, Chair()))
        {
            var staged = await StageAsync(db, option.EpaId, RungOf(option.ScaleId!.Value, 3));
            staged.EpaId.Should().Be(option.EpaId);
        }

        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(3);
    }

    [Theory]
    [InlineData(ForeignLocalEpa)]
    [InlineData(OffCurriculumEpa)]
    public async Task Stage_RefusesAnEpaOffTheTraineesCurriculum_AndWritesNothing(int epaId)
    {
        await using var db = await SeededDbAsync();
        (await ListAsync(db, Chair())).Should().NotContain(option => option.EpaId == epaId, "the picker must not offer it");

        var act = () => StageAsync(db, epaId, RungOf(ProgrammeLadder, 3));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not on this trainee's curriculum*");
        await SaveAndClearAsync(db);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// The item's pinned ladder, not the programme's: a rung of the programme's scale is refused on an EPA whose item is
    /// pinned elsewhere, which the T076 check alone would have accepted.
    /// </summary>
    [Fact]
    public async Task Stage_RefusesALevelOffTheItemsPinnedLadder_AndWritesNothing()
    {
        await using var db = await SeededDbAsync();

        var act = () => StageAsync(db, PinnedCoreEpa, RungOf(ProgrammeLadder, 3));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Pinned ladder*PAED-001*");
        await SaveAndClearAsync(db);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Stage_RefusesALevelOffTheProgrammeLadder_ForAnUnpinnedItem()
    {
        await using var db = await SeededDbAsync();

        var act = () => StageAsync(db, UnpinnedCoreEpa, RungOf(PinnedLadder, 3));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Programme ladder*");
    }

    /// <summary>
    /// An edit keeps the EPA it was staged on, and is held to that EPA's ladder. Both refusals leave the staged decision
    /// exactly as it was.
    /// </summary>
    [Fact]
    public async Task Stage_AnEditCannotMoveTheEpaNorLeaveItsLadder_AndChangesNothing()
    {
        await using var db = await SeededDbAsync();
        var staged = await StageAsync(db, PinnedCoreEpa, RungOf(PinnedLadder, 3));

        var moveEpa = () => StageAsync(db, UnpinnedCoreEpa, RungOf(ProgrammeLadder, 3), staged.Id);
        var offLadder = () => StageAsync(db, PinnedCoreEpa, RungOf(ProgrammeLadder, 4), staged.Id);

        await moveEpa.Should().ThrowAsync<InvalidOperationException>().WithMessage("*EPA cannot be changed*");
        await SaveAndClearAsync(db);
        await offLadder.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Pinned ladder*");
        await SaveAndClearAsync(db);

        var stored = await db.PendingEntrustmentDecisions.SingleAsync();
        stored.EpaId.Should().Be(PinnedCoreEpa);
        stored.AuthorisedLevelId.Should().Be(RungOf(PinnedLadder, 3));
    }

    /// <summary>
    /// A panel's chair is refused first by the panel's institution (T182), so the curriculum is judged for an
    /// Administrator, whom that does not bind.
    /// </summary>
    [Fact]
    public async Task ATraineeWithNoProfile_HasNoEpaAStarCanBeStagedOn()
    {
        await using var db = await SeededDbAsync();
        db.TraineeProfiles.RemoveRange(db.TraineeProfiles);
        await db.SaveChangesAsync();

        (await ListAsync(db, Chair())).Should().BeEmpty();

        var act = () => StageAsync(db, PinnedCoreEpa, RungOf(PinnedLadder, 3), principal: TestPrincipals.Administrator());
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not on this trainee's curriculum*");
        await SaveAndClearAsync(db);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// The picker names a trainee's curriculum, so it climbs the review read ladder like the other queries on the page.
    /// </summary>
    [Fact]
    public async Task ThePicker_RefusesSomeoneOffThePanel()
    {
        await using var db = await SeededDbAsync();

        var act = () => ListAsync(db, TestPrincipals.InRole(WombatRoles.CommitteeMember, "stranger", HostInstitution));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Theory]
    [InlineData(ForeignLocalEpa, 3)]
    [InlineData(OffCurriculumEpa, 3)]
    public async Task Issue_RefusesAnEpaOffTheTraineesCurriculum_AndWritesNothing(int epaId, int order)
    {
        await using var db = await SeededDbAsync();
        await RatifyAsync(db);

        var act = () => IssueAsync(db, epaId, RungOf(ProgrammeLadder, order));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not on this trainee's curriculum*");
        await SaveAndClearAsync(db);
        (await db.EntrustmentDecisions.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task Issue_RefusesALevelOffTheItemsLadder_AndSupersedesNothing()
    {
        await using var db = await SeededDbAsync();
        await RatifyAsync(db);
        var prior = await IssueAsync(db, PinnedCoreEpa, RungOf(PinnedLadder, 3));

        var act = () => IssueAsync(db, PinnedCoreEpa, RungOf(ProgrammeLadder, 4));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Pinned ladder*");
        await SaveAndClearAsync(db);
        var decisions = await db.EntrustmentDecisions.ToListAsync();
        decisions.Should().ContainSingle().Which.Id.Should().Be(prior.Id);
        decisions[0].Status.Should().Be(EntrustmentDecisionStatus.Active);
    }

    // ---- A deactivated EPA (T158) ----------------------------------------------------------------------------------

    /// <summary>
    /// A deactivated EPA's items are not in force (<c>CurriculumItemsInForce</c>): no credit lands on them and no picker
    /// offers them, so no STAR is granted on them either.
    /// </summary>
    [Fact]
    public async Task ADeactivatedEpa_IsNotOffered_AndStagingItIsRefused_AndWritesNothing()
    {
        await using var db = await SeededDbAsync();
        (await db.Epas.SingleAsync(epa => epa.Id == UnpinnedCoreEpa)).IsActive = false;
        await SaveAndClearAsync(db);

        (await ListAsync(db, Chair())).Select(option => option.Code).Should().Equal("PAED-001", "PAED-003");

        var act = () => StageAsync(db, UnpinnedCoreEpa, RungOf(ProgrammeLadder, 3));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*PAED-002*deactivated*");
        await SaveAndClearAsync(db);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
    }

    // ---- Ratify: where a staged decision becomes a STAR --------------------------------------------------------------

    public static TheoryData<string> WaysAStagedDecisionStopsFitting => new()
    {
        "the item is removed", "the item is re-pinned", "the EPA is deactivated", "the trainee changes curriculum"
    };

    /// <summary>
    /// A decision staged on PAED-001 at a rung of its pinned ladder, then something changes before the chair ratifies.
    /// Ratify issues every staged decision and supersedes the trainee's active one on the same EPA, so it is held to
    /// the rule staging was; the refusal leaves the review decided and the staged decision staged, for the chair to
    /// remove and re-stage.
    /// </summary>
    [Theory]
    [MemberData(nameof(WaysAStagedDecisionStopsFitting))]
    public async Task Ratify_RefusesAStagedDecisionThatNoLongerFits_AndChangesNothing(string change)
    {
        await using var db = await SeededDbAsync();
        await StageAsync(db, PinnedCoreEpa, RungOf(PinnedLadder, 3));
        await RecordDecisionAsync(db);
        await SaveAndClearAsync(db);

        switch (change)
        {
            case "the item is removed":
                db.CurriculumItems.Remove(await db.CurriculumItems.SingleAsync(item => item.EpaId == PinnedCoreEpa));
                break;
            case "the item is re-pinned":
                (await db.CurriculumItems.SingleAsync(item => item.EpaId == PinnedCoreEpa)).ScaleId = ProgrammeLadder;
                break;
            case "the EPA is deactivated":
                (await db.Epas.SingleAsync(epa => epa.Id == PinnedCoreEpa)).IsActive = false;
                break;
            case "the trainee changes curriculum":
                (await db.TraineeProfiles.SingleAsync()).CurriculumId = OtherCurriculumId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }

        await SaveAndClearAsync(db);

        var act = () => RatifyOnlyAsync(db);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("This review cannot be ratified: 1 staged entrustment decision no longer fits*PAED-001*Remove it*");
        await SaveAndClearAsync(db);
        (await db.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.Decided);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(1);
        (await db.EntrustmentDecisions.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// The refusal names each decision that no longer fits and no other, and issues none of them: ratification is
    /// all-or-nothing.
    /// </summary>
    [Fact]
    public async Task Ratify_NamesOnlyTheDecisionsThatNoLongerFit_AndIssuesNone()
    {
        await using var db = await SeededDbAsync();
        await StageAsync(db, PinnedCoreEpa, RungOf(PinnedLadder, 3));
        await StageAsync(db, UnpinnedCoreEpa, RungOf(ProgrammeLadder, 3));
        await RecordDecisionAsync(db);
        (await db.Epas.SingleAsync(epa => epa.Id == UnpinnedCoreEpa)).IsActive = false;
        await SaveAndClearAsync(db);

        var act = () => RatifyOnlyAsync(db);

        var refusal = (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message;
        refusal.Should().Contain("PAED-002").And.NotContain("PAED-001");
        await SaveAndClearAsync(db);
        (await db.EntrustmentDecisions.CountAsync()).Should().Be(0);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(2);
    }

    /// <summary>The control for the refusals above: the same fixture, nothing changed, ratifies and issues.</summary>
    [Fact]
    public async Task Ratify_IssuesAStagedDecisionThatStillFits()
    {
        await using var db = await SeededDbAsync();
        await StageAsync(db, PinnedCoreEpa, RungOf(PinnedLadder, 3));
        await RecordDecisionAsync(db);
        await SaveAndClearAsync(db);

        await RatifyOnlyAsync(db);

        db.ChangeTracker.Clear();
        (await db.CommitteeReviews.SingleAsync()).State.Should().Be(CommitteeReviewState.Ratified);
        (await db.EntrustmentDecisions.SingleAsync()).EpaId.Should().Be(PinnedCoreEpa);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);
    }

    // ---- Which profile names the curriculum --------------------------------------------------------------------------

    /// <summary>
    /// A trainee who moved holds an inactive profile on another curriculum at another institution. The picker and the
    /// gate follow the active one (<c>TraineeScopeResolver.PreferredProfiles</c>) whichever of the two is older, so
    /// neither "first by id" nor "last by id" passes both rows.
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ThePickerAndTheGate_FollowTheActiveProfile_NotAnInactiveOne(bool theInactiveProfileIsOlder)
    {
        await using var db = await SeededDbAsync();
        await GiveTheTraineeProfilesAsync(
            db,
            new TraineeProfile
            {
                Id = theInactiveProfileIsOlder ? 20 : 10,
                InstitutionId = HostInstitution,
                CurriculumId = CurriculumId,
                IsActive = true
            },
            new TraineeProfile
            {
                Id = theInactiveProfileIsOlder ? 10 : 20,
                InstitutionId = OtherInstitution,
                CurriculumId = OtherCurriculumId,
                IsActive = false
            });

        (await ListAsync(db, Chair())).Select(option => option.Code).Should().Equal("PAED-001", "PAED-002", "PAED-003");

        var offTheActiveCurriculum = () => StageAsync(db, OffCurriculumEpa, RungOf(ProgrammeLadder, 3));
        await offTheActiveCurriculum.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not on this trainee's curriculum*");
        await SaveAndClearAsync(db);

        (await StageAsync(db, OwnLocalEpa, RungOf(ProgrammeLadder, 3))).EpaId.Should().Be(OwnLocalEpa);
    }

    /// <summary>
    /// The mirror of the owner predicate: a trainee at the other institution, on the same shared curriculum, gets that
    /// institution's local extra and not the host's. Staged by an Administrator, whom a panel's institution does not
    /// bind (T182), so the curriculum is the only thing judged.
    /// </summary>
    [Fact]
    public async Task ATraineeAtTheOtherInstitution_GetsItsLocalExtra_AndNotTheHosts()
    {
        await using var db = await SeededDbAsync();
        await GiveTheTraineeProfilesAsync(
            db,
            new TraineeProfile { Id = 10, InstitutionId = OtherInstitution, CurriculumId = CurriculumId, IsActive = true });

        (await ListAsync(db, TestPrincipals.Administrator())).Select(option => option.Code)
            .Should().Equal("PAED-001", "PAED-002", "PAED-004");

        var theHostsExtra = () => StageAsync(db, OwnLocalEpa, RungOf(ProgrammeLadder, 3), principal: TestPrincipals.Administrator());
        await theHostsExtra.Should().ThrowAsync<InvalidOperationException>().WithMessage("*PAED-003*not on this trainee's curriculum*");
        await SaveAndClearAsync(db);
        (await db.PendingEntrustmentDecisions.CountAsync()).Should().Be(0);

        (await StageAsync(db, ForeignLocalEpa, RungOf(ProgrammeLadder, 3), principal: TestPrincipals.Administrator()))
            .EpaId.Should().Be(ForeignLocalEpa);
    }

    // ---- Fixture -------------------------------------------------------------------------------------------------

    private static ClaimsPrincipal Chair()
        => TestPrincipals.InRole(WombatRoles.CommitteeMember, "chair-1", HostInstitution);

    private static Task<IReadOnlyList<StarEpaOptionDto>> ListAsync(ApplicationDbContext db, ClaimsPrincipal principal)
        => new ListStarEpaOptionsForReviewQueryHandler(db).Handle(
            new ListStarEpaOptionsForReviewQuery(ReviewId, principal), CancellationToken.None);

    private static Task<PendingEntrustmentDecisionDto> StageAsync(
        ApplicationDbContext db, int epaId, int levelId, int? pendingId = null, ClaimsPrincipal? principal = null)
        => new StagePendingEntrustmentDecisionCommandHandler(db).Handle(
            new StagePendingEntrustmentDecisionCommand(
                ReviewId,
                pendingId,
                epaId,
                levelId,
                new DateOnly(2027, 1, 8),
                null,
                "Target met.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                principal ?? Chair()),
            CancellationToken.None);

    private static Task RecordDecisionAsync(ApplicationDbContext db)
        => new RecordCommitteeDecisionCommandHandler(db).Handle(
            new RecordCommitteeDecisionCommand(ReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "Satisfactory.", null, Chair()),
            CancellationToken.None);

    private static Task RatifyOnlyAsync(ApplicationDbContext db)
        => new RatifyCommitteeDecisionCommandHandler(db).Handle(
            new RatifyCommitteeDecisionCommand(ReviewId, Chair()),
            CancellationToken.None);

    /// <summary>Replaces the trainee's one profile with these, each with an explicit id so the tie-break is known.</summary>
    private static async Task GiveTheTraineeProfilesAsync(ApplicationDbContext db, params TraineeProfile[] profiles)
    {
        db.TraineeProfiles.RemoveRange(await db.TraineeProfiles.ToListAsync());
        await db.SaveChangesAsync();
        foreach (var profile in profiles)
        {
            profile.UserId = TraineeUserId;
            profile.ProgrammeStartDate = new DateOnly(2025, 1, 15);
            profile.ExpectedCompletionDate = new DateOnly(2029, 12, 31);
        }

        db.TraineeProfiles.AddRange(profiles);
        await SaveAndClearAsync(db);
    }

    private static Task<EntrustmentDecisionDto> IssueAsync(ApplicationDbContext db, int epaId, int levelId)
        => new IssueEntrustmentDecisionCommandHandler(db).Handle(
            new IssueEntrustmentDecisionCommand(
                TraineeUserId,
                epaId,
                levelId,
                new DateOnly(2027, 1, 8),
                null,
                ReviewId,
                "Target met.",
                Array.Empty<EntrustmentEvidenceLinkInput>(),
                Chair()),
            CancellationToken.None);

    private static async Task RatifyAsync(ApplicationDbContext db)
    {
        await new RecordCommitteeDecisionCommandHandler(db).Handle(
            new RecordCommitteeDecisionCommand(ReviewId, CommitteeDecisionCategory.SatisfactoryProgress, "Satisfactory.", null, Chair()),
            CancellationToken.None);
        await new RatifyCommitteeDecisionCommandHandler(db).Handle(
            new RatifyCommitteeDecisionCommand(ReviewId, Chair()),
            CancellationToken.None);
    }

    /// <summary>What the audit pipeline does after a refusal, then a fresh read of what is stored.</summary>
    private static async Task SaveAndClearAsync(ApplicationDbContext db)
    {
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
    }

    private static async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        db.Institutions.AddRange(
            new Institution { Id = HostInstitution, Name = "Host", ShortCode = "HST", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = OtherInstitution, Name = "Other", ShortCode = "OTH", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = 1, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality
        {
            Id = 10,
            SpecialityId = 1,
            Name = "General paediatrics",
            IsActive = true,
            DefaultEntrustmentScaleId = ProgrammeLadder
        });

        db.EntrustmentScales.AddRange(
            new EntrustmentScale { Id = PinnedLadder, Name = "Pinned ladder" },
            new EntrustmentScale { Id = ProgrammeLadder, Name = "Programme ladder" });
        foreach (var ladder in new[] { PinnedLadder, ProgrammeLadder })
        {
            db.EntrustmentLevels.AddRange(Enumerable.Range(1, 5).Select(order => new EntrustmentLevel
            {
                Id = RungOf(ladder, order),
                ScaleId = ladder,
                Order = order,
                Label = $"Rung {order}"
            }));
        }

        db.Epas.AddRange(
            new Epa { Id = PinnedCoreEpa, SubSpecialityId = 10, Code = "PAED-001", Title = "Acute admission", IsActive = true },
            new Epa { Id = UnpinnedCoreEpa, SubSpecialityId = 10, Code = "PAED-002", Title = "Ward round", IsActive = true },
            new Epa { Id = OwnLocalEpa, SubSpecialityId = 10, Code = "PAED-003", Title = "Host local extra", IsActive = true },
            new Epa { Id = ForeignLocalEpa, SubSpecialityId = 10, Code = "PAED-004", Title = "Other local extra", IsActive = true },
            new Epa { Id = OffCurriculumEpa, SubSpecialityId = 10, Code = "PAED-099", Title = "Another curriculum", IsActive = true });

        db.Curricula.AddRange(
            new Curriculum { Id = CurriculumId, SubSpecialityId = 10, Name = "Paediatrics", Version = "v11.1" },
            new Curriculum { Id = OtherCurriculumId, SubSpecialityId = 10, Name = "Paediatrics", Version = "v10" });
        db.CurriculumItems.AddRange(
            new CurriculumItem { Id = 1, CurriculumId = CurriculumId, EpaId = PinnedCoreEpa, RequiredCount = 1, MinimumLevelOrder = 3, ScaleId = PinnedLadder },
            new CurriculumItem { Id = 2, CurriculumId = CurriculumId, EpaId = UnpinnedCoreEpa, RequiredCount = 1, MinimumLevelOrder = 3 },
            new CurriculumItem { Id = 3, CurriculumId = CurriculumId, EpaId = OwnLocalEpa, RequiredCount = 1, MinimumLevelOrder = 3, OwningInstitutionId = HostInstitution },
            new CurriculumItem { Id = 4, CurriculumId = CurriculumId, EpaId = ForeignLocalEpa, RequiredCount = 1, MinimumLevelOrder = 3, OwningInstitutionId = OtherInstitution },
            new CurriculumItem { Id = 5, CurriculumId = OtherCurriculumId, EpaId = OffCurriculumEpa, RequiredCount = 1, MinimumLevelOrder = 3 });

        db.TraineeProfiles.Add(new TraineeProfile
        {
            UserId = TraineeUserId,
            InstitutionId = HostInstitution,
            CurriculumId = CurriculumId,
            ProgrammeStartDate = new DateOnly(2025, 1, 15),
            ExpectedCompletionDate = new DateOnly(2029, 12, 31)
        });

        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = 50,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = HostInstitution,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair }]
        });
        var review = new CommitteeReview
        {
            Id = ReviewId,
            PanelId = 50,
            TraineeUserId = TraineeUserId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8)
        };
        review.Start(Array.Empty<CommitteeEvidence>(), "chair-1", DateTime.UtcNow);
        db.CommitteeReviews.Add(review);

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }
}
