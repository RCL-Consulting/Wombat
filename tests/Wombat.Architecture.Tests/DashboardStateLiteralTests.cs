using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Wombat.Architecture.Tests;

/// <summary>
/// The forcing function for T297: no query under <c>Features/Dashboards</c> decides what a card shows by comparing an
/// activity's <c>CurrentState</c> with a string literal.
/// </summary>
/// <remarks>
/// <para>
/// A state's key means nothing on its own: each activity type names its own states, and what a state means (finished,
/// waiting on the caller, awaiting a reviewer, a dead end) is in the workflow the activity is PINNED to. A card that reads
/// a key is right for the seeds its author had in mind and wrong for the rest. This class of defect has now come back
/// three times: T074's coordinator card read <c>submitted</c>, T203's cards read <c>completed</c> for "done", and T297
/// found five cards reading <c>requested</c>, <c>accepted</c>, <c>declined</c>, <c>cancelled</c>, <c>draft</c>,
/// <c>submitted</c> and <c>in_review</c>, each disagreeing with the page it linked to on every rated CPSA instrument.
/// Read the workflow instead: <c>ActivityCompletion</c> (finished) and <c>ActivityWaiting</c> (actionable by the caller,
/// awaiting a reviewer).
/// </para>
/// <para>
/// A source scan, comments stripped, because an EF query's comparison is an expression tree whose literal the IL holds
/// only as an argument to <c>Expression.Constant</c>. It catches the shapes a comparison is written in: <c>==</c> and
/// <c>!=</c> either way round, <c>is</c> patterns, <c>Equals</c>, a literal array's <c>Contains</c>, and a
/// <c>switch</c> on the state. A second check catches a key held apart from the comparison (a constant, a field, a
/// renamed projection): no literal outside a comment may be a state key a shipped workflow declares.
/// </para>
/// </remarks>
public sealed class DashboardStateLiteralTests
{
    private const string Literal = "\"(?:[^\"\\\\]|\\\\.)*\"";
    private const string StateMember = @"[\w.?!]*\bCurrentState\b";

    /// <summary>Each shape of "the state key compared with a literal", with what it catches.</summary>
    private static readonly (string Shape, Regex Pattern)[] Comparisons =
    [
        ("CurrentState == \"…\" or != \"…\"", new Regex($@"\bCurrentState\b\s*[!=]=\s*{Literal}")),
        ("\"…\" == CurrentState", new Regex($@"{Literal}\s*[!=]=\s*{StateMember}")),
        ("CurrentState is \"…\" (or not, or a disjunction)", new Regex($@"\bCurrentState\b\s+is\s+(?:not\s+)?\(?\s*{Literal}")),
        ("CurrentState.Equals(\"…\")", new Regex($@"\bCurrentState\b\s*\??\.\s*Equals\s*\(\s*{Literal}")),
        ("Equals(CurrentState, \"…\") either way round", new Regex($@"\bEquals\s*\(\s*(?:{StateMember}\s*,\s*{Literal}|{Literal}\s*,\s*{StateMember})")),
        ("{ \"…\", … }.Contains(CurrentState)", new Regex($@"{Literal}\s*\}}\s*\)?\s*\.\s*Contains\s*\(\s*{StateMember}")),
        ("CurrentState switch { \"…\" => … }", new Regex(@"\bCurrentState\b\s+switch\b")),
        ("switch (CurrentState) { case \"…\": }", new Regex($@"\bswitch\s*\(\s*{StateMember}\s*\)"))
    ];

    [Fact]
    public void No_dashboard_query_compares_an_activitys_state_with_a_literal()
    {
        var violations = DashboardSources()
            .SelectMany(path => Violations(File.ReadAllText(path))
                .Select(violation => $"{Path.GetRelativePath(SolutionRoot(), path)}:{violation.Line}: {violation.Shape}: {violation.Text}"))
            .ToList();

        violations.Should().BeEmpty(
            "a dashboard card must read what a state means from the activity's pinned workflow (ActivityCompletion for " +
            "finished, ActivityWaiting for waiting on the caller or on a reviewer), never from its key: each literal state " +
            "a card read made it disagree with the page it links to. (T297; T074 and T203 before it)");
    }

    /// <summary>
    /// The shapes above are a literal written beside <c>CurrentState</c>. A literal held apart from it is not caught by
    /// them: a <c>const</c> or a <c>static readonly</c> array of states, <c>"requested".Equals(a.CurrentState)</c>, or a
    /// projection that renames the state (the cards already project it into <c>FinalState</c>) and a comparison on the new
    /// name (T297 review). So no string literal outside a comment under <c>Features/Dashboards</c> may be a state key a
    /// shipped workflow declares: a card has no use for one but to compare a state with it.
    /// </summary>
    [Fact]
    public void No_dashboard_source_holds_a_literal_that_is_a_shipped_workflows_state_key()
    {
        var stateKeys = ShippedStateKeys();
        var violations = DashboardSources()
            .SelectMany(path => StateKeyLiterals(File.ReadAllText(path), stateKeys)
                .Select(violation => $"{Path.GetRelativePath(SolutionRoot(), path)}:{violation.Line}: {violation.Text}"))
            .ToList();

        violations.Should().BeEmpty(
            "a string that is a workflow's state key has one use on a dashboard, which is to compare a state with it, " +
            "however far from CurrentState it is written; read the pinned workflow instead (T297 review)");
    }

    /// <summary>The guard for the test above: it must scan the dashboards' queries, and catch each shape it names.</summary>
    [Fact]
    public void The_scan_reads_the_dashboard_queries_and_catches_each_shape()
    {
        DashboardSources().Select(Path.GetFileName).Should().Contain(
            ["GetCoordinatorDashboardSummaryQuery.cs", "GetAssessorDashboardSummaryQuery.cs", "GetTraineeDashboardSummaryQuery.cs",
             "GetSpecialityAdminDashboardSummaryQuery.cs", "GetSubSpecialityAdminDashboardSummaryQuery.cs"]);

        string[] offenders =
        [
            // The four the cards held until T297, as they were written.
            """.Where(a => a.CurrentState == "submitted" && a.UpdatedOn < stallCutoff)""",
            """.Where(a => a.CurrentState is "requested" or "accepted" or "declined" or "draft")""",
            """.Where(a => IsFinished(a) || a.CurrentState is "declined" or "cancelled")""",
            """.Where(a => a.CurrentState == "submitted" || a.CurrentState == "in_review")""",
            """.Where(a => a.CurrentState != "cancelled")""",
            // And the other ways to write one.
            """.Where(a => "requested" == a.CurrentState)""",
            """.Where(a => a.CurrentState is not "draft")""",
            """.Where(a => a.CurrentState.Equals("requested"))""",
            """.Where(a => string.Equals(a.CurrentState, "requested", StringComparison.Ordinal))""",
            """.Where(a => new[] { "requested", "submitted" }.Contains(a.CurrentState))""",
            """var tint = activity.CurrentState switch { "declined" => 1, _ => 0 };""",
            """switch (row.CurrentState) { case "requested": break; }"""
        ];
        foreach (var offender in offenders)
        {
            Violations(offender).Should().NotBeEmpty($"the scan should catch {offender}");
        }

        string[] clean =
        [
            // Reading the pinned workflow, as the cards do since T297, and a literal in a comment.
            """.Where(a => awaiting.Waits(a.ActivityTypeId, a.SchemaVersion, a.CurrentState))""",
            """PinnedWorkflows.StateLabel(workflow, a.CurrentState)""",
            """// Until T297 the card read a.CurrentState == "submitted".""",
            """/* a.CurrentState is "requested" */ var x = 1;""",
            """var due = prop.Name.Contains("due_date", StringComparison.OrdinalIgnoreCase);"""
        ];
        foreach (var source in clean)
        {
            Violations(source).Should().BeEmpty($"{source} compares no state with a literal");
            StateKeyLiterals(source, ShippedStateKeys()).Should().BeEmpty($"{source} holds no state key outside a comment");
        }
    }

    /// <summary>The guard for the state-key check: it reads the shipped workflows' keys, and catches a key held apart.</summary>
    [Fact]
    public void The_state_key_check_reads_the_shipped_workflows_and_catches_a_key_held_apart_from_the_comparison()
    {
        ShippedStateKeys().Should().Contain(["draft", "requested", "submitted", "accepted", "declined", "cancelled", "completed"]);

        string[] heldApart =
        [
            // The four shapes the T297 review found the comparison check blind to.
            """private static readonly string[] Pending = ["submitted", "requested"]; var n = rows.Count(a => Pending.Contains(a.CurrentState));""",
            """private const string Requested = "requested"; var n = rows.Count(a => a.CurrentState == Requested);""",
            """var n = rows.Count(a => "requested".Equals(a.CurrentState));""",
            """var n = rows.Select(a => new { a.Id, FinalState = a.CurrentState }).Count(row => row.FinalState == "declined");""",
            // And the key in a verbatim string.
            """var n = rows.Count(a => a.CurrentState == @"cancelled");"""
        ];
        foreach (var source in heldApart)
        {
            StateKeyLiterals(source, ShippedStateKeys()).Should().NotBeEmpty($"the scan should catch {source}");
        }

        StateKeyLiterals("""// the card read "requested" until T297""", ShippedStateKeys()).Should().BeEmpty("a comment may name a key");
        StateKeyLiterals("""var label = "Requested";""", ShippedStateKeys()).Should().BeEmpty("a key is compared ordinally, as the workflow does");
    }

    private sealed record Violation(int Line, string Shape, string Text);

    private sealed record KeyLiteral(int Line, string Text);

    /// <summary>Each string literal outside a comment whose value is one of <paramref name="stateKeys" />.</summary>
    private static IEnumerable<KeyLiteral> StateKeyLiterals(string source, IReadOnlySet<string> stateKeys)
    {
        var code = StripComments(source);
        foreach (Match match in new Regex(Literal).Matches(code))
        {
            if (stateKeys.Contains(match.Value[1..^1]))
            {
                var line = code[..match.Index].Count(character => character == '\n') + 1;
                yield return new KeyLiteral(line, match.Value);
            }
        }
    }

    /// <summary>Every state key the shipped seed workflows declare (<c>Activities/Seeds/&lt;key&gt;/workflow.json</c>).</summary>
    private static IReadOnlySet<string> ShippedStateKeys()
    {
        var keys = new HashSet<string>(StringComparer.Ordinal);
        var seeds = Path.Combine(SolutionRoot(), "src", "Wombat.Infrastructure", "Activities", "Seeds");
        foreach (var path in Directory.GetFiles(seeds, "workflow.json", SearchOption.AllDirectories))
        {
            using var workflow = JsonDocument.Parse(File.ReadAllText(path));
            foreach (var state in workflow.RootElement.GetProperty("states").EnumerateArray())
            {
                var key = state.ValueKind == JsonValueKind.String ? state.GetString() : state.GetProperty("key").GetString();
                if (!string.IsNullOrEmpty(key))
                {
                    keys.Add(key);
                }
            }
        }

        return keys;
    }

    private static IEnumerable<Violation> Violations(string source)
    {
        var code = StripComments(source);
        foreach (var (shape, pattern) in Comparisons)
        {
            foreach (Match match in pattern.Matches(code))
            {
                var line = code[..match.Index].Count(character => character == '\n') + 1;
                yield return new Violation(line, shape, match.Value.Trim());
            }
        }
    }

    /// <summary>
    /// The source with its comments blanked and every newline kept, so a line number still points at the source. String
    /// literals are kept whole, so a "//" inside one is not taken for a comment.
    /// </summary>
    private static string StripComments(string source)
        => Regex.Replace(
            source,
            $@"{Literal}|//[^\n]*|/\*.*?\*/",
            match => match.Value.StartsWith('"')
                ? match.Value
                : new string(match.Value.Select(character => character == '\n' ? '\n' : ' ').ToArray()),
            RegexOptions.Singleline);

    private static IReadOnlyList<string> DashboardSources()
        => Directory.GetFiles(
                Path.Combine(SolutionRoot(), "src", "Wombat.Application", "Features", "Dashboards"),
                "*.cs",
                SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();

    private static string SolutionRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Wombat.sln")))
            {
                return directory.FullName;
            }
        }

        throw new InvalidOperationException($"No Wombat.sln above {AppContext.BaseDirectory}.");
    }
}
