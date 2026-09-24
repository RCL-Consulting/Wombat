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
/// T131 slice 2: the curriculum item editor shows and edits each item's entrustment-decision cells, the cadence, the body
/// that decides, and whether it is decided as opportunity allows.
/// </summary>
/// <remarks>
/// Both silent failures are permissive-looking. A cadence select with no empty option would load a null cadence as its
/// first option and save it; a form that dropped the body would send EPAs 4 and 5 back to the general panel on an unchanged
/// save. The command takes all three positionally; these tests pin the page that fills them.
/// </remarks>
public sealed class CurriculumItemsDecisionTests : TestContext
{
    private const string NeonatalName = "Neonatal team Clinical Competency Committee";

    public CurriculumItemsDecisionTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
    }

    // ---- The table ----

    [Fact]
    public void TheTargetCell_SaysHowEachItemIsDecided_AndByWhom()
    {
        var cut = RenderPage(Catalogue());

        cut.FindAll("thead th").Select(header => header.TextContent.Trim()).Should().Contain("Target and decision")
            .And.HaveCount(9, "the decision shares the target's cell: a tenth column would not fit at 1280px (T176)");

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
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");

        SelectedValue(cut, "#edit-decision-cadence").Should().Be(nameof(QuotaPeriod.Semester));
        SelectedValue(cut, "#edit-decision-body").Should().Be("neonatal");
        cut.Find("#edit-decision-body").QuerySelectorAll("option").Select(option => option.TextContent.Trim())
            .Should().Equal("The trainee's general panel", NeonatalName);

        ClickButton(cut, "Save");

        var command = sender.Updates.Should().ContainSingle().Subject;
        (command.DecisionCadence, command.DecisionBodyKey, command.DecisionIsOpportunistic)
            .Should().Be(((QuotaPeriod?)QuotaPeriod.Semester, (string?)"neonatal", false));
    }

    [Fact]
    public void AnItemWithNoCadence_OpensOnNoCadence_AndSavesNull()
    {
        var sender = Catalogue();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-003");

        SelectedValue(cut, "#edit-decision-cadence").Should().BeEmpty();
        cut.Find("#edit-decision-opportunistic").HasAttribute("disabled").Should().BeTrue("with no cadence the flag says nothing");

        ClickButton(cut, "Save");

        var command = sender.Updates.Should().ContainSingle().Subject;
        command.DecisionCadence.Should().BeNull("an unchanged save must not turn 'no cadence' into the zero value");
        command.DecisionBodyKey.Should().BeNull();
    }

    [Fact]
    public void ClearingTheCadence_ClearsAndDisablesTheOpportunisticBox_SoTheSaveIsNotRefused()
    {
        var sender = Catalogue();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-002");
        cut.Find("#edit-decision-opportunistic").HasAttribute("checked").Should().BeTrue("guard: PAED-002 starts opportunistic");

        cut.Find("#edit-decision-cadence").Change(string.Empty);

        var box = cut.Find("#edit-decision-opportunistic");
        box.HasAttribute("checked").Should().BeFalse();
        box.HasAttribute("disabled").Should().BeTrue();

        ClickButton(cut, "Save");

        var command = sender.Updates.Should().ContainSingle().Subject;
        (command.DecisionCadence, command.DecisionIsOpportunistic).Should().Be(((QuotaPeriod?)null, false));
    }

    [Fact]
    public void ChoosingTheGeneralPanel_SendsNoBody()
    {
        var sender = Catalogue();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");
        cut.Find("#edit-decision-body").Change(string.Empty);
        ClickButton(cut, "Save");

        sender.Updates.Should().ContainSingle().Which.DecisionBodyKey.Should().BeNull("a blank select value is no body, not an empty key");
    }

    // ---- The Add form ----

    [Fact]
    public void TheAddForm_StartsOnNoCadence_TheGeneralPanel_AndAnUntickedDisabledBox()
    {
        var cut = RenderPage(Catalogue());

        SelectedValue(cut, "#curriculum-item-decision-cadence").Should().BeEmpty("no default anybody chose");
        SelectedValue(cut, "#curriculum-item-decision-body").Should().BeEmpty();
        var box = cut.Find("#curriculum-item-decision-opportunistic");
        box.HasAttribute("checked").Should().BeFalse();
        box.HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void AddingAnItem_SendsTheChosenDecision()
    {
        var sender = Catalogue();
        var cut = RenderPage(sender);

        cut.Find("#curriculum-item-decision-cadence").Change(nameof(QuotaPeriod.AcademicYear));
        cut.Find("#curriculum-item-decision-opportunistic").HasAttribute("disabled").Should().BeFalse("a cadence enables it");
        cut.Find("#curriculum-item-decision-opportunistic").Change(true);
        cut.Find("#curriculum-item-decision-body").Change("neonatal");
        cut.Find("#curriculum-item-level").Change("3");
        cut.Find("form").Submit();

        var command = sender.Adds.Should().ContainSingle().Subject;
        (command.DecisionCadence, command.DecisionBodyKey, command.DecisionIsOpportunistic)
            .Should().Be(((QuotaPeriod?)QuotaPeriod.AcademicYear, (string?)"neonatal", true));
    }

    [Fact]
    public void AddingAnItemWithNothingChosen_SendsNoCadenceAndNoBody()
    {
        var sender = Catalogue();
        var cut = RenderPage(sender);

        cut.Find("#curriculum-item-level").Change("3");
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

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    private static IElement ItemRow(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelector("td[colspan]") is null)
            .Single(row => row.QuerySelector("td")!.TextContent.Trim().StartsWith(epaCode, StringComparison.Ordinal));

    private static string DecisionLine(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
    {
        var headers = cut.FindAll("thead th").Select(header => header.TextContent.Trim()).ToList();
        var cell = ItemRow(cut, epaCode).QuerySelectorAll("td")[headers.IndexOf("Target and decision")];
        return cell.QuerySelector("div")!.TextContent.Trim();
    }

    private static void BeginEdit(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => ItemRow(cut, epaCode).QuerySelectorAll("button")
            .First(button => button.TextContent.Trim() == "Edit")
            .Click();

    private static void ClickButton(IRenderedComponent<CurriculumItemsEdit> cut, string label)
        => cut.FindAll("button")
            .First(button => string.Equals(button.TextContent.Trim(), label, StringComparison.Ordinal))
            .Click();

    // The option a select shows. Blazor renders a select's value as `selected` on the matching option.
    private static string SelectedValue(IRenderedComponent<CurriculumItemsEdit> cut, string selector)
        => cut.Find(selector).QuerySelectorAll("option")
            .Where(option => option.HasAttribute("selected"))
            .Select(option => option.GetAttribute("value"))
            .SingleOrDefault() ?? string.Empty;
}
