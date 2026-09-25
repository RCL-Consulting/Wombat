using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The committee reviews page lists each caller the reviews they may open, by the review's own read ladder, and its empty
/// card never tells a scheduler to schedule the first review, since reviews out of their reach may exist. (T218)
/// </summary>
/// <remarks>
/// The real handlers run against an in-memory store, as in <see cref="ReviewsSchedulePanelListTests" />: which reviews a
/// caller may open is the handler's rule (<c>CommitteeDecisionAuthorization.ReadableReviewsAsync</c>), and a stub would
/// prove only that the page renders what it is handed. Until T218 a Coordinator was listed only the panels they sat on,
/// so the page read "No reviews yet – Schedule the first committee review…" beside eight reviews at their institution.
/// </remarks>
public sealed class ReviewsScheduleListScopeTests : TestContext
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int Psychiatry = 3;
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 21;

    private const int PanelA = 10;
    private const int PanelB = 20;

    private const int PaedsReviewAtA = 100;
    private const int SurgeryReviewAtA = 101;
    private const int PaedsReviewAtB = 200;

    private const string EmptyBodyForAScheduler =
        "The reviews you can open appear here once they are scheduled. Use Schedule review to put a trainee before a panel.";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly TestAuthorizationContext _auth;

    public ReviewsScheduleListScopeTests()
    {
        _auth = this.AddTestAuthorization();
        Services.AddSingleton<IScopedSender>(new HandlerSender(CreateDb));
        Seed();
    }

    [Fact]
    public void ACoordinator_IsListedEveryReviewOfTheirInstitutionsPanels_EachOpeningTheReview()
    {
        // The symptom: a Coordinator, who sits on no panel, was listed nothing.
        SignIn(WombatRoles.Coordinator, "coord-a");
        var cut = RenderList();

        Rows(cut).Should().BeEquivalentTo(
        [
            ("Palesa Paeds", $"/committee/reviews/{PaedsReviewAtA}"),
            ("Sipho Surgery", $"/committee/reviews/{SurgeryReviewAtA}")
        ]);
        cut.FindAll(".detail-card--empty").Should().BeEmpty();
    }

    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin, Paediatrics, null, "Palesa Paeds")]
    [InlineData(WombatRoles.SubSpecialityAdmin, null, GeneralPaediatrics, "Palesa Paeds")]
    [InlineData(WombatRoles.SpecialityAdmin, Surgery, null, "Sipho Surgery")]
    public void ASpecialityOrSubSpecialityAdmin_IsListedTheReviewsOfTheirOwnTraineesAtTheirInstitution(
        string role, int? specialityId, int? subSpecialityId, string trainee)
    {
        SignIn(role, "speciality-admin", specialityId, subSpecialityId);
        var cut = RenderList();

        Rows(cut).Select(row => row.Trainee).Should().Equal(trainee);
    }

    [Fact]
    public void AnEmptyList_NeverTellsASchedulerToScheduleTheFirstReview_WhenReviewsExistOutOfTheirReach()
    {
        // Institution A holds two reviews, and neither is a Psychiatry trainee's.
        SignIn(WombatRoles.SpecialityAdmin, "psychiatry-admin", specialityId: Psychiatry);
        var cut = RenderList();

        var empty = cut.Find(".detail-card--empty");
        empty.QuerySelector(".state-panel-title")!.TextContent.Trim().Should().Be("No reviews yet");
        empty.QuerySelector(".state-panel-copy")!.TextContent.Trim().Should().Be(EmptyBodyForAScheduler);
        cut.Markup.Should().NotContain("Schedule the first committee review");
        cut.FindAll("button").Select(button => button.TextContent.Trim()).Should().Contain(
            "Schedule review", "the body's pointer names the header's button, which a scheduler is offered");
    }

    [Fact]
    public void ACommitteeMemberWhoSitsOnNoPanel_IsToldTheirPanelsReviewsAppearHere()
    {
        // The control: a committee member schedules nothing, so the card says nothing about scheduling.
        SignIn(WombatRoles.CommitteeMember, "member-elsewhere");
        var cut = RenderList();

        cut.Find(".detail-card--empty .state-panel-copy").TextContent.Trim().Should().Be(
            "The reviews of the panels you sit on appear here once they are scheduled.");
    }

    private IRenderedComponent<ReviewsSchedule> RenderList()
    {
        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll("table tbody tr").Count > 0 || cut.FindAll(".detail-card--empty").Count > 0);
        cut.FindAll(".alert").Should().BeEmpty();
        return cut;
    }

    private static IReadOnlyList<(string Trainee, string Href)> Rows(IRenderedComponent<ReviewsSchedule> cut)
        => cut.FindAll("table tbody tr")
            .Select(row => (
                row.QuerySelector("td")!.TextContent.Trim(),
                row.QuerySelector("a[href^='/committee/reviews/']")!.GetAttribute("href") ?? string.Empty))
            .ToArray();

    private void SignIn(string role, string userId, int? specialityId = null, int? subSpecialityId = null)
    {
        _auth.SetAuthorized($"{userId}@test");
        _auth.SetRoles(role);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(WombatClaimTypes.InstitutionId, InstitutionA.ToString())
        };
        if (specialityId is int speciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, speciality.ToString()));
        }

        if (subSpecialityId is int subSpeciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, subSpeciality.ToString()));
        }

        _auth.SetClaims(claims.ToArray());
    }

    private void Seed()
    {
        using var db = CreateDb();

        db.Institutions.AddRange(
            new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow },
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery", IsActive = true },
            new Speciality { Id = Psychiatry, CollegeId = 1, Name = "Psychiatry", IsActive = true });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery", IsActive = true });
        db.Curricula.AddRange(
            new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" },
            new Curriculum { Id = 200, SubSpecialityId = GeneralSurgery, Name = "General Surgery", Version = "1" });

        AddProfile(db, 1, "paeds-a", InstitutionA, 100);
        AddProfile(db, 2, "surgery-a", InstitutionA, 200);
        AddProfile(db, 3, "paeds-b", InstitutionB, 100);

        db.DecisionPanels.AddRange(Panel(PanelA, "A's annual review panel", InstitutionA, "chair-a"),
            Panel(PanelB, "B's annual review panel", InstitutionB, "chair-b"));

        db.CommitteeReviews.AddRange(
            Review(PaedsReviewAtA, "paeds-a", PanelA),
            Review(SurgeryReviewAtA, "surgery-a", PanelA),
            Review(PaedsReviewAtB, "paeds-b", PanelB));

        db.SaveChanges();
    }

    private static DecisionPanel Panel(int id, string name, int institutionId, string chair)
        => new()
        {
            Id = id,
            Name = name,
            Scope = DecisionPanelScope.Institution,
            InstitutionId = institutionId,
            CreatedOn = DateTime.UtcNow,
            Members = [new DecisionPanelMember { UserId = chair, Role = DecisionPanelMemberRole.Chair }]
        };

    private static CommitteeReview Review(int id, string trainee, int panelId)
        => new()
        {
            Id = id,
            AcademicYear = 2026,
            Semester = 2,
            TraineeUserId = trainee,
            PanelId = panelId,
            ReviewPeriodFrom = new DateOnly(2026, 1, 1),
            ReviewPeriodTo = new DateOnly(2026, 12, 31),
            ScheduledOn = new DateOnly(2027, 1, 8)
        };

    private static void AddProfile(ApplicationDbContext db, int id, string userId, int institutionId, int curriculumId)
        => db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = id,
            UserId = userId,
            InstitutionId = institutionId,
            CurriculumId = curriculumId,
            ProgrammeStartDate = new DateOnly(2024, 1, 15),
            ExpectedCompletionDate = new DateOnly(2028, 1, 15),
            IsActive = true
        });

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    /// <summary>Sends the page's requests to the real handlers, each on a fresh context over the one store.</summary>
    private sealed class HandlerSender(Func<ApplicationDbContext> createDb) : IScopedSender
    {
        private readonly FakeUserDirectory _users =
            new FakeUserDirectory(("paeds-a", "Palesa Paeds"), ("surgery-a", "Sipho Surgery"), ("paeds-b", "Bongani Paeds"))
                .WithTrainees("paeds-a", "surgery-a", "paeds-b");

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            await using var db = createDb();

            object answer = request switch
            {
                GetCommitteeReviewsAccessQuery query =>
                    await new GetCommitteeReviewsAccessQueryHandler().Handle(query, cancellationToken),
                ListDecisionPanelsQuery query => await new ListDecisionPanelsQueryHandler(db, _users).Handle(query, cancellationToken),
                ListReviewsForPanelQuery query =>
                    await new ListReviewsForPanelQueryHandler(db, _users).Handle(query, cancellationToken),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
