using FluentAssertions;

namespace Wombat.Web.Tests.Scenario;

/// <summary>
/// T294's rules and their reading of the runbook, proven on a small runbook and app of their own, so each rule is shown
/// holding on a runbook that keeps it and failing on one that breaks it, whatever state the real one is in. The five
/// breaks are the task's mutation checks: a misspelt route, a page whose only step is removed, a stale
/// <c>## Not played</c> row, a template missing from <c>## Pages</c>, and a step without <c>Expect:</c>.
/// </summary>
public sealed class ScenarioRulesTests
{
    private static readonly string[] Pages =
    [
        "/",
        "/activities/inbox",
        "/activities/{ActivityId:int}",
        "/admin/jobs",
        "/placeholder/{Feature}",
    ];

    private static readonly string[] Endpoints = ["/dashboard/switch/{role}", "/account/logout"];

    private static readonly string Act = """
        # Act 1 — Setup

        ## Phase 1.A — The first morning

        ### Step 1.1 — Dr Naidoo opens Dr Dlamini's Mini-CEX
        Role: Assessor — Dr David Naidoo
        Route: / → /activities/inbox (the queue) →
          /activities/{ActivityId:int}?tab=history
        Do: Open the Mini-CEX from the inbox and read its history.
        Expect: The history lists the filing.
          It reads Submitted.
        Actual:
        Gap:

        ### Step 1.2 — Mr Smit switches to his coordinator dashboard and runs the jobs
        Role: Coordinator — Mr Pieter Smit
        Route: /dashboard/switch/{role}, /admin/jobs
        Note: Only a holder of two roles is offered the switch.
        Do: Switch to the coordinator's dashboard, then open the scheduled jobs.
        Expect: The jobs are listed.
        Actual:
        Gap:

        ### Step 1.3 — The reminder job mails the assessors
        Role: System — the draft reminder
        Route: n/a
        Do: Let the reminder run.
        Expect: Each assessor with a draft is emailed once.
        Actual (2026-09-30, T295): Two emails were logged.
        Gap: none

        ## Outcome

        ```sql
        SELECT count(*) FROM "Activities";
        ```
        """.ReplaceLineEndings("\n");

    private static readonly string Coverage = """
        # Coverage

        ## Pages

        | File | Template | Steps |
        |---|---|---|
        | `Pages/Home.razor` | `/` | 1.1 |
        | `Pages/Activities/Inbox.razor` | `/activities/inbox` | 1.1 |
        | `Pages/Activities/Detail.razor` | `/activities/{ActivityId:int}` | 1.1 |
        | `Pages/Admin/Jobs.razor` | `/admin/jobs` | 1.2 |
        | `Pages/Placeholder.razor` | `/placeholder/{Feature}` | — |

        ## Not played

        | Template | Reason |
        |---|---|
        | `/placeholder/{Feature}` | A placeholder: nothing is built behind it. |

        ## Endpoints

        | Endpoint | Steps |
        |---|---|
        | `/dashboard/switch/{role}` | 1.2 |
        """.ReplaceLineEndings("\n");

    // ---- the fixture keeps every rule ----

    [Fact]
    public void TheFixture_KeepsEveryRule()
    {
        var runbook = Parse();

        runbook.Steps.Select(step => step.Id).Should().Equal("1.1", "1.2", "1.3");
        ScenarioRules.UnplayedPages(runbook, Pages).Should().BeEmpty();
        ScenarioRules.UnknownRoutes(runbook, Pages, Endpoints).Should().BeEmpty();
        ScenarioRules.StaleExcuses(runbook, Pages, Endpoints).Should().BeEmpty();
        ScenarioRules.PagesMissingFromTable(runbook, Pages).Should().BeEmpty();
        ScenarioRules.UnknownPagesInTable(runbook, Pages).Should().BeEmpty();
        ScenarioRules.MalformedSteps(runbook).Should().BeEmpty();
    }

    // ---- the five mutation checks ----

    [Fact]
    public void AMisspeltRoute_IsCaught_AndNamesItsStep()
    {
        var runbook = Parse(act: Act.Replace("/activities/inbox (the queue)", "/activities/inbx (the queue)"));

        ScenarioRules.UnknownRoutes(runbook, Pages, Endpoints).Should().ContainSingle()
            .Which.Should().Contain("act-1-setup.md:5 Step 1.1").And.Contain("`/activities/inbx`");
    }

    [Fact]
    public void ARouteSpeltWithTheWrongCase_OrAConcreteId_IsCaught_AndTheTemplateSuggested()
    {
        var runbook = Parse(act: Act
            .Replace("/admin/jobs\n", "/Admin/Jobs\n")
            .Replace("/activities/{ActivityId:int}?tab=history", "/activities/42"));

        ScenarioRules.UnknownRoutes(runbook, Pages, Endpoints).Should().BeEquivalentTo(
        [
            "act-1-setup.md:5 Step 1.1: `/activities/42` is neither a page's @page template nor a mapped endpoint " +
            "(did you mean `/activities/{ActivityId:int}`?)",
            "act-1-setup.md:15 Step 1.2: `/Admin/Jobs` is neither a page's @page template nor a mapped endpoint " +
            "(did you mean `/admin/jobs`?)",
        ]);
    }

    [Fact]
    public void ARouteToAPageThatDoesNotExist_IsNotOfferedAParameterTemplate()
    {
        // Acts 4–5 named /admin/committee-reviews/new before T293. /activities/{ActivityId:int} would pass, and be wrong.
        var runbook = Parse(act: Act.Replace("/activities/{ActivityId:int}?tab=history", "/activities/new"));

        ScenarioRules.UnknownRoutes(runbook, Pages, Endpoints).Should().Equal(
            "act-1-setup.md:5 Step 1.1: `/activities/new` is neither a page's @page template nor a mapped endpoint");
    }

    [Fact]
    public void APageWhoseOnlyStepIsRemoved_IsCaught()
    {
        var runbook = Parse(act: Act.Replace("Route: /dashboard/switch/{role}, /admin/jobs", "Route: /dashboard/switch/{role}"));

        ScenarioRules.UnplayedPages(runbook, Pages).Should().ContainSingle()
            .Which.Should().StartWith("`/admin/jobs` is played by no step");
    }

    [Fact]
    public void AStaleNotPlayedRow_IsCaught_WhenAStepPlaysIt_OrNoPageDeclaresIt_OrItGivesNoReason()
    {
        var runbook = Parse(coverage: Coverage.Replace(
            "| `/placeholder/{Feature}` | A placeholder: nothing is built behind it. |",
            """
            | `/placeholder/{Feature}` | A placeholder: nothing is built behind it. |
            | `/admin/jobs` | Needs a job host. |
            | `/admin/job-runs` | Renamed. |
            | `/account/logout` |  |
            | the SSO callback | No provider. |
            """.ReplaceLineEndings("\n")));

        ScenarioRules.StaleExcuses(runbook, Pages, Endpoints).Should().BeEquivalentTo(
        [
            "coverage.md:21: a `## Not played` row names no template in backticks",
            "coverage.md:18: `## Not played` excuses `/admin/jobs`, but act-1-setup.md:15 Step 1.2 plays it",
            "coverage.md:19: `## Not played` names `/admin/job-runs`, which is neither a page's @page template nor a " +
            "mapped endpoint",
            "coverage.md:20: `## Not played` excuses `/account/logout` with no reason",
        ]);
    }

    [Fact]
    public void ATemplateMissingFromPages_IsCaught()
    {
        var runbook = Parse(coverage: Coverage.Replace("| `Pages/Admin/Jobs.razor` | `/admin/jobs` | 1.2 |\n", string.Empty));

        ScenarioRules.PagesMissingFromTable(runbook, Pages).Should().Equal(
            "`/admin/jobs` is not in coverage.md's `## Pages` table");
    }

    [Fact]
    public void AStepWithoutExpect_IsCaught_AndNamesItsStep()
    {
        var runbook = Parse(act: Act.Replace("Expect: The jobs are listed.\n", string.Empty));

        ScenarioRules.MalformedSteps(runbook).Should().ContainSingle()
            .Which.Should().StartWith("act-1-setup.md:15 Step 1.2: carries Role, Route, Note, Do, Actual, Gap;");
    }

    // ---- the rest of each rule ----

    [Fact]
    public void APagesRowNamingNoPage_IsCaught()
    {
        var runbook = Parse(coverage: Coverage.Replace("`/admin/jobs` | 1.2", "`/admin/jobs/runs` | 1.2"));

        ScenarioRules.UnknownPagesInTable(runbook, Pages).Should().ContainSingle()
            .Which.Should().StartWith("coverage.md:10: `## Pages` names `/admin/jobs/runs`");
        ScenarioRules.PagesMissingFromTable(runbook, Pages).Should().ContainSingle()
            .Which.Should().Contain("`/admin/jobs`");
    }

    [Fact]
    public void WithNoCoverage_TheCoverageRulesSaySo()
    {
        var runbook = ScenarioRunbook.Parse([("act-1-setup.md", Act)], coverage: null);

        ScenarioRules.PagesMissingFromTable(runbook, Pages).Should().Equal("there is no coverage.md, so no `## Pages` table");
        ScenarioRules.UnplayedPages(runbook, Pages).Should().Equal(
            "there is no coverage.md, so no page is excused",
            "`/placeholder/{Feature}` is played by no step's Route line and is not in coverage.md's `## Not played` table");
    }

    [Theory]
    [InlineData("Role Route Do Expect Actual Gap Note", true)]
    [InlineData("Role Route Note Do Expect Actual Gap", true)]
    [InlineData("Role Note Route Do Expect Actual Gap", false)]
    [InlineData("Role Route Note Do Expect Note Actual Gap", false)]
    [InlineData("Role Route Expect Do Actual Gap", false)]
    [InlineData("Role Route Do Expect Actual", false)]
    [InlineData("Role Route Do Expected Actual Gap", false)]
    [InlineData("Role Route Do Expect Expect Actual Gap", false)]
    [InlineData("Route Do Expect Actual Gap", false)]
    public void TheFieldOrder_IsExactly_RoleRouteDoExpectActualGap_WithOneNoteAfterRoute(string fields, bool holds)
    {
        var block = string.Join("\n", fields.Split(' ').Select(field => field == "Route" ? "Route: /" : $"{field}: x"));
        var runbook = ScenarioRunbook.Parse([("act-9.md", $"### Step 9.1 — A step\n{block}\n")], coverage: null);

        runbook.Steps.Single().Fields.Should().Equal(fields.Split(' '));
        ScenarioRules.MalformedSteps(runbook).Should().HaveCount(holds ? 0 : 1);
    }

    [Theory]
    [InlineData("Route:", "Route is empty")]
    [InlineData("Route: the inbox, then the activity", "Route names no page")]
    public void ARouteThatNamesNoPage_MustSayNA(string route, string failure)
    {
        var step = $"### Step 9.1 — A step\nRole: x\n{route}\nDo: x\nExpect: x\nActual:\nGap:\n";
        var runbook = ScenarioRunbook.Parse([("act-9.md", step)], coverage: null);

        ScenarioRules.MalformedSteps(runbook).Should().ContainSingle().Which.Should().Contain(failure);
    }

    [Fact]
    public void AStepUnderTheWrongHeadingLevel_IsCaught()
    {
        var runbook = Parse(act: Act.Replace("### Step 1.3", "#### Step 1.3"));

        ScenarioRules.MalformedSteps(runbook).Should().ContainSingle()
            .Which.Should().Contain("Step 1.3: the heading is level 4");
    }

    // ---- the reading ----

    [Theory]
    [InlineData("/activities/inbox → /activities/{ActivityId:int}", "/activities/inbox|/activities/{ActivityId:int}")]
    [InlineData("/ → /activities/inbox (the queue) → /activities/{ActivityId:int}?tab=history", "/|/activities/inbox|/activities/{ActivityId:int}")]
    [InlineData("/dashboard/switch/{role}, /admin/jobs", "/dashboard/switch/{role}|/admin/jobs")]
    [InlineData("/account/login?returnUrl=/activities/mine → /activities/mine.", "/account/login|/activities/mine")]
    [InlineData("`/admin/jobs` → `/admin/jobs/runs`", "/admin/jobs|/admin/jobs/runs")]
    [InlineData("/admin/audit/{Id:guid} (opened from /admin/audit)", "/admin/audit/{Id:guid}|/admin/audit")]
    [InlineData("/msf/respond#section-2 → /x/{n:range(1,10)}", "/msf/respond|/x/{n:range(1,10)}")]
    [InlineData("/admin/users/{UserId?} → /admin/users", "/admin/users/{UserId?}|/admin/users")]
    [InlineData("/activities/new (the Mini-CEX, and/or a CbD: see https://wombat.local/help)", "/activities/new")]
    [InlineData("n/a", "")]
    [InlineData("n/a (the job runs unattended; its log is at /admin/jobs/runs)", "")]
    [InlineData("N/A", "")]
    public void TheRouteTokens_AreEachPathAsWritten_WithoutQueryPunctuationOrProse(string value, string expected)
    {
        ScenarioRunbook.RouteTokens(value).Should().Equal(
            expected.Length == 0 ? Array.Empty<string>() : expected.Split('|'));
    }

    [Fact]
    public void AStepsRoute_ReadsItsContinuationLines_AndOnlyItsFirstRoute()
    {
        var step = Parse().Steps[0];
        step.Route.Should().Be("/ → /activities/inbox (the queue) → /activities/{ActivityId:int}?tab=history");
        step.RouteTokens.Should().Equal("/", "/activities/inbox", "/activities/{ActivityId:int}");

        var twice = ScenarioRunbook.ParseSteps(
            "act-9.md", "### Step 9.1 — x\nRole: x\nRoute: /a\nDo: x\nRoute: /b\n  /c\nExpect: x\nActual:\nGap:\n").Single();
        twice.RouteTokens.Should().Equal("/a");
        twice.Fields.Should().Equal("Role", "Route", "Do", "Route", "Expect", "Actual", "Gap");
    }

    [Fact]
    public void AStepInFencedCode_IsNotAStep_AndAFenceEndsNoBlock()
    {
        const string text = """
            ```
            ### Step 9.9 — an example, not a step
            Role: x
            ```

            ### Step A.1.2 — a real one
            Role: x
            Route: n/a
            Do: x
            Expect: x
            Actual:
            Gap:
            """;

        var steps = ScenarioRunbook.ParseSteps("appendix-cross-cutting.md", text);

        steps.Should().ContainSingle().Which.Should().Match<ScenarioStep>(step =>
            step.Id == "A.1.2" && step.Line == 6 && step.Level == 3 && step.Route == "n/a" && step.RouteTokens.Count == 0);
    }

    [Fact]
    public void TheCoverageTables_AreReadFromTheirOwnSections_OnlyTheTemplateCell_AndTheirHeadersSkipped()
    {
        var runbook = Parse();

        runbook.Pages.Select(row => row.Template).Should().Equal(Pages);
        runbook.Pages.Select(row => row.Line).Should().Equal(7, 8, 9, 10, 11);
        runbook.NotPlayed.Should().ContainSingle().Which.Should().Be(
            new NotPlayedRow(17, "/placeholder/{Feature}", "A placeholder: nothing is built behind it."));
    }

    [Theory]
    [InlineData("act-1-setup.md", true)]
    [InlineData("appendix-cross-cutting.md", true)]
    [InlineData("README.md", false)]
    [InlineData("coverage.md", false)]
    [InlineData("states.md", false)]
    public void TheStepFiles_AreTheActsAndTheAppendix(string file, bool holdsSteps)
    {
        ScenarioRunbook.IsStepFile(file).Should().Be(holdsSteps);
    }

    /// <summary>The fixture, or a mutation of it. Its text is read with LF line ends, whatever the checkout wrote.</summary>
    private static ScenarioRunbook Parse(string? act = null, string? coverage = null)
        => ScenarioRunbook.Parse([("act-1-setup.md", act ?? Act)], coverage ?? Coverage);
}
