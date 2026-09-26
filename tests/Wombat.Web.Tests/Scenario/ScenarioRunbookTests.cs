using FluentAssertions;
using Wombat.Web.Tests.Navigation;

namespace Wombat.Web.Tests.Scenario;

/// <summary>
/// T294: the scenario runbook (<c>execution/knowledge/scenario-paediatrics/</c>) is tied to the pages the app serves.
/// Every page is played by a step or excused in <c>coverage.md</c>, every route a step names exists, no excuse is stale,
/// <c>coverage.md</c>'s <c>## Pages</c> table names every page, and every step holds the format README.md § "The step
/// format" sets. Before T293 nothing read the runbook: 29 of 68 pages had no step, and Acts 4–5 named routes that did
/// not exist.
/// </summary>
/// <remarks>
/// The rules are <see cref="ScenarioRules" />, proven on fixtures of their own in <see cref="ScenarioRulesTests" />; the
/// parsing is <see cref="ScenarioRunbook" />, and the routes are <see cref="ScenarioRoutes" />. Each test here applies
/// one rule to the real runbook and the real app, after guards that fail it when the reading found nothing to judge.
/// </remarks>
public sealed class ScenarioRunbookTests
{
    private static readonly string Folder =
        Path.Combine(PageAccess.SolutionRoot(), "execution", "knowledge", "scenario-paediatrics");

    private static readonly Lazy<ScenarioRunbook> Runbook = new(() => ScenarioRunbook.Load(Folder));

    private static readonly Lazy<(IReadOnlyList<MappedEndpoint> Endpoints, IReadOnlyList<string> Problems)> Endpoints =
        new(ScenarioRoutes.MappedEndpoints);

    [Fact]
    public void EveryPage_IsPlayedByAStep_OrExcusedInNotPlayed()
    {
        Report(
            "Every @page template must appear in some step's Route line, or in coverage.md's `## Not played` table",
            [.. StepGuards(), .. PageGuards()],
            ScenarioRules.UnplayedPages(Runbook.Value, Templates()));
    }

    [Fact]
    public void EveryRouteAStepNames_IsAPage_OrAMappedEndpoint()
    {
        Report(
            "Every Route token must be a page's @page template or a mapped endpoint, spelled exactly",
            [.. StepGuards(), .. PageGuards(), .. EndpointGuards()],
            ScenarioRules.UnknownRoutes(Runbook.Value, Templates(), EndpointPatterns()));
    }

    [Fact]
    public void NoNotPlayedRow_IsStale()
    {
        Report(
            "Every `## Not played` row must name a real template, with a reason, that no step plays",
            [.. CoverageGuards(needsPages: false), .. PageGuards(), .. EndpointGuards()],
            ScenarioRules.StaleExcuses(Runbook.Value, Templates(), EndpointPatterns()));
    }

    [Fact]
    public void ThePagesTable_NamesEveryPage()
    {
        Report(
            "coverage.md's `## Pages` table must name every @page template",
            PageGuards(),
            ScenarioRules.PagesMissingFromTable(Runbook.Value, Templates()));
    }

    [Fact]
    public void ThePagesTable_NamesNoPageThatDoesNotExist()
    {
        Report(
            "Every template in coverage.md's `## Pages` table must be a page's @page template",
            [.. CoverageGuards(needsPages: true), .. PageGuards()],
            ScenarioRules.UnknownPagesInTable(Runbook.Value, Templates()));
    }

    [Fact]
    public void EveryStep_CarriesRoleRouteDoExpectActualGap_InThatOrder()
    {
        Report(
            "Every `### Step` block must carry Role, Route, Do, Expect, Actual and Gap, in that order (README § \"The step format\")",
            StepGuards(),
            ScenarioRules.MalformedSteps(Runbook.Value));
    }

    [Fact]
    public void TheEndpointScan_ReadsProgramsEndpoints_LiteralsAndConstantsAlike()
    {
        // The scan's own check: a literal, a route with a parameter, and each of the three constants Program.cs maps.
        var (endpoints, problems) = Endpoints.Value;

        problems.Should().BeEmpty("every Map call in Wombat.Web has a pattern the scan can read");
        endpoints.Select(endpoint => endpoint.Pattern).Should().Contain(new[]
        {
            "/health",
            "/account/login/submit",
            "/dashboard/switch/{role}",
            "/account/data-rights/download/{id:guid}",
            Wombat.Web.Security.SessionEnd.Path,
            Wombat.Web.Security.RegisterOutcome.SubmitPath,
            Wombat.Web.Security.ChangePasswordOutcome.SubmitPath,
        });
    }

    private static IReadOnlyList<string> Templates() => ScenarioRoutes.ComponentTemplates();

    private static IReadOnlyList<string> EndpointPatterns()
        => Endpoints.Value.Endpoints.Select(endpoint => endpoint.Pattern).Distinct(StringComparer.Ordinal).ToList();

    /// <summary>The runbook's act and appendix files are there, and each yields steps, or the rules judge nothing.</summary>
    private static IEnumerable<string> StepGuards()
    {
        var runbook = Runbook.Value;
        if (runbook.StepFiles.Count == 0)
        {
            yield return $"guard: no act-*.md or appendix*.md in {Folder}";
        }

        foreach (var file in runbook.StepFiles.Where(file => runbook.Steps.All(step => step.File != file)))
        {
            yield return $"guard: {file} has no `### Step` block";
        }
    }

    /// <summary>
    /// There is a coverage.md, and a <c>## Pages</c> table naming templates when the rule reads it, or a rule that reads
    /// only coverage.md passes on nothing. A missing <c>## Not played</c> section is allowed: every page may be played.
    /// </summary>
    private static IEnumerable<string> CoverageGuards(bool needsPages)
    {
        var runbook = Runbook.Value;
        if (!runbook.HasCoverage)
        {
            yield return $"guard: no {ScenarioRunbook.CoverageFile} in {Folder}";
        }
        else if (needsPages && (!runbook.HasPagesSection || runbook.Pages.Count == 0))
        {
            yield return $"guard: {ScenarioRunbook.CoverageFile} has no `## Pages` table naming a template";
        }
    }

    /// <summary>The reflection finds the pages (68 when T293 counted, 80 templates in 2026-09).</summary>
    private static IEnumerable<string> PageGuards()
    {
        var count = Templates().Count;
        if (count < 60)
        {
            yield return $"guard: reflection found {count} @page templates, fewer than 60";
        }
    }

    /// <summary>The source scan reads every Map call, and finds Program.cs's (11 patterns in 2026-09).</summary>
    private static IEnumerable<string> EndpointGuards()
    {
        var (endpoints, problems) = Endpoints.Value;
        foreach (var problem in problems)
        {
            yield return $"guard: {problem}";
        }

        var count = EndpointPatterns().Count;
        if (count < 10)
        {
            yield return $"guard: the scan found {count} mapped endpoints in {endpoints.Select(e => e.File).Distinct().Count()} files, fewer than 10";
        }
    }

    private static void Report(string rule, IEnumerable<string> guards, IReadOnlyList<string> failures)
    {
        var all = guards.Concat(failures).ToList();
        if (all.Count > 0)
        {
            Assert.Fail($"{rule}. {all.Count} failure(s):{Environment.NewLine}{string.Join(Environment.NewLine, all)}");
        }
    }
}
