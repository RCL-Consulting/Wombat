using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.EntrustmentDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.EntrustmentDecisions;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Domain.Scheduling;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T266: every class a page names is one a stylesheet defines. Until T266 eight pages wore <c>badge-success</c>,
/// <c>-danger</c>, <c>-warning</c>, <c>-info</c> and <c>-primary</c>, which app.css does not define, NavMenu carried
/// Bootstrap's <c>ps-3</c>, <c>navbar</c>, <c>navbar-dark</c>, <c>container-fluid</c> and <c>px-3</c>, which nothing
/// loads, and three list pages laid their filters out with <c>filter-bar</c> and <c>form-row</c>, which nothing styles.
/// A class nothing defines styles nothing, silently: the page renders, untinted and unlaid-out.
/// </summary>
/// <remarks>
/// The scan reads every <c>class="…"</c> in every <c>.razor</c> file as Razor writes it: the words written out, the
/// string literals an explicit expression <c>@(…)</c> can put there (a ternary's two classes), and a word glued to an
/// expression (<c>alert-@Kind</c>, <c>$"badge-{key}"</c>) as the start of a class. A class is defined when app.css has a
/// selector naming it, or the component's own scoped stylesheet does (<c>NavMenu.razor.css</c> for <c>NavMenu.razor</c>),
/// or it is one the framework reads itself (<see cref="FrameworkClasses" />). The start of a class is never defined by
/// what it starts: <c>badge-</c> glued to a state key is the untinted <c>badge-signed_off</c> T220 found, and nothing the
/// scan can read says which keys the expression will write. It passes only when it is a whole class itself (RungPicker's
/// <c>form-select{InvalidClass}</c>, whose hole adds <c> invalid</c> or nothing), or where a test of its own holds every
/// value the expression can take (<see cref="GluedPrefixes" />). A class a method returns is out of the scan's sight, so
/// every badge class is named in one place, <see cref="BadgeFor" />, and held to app.css below.
/// </remarks>
public sealed partial class DefinedClassTests
{
    /// <summary>
    /// Classes that are not styling but the framework's own, which no stylesheet need define: blazor.web.js wires the error
    /// bar's Reload button by <c>.reload</c> and its Dismiss button by <c>.dismiss</c> (T335 flow 01; ErrorBarTests holds
    /// that nothing styles either). Nothing else: a class the framework puts on at run time (<c>NavLink</c>'s
    /// <c>active</c>, an input's <c>invalid</c>) is not in a page's markup, so the scan never meets it, and a page that
    /// wrote one itself would be styling by it. The scan fails on an entry it did not need (T266 review).
    /// </summary>
    private static readonly HashSet<string> FrameworkClasses = new(StringComparer.Ordinal) { "reload", "dismiss" };

    /// <summary>
    /// The start of a class an expression finishes, allowed in the one file that writes it, because a test holds every
    /// value the expression takes: <c>Alert.razor</c>'s <c>alert-@Kind</c>, by <see cref="EveryAlertKind_IsOneAppCssDefines" />.
    /// The scan fails on an entry it did not need.
    /// </summary>
    private static readonly HashSet<(string File, string Prefix)> GluedPrefixes = [("Alert.razor", "alert-")];

    [Fact]
    public void EveryClassAPageNames_IsOneAStylesheetDefines()
    {
        var appCss = DefinedIn(File.ReadAllText(WebFile("wwwroot", "app.css")));
        var web = WebFile();
        var files = Directory.EnumerateFiles(web, "*.razor", SearchOption.AllDirectories).ToList();
        var scanned = 0;
        var undefined = new List<string>();
        var allowancesUsed = new HashSet<string>(StringComparer.Ordinal);

        foreach (var path in files)
        {
            var scoped = path + ".css";
            var defined = File.Exists(scoped) ? appCss.Union(DefinedIn(File.ReadAllText(scoped))).ToHashSet() : appCss;
            var file = Path.GetFileName(path);
            foreach (var use in ClassesNamedIn(File.ReadAllText(path)))
            {
                scanned++;
                switch (Judge(use, defined, file))
                {
                    case Verdict.Undefined:
                        undefined.Add($"{Path.GetRelativePath(web, path)}: {use}");
                        break;
                    case Verdict.Framework:
                        allowancesUsed.Add(use.Name);
                        break;
                    case Verdict.GluedPrefix:
                        allowancesUsed.Add($"{file}: {use.Name}");
                        break;
                }
            }
        }

        files.Should().HaveCountGreaterThanOrEqualTo(100, "guard: the scan finds the pages (110), or it proves nothing");
        scanned.Should().BeGreaterThanOrEqualTo(1500, "guard: the scan reads the pages' classes (about 1,780), or it proves nothing");
        string.Join(Environment.NewLine, undefined).Should().BeEmpty(
            "a class no stylesheet defines styles nothing (DESIGN.md § Non-negotiables); every one is listed");
        allowancesUsed.Should().BeEquivalentTo(
            FrameworkClasses.Concat(GluedPrefixes.Select(entry => $"{entry.File}: {entry.Prefix}")),
            "an allowance the scan does not need is one a page could hide an undefined class behind");
    }

    [Fact]
    public void EveryAlertKind_IsOneAppCssDefines()
    {
        // Alert renders class="alert alert-@Kind", so a Kind is half a class name, and the class scan lets Alert.razor's
        // alert- through on this test's word (GluedPrefixes). A Kind written out, and each a ternary can give
        // (Kind="@(ok ? "success" : "danger")"), is read here. An implicit one (Kind="@_notice.Kind") is out of sight.
        var appCss = DefinedIn(File.ReadAllText(WebFile("wwwroot", "app.css")));
        var kinds = Directory.EnumerateFiles(WebFile(), "*.razor", SearchOption.AllDirectories)
            .SelectMany(path => AlertKindsIn(File.ReadAllText(path)).Select(kind => (File: Path.GetFileName(path), Kind: kind)))
            .ToList();

        kinds.Should().HaveCountGreaterThanOrEqualTo(100, "guard: the scan finds the pages' alerts (114), or it proves nothing");
        kinds.Should().Contain(("ScheduledJobsList.razor", "success"), "guard: a Kind a ternary gives is read");
        kinds.Where(alert => !appCss.Contains($"alert-{alert.Kind}"))
            .Select(alert => $"{alert.File}: Kind=\"{alert.Kind}\"")
            .Should().BeEmpty();

        AlertKindsIn("""<Alert Id="x" Kind="warning" Role="">""").Should().Equal("warning");
        AlertKindsIn("""<Alert Kind="@(ok ? "success" : "primary")">""").Should().Equal("success", "primary");
        AlertKindsIn("""<Alert Kind="@_notice.Kind">""").Should().BeEmpty();
    }

    [Fact]
    public void TheScan_ReadsAClassAttribute_AsRazorWritesIt()
    {
        // The scan's own rules, shown on markup whose answers are known, so a scan that read nothing would not pass.
        const string markup = """
            <div class="card  wide">
            <b class="btn @(active ? "is-active" : "is-idle") @(state == "draft" ? "x" : null)">
            <i class="badge-@key icon @Css">
            <p class="@($"form-select{Invalid}") trailing">
            <s class="@($"badge badge-{item.CurrentState}")">
            <a class="@@literal">
            @* <div class="commented-out"> *@
            <!-- <div class="html-comment"> -->
            <Icon Class="named" />
            """;

        ClassesNamedIn(markup).Select(use => use.ToString()).Should().Equal(
            "card", "wide",
            "btn", "is-active", "is-idle", "x",
            "badge-…", "icon",
            "form-select…", "trailing",
            "badge", "badge-…",
            "@literal",
            "named");

        var defined = new HashSet<string>(StringComparer.Ordinal) { "card", "badge", "badge-draft", "form-select" };
        Judge(new ClassUse("card", IsPrefix: false), defined, "Page.razor").Should().Be(Verdict.Defined);
        Judge(new ClassUse("wide", IsPrefix: false), defined, "Page.razor").Should().Be(Verdict.Undefined, "app.css does not define it");
        Judge(new ClassUse("form-select", IsPrefix: true), defined, "Page.razor").Should().Be(Verdict.Defined,
            "a whole class, which the hole leaves whole or follows with another");
        Judge(new ClassUse("badge-", IsPrefix: true), defined, "Page.razor").Should().Be(Verdict.Undefined,
            "a defined class starting with it says nothing of the classes the expression will finish (T220's badge-signed_off)");
        Judge(new ClassUse("alert-", IsPrefix: true), defined, "Page.razor").Should().Be(Verdict.Undefined);
        Judge(new ClassUse("alert-", IsPrefix: true), defined, "Alert.razor").Should().Be(Verdict.GluedPrefix,
            "the one file whose expression a test holds to app.css");
        Judge(new ClassUse("reload", IsPrefix: false), defined, "Page.razor").Should().Be(Verdict.Framework, "the framework's own");
        Judge(new ClassUse("dismiss", IsPrefix: false), defined, "Page.razor").Should().Be(Verdict.Framework, "the framework's own");
        Judge(new ClassUse("active", IsPrefix: false), defined, "Page.razor").Should().Be(Verdict.Undefined,
            "a page that writes it is styling by it");

        DefinedIn(".a, .b:hover > .c-d { } @container x (width < 4rem) { .e .f::before {} } .g.h[aria-x] {}")
            .Should().BeEquivalentTo(["a", "b", "c-d", "e", "f", "g", "h"]);
    }

    // ---- badges ----

    [Fact]
    public void EveryBadgeBadgeForNames_IsOneAppCssDefines()
    {
        var appCss = DefinedIn(File.ReadAllText(WebFile("wwwroot", "app.css")));
        var states = Enum.GetValues<BadgeState>().Select(BadgeFor.State).ToList();
        states.Should().OnlyHaveUniqueItems("each of the five is its own tint").And.HaveCount(5);

        var classes = states
            .Concat(SeededStates().Select(state => state.Key).Append("an_institutions_own_state").Append(null)
                .SelectMany(key => new[] { BadgeFor.ActivityState(key, isFinished: false), BadgeFor.ActivityState(key, isFinished: true) }))
            .Concat(Enum.GetValues<CommitteeAgendaLineStatus>().Select(BadgeFor.AgendaLine))
            .Concat(Enum.GetValues<CommitteeAgendaElsewhereStatus>().Select(BadgeFor.AgendaElsewhere))
            .Concat(Enum.GetValues<EntrustmentDecisionDueStatus>().Select(BadgeFor.DecisionDue))
            .Concat(Enum.GetValues<MsfCampaignState>().Select(BadgeFor.MsfCampaign))
            .Concat(Enum.GetValues<EntrustmentDecisionStatus>().Select(BadgeFor.EntrustmentDecision))
            .Concat(Enum.GetValues<DataRightsRequestStatus>().Select(BadgeFor.DataRightsRequest))
            .Concat(Enum.GetNames<ScheduledJobRunStatus>().Append("Skipped").Append(null).Select(BadgeFor.JobRun))
            .Concat([BadgeFor.AuditResult(true), BadgeFor.AuditResult(false)])
            .Concat(Enum.GetValues<EntrustmentStandingStatus>().Select(BadgeFor.Standing))
            // T350 (note 14): Overdue, worded beside a waiting activity's state badge.
            .Append(BadgeFor.Overdue)
            .Distinct()
            .ToList();

        classes.Where(name => !appCss.Contains(name)).Should().BeEmpty("a badge class app.css does not define is an untinted pill");
    }

    [Fact]
    public void AnActivityStatesBadge_IsGreenWhenItsPinnedWorkflowIsFinished_NotByTheKeysName()
    {
        // The dashboards badged a state as badge-{key}, tinted for draft, submitted, accepted, completed and declined
        // only: Signed off, Requested, Verified and eight more were untinted pills (T266). Done is a terminal state of the
        // pinned workflow (D44), which the key does not say: teaching_session finishes in "accepted", which on a Mini-CEX
        // is a supervisor's work in hand, and was amber on the assessor's Recent decisions (T266 review).
        BadgeFor.ActivityState("accepted", isFinished: true).Should().Be("badge-completed", "a teaching session ends there");
        BadgeFor.ActivityState("accepted", isFinished: false).Should().Be("badge-accepted", "a Mini-CEX in hand");
        BadgeFor.ActivityState("closed", isFinished: true).Should().Be("badge-completed", "an institution's own last state");
        BadgeFor.ActivityState("completed", isFinished: false).Should().Be("badge-draft", "a key's name does not make it done");
        BadgeFor.ActivityState("draft", isFinished: false).Should().Be("badge-draft");
        BadgeFor.ActivityState("requested", isFinished: false).Should().Be("badge-submitted", "it waits on the assessor");
        BadgeFor.ActivityState("submitted", isFinished: false).Should().Be("badge-submitted");
        foreach (var refused in new[] { "declined", "rejected", "cancelled" })
        {
            BadgeFor.ActivityState(refused, isFinished: false).Should().Be("badge-declined", $"{refused} ended without the outcome");
            BadgeFor.ActivityState(refused, isFinished: true).Should().Be("badge-declined", "even where a version makes it terminal");
        }

        BadgeFor.ActivityState("awaiting_panel", isFinished: false).Should().Be("badge-draft", "grey claims nothing; the label says the state");
        BadgeFor.ActivityState(null, isFinished: false).Should().Be("badge-draft");

        // Every seeded state, as the dashboards badge it: each finished one green, and no state but a draft grey.
        var seeded = SeededStates();
        seeded.Should().HaveCountGreaterThanOrEqualTo(15, "guard: the scan finds the seeds' states");
        seeded.Where(state => state.Terminal)
            .Select(state => (state.Key, Badge: BadgeFor.ActivityState(state.Key, isFinished: true)))
            .Should().OnlyContain(state => state.Badge == "badge-completed", "every seed's last state is done")
            .And.Contain(("accepted", "badge-completed"), "guard: teaching_session's");
        seeded.Where(state => !state.Terminal && state.Key != "draft")
            .Select(state => (state.Key, Badge: BadgeFor.ActivityState(state.Key, isFinished: false)))
            .Should().NotContain(state => state.Badge == "badge-draft", "a seeded state is badged by what it means");
    }

    [Fact]
    public void OnlyBadgeFor_NamesABadgeClass()
    {
        // One place names a badge class, so a page cannot name one app.css does not define (T266). A page writes
        // class="badge @BadgeFor.…(…)".
        var web = WebFile();
        var named = Directory.EnumerateFiles(web, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".razor", StringComparison.Ordinal) || path.EndsWith(".cs", StringComparison.Ordinal))
            .Where(path => Path.GetFileName(path) != "BadgeFor.cs"
                && !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .SelectMany(path => BadgeClassName().Matches(StripComments(File.ReadAllText(path)))
                .Select(match => $"{Path.GetRelativePath(web, path)}: {match.Value}"))
            .ToList();

        named.Should().BeEmpty("every badge class comes from BadgeFor, which names only those app.css defines");
        BadgeClassName().IsMatch("""<span class="badge badge-@item.CurrentState">""").Should().BeTrue("guard: the pattern sees a glued key");
        BadgeClassName().IsMatch("""<span class="@($"badge badge-{item.CurrentState}")">""").Should().BeTrue(
            "guard: the pattern sees an interpolated key (T266 review)");
        BadgeClassName().IsMatch("""var css = $"badge-{state}";""").Should().BeTrue("guard: and one built in C#");
        BadgeClassName().IsMatch("""<span class="badge @BadgeFor.State(BadgeState.Draft)">""").Should().BeFalse();
    }

    // ---- the scan ----

    /// <summary>A class a page names: a word, or the start of one glued to an expression (<c>badge-@key</c>).</summary>
    private sealed record ClassUse(string Name, bool IsPrefix)
    {
        public override string ToString() => IsPrefix ? $"{Name}…" : Name;
    }

    private enum Verdict
    {
        Defined,
        Framework,
        GluedPrefix,
        Undefined
    }

    /// <summary>
    /// Whether a stylesheet defines the class. The start of a class passes only as a whole class of its own, or on
    /// <see cref="GluedPrefixes" />: never because some defined class starts with it.
    /// </summary>
    private static Verdict Judge(ClassUse use, IReadOnlySet<string> defined, string file)
        => defined.Contains(use.Name) ? Verdict.Defined
            : !use.IsPrefix && FrameworkClasses.Contains(use.Name) ? Verdict.Framework
            : use.IsPrefix && GluedPrefixes.Contains((file, use.Name)) ? Verdict.GluedPrefix
            : Verdict.Undefined;

    /// <summary>
    /// The kinds an <c>&lt;Alert&gt;</c> can be given: a <c>Kind="…"</c> written out, or the string literals of an explicit
    /// expression (<c>Kind="@(ok ? "success" : "danger")"</c>), read as the class scan reads one.
    /// </summary>
    private static List<string> AlertKindsIn(string razor)
    {
        var text = StripComments(razor);
        var kinds = new List<string>();
        foreach (Match match in AlertKindStart().Matches(text))
        {
            var start = match.Index + match.Length;
            if (text[start] == '@' && start + 1 < text.Length && text[start + 1] == '(')
            {
                var uses = new List<ClassUse>();
                ReadExplicitExpression(text, start + 1, uses);
                kinds.AddRange(uses.Select(use => use.Name));
            }
            else if (text[start] != '@')
            {
                kinds.Add(text[start..text.IndexOf('"', start)]);
            }
        }

        return kinds;
    }

    /// <summary>Every class selector in a stylesheet, its comments and quoted strings (a data URI, a font's file) removed.</summary>
    private static HashSet<string> DefinedIn(string css)
        => ClassSelector().Matches(CssString().Replace(CssComment().Replace(css, string.Empty), string.Empty))
            .Select(match => match.Groups["name"].Value)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>The classes named by every <c>class="…"</c> (or a component's <c>Class="…"</c>) in Razor markup.</summary>
    private static List<ClassUse> ClassesNamedIn(string razor)
    {
        var text = StripComments(razor);
        var uses = new List<ClassUse>();
        foreach (Match attribute in ClassAttributeStart().Matches(text))
        {
            ReadValue(text, attribute.Index + attribute.Length, uses);
        }

        return uses;
    }

    /// <summary>Razor's <c>@* … *@</c> and HTML's <c>&lt;!-- … --&gt;</c> comments, and C#'s, blanked out.</summary>
    private static string StripComments(string text)
        => LineComment().Replace(BlockComment().Replace(text, " "), string.Empty);

    /// <summary>Reads one attribute value from just after its opening quote to its closing one.</summary>
    private static void ReadValue(string text, int index, List<ClassUse> uses)
    {
        var word = new StringBuilder();
        void Flush(bool isPrefix)
        {
            if (word.Length > 0)
            {
                uses.Add(new ClassUse(word.ToString(), isPrefix));
                word.Clear();
            }
        }

        while (index < text.Length)
        {
            var character = text[index];
            if (character == '"')
            {
                Flush(isPrefix: false);
                return;
            }

            if (character == '@' && index + 1 < text.Length && text[index + 1] == '@')
            {
                word.Append('@');
                index += 2;
                continue;
            }

            if (character == '@')
            {
                // A word written against an expression is the start of a class the expression finishes.
                Flush(isPrefix: true);
                if (index + 1 < text.Length && text[index + 1] == '(')
                {
                    index = ReadExplicitExpression(text, index + 1, uses);
                }
                else
                {
                    index = SkipImplicitExpression(text, index + 1);
                }

                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                Flush(isPrefix: false);
            }
            else
            {
                word.Append(character);
            }

            index++;
        }
    }

    /// <summary>
    /// Reads <c>@( … )</c> from its opening parenthesis, adding the classes its string literals can put there: every
    /// literal but one compared with <c>==</c> or <c>!=</c>. An interpolated literal's text before a hole is the start of a
    /// class the hole finishes. Returns the index after the closing parenthesis.
    /// </summary>
    private static int ReadExplicitExpression(string text, int index, List<ClassUse> uses)
    {
        var depth = 0;
        while (index < text.Length)
        {
            var character = text[index];
            if (character == '"' || (character == '$' && index + 1 < text.Length && text[index + 1] == '"'))
            {
                var interpolated = character == '$';
                var start = interpolated ? index + 1 : index;
                var end = SkipString(text, start);
                var literal = text[(start + 1)..(end - 1)];
                if (!IsCompared(text, index, end))
                {
                    AddLiteral(literal, interpolated, uses);
                }

                index = end;
                continue;
            }

            if (character == '(')
            {
                depth++;
            }
            else if (character == ')' && --depth == 0)
            {
                return index + 1;
            }

            index++;
        }

        throw new InvalidOperationException("An explicit expression in a class attribute does not close.");
    }

    /// <summary>Whether the literal from <paramref name="start" /> to <paramref name="end" /> is one side of == or !=.</summary>
    private static bool IsCompared(string text, int start, int end)
    {
        var before = start - 1;
        while (before >= 0 && char.IsWhiteSpace(text[before]))
        {
            before--;
        }

        var after = end;
        while (after < text.Length && char.IsWhiteSpace(text[after]))
        {
            after++;
        }

        return (before >= 1 && text[before] == '=' && text[before - 1] is '=' or '!')
            || (after + 1 < text.Length && text[after] is '=' or '!' && text[after + 1] == '=');
    }

    private static void AddLiteral(string literal, bool interpolated, List<ClassUse> uses)
    {
        if (!interpolated)
        {
            uses.AddRange(literal.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(name => new ClassUse(name, IsPrefix: false)));
            return;
        }

        // "form-select{Invalid}": the text before a hole is the start of a class, unless a space ends it.
        var parts = InterpolationHole().Split(literal);
        for (var part = 0; part < parts.Length; part++)
        {
            var words = parts[part].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var gluedToHole = part < parts.Length - 1 && parts[part].Length > 0 && !parts[part].EndsWith(' ');
            for (var word = 0; word < words.Length; word++)
            {
                uses.Add(new ClassUse(words[word], IsPrefix: gluedToHole && word == words.Length - 1));
            }
        }
    }

    /// <summary>From an opening quote to just after its closing one, over escapes.</summary>
    private static int SkipString(string text, int index)
    {
        for (var at = index + 1; at < text.Length; at++)
        {
            if (text[at] == '\\')
            {
                at++;
            }
            else if (text[at] == '"')
            {
                return at + 1;
            }
        }

        throw new InvalidOperationException("A string literal in a class attribute does not close.");
    }

    /// <summary>
    /// Skips <c>@Name.Member(args)[index]</c>, the implicit expression Razor reads: its literals are arguments, not
    /// classes. A trailing dot is text.
    /// </summary>
    private static int SkipImplicitExpression(string text, int index)
    {
        var at = index;
        while (at < text.Length)
        {
            var character = text[at];
            if (char.IsLetterOrDigit(character) || character is '_' or '.')
            {
                at++;
            }
            else if (character is '(' or '[')
            {
                var close = character == '(' ? ')' : ']';
                var depth = 0;
                while (at < text.Length)
                {
                    if (text[at] == '"')
                    {
                        at = SkipString(text, at);
                        continue;
                    }

                    if (text[at] == character)
                    {
                        depth++;
                    }
                    else if (text[at] == close && --depth == 0)
                    {
                        at++;
                        break;
                    }

                    at++;
                }
            }
            else
            {
                break;
            }
        }

        while (at > index && text[at - 1] == '.')
        {
            at--;
        }

        return at;
    }

    // ---- the seeds' states ----

    /// <summary>Every state the seeded workflows declare, with whether it is terminal there.</summary>
    private static IReadOnlyList<(string Key, bool Terminal)> SeededStates()
        => Directory.EnumerateFiles(
                Path.Combine(SolutionRoot(), "src", "Wombat.Infrastructure", "Activities", "Seeds"), "workflow.json", SearchOption.AllDirectories)
            .SelectMany(path =>
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                return document.RootElement.GetProperty("states").EnumerateArray()
                    .Select(state => (
                        Key: state.GetProperty("key").GetString()!,
                        Terminal: state.TryGetProperty("terminal", out var terminal) && terminal.GetBoolean()))
                    .ToList();
            })
            .Distinct()
            .ToList();

    private static string WebFile(params string[] parts)
        => Path.Combine([SolutionRoot(), "src", "Wombat.Web", .. parts]);

    private static string SolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
    }

    /// <summary>The start of a class attribute's value: <c>class="</c>, or a component's <c>Class="</c>.</summary>
    [GeneratedRegex(@"(?<![\w-])[cC]lass\s*=\s*""")]
    private static partial Regex ClassAttributeStart();

    /// <summary>A class selector: a dot and a name, which starts with a letter, so not a decimal point (<c>0.5rem</c>).</summary>
    [GeneratedRegex(@"\.(?<name>-?[_a-zA-Z][\w-]*)")]
    private static partial Regex ClassSelector();

    [GeneratedRegex(@"""[^""]*""|'[^']*'")]
    private static partial Regex CssString();

    /// <summary>An <c>&lt;Alert&gt;</c>'s <c>Kind="</c>, up to the value.</summary>
    [GeneratedRegex(@"<Alert\b[^>]*?\bKind=""")]
    private static partial Regex AlertKindStart();

    /// <summary>
    /// A badge's own class: <c>badge-</c> and a word, a Razor expression or an interpolation hole
    /// (<c>$"badge-{key}"</c>), not <c>badge</c> alone.
    /// </summary>
    [GeneratedRegex(@"(?<![\w-])badge-[\w@{]")]
    private static partial Regex BadgeClassName();

    [GeneratedRegex(@"\{[^{}]*\}")]
    private static partial Regex InterpolationHole();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex CssComment();

    /// <summary>Razor, HTML and C# block comments.</summary>
    [GeneratedRegex(@"@\*.*?\*@|<!--.*?-->|/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex BlockComment();

    /// <summary>A C# line comment (<c>//</c> or <c>///</c>) standing on its own line.</summary>
    [GeneratedRegex(@"^\s*//.*$", RegexOptions.Multiline)]
    private static partial Regex LineComment();
}
