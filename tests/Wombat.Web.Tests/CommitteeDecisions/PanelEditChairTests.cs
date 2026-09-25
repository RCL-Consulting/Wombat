using System.Security.Claims;
using System.Text.RegularExpressions;
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
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.CommitteeDecisions;

/// <summary>
/// Choosing as chair someone already selected under Members or External members takes them off that selection and says
/// so, so the save does not send them twice to be refused; choosing another chair puts them back. (T257)
/// </summary>
/// <remarks>
/// The Members and External members pickers leave the chair out, but the selection behind each kept a member chosen
/// before they became the chair, unseen, and the save sent them as the chair and as a member. The sender here runs the
/// commands' own validators, which is where "A panel member is listed more than once." comes from. In Chrome and Edge on
/// Windows each arrow key on a closed select is a change, so every candidate passed on the way down the Chair select is
/// chosen for a moment: each is put back when the next is chosen, or moving through the list emptied the panel.
/// </remarks>
public sealed class PanelEditChairTests : TestContext
{
    private const int InstitutionA = 1;
    private const int InstitutionB = 2;
    private const int PanelId = 20;

    private readonly RecordingSender _sender = new();

    public PanelEditChairTests()
    {
        Services.AddSingleton<IScopedSender>(_sender);
        _sender
            .On<GetInstitutionsListQuery>(_ => new[]
            {
                new InstitutionDto(InstitutionA, "Kgosi Kgari", "KGK", null, true, DateTime.UtcNow),
                new InstitutionDto(InstitutionB, "Other Hospital", "OTH", null, true, DateTime.UtcNow)
            })
            .On<GetSpecialitiesListQuery>(_ => Array.Empty<SpecialityDto>())
            .On<GetDecisionPanelFormOptionsQuery>(_ => new DecisionPanelFormOptionsDto(MayCreateInstitutionWide: true, Specialities: null))
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
                PanelId, "Paediatrics CCC", DecisionPanelScope.Institution, InstitutionA, null,
                [
                    new DecisionPanelMemberDto(1, "zulu", DecisionPanelMemberRole.Chair),
                    new DecisionPanelMemberDto(2, "naidoo", DecisionPanelMemberRole.Member),
                    new DecisionPanelMemberDto(3, "mokoena", DecisionPanelMemberRole.Member),
                    new DecisionPanelMemberDto(4, "botha", DecisionPanelMemberRole.External)
                ]))
            .On<UpdateDecisionPanelCommand>(command =>
            {
                new UpdateDecisionPanelCommandValidator().ValidateAndThrow(command);
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
    public void ChoosingAMemberAsChair_TakesThemOffMembers_SaysSo_AndTheSaveSucceeds()
    {
        var cut = RenderPanel();

        cut.Find("#panel-chair").Change("naidoo");

        Selected(cut, "#panel-members").Should().Equal("mokoena");
        Offered(cut, "#panel-members").Should().NotContain("naidoo").And.Contain("zulu", "the chair before is offered, unselected");
        Note(cut).Should().Be("Priya Naidoo is the chair now, so is no longer selected under Members.");
        cut.Find("#panel-chair").GetAttribute("aria-describedby").Should().Be("panel-chair-note",
            "the Chair select names the line while it says something");
        IdReferences.Broken(cut).Should().BeEmpty();

        PanelForm(cut).Submit();

        cut.FindAll(".alert-danger").Should().BeEmpty("the chair is sent once");
        _sender.Received.OfType<UpdateDecisionPanelCommand>().Should().ContainSingle().Which.Members
            .Select(member => $"{member.UserId}:{member.Role}")
            .Should().Equal("naidoo:Chair", "mokoena:Member", "botha:External");
        Squash(cut.Find(".alert-success").TextContent).Should().Be("Panel members updated.");
        Note(cut).Should().BeEmpty("the change it described is saved, and the result says so");
        cut.Find("#panel-chair").HasAttribute("aria-describedby").Should().BeFalse();

        // Naidoo is the stored chair now, so choosing another does not put them back under Members.
        cut.Find("#panel-chair").Change("mokoena");

        Selected(cut, "#panel-members").Should().BeEmpty();
        Note(cut).Should().Be("Sipho Mokoena is the chair now, so is no longer selected under Members.");
    }

    [Fact]
    public void ChoosingAnExternalMemberAsChair_TakesThemOffExternalMembers_AndTheSaveSucceeds()
    {
        var cut = RenderPanel();

        cut.Find("#panel-chair").Change("botha");

        Selected(cut, "#panel-external").Should().BeEmpty();
        Selected(cut, "#panel-members").Should().Equal("naidoo", "mokoena");
        Note(cut).Should().Be("Anna Botha is the chair now, so is no longer selected under External members.");

        PanelForm(cut).Submit();

        cut.FindAll(".alert-danger").Should().BeEmpty();
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
        cut.Find("#panel-chair").HasAttribute("aria-describedby").Should().BeFalse("it names nothing while the line is empty");

        cut.Find("#panel-chair").Change("naidoo");
        Note(cut).Should().StartWith("Priya Naidoo is the chair now");

        // Back to the stored chair, who is on no other list: Naidoo is put back under Members, and the line says so, since
        // it said a moment ago that they were taken off.
        cut.Find("#panel-chair").Change("zulu");

        Note(cut).Should().Be("Priya Naidoo is selected under Members again.");
        Selected(cut, "#panel-members").Should().Equal("naidoo", "mokoena");

        // A chair on no list, with nobody to put back: nothing to say.
        cut.Find("#panel-chair").Change("dlamini");

        Note(cut).Should().BeEmpty();
        cut.Find("#panel-chair").HasAttribute("aria-describedby").Should().BeFalse();
    }

    [Fact]
    public void ChoosingAMemberAsChair_AndThenTheChairBefore_KeepsEveryoneOnThePanel()
    {
        // The review's scenario: a keyboard user presses Down once (Naidoo) and Up once (back to Zulu). Each is a change.
        var cut = RenderPanel();

        cut.Find("#panel-chair").Change("naidoo");
        cut.Find("#panel-chair").Change("zulu");
        PanelForm(cut).Submit();

        cut.FindAll(".alert-danger").Should().BeEmpty();
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

        Selected(cut, "#panel-members").Should().Equal("naidoo");
        Note(cut).Should().Be(
            "Sipho Mokoena is the chair now, so is no longer selected under Members. Priya Naidoo is selected under Members again.");

        cut.Find("#panel-chair").Change("botha");

        Selected(cut, "#panel-members").Should().Equal("naidoo", "mokoena");
        Selected(cut, "#panel-external").Should().BeEmpty();
        Note(cut).Should().Be(
            "Anna Botha is the chair now, so is no longer selected under External members. Sipho Mokoena is selected under Members again.");

        cut.Find("#panel-chair").Change("dlamini");

        Selected(cut, "#panel-external").Should().Equal("botha");
        Note(cut).Should().Be("Anna Botha is selected under External members again.");

        // Choosing no chair after one who was on no list takes nobody off and puts nobody back.
        cut.Find("#panel-chair").Change(string.Empty);

        Note(cut).Should().BeEmpty();
        Selected(cut, "#panel-members").Should().Equal("naidoo", "mokoena");
        Selected(cut, "#panel-external").Should().Equal("botha");
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
        Selected(cut, "#panel-members").Should().Equal("naidoo", "mokoena");

        // Naidoo is under Members because the panel was read, not because a choice of chair is undone.
        cut.Find("#panel-chair").Change("mokoena");

        Note(cut).Should().Be("Sipho Mokoena is the chair now, so is no longer selected under Members.");
    }

    [Fact]
    public void ChangingTheInstitution_EmptiesTheLine_AndPutsNobodyBackLater()
    {
        var cut = RenderNewInstitutionPanel();
        cut.Find("#panel-members").Change(new[] { "naidoo", "mokoena" });
        cut.Find("#panel-chair").Change("naidoo");
        Note(cut).Should().NotBeEmpty();

        cut.Find("#panel-institution").Change(InstitutionB.ToString());

        Note(cut).Should().BeEmpty("the people it names cannot sit at the institution now chosen");
        cut.Find("#panel-chair").HasAttribute("aria-describedby").Should().BeFalse();

        // Back at the first institution the selections are empty, and choosing a chair puts nobody back.
        cut.Find("#panel-institution").Change(InstitutionA.ToString());
        cut.Find("#panel-chair").Change("mokoena");

        Selected(cut, "#panel-members").Should().BeEmpty();
        Note(cut).Should().BeEmpty();
    }

    [Fact]
    public void ANewPanel_WhoseChairWasFirstChosenAsAMember_IsCreated()
    {
        var cut = RenderNewInstitutionPanel();
        cut.Find("#panel-members").Change(new[] { "naidoo", "mokoena" });
        cut.Find("#panel-chair").Change("naidoo");

        Note(cut).Should().Be("Priya Naidoo is the chair now, so is no longer selected under Members.");

        cut.Find("form").Submit();

        cut.FindAll(".alert-danger").Should().BeEmpty();
        _sender.Received.OfType<CreateDecisionPanelCommand>().Should().ContainSingle().Which.Members
            .Select(member => $"{member.UserId}:{member.Role}")
            .Should().Equal("naidoo:Chair", "mokoena:Member");
        Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/committee/panels/21");
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
        cut.Find("#panel-scope").Change(DecisionPanelScope.Institution.ToString());
        cut.Find("#panel-institution").Change(InstitutionA.ToString());
        return cut;
    }

    private static AngleSharp.Dom.IElement PanelForm(IRenderedComponent<PanelEdit> cut)
        => cut.FindAll("form").Single(form => form.QuerySelector("#panel-chair") is not null);

    private static string Note(IRenderedComponent<PanelEdit> cut) => Squash(cut.Find("#panel-chair-note").TextContent);

    private static IReadOnlyList<string?> Selected(IRenderedComponent<PanelEdit> cut, string selector)
        => cut.Find(selector).QuerySelectorAll("option").Where(option => option.HasAttribute("selected"))
            .Select(option => option.GetAttribute("value")).ToArray();

    private static IReadOnlyList<string?> Offered(IRenderedComponent<PanelEdit> cut, string selector)
        => cut.Find(selector).QuerySelectorAll("option").Select(option => option.GetAttribute("value")).ToArray();

    private static string Squash(string text) => Regex.Replace(text, @"\s+", " ").Trim();

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
