namespace Wombat.Web.Tests.Scenario;

/// <summary>
/// T294's rules for the scenario runbook, each returning one failure per breach, naming the step, the coverage row or the
/// template. Empty means the rule holds. They take the routes as arguments, so the fixture tests prove each rule on a
/// runbook and an app of their own (<c>ScenarioRulesTests</c>) and the corpus tests apply them to the real ones
/// (<c>ScenarioRunbookTests</c>).
/// </summary>
internal static class ScenarioRules
{
    /// <summary>The fields every step carries, in this order. <c>Note</c> may be added once, anywhere after Route.</summary>
    public static readonly IReadOnlyList<string> RequiredFields = ["Role", "Route", "Do", "Expect", "Actual", "Gap"];

    public const string NoteField = "Note";

    /// <summary>
    /// (a) Every page is played or excused: each template is a route of some step, or a row of <c>## Not played</c>.
    /// </summary>
    public static IReadOnlyList<string> UnplayedPages(ScenarioRunbook runbook, IEnumerable<string> templates)
    {
        var played = Played(runbook);
        var excused = runbook.NotPlayed.Select(row => row.Template).ToHashSet(StringComparer.Ordinal);

        var failures = new List<string>();
        if (!runbook.HasCoverage)
        {
            failures.Add($"there is no {ScenarioRunbook.CoverageFile}, so no page is excused");
        }

        failures.AddRange(templates
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Where(template => !played.ContainsKey(template) && !excused.Contains(template))
            .Select(template => $"`{template}` is played by no step's Route line and is not in " +
                                $"{ScenarioRunbook.CoverageFile}'s `## Not played` table"));
        return failures;
    }

    /// <summary>
    /// (b) Every route a step names exists: each Route token is a component template or a mapped endpoint, spelled
    /// exactly, case included.
    /// </summary>
    public static IReadOnlyList<string> UnknownRoutes(
        ScenarioRunbook runbook, IEnumerable<string> templates, IEnumerable<string> endpoints)
    {
        var known = templates.Concat(endpoints).ToHashSet(StringComparer.Ordinal);

        return runbook.Steps
            .SelectMany(step => step.RouteTokens
                .Where(token => !known.Contains(token))
                .Select(token => $"{step.Where}: `{token}` is neither a page's @page template nor a mapped endpoint" +
                                 Suggestion(token, known)))
            .ToList();
    }

    /// <summary>
    /// (c) No stale excuse: each <c>## Not played</c> row names a real template (a page's, or a mapped endpoint's), gives
    /// a reason, and names a template no step plays.
    /// </summary>
    public static IReadOnlyList<string> StaleExcuses(
        ScenarioRunbook runbook, IEnumerable<string> templates, IEnumerable<string> endpoints)
    {
        var known = templates.Concat(endpoints).ToHashSet(StringComparer.Ordinal);
        var played = Played(runbook);
        var coverage = ScenarioRunbook.CoverageFile;

        var failures = new List<string>();
        failures.AddRange(runbook.NotPlayedRowsWithoutTemplate
            .Select(line => $"{coverage}:{line}: a `## Not played` row names no template in backticks"));

        foreach (var row in runbook.NotPlayed)
        {
            if (!known.Contains(row.Template))
            {
                failures.Add($"{coverage}:{row.Line}: `## Not played` names `{row.Template}`, which is neither a page's " +
                             "@page template nor a mapped endpoint" + Suggestion(row.Template, known));
            }

            if (played.TryGetValue(row.Template, out var steps))
            {
                failures.Add($"{coverage}:{row.Line}: `## Not played` excuses `{row.Template}`, but " +
                             $"{string.Join(", ", steps.Select(step => step.Where))} plays it");
            }

            if (string.IsNullOrWhiteSpace(row.Reason))
            {
                failures.Add($"{coverage}:{row.Line}: `## Not played` excuses `{row.Template}` with no reason");
            }
        }

        return failures;
    }

    /// <summary>(d) <c>coverage.md</c>'s <c>## Pages</c> table names every template.</summary>
    public static IReadOnlyList<string> PagesMissingFromTable(ScenarioRunbook runbook, IEnumerable<string> templates)
    {
        if (!runbook.HasCoverage)
        {
            return [$"there is no {ScenarioRunbook.CoverageFile}, so no `## Pages` table"];
        }

        if (!runbook.HasPagesSection)
        {
            return [$"{ScenarioRunbook.CoverageFile} has no `## Pages` section"];
        }

        var listed = runbook.Pages.Select(row => row.Template).ToHashSet(StringComparer.Ordinal);
        return templates
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .Where(template => !listed.Contains(template))
            .Select(template => $"`{template}` is not in {ScenarioRunbook.CoverageFile}'s `## Pages` table")
            .ToList();
    }

    /// <summary>(d, the other way) Every template the <c>## Pages</c> table names is a page's.</summary>
    public static IReadOnlyList<string> UnknownPagesInTable(ScenarioRunbook runbook, IEnumerable<string> templates)
    {
        var known = templates.ToHashSet(StringComparer.Ordinal);
        return runbook.Pages
            .Where(row => !known.Contains(row.Template))
            .Select(row => $"{ScenarioRunbook.CoverageFile}:{row.Line}: `## Pages` names `{row.Template}`, which no " +
                           "page's @page directive declares" + Suggestion(row.Template, known))
            .ToList();
    }

    /// <summary>
    /// (e) The step format holds: each step is a <c>###</c> heading carrying exactly <see cref="RequiredFields" /> in
    /// that order, with at most one <c>Note</c>, after Route; and its Route names a page, or says <c>n/a</c>.
    /// </summary>
    public static IReadOnlyList<string> MalformedSteps(ScenarioRunbook runbook)
    {
        var failures = new List<string>();
        foreach (var step in runbook.Steps)
        {
            if (step.Level != 3)
            {
                failures.Add($"{step.Where}: the heading is level {step.Level}; a step is `### Step {step.Id} — …`");
            }

            var withoutNote = step.Fields.Where(field => field != NoteField).ToList();
            var notes = step.Fields.Select((field, index) => (field, index)).Where(f => f.field == NoteField).ToList();
            var route = step.Fields.ToList().IndexOf("Route");

            if (!withoutNote.SequenceEqual(RequiredFields, StringComparer.Ordinal) ||
                notes.Count > 1 ||
                notes.Any(note => route < 0 || note.index < route))
            {
                failures.Add($"{step.Where}: carries {(step.Fields.Count == 0 ? "no fields" : string.Join(", ", step.Fields))}; " +
                             $"a step carries {string.Join(", ", RequiredFields)}, in that order, with one optional Note " +
                             "after Route");
            }

            if (step.Route is null)
            {
                continue;
            }

            if (step.Route.Trim().Length == 0)
            {
                failures.Add($"{step.Where}: Route is empty; a step that visits no page writes `n/a`");
            }
            else if (!ScenarioRunbook.IsNotApplicable(step.Route) && step.RouteTokens.Count == 0)
            {
                failures.Add($"{step.Where}: Route names no page (`{step.Route}`); write each page as its @page template, " +
                             "or `n/a`");
            }
        }

        return failures;
    }

    /// <summary>Each template a step's Route names, with the steps that name it.</summary>
    private static Dictionary<string, List<ScenarioStep>> Played(ScenarioRunbook runbook)
    {
        var played = new Dictionary<string, List<ScenarioStep>>(StringComparer.Ordinal);
        foreach (var step in runbook.Steps)
        {
            foreach (var token in step.RouteTokens.Distinct(StringComparer.Ordinal))
            {
                if (!played.TryGetValue(token, out var steps))
                {
                    played[token] = steps = [];
                }

                steps.Add(step);
            }
        }

        return played;
    }

    /// <summary>
    /// The known route a misspelt one probably meant: the same but for case, or the same segments with a parameter
    /// written differently (<c>/committee/reviews/{Id:int}</c>) or given a value (<c>/committee/reviews/12</c>). A word
    /// is not a parameter's value: <c>/committee/reviews/new</c> is a page that does not exist, not a review, so it is
    /// offered nothing rather than a template that would pass and be wrong.
    /// </summary>
    private static string Suggestion(string route, IReadOnlySet<string> known)
    {
        var segments = route.Trim('/').Split('/');
        var match = known.Order(StringComparer.Ordinal).FirstOrDefault(candidate =>
        {
            var want = candidate.Trim('/').Split('/');
            return want.Length == segments.Length &&
                   want.Zip(segments).All(pair => (pair.First.StartsWith('{') && IsParameterValue(pair.Second)) ||
                                                  string.Equals(pair.First, pair.Second, StringComparison.OrdinalIgnoreCase));
        });

        return match is null ? string.Empty : $" (did you mean `{match}`?)";
    }

    private static bool IsParameterValue(string segment)
        => segment.StartsWith('{') ||
           (segment.Length > 0 && segment.All(char.IsAsciiDigit)) ||
           Guid.TryParse(segment, out _);
}
