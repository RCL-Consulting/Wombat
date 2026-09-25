using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The programme's MSF coverage (<c>/msf/coverage</c>, T210): per programme, EPA and semester, "n of m trainees covered",
/// and per trainee the count their own progress page gives. Coverage, never a shortfall: no badge, no warning tint, no
/// bar.
/// </summary>
public sealed partial class ProgrammeCoveragePageTests : TestContext
{
    private const string CoordinatorId = "coordinator-1";

    private static readonly MsfProgrammeCoveragePeriodDto Semester1 =
        new(2026, 1, "Semester 1, 2026", "January to June", new DateOnly(2026, 1, 1), new DateOnly(2026, 6, 30), HasEnded: true);

    private static readonly MsfProgrammeCoveragePeriodDto Semester2 =
        new(2026, 2, "Semester 2, 2026", "July to November", new DateOnly(2026, 7, 1), new DateOnly(2026, 12, 31), HasEnded: false);

    private readonly TestAuthorizationContext _auth;
    private readonly Sender _sender = new();

    public ProgrammeCoveragePageTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("coordinator@test");
        Services.AddSingleton<IScopedSender>(_sender);
    }

    [Fact]
    public void EachProgramme_IsASectionNamedByItsCurriculumAndInstitution_CountingTraineesPerSemester()
    {
        SignIn(WombatRoles.Coordinator);
        _sender.Answer = Coverage(Paediatrics());

        var cut = Render();

        var section = cut.Find("section#msf-programme-1-10");
        section.GetAttribute("aria-labelledby").Should().Be("msf-programme-1-10-heading");
        Text(cut.Find("#msf-programme-1-10-heading")).Should().Be("Paediatric EPA Curriculum 11.1 at Demo Institution");
        cut.Find("#msf-programme-1-10-heading").TagName.Should().Be("H2");

        section.QuerySelectorAll(".details-list > div").Select(Text).Should().Equal(
            "Semester 1, 2026 (January to June) 3 trainees, whose programme had started by 30 June 2026",
            "Semester 2, 2026 (July to November) 4 trainees, whose programme had started by 31 December 2026");

        Text(cut.Find("#msf-coverage-intro")).Should().Contain("covering the EPA closed in the semester")
            .And.Contain("counts towards no target").And.NotContain("about the EPA");

        var query = _sender.Asked.Should().ContainSingle().Which.Should().BeOfType<GetMsfProgrammeCoverageQuery>().Subject;
        query.Principal.FindFirst(ClaimTypes.NameIdentifier)!.Value.Should().Be(CoordinatorId, "the handler scopes by the signed-in caller");
        query.AsOf.Should().BeNull("the page reads today");
    }

    [Fact]
    public void TheEpaTable_SaysNOfMTraineesCovered_ByEpaAndSemester_WithTheCoveredInBold()
    {
        SignIn(WombatRoles.Coordinator);
        _sender.Answer = Coverage(Paediatrics());

        var cut = Render();

        var region = cut.Find("[aria-labelledby='msf-programme-1-10-epas msf-programme-1-10-heading']");
        region.GetAttribute("tabindex").Should().Be("0", "a keyboard can scroll it at 390px: it holds nothing focusable");
        region.GetAttribute("role").Should().Be("region");
        Text(cut.Find("#msf-programme-1-10-epas")).Should().Be("By EPA");

        var table = region.QuerySelector("table")!;
        table.ClassList.Should().Contain(["clinic-table", "clinic-table--compact"]);
        Text(table.QuerySelector("caption")!).Should().Be(MsfProgrammeCoverageText.EpaCaption);
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal("EPA", "Semester 1, 2026", "Semester 2, 2026");

        var paed001 = cut.Find("#msf-programme-1-10-epa-101");
        Text(paed001.QuerySelector("th[scope='row']")!).Should().Be("PAED-001 Assess and manage the unwell child");
        paed001.QuerySelectorAll("td").Select(Text).Should().Equal("1 of 3 trainees covered", "1 of 4 trainees covered");
        paed001.QuerySelectorAll("td strong").Should().HaveCount(2);

        var local = cut.Find("#msf-programme-1-10-epa-130");
        Text(local.QuerySelector("th")!).Should().EndWith("The institution's own EPA");
        local.QuerySelectorAll("td").Select(Text).Should().Equal("1 of 1 trainee covered", "0 of 4 trainees covered");
        local.QuerySelectorAll("td")[1].QuerySelector("span.muted").Should().NotBeNull("an uncovered count is muted, not a warning");

        cut.FindAll(".badge, .alert-warning, [role='progressbar'], .progress-bar").Should().BeEmpty(
            "coverage is not a verdict against a target, and there is no shortfall to show");
    }

    [Fact]
    public void TheTraineeTable_GivesEachTraineesOwnCount_OrNotStarted()
    {
        SignIn(WombatRoles.Coordinator);
        _sender.Answer = Coverage(Paediatrics());

        var cut = Render();

        var region = cut.Find("[aria-labelledby='msf-programme-1-10-trainees msf-programme-1-10-heading']");
        Text(region.QuerySelector("caption")!).Should().Be(MsfProgrammeCoverageText.TraineeCaption);
        region.QuerySelectorAll("tbody tr").Select(row => row.QuerySelectorAll("th, td").Select(Text).ToArray())
            .Should().BeEquivalentTo(
                new[]
                {
                    new[] { "Ada Adams", "2 of 2 EPAs covered", "0 of 2 EPAs covered" },
                    new[] { "Dan Dube", "Not started", "1 of 2 EPAs covered" }
                },
                options => options.WithStrictOrdering());
        region.QuerySelectorAll("tbody th[scope='row']").Should().HaveCount(2);
    }

    [Fact]
    public void WithSeveralProgrammes_EachTableRegion_IsNamedByItsTableAndItsProgramme()
    {
        SignIn(WombatRoles.Administrator);
        _sender.Answer = Coverage(
            Paediatrics(),
            Paediatrics() with { InstitutionId = 2, InstitutionName = "Other Hospital" });

        var cut = Render();

        // A region's accessible name is the text of the elements its aria-labelledby names, in order.
        var names = cut.FindAll("[role='region']")
            .Select(region => string.Join(
                " ",
                region.GetAttribute("aria-labelledby")!.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => Text(cut.Find($"#{id}")))))
            .ToList();

        names.Should().Equal(
            "By EPA Paediatric EPA Curriculum 11.1 at Demo Institution",
            "By trainee Paediatric EPA Curriculum 11.1 at Demo Institution",
            "By EPA Paediatric EPA Curriculum 11.1 at Other Hospital",
            "By trainee Paediatric EPA Curriculum 11.1 at Other Hospital");
    }

    [Fact]
    public void ASemesterNoTraineeHadStartedIn_SaysSo_RatherThanZeroOfZero()
    {
        SignIn(WombatRoles.Coordinator);
        var programme = Paediatrics() with
        {
            Periods = [new MsfProgrammePeriodDto(2026, 1, 0), new MsfProgrammePeriodDto(2026, 2, 1)],
            Epas = [Epa(101, "PAED-001", false, (0, 0), (1, 1))]
        };
        _sender.Answer = Coverage(programme);

        var cut = Render();

        cut.Find("#msf-programme-1-10-epa-101").QuerySelectorAll("td").Select(Text).Should().Equal(
            "No trainee had started", "1 of 1 trainee covered");
        cut.Find("section .details-list > div").TextContent.Should().Contain("No trainee on the programme had started by 30 June 2026");
    }

    [Fact]
    public void WithNoTraineeToShow_ThePageSaysSo()
    {
        SignIn(WombatRoles.Coordinator);
        _sender.Answer = Coverage();

        var cut = Render();

        Text(cut.Find(".detail-card--empty")).Should().Contain(MsfProgrammeCoverageText.EmptyTitle).And.Contain("not counted");
        cut.FindAll("section").Should().BeEmpty();
    }

    [Fact]
    public void AFailedRead_IsTheStatePanelsError()
    {
        SignIn(WombatRoles.Coordinator);
        _sender.Failure = new InvalidOperationException("The coverage could not be read.");

        var cut = Render();

        var alert = cut.Find(".alert.alert-danger");
        alert.GetAttribute("role").Should().Be("alert");
        Text(alert).Should().Be("The coverage could not be read.");
    }

    [Theory]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.Administrator)]
    public void SomeoneWhoHoldsTrainee_IsToldWhy_AndNothingIsAsked(string role)
    {
        SignIn(role, WombatRoles.Trainee);

        var cut = Render();

        var notice = cut.Find("#msf-coverage-trainee");
        notice.ClassList.Should().Contain("alert-warning");
        notice.HasAttribute("role").Should().BeFalse("standing content, there on every visit, is announced by no role");
        Text(notice).Should().Be(MsfProgrammeCoverageText.TraineeSeesNoProgramme);
        cut.FindAll("section, .detail-card--empty").Should().BeEmpty();
        _sender.Asked.Should().BeEmpty("the handler would answer with no programme");
    }

    [Fact]
    public void ThePage_AdmitsCoordinatorsAndAdministrators_AsTheCampaignPagesDo()
    {
        var attribute = typeof(ProgrammeCoverage).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Should().ContainSingle().Subject;
        attribute.Roles.Should().Be("Coordinator,Administrator");

        var campaigns = typeof(CampaignsList).GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), inherit: true)
            .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .Single();
        attribute.Roles.Should().Be(campaigns.Roles, "the page is reached from the campaign list");
    }

    // ─── Fixture ─────────────────────────────────────────────────────────────

    private IRenderedComponent<ProgrammeCoverage> Render()
    {
        var cut = RenderComponent<ProgrammeCoverage>();
        cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0);
        return cut;
    }

    private void SignIn(params string[] roles)
    {
        _auth.SetRoles(roles);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, CoordinatorId), new Claim(WombatClaimTypes.InstitutionId, "1"));
    }

    private static MsfProgrammeCoverageDto Coverage(params MsfProgrammeDto[] programmes)
        => new(new DateOnly(2026, 9, 1), [Semester1, Semester2], programmes);

    private static MsfProgrammeDto Paediatrics()
        => new(
            1,
            "Demo Institution",
            10,
            "Paediatric EPA Curriculum",
            "11.1",
            [new MsfProgrammePeriodDto(2026, 1, 3), new MsfProgrammePeriodDto(2026, 2, 4)],
            [
                Epa(101, "PAED-001", false, (1, 3), (1, 4)),
                Epa(130, "HOST-030", true, (1, 1), (0, 4))
            ],
            [
                new MsfProgrammeTraineeDto("ada", "Ada Adams", new DateOnly(2025, 1, 1),
                    [new MsfProgrammeTraineePeriodDto(2026, 1, true, 2), new MsfProgrammeTraineePeriodDto(2026, 2, true, 0)]),
                new MsfProgrammeTraineeDto("dan", "Dan Dube", new DateOnly(2026, 8, 1),
                    [new MsfProgrammeTraineePeriodDto(2026, 1, false, 0), new MsfProgrammeTraineePeriodDto(2026, 2, true, 1)])
            ]);

    private static MsfProgrammeEpaDto Epa(
        int itemId, string code, bool isLocal, (int Covered, int Trainees) first, (int Covered, int Trainees) second)
        => new(
            itemId,
            itemId - 100,
            code,
            code == "PAED-001" ? "Assess and manage the unwell child" : $"{code} title",
            isLocal,
            [
                new MsfProgrammeEpaPeriodDto(2026, 1, first.Covered, first.Trainees),
                new MsfProgrammeEpaPeriodDto(2026, 2, second.Covered, second.Trainees)
            ]);

    private static string Text(IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private sealed class Sender : IScopedSender
    {
        public List<object> Asked { get; } = [];

        public MsfProgrammeCoverageDto? Answer { get; set; }

        public Exception? Failure { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Asked.Add(request);
            if (request is not GetMsfProgrammeCoverageQuery)
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            return Failure is not null
                ? Task.FromException<TResponse>(Failure)
                : Task.FromResult((TResponse)(object)Answer!);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
