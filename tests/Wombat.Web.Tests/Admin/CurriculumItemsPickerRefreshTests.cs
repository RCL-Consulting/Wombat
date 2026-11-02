using Bunit;
using FluentAssertions;
using Wombat.Application.Common;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T222 and its review, on the items list and an item's page of its own since T392 (flow 15, Q14, F4): the add is offered
/// only where an EPA is left to add, which the picker query decides as the Add command does; with none left the list says
/// so in the built words (by caller, F3); and a refusal of the EPA, likeliest from a page out of date, asks the picker
/// again, keeps the refusal under EPA, and puts it at the head of the empty state when nothing is left.
/// </summary>
public sealed class CurriculumItemsPickerRefreshTests : CurriculumItemsTestBase
{
    public CurriculumItemsPickerRefreshTests()
    {
        SignIn(WombatRoles.Administrator);
    }

    [Theory]
    [InlineData(true,
        "Every national EPA of General Paediatrics is already on this curriculum, as a national item or as an institution's own item, so there is none left to add. To add an item, first add a national EPA of General Paediatrics, or remove a national item to free its EPA. An EPA an institution has as its own item is freed only when that institution removes it.")]
    [InlineData(false,
        "Every EPA your institution could add is already on this curriculum: each national EPA of General Paediatrics, and each of your institution's own EPAs of General Paediatrics. To add an item of your own, first create a local EPA of General Paediatrics.")]
    public void TheList_WithNothingLeftToAdd_SaysSo_InPlaceOfTheAddLink(bool canEditCurriculum, string emptyText)
    {
        var cut = RenderList(new FakeSender { CanEditCurriculum = canEditCurriculum, EpaList = [] });

        cut.FindAll(".form-section-head a").Should().BeEmpty("no add with nothing to add");
        Text(cut.Find(".curriculum-items-page > p.muted")).Should().Be(emptyText);
    }

    [Fact]
    public void TheList_WithSomethingToAdd_OffersTheAdd_AndNoEmptyLine()
    {
        var cut = RenderList(new FakeSender { EpaList = FakeSender.MoreEpas });

        Text(cut.Find(".form-section-head a")).Should().Be("Add item");
        cut.FindAll(".curriculum-items-page > p.muted").Should().BeEmpty();
    }

    [Fact]
    public void ARefusedAdd_OfAnEpaAddedElsewhere_AsksThePickerAgain_SoItNoLongerOffersIt_AndKeepsTheRefusal()
    {
        // The T222 review's stale page: PAED-004 was added in another tab since the picker was asked.
        var answers = new Queue<IReadOnlyList<EpaDto>>([FakeSender.MoreEpas, [FakeSender.MoreEpas[1]]]);
        var sender = new FakeSender
        {
            EpaOptionsFor = _ => answers.Count > 1 ? answers.Dequeue() : answers.Peek(),
            AddFailure = new FieldRefusalException("EpaId", "This curriculum already contains the selected EPA.")
        };
        var cut = RenderItem(sender);
        cut.Find("#item-minimum").Change("3");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.FindAll("#item-epa option").Select(option => option.GetAttribute("value")).Should().Equal("5"));
        cut.Find("#item-epa").GetAttribute("value").Should().Be("5", "a choice no longer offered moves to the first that is");
        Text(cut.Find("#item-epa-refusal")).Should().Be("This curriculum already contains the selected EPA.");
    }

    [Fact]
    public void ARefusedAdd_ThatLeavesNothingToAdd_PutsTheRefusalAtTheHeadOfTheEmptyState_WhichTakesTheFocus()
    {
        var answers = new Queue<IReadOnlyList<EpaDto>>([FakeSender.MoreEpas, []]);
        var sender = new FakeSender
        {
            EpaOptionsFor = _ => answers.Count > 1 ? answers.Dequeue() : answers.Peek(),
            AddFailure = new FieldRefusalException("EpaId", "This curriculum already contains the selected EPA.")
        };
        var cut = RenderItem(sender);
        cut.Find("#item-minimum").Change("3");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.FindAll("form").Should().BeEmpty());
        Text(cut.Find(".action-result .alert-danger")).Should().Be("This curriculum already contains the selected EPA.");
        Text(cut.Find(".state-panel-copy")).Should().StartWith("Every national EPA of General Paediatrics");
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle());
    }

    [Fact]
    public void ARefusedSave_OfAnEpaAddedElsewhere_GoesBackToTheItemsStoredEpa()
    {
        var answers = new Queue<IReadOnlyList<EpaDto>>([[.. FakeSender.Epas, .. FakeSender.MoreEpas], FakeSender.Epas]);
        var sender = new FakeSender
        {
            EpaOptionsFor = _ => answers.Count > 1 ? answers.Dequeue() : answers.Peek(),
            UpdateFailure = new FieldRefusalException("EpaId", "This curriculum already contains the selected EPA.")
        };
        var cut = RenderItem(sender, 11);
        cut.Find("#item-epa").Change("4");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.Find("#item-epa").GetAttribute("value").Should().Be("1"));
        Text(cut.Find("#item-epa-refusal")).Should().Be("This curriculum already contains the selected EPA.");
    }

    [Fact]
    public void WhileAnAddRuns_ASecondPressSendsNothing()
    {
        // T234: the submit stays a focusable button, aria-disabled while it runs.
        var sender = new FakeSender { EpaList = FakeSender.MoreEpas, HoldAdds = true, StoresItems = true };
        var cut = RenderItem(sender);
        cut.Find("#item-minimum").Change("3");

        cut.Find("form").Submit();
        cut.WaitForAssertion(() => sender.AddsSent.Should().Be(1));
        cut.Find("form button[type=submit]").GetAttribute("aria-disabled").Should().Be("true");
        cut.Find("form button[type=submit]").HasAttribute("disabled").Should().BeFalse();
        cut.Find("form").Submit();
        sender.AddsSent.Should().Be(1);

        sender.Release();
        cut.WaitForAssertion(() => Navigation.Uri.Should().Contain("?added="));
    }
}
