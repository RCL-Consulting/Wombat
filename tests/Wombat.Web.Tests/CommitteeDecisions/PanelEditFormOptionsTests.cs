using System.Security.Claims;
using AngleSharp.Dom;
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
/// The new-panel form offers each administrator exactly the scopes and specialities creating a panel accepts from them,
/// and nothing when that is nothing. (T194) A speciality is offered, and accepted, only where the panel's institution has
/// adopted it, whoever creates the panel (T245 and its review).
/// </summary>
/// <remarks>
/// <para>
/// These run the real handlers against an in-memory store, as <see cref="ReviewsScheduleTraineeFirstTests" /> does: the
/// rule under test is the handler's (<c>CommitteeDecisionAuthorization.PanelReachAsync</c> and
/// <c>CreatableSpecialityIdsAsync</c>, read through <c>GetDecisionPanelFormOptionsQuery</c>). The speciality list the form
/// used to read (<c>GetSpecialitiesListQuery</c>, which left a Speciality or SubSpecialityAdmin with no speciality to
/// choose before T194, and offered an Administrator every speciality at every institution before the T245 review) is not
/// answered here, so a page that read it again would fail to load.
/// </para>
/// <para>
/// Each offer is then taken: the form is submitted and the real create handler accepts it, so what is offered is shown to
/// be what is accepted, not only what the options say.
/// </para>
/// </remarks>
public sealed class PanelEditFormOptionsTests : TestContext
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int Paediatrics = 1;
    private const int Surgery = 2;
    private const int GeneralMedicine = 3;
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 21;
    private const int GeneralInternalMedicine = 31;

    private const string AdoptedOnlyHelp =
        "The panel runs at your institution. Only the specialities it has adopted a curriculum in are listed.";

    private const string ChooseTheInstitutionFirstHelp =
        "Choose the institution first: only the specialities it has adopted a curriculum in are listed.";

    private const string ChosenInstitutionHelp =
        "Only the specialities the chosen institution has adopted a curriculum in are listed.";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly TestAuthorizationContext _auth;

    public PanelEditFormOptionsTests()
    {
        _auth = this.AddTestAuthorization();
        Services.AddSingleton<IScopedSender>(new HandlerSender(CreateDb));
        Seed();
    }

    [Fact]
    public void ASpecialityAdmin_IsOfferedOnlyTheSpecialityScope_AndOnlyTheirOwnSpeciality_AndItIsAccepted()
    {
        // Before T194 the speciality list offered them nothing (it lists a College's specialities, or an institutional
        // administrator's adopted ones), so a SpecialityAdmin could create no panel at all.
        SignInAs(WombatRoles.SpecialityAdmin, specialityId: Paediatrics);
        var cut = RenderForm();

        Values(cut, "#panel-scope option").Should().Equal(DecisionPanelScope.Speciality.ToString());
        Values(cut, "#panel-speciality option").Should().Equal(string.Empty, Paediatrics.ToString());
        cut.FindAll("#panel-institution").Should().BeEmpty("the panel runs at their own institution");

        Create(cut, Paediatrics);

        StoredPanels().Should().ContainSingle()
            .Which.Should().Be((DecisionPanelScope.Speciality, InstitutionA, (int?)Paediatrics));
    }

    [Fact]
    public void ASubSpecialityAdmin_IsOfferedTheSpecialityTheirSubSpecialityBelongsTo_AndItIsAccepted()
    {
        SignInAs(WombatRoles.SubSpecialityAdmin, subSpecialityId: GeneralSurgery);
        var cut = RenderForm();

        Values(cut, "#panel-scope option").Should().Equal(DecisionPanelScope.Speciality.ToString());
        Values(cut, "#panel-speciality option").Should().Equal(string.Empty, Surgery.ToString());

        Create(cut, Surgery);

        StoredPanels().Should().ContainSingle()
            .Which.Should().Be((DecisionPanelScope.Speciality, InstitutionA, (int?)Surgery));
    }

    [Fact]
    public void AnInstitutionalAdmin_IsOfferedBothScopes_AndTheSpecialitiesTheirInstitutionTrains()
    {
        // The form offers the specialities their institution has adopted, which the speciality list always offered them,
        // and since T245 create accepts no other: General Medicine, which A does not train, is neither.
        SignInAs(WombatRoles.InstitutionalAdmin);
        var cut = RenderForm();

        Values(cut, "#panel-scope option")
            .Should().Equal(DecisionPanelScope.Institution.ToString(), DecisionPanelScope.Speciality.ToString());
        Values(cut, "#panel-speciality option").Should().Equal(string.Empty, Paediatrics.ToString(), Surgery.ToString());
        cut.FindAll("#panel-none-creatable").Should().BeEmpty();
    }

    [Fact]
    public void AnAdministrator_IsOfferedTheSpecialitiesOfTheInstitutionTheyChoose_AndTheHelpSaysSo()
    {
        // T245 review. An Administrator names the institution of every panel, and was offered every speciality whichever
        // they chose, General Medicine included, which nobody trains: create accepted it, a panel with no trainee.
        SignInAs(WombatRoles.Administrator, institutionId: null);
        var cut = RenderForm();

        Values(cut, "#panel-scope option")
            .Should().Equal(DecisionPanelScope.Institution.ToString(), DecisionPanelScope.Speciality.ToString());
        Values(cut, "#panel-speciality option").Should().Equal(string.Empty);
        var helpId = FieldHelp.Id("panel-speciality");
        cut.Find("#" + helpId).TextContent.Trim().Should().Be(ChooseTheInstitutionFirstHelp);
        cut.Find("#panel-speciality").GetAttribute("aria-describedby").Should().Contain(helpId);
        cut.Markup.Should().NotContain(AdoptedOnlyHelp, "an Administrator's panel does not run at an institution of theirs");

        cut.Find("#panel-institution").Change(InstitutionA.ToString());

        Values(cut, "#panel-speciality option").Should().Equal(string.Empty, Paediatrics.ToString(), Surgery.ToString());
        cut.Find("#" + helpId).TextContent.Trim().Should().Be(ChosenInstitutionHelp);

        // B has adopted nothing: the Surgery chosen at A is unchosen, so the form never sends what the save refuses.
        cut.Find("#panel-speciality").Change(Surgery.ToString());
        cut.Find("#panel-institution").Change(InstitutionB.ToString());

        Values(cut, "#panel-speciality option").Should().Equal(string.Empty);
        cut.Find("#panel-speciality").GetAttribute("value").Should().BeNullOrEmpty();

        cut.Find("#panel-institution").Change(InstitutionA.ToString());
        cut.Find("#panel-speciality").GetAttribute("value").Should().BeNullOrEmpty("going back to A chooses nothing again");

        Create(cut, Paediatrics);

        StoredPanels().Should().ContainSingle()
            .Which.Should().Be((DecisionPanelScope.Speciality, InstitutionA, (int?)Paediatrics));
    }

    [Fact]
    public void AnAdministratorsCreateForASpecialityTheChosenInstitutionHasNotAdopted_IsRefusedInAnAlert_AndNothingIsStored()
    {
        // T245 review. B has adopted nothing, so the form offers no speciality there; the Speciality select is made to
        // carry Paediatrics, which A trains, to reach the refusal a request the form did not build gets.
        SignInAs(WombatRoles.Administrator, institutionId: null);
        var cut = RenderForm();

        cut.Find("#panel-name").Change("Paediatrics CCC");
        cut.Find("#panel-institution").Change(InstitutionB.ToString());
        cut.Find("#panel-speciality").Change(Paediatrics.ToString());
        cut.Find("#panel-chair").Change("chair-b");
        cut.Find("#panel-members").Change(new[] { "member-b" });
        cut.Find("form").Submit();

        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);
        var refusal = cut.Find(".alert-danger");
        refusal.GetAttribute("role").Should().Be("alert", "a refusal after a click is announced at once");
        refusal.TextContent.Trim().Should().Be(
            "The institution you chose has adopted no curriculum in this speciality, so a panel for it would have no " +
            "trainee to review.");
        StoredPanels().Should().BeEmpty();
    }

    [Fact]
    public void ASpecialityAdmin_IsOfferedOnlyTheirSpecialitiesTheirInstitutionHasAdopted_AndTheHelpSaysSo()
    {
        // T245. Their claims name Paediatrics and General Medicine, and A trains only Paediatrics of the two. Before T245
        // the form offered both, and create accepted a General Medicine panel that could never have a trainee.
        SignInAs(WombatRoles.SpecialityAdmin, specialityId: Paediatrics, secondSpecialityId: GeneralMedicine);
        var cut = RenderForm();

        Values(cut, "#panel-speciality option").Should().Equal(string.Empty, Paediatrics.ToString());
        var helpId = FieldHelp.Id("panel-speciality");
        cut.Find("#" + helpId).TextContent.Trim().Should().Be(AdoptedOnlyHelp);
        cut.Find("#panel-speciality").GetAttribute("aria-describedby").Should().Contain(helpId);

        Create(cut, Paediatrics);

        StoredPanels().Should().ContainSingle()
            .Which.Should().Be((DecisionPanelScope.Speciality, InstitutionA, (int?)Paediatrics));
    }

    [Theory]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.SubSpecialityAdmin)]
    public void ASpecialityAdmin_WhoseSpecialityTheirInstitutionHasNotAdopted_IsOfferedNoForm_ButToldWhy(string role)
    {
        // T245. Their only speciality is General Medicine, which A does not train, so create accepts no panel from them.
        if (role == WombatRoles.SpecialityAdmin)
        {
            SignInAs(role, specialityId: GeneralMedicine);
        }
        else
        {
            SignInAs(role, subSpecialityId: GeneralInternalMedicine);
        }

        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-none-creatable").Count == 1);

        cut.FindAll("form").Should().BeEmpty();
        cut.Find("#panel-none-creatable").TextContent.Should()
            .Contain("You cannot create a decision panel.").And
            .Contain("once the institution has adopted a curriculum in it");
    }

    [Fact]
    public void ACreateForASpecialityTheFormDoesNotOffer_IsRefusedInAnAlert_AndNothingIsStored()
    {
        // T245. The form and create read one rule, so the refusal is reached only by a request the form did not build:
        // here the Speciality select is made to carry General Medicine, which it does not offer.
        SignInAs(WombatRoles.SpecialityAdmin, specialityId: Paediatrics, secondSpecialityId: GeneralMedicine);
        var cut = RenderForm();

        cut.Find("#panel-name").Change("General Medicine CCC");
        cut.Find("#panel-speciality").Change(GeneralMedicine.ToString());
        cut.Find("#panel-chair").Change("chair-a");
        cut.Find("#panel-members").Change(new[] { "member-a" });
        cut.Find("form").Submit();

        cut.WaitForState(() => cut.FindAll(".alert-danger").Count == 1);
        var refusal = cut.Find(".alert-danger");
        refusal.GetAttribute("role").Should().Be("alert", "a refusal after a click is announced at once");
        refusal.TextContent.Trim().Should().Be(
            "Your institution has adopted no curriculum in this speciality, so a panel for it would have no trainee to review.");
        StoredPanels().Should().BeEmpty();
    }

    [Fact]
    public void AnInstitutionalAdminWhoseInstitutionTrainsNoSpeciality_StartsOnTheInstitutionWideScope_AndItIsAccepted()
    {
        // B has adopted no curriculum, so the only scope offered is the institution-wide one, and the form must start on
        // it. A model left on Speciality would render an empty Speciality select and no institution picker, and choosing
        // Institution, already shown as selected, would fire no change: every save would be refused. (T194 review)
        SignInAs(WombatRoles.InstitutionalAdmin, institutionId: InstitutionB);
        var cut = RenderForm();

        Values(cut, "#panel-scope option").Should().Equal(DecisionPanelScope.Institution.ToString());
        cut.FindAll("#panel-speciality").Should().BeEmpty("no speciality is theirs to offer");
        cut.FindAll("#panel-institution").Should().ContainSingle("an institution-wide panel names its institution");

        cut.Find("#panel-name").Change("Hospital CCC");
        cut.Find("#panel-institution").Change(InstitutionB.ToString());
        cut.Find("#panel-chair").Change("chair-b");
        cut.Find("#panel-members").Change(new[] { "member-b" });
        cut.Find("form").Submit();

        cut.FindAll(".alert-danger").Should().BeEmpty("the create handler accepts what the form offered");
        StoredPanels().Should().ContainSingle()
            .Which.Should().Be((DecisionPanelScope.Institution, InstitutionB, (int?)null));
    }

    [Fact]
    public void ASpecialityAdminWithNoInstitution_IsOfferedNoEmptyForm_ButToldWhy()
    {
        // Creating any panel would be refused them: a panel runs at the creator's institution, and they have none.
        SignInAs(WombatRoles.SpecialityAdmin, specialityId: Paediatrics, institutionId: null);
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-none-creatable").Count == 1);

        cut.FindAll("#panel-scope").Should().BeEmpty();
        cut.FindAll("form").Should().BeEmpty();
        cut.Find("#panel-none-creatable").TextContent.Should().Contain("You cannot create a decision panel.");
    }

    private IRenderedComponent<PanelEdit> RenderForm()
    {
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);
        cut.FindAll("#panel-none-creatable").Should().BeEmpty();
        return cut;
    }

    private static void Create(IRenderedComponent<PanelEdit> cut, int specialityId)
    {
        cut.Find("#panel-name").Change("Programme CCC");
        cut.Find("#panel-speciality").Change(specialityId.ToString());
        cut.Find("#panel-chair").Change("chair-a");
        cut.Find("#panel-members").Change(new[] { "member-a" });
        cut.Find("form").Submit();
        cut.FindAll(".alert-danger").Should().BeEmpty("the create handler accepts what the form offered");
    }

    private static IReadOnlyList<string> Values(IRenderedComponent<PanelEdit> cut, string selector)
        => cut.FindAll(selector).Select(option => option.GetAttribute("value") ?? string.Empty).ToArray();

    private IReadOnlyList<(DecisionPanelScope Scope, int InstitutionId, int? SpecialityId)> StoredPanels()
    {
        using var db = CreateDb();
        return db.DecisionPanels.AsNoTracking()
            .AsEnumerable()
            .Select(panel => (panel.Scope, panel.InstitutionId, panel.SpecialityId))
            .ToArray();
    }

    private void SignInAs(
        string role,
        int? specialityId = null,
        int? subSpecialityId = null,
        int? institutionId = InstitutionA,
        int? secondSpecialityId = null)
    {
        _auth.SetAuthorized($"{role}@test");
        _auth.SetRoles(role);

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, $"{role}-user") };
        if (institutionId is int institution)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, institution.ToString()));
        }

        if (specialityId is int speciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, speciality.ToString()));
        }

        // A user's claims may name several specialities (T245).
        if (secondSpecialityId is int secondSpeciality)
        {
            claims.Add(new Claim(WombatClaimTypes.SpecialityId, secondSpeciality.ToString()));
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
            // B trains nothing yet: it has adopted no curriculum.
            new Institution { Id = InstitutionB, Name = "B", ShortCode = "B", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.Colleges.Add(new College { Id = 1, Name = "CMSA", ShortCode = "CMSA", IsActive = true });
        db.Specialities.AddRange(
            new Speciality { Id = Paediatrics, CollegeId = 1, Name = "Paediatrics", IsActive = true },
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery", IsActive = true },
            new Speciality { Id = GeneralMedicine, CollegeId = 1, Name = "General Medicine", IsActive = true });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery", IsActive = true },
            new SubSpeciality { Id = GeneralInternalMedicine, SpecialityId = GeneralMedicine, Name = "General Internal Medicine", IsActive = true });
        db.Curricula.AddRange(
            new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" },
            new Curriculum { Id = 200, SubSpecialityId = GeneralSurgery, Name = "General Surgery", Version = "1" });
        // A trains paediatrics and surgery, the specialities an institutional administrator's list holds and the only ones
        // a panel at A may cover (T245); nobody trains General Medicine.
        AdoptionSeed.Adopt(db, 1, InstitutionA, 100, GeneralPaediatrics);
        AdoptionSeed.Adopt(db, 2, InstitutionA, 200, GeneralSurgery);
        db.SaveChanges();
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    /// <summary>
    /// Sends the form's requests to the real handlers, each on a fresh context over the one store, as a circuit's scoped
    /// sender would. The committee members at A, and at B, are the people a panel at each may seat.
    /// </summary>
    private sealed class HandlerSender(Func<ApplicationDbContext> createDb) : IScopedSender
    {
        private readonly FakeUserDirectory _users = FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a")
            .WithCommitteeMembers(InstitutionB, "chair-b", "member-b");

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            await using var db = createDb();

            object answer = request switch
            {
                GetDecisionPanelFormOptionsQuery query =>
                    await new GetDecisionPanelFormOptionsQueryHandler(db).Handle(query, cancellationToken),
                GetInstitutionsListQuery query => await new GetInstitutionsListQueryHandler(db).Handle(query, cancellationToken),
                GetDecisionBodiesQuery query => await new GetDecisionBodiesQueryHandler(db).Handle(query, cancellationToken),
                ListPanelMemberCandidatesQuery query =>
                    await new ListPanelMemberCandidatesQueryHandler(_users).Handle(query, cancellationToken),
                CreateDecisionPanelCommand command =>
                    await new CreateDecisionPanelCommandHandler(db, _users).Handle(command, cancellationToken),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
