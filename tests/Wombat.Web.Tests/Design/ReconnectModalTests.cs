using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using FluentAssertions;
using Wombat.Web.Tests.Hosting;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// T330, T335 flow 01: the reconnect dialog shows one state at a time, in the design's six states and words, and each
/// button says what its text says (DESIGN.md § The reconnect dialog and the error bar).
/// </summary>
/// <remarks>
/// <para>
/// Until T330 the dialog was the .NET 10 template's. While the runtime retried it showed "Rejoining the server..." above
/// "Rejoin failed... trying again in N seconds.": blazor.web.js adds <c>components-reconnect-retrying</c> beside
/// <c>components-reconnect-show</c> and takes neither away while it retries, and the stylesheet showed the first
/// attempt's line under <c>-show</c> whatever else was there. And Resume failed said "Please retry" above a button
/// labelled Resume.
/// </para>
/// <para>
/// bUnit lays nothing out and runs no script, so these read what makes the states: the markup the app serves, the rules
/// of <c>ReconnectModal.razor.css</c>, evaluated on that markup for each class the runtime puts on the dialog, and the
/// runtime the app serves, held to the names the dialog is built on. What the script does with the runtime's events is
/// checked in a browser by suspending the app (states.md § Shell and framework).
/// </para>
/// </remarks>
public sealed partial class ReconnectModalTests
{
    private const string Lost =
        "Wombat cannot be reached. Try again when your connection is back. If the page cannot be restored, it reloads, and "
        + "anything not yet saved on it is lost.";

    /// <summary>The six states, in the order the dialog holds them.</summary>
    private static readonly ReconnectState[] States =
    [
        new("rejoining", "components-reconnect-first-attempt-visible", "Reconnecting",
            [new(Attempt: null, "The connection to Wombat dropped. Reconnecting now.", HasBar: true)],
            Button: null, Action: null, Role: null),
        // One state with two lines: the count while the runtime waits its turn, and "Trying again now." with the bar once
        // the attempt starts (R2-Reconnect, states 2 and 3: "The same line, replaced when the attempt starts.").
        new("retrying", "components-reconnect-repeated-attempt-visible", "Could not reconnect",
            [new("waiting", "Trying again in a few seconds.", HasBar: false), new("started", "Trying again now.", HasBar: true)],
            Button: null, Action: null, Role: null),
        new("failed", "components-reconnect-failed-visible", "Connection lost",
            [new(Attempt: null, Lost, HasBar: false)], Button: "Try again", Action: "retry", Role: "alertdialog"),
        new("paused", "components-pause-visible", "Page paused",
            [new(Attempt: null, "This page is paused. Resume to carry on.", HasBar: false)], Button: "Resume", Action: "resume", Role: null),
        new("resume-failed", "components-resume-failed-visible", "Could not resume",
            [new(Attempt: null, Lost, HasBar: false)], Button: "Try again", Action: "resume", Role: "alertdialog"),
        new("rejected", "components-reconnect-rejected-visible", "Reloading",
            [new(Attempt: null, "Reloading the page…", HasBar: false)], Button: null, Action: null, Role: null),
    ];

    // ---- the markup ----

    [Fact]
    public async Task TheDialog_HoldsOneElementPerState_InTheDesignsWords()
    {
        var dialog = await ServedDialogAsync();

        dialog.LocalName.Should().Be("dialog", "a native <dialog>, opened modal over the page");
        dialog.HasAttribute("data-nosnippet").Should().BeTrue();

        var elements = StateElements(dialog);
        elements.Select(Key).Should().Equal(States.Select(state => state.Key), "one element per state, and no other");

        foreach (var (element, state) in elements.Zip(States))
        {
            element.ClassList.Should().Contain(["reconnect-state", state.VisibleClass], $"{state.Key} is shown by its own rule");
            element.ClassList.Where(name => name.EndsWith("-visible", StringComparison.Ordinal)).Should().ContainSingle(
                $"{state.Key} shows under one state's rule, never two");

            var heading = element.QuerySelectorAll("h2").Should().ContainSingle($"{state.Key} has one heading").Which;
            Text(heading).Should().Be(state.Heading);
            heading.Id.Should().NotBeNullOrEmpty("the dialog is named by the heading of the state it shows");
            heading.GetAttribute("tabindex").Should().Be("-1", "it takes the focus when the dialog opens, and Tab never reaches it");
            var icon = heading.QuerySelector("svg")!;
            icon.GetAttribute("aria-hidden").Should().Be("true", "its icon is decoration");
            icon.Children.Should().NotBeEmpty("its paths are inlined: the dialog shows when no /icons/ file can be fetched");
            icon.QuerySelectorAll("use").Should().BeEmpty();

            var sentence = element.QuerySelectorAll("p[id]").Should().ContainSingle($"{state.Key} has one sentence").Which;
            var bars = element.QuerySelectorAll("[role=progressbar]").ToList();
            if (state.Lines.Length == 1)
            {
                Text(sentence).Should().Be(state.Lines[0].Sentence);
                element.QuerySelectorAll("[data-reconnect-line]").Should().BeEmpty($"{state.Key} has one line");
                bars.Should().HaveCount(state.Lines[0].HasBar ? 1 : 0, "the bar runs while an attempt is under way");
            }
            else
            {
                // Each line in the one sentence the dialog is described by, marked with the data-attempt it shows under;
                // the bar belongs to the line whose attempt is under way.
                sentence.Children.Select(line => (line.GetAttribute("data-reconnect-line"), Text(line))).Should().Equal(
                    state.Lines.Select(line => (line.Attempt, line.Sentence)), $"{state.Key}'s sentence is one of its lines at a time");
                bars.Select(bar => bar.GetAttribute("data-reconnect-line")).Should().Equal(
                    state.Lines.Where(line => line.HasBar).Select(line => line.Attempt), "the bar runs while an attempt is under way");
            }

            element.GetAttribute("data-role").Should().Be(state.Role,
                "an alertdialog where the person has to act; the <dialog>'s own role elsewhere");
        }

        dialog.GetAttribute("aria-labelledby").Should().Be(elements[0].QuerySelector("h2")!.Id, "it opens on Reconnecting");
        dialog.GetAttribute("aria-describedby").Should().Be(elements[0].QuerySelector("p[id]")!.Id);
        var countdown = dialog.QuerySelectorAll("[data-reconnect-countdown]").Should().ContainSingle().Which;
        countdown.Closest("[data-reconnect-line]")!.GetAttribute("data-reconnect-line").Should().Be("waiting",
            "the script writes the runtime's count into Could not reconnect's line while it waits");
        Key(countdown.Closest("[data-reconnect-state]")!).Should().Be("retrying");
    }

    [Fact]
    public async Task NoStateOffersTryNow_AndEachButtonSaysWhatItsSentenceSays()
    {
        // T330: Resume failed said "Please retry or reload the page." above a button labelled Resume. And the runtime has
        // no call that skips its countdown, so a Try now could only race the attempt the runtime makes itself.
        var dialog = await ServedDialogAsync();

        Text(dialog).Should().NotContainAny(["Try now", "Retry", "retry", "Rejoin", "Please"],
            "the template's words are gone, and nothing offers to skip the countdown");

        foreach (var state in States)
        {
            var element = dialog.QuerySelector($"[data-reconnect-state=\"{state.Key}\"]")!;
            var buttons = element.QuerySelectorAll("button").ToList();
            if (state.Button is null)
            {
                buttons.Should().BeEmpty($"{state.Key} asks nothing of the person");
                continue;
            }

            var button = buttons.Should().ContainSingle($"{state.Key} has one button").Which;
            Text(button).Should().Be(state.Button);
            button.GetAttribute("type").Should().Be("button");
            button.ClassList.Should().Contain(["btn", "btn-primary"]);
            button.GetAttribute("data-reconnect-action").Should().Be(state.Action,
                "Connection lost's Try again runs the template's retry(); Resume and Could not resume's Try again, its resume()");
            state.Lines.Should().ContainSingle().Which.Sentence.Should().Contain(state.Button,
                $"{state.Key}'s sentence names the button beside it");
        }

        dialog.QuerySelectorAll("button").Should().HaveCount(3, "Try again, Resume and Try again, and no other");
    }

    [Fact]
    public async Task ALiveRegion_SaysEachStatesSentence_WithoutTheCount()
    {
        var dialog = await ServedDialogAsync();

        var live = dialog.QuerySelectorAll("[aria-live]").Should().ContainSingle("one live region").Which;
        live.GetAttribute("aria-live").Should().Be("polite");
        live.HasAttribute("data-reconnect-live").Should().BeTrue("the script writes it");
        live.ParentElement.Should().BeSameAs(dialog,
            "inside the dialog, outside every state: while the dialog is modal the page behind it is inert, and so is any "
            + "live region there");
        Text(live).Should().BeEmpty("it is written only as the state changes");

        foreach (var state in States)
        {
            var announce = dialog.QuerySelector($"[data-reconnect-state=\"{state.Key}\"]")!.GetAttribute("data-announce");
            announce.Should().NotBeNullOrWhiteSpace($"{state.Key} has a sentence to announce");
            announce.Should().NotMatchRegex(@"\d", "the count is never announced, so a tick of it says nothing");
            if (state.Lines.Length == 1)
            {
                announce.Should().Be(state.Lines[0].Sentence, $"{state.Key} announces the sentence it shows");
            }
        }

        dialog.QuerySelector("[data-reconnect-state=\"retrying\"]")!.GetAttribute("data-announce").Should().Be(
            "Could not reconnect. Trying again.",
            "the retry's two lines are one sentence to a listener, so an attempt starting or its count beginning says nothing new");
    }

    // ---- the stylesheet ----

    public static TheoryData<string, string, string?, string?> RuntimeStates => new()
    {
        // What the runtime (UserSpecifiedDisplay) puts on the dialog, and what the script adds as data-attempt.
        { "the connection drops", "components-reconnect-show", null, "rejoining" },
        { "the first ten attempts, made at once", "components-reconnect-show", "started", "rejoining" },
        { "counting down to the next attempt (T330)", "components-reconnect-show components-reconnect-retrying", "waiting", "retrying" },
        { "the attempt under way (T330)", "components-reconnect-show components-reconnect-retrying", "started", "retrying" },
        { "the attempts are spent", "components-reconnect-failed", null, "failed" },
        { "the circuit is paused", "components-reconnect-paused", null, "paused" },
        { "Resume could not reach the server", "components-reconnect-resume-failed", null, "resume-failed" },
        { "the server no longer holds the page", "components-reconnect-rejected", null, "rejected" },
        { "the connection is back", "components-reconnect-hide", null, null },
    };

    [Theory]
    [MemberData(nameof(RuntimeStates))]
    public async Task EachStateOfTheRuntime_ShowsOneElement_AndOneLine(string when, string classes, string? attempt, string? shown)
    {
        var dialog = await ServedDialogAsync();
        var rules = ReconnectCss();
        var dialogClasses = classes.Split(' ').ToHashSet(StringComparer.Ordinal);

        var displayed = StateElements(dialog).Where(element => Shows(rules, dialog, element, dialogClasses, attempt)).ToList();

        var expected = shown is null ? Array.Empty<string>() : new[] { shown };
        displayed.Select(Key).Should().Equal(expected, $"when {when}, {(shown is null ? "no state shows" : $"one state shows, {shown}")}");
        if (shown is null)
        {
            return;
        }

        var state = States.Single(candidate => candidate.Key == shown);
        var line = state.Lines.Length == 1 ? state.Lines[0] : state.Lines.Single(candidate => candidate.Attempt == attempt);
        var element = displayed[0];
        VisibleText(rules, dialog, element.QuerySelector("p[id]")!, dialogClasses, attempt).Should().Be(line.Sentence,
            $"when {when}, {shown} shows one line");
        element.QuerySelectorAll("[role=progressbar]").Count(bar => Shows(rules, dialog, bar, dialogClasses, attempt))
            .Should().Be(line.HasBar ? 1 : 0, "the bar runs while an attempt is under way");
    }

    // T335 flow 01, the last review of the t335 branch: "waiting" and "attempt started" were two elements, each with its
    // own "Could not reconnect". settle() moves the focus to the shown state's heading whenever the shown state does not
    // hold it, so in an outage the focus jumped from one heading to the other as each attempt started and each count
    // began, and a screen reader read the heading again every few seconds. The design (R2-Reconnect, states 2 and 3):
    // "The same line, replaced when the attempt starts."
    [Fact]
    public async Task AcrossTheRetrysAttempts_OneStateStaysShown_SoItsHeadingKeepsTheFocus()
    {
        var dialog = await ServedDialogAsync();
        var rules = ReconnectCss();
        var retrying = new HashSet<string>(["components-reconnect-show", "components-reconnect-retrying"], StringComparer.Ordinal);
        string[] attempts = ["waiting", "started", "waiting"];

        var shown = attempts
            .Select(attempt => StateElements(dialog).Where(element => Shows(rules, dialog, element, retrying, attempt))
                .Should().ContainSingle($"one state shows while the attempt is {attempt}").Which)
            .ToList();

        shown.Select(Key).Distinct().Should().ContainSingle(
            "the retry is one element as each attempt starts and each count begins, so settle() finds the focus inside it "
            + "and leaves it on its heading, and the dialog keeps its name");
        shown.Should().OnlyContain(element => element == shown[0], "the same element, and so the same heading");
        attempts.Select((attempt, index) => VisibleText(rules, dialog, shown[index].QuerySelector("p[id]")!, retrying, attempt))
            .Should().Equal(["Trying again in a few seconds.", "Trying again now.", "Trying again in a few seconds."],
                "its line is replaced as the attempt starts, and again as the next count begins");
    }

    [Fact]
    public void WhileTheRuntimeRetries_TheFirstAttemptsStateIsHidden()
    {
        // T330's fix, by name: the runtime never takes -show away while it retries, so a rule under -retrying must hide
        // the state -show shows, and come after the rule that shows it, the two being equally specific.
        var rules = ReconnectCss();
        const string first = ".components-reconnect-first-attempt-visible";

        var hides = rules.Should().ContainSingle(rule => rule.AtRule.Length == 0
                && rule.Selectors.Contains($"#components-reconnect-modal.components-reconnect-retrying {first}"))
            .Which;
        hides.Value("display").Should().Be("none");
        var shows = rules.Should().ContainSingle(rule => rule.AtRule.Length == 0
                && rule.Selectors.Contains($"#components-reconnect-modal.components-reconnect-show {first}"))
            .Which;
        hides.Index.Should().BeGreaterThan(shows.Index);
    }

    [Fact]
    public void OnlyTheStateRules_ShowOrHideAStateOrALine()
    {
        // The evaluation above reads rules of one shape. A rule of any other shape that set a state's or a line's display,
        // or one in a media query, would change what shows without the evaluation seeing it.
        var rules = ReconnectCss();
        var targets = States.Select(state => state.VisibleClass).Append("reconnect-state").ToHashSet(StringComparer.Ordinal);

        var unread = rules
            .Where(rule => rule.Value("display") is not null)
            .SelectMany(rule => rule.Selectors.Select(selector => (rule.AtRule, Selector: selector)))
            .Where(entry => targets.Any(target => Regex.IsMatch(entry.Selector, $@"\.{Regex.Escape(target)}(?![\w-])"))
                || entry.Selector.Contains("data-reconnect-line", StringComparison.Ordinal))
            .Where(entry => entry.AtRule.Length > 0 || !StateSelector().IsMatch(entry.Selector))
            .Select(entry => $"{entry.AtRule} {entry.Selector}".Trim());

        unread.Should().BeEmpty();
    }

    [Fact]
    public void TheDialog_Is400PxWide_And16PxFromEachSideWhereTheScreenIsNarrower_OnTheScrim()
    {
        var rules = ReconnectCss();

        var dialog = TopLevel(rules, "#components-reconnect-modal");
        dialog.Value("width").Should().Be("25rem", "400px");
        dialog.Value("max-width").Should().Be("calc(100% - 2 * var(--space-md))", "358px at 390");
        dialog.Value("border-radius").Should().Be("var(--radius-xl)");
        dialog.Value("box-shadow").Should().Be("var(--shadow-dialog)");
        dialog.Value("background-color").Should().Be("var(--surface-color)");
        TopLevel(rules, "#components-reconnect-modal::backdrop").Value("background-color").Should().Be("var(--scrim)");

        rules.Should().Contain(rule => rule.AtRule == "@media (max-width: 640.98px)"
                && rule.Selector == ".reconnect-actions .btn" && rule.Value("min-height") == "44px",
            "a phone's button is a 44px target, the whole width of the dialog");

        RawColour().Matches(Stylesheet.WithoutComments(File.ReadAllText(CssPath()))).Select(match => match.Value)
            .Should().BeEmpty("a colour is a token from :root (DESIGN.md § Design tokens)");
    }

    [Fact]
    public void UnderReducedMotion_TheSpinnerTheBarAndTheDialogStop_AndTheDialogIsStillSeen()
    {
        var rules = ReconnectCss();

        var still = rules.Should().ContainSingle(rule => rule.AtRule == "@media (prefers-reduced-motion: reduce)").Which;
        still.Selectors.Should().Contain([
            "#components-reconnect-modal", "#components-reconnect-modal[open]", "#components-reconnect-modal::backdrop",
            ".reconnect-spinner", ".reconnect-bar-fill",
        ]);
        still.Value("animation").Should().Be("none");
        still.Value("transition").Should().Be("none");

        // The template started the dialog at opacity 0 and let its fade-in take it to 1. With the animations off it would
        // have stayed transparent, over a scrim that blocks the page: nothing to read and nothing to press.
        rules.Where(rule => rule.Selectors.Any(selector => selector.StartsWith("#components-reconnect-modal", StringComparison.Ordinal)
                && !selector.Contains(' ', StringComparison.Ordinal)))
            .Where(rule => rule.Value("opacity") is not null)
            .Select(rule => rule.Selector)
            .Should().BeEmpty("the dialog is opaque at rest; only an animation starts it transparent");
    }

    // ---- the runtime ----

    [Fact]
    public async Task TheRuntimeTheAppServes_KeepsTheNamesTheDialogIsBuiltOn()
    {
        await using var host = await AppTestHost.StartAsync();
        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);
        var runtime = await ServedScriptAsync(host, document, "blazor.web");
        var module = LineComment().Replace(await ServedScriptAsync(host, document, "ReconnectModal"), string.Empty);

        runtime.Should().ContainAll(["\"components-reconnect-modal\"", "\"components-reconnect-state-changed\"", "secondsToNextAttempt"],
            "the runtime finds the dialog by its id, raises its event, and counts down in secondsToNextAttempt");

        // Every class the stylesheet keys a state on and every one the script sets is one the runtime itself uses.
        var runtimeClasses = ReconnectCss()
            .SelectMany(rule => rule.Selectors)
            .Select(selector => StateSelector().Match(selector))
            .Where(match => match.Success)
            .SelectMany(match => match.Groups["qual"].Captures.Select(capture => capture.Value))
            .Where(qual => qual.StartsWith('.'))
            .Select(qual => qual[1..])
            .Concat(ModuleClass().Matches(module).Select(match => match.Value))
            .Distinct()
            .ToList();
        runtimeClasses.Should().Contain(["components-reconnect-show", "components-reconnect-retrying",
            "components-reconnect-failed", "components-reconnect-paused", "components-reconnect-resume-failed",
            "components-reconnect-rejected"], "guard: the scan reads the classes");
        runtimeClasses.Where(name => !runtime.Contains($"\"{name}\"", StringComparison.Ordinal))
            .Should().BeEmpty("a class the runtime never puts on the dialog shows nothing");

        // Every detail.state the script acts on is one the runtime raises.
        var handled = HandledState().Matches(module).Select(match => match.Groups["state"].Value).ToList();
        handled.Should().BeEquivalentTo(["show", "hide", "retrying", "failed", "rejected"]);
        handled.Where(state => !Regex.IsMatch(runtime, $@"state:\s*[""']{state}[""']"))
            .Should().BeEmpty("the script waits on no state the runtime does not raise");

        module.Should().Contain("addEventListener(\"components-reconnect-state-changed\"");
        Regex.Matches(module, @"Blazor\.reconnect\(").Should().ContainSingle(
            "the runtime's own attempt is never raced: Blazor.reconnect() runs only under Connection lost's Try again");
    }

    // The review of the t335 branch: the heading took the focus only as the dialog opened. When Reconnecting hid as the
    // runtime began to retry, the focus fell to the body inside the modal, and Connection lost, an alertdialog, never took
    // it. After every event the script settles on the shown state; a focus that is not inside it moves to its heading.
    [Fact]
    public async Task WhenTheStateHoldingTheFocusGoes_TheShownStatesHeadingTakesIt()
    {
        await using var host = await AppTestHost.StartAsync();
        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);
        var module = LineComment().Replace(await ServedScriptAsync(host, document, "ReconnectModal"), string.Empty);

        var settle = Regex.Match(module, @"function settle\(\) \{(?<body>.*?)\n\}", RegexOptions.Singleline);
        settle.Success.Should().BeTrue("guard: the script settles the dialog on its shown state");
        var body = settle.Groups["body"].Value;

        Regex.IsMatch(body, @"if \(!shown\.contains\(document\.activeElement\)\) \{\s*heading\.focus\(\);", RegexOptions.Singleline)
            .Should().BeTrue("a focus outside the shown state, fallen to the body with the state that held it, moves to its heading");
        body.IndexOf("aria-labelledby", StringComparison.Ordinal).Should().BeLessThan(
            body.IndexOf("!shown.contains(document.activeElement)", StringComparison.Ordinal),
            "the dialog is named by the new state's heading before the heading takes the focus");
        Regex.Matches(body, @"heading\.focus\(\)").Should().HaveCount(2, "as the dialog opens, and whenever the focus is not in the shown state");
        Regex.IsMatch(body, @"else if \(focusWhenSettled\)|if \(focusWhenSettled\)").Should().BeTrue("the opening's focus stays");
    }

    // ---- helpers ----

    /// <summary>A state of the dialog: what it shows, and how the page shows it.</summary>
    private sealed record ReconnectState(
        string Key, string VisibleClass, string Heading, ReconnectLine[] Lines, string? Button, string? Action, string? Role);

    /// <summary>
    /// A line of a state: its sentence, whether the bar runs under it, and the <c>data-attempt</c> it shows under, or null
    /// for a state's only line.
    /// </summary>
    private sealed record ReconnectLine(string? Attempt, string Sentence, bool HasBar);

    private static async Task<IElement> ServedDialogAsync()
    {
        await using var host = await AppTestHost.StartAsync();
        var (_, _, document) = await host.LoadAsync(AppTestHost.AnonymousPage);
        return document.QuerySelectorAll("#components-reconnect-modal").Should().ContainSingle("App renders it on every page").Which;
    }

    private static async Task<string> ServedScriptAsync(AppTestHost host, IDocument document, string name)
    {
        var src = document.QuerySelectorAll("script[src]").Select(script => script.GetAttribute("src")!)
            .Should().ContainSingle(url => url.Contains(name, StringComparison.Ordinal), $"the page loads {name} once").Which;
        using var response = await host.Client.GetAsync(src);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static List<IElement> StateElements(IElement dialog) => dialog.QuerySelectorAll("[data-reconnect-state]").ToList();

    private static string Key(IElement state) => state.GetAttribute("data-reconnect-state")!;

    /// <summary>The rendered text, its whitespace collapsed.</summary>
    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private static string CssPath() => Stylesheet.WebFile("Components", "Layout", "ReconnectModal.razor.css");

    private static IReadOnlyList<CssRule> ReconnectCss() => Stylesheet.Parse(File.ReadAllText(CssPath()));

    private static CssRule TopLevel(IReadOnlyList<CssRule> rules, string selector)
        => rules.Should().ContainSingle(rule => rule.AtRule.Length == 0 && rule.Selector == selector,
            $"the stylesheet styles {selector} once at the top level").Which;

    /// <summary>Whether an element of the dialog shows: neither it nor anything between it and the dialog is display: none.</summary>
    private static bool Shows(
        IReadOnlyList<CssRule> rules, IElement dialog, IElement element, IReadOnlySet<string> dialogClasses, string? attempt)
    {
        for (var node = element; node is not null && node != dialog; node = node.ParentElement)
        {
            if (Display(rules, node, dialogClasses, attempt) == "none")
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>The text an element shows: its text, and that of each descendant that is not display: none.</summary>
    private static string VisibleText(
        IReadOnlyList<CssRule> rules, IElement dialog, IElement element, IReadOnlySet<string> dialogClasses, string? attempt)
    {
        Shows(rules, dialog, element, dialogClasses, attempt).Should().BeTrue("guard: the element itself shows");
        var text = new StringBuilder();
        Append(element);
        return Regex.Replace(text.ToString(), @"\s+", " ").Trim();

        void Append(INode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child is IElement childElement)
                {
                    if (Display(rules, childElement, dialogClasses, attempt) != "none")
                    {
                        Append(childElement);
                    }
                }
                else if (child.NodeType == NodeType.Text)
                {
                    text.Append(child.TextContent);
                }
            }
        }
    }

    /// <summary>
    /// The display an element of the dialog takes: the top-level rules whose selector matches it, the most specific and
    /// then the latest winning, as the cascade decides; or "initial", its own, where none does. A selector is the dialog,
    /// with the runtime's classes and the script's data-attempt, above a class or a line the element carries; or that
    /// class or line alone.
    /// </summary>
    private static string Display(IReadOnlyList<CssRule> rules, IElement element, IReadOnlySet<string> dialogClasses, string? attempt)
    {
        var candidates =
            from rule in rules
            where rule.AtRule.Length == 0 && rule.Value("display") is not null
            from selector in rule.Selectors
            let match = StateSelector().Match(selector)
            where match.Success && Carries(element, match.Groups["target"].Value)
            let quals = match.Groups["qual"].Captures.Select(capture => capture.Value).ToList()
            where quals.All(qual => Holds(qual, dialogClasses, attempt))
            select (Ids: match.Groups["dialog"].Success ? 1 : 0, Classes: quals.Count + 1, rule.Index, Display: rule.Value("display")!);

        return candidates.OrderBy(candidate => candidate.Ids).ThenBy(candidate => candidate.Classes).ThenBy(candidate => candidate.Index)
            .Select(candidate => candidate.Display)
            .LastOrDefault() ?? "initial";
    }

    /// <summary>Whether the element carries a selector's target: a class, or a <c>data-reconnect-line</c>.</summary>
    private static bool Carries(IElement element, string target)
    {
        if (target.StartsWith('.'))
        {
            return element.ClassList.Contains(target[1..]);
        }

        var line = LineTarget().Match(target);
        return line.Success && element.GetAttribute("data-reconnect-line") == line.Groups["value"].Value;
    }

    /// <summary>Whether the dialog meets a qualifier: one of the runtime's classes, or the script's data-attempt, or not it.</summary>
    private static bool Holds(string qual, IReadOnlySet<string> dialogClasses, string? attempt)
    {
        if (qual.StartsWith('.'))
        {
            return dialogClasses.Contains(qual[1..]);
        }

        var match = AttemptQual().Match(qual);
        match.Success.Should().BeTrue($"guard: {qual} is a data-attempt qualifier");
        return (attempt == match.Groups["value"].Value) != match.Groups["not"].Success;
    }

    /// <summary>
    /// A state rule's selector: optionally the dialog with the runtime's classes and data-attempt (or <c>:not()</c> it),
    /// then a class or a line.
    /// </summary>
    [GeneratedRegex(@"^(?:(?<dialog>#components-reconnect-modal)(?<qual>\.[\w-]+|\[data-attempt=""[\w-]+""\]|:not\(\[data-attempt=""[\w-]+""\]\))*\s+)?(?<target>\.[\w-]+|\[data-reconnect-line=""[\w-]+""\])$")]
    private static partial Regex StateSelector();

    [GeneratedRegex(@"^\[data-reconnect-line=""(?<value>[\w-]+)""\]$")]
    private static partial Regex LineTarget();

    [GeneratedRegex(@"^(?<not>:not\()?\[data-attempt=""(?<value>[\w-]+)""\]\)?$")]
    private static partial Regex AttemptQual();

    /// <summary>A runtime class the script names: components-reconnect- and a state, not the dialog's id or its event.</summary>
    [GeneratedRegex(@"(?<=[""'])components-reconnect-(?!modal[""']|state-changed[""'])[a-z-]+(?=[""'])")]
    private static partial Regex ModuleClass();

    /// <summary>A <c>case "…":</c> of the script's switch on <c>detail.state</c>.</summary>
    [GeneratedRegex(@"case ""(?<state>[a-z-]+)"":")]
    private static partial Regex HandledState();

    [GeneratedRegex(@"#[0-9a-fA-F]{3,8}\b|rgba?\(|hsla?\(|\blightyellow\b")]
    private static partial Regex RawColour();

    /// <summary>A line comment of the script, standing on its own line.</summary>
    [GeneratedRegex(@"^\s*//.*$", RegexOptions.Multiline)]
    private static partial Regex LineComment();
}
