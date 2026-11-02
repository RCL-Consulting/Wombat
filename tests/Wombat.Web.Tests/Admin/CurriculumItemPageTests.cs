using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Application.Common;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Shared;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// An item's page of its own (T392, flow 15, Q14, F1, F3, F4, P2, P6; R3-B22, B24, R2-B04, B05, B09, B12, B15) and the list's
/// half of the one-shot tokens it returns by (R3-B06, B10, B11, B23): the add and the edit by caller, the tokens and their
/// focus, a stale token, Item unavailable, the refusal under EPA, Cancel, the national item's record, the load error.
/// </summary>
public sealed class CurriculumItemPageTests : CurriculumItemsTestBase
{
    private const int InstitutionA = 40;

    // ---- the add ----

    [Fact]
    public void TheCollegesAddPage_IsAddItem_OnTheCurriculum_ForEveryInstitution()
    {
        // F3, R3-B22.
        var cut = RenderItem(new FakeSender { EpaList = FakeSender.MoreEpas });

        cut.Find("h1").TextContent.Trim().Should().Be("Add item");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(
            "On Paediatrics v11.1 (11.1). It applies to every institution that has adopted it.");
        Crumbs(cut).Should().Equal("Home", "Curricula", "Items of Paediatrics v11.1 (11.1)", "Add item");
        cut.Find("nav[aria-label=Breadcrumb] li.breadcrumb-parent a").GetAttribute("href").Should().Be("/admin/curricula/5/items");
        Text(cut.Find("form button[type=submit]")).Should().Be("Add item");
    }

    [Fact]
    public void AnInstitutionsAddPage_IsItsOwnItem_ForItsTraineesAlone()
    {
        // R2-B04.
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderItem(new FakeSender
        {
            CanEditCurriculum = false,
            Institution = new CurriculumInstitutionStateDto(InstitutionA, "Kgosi Kgari Teaching Hospital", true, new DateOnly(2026, 10, 30), null, null, null, 3, 3, 0, []),
            EpaList = [FakeSender.Epa(41, "KGK-001")]
        });

        cut.Find("h1").TextContent.Trim().Should().Be("Add your institution's own item");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be(
            "On Paediatrics v11.1 (11.1). It applies only to Kgosi Kgari Teaching Hospital's trainees.");
        cut.FindAll("#item-epa option").Select(Text).Should().Equal("KGK-001 — KGK-001 title");
    }

    [Fact]
    public void Cancel_OnTheAddPage_ReturnsToTheList_WithNoToken()
        => RenderItem(new FakeSender { EpaList = FakeSender.MoreEpas })
            .FindAll(".form-actions a").Single(link => Text(link) == "Cancel").GetAttribute("href").Should().Be("/admin/curricula/5/items",
                "P6: an add has no item to return to");

    [Fact]
    public void AddItem_SendsTheItem_AndReturnsToTheList_ByAddedWithTheNewItemsId()
    {
        // F1, M6: the list writes the result itself.
        var sender = new FakeSender { EpaList = FakeSender.MoreEpas, StoresItems = true };
        var cut = RenderItem(sender);

        cut.Find("#item-minimum").Change("3");
        cut.Find("form").Submit();

        sender.Adds.Should().ContainSingle().Which.EpaId.Should().Be(4);
        Navigation.Uri.Should().EndWith("/admin/curricula/5/items?added=14");
    }

    [Fact]
    public void AddItem_IsOff_UntilEveryMinimumIsARung_AndSaysWhy_WithoutNativeDisabled()
    {
        // R2-B04: aria-disabled with its reason beside it; a press sends nothing.
        var sender = new FakeSender { EpaList = FakeSender.MoreEpas };
        var cut = RenderItem(sender);

        var submit = cut.Find("form button[type=submit]");
        submit.HasAttribute("disabled").Should().BeFalse();
        submit.GetAttribute("aria-disabled").Should().Be("true");
        submit.GetAttribute("aria-describedby").Should().Be("item-minima-status");
        Text(cut.Find("#item-minima-status")).Should().Be("Adding is off until every minimum is a level from 1 to 20.");

        cut.Find("form").Submit();
        sender.Adds.Should().BeEmpty();
    }

    [Fact]
    public void ARefusalOfTheEpa_StaysUnderEpa_Focused_TheValuesKept()
    {
        // F4, R2-B05: a page out of date, the EPA added elsewhere since.
        var sender = new FakeSender
        {
            EpaList = FakeSender.MoreEpas,
            AddFailure = new FieldRefusalException("EpaId", "This curriculum already contains the selected EPA.")
        };
        var cut = RenderItem(sender);
        cut.Find("#item-count").Change("4");
        cut.Find("#item-minimum").Change("3");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find("#item-epa-refusal")).Should().Be("This curriculum already contains the selected EPA."));
        cut.Find("#item-epa-refusal").GetAttribute("role").Should().Be("alert");
        cut.Find("#item-epa").GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#item-epa").GetAttribute("aria-describedby")!.Split(' ')[0].Should().Be("item-epa-refusal", "the refusal first (N17)");
        cut.Find("#item-count").GetAttribute("value").Should().Be("4", "what was typed is kept");
        FocusedIds().Should().Contain("item-epa");
        sender.EpaOptionQueries.Should().HaveCount(2, "the picker is asked again after its EPA was refused (T222 review)");
        cut.FindAll(".alert-danger").Should().BeEmpty("a field's refusal is under its field, not in the alert");
    }

    [Fact]
    public void AFailureNoFieldNames_IsFixedWords_AtThePagesTop_NeverTheExceptionsText()
    {
        // T329.
        var sender = new FakeSender { EpaList = FakeSender.MoreEpas, AddFailure = new InvalidOperationException("Npgsql exploded: 23505") };
        var cut = RenderItem(sender);
        cut.Find("#item-minimum").Change("3");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "The item could not be added. Nothing has changed. Try again, or come back in a few minutes."));
        cut.Markup.Should().NotContain("Npgsql");
        FocusCalls().Should().ContainSingle("the failure takes the focus");
    }

    [Fact]
    public void TheAddPage_ForACallerWhoMayNotAdd_IsItemUnavailable()
    {
        // F6: no add on a superseded version.
        SignIn(WombatRoles.InstitutionalAdmin);
        var sender = new FakeSender { CanEditCurriculum = false, MayAddItem = false };
        var cut = RenderItem(sender);

        cut.Find("h1").TextContent.Trim().Should().Be("Item unavailable");
        sender.EpaOptionQueries.Should().BeEmpty();
        cut.FindAll("form").Should().BeEmpty();
    }

    [Theory]
    [InlineData("#item-count", "item-count", "Enter a target of 1 or more.")]
    [InlineData("#item-window", "item-window", "Enter a completion window of 1 month or more.")]
    public void ATargetOrWindowBelowOne_IsRefusedUnderItsField_InPlainWords_AndNothingIsSent(string input, string id, string words)
    {
        // M1 (T392 build review): never FluentValidation's "'Required Count' must be greater than '0'.".
        var sender = new FakeSender { EpaList = FakeSender.MoreEpas };
        var cut = RenderItem(sender);
        cut.Find("#item-minimum").Change("3");
        cut.Find(input).Change("0");

        cut.Find("form").Submit();

        sender.Adds.Should().BeEmpty();
        Text(cut.Find($"#{id}-refusal")).Should().Be(words);
        cut.Find(input).GetAttribute("aria-invalid").Should().Be("true");
        cut.Find(input).GetAttribute("aria-describedby")!.Split(' ')[0].Should().Be($"{id}-refusal");
        FocusedIds().Should().Contain(id);
        cut.Markup.Should().NotContain("Required Count").And.NotContain("Window Months");
    }

    [Fact]
    public void TheCommandsTargetRule_SaysTheSameWords()
    {
        // M1: the validators' words are the page's, so a refusal that reaches the command reads the same.
        var add = new AddCurriculumItemCommand(5, 4, 0, QuotaPeriod.Semester, 3, 0, null, null, null, null, null, false, new System.Security.Claims.ClaimsPrincipal());
        var errors = new AddCurriculumItemCommandValidator().Validate(add).Errors;

        errors.Select(error => error.ErrorMessage).Should().BeEquivalentTo(
            "Enter a target of 1 or more.", "Enter a completion window of 1 month or more.");
    }

    [Fact]
    public void TheAddPage_OnAVersionTheInstitutionHasSuperseded_SaysSo_AndOffersTheListBack()
    {
        // P3: not "does not exist".
        SignIn(WombatRoles.InstitutionalAdmin);
        var sender = new FakeSender
        {
            CanEditCurriculum = false,
            MayAddItem = false,
            Institution = new CurriculumInstitutionStateDto(InstitutionA, "Kgosi Kgari Teaching Hospital", false, new DateOnly(2026, 1, 10),
                new DateOnly(2026, 10, 30), "Paediatrics v11.2", "11.2", 3, 15, 1, [])
        };
        var cut = RenderItem(sender);

        cut.Find("h1").TextContent.Trim().Should().Be("Cannot add an item");
        Text(cut.Find(".state-panel-copy")).Should().Be(
            "Your institution has superseded this version, so no item is added to it. Its own items stay for the registrars still on it.");
        Text(cut.Find(".detail-card--empty a")).Should().Be("Go to Items of Paediatrics v11.1 (11.1)");
        sender.EpaOptionQueries.Should().BeEmpty();
        cut.FindAll("form").Should().BeEmpty();
    }

    [Fact]
    public void AnAddRefusedBecauseTheVersionWasSupersededMeanwhile_SaysTheCommandsWords_AtThePagesTop()
    {
        // C2.
        var sender = new FakeSender { EpaList = FakeSender.MoreEpas, AddFailure = new CurriculumVersionSupersededException() };
        var cut = RenderItem(sender);
        cut.Find("#item-minimum").Change("3");

        cut.Find("form").Submit();

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "Your institution has superseded this version, so no item is added to it. Nothing has changed."));
        FocusCalls().Should().ContainSingle("the refusal takes the focus");
    }

    [Fact]
    public void TheAddPage_WithNothingLeftToAdd_SaysSo_AndOffersTheListBack()
    {
        var cut = RenderItem(new FakeSender { EpaList = [] });

        cut.FindAll("form").Should().BeEmpty();
        Text(cut.Find(".state-panel-copy")).Should().StartWith("Every national EPA of General Paediatrics is already on this curriculum");
        Text(cut.Find(".detail-card--empty a")).Should().Be("Go to Items of Paediatrics v11.1 (11.1)");
    }

    // ---- the edit ----

    [Fact]
    public void TheEditPage_IsTitledByItsEpa_ItsCrumbTheCode_AndCancelCarriesBack()
    {
        // R2-B09: an institution's own item.
        SignIn(WombatRoles.InstitutionalAdmin);
        var sender = new FakeSender([FakeSender.Item(13, 3, 1, QuotaPeriod.AcademicYear, null, owningInstitutionId: InstitutionA)])
        {
            CanEditCurriculum = false
        };
        var cut = RenderItem(sender, 13);

        cut.Find("h1").TextContent.Trim().Should().Be("Edit PAED-003 — PAED-003 title");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("Your institution's own item on Paediatrics v11.1 (11.1)");
        Crumbs(cut).Should().Equal("Home", "Curricula", "Items of Paediatrics v11.1 (11.1)", "PAED-003");
        cut.FindAll(".form-actions a").Single(link => Text(link) == "Cancel").GetAttribute("href").Should().Be("/admin/curricula/5/items?back=13");
        Text(cut.Find("form button[type=submit]")).Should().Be("Save item");
        sender.EpaOptionQueries.Should().ContainSingle().Which.ItemId.Should().Be(13);
    }

    [Fact]
    public void TheCollegesEditPage_OfANationalItem_SaysItAppliesToEveryInstitution()
    {
        // R3-B24.
        var cut = RenderItem(new FakeSender(), 11);

        cut.Find("h1").TextContent.Trim().Should().Be("Edit PAED-001 — PAED-001 title");
        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("On Paediatrics v11.1 (11.1). It applies to every institution that has adopted it.");
    }

    [Fact]
    public void SaveItem_SendsTheItem_AndReturnsToTheList_BySaved()
    {
        var sender = new FakeSender();
        var cut = RenderItem(sender, 12);

        cut.Find("#item-count").Change("2");
        cut.Find("form").Submit();

        var update = sender.Updates.Should().ContainSingle().Subject;
        update.ItemId.Should().Be(12);
        update.RequiredCount.Should().Be(2);
        Navigation.Uri.Should().EndWith("/admin/curricula/5/items?saved=12");
    }

    [Fact]
    public void AnItemTheCurriculumDoesNotHold_IsItemUnavailable_UnderCurricula_WithTheWayBack()
    {
        // R2-B15: no id shown.
        var cut = RenderItem(new FakeSender(), 999);

        cut.Find("h1").TextContent.Trim().Should().Be("Item unavailable");
        Crumbs(cut).Should().Equal("Home", "Curricula", "Item unavailable");
        Text(cut.Find(".state-panel-copy")).Should().Be("This curriculum item does not exist, or you cannot change it.");
        var link = cut.Find(".detail-card--empty a");
        Text(link).Should().Be("Go to Items of Paediatrics v11.1 (11.1)");
        link.GetAttribute("href").Should().Be("/admin/curricula/5/items");
        cut.Markup.Should().NotContain("999");
    }

    [Fact]
    public void ACurriculumOutOfScope_IsCurriculumUnavailable()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var cut = RenderItem(new FakeSender { Unavailable = true }, 11);

        cut.Find("h1").TextContent.Trim().Should().Be("Curriculum unavailable");
        Text(cut.Find(".detail-card--empty a")).Should().Be("Go to Curricula");
    }

    // ---- the record (P2) ----

    [Fact]
    public void ANationalItem_ACallerMayNotChange_IsItsRecord_NoFormNoSave_ItsBackCarryingTheToken()
    {
        SignIn(WombatRoles.InstitutionalAdmin);
        var scale = FakeSender.CpsaScale;
        var sender = new FakeSender(
        [
            FakeSender.Item(11, 1, 3, QuotaPeriod.Semester, """["cbd","mini_cex"]""", minimumLevelOrder: 6,
                stageMinimaJson: """{"1":3,"2":4}""", scale: scale, decisionCadence: QuotaPeriod.Semester, canEdit: false)
        ])
        {
            CanEditCurriculum = false,
            Scales = [scale]
        };
        var cut = RenderItem(sender, 11);

        cut.Find("h1").TextContent.Trim().Should().Be("PAED-001 — PAED-001 title");
        Crumbs(cut).Last().Should().Be("PAED-001");
        cut.FindAll("form").Should().BeEmpty();
        cut.FindAll("button[type=submit]").Should().BeEmpty();
        var record = cut.FindAll("dl.detail-list dt").Select(Text).ToList();
        record.Should().Equal("EPA", "Target and decision", "Minimum level", "Minimum by training year", "Completion window", "Weight", "Tools");
        var values = cut.FindAll("dl.detail-list dd").Select(Text).ToList();
        values[1].Should().Be("3 per semesterDecided each semester", "the decision is the target's second line");
        values[2].Should().Be("5CPSA Paediatric Entrustment Scale v11.1");
        cut.FindAll("dl.detail-list dd li").Select(Text).Should().Equal("Year 1: 3a", "Year 2: 3b");
        values[4].Should().Be("12 months");
        values[6].Should().Be("Case-based discussion, Mini-CEX");
        var back = cut.Find(".form-actions a");
        Text(back).Should().Be("Back to Items of Paediatrics v11.1 (11.1)");
        back.GetAttribute("href").Should().Be("/admin/curricula/5/items?back=11");
        sender.EpaOptionQueries.Should().BeEmpty("a record offers no picker");
    }

    // ---- loading and failure ----

    [Fact]
    public void ALoadFailure_IsFixedWords_WithTryAgain_NoForm_TitledAsThePage()
    {
        var sender = new FakeSender { ReadFailure = new InvalidOperationException("boom"), ReadFailures = 1 };
        var cut = RenderItem(sender, 11);

        cut.Find("h1").TextContent.Trim().Should().Be("Curriculum item");
        Text(cut.Find(".action-result .alert-danger .alert-row-text")).Should().Be(
            "Could not load this curriculum item. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("boom");
        cut.FindAll("form").Should().BeEmpty();

        cut.Find(".action-result button").Click();
        cut.WaitForAssertion(() => cut.FindAll("form").Should().ContainSingle());
    }

    // ---- the list's half of the tokens (F1) ----

    [Theory]
    [InlineData("added", "Curriculum item added.")]
    [InlineData("saved", "Curriculum item saved.")]
    public void TheListReadsItsToken_WritesTheResult_FocusesIt_AndStripsTheToken(string token, string result)
    {
        var cut = RenderList(new FakeSender(), $"/admin/curricula/5/items?{token}=12");

        cut.WaitForAssertion(() => Text(cut.Find(".action-result .alert-success")).Should().Be(result));
        cut.Find(".action-result .alert-success strong").TextContent.Should().Be(result);
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle());
        cut.WaitForAssertion(() => Navigation.Uri.Should().EndWith("/admin/curricula/5/items"));
        Text(cut.Find(".action-result .alert-success")).Should().Be(result, "the strip reads nothing again");
    }

    [Fact]
    public void BackFromAnItem_FocusesItsRowsEdit_AndSaysNothing()
    {
        var cut = RenderList(new FakeSender(), "/admin/curricula/5/items?back=12");

        cut.WaitForAssertion(() => FocusedIds().Should().Equal("item-12-open"));
        cut.FindAll(".action-result .alert").Should().BeEmpty();
        cut.Find("#item-12-open").TextContent.Trim().Should().Be("Edit");
        cut.WaitForAssertion(() => Navigation.Uri.Should().EndWith("/admin/curricula/5/items"));
    }

    [Theory]
    [InlineData("added=999")]
    [InlineData("saved=999")]
    [InlineData("back=999")]
    [InlineData("added=x")]
    public void AStaleOrForeignToken_WritesNothing_FocusesNothing_AndIsStripped(string token)
    {
        var cut = RenderList(new FakeSender(), $"/admin/curricula/5/items?{token}");

        cut.WaitForAssertion(() => Navigation.Uri.Should().EndWith("/admin/curricula/5/items"));
        cut.FindAll(".action-result .alert").Should().BeEmpty();
        FocusCalls().Should().BeEmpty();
        FocusedIds().Should().BeEmpty();
    }

    private static IReadOnlyList<string> Crumbs(IRenderedFragment cut)
        => cut.FindAll("nav[aria-label=Breadcrumb] li").Select(Text).ToList();
}
