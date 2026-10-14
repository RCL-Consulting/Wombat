using System.Security.Claims;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.Epas;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.EntrustmentDecisions;

public sealed class EntrustmentDecisionHandlersTests
{
    private const int EvidenceOnEpa7 = 501;
    private const int EvidenceOnEpa8 = 502;

    [Fact]
    public void ARevocationReasonOverTheLimit_IsRefusedInWombatsWords_NeverTheValidatorsDefault()
    {
        // T366, R3: the register says it under the box; the validator says the same, should the page be bypassed.
        var result = new RevokeEntrustmentDecisionCommandValidator().Validate(new RevokeEntrustmentDecisionCommand(
            4, new string('a', RevokeEntrustmentDecisionCommand.MaxReasonLength + 1), CreatePrincipal("admin", [WombatRoles.Administrator])));

        result.Errors.Select(error => error.ErrorMessage).Should().Equal("Give the reason in 1,000 characters or fewer.");
    }

    [Fact]
    public async Task Ratify_OnlyByTheChair_IssuesTheStagedStar_AndSupersedesThePriorActiveOne()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);
        var prior = await SeedStarAsync(dbContext, review.Id, epaId: 7, levelId: 3, new DateOnly(2026, 1, 10), null);

        // Staged while the review is in progress: since T165 the staged STARs are part of the decision recorded next, and
        // are fixed with it.
        await StageAsync(dbContext, review.Id, epaId: 7, levelId: 4, new DateOnly(2026, 4, 1), null, "Level advanced.", EvidenceOnEpa7);
        await RecordDecisionAsync(dbContext, review.Id);

        var ratify = new RatifyCommitteeDecisionCommandHandler(dbContext, Committee);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => ratify.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("member-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None));

        await ratify.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var issued = await dbContext.Set<EntrustmentDecision>().SingleAsync(d => d.Id != prior.Id);
        issued.Status.Should().Be(EntrustmentDecisionStatus.Active);
        issued.AuthorisedLevelId.Should().Be(4);

        var superseded = await dbContext.Set<EntrustmentDecision>().SingleAsync(d => d.Id == prior.Id);
        superseded.Status.Should().Be(EntrustmentDecisionStatus.Superseded);
        superseded.SupersededByDecisionId.Should().Be(issued.Id);
    }

    [Fact]
    public async Task Stage_OnAReviewNotYetStarted_IsRefused_AfterTheChairIsAuthorised()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: false);

        var act = () => StageAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null, "Rationale.", EvidenceOnEpa7);

        // Since T165 a STAR is staged only while the review is in progress (StagedStars).
        (await act.Should().ThrowAsync<InvalidOperationException>()).Which.Message.Should().Be(StagedStars.FixedWhenDecided);
    }

    [Fact]
    public async Task Revoke_AllowsInstitutionalAdminsAndIssuingChair()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        var issued = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1));

        var revokeHandler = new RevokeEntrustmentDecisionCommandHandler(dbContext, FakeUserDirectory.Empty, TestNotices.None(FakeUserDirectory.Empty));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => revokeHandler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Arbitrary revocation.", CreatePrincipal("trainee-1", [WombatRoles.Trainee])),
            CancellationToken.None));

        var revokedByAdmin = await revokeHandler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Scope reduced by programme.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);

        revokedByAdmin.Status.Should().Be(EntrustmentDecisionStatus.Revoked);
        revokedByAdmin.RevocationReason.Should().Be("Scope reduced by programme.");
        revokedByAdmin.RevokedByUserId.Should().Be("admin-1");
    }

    [Fact]
    public async Task StagePendingAndRatify_IssuesAtomically_EachStarWithTheEvidenceItWasStagedOn()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);

        // Staged while the review is in progress: since T165 the staged STARs are part of the decision recorded next, and
        // are fixed with it.
        await StageAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), new DateOnly(2027, 4, 1), "EPA 7 rationale.", EvidenceOnEpa7);
        await StageAsync(dbContext, review.Id, 8, 4, new DateOnly(2026, 4, 1), null, "EPA 8 rationale.", EvidenceOnEpa7, EvidenceOnEpa8);

        (await dbContext.Set<PendingEntrustmentDecision>().CountAsync()).Should().Be(2);

        await RecordDecisionAsync(dbContext, review.Id);

        var ratifyHandler = new RatifyCommitteeDecisionCommandHandler(dbContext, Committee);
        await ratifyHandler.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        (await dbContext.Set<PendingEntrustmentDecision>().CountAsync()).Should().Be(0);

        var decisions = await dbContext.Set<EntrustmentDecision>().Include(d => d.EvidenceLinks).OrderBy(d => d.EpaId).ToListAsync();
        decisions.Should().HaveCount(2);
        decisions.Should().AllSatisfy(d =>
        {
            d.Status.Should().Be(EntrustmentDecisionStatus.Active);
            d.IssuedByCommitteeReviewId.Should().Be(review.Id);
            d.IssuedByChairUserId.Should().Be("chair-1");
        });
        decisions[0].EvidenceLinks.Select(link => link.CommitteeEvidenceId).Should().Equal(EvidenceOnEpa7);
        decisions[1].EvidenceLinks.Select(link => link.CommitteeEvidenceId).Should().BeEquivalentTo([EvidenceOnEpa7, EvidenceOnEpa8]);
    }

    /// <summary>
    /// T367. My authorisations reads the STARs in force and those no longer in force: Active, Revoked (with its reason, its
    /// reviser by name and its date) and Expired. A superseded STAR is left off: another replaced it.
    /// </summary>
    [Fact]
    public async Task GetDecisionsForTrainee_ReadsActiveRevokedAndExpired_NotSuperseded()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);

        var revoked = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        var expired = await SeedStarAsync(dbContext, review.Id, 8, 4, new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 30));
        var replaced = await SeedStarAsync(dbContext, review.Id, 7, 4, new DateOnly(2026, 4, 2), null);
        var current = await SeedStarAsync(dbContext, review.Id, 7, 5, new DateOnly(2026, 4, 3), null);

        expired.MarkExpired(new DateTime(2026, 5, 2, 8, 0, 0, DateTimeKind.Utc));
        replaced.SupersedeBy(current);
        await dbContext.SaveChangesAsync();

        var users = new FakeUserDirectory(("admin-1", "Refilwe Mokoena"));
        await new RevokeEntrustmentDecisionCommandHandler(dbContext, users, TestNotices.None(users)).Handle(
            new RevokeEntrustmentDecisionCommand(revoked.Id, "Staged on the wrong evidence.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var mine = await new GetDecisionsForTraineeQueryHandler(dbContext, users).Handle(
            new GetDecisionsForTraineeQuery("trainee-1", CreatePrincipal("trainee-1", [WombatRoles.Trainee])),
            CancellationToken.None);

        mine.Select(decision => (decision.Id, decision.Status)).Should().BeEquivalentTo(new[]
        {
            (revoked.Id, EntrustmentDecisionStatus.Revoked),
            (expired.Id, EntrustmentDecisionStatus.Expired),
            (current.Id, EntrustmentDecisionStatus.Active),
        });

        var withdrawn = mine.Single(decision => decision.Id == revoked.Id);
        withdrawn.RevocationReason.Should().Be("Staged on the wrong evidence.");
        withdrawn.RevokedByName.Should().Be("Refilwe Mokoena");
        withdrawn.RevokedOn.Should().NotBeNull();
        mine.Where(decision => decision.Id != revoked.Id).Should().AllSatisfy(decision => decision.RevokedByName.Should().BeNull());
        mine.Single(decision => decision.Id == expired.Id).EvidenceLinks.Should().ContainSingle()
            .Which.CommitteeEvidenceId.Should().Be(EvidenceOnEpa8);
    }

    /// <summary>
    /// T367. A revoke mails the trainee once, after the save: the EPA, the level, the day on the South African calendar,
    /// the reviser by name and a link to My authorisations. Never the reason.
    /// </summary>
    [Fact]
    public async Task Revoke_MailsTheTraineeOnce_NamingTheReviser_WithoutTheReason()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        var issued = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        var users = RevocationDirectory();
        var notices = new TestNotices(users);

        var result = await new RevokeEntrustmentDecisionCommandHandler(dbContext, users, notices.Notices).Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "A concern about unsupervised practice.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);

        var mail = notices.Sender.Sent.Should().ContainSingle().Subject;
        mail.To.Should().Be("trainee-1@test");
        mail.Subject.Should().Be("Entrustment decision revoked: EPA-07");
        mail.TextBody.Should().Contain("EPA-07 — Emergency triage at level Level 3")
            .And.Contain($"revoked on {ProgrammeCalendar.DateOf(result.RevokedOn!.Value):yyyy-MM-dd} by Refilwe Mokoena.")
            .And.Contain("https://wombat.test/portfolio/authorisations");
        mail.TextBody.Should().NotContain("unsupervised");
        mail.HtmlBody.Should().NotContain("unsupervised");
    }

    /// <summary>T367. A refused revoke mails nobody: neither one out of the caller's reach nor one already revoked.</summary>
    [Fact]
    public async Task Revoke_Refused_MailsNobody()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        var issued = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        var users = RevocationDirectory();
        var notices = new TestNotices(users);
        var handler = new RevokeEntrustmentDecisionCommandHandler(dbContext, users, notices.Notices);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Not mine to revoke.", CreatePrincipal("trainee-1", [WombatRoles.Trainee])),
            CancellationToken.None));
        notices.Sender.Sent.Should().BeEmpty();

        var admin = CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1);
        await handler.Handle(new RevokeEntrustmentDecisionCommand(issued.Id, "First.", admin), CancellationToken.None);
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Second.", admin), CancellationToken.None));
        notices.Sender.Sent.Should().ContainSingle();
    }

    /// <summary>T367. A deactivated account is not mailed; the revocation stands.</summary>
    [Fact]
    public async Task Revoke_OfADeactivatedTrainee_MailsNobody_AndStands()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        var issued = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        var users = RevocationDirectory(traineeDeactivated: true);
        var notices = new TestNotices(users);

        var result = await new RevokeEntrustmentDecisionCommandHandler(dbContext, users, notices.Notices).Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Revoked.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);

        result.Status.Should().Be(EntrustmentDecisionStatus.Revoked);
        notices.Sender.Sent.Should().BeEmpty();
        notices.Logger.Entries.Should().ContainSingle(entry => entry.Message.Contains("entrustment-revoked"));
    }

    /// <summary>T367. A send that fails is logged and does not fail the command: the revocation has committed.</summary>
    [Fact]
    public async Task Revoke_WhoseMailFails_StillRevokes_AndLogsTheFailure()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        var issued = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        var users = RevocationDirectory();
        var notices = new TestNotices(users, failSendWith: new InvalidOperationException("The mail server is down."));

        var result = await new RevokeEntrustmentDecisionCommandHandler(dbContext, users, notices.Notices).Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Revoked.", CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        result.Status.Should().Be(EntrustmentDecisionStatus.Revoked);
        (await dbContext.Set<EntrustmentDecision>().SingleAsync(d => d.Id == issued.Id)).Status
            .Should().Be(EntrustmentDecisionStatus.Revoked);
        notices.Logger.Entries.Should().ContainSingle(entry =>
            entry.Level == Microsoft.Extensions.Logging.LogLevel.Error && entry.Message.Contains("entrustment-revoked"));
    }

    /// <summary>
    /// T366 (round 2's data rows; S4, C22): the register's row line says who revoked a STAR, by name, as My authorisations
    /// does (T367), and a superseded STAR's line names its successor and the day it was issued, since no supersession date
    /// is stored. One lookup for the trainees and the revisers together.
    /// </summary>
    [Fact]
    public async Task TheRegister_NamesTheReviser_AndASupersededStarsSuccessor()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        var revoked = await SeedStarAsync(dbContext, review.Id, 8, 3, new DateOnly(2026, 4, 1), null);
        var replaced = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        var current = await SeedStarAsync(dbContext, review.Id, 7, 4, new DateOnly(2026, 4, 3), null);
        replaced.SupersedeBy(current);
        await dbContext.SaveChangesAsync();

        var users = new FakeUserDirectory(("admin-1", "Refilwe Mokoena"), ("trainee-1", "Anele Dlamini"));
        var admin = CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1);
        await new RevokeEntrustmentDecisionCommandHandler(dbContext, users, TestNotices.None(users)).Handle(
            new RevokeEntrustmentDecisionCommand(revoked.Id, "Staged on the wrong evidence.", admin), CancellationToken.None);
        dbContext.ChangeTracker.Clear();
        users.Lookups.Clear();

        var rows = await new ListEntrustmentDecisionsForAdminQueryHandler(dbContext, users).Handle(
            new ListEntrustmentDecisionsForAdminQuery(null, null, admin), CancellationToken.None);

        rows.Single(row => row.Id == revoked.Id).RevokedByName.Should().Be("Refilwe Mokoena");
        rows.Where(row => row.Id != revoked.Id).Should().AllSatisfy(row => row.RevokedByName.Should().BeNull());

        var superseded = rows.Single(row => row.Id == replaced.Id);
        superseded.SupersededByDecisionId.Should().Be(current.Id);
        superseded.SupersededByIssuedOn.Should().Be(new DateOnly(2026, 4, 3));
        rows.Where(row => row.Id != replaced.Id).Should().AllSatisfy(row => row.SupersededByIssuedOn.Should().BeNull());

        users.Lookups.Should().ContainSingle().Which.Should().BeEquivalentTo(["trainee-1", "admin-1"]);
    }

    /// <summary>
    /// T366 (S7): Issued links its review only where the viewer may open that review, by the review read ladder
    /// (<see cref="CommitteeReviewReadAccess" />). An InstitutionalAdmin administers a trainee who trains at their
    /// institution, so the register lists the STAR, but a review held by another institution's panel is not theirs to
    /// read; an Administrator reads every review.
    /// </summary>
    [Fact]
    public async Task TheRegister_SaysWhetherTheViewerMayOpenEachStarsIssuingReview()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        var handler = new ListEntrustmentDecisionsForAdminQueryHandler(dbContext, FakeUserDirectory.Empty);
        var institutionalAdmin = CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1);

        (await handler.Handle(new ListEntrustmentDecisionsForAdminQuery(null, null, institutionalAdmin), CancellationToken.None))
            .Should().ContainSingle().Which.IssuingReviewReadable.Should().BeTrue("the panel is at the admin's institution");

        // The panel is another institution's now: the trainee is still the admin's, the review no longer is.
        dbContext.Institutions.Add(new Institution { Id = 2, Name = "Other Hospital", IsActive = true, CreatedOn = DateTime.UtcNow });
        (await dbContext.DecisionPanels.SingleAsync()).InstitutionId = 2;
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        (await handler.Handle(new ListEntrustmentDecisionsForAdminQuery(null, null, institutionalAdmin), CancellationToken.None))
            .Should().ContainSingle().Which.IssuingReviewReadable.Should().BeFalse();
        (await handler.Handle(new ListEntrustmentDecisionsForAdminQuery(null, null, CreatePrincipal("root", [WombatRoles.Administrator])), CancellationToken.None))
            .Should().ContainSingle().Which.IssuingReviewReadable.Should().BeTrue("an Administrator reads every review");
    }

    /// <summary>
    /// T366 (M4): which of a revoke's failures are its own refusals, shown in their words in the dialog, and which are
    /// unexpected (a fixed sentence, logged). A second tab's revoke is refused by the decision itself.
    /// </summary>
    [Fact]
    public async Task ARevokesOwnRefusals_AreKnown_AndNothingElseIs()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        var issued = await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        var users = RevocationDirectory();
        var handler = new RevokeEntrustmentDecisionCommandHandler(dbContext, users, TestNotices.None(users));
        var admin = CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1);
        await handler.Handle(new RevokeEntrustmentDecisionCommand(issued.Id, "First.", admin), CancellationToken.None);

        var second = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new RevokeEntrustmentDecisionCommand(issued.Id, "Second.", admin), CancellationToken.None));
        var unknown = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(
            new RevokeEntrustmentDecisionCommand(9999, "Nothing.", CreatePrincipal("root", [WombatRoles.Administrator])), CancellationToken.None));
        var outOfReach = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(
            new RevokeEntrustmentDecisionCommand(9999, "Nothing.", admin), CancellationToken.None));

        second.Message.Should().Be(EntrustmentRevocationRefusals.NotActive);
        unknown.Message.Should().Be(EntrustmentRevocationRefusals.NotFound);
        EntrustmentRevocationRefusals.IsRefusal(second).Should().BeTrue();
        EntrustmentRevocationRefusals.IsRefusal(unknown).Should().BeTrue();
        EntrustmentRevocationRefusals.IsRefusal(outOfReach).Should().BeTrue();
        EntrustmentRevocationRefusals.IsRefusal(new FluentValidation.ValidationException("Reason is required.")).Should().BeTrue();
        EntrustmentRevocationRefusals.IsRefusal(new InvalidOperationException("A second operation was started on this context instance."))
            .Should().BeFalse("the framework's own failure is never shown on the page (T338)");
        EntrustmentRevocationRefusals.IsRefusal(new TimeoutException()).Should().BeFalse();
    }

    private static FakeUserDirectory RevocationDirectory(bool traineeDeactivated = false)
        => new FakeUserDirectory(("admin-1", "Refilwe Mokoena"))
            .With(new UserIdentityDetails(
                "trainee-1", "trainee-1@test", "Anele", "Dlamini", 1, [], [], [WombatRoles.Trainee],
                IsDeactivated: traineeDeactivated));

    /// <summary>
    /// T255, D48. Both entrustment decision lists say whether each STAR's EPA is in force now, by the flag the EPA picker
    /// labels by (<c>EpaOptionLabel</c>): My authorisations (<see cref="GetDecisionsForTraineeQuery" />) and the
    /// admin's list (<see cref="ListEntrustmentDecisionsForAdminQuery" />). Deactivating an EPA leaves its STAR standing.
    /// </summary>
    [Fact]
    public async Task BothDecisionLists_SayWhetherEachStarsEpaIsInForceNow()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedRatifiedReviewAsync(dbContext);
        await SeedStarAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null);
        await SeedStarAsync(dbContext, review.Id, 8, 4, new DateOnly(2026, 4, 1), null);

        (await dbContext.Epas.SingleAsync(epa => epa.Id == 8))
            .Deactivate(new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var mine = await new GetDecisionsForTraineeQueryHandler(dbContext, FakeUserDirectory.Empty).Handle(
            new GetDecisionsForTraineeQuery("trainee-1", CreatePrincipal("trainee-1", [WombatRoles.Trainee])),
            CancellationToken.None);
        var administered = await new ListEntrustmentDecisionsForAdminQueryHandler(dbContext, FakeUserDirectory.Empty).Handle(
            new ListEntrustmentDecisionsForAdminQuery(null, null, CreatePrincipal("admin-1", [WombatRoles.InstitutionalAdmin], institutionId: 1)),
            CancellationToken.None);

        mine.Select(decision => (decision.EpaCode, decision.EpaInForce)).Should().Equal(("EPA-07", true), ("EPA-08", false));
        administered.Select(decision => (decision.EpaCode, decision.EpaInForce))
            .Should().BeEquivalentTo([("EPA-07", true), ("EPA-08", false)]);
    }

    /// <summary>
    /// T273 (T361, round 3 item 13): a staged STAR says whether its EPA is in force now, from the EPA row the list already
    /// loads, so the review page marks one no longer in use as the sampling card does.
    /// </summary>
    [Fact]
    public async Task TheStagedList_SaysWhetherEachEpaIsInForceNow()
    {
        await using var dbContext = CreateDbContext();
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);
        await StageAsync(dbContext, review.Id, 7, 3, new DateOnly(2026, 4, 1), null, "Level 3.", EvidenceOnEpa7);
        await StageAsync(dbContext, review.Id, 8, 3, new DateOnly(2026, 4, 1), null, "Level 3.", EvidenceOnEpa8);

        (await dbContext.Epas.SingleAsync(epa => epa.Id == 8))
            .Deactivate(new DateTime(2026, 5, 1, 8, 0, 0, DateTimeKind.Utc)).Should().BeTrue();
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var staged = await new ListPendingEntrustmentDecisionsForReviewQueryHandler(dbContext, Committee).Handle(
            new ListPendingEntrustmentDecisionsForReviewQuery(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

        staged.Select(decision => (decision.EpaCode, decision.EpaInForce)).Should().Equal(("EPA-07", true), ("EPA-08", false));
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static Task<PendingEntrustmentDecisionDto> StageAsync(
        ApplicationDbContext dbContext, int reviewId, int epaId, int levelId, DateOnly issuedOn, DateOnly? expiresOn,
        string rationale, params int[] evidenceItemIds)
        => new StagePendingEntrustmentDecisionCommandHandler(dbContext, Committee).Handle(
            new StagePendingEntrustmentDecisionCommand(
                reviewId, null, epaId, levelId, issuedOn, expiresOn, rationale, evidenceItemIds,
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

    /// <summary>The panel's two members, each an active committee member at its institution (T165, PanelSeat).</summary>
    private static FakeUserDirectory Committee => FakeUserDirectory.CommitteeMembersAt(1, "chair-1", "member-1");

    /// <summary>Records the decision with a quorate sitting, the chair and the other member present (T165).</summary>
    private static Task RecordDecisionAsync(ApplicationDbContext dbContext, int reviewId)
        => new RecordCommitteeDecisionCommandHandler(dbContext, Committee).Handle(
            new RecordCommitteeDecisionCommand(reviewId, CommitteeDecisionCategory.SatisfactoryProgress, "Satisfactory.", null, null, ["chair-1", "member-1"],
                CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);

    /// <summary>
    /// A STAR issued by the review, resting on its snapshot line about the same EPA: what a ratification writes, stored
    /// directly for the tests that only read or revoke one.
    /// </summary>
    private static async Task<EntrustmentDecision> SeedStarAsync(
        ApplicationDbContext dbContext, int reviewId, int epaId, int levelId, DateOnly issuedOn, DateOnly? expiresOn)
    {
        var line = await dbContext.Set<CommitteeEvidence>()
            .SingleAsync(item => item.Id == (epaId == 7 ? EvidenceOnEpa7 : EvidenceOnEpa8));
        var decision = EntrustmentDecision.Issue(
            "trainee-1", epaId, levelId, issuedOn, expiresOn, reviewId, "chair-1", $"EPA {epaId}.",
            [EntrustmentEvidenceLink.FromSnapshot(line)]);
        dbContext.Set<EntrustmentDecision>().Add(decision);
        await dbContext.SaveChangesAsync();
        return decision;
    }

    private static async Task<CommitteeReview> SeedRatifiedReviewAsync(ApplicationDbContext dbContext)
    {
        var review = await SeedReviewInStateAsync(dbContext, startReview: true);
        await RecordDecisionAsync(dbContext, review.Id);
        var ratifyHandler = new RatifyCommitteeDecisionCommandHandler(dbContext, Committee);
        await ratifyHandler.Handle(
            new RatifyCommitteeDecisionCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
            CancellationToken.None);
        return await dbContext.Set<CommitteeReview>().SingleAsync(r => r.Id == review.Id);
    }

    private static async Task<CommitteeReview> SeedReviewInStateAsync(ApplicationDbContext dbContext, bool startReview)
    {
        var institution = new Institution { Id = 1, Name = "Test Hospital", IsActive = true, CreatedOn = DateTime.UtcNow };
        var speciality = new Speciality { Id = 5, CollegeId = 1, Name = "General Medicine", IsActive = true };
        var subSpec = new SubSpeciality { Id = 9, SpecialityId = 5, Name = "Acute Care", IsActive = true };

        var scale = new EntrustmentScale { Id = 1, Name = "Standard 5-point" };
        var levels = Enumerable.Range(1, 5).Select(order => new EntrustmentLevel
        {
            Id = order,
            ScaleId = 1,
            Order = order,
            Label = $"Level {order}"
        }).ToList();

        var epa7 = new Epa { Id = 7, SubSpecialityId = 9, Code = "EPA-07", Title = "Emergency triage", IsActive = true };
        var epa8 = new Epa { Id = 8, SubSpecialityId = 9, Code = "EPA-08", Title = "Admission", IsActive = true };

        var panel = new DecisionPanel
        {
            Id = 20,
            Name = "ARCP panel",
            Scope = DecisionPanelScope.Speciality,
            InstitutionId = 1,
            SpecialityId = 5,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-1", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-1", Role = DecisionPanelMemberRole.Member }
            ]
        };

        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 1,
            Id = 30,
            Panel = panel,
            TraineeUserId = "trainee-1",
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 3, 31),
            ScheduledOn = new DateOnly(2026, 4, 1)
        };

        dbContext.Institutions.Add(institution);
        dbContext.Specialities.Add(speciality);
        dbContext.SubSpecialities.Add(subSpec);

        // The trainee trains at the test hospital, so its InstitutionalAdmin oversees them (T183), and it is the panel's
        // institution: a panel acts only on its own institution's trainees (T182). A STAR is granted only on an EPA of
        // the trainee's curriculum (T167), so the curriculum holds both EPAs.
        dbContext.Curricula.Add(new Curriculum { Id = 40, SubSpecialityId = 9, Name = "Acute Care", Version = "1" });
        dbContext.CurriculumItems.AddRange(
            new CurriculumItem { Id = 41, CurriculumId = 40, EpaId = 7, RequiredCount = 1, MinimumLevelOrder = 3 },
            new CurriculumItem { Id = 42, CurriculumId = 40, EpaId = 8, RequiredCount = 1, MinimumLevelOrder = 3 });
        dbContext.Set<TraineeProfile>().Add(new TraineeProfile
        {
            UserId = "trainee-1", InstitutionId = 1, CurriculumId = 40,
            ProgrammeStartDate = new DateOnly(2025, 1, 1), ExpectedCompletionDate = new DateOnly(2029, 1, 1), IsActive = true
        });

        dbContext.EntrustmentScales.Add(scale);
        dbContext.EntrustmentLevels.AddRange(levels);
        dbContext.Epas.AddRange(epa7, epa8);
        dbContext.DecisionPanels.Add(panel);
        dbContext.CommitteeReviews.Add(review);
        await dbContext.SaveChangesAsync();

        if (startReview)
        {
            var startHandler = new StartCommitteeReviewCommandHandler(dbContext, FakeUserDirectory.PanelMembersOf(dbContext));
            await startHandler.Handle(
                new StartCommitteeReviewCommand(review.Id, CreatePrincipal("chair-1", [WombatRoles.CommitteeMember])),
                CancellationToken.None);

            // The window held no activity, so the snapshot is written here: one line about each EPA, which a staged
            // decision names as the evidence it rests on (D38, T131).
            dbContext.Set<CommitteeEvidence>().AddRange(
                new CommitteeEvidence
                {
                    Id = EvidenceOnEpa7, ReviewId = review.Id, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 101,
                    EpaId = 7, EpaCode = "EPA-07", SourceLabel = "Mini-CEX #101", Summary = "State: completed.",
                    ObservedOn = new DateOnly(2026, 2, 1)
                },
                new CommitteeEvidence
                {
                    Id = EvidenceOnEpa8, ReviewId = review.Id, SourceType = CommitteeEvidenceSourceType.Activity, ActivityId = 102,
                    EpaId = 8, EpaCode = "EPA-08", SourceLabel = "CbD #102", Summary = "State: completed.",
                    ObservedOn = new DateOnly(2026, 2, 2)
                });
            await dbContext.SaveChangesAsync();
        }

        return await dbContext.Set<CommitteeReview>().SingleAsync(r => r.Id == review.Id);
    }

    private static ClaimsPrincipal CreatePrincipal(string userId, IReadOnlyCollection<string> roles, int? institutionId = null)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        claims.Add(new Claim(WombatClaimTypes.SpecialityId, "5"));
        if (institutionId.HasValue)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institutionId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }
}
