using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Domain.Activities.Workflow;
using Wombat.Web.Components.Shared.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// T350 (flow 04, lane A2): the move words an assessor meets. A running move says its first word's -ing form with the
/// rest kept (note 2); a refused note panel says what did not happen and the state the activity is still in (note 3); the
/// note panel is one pattern for both moves that need a note (note 4; round 1, E2).
/// </summary>
public sealed class MoveWordsTests
{
    // ---- FilingWords.Running (note 2) ----

    /// <summary>Every move key the shipped seeds declare, by its sentence-case label, and what its button says running.</summary>
    public static TheoryData<string, string> SeededMoves()
    {
        var expected = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["submit"] = "Submitting…",
            ["cancel"] = "Cancelling…",
            ["complete"] = "Completing…",
            ["decline"] = "Declining…",
            ["return"] = "Returning…",
            ["accept"] = "Accepting…",
            ["approve"] = "Approving…",
            ["verify"] = "Verifying…",
            ["reject"] = "Rejecting…",
            ["review"] = "Reviewing…",
            ["revise"] = "Revising…",
            ["record"] = "Recording…",
            ["record_discussion"] = "Recording discussion…",
            ["sign_off"] = "Signing off…",
        };

        var keys = SeedSchemas.Keys()
            .SelectMany(seedKey => WorkflowParser.Parse(SeedSchemas.Workflow(seedKey)).Transitions)
            .Select(transition => transition.Key)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
        keys.Should().NotBeEmpty();
        keys.Should().BeSubsetOf(expected.Keys, "a new seeded move needs its running words checked here");

        var data = new TheoryData<string, string>();
        foreach (var key in keys)
        {
            data.Add(key, expected[key]);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(SeededMoves))]
    public void EverySeededMove_RunsInItsIngForm(string transitionKey, string running)
        => FilingWords.Running(WorkflowTransition.LabelFor(transitionKey)).Should().Be(running);

    [Theory]
    [InlineData("Log", "Logging…")]
    [InlineData("Record discussion", "Recording discussion…")]
    [InlineData("Sign off", "Signing off…")]
    [InlineData("Hand it back", "Handing it back…")]
    [InlineData("Agree", "Agreeing…")]
    [InlineData("Untie", "Untying…")]
    [InlineData("  Sign off ", "Signing off…")]
    public void AMove_RunsAsItsFirstWordsIngForm_TheRestKept(string label, string running)
        => FilingWords.Running(label).Should().Be(running);

    // The build review's D3 and G6: a builder-authored move whose last consonant doubles is spelled right while it runs,
    // and one that does not double is left alone.
    [Theory]
    [InlineData("Refer to a specialist", "Referring to a specialist…")]
    [InlineData("Admit", "Admitting…")]
    [InlineData("Commit", "Committing…")]
    [InlineData("Permit", "Permitting…")]
    [InlineData("Transfer", "Transferring…")]
    [InlineData("Begin", "Beginning…")]
    [InlineData("Stop", "Stopping…")]
    [InlineData("Plan", "Planning…")]
    [InlineData("Flag", "Flagging…")]
    [InlineData("Visit", "Visiting…")]
    [InlineData("Open", "Opening…")]
    [InlineData("Offer", "Offering…")]
    [InlineData("Audit", "Auditing…")]
    public void ABuilderMove_WhoseLastConsonantDoubles_IsSpelledRight(string label, string running)
        => FilingWords.Running(label).Should().Be(running);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Re-open")]
    [InlineData("2nd review")]
    public void AFirstWordItCannotForm_RunsAsWorking(string label)
        => FilingWords.Running(label).Should().Be("Working…");

    // ---- RefusalWords (note 3) ----

    [Theory]
    [InlineData("decline", true, "Declined", "Not declined.")]
    [InlineData("return", false, "Draft", "Not returned.")]
    [InlineData("record_discussion", true, "Discussed", "Not discussed.")]
    [InlineData("sign_off", true, "Signed off", "Not signed off.")]
    [InlineData("complete", true, "Completed", "Not completed.")]
    [InlineData("submit", false, "Requested", "Not submitted.")]
    public void NotDone_SaysWhatDidNotHappen(string key, bool ends, string target, string words)
        => RefusalWords.NotDone(Action(key, ends, target)).Should().Be(words);

    [Fact]
    public void NotDone_ForAMoveThatLeadsOn_WithNoWordForIt_NamesTheMove()
        => RefusalWords.NotDone(Action("verify", false, "Verified")).Should().Be("Verify was not made.");

    [Theory]
    [InlineData("decline", true, "Declined", "Requested", "Not declined. It is still Requested.")]
    [InlineData("return", false, "Draft", "Awaiting discussion", "Not returned. It is still Awaiting discussion.")]
    [InlineData("return", false, "Draft", "Awaiting review", "Not returned. It is still Awaiting review.")]
    [InlineData("sign_off", true, "Signed off", "Awaiting supervisor", "Not signed off. It is still Awaiting supervisor.")]
    public void ForNote_SaysWhatDidNotHappen_AndTheStateItIsStillIn(
        string key, bool ends, string target, string current, string title)
        => RefusalWords.ForNote(Action(key, ends, target), current).Should().Be(title);

    [Fact]
    public void ForNote_ReadsADraftAsADraft_AsForMoveDoes()
        => RefusalWords.ForNote(Action("return", false, "Draft"), "Draft").Should().Be("Not returned. It is still a draft.");

    [Fact]
    public void ForMove_StillSaysNotReturned_ForAReturnRefusedForItsFields()
        => RefusalWords.ForMove(Action("return", false, "Draft"), "Awaiting review", 1).Title
            .Should().Be("Not returned. It is still Awaiting review.");

    // ---- NotePanelWords (note 4; round 1, E2) ----

    [Theory]
    [InlineData("request", "request")]
    [InlineData("reflection", "reflection")]
    [InlineData("review request", "review")]
    [InlineData("  review  request ", "review")]
    [InlineData("", "activity")]
    [InlineData(null, "activity")]
    public void ThePart_IsTheAuthorPartsFirstWord(string? authorPart, string part)
        => NotePanelWords.PartOf(authorPart).Should().Be(part);

    [Theory]
    [InlineData("decline", true, "Declined", "request", "Decline this request", "Keep the request", "Decline with this note", "btn-danger")]
    [InlineData("return", false, "Draft", "reflection", "Return this reflection", "Keep the reflection", "Return with this note", "btn-primary")]
    [InlineData("return", false, "Draft", "review", "Return this review", "Keep the review", "Return with this note", "btn-primary")]
    public void ThePanel_IsOnePattern_ForEveryMoveThatNeedsANote(
        string key, bool ends, string target, string part, string heading, string keep, string send, string sendClass)
    {
        var action = Action(key, ends, target, requiresNote: true);

        NotePanelWords.Heading(action, part).Should().Be(heading);
        NotePanelWords.Keep(part).Should().Be(keep);
        NotePanelWords.Send(action).Should().Be(send);
        NotePanelWords.SendClass(action).Should().Be(sendClass, "danger only for a move that ends the activity");
    }

    [Fact]
    public void TheNote_IsForTheRegistrar_WhoReadsItOnThePage()
    {
        NotePanelWords.Label("Sipho Ndlovu").Should().Be("Note for Sipho Ndlovu");
        NotePanelWords.Help("Sipho Ndlovu").Should()
            .Be("Sipho Ndlovu reads it on the activity's page. It is kept with the activity's history.");
    }

    private static ActivityActionDto Action(string key, bool ends, string target, bool requiresNote = false)
        => new(key, requiresNote) { TargetIsFinal = ends, TargetIsTerminal = ends, TargetStateLabel = target };
}
