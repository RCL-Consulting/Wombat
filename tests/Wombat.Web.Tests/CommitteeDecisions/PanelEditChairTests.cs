using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using static Wombat.Web.Tests.CommitteeDecisions.PanelSeatPicker;

namespace Wombat.Web.Tests.CommitteeDecisions;

//// <summary>
/// Choosing as chair someone already seated takes them out of that seat and says so, so the save does not send them twice
/// to be refused; choosing another chair puts them back. (T257; T364, Q5: the seat picker replaces the multi-selects)
/// </summary>
/// <remarks>
/// The sender here runs the commands' own validators, which is where "A panel member is listed more than once." comes
/// from. In Chrome and Edge on Windows each arrow key on a closed select is a change, so every candidate passed on the way
/// down the Chair select is chosen for a moment: each is put back when the next is chosen, or moving through the list
/// emptied the panel.
/// </remarks>
public sealed class PanelEditChairTests : TestContext
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int PanelId = 20;

    private readonly RecordingSender _sender = new();

    // The panel as stored, which Save panel writes and the page reads again (C12).
    private IReadOnlyList<DecisionPanelMemberDto> _stored =
    [
        new(1, "zulu", DecisionPanelMemberRole.Chair),
        new(2, "naidoo", DecisionPanelMemberRole.Member),
        new(3, "mokoena", DecisionPanelMemberRole.Member),
        new(4, "botha", DecisionPanelMemberRole.External)
    ];

    public PanelEditChairTests()
    {
        Services.AddSingleton<IScopedSender>(_sender);
        _sender
            .On<GetInstitutionsListQuery>(_ => new[]
            {
                new InstitutionDto(InstitutionA, "Kgosi Kgari", "KGK", null, true, DateTime.UtcNow),
                new InstitutionDto(InstitutionB, "Other Hospital", "OTH", null, true, DateTime.UtcNow)
            })
            // An InstitutionalAdmin: both scopes, and the specialities their institution has adopted (T245).
            .On<GetDecisionPanelFormOptionsQuery>(_ => new DecisionPanelFormOptionsDto(
                MayCreateInstitutionWide: true, Specialities: [new SpecialityDto(4, 1, "Paediatrics", null, true)]))
            .On<GetDecisionBodiesQuery>(_ => Array.Empty<DecisionBodyDto>())
            .On<ListPanelMemberCandidatesQuery>(query => query.InstitutionId == InstitutionB
                ? new[] { new PanelMemberCandidateDto("dube", "dube@test", "Musa", "Dube", InstitutionB) }
                : new[]
                {
                    new PanelMemberCandidateDto("zulu", "zulu@test", "Thandi", "Zulu", InstitutionA),
                    new PanelMemberCandidateDto("naidoo", "naidoo@test", "Priya", "Naidoo", InstitutionA),
                    new PanelMemberCandidateDto("mokoena", "mokoena@test", "Sipho", "Mokoena", InstitutionA),
                    new PanelMemberCandidateDto("botha", "botha@test", "Anna", "Botha", InstitutionA),
                    // On no seat of the stored panel.
                    new PanelMemberCandidateDto("dlamini", "dlamini@test", "Lindiwe", "Dlamini", InstitutionA)
                })
            .On<GetDecisionPanelByIdQuery>(_ => new DecisionPanelDetailDto(
                PanelId, "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionA, null, _stored))
            .On<UpdateDecisionPanelCommand>(command =>
            {
                new UpdateDecisionPanelCommandValidator().ValidateAndThrow(command);
                _stored = command.Members.Select((member, index) => new DecisionPanelMemberDto(index + 10, member.UserId, member.Role)).ToArray();
                return new DecisionPanelDetailDto(PanelId, "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionA, null, []);
            })
            .On<CreateDecisionPanelCommand>(command =>
            {
                new CreateDecisionPanelCommandValidator().ValidateAndThrow(command);
                return new DecisionPanelDetailDto(21, command.Name, command.Scope, InstitutionA, null, []);
            });

        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("instadmin@test");
        auth.SetRoles(WombatRoles.InstitutionalAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "instadmin"));
    }

    [Fact]
    public void ChoosingAMemberAsChair_TakesThemOutOfTheirSeat_SaysSo_AndTheSaveSucceeds()
    {
        var cut = RenderPanel();

        cut.Find("#panel-chair").Change("naidoo");

        SeatOf(cut, "Priya Naidoo").Should().Be("chair", "the chair's row reads Chair · chosen above");
        SeatOf(cut, "Sipho Mokoena").Should().Be("member");
        SeatOf(cut, "Thandi Zulu").Should().Be("none", "the chair before is offered, not seated");
        Note(cut).Should().Be("Priya Naidoo is the chair now, so is no longer seated as a member.");
        cut.Find("#panel-chair").GetAttribute("aria-describedby").Should().Be("panel-chair-help panel-chair-note",
            "the Chair select names the line while it says something");
        IdReferences.Broken(cut).Should().BeEmpty();

        PanelForm(cut).Submit();

        cut.FindAll(".validation-summary-errors").Should().BeEmpty("the chair is sent once");
        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().ContainSingle().Which.Members
            .Select(member => $"{member.UserId}:{member.Role}")
            .Should().Equal("naidoo:Chair", "mokoena:Member", "botha:External");
        Squash(cut.Find(".alert-success").TextContent)
            .Should().Be("Panel members updated. Priya Naidoo chairs, with 1 member and 1 external member.");
        Note(cut).Should().BeEmpty("the change it described is saved, and the result says so");
        cut.Find("#panel-chair").GetAttribute("aria-describedby").Should().Be("panel-chair-help");

        // Naidoo is the stored chair now, so choosing another does not seat them again.
        cut.Find("#panel-chair").Change("mokoena");

        SeatOf(cut, "Priya Naidoo").Should().Be("none");
        Note(cut).Should().Be("Sipho Mokoena is the chair now, so is no longer seated as a member.");
    }

    [Fact]
    public void ChoosingAnExternalMemberAsChair_TakesThemOutOfThatSeat_AndTheSaveSucceeds()
    {
        var cut = RenderPanel();

        cut.Find("#panel-chair").Change("botha");

        SeatOf(cut, "Anna Botha").Should().Be("chair");
        SeatOf(cut, "Priya Naidoo").Should().Be("member");
        Note(cut).Should().Be("Anna Botha is the chair now, so is no longer seated as an external member.");

        PanelForm(cut).Submit();

        cut.FindAll(".validation-summary-errors").Should().BeEmpty();
        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().ContainSingle().Which.Members
            .Select(member => $"{member.UserId}:{member.Role}")
            .Should().Equal("botha:Chair", "naidoo:Member", "mokoena:Member");
    }

    [Fact]
    public void TheLine_IsALiveRegionBeforeAnythingIsChosen_AndClearsWhenAChoiceTakesNobodyOffAndPutsNobodyBack()
    {
        var cut = RenderPanel();

        var region = cut.Find("#panel-chair-note");
        region.GetAttribute("role").Should().Be("status", "polite, and present before it says anything, so it is read");
        region.TextContent.Should().BeEmpty();
        cut.Find("#panel-chair").GetAttribute("aria-describedby").Should().Be("panel-chair-help", "it names no empty line");

        cut.Find("#panel-chair").Change("naidoo");
        Note(cut).Should().StartWith("Priya Naidoo is the chair now");

        // Back to the stored chair, who holds no other seat: Naidoo is seated again, and the line says so, since it said a
        // moment ago that they were taken out.
        cut.Find("#panel-chair").Change("zulu");

        Note(cut).Should().Be("Priya Naidoo is seated as a member again.");
        SeatOf(cut, "Priya Naidoo").Should().Be("member");
        SeatOf(cut, "Sipho Mokoena").Should().Be("member");

        // A chair who held no seat, with nobody to put back: nothing to say.
        cut.Find("#panel-chair").Change("dlamini");

        Note(cut).Should().BeEmpty();
        cut.Find("#panel-chair").GetAttribute("aria-describedby").Should().Be("panel-chair-help");
    }

    [Fact]
    public void ChoosingAMemberAsChair_AndThenTheChairBefore_KeepsEveryoneOnThePanel()
    {
        // The review's scenario: a keyboard user presses Down once (Naidoo) and Up once (back to Zulu). Each is a change.
        var cut = RenderPanel();

        cut.Find("#panel-chair").Change("naidoo");
        cut.Find("#panel-chair").Change("zulu");
        PanelForm(cut).Submit();

        cut.FindAll(".validation-summary-errors").Should().BeEmpty();
        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().ContainSingle().Which.Members
            .Select(member => $"{member.UserId}:{member.Role}")
            .Should().BeEquivalentTo("zulu:Chair", "naidoo:Member", "mokoena:Member", "botha:External");
    }

    [Fact]
    public void MovingThroughTheChairSelect_PutsEachCandidatePassedBackWhereTheyWere()
    {
        var cut = RenderPanel();

        cut.Find("#panel-chair").Change("naidoo");
        cut.Find("#panel-chair").Change("mokoena");

        SeatOf(cut, "Priya Naidoo").Should().Be("member");
        Note(cut).Should().Be(
            "Sipho Mokoena is the chair now, so is no longer seated as a member. Priya Naidoo is seated as a member again.");

        cut.Find("#panel-chair").Change("botha");

        SeatOf(cut, "Sipho Mokoena").Should().Be("member");
        Note(cut).Should().Be(
            "Anna Botha is the chair now, so is no longer seated as an external member. Sipho Mokoena is seated as a member again.");

        cut.Find("#panel-chair").Change("dlamini");

        SeatOf(cut, "Anna Botha").Should().Be("external");
        Note(cut).Should().Be("Anna Botha is seated as an external member again.");

        // Choosing no chair after one who held no seat takes nobody off and puts nobody back.
        cut.Find("#panel-chair").Change(string.Empty);

        Note(cut).Should().BeEmpty();
        Seats(cut).Should().Equal(
            "Thandi Zulu:none", "Priya Naidoo:member", "Sipho Mokoena:member", "Anna Botha:external", "Lindiwe Dlamini:none");
    }

    [Fact]
    public void ReadingThePanelAgain_EmptiesTheLine_AndPutsNobodyBackLater()
    {
        var cut = RenderPanel();
        cut.Find("#panel-chair").Change("naidoo");
        Note(cut).Should().NotBeEmpty();

        cut.SetParametersAndRender(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair").Count == 1);

        Note(cut).Should().BeEmpty("the panel is as stored again, so what the line said no longer holds");
        SeatOf(cut, "Priya Naidoo").Should().Be("member");

        // Naidoo is seated because the panel was read, not because a choice of chair is undone.
        cut.Find("#panel-chair").Change("mokoena");

        Note(cut).Should().Be("Sipho Mokoena is the chair now, so is no longer seated as a member.");
    }

    [Fact]
    public void ChangingTheInstitution_EmptiesTheLine_AndPutsNobodyBackLater()
    {
        var cut = RenderNewInstitutionPanel();
        Choose(cut, "Priya Naidoo", "member");
        Choose(cut, "Sipho Mokoena", "member");
        cut.Find("#panel-chair").Change("naidoo");
        Note(cut).Should().NotBeEmpty();

        cut.Find("#panel-institution").Change(InstitutionB.ToString());

        Note(cut).Should().BeEmpty("the people it names cannot sit at the institution now chosen");
        cut.Find("#panel-chair").GetAttribute("aria-describedby").Should().Be("panel-chair-help");
        Listed(cut).Should().Equal("Musa Dube");

        // Back at the first institution nobody is seated, and choosing a chair puts nobody back.
        cut.Find("#panel-institution").Change(InstitutionA.ToString());
        cut.Find("#panel-chair").Change("mokoena");

        SeatOf(cut, "Priya Naidoo").Should().Be("none");
        Note(cut).Should().BeEmpty();
    }

    [Fact]
    public void ANewPanel_WhoseChairWasFirstSeatedAsAMember_IsCreated_AndItsPageSaysSo()
    {
        var cut = RenderNewInstitutionPanel();
        Choose(cut, "Priya Naidoo", "member");
        Choose(cut, "Sipho Mokoena", "member");
        cut.Find("#panel-chair").Change("naidoo");

        Note(cut).Should().Be("Priya Naidoo is the chair now, so is no longer seated as a member.");
        Squash(cut.Find("#panel-seat-count").TextContent).Should().Be("2 seats: the chair, 1 member and 0 external members.");

        cut.Find("form").Submit();

        cut.FindAll(".validation-summary-errors").Should().BeEmpty();
        _sender.Received.OfType<CreateDecisionPanelCommand>().Should().ContainSingle().Which.Members
            .Select(member => $"{member.UserId}:{member.Role}")
            .Should().Equal("naidoo:Chair", "mokoena:Member");
        Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/committee/panels/21?created=1");
    }

    private IRenderedComponent<PanelEdit> RenderPanel()
    {
        var cut = RenderComponent<PanelEdit>(parameters => parameters.Add(page => page.PanelId, PanelId));
        cut.WaitForState(() => cut.FindAll("#panel-chair").Count == 1);
        return cut;
    }

    private IRenderedComponent<PanelEdit> RenderNewInstitutionPanel()
    {
        var cut = RenderComponent<PanelEdit>();
        cut.WaitForState(() => cut.FindAll("#panel-scope").Count == 1);

        cut.Find("#panel-name").Change("Paediatrics CCC");
        cut.Find("#panel-scope-institution").Change("Institution");
        cut.Find("#panel-institution").Change(InstitutionA.ToString());
        return cut;
    }

    private static AngleSharp.Dom.IElement PanelForm(IRenderedComponent<PanelEdit> cut)
        => cut.FindAll("form").Single(form => form.QuerySelector("#panel-chair") is not null);

    private static string Note(IRenderedComponent<PanelEdit> cut) => Squash(cut.Find("#panel-chair-note").TextContent);

    /// <summary>Answers what a test registers, records every request, and refuses anything unregistered.</summary>
    private sealed class RecordingSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, object?>> _answers = [];

        public List<object> Received { get; } = [];

        public RecordingSender On<TRequest>(Func<TRequest, object?> answer)
        {
            _answers[typeof(TRequest)] = request => answer((TRequest)request);
            return this;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            try
            {
                return Task.FromResult((TResponse)Answer(request)!);
            }
            catch (Exception exception)
            {
                return Task.FromException<TResponse>(exception);
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Answer(request);
            return Task.CompletedTask;
        }

        private object? Answer(object request)
        {
            Received.Add(request);

            return _answers.TryGetValue(request.GetType(), out var answer)
                ? answer(request)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
