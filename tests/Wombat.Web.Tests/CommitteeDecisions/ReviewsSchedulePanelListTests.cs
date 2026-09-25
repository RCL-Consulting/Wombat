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
/// The scheduling form's Panel select offers exactly the panels on which the caller may put someone before the panel: not
/// a panel they sit on at another institution, and not one covering a speciality they do not administer. (T194 item 3)
/// </summary>
/// <remarks>
/// The real handlers run against an in-memory store, as in <see cref="ReviewsScheduleTraineeFirstTests" />: the rule is
/// the handler's (<c>CommitteeTraineeScope.ListSchedulablePanelsAsync</c>, which asks the scheduling predicate), and a stub
/// would prove only that the page renders what it is handed.
/// </remarks>
public sealed class ReviewsSchedulePanelListTests : TestContext
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
    private const int PaediatricsPanelA = 30;

    /// <summary>A Coordinator of A who sits as an External member on B's panel.</summary>
    private const string CoordinatorOfA = "coord-a";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly TestAuthorizationContext _auth;

    public ReviewsSchedulePanelListTests()
    {
        _auth = this.AddTestAuthorization();
        Services.AddSingleton<IScopedSender>(new HandlerSender(CreateDb));
        Seed();
    }

    [Fact]
    public void AnExternalMemberFromAnotherInstitution_IsNotOfferedThatPanel()
    {
        // Before T194 B's panel was offered to them, with an empty trainee list: they sit on it, but a panel reviews its
        // own institution's trainees, and they coordinate at A.
        SignIn(WombatRoles.Coordinator, CoordinatorOfA);
        var cut = OpenTheForm();

        PanelOptions(cut).Should().Equal(string.Empty, PanelA.ToString(), PaediatricsPanelA.ToString());
        cut.FindAll("#review-panel-help").Should().BeEmpty();
    }

    [Fact]
    public void ASurgerySpecialityAdmin_IsNotOfferedThePaediatricsPanel()
    {
        SignIn(WombatRoles.SpecialityAdmin, "surgery-admin", specialityId: Surgery);
        var cut = OpenTheForm();

        PanelOptions(cut).Should().Equal(string.Empty, PanelA.ToString());
    }

    [Fact]
    public void ASchedulerWithNoPanelToScheduleOn_IsToldWhyTheListIsEmpty()
    {
        // A programme with no trainee at A yet: no panel has anyone they could put before it.
        SignIn(WombatRoles.SpecialityAdmin, "psychiatry-admin", specialityId: Psychiatry);
        var cut = OpenTheForm();

        PanelOptions(cut).Should().Equal(string.Empty);
        cut.Find("#review-panel-help").TextContent.Should().Contain("No panel has a trainee you can schedule a review for.");
        cut.Find("#review-panel").GetAttribute("aria-describedby").Should().Contain("review-panel-help");
    }

    private IRenderedComponent<ReviewsSchedule> OpenTheForm()
    {
        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll("button").Any(button => button.TextContent.Trim() == "Schedule review"));
        cut.FindAll("button").Single(button => button.TextContent.Trim() == "Schedule review").Click();
        cut.WaitForState(() => cut.FindAll("#review-panel").Count == 1);
        cut.FindAll(".alert").Should().BeEmpty();
        return cut;
    }

    private static IReadOnlyList<string> PanelOptions(IRenderedComponent<ReviewsSchedule> cut)
        => cut.FindAll("select#review-panel option").Select(option => option.GetAttribute("value") ?? string.Empty).ToArray();

    private void SignIn(string role, string userId, int? specialityId = null)
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

        db.DecisionPanels.AddRange(
            Panel(PanelA, "A's annual review panel", InstitutionA, DecisionPanelScope.Institution, null, "chair-a"),
            Panel(PaediatricsPanelA, "A's paediatrics panel", InstitutionA, DecisionPanelScope.Speciality, Paediatrics, "chair-a"),
            Panel(PanelB, "B's annual review panel", InstitutionB, DecisionPanelScope.Institution, null, "chair-b", CoordinatorOfA));

        db.SaveChanges();
    }

    private static DecisionPanel Panel(
        int id, string name, int institutionId, DecisionPanelScope scope, int? specialityId, string chair, string? external = null)
        => new()
        {
            Id = id,
            Name = name,
            Scope = scope,
            InstitutionId = institutionId,
            SpecialityId = specialityId,
            CreatedOn = DateTime.UtcNow,
            Members = external is null
                ? [new DecisionPanelMember { UserId = chair, Role = DecisionPanelMemberRole.Chair }]
                :
                [
                    new DecisionPanelMember { UserId = chair, Role = DecisionPanelMemberRole.Chair },
                    new DecisionPanelMember { UserId = external, Role = DecisionPanelMemberRole.External }
                ]
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
                ListSchedulableTraineesQuery query =>
                    await new ListSchedulableTraineesQueryHandler(db, _users).Handle(query, cancellationToken),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
