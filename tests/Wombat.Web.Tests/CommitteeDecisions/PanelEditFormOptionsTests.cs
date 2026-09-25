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
/// The new-panel form offers each administrator exactly the scopes and specialities creating a panel accepts from them,
/// and nothing when that is nothing. (T194)
/// </summary>
/// <remarks>
/// <para>
/// These run the real handlers against an in-memory store, as <see cref="ReviewsScheduleTraineeFirstTests" /> does: the
/// rule under test is the handler's (<c>CommitteeDecisionAuthorization.PanelReachAsync</c>, read through
/// <c>GetDecisionPanelFormOptionsQuery</c>), and the speciality list the form used to read
/// (<c>GetSpecialitiesListQuery</c>) is real too, because it is what left a Speciality or SubSpecialityAdmin with no
/// speciality to choose before T194.
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
    private const int GeneralPaediatrics = 11;
    private const int GeneralSurgery = 21;

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
        // Every speciality is theirs to create a panel for; the form offers the ones their institution has adopted, the
        // speciality list it always read for them.
        SignInAs(WombatRoles.InstitutionalAdmin);
        var cut = RenderForm();

        Values(cut, "#panel-scope option")
            .Should().Equal(DecisionPanelScope.Institution.ToString(), DecisionPanelScope.Speciality.ToString());
        Values(cut, "#panel-speciality option").Should().Equal(string.Empty, Paediatrics.ToString());
        cut.FindAll("#panel-none-creatable").Should().BeEmpty();
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

    private void SignInAs(string role, int? specialityId = null, int? subSpecialityId = null, int? institutionId = InstitutionA)
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
            new Speciality { Id = Surgery, CollegeId = 1, Name = "Surgery", IsActive = true });
        db.SubSpecialities.AddRange(
            new SubSpeciality { Id = GeneralPaediatrics, SpecialityId = Paediatrics, Name = "General Paediatrics", IsActive = true },
            new SubSpeciality { Id = GeneralSurgery, SpecialityId = Surgery, Name = "General Surgery", IsActive = true });
        db.Curricula.Add(new Curriculum { Id = 100, SubSpecialityId = GeneralPaediatrics, Name = "General Paediatrics", Version = "11.1" });
        // A trains paediatrics only: the specialities an institutional administrator's list holds.
        db.Set<InstitutionCurriculumAdoption>().Add(new InstitutionCurriculumAdoption
        {
            Id = 1, InstitutionId = InstitutionA, CurriculumId = 100, SubSpecialityId = GeneralPaediatrics,
            AdoptedOn = new DateOnly(2026, 1, 1), IsActive = true
        });
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
                GetSpecialitiesListQuery query => await new GetSpecialitiesListQueryHandler(db).Handle(query, cancellationToken),
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
