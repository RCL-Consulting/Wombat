using System.Security.Claims;
using System.Text.RegularExpressions;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Domain.Institutions;
using Wombat.Infrastructure.Persistence;
using Wombat.Tests.Shared;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// No one who holds Trainee is offered for a seat on a decision panel, the Chair's, a member's or an External member's,
/// and a panel that already seats one says so and takes them off at its next save. (T237)
/// </summary>
/// <remarks>
/// <para>
/// Since T194 someone who holds Trainee acts from no seat (<c>CommitteeDecisionAuthorization.HoldsSeat</c>), so a panel
/// chaired by one had no working chair, and a quorum could count one. The rule is <c>PanelSeat</c>'s, which the picker
/// lists by and the save enforces: these run the real handlers against an in-memory store, as
/// <see cref="PanelEditFormOptionsTests" /> does, so what the form offers is shown to be what the save accepts. The user
/// directory answers the role listing as the real one does, each record carrying only the role it was listed by.
/// </para>
/// </remarks>
public sealed partial class PanelEditTraineeSeatTests : TestContext
{
    private const int InstitutionA = 1;
    private const int PanelId = 20;

    /// <summary>A committee member at A who also holds Trainee: the trainees' representative.</summary>
    private const string Representative = "rep-a";

    private readonly string _databaseName = Guid.NewGuid().ToString();
    private readonly TestAuthorizationContext _auth;

    public PanelEditTraineeSeatTests()
    {
        _auth = this.AddTestAuthorization();
        Services.AddSingleton<IScopedSender>(new HandlerSender(CreateDb));
        SignInAsInstitutionalAdmin();
        Seed();
    }

    [Fact]
    public void ANewPanel_OffersSomeoneWhoHoldsTrainee_ForNoSeat_AndWhatItOffersIsAccepted()
    {
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-chair option").Count > 1);

        foreach (var seat in Seats)
        {
            Values(cut, seat).Should().NotContain(Representative, seat).And.Contain("member-a", seat);
        }

        Values(cut, "#panel-chair").Should().Equal("chair-a", "external-a", "member-a");
        Text(cut.Find("#panel-external-help")).Should().Be(
            "External examiners, who sit with the chair on the panel's appeal body. Only active committee members at the " +
            "panel's institution who are not trainees are listed: cross-institution externals are not yet supported.");

        cut.Find("#panel-name").Change("Hospital CCC");
        cut.Find("#panel-institution").Change(InstitutionA.ToString());
        cut.Find("#panel-chair").Change("chair-a");
        cut.Find("#panel-members").Change(new[] { "member-a" });
        cut.Find("#panel-external").Change(new[] { "external-a" });
        cut.Find("form").Submit();

        cut.FindAll(".alert-danger").Should().BeEmpty("the create handler accepts what the form offered");
        StoredMembers().Should().Equal("chair-a:Chair", "external-a:External", "member-a:Member");
    }

    [Theory]
    [InlineData(DecisionPanelMemberRole.Chair)]
    [InlineData(DecisionPanelMemberRole.External)]
    public void APanelSeatingSomeoneWhoHoldsTrainee_SaysTheyCanNoLongerSit_AndTheSaveTakesThemOff(DecisionPanelMemberRole seat)
    {
        // Seated before T237, or given Trainee since: the seat cannot act, so the form does not carry them over, and says
        // so before the save rather than after it.
        SeatPanel(seat == DecisionPanelMemberRole.Chair
            ? [(Representative, DecisionPanelMemberRole.Chair), ("member-a", DecisionPanelMemberRole.Member)]
            : [
                ("chair-a", DecisionPanelMemberRole.Chair), ("member-a", DecisionPanelMemberRole.Member),
                (Representative, DecisionPanelMemberRole.External)
            ]);

        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair option").Count > 1);

        Text(cut.Find(".alert-warning")).Should().Be(
            "1 member of this panel can no longer sit on it, so is not listed below: only an active committee member at " +
            "its institution who is not a trainee can. Saving takes that member off the panel.");
        foreach (var picker in Seats)
        {
            Values(cut, picker).Should().NotContain(Representative, picker);
        }

        if (seat == DecisionPanelMemberRole.Chair)
        {
            // The chair's seat is left empty, so the form is not sent until another chair is chosen.
            MembersForm(cut).Submit();
            cut.FindAll(".validation-summary-errors li").Select(Text).Should().Equal(
                ["Choose a chair."], "a panel needs a chair, said in words rather than by the model's property name");
            StoredMembers().Should().Equal("member-a:Member", $"{Representative}:Chair");

            cut.Find("#panel-chair").Change("chair-a");
        }

        MembersForm(cut).Submit();

        cut.FindAll(".alert-danger").Should().BeEmpty("the update handler accepts what the form sends");
        StoredMembers().Should().Equal("chair-a:Chair", "member-a:Member");
    }

    /// <summary>Every seat's picker on the members form.</summary>
    private static readonly string[] Seats = ["#panel-chair", "#panel-members", "#panel-external"];

    private static AngleSharp.Dom.IElement MembersForm(IRenderedComponent<PanelEdit> cut)
        => cut.FindAll("form").Single(form => form.QuerySelector("#panel-chair") is not null);

    private static IReadOnlyList<string> Values(IRenderedComponent<PanelEdit> cut, string select)
        => cut.FindAll($"{select} option")
            .Select(option => option.GetAttribute("value") ?? string.Empty)
            .Where(value => value.Length > 0)
            .ToArray();

    private static string Text(AngleSharp.Dom.IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private IReadOnlyList<string> StoredMembers()
    {
        using var db = CreateDb();
        return db.Set<DecisionPanelMember>().AsNoTracking()
            .AsEnumerable()
            .Select(member => $"{member.UserId}:{member.Role}")
            .OrderBy(member => member, StringComparer.Ordinal)
            .ToArray();
    }

    private void SignInAsInstitutionalAdmin()
    {
        _auth.SetAuthorized("instadmin@test");
        _auth.SetRoles(WombatRoles.InstitutionalAdmin);
        _auth.SetClaims(
            new Claim(ClaimTypes.NameIdentifier, "instadmin"),
            new Claim(WombatClaimTypes.InstitutionId, InstitutionA.ToString()));
    }

    private void Seed()
    {
        using var db = CreateDb();
        // A has adopted no curriculum, so an institutional administrator is offered the institution-wide scope only.
        db.Institutions.Add(new Institution { Id = InstitutionA, Name = "A", ShortCode = "A", IsActive = true, CreatedOn = DateTime.UtcNow });
        db.SaveChanges();
    }

    private void SeatPanel(IReadOnlyList<(string UserId, DecisionPanelMemberRole Role)> members)
    {
        using var db = CreateDb();
        db.DecisionPanels.Add(new DecisionPanel
        {
            Id = PanelId,
            Name = "Hospital CCC",
            Scope = DecisionPanelScope.Institution,
            InstitutionId = InstitutionA,
            CreatedOn = DateTime.UtcNow,
            Members = members.Select(member => new DecisionPanelMember { UserId = member.UserId, Role = member.Role }).ToList()
        });
        db.SaveChanges();
    }

    private ApplicationDbContext CreateDb()
        => new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options);

    /// <summary>
    /// Sends the form's requests to the real handlers, each on a fresh context over the one store, as a circuit's scoped
    /// sender would. Three committee members at A may sit; the fourth also holds Trainee.
    /// </summary>
    private sealed class HandlerSender(Func<ApplicationDbContext> createDb) : IScopedSender
    {
        private readonly FakeUserDirectory _users = FakeUserDirectory.CommitteeMembersAt(InstitutionA, "chair-a", "member-a", "external-a")
            .With(new UserIdentityDetails(
                Representative, "rep@test", "Demo", "Trainee", InstitutionA, [], [],
                [WombatRoles.CommitteeMember, WombatRoles.Trainee]));

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
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
                CreateDecisionPanelCommand command =>
                    await new CreateDecisionPanelCommandHandler(db, _users).Handle(command, cancellationToken),
                UpdateDecisionPanelCommand command =>
                    await new UpdateDecisionPanelCommandHandler(db, _users).Handle(command, cancellationToken),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return (TResponse)answer!;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
