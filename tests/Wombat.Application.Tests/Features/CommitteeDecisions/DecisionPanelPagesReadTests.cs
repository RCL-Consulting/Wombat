using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Tests.TestHelpers;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;

namespace Wombat.Application.Tests.Features.CommitteeDecisions;

/// <summary>
/// What Decision panels and a panel's page read (T364, flow 08: Q5, Q6, Q8, round 3 M6, M8, F2, F5, C13, S4): the list names
/// what each panel covers and who sits on it, an Administrator lists the institution they chose, a panel's page names its
/// institution, speciality, members and open reviews and says first why its committee cannot change, and the refusals are
/// in Wombat's words.
/// </summary>
public sealed class DecisionPanelPagesReadTests
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 4;
    private const int PanelA = 10;
    private const int InstitutionPanelA = 11;
    private const int PanelB = 20;

    private readonly string _databaseName = Guid.NewGuid().ToString();

    private static readonly FakeUserDirectory Names = new FakeUserDirectory(
            ("zulu", "Thandi Zulu"),
            ("naidoo", "David Naidoo"),
            ("botha", "Sarah Botha"),
            ("vanrensburg", "John van Rensburg"),
            ("pseudonym", "Former user 7f3a"))
        .WithCommitteeMembers(InstitutionA, "zulu", "naidoo", "botha", "vanrensburg");

    // ─── Decision panels ─────────────────────────────────────────────────────

    [Fact]
    public async Task TheList_NamesEachPanelsInstitution_Speciality_AndMembers_ChairFirst_ThenMembers_ThenExternals()
    {
        await using var db = await SeededDbAsync();

        var listed = await ListAsync(db, TestPrincipals.InstitutionalAdmin(InstitutionA));

        var panel = listed.Single(row => row.Id == PanelA);
        panel.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        panel.SpecialityName.Should().Be("Paediatrics");
        panel.MemberCount.Should().Be(4);
        panel.MemberNames.Select(member => $"{member.Name}:{member.Role}").Should().Equal(
            "Thandi Zulu:Chair", "David Naidoo:Member", "Sarah Botha:Member", "John van Rensburg:External");

        var institutionWide = listed.Single(row => row.Id == InstitutionPanelA);
        institutionWide.SpecialityName.Should().BeNull("an institution-wide panel covers no one speciality");
        institutionWide.MemberNames.Select(member => member.Name).Should().Equal(
            "Thandi Zulu", "Former user 7f3a", "A member with no account");
    }

    [Fact]
    public async Task AnAdministrator_ListsThePanelsOfTheInstitutionChosen_AndEveryPanelWhenNoneIs()
    {
        await using var db = await SeededDbAsync();

        (await ListAsync(db, TestPrincipals.Administrator(), InstitutionB)).Select(row => row.Id).Should().Equal(PanelB);
        (await ListAsync(db, TestPrincipals.Administrator())).Select(row => row.Id)
            .Should().BeEquivalentTo([PanelA, InstitutionPanelA, PanelB]);
    }

    [Fact]
    public async Task AnInstitutionalAdmin_NamingAnotherInstitution_StillListsOnlyTheirOwn()
    {
        await using var db = await SeededDbAsync();

        (await ListAsync(db, TestPrincipals.InstitutionalAdmin(InstitutionA), InstitutionB)).Select(row => row.Id)
            .Should().BeEquivalentTo([PanelA, InstitutionPanelA]);
    }

    // ─── A panel's page ──────────────────────────────────────────────────────

    [Fact]
    public async Task APanelsPage_NamesItsInstitution_Speciality_AndEveryMember_AsStored()
    {
        await using var db = await SeededDbAsync();

        var panel = await ReadAsync(db, PanelA, TestPrincipals.InstitutionalAdmin(InstitutionA));

        panel!.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        panel.SpecialityName.Should().Be("Paediatrics");
        panel.Members.Select(member => $"{member.UserId}:{member.Name}").Should().BeEquivalentTo(
            ["zulu:Thandi Zulu", "naidoo:David Naidoo", "botha:Sarah Botha", "vanrensburg:John van Rensburg"]);
        panel.OpenReviews.Should().BeEmpty();
        panel.DecisionBodyLock.Should().BeNull("no review holds what the panel sits as");
    }

    [Fact]
    public async Task APanelsPage_ListsItsOpenReviews_OldestFirst_AndSaysFirstWhyItsCommitteeCannotChange()
    {
        await using var db = await SeededDbAsync();
        var first = await SeedReviewAsync(db, PanelA, CommitteeReviewState.Scheduled);
        var second = await SeedReviewAsync(db, PanelA, CommitteeReviewState.InProgress);
        await SeedReviewAsync(db, PanelA, CommitteeReviewState.Ratified);
        await SeedReviewAsync(db, PanelA, CommitteeReviewState.Withdrawn);

        var panel = await ReadAsync(db, PanelA, TestPrincipals.InstitutionalAdmin(InstitutionA));

        panel!.OpenReviews.Select(review => $"{review.Id} {review.Period} {review.StateInSentence}")
            .Should().Equal($"{first} 2026 S2 scheduled", $"{second} 2026 S2 in progress");
        panel.DecisionBodyLock.Should().Be(
            $"Review #{first} (2026 S2, scheduled) still sits before this panel. What a review decides, and its agenda, " +
            "follow the College committee the panel sat as when it was scheduled, so the panel keeps what it sits as until " +
            "its open reviews are ratified, or closed if formative.");
    }

    [Fact]
    public async Task APanelsOpenReview_IsMarkedOpenable_ByTheReviewReadLadder()
    {
        await using var db = await SeededDbAsync();
        await SeedReviewAsync(db, PanelA, CommitteeReviewState.Scheduled);

        (await ReadAsync(db, PanelA, TestPrincipals.InstitutionalAdmin(InstitutionA)))!.OpenReviews.Single().MayOpen
            .Should().BeTrue("an institutional administrator reads every review at the institution");
    }

    [Fact]
    public async Task APanelOutOfReach_ReadsAsNotFound()
    {
        await using var db = await SeededDbAsync();

        (await ReadAsync(db, PanelB, TestPrincipals.InstitutionalAdmin(InstitutionA))).Should().BeNull();
        (await ReadAsync(db, 999, TestPrincipals.InstitutionalAdmin(InstitutionA))).Should().BeNull();
    }

    // ─── Decides for, refused by a race (C13) ────────────────────────────────

    [Fact]
    public async Task TheCommitteeChange_RefusedByAnOpenReview_NamesTheReview_ItsPeriodAndState()
    {
        await using var db = await SeededDbAsync();
        db.DecisionBodies.Add(new DecisionBody { Key = "neonatal", Name = "Neonatal team Clinical Competency Committee" });
        await db.SaveChangesAsync();
        var open = await SeedReviewAsync(db, InstitutionPanelA, CommitteeReviewState.Scheduled);

        var act = () => new SetDecisionPanelBodyCommandHandler(db).Handle(
            new SetDecisionPanelBodyCommand(InstitutionPanelA, "neonatal", TestPrincipals.InstitutionalAdmin(InstitutionA)),
            CancellationToken.None);

        var refusal = (await act.Should().ThrowAsync<DecisionBodyLockedException>()).Which;
        refusal.ReviewId.Should().Be(open);
        refusal.Period.Should().Be("2026 S2");
        refusal.StateInSentence.Should().Be("scheduled");
        refusal.Message.Should().StartWith($"Review #{open} (2026 S2, scheduled) still sits before this panel.");
    }

    // ─── The form's options ──────────────────────────────────────────────────

    [Theory]
    [InlineData(WombatRoles.InstitutionalAdmin, true)]
    [InlineData(WombatRoles.SpecialityAdmin, true)]
    [InlineData(WombatRoles.Coordinator, false)]
    [InlineData(WombatRoles.CommitteeMember, false)]
    public async Task TheFormsOptions_SayWhetherTheCallerManagesPanels_AndNameTheirInstitution(string role, bool manages)
    {
        await using var db = await SeededDbAsync();
        var caller = TestPrincipals.InRole(role, "caller", InstitutionA, specialityId: Paediatrics);

        var options = await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
            new GetDecisionPanelFormOptionsQuery(caller), CancellationToken.None);

        options.MayManagePanels.Should().Be(manages);
        if (manages)
        {
            options.InstitutionName.Should().Be("Kgosi Kgari Teaching Hospital");
        }
    }

    [Fact]
    public async Task AnAdministratorsOptions_NameTheInstitutionChosen_AndNoneBefore()
    {
        await using var db = await SeededDbAsync();

        (await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
                new GetDecisionPanelFormOptionsQuery(TestPrincipals.Administrator()), CancellationToken.None))
            .InstitutionName.Should().BeNull();
        (await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(
                new GetDecisionPanelFormOptionsQuery(TestPrincipals.Administrator(), InstitutionB), CancellationToken.None))
            .InstitutionName.Should().Be("Other Hospital");
    }

    // ─── The refusals' words (S4, T324) ──────────────────────────────────────

    [Fact]
    public void ANewPanel_MissingItsName_Institution_OrSpeciality_IsRefusedOnThatField_InWombatsWords()
    {
        var validator = new CreateDecisionPanelCommandValidator();
        IReadOnlyList<DecisionPanelMemberInput> members =
            [new("zulu", DecisionPanelMemberRole.Chair), new("naidoo", DecisionPanelMemberRole.Member)];
        var administrator = TestPrincipals.Administrator();

        var institutionWide = validator.TestValidate(new CreateDecisionPanelCommand(
            " ", DecisionPanelScope.Institution, null, null, members, administrator));
        institutionWide.ShouldHaveValidationErrorFor(command => command.Name).WithErrorMessage("Give the panel a name.");
        institutionWide.ShouldHaveValidationErrorFor(command => command.InstitutionId).WithErrorMessage("Choose an institution.");

        validator.TestValidate(new CreateDecisionPanelCommand(
                "Paed Annual Review Panel", DecisionPanelScope.Speciality, InstitutionA, null, members, administrator))
            .ShouldHaveValidationErrorFor(command => command.SpecialityId).WithErrorMessage("Choose a speciality.");
    }

    [Fact]
    public void ANewPanelsName_IsMeasuredAsItIsStored_Trimmed()
    {
        // T364, build review R5: the handler stores the name trimmed, so the length is the trimmed one's.
        var validator = new CreateDecisionPanelCommandValidator();
        IReadOnlyList<DecisionPanelMemberInput> members =
            [new("zulu", DecisionPanelMemberRole.Chair), new("naidoo", DecisionPanelMemberRole.Member)];
        var administrator = TestPrincipals.Administrator();

        validator.TestValidate(new CreateDecisionPanelCommand(
                "  " + new string('a', 200) + "  ", DecisionPanelScope.Institution, InstitutionA, null, members, administrator))
            .ShouldNotHaveValidationErrorFor(command => command.Name);
        validator.TestValidate(new CreateDecisionPanelCommand(
                new string('a', 201), DecisionPanelScope.Institution, InstitutionA, null, members, administrator))
            .ShouldHaveValidationErrorFor(command => command.Name).WithErrorMessage(CreateDecisionPanelCommandValidator.NameTooLong);
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static async Task<IReadOnlyList<DecisionPanelSummaryDto>> ListAsync(
        ApplicationDbContext db, System.Security.Claims.ClaimsPrincipal principal, int? institutionId = null)
        => await new ListDecisionPanelsQueryHandler(db, Names).Handle(
            new ListDecisionPanelsQuery(principal, InstitutionId: institutionId), CancellationToken.None);

    private static async Task<DecisionPanelDetailDto?> ReadAsync(
        ApplicationDbContext db, int panelId, System.Security.Claims.ClaimsPrincipal principal)
        => await new GetDecisionPanelByIdQueryHandler(db, Names).Handle(
            new GetDecisionPanelByIdQuery(panelId, principal), CancellationToken.None);

    private static async Task<int> SeedReviewAsync(ApplicationDbContext db, int panelId, CommitteeReviewState state)
    {
        var review = new CommitteeReview
        {
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = "molefe",
            PanelId = panelId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2026, 10, 12)
        };
        db.CommitteeReviews.Add(review);
        db.Entry(review).Property(nameof(CommitteeReview.State)).CurrentValue = state;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return review.Id;
    }

    private async Task<ApplicationDbContext> SeededDbAsync()
    {
        var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "Kgosi Kgari Teaching Hospital", ShortCode = "KGK", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "Other Hospital", ShortCode = "OTH", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.DecisionPanels.AddRange(
            new DecisionPanel
            {
                Id = PanelA,
                Name = "Paed Annual Review Panel",
                Scope = DecisionPanelScope.Speciality,
                InstitutionId = InstitutionA,
                SpecialityId = Paediatrics,
                CreatedOn = DateTime.UtcNow,
                Members =
                [
                    new DecisionPanelMember { Id = 1, UserId = "vanrensburg", Role = DecisionPanelMemberRole.External },
                    new DecisionPanelMember { Id = 2, UserId = "naidoo", Role = DecisionPanelMemberRole.Member },
                    new DecisionPanelMember { Id = 3, UserId = "zulu", Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { Id = 4, UserId = "botha", Role = DecisionPanelMemberRole.Member }
                ]
            },
            new DecisionPanel
            {
                Id = InstitutionPanelA,
                Name = "Hospital Review Panel",
                Scope = DecisionPanelScope.Institution,
                InstitutionId = InstitutionA,
                CreatedOn = DateTime.UtcNow,
                Members =
                [
                    new DecisionPanelMember { Id = 5, UserId = "zulu", Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { Id = 6, UserId = "pseudonym", Role = DecisionPanelMemberRole.Member },
                    new DecisionPanelMember { Id = 7, UserId = "gone", Role = DecisionPanelMemberRole.Member }
                ]
            },
            new DecisionPanel
            {
                Id = PanelB,
                Name = "Other Panel",
                Scope = DecisionPanelScope.Institution,
                InstitutionId = InstitutionB,
                CreatedOn = DateTime.UtcNow,
                Members = [new DecisionPanelMember { Id = 8, UserId = "other-chair", Role = DecisionPanelMemberRole.Chair }]
            });

        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        return db;
    }
}
