using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, lane B, Q2): the workflow in words as R2-B04, R2-D02 and R3-D05 draw it, from read DTOs built by hand
/// (never through the editor query): the states on one line, then "Moves, as saved", each move by its label with From,
/// To, Who moves it, Checks and Note; a rule the words could not read shown as written, in code.
/// </summary>
public sealed class WorkflowReadoutTests : TestContext
{
    [Fact]
    public void KgksTwoMoves_ReadAsSaved()
    {
        var cut = Render(new ActivityTypeWorkflowReadDto(
            [State("draft", "Draft", initial: true), State("logged", "Logged", terminal: true), State("cancelled", "Cancelled")],
            [
                Move("log", "Log", ["Draft"], "Logged", "The registrar, or whoever filed it", "The whole form"),
                Move("cancel", "Cancel", ["Draft"], "Cancelled", "The registrar, or whoever filed it", "Formats only")
            ]));

        cut.Find("div.workflow-readout").Should().NotBeNull();
        Text(cut.Find("div.workflow-readout > p")).Should().Be("Draft, where a new one starts · Logged, final · Cancelled");
        Text(cut.Find("caption")).Should().Be("Moves, as saved");
        cut.FindAll("thead th").Select(Text).Should().Equal("Move", "From", "To", "Who moves it", "Checks", "Note");
        cut.Find("table").ClassList.Should().Contain("clinic-table--stack-wide");
        cut.FindAll("tbody th[scope=row]").Select(Text).Should().Equal("Log", "Cancel");
        Cells(cut.FindAll("tbody tr").ElementAt(0)).Should().Equal("Draft", "Logged", "The registrar, or whoever filed it", "The whole form", "No");
        Cells(cut.FindAll("tbody tr").ElementAt(1)).Should().Equal("Draft", "Cancelled", "The registrar, or whoever filed it", "Formats only", "No");
        cut.FindAll("tbody td").Should().OnlyContain(cell => cell.HasAttribute("data-label"));
        cut.FindAll("h2, h3, h4").Should().BeEmpty("the builder's panel h2 heads it; the boards draw no heading of its own");
    }

    [Fact]
    public void MiniCexsFourMoves_ReadAsSaved_WithAMoveFromTwoStates_AndARequiredNote()
    {
        var cut = Render(new ActivityTypeWorkflowReadDto(
            [
                State("draft", "Draft", initial: true), State("requested", "Requested"), State("completed", "Completed", terminal: true),
                State("declined", "Declined"), State("cancelled", "Cancelled")
            ],
            [
                Move("submit", "Submit", ["Draft"], "Requested", "The registrar, or whoever filed it", "The mover's own fields"),
                Move("complete", "Complete", ["Requested"], "Completed", "The person Assessor names", "The whole form"),
                Move("decline", "Decline", ["Requested"], "Declined", "The person Assessor names", "Formats only", requiresNote: true),
                Move("cancel", "Cancel", ["Draft", "Requested"], "Cancelled", "The registrar, or whoever filed it", "Formats only")
            ]));

        Text(cut.Find("div.workflow-readout > p")).Should().Be(
            "Draft, where a new one starts · Requested · Completed, final · Declined · Cancelled");
        cut.FindAll("tbody tr").Should().HaveCount(4);
        Text(cut.FindAll("tbody tr").ElementAt(2).QuerySelector("td[data-label=Note]")!).Should().Be("Required");
        Text(cut.FindAll("tbody tr").ElementAt(3).QuerySelector("td[data-label=From]")!).Should().Be("Draft or Requested");
    }

    [Fact]
    public void AMoveFromThreeStates_ListsThemWithCommasAndOr()
        => ActivityTypeReadWords.From(["Draft", "Requested", "Returned"]).Should().Be("Draft, Requested or Returned");

    [Fact]
    public void ARuleTheWordsCouldNotRead_IsShownAsWritten_InCode()
    {
        var cut = Render(new ActivityTypeWorkflowReadDto(
            [State("draft", "Draft", initial: true), State("done", "Done", terminal: true)],
            [Move("finish", "Finish", ["Draft"], "Done", "subject+scope:galaxy", "The whole form") with { WhoAsWritten = "subject+scope:galaxy" }]));

        var who = cut.Find("tbody td[data-label='Who moves it']");
        who.QuerySelector("code")!.TextContent.Should().Be("subject+scope:galaxy");
    }

    private IRenderedComponent<WorkflowReadout> Render(ActivityTypeWorkflowReadDto read)
        => RenderComponent<WorkflowReadout>(parameters => parameters.Add(component => component.Read, read));

    private static ActivityTypeStateReadDto State(string key, string label, bool initial = false, bool terminal = false)
        => new(key, label, initial, terminal);

    private static ActivityTypeMoveReadDto Move(
        string key, string label, string[] from, string to, string who, string checks, bool requiresNote = false)
        => new(key, label, from, to, who, checks, requiresNote);

    private static List<string> Cells(IElement row) => row.QuerySelectorAll("td").Select(Text).ToList();

    private static string Text(IElement element) => element.TextContent.Trim();
}
