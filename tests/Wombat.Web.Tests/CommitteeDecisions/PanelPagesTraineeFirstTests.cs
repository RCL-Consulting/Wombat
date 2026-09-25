using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
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
/// The panel pages offer someone who holds Trainee nothing to administer, whatever role admits them to the pages, and say
/// why: the panel list offers no New panel and no Edit, and the panel form, for a new panel or an existing one, offers no
/// form and reads nothing about the panel. (T256)
/// </summary>
/// <remarks>
/// These run the real handlers against an in-memory store, as <see cref="PanelEditFormOptionsTests" /> does: the rule
/// under test is the handlers' (<c>CommitteeDecisionAuthorization.MayAdministerPanels</c> and <c>PanelReachAsync</c>),
/// read through <c>GetDecisionPanelFormOptionsQuery</c> and <c>ListDecisionPanelsQuery</c>. Each role pair has a control:
/// the same caller without Trainee is offered the panel, so what the trainee is not offered is the rung's doing.
/// </remarks>
public sealed class PanelPagesTraineeFirstTests : TestContext
{
    private const int InstitutionA = 1;
    private const int Paediatrics = 1;
    private const int GeneralPaediatrics = 11;
    private const int PanelId = 10;

    private const string TraineeManagesNoPanel =
        "You hold the Trainee role, so you cannot create or change a decision panel, including one that reviews you.";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly TestAuthorizationContext _auth;
    private readonly HandlerSender _sender;

    public PanelPagesTraineeFirstTests()
    {
        _auth = this.AddTestAuthorization();
        _sender = new HandlerSender(CreateDb);
        Services.AddSingleton<IScopedSender>(_sender);
        Seed();
    }

    public static TheoryData<string> PanelAdministrationRoles => new()
    {
        WombatRoles.Administrator,
        WombatRoles.InstitutionalAdmin,
        WombatRoles.SpecialityAdmin,
        WombatRoles.SubSpecialityAdmin
    };

    // ─── The panel form ──────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public void TheNewPanelForm_OffersATraineeWhoManagesPanelsNoForm_AndSaysWhy(string role)
    {
        SignInAs(role, holdsTrainee: true);

        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-trainee-note").Count == 1);

        cut.Find("#panel-trainee-note h3").TextContent.Should().Be("Create panel");
        cut.Find("#panel-trainee-note p").TextContent.Should().Be(TraineeManagesNoPanel);
        cut.FindAll("form").Should().BeEmpty(role);
        cut.FindAll("#panel-scope").Should().BeEmpty(role);
        cut.FindAll("#panel-none-creatable").Should().BeEmpty("the reason is the Trainee role, and the card says so");
        _sender.Received.Should().NotContain(request => request is ListPanelMemberCandidatesQuery, "nothing else is read");
    }

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public void AnExistingPanel_OffersATraineeWhoManagesPanelsNoForm_ReadsNothingAboutIt_AndSaysWhy(string role)
    {
        // Before T256 the form opened on the panel that reviews them, with every seat's picker.
        SignInAs(role, holdsTrainee: true);

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-trainee-note").Count == 1);

        cut.Find("#panel-trainee-note h3").TextContent.Should().Be("Update members");
        cut.Find("#panel-trainee-note p").TextContent.Should().Be(TraineeManagesNoPanel);
        cut.FindAll("form").Should().BeEmpty(role);
        cut.FindAll("#panel-chair").Should().BeEmpty(role);
        cut.FindAll("#panel-body-title").Should().BeEmpty("which College committee it sits as is not theirs to say either");
        _sender.Received.Should().NotContain(
            request => request is GetDecisionPanelByIdQuery || request is ListPanelMemberCandidatesQuery,
            "the panel and its candidates are not read for them");
    }

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public void TheControl_TheSameCallerWithoutTrainee_IsOfferedThePanelsForm(string role)
    {
        SignInAs(role, holdsTrainee: false);

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair").Count == 1);

        cut.FindAll("#panel-trainee-note").Should().BeEmpty(role);
        cut.FindAll("#panel-chair option").Select(option => option.GetAttribute("value"))
            .Should().Contain(["chair-a", "member-a"], $"{role}: every seat's picker offers the panel's committee");
    }

    // ─── The panel list ──────────────────────────────────────────────────────

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public void ThePanelList_OffersATraineeWhoManagesPanelsNoNewPanel_NorEdit_AndSaysWhy(string role)
    {
        SignInAs(role, holdsTrainee: true);

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll("#panels-trainee-note").Count == 1);
        cut.WaitForState(() => cut.Markup.Contains("Paediatrics CCC"));

        cut.Find("#panels-trainee-note").TextContent.Trim().Should().Be(TraineeManagesNoPanel);
        cut.Find("#panels-trainee-note").GetAttribute("role").Should().BeNull("standing page content is not announced on every load");
        Links(cut).Should().NotContain(["New panel", "Edit"], role);
    }

    [Theory]
    [MemberData(nameof(PanelAdministrationRoles))]
    public void TheControl_ThePanelList_OffersTheSameCallerWithoutTrainee_NewPanel_AndEdit(string role)
    {
        SignInAs(role, holdsTrainee: false);

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.Markup.Contains("Paediatrics CCC") && Links(cut).Contains("Edit"));

        Links(cut).Should().Contain(["New panel", "Edit"], role);
        cut.FindAll("#panels-trainee-note").Should().BeEmpty(role);
    }

    [Fact]
    public void ThePanelList_OffersACommitteeMember_NoNewPanel_NorEdit_AndNoTraineeNote()
    {
        // Picker = gate: a committee member manages no panel, so the list offers neither; and it is not their Trainee role
        // that stands in the way, so the note is not theirs.
        SignInAs(WombatRoles.CommitteeMember, holdsTrainee: true);

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.Markup.Contains("Paediatrics CCC"));

        Links(cut).Should().NotContain(["New panel", "Edit"]);
        cut.FindAll("#panels-trainee-note").Should().BeEmpty();
    }

    [Fact]
    public void ThePanelList_ShowsACallerWhoMayManageNoPanel_NoActionsColumn()
    {
        // T239 review: a committee member was shown an "Actions" column of blank cells, since no row offered them Edit.
        SignInAs(WombatRoles.CommitteeMember, holdsTrainee: false);

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.Markup.Contains("Paediatrics CCC"));

        // The page's first table is the panel list; the routing section's follow it.
        var list = cut.Find("table");
        list.QuerySelectorAll("thead th").Select(header => header.TextContent.Trim())
            .Should().Equal("Name", "Scope", "Decides for", "Members");
        list.QuerySelectorAll("tbody tr").Should().OnlyContain(row => row.QuerySelectorAll("td").Length == 4);
    }

    [Fact]
    public void ThePanelList_NamesEachEdit_ByItsPanel_UnderAnActionsHeader()
    {
        // The control: an institutional admin manages the panel, so the column is there, headed, and its Edit named.
        SignInAs(WombatRoles.InstitutionalAdmin, holdsTrainee: false);

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => Links(cut).Contains("Edit"));

        var list = cut.Find("table");
        list.QuerySelectorAll("thead th").Last().QuerySelector("span.visually-hidden")!.TextContent.Should().Be("Actions");
        var edit = list.QuerySelectorAll("tbody a.btn").Single(link => link.TextContent.Trim() == "Edit");
        Accessibility.AccessibleNames.NameOf(cut, edit).Should().Be("Edit Paediatrics CCC");
    }

    public static TheoryData<string, bool, string> EmptyListCopy => new()
    {
        // Offered New panel: told to create one.
        { WombatRoles.Administrator, false, "Create a panel before scheduling reviews." },
        { WombatRoles.InstitutionalAdmin, false, "Create a panel before scheduling reviews." },
        // Not offered it, so not told to. An Administrator lists every institution's panels and belongs to none.
        { WombatRoles.Administrator, true, "No decision panel has been created yet." },
        { WombatRoles.InstitutionalAdmin, true, "No decision panel runs at your institution yet." },
        { WombatRoles.CommitteeMember, false, "No decision panel runs at your institution yet." }
    };

    [Theory]
    [MemberData(nameof(EmptyListCopy))]
    public void AnEmptyPanelList_TellsOnlyWhoMayCreateAPanel_ToCreateOne_AndNeverTellsAnAdministratorAboutAnInstitution(
        string role, bool holdsTrainee, string expected)
    {
        using (var db = CreateDb())
        {
            db.DecisionPanels.RemoveRange(db.DecisionPanels);
            db.SaveChanges();
        }

        SignInAs(role, holdsTrainee);

        var cut = RenderComponent<PanelsList>();
        cut.WaitForState(() => cut.FindAll(".state-panel-copy").Count == 1);

        cut.Find(".state-panel-copy").TextContent.Should().Be(expected, $"{role}, holding Trainee: {holdsTrainee}");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private static IReadOnlyList<string> Links(IRenderedComponent<PanelsList> cut)
        => cut.FindAll("a.btn").Select(link => link.TextContent.Trim()).ToArray();

    /// <summary>
    /// The registrar at A in <paramref name="role" />, with its scope claims, and Trainee beside it when
    /// <paramref name="holdsTrainee" />: a SpecialityAdmin of Paediatrics, a SubSpecialityAdmin of General Paediatrics, so
    /// each reaches the Paediatrics panel without Trainee.
    /// </summary>
    private void SignInAs(string role, bool holdsTrainee)
    {
        _auth.SetAuthorized("registrar@test");
        _auth.SetRoles(holdsTrainee ? [WombatRoles.Trainee, role] : [role]);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "registrar-a") };
        if (role != WombatRoles.Administrator)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, InstitutionA.ToString()));
        }

        if (role == WombatRoles.SpecialityAdmin)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, Paediatrics.ToString()));
        }

        if (role == WombatRoles.SubSpecialityAdmin)
        {
            claims.Add(new Claim(WombatClaimTypes.SubSpecialityId, GeneralPaediatrics.ToString()));
        }

        _auth.SetClaims(claims.ToArray());
    }

    private void Seed()
    {
        using var db = CreateDb();
        db.Institutions.Add(new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Colleges.Add(new College { Id = 1, Name = "CMSA", ShortCode = "CMSA", IsActive = true });
        db.Specialities.Add(new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true });
        db.SubSpecialities.Add(new SubSpeciality
        {
            Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true
        });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" });
        db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
        {
            Id = 1, InstitutionId = InstitutionA, CurriculumId = 100, SubSpecialityId = GeneralPaediatrics,
            AdoptedOn = new DateOnly(2026, 1, 1), IsActive = true
        });
        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = PanelId,
            Name = "Paediatrics CCC",
            Scope = DecisionPanelScope.Speciality,
            InstitutionId = InstitutionA,
            SpecialityId = Paediatrics,
            CreatedOn = DateTime.UtcNow,
            Members =
            [
                new DecisionPanelMember { UserId = "chair-a", Role = DecisionPanelMemberRole.Chair },
                new DecisionPanelMember { UserId = "member-a", Role = DecisionPanelMemberRole.Member }
            ]
        });
        db.SaveChanges();
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    /// <summary>
    /// Sends the pages' requests to the real handlers, each on a fresh context over the one store, as a circuit's scoped
    /// sender would, and records each request. The panel's members may sit on it.
    /// </summary>
    private sealed class HandlerSender(Func<ApplicationDbContext> createDb) : IScopedSender
    {
        private readonly FakeUserDirectory _users = FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a");

        public List<object> Received { get; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            await using var db = createDb();

            object? answer = request switch
            {
                GetDecisionPanelFormOptionsQuery query =>
                    await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(query, cancellationToken),
                GetInstitutionsListQuery query => await new GetInstitutionsListQueryHandler(db).Handle(query, cancellationToken),
                GetSpecialitiesListQuery query => await new GetSpecialitiesListQueryHandler(db).Handle(query, cancellationToken),
                GetDecisionBodiesQuery query => await new GetDecisionBodiesQueryHandler(db).Handle(query, cancellationToken),
                GetDecisionPanelByIdQuery query => await new GetDecisionPanelByIdQueryHandler(db).Handle(query, cancellationToken),
                ListPanelMemberCandidatesQuery query =>
                    await new ListPanelMemberCandidatesQueryHandler(_users).Handle(query, cancellationToken),
                ListDecisionPanelsQuery query => await new ListDecisionPanelsQueryHandler(db, _users).Handle(query, cancellationToken),
                GetCommitteeRoutingQuery query => await new GetCommitteeRoutingQueryHandler(db).Handle(query, cancellationToken),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
