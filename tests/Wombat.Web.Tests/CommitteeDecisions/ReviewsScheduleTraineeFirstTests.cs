using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula.Quota;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// The committee reviews page offers someone who holds Trainee beside another role no scheduling and lists them no
/// review, even when the decisions-due link names a peer, and its empty card says why. (T216)
/// </summary>
/// <remarks>
/// <para>
/// Unlike the page's other tests, these run the real handlers against an in-memory store: the rules under test are the
/// handlers' (<c>CommitteeDecisionAuthorization.MayScheduleReviews</c>, which the page reads through
/// <c>GetCommitteeReviewsAccessQuery</c>, and the list's trainee rung), so a stub that answered "nobody" would prove only
/// that the page renders an empty list. The handler tests hold the rules (<c>CommitteeTraineeScopeTests</c>); these hold
/// that the page, signed in as each role pair, is what the rules say.
/// </para>
/// <para>
/// Each role pair has its control: the same sign-in without the Trainee role is offered the peer, sees the peer's agenda
/// and lists the peer's review, so what is withheld is the rung's doing, not the fixture's.
/// </para>
/// </remarks>
public sealed class ReviewsScheduleTraineeFirstTests : TestContext
{
    private const int InstitutionA = 1;
    private const int Paediatrics = 1;
    private const int GeneralPaediatrics = 11;
    private const int PanelA = 10;
    private const string Peer = "paeds-a";

    /// <summary>The caller: a registrar who sits on panel A, so every role's own reach lists its reviews.</summary>
    private const string Registrar = "registrar-a";

    private const string TraineeWhoSchedulesNote =
        "You hold the Trainee role, so you cannot schedule a committee review or preview its agenda, and this page lists " +
        "no one's reviews. Your own are on My committee reviews once they are ratified.";

    private const string TraineeOnTheCommitteeNote =
        "You hold the Trainee role, so this page lists no one's committee reviews. Your own are on My committee reviews " +
        "once they are ratified.";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly TestAuthorizationContext _auth;
    private readonly HandlerSender _sender;

    public ReviewsScheduleTraineeFirstTests()
    {
        _auth = this.AddTestAuthorization();
        _sender = new HandlerSender(CreateDb);
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton(TimeProvider.System);
        Seed();
    }

    public static TheoryData<string> RolesThatSchedule => new()
    {
        WombatRoles.Coordinator,
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin,
        WombatRoles.Administrator
    };

    [Theory]
    [MemberData(nameof(RolesThatSchedule))]
    public void ATraineeWhoAlsoSchedules_IsOfferedNoScheduling_AndListedNoReview_EvenWhenTheLinkNamesAPeer(string role)
    {
        SignIn(role, alsoTrainee: true);

        var list = RenderComponent<ReviewsSchedule>();
        list.WaitForState(() => list.FindAll(".detail-card--empty").Count == 1);

        EmptyCardSays(list, "No reviews to show", TraineeWhoSchedulesNote);
        ScheduleLinks(list).Should().BeEmpty("the scheduling command refuses this caller whoever they name");
        list.Find(".page-subtitle").TextContent.Should().Be("Open existing committee reviews.");

        // The empty card is an answer, not a failure the page swallowed.
        list.FindAll(".alert").Should().BeEmpty();

        // The schedule page, opened from Decisions due's link naming the peer, offers no form either (T216; T364, Q1).
        NavigateToTheLinkFor(Peer);
        var form = RenderComponent<ScheduleReview>();
        form.WaitForState(() => form.FindAll(".detail-card--empty").Count == 1);

        EmptyCardSays(form, "No review can be scheduled", TraineeWhoSchedulesNote);
        form.FindAll("#review-panel").Should().BeEmpty("the link opens no form");
        form.FindAll(".alert").Should().BeEmpty();

        _sender.Received.OfType<ListDecisionPanelsQuery>().Should().BeEmpty();
        _sender.Received.OfType<ListSchedulableTraineesQuery>().Should().BeEmpty();
        _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Should().BeEmpty();
        _sender.Received.OfType<GetSchedulingSeatQuery>().Should().BeEmpty();
        _sender.Received.OfType<ScheduleCommitteeReviewCommand>().Should().BeEmpty();

        var listed = _sender.Received.OfType<ListReviewsForPanelQuery>().Should().ContainSingle().Subject;
        listed.Principal.IsInRole(WombatRoles.Trainee).Should().BeTrue("the list is asked as the caller, both roles and all");
        listed.Principal.IsInRole(role).Should().BeTrue();
        ReviewCount().Should().Be(1, "the peer's review is there; it is this caller's view of it that is withheld");
    }

    [Theory]
    [MemberData(nameof(RolesThatSchedule))]
    public void TheSameRoleWithoutTrainee_IsOfferedThePeer_SeesTheirAgenda_AndListsTheirReview(string role)
    {
        SignIn(role, alsoTrainee: false);

        var list = RenderComponent<ReviewsSchedule>();
        list.WaitForState(() => list.FindAll("tbody tr").Count == 1);

        ScheduleLinks(list).Should().ContainSingle();
        list.Find(".page-subtitle").TextContent.Should().Be("Schedule progression reviews and open existing ones.");
        list.Find("tbody tr").TextContent.Should().Contain("Palesa Paeds");
        list.FindAll(".alert").Should().BeEmpty();

        NavigateToTheLinkFor(Peer);
        var form = RenderComponent<ScheduleReview>();
        form.WaitForState(() => _sender.Received.OfType<PreviewCommitteeAgendaQuery>().Any());
        form.WaitForState(() => form.Find("#agenda-preview-summary").TextContent.Contains("decides no EPA"));

        OptionValues(form).Should().Equal(string.Empty, Peer);
        form.Find("select#review-trainee option[value='paeds-a']").TextContent.Should().StartWith("Palesa Paeds");
        form.Find("#review-trainee").GetAttribute("value").Should().Be(Peer);
        form.Find("#agenda-preview-summary").TextContent.Should().Contain("No EPA will be on the agenda.");
        form.Markup.Should().NotContain(ScheduleWords.PreviewFailed);
        form.FindAll(".alert").Should().BeEmpty();
    }

    [Fact]
    public void ATraineeOnTheCommittee_IsListedNoReview_AndIsToldWhy()
    {
        SignIn(WombatRoles.CommitteeMember, alsoTrainee: true);

        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll(".detail-card--empty").Count == 1);

        EmptyCardSays(cut, "No reviews to show", TraineeOnTheCommitteeNote);
        ScheduleLinks(cut).Should().BeEmpty();
        cut.FindAll(".alert").Should().BeEmpty();
    }

    [Fact]
    public void ACommitteeMember_ListsTheirPanelsReviews_ButIsOfferedNoScheduling_NotEvenFromALink()
    {
        // Before T216 the page offered every caller "Schedule review", a committee member included, whom the command
        // refuses whoever they name. The schedule page does not admit the role (its [Authorize]); drawn anyway, it offers
        // no form.
        SignIn(WombatRoles.CommitteeMember, alsoTrainee: false);

        var cut = RenderComponent<ReviewsSchedule>();
        cut.WaitForState(() => cut.FindAll("tbody tr").Count == 1);

        cut.Find("tbody tr").TextContent.Should().Contain("Palesa Paeds");
        ScheduleLinks(cut).Should().BeEmpty();
        cut.Find(".page-subtitle").TextContent.Should().Be("Open existing committee reviews.");
        cut.FindAll(".alert").Should().BeEmpty();

        NavigateToTheLinkFor(Peer);
        var form = RenderComponent<ScheduleReview>();
        form.WaitForState(() => form.FindAll(".detail-card--empty").Count == 1);
        form.FindAll("#review-panel").Should().BeEmpty();
        _sender.Received.OfType<ListSchedulableTraineesQuery>().Should().BeEmpty();
    }

    // ─── What the page shows ─────────────────────────────────────────────────

    private static void EmptyCardSays(IRenderedFragment cut, string title, string note)
    {
        var card = cut.Find(".detail-card--empty");
        card.QuerySelector(".state-panel-title")!.TextContent.Should().Be(title);
        card.QuerySelector(".state-panel-copy")!.TextContent.Should().Be(note);

        var link = card.QuerySelector("a")!;
        link.GetAttribute("href").Should().Be("/committee/my-reviews");
        link.TextContent.Trim().Should().Be("Open My committee reviews");
    }

    private static IReadOnlyList<IElement> ScheduleLinks(IRenderedFragment cut)
        => cut.FindAll("a").Where(link => link.TextContent.Trim() == "Schedule a review").ToArray();

    private static IReadOnlyList<string> OptionValues(IRenderedFragment cut)
        => cut.FindAll("select#review-trainee option")
            .Select(option => option.GetAttribute("value") ?? string.Empty)
            .ToArray();

    // ─── The sign-in, the link and the store ─────────────────────────────────

    /// <summary>
    /// <paramref name="role" /> at A, with the scope claims that role's reach reads, and the Trainee role beside it when
    /// <paramref name="alsoTrainee" />.
    /// </summary>
    private void SignIn(string role, bool alsoTrainee)
    {
        _auth.SetAuthorized($"{Registrar}@test");
        _auth.SetRoles(alsoTrainee ? new[] { WombatRoles.Trainee, role } : new[] { role });

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Registrar),
            new(WombatClaimTypes.InstitutionId, InstitutionA.ToString())
        };
        if (role == WombatRoles.SpecialityAdmin)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, Paediatrics.ToString()));
        }
        else if (role == WombatRoles.SubSpecialityAdmin)
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, GeneralPaediatrics.ToString()));
        }

        _auth.SetClaims(claims.ToArray());
    }

    /// <summary>The decisions-due page's Schedule link, naming the peer, panel A and the current period.</summary>
    private void NavigateToTheLinkFor(string traineeUserId)
    {
        var period = CommitteeReviewPeriods.Current(QuotaCalendar.Today());
        Services.GetRequiredService<NavigationManager>().NavigateTo(
            ReviewLinks.Schedule(PanelA, traineeUserId, period.Key, "/committee/decisions-due"));
    }

    private void Seed()
    {
        using var db = CreateDb();

        db.Institutions.Add(new Institution
        {
            Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow
        });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality
        {
            Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true
        });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" });
        db.Set<TraineeProfile>().Add(new TraineeProfile
        {
            Id = 1,
            UserId = Peer,
            InstitutionId = InstitutionA,
            CurriculumId = 100,
            ProgrammeStartDate = new DateOnly(2024, 1, 15),
            ExpectedCompletionDate = new DateOnly(2028, 1, 15),
            IsActive = true
        });
        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = PanelA,
            Name = "A's annual review panel",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = InstitutionA,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-a", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = Registrar, Role = DecisionPanelMemberRole.Member }
            ]
        });

        // The peer's formative review, last year: on the list for anyone the list admits, and no open binding review.
        db.CommitteeReviews.Add(new CommitteeReview
        {
            AcademicYear = 2025,
            Semester = 1,
            TraineeUserId = Peer,
            PanelId = PanelA,
            ReviewPeriodFrom = new DateOnly(2025, 1, 1),
            ReviewPeriodTo = new DateOnly(2025, 6, 30),
            ScheduledOn = new DateOnly(2025, 7, 10),
            IsFormative = true
        });

        db.SaveChanges();
    }

    private int ReviewCount()
    {
        using var db = CreateDb();
        return db.CommitteeReviews.Count();
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    /// <summary>
    /// Sends the page's requests to the real handlers, each on a fresh context over the one store, as a circuit's scoped
    /// sender would; records every request.
    /// </summary>
    private sealed class HandlerSender(Func<ApplicationDbContext> createDb) : IScopedSender
    {
        // The registrar sits on panel A and may sit on it: a seat admits its holder to the panel's reviews only while they
        // may (T279).
        private readonly FakeUserDirectory _users = new FakeUserDirectory((Peer, "Palesa Paeds"))
            .WithTrainees(Peer)
            .WithCommitteeMembers(InstitutionA, "chair-a", Registrar);

        public List<object> Received { get; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            await using var db = createDb();

            object? answer = request switch
            {
                GetCommitteeReviewsAccessQuery query =>
                    await new GetCommitteeReviewsAccessQueryHandler().Handle(query, cancellationToken),
                ListDecisionPanelsQuery query => await new ListDecisionPanelsQueryHandler(db, _users).Handle(query, cancellationToken),
                ListReviewsForPanelQuery query =>
                    await new ListReviewsForPanelQueryHandler(db, _users).Handle(query, cancellationToken),
                ListSchedulableTraineesQuery query =>
                    await new ListSchedulableTraineesQueryHandler(db, _users).Handle(query, cancellationToken),
                PreviewCommitteeAgendaQuery query =>
                    await new PreviewCommitteeAgendaQueryHandler(db, _users).Handle(query, cancellationToken),
                GetSchedulingSeatQuery query =>
                    (object?)await new GetSchedulingSeatQueryHandler(db, _users).Handle(query, cancellationToken),
                ScheduleCommitteeReviewCommand command =>
                    await new ScheduleCommitteeReviewCommandHandler(db, _users).Handle(command, cancellationToken),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
