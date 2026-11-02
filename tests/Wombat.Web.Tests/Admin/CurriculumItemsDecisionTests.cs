using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T131 slice 2, on the items list and an item's page of its own since T392 (flow 15, Q14): the item editor shows and edits each item's entrustment-decision cells, the cadence, the body
/// that decides, and whether it is decided as opportunity allows.
/// </summary>
/// <remarks>
/// Both silent failures are permissive-looking. A cadence select with no empty option would load a null cadence as its
/// first option and save it; a form that dropped the body would send EPAs 4 and 5 back to the general panel on an unchanged
/// save. The command takes all three positionally; these tests pin the page that fills them.
/// </remarks>
public sealed class CurriculumItemsDecisionTests : CurriculumItemsTestBase
{
    private const string NeonatalName = "Neonatal team Clinical Competency Committee";

    public CurriculumItemsDecisionTests()
    {
        SignIn(WombatRoles.Administrator);
    }

    // ---- The table ----

    [Fact]
    public void TheTargetCell_SaysHowEachItemIsDecided_AndByWhom()
    {
        var cut = RenderList(Catalogue());

        cut.FindAll("thead th").Select(header => header.TextContent.Trim()).Should().Contain("Target and decision")
            .And.HaveCount(6, "the decision shares the target's cell (T131), in six columns (decision 1)");

        DecisionLine(cut, "PAED-001").Should().Be($"Decided each semester, by the {NeonatalName}");
        DecisionLine(cut, "PAED-002").Should().Be("Decided each academic year, as opportunity allows");
        DecisionLine(cut, "PAED-003").Should().Be("No decision cadence",
            "null is 'no published cadence', never shown as the zero value's 'each academic year'");
    }

    // ---- The edit form ----

    [Fact]
    public void OpeningAnItem_ShowsItsDecision_AndSavingItUnchanged_SendsItBack()
    {
        var sender = Catalogue();
        var cut = RenderItem(sender, 11);

        SelectedValue(cut, "#item-decision-cadence").Should().Be(nameof(QuotaPeriod.Semester));
        SelectedValue(cut, "#item-decision-body").Should().Be("neonatal");
        cut.Find("#item-decision-body").QuerySelectorAll("option").Select(option => option.TextContent.Trim())
            .Should().Equal("The trainee's general panel", NeonatalName);

        cut.Find("form").Submit();

        var command = sender.Updates.Should().ContainSingle().Subject;
        (command.DecisionCadence, command.DecisionBodyKey, command.DecisionIsOpportunistic)
            .Should().Be(((QuotaPeriod?)QuotaPeriod.Semester, (string?)"neonatal", false));
    }

    [Fact]
    public void AnItemWithNoCadence_OpensOnNoCadence_AndSavesNull()
    {
        var sender = Catalogue();
        var cut = RenderItem(sender, 13);

        SelectedValue(cut, "#item-decision-cadence").Should().BeEmpty();
        cut.Find("#item-decision-opportunistic").HasAttribute("disabled").Should().BeTrue("with no cadence the flag says nothing");

        cut.Find("form").Submit();

        var command = sender.Updates.Should().ContainSingle().Subject;
        command.DecisionCadence.Should().BeNull("an unchanged save must not turn 'no cadence' into the zero value");
        command.DecisionBodyKey.Should().BeNull();
    }

    [Fact]
    public void ClearingTheCadence_ClearsAndDisablesTheOpportunisticBox_SoTheSaveIsNotRefused()
    {
        var sender = Catalogue();
        var cut = RenderItem(sender, 12);
        cut.Find("#item-decision-opportunistic").HasAttribute("checked").Should().BeTrue("guard: PAED-002 starts opportunistic");

        cut.Find("#item-decision-cadence").Change(string.Empty);

        var box = cut.Find("#item-decision-opportunistic");
        box.HasAttribute("checked").Should().BeFalse();
        box.HasAttribute("disabled").Should().BeTrue();

        cut.Find("form").Submit();

        var command = sender.Updates.Should().ContainSingle().Subject;
        (command.DecisionCadence, command.DecisionIsOpportunistic).Should().Be(((QuotaPeriod?)null, false));
    }

    [Fact]
    public void ChoosingTheGeneralPanel_SendsNoBody()
    {
        var sender = Catalogue();
        var cut = RenderItem(sender, 11);
        cut.Find("#item-decision-body").Change(string.Empty);
        cut.Find("form").Submit();

        sender.Updates.Should().ContainSingle().Which.DecisionBodyKey.Should().BeNull("a blank select value is no body, not an empty key");
    }

    // ---- The Add form ----

    [Fact]
    public void TheAddForm_StartsOnNoCadence_TheGeneralPanel_AndAnUntickedDisabledBox()
    {
        var cut = RenderItem(Catalogue());

        SelectedValue(cut, "#item-decision-cadence").Should().BeEmpty("no default anybody chose");
        SelectedValue(cut, "#item-decision-body").Should().BeEmpty();
        var box = cut.Find("#item-decision-opportunistic");
        box.HasAttribute("checked").Should().BeFalse();
        box.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void AddingAnItem_SendsTheChosenDecision()
    {
        var sender = Catalogue();
        var cut = RenderItem(sender);

        cut.Find("#item-decision-cadence").Change(nameof(QuotaPeriod.AcademicYear));
        cut.Find("#item-decision-opportunistic").HasAttribute("disabled").Should().BeFalse("a cadence enables it");
        cut.Find("#item-decision-opportunistic").Change(true);
        cut.Find("#item-decision-body").Change("neonatal");
        cut.Find("#item-minimum").Change("3");
        cut.Find("form").Submit();

        var command = sender.Adds.Should().ContainSingle().Subject;
        (command.DecisionCadence, command.DecisionBodyKey, command.DecisionIsOpportunistic)
            .Should().Be(((QuotaPeriod?)QuotaPeriod.AcademicYear, (string?)"neonatal", true));
    }

    [Fact]
    public void AddingAnItemWithNothingChosen_SendsNoCadenceAndNoBody()
    {
        var sender = Catalogue();
        var cut = RenderItem(sender);

        cut.Find("#item-minimum").Change("3");
        cut.Find("form").Submit();

        var command = sender.Adds.Should().ContainSingle().Subject;
        (command.DecisionCadence, command.DecisionBodyKey, command.DecisionIsOpportunistic)
            .Should().Be(((QuotaPeriod?)null, (string?)null, false));
    }

    // ---- helpers ----

    // Unpinned items, so the Add form's minimum is a typed level.
    private static FakeSender Catalogue()
        => new(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, null, decisionCadence: QuotaPeriod.Semester, decisionBodyKey: "neonatal"),
            FakeSender.Item(12, 2, 1, QuotaPeriod.AcademicYear, null, decisionCadence: QuotaPeriod.AcademicYear, decisionIsOpportunistic: true),
            FakeSender.Item(13, 3, 2, QuotaPeriod.Semester, null)
        ]);

    private static string DecisionLine(IRenderedFragment cut, string epaCode)
        => Text(Cell(cut, epaCode, "Target and decision").QuerySelector("div")!);

    // The option a select shows. Blazor renders a select's value as `selected` on the matching option.
    private static string SelectedValue(IRenderedFragment cut, string selector)
        => cut.Find(selector).QuerySelectorAll("option")
            .Where(option => option.HasAttribute("selected"))
            .Select(option => option.GetAttribute("value"))
            .SingleOrDefault() ?? string.Empty;
}
