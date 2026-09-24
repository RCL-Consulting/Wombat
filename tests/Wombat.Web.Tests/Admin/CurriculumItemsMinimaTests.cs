using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Curricula;
using Wombat.Domain.Curricula;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Services;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T125 (with T136 folded in): a curriculum minimum is picked as a rung on the item's ladder, never typed as a bare
/// ordinal against a ladder the administrator cannot see.
/// </summary>
/// <remarks>
/// <para>
/// A minimum is the target every trainee on the curriculum is measured against, and an ordinal means a different rung
/// on each ladder: 4 is "Independent" on the O-R Scale and "3b" on v11.1. So the picker offers the rungs by name, a
/// scale change empties every minimum rather than carrying a number across, and the read-mode cells print the rung.
/// </para>
/// <para>
/// Every failure here is silent. A carried ordinal changes its meaning without a word; a dropped year-5 key or an
/// entry the editor cannot show would change the item on an unchanged save; a refusal shown only at the top of a long
/// page reads as a Save button that does nothing (T136).
/// </para>
/// </remarks>
public sealed class CurriculumItemsMinimaTests : TestContext
{
    private static readonly string[] CpsaRungs = ["1", "2", "3a", "3b", "4", "5"];

    public CurriculumItemsMinimaTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
    }

    // PAED-001 as the catalogue seeds it: pinned to v11.1, exit standard rung "5" (Order 6), the Annexure A year curve.
    private static FakeSender Catalogue(Exception? updateFailure = null, int? subSpecialityDefaultScaleId = null)
        => new(
        [
            FakeSender.Item(21, 1, 3, QuotaPeriod.Semester, null, 6, """{"1":3,"2":4,"3":5,"4":6}""", FakeSender.CpsaScale),
            FakeSender.Item(22, 2, 3, QuotaPeriod.AcademicYear, null, 4),
            FakeSender.Item(23, 3, 2, QuotaPeriod.Semester, null, 4, """{"1":2}""", FakeSender.OrScale)
        ])
        {
            Scales = [FakeSender.CpsaScale, FakeSender.OrScale],
            UpdateFailure = updateFailure,
            SubSpecialityDefaultScaleId = subSpecialityDefaultScaleId
        };

    // Two items, both on v11.1, and no sub-speciality default: the Add form suggests v11.1 because the siblings share it.
    private static FakeSender SharedLadderCatalogue()
        => new(
        [
            FakeSender.Item(21, 1, 3, QuotaPeriod.Semester, null, 6, null, FakeSender.CpsaScale),
            FakeSender.Item(22, 2, 3, QuotaPeriod.Semester, null, 5, null, FakeSender.CpsaScale)
        ])
        {
            Scales = [FakeSender.CpsaScale, FakeSender.OrScale]
        };

    // ---- The picker ----

    [Fact]
    public void AV11Item_OffersTheSixRungsByTheirCollegeLabels_ValuedByOrder_AndStoresTheOrdinal()
    {
        var sender = Catalogue();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");

        var options = cut.FindAll("#edit-minimum option").ToList();
        options[0].GetAttribute("value").Should().BeEmpty("the first option is the empty 'choose a rung'");
        options.Skip(1).Select(option => option.TextContent.Trim()).Should().Equal(CpsaRungs,
            "the College's rungs, by the names it prints");
        options.Skip(1).Select(option => option.GetAttribute("value")).Should().Equal(["1", "2", "3", "4", "5", "6"],
            "each rung's value is its Order, which is what is stored and compared");
        SelectedValue(cut, "#edit-minimum").Should().Be("6", "the stored exit standard, shown as rung 5");

        // Every year gets the same ladder.
        cut.FindAll("#edit-stage-1 option").Skip(1).Select(option => option.TextContent.Trim()).Should().Equal(CpsaRungs);
        SelectedValue(cut, "#edit-stage-1").Should().Be("3");

        cut.Find("#edit-minimum").Change("4");
        ClickButton(cut, "Save");

        sender.Updates.Should().ContainSingle().Which.MinimumLevelOrder.Should().Be(4, "3b is stored as its Order");
    }

    [Fact]
    public void AnUnpinnedItem_KeepsATypedLevel()
    {
        // No ladder, no rungs to offer (T109: unpinned is a meaningful state, not a gap to fill).
        var sender = Catalogue();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-002");

        var input = cut.Find("#edit-minimum");
        input.TagName.Should().BeEquivalentTo("input");
        input.GetAttribute("type").Should().Be("number");
        input.GetAttribute("value").Should().Be("4");

        input.Change("5");
        ClickButton(cut, "Save");

        sender.Updates.Should().ContainSingle().Which.MinimumLevelOrder.Should().Be(5);
    }

    // ---- A scale change ----

    [Fact]
    public void ChangingTheScale_EmptiesEveryMinimumPicker_AndDisablesSave_UntilEachIsRePicked()
    {
        // T136's reproduction, done through the page: PAED-001 re-pinned from v11.1 to the O-R Scale. Its Order 6 is
        // not a rung there and its Order 4 would silently turn from "3b" into "Independent", so nothing is carried.
        var sender = Catalogue();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");
        SaveButton(cut).HasAttribute("disabled").Should().BeFalse("an unchanged item can be saved as it is");

        // The status region is there, empty, before anything is cleared: a screen reader announces a change to a live
        // region it already knows, not reliably one that arrives already filled. Save points at it.
        var status = cut.Find("#edit-minima-status");
        status.GetAttribute("role").Should().Be("status");
        status.TextContent.Trim().Should().BeEmpty();
        SaveButton(cut).GetAttribute("aria-describedby").Should().Be("edit-minima-status");

        cut.Find("#edit-scale").Change(FakeSender.OrScale.Id.ToString());

        SelectedValue(cut, "#edit-minimum").Should().BeEmpty("the flat minimum is not carried to the new ladder");
        foreach (var year in new[] { 1, 2, 3, 4 })
        {
            SelectedValue(cut, $"#edit-stage-{year}").Should().BeEmpty($"year {year}'s minimum is not carried either");
        }

        cut.FindAll("#edit-minimum option").Skip(1).Select(option => option.TextContent.Trim()).Should().Equal(
            "Observe only", "Direct supervision", "Indirect supervision", "Independent", "Supervises others");
        SaveButton(cut).HasAttribute("disabled").Should().BeTrue();
        cut.Find("#edit-minima-status").TextContent.Should().Contain(
            "The scale is now O-R Scale, so the minima picked on the previous ladder have been cleared");
        cut.Find("#edit-minimum").GetAttribute("aria-invalid").Should().Be("false",
            "an empty picker is required, not wrong; the status says why saving is off");

        // Re-picking some is not enough.
        cut.Find("#edit-minimum").Change("4");
        cut.Find("#edit-stage-1").Change("2");
        cut.Find("#edit-stage-2").Change("3");
        cut.Find("#edit-stage-3").Change("3");
        SaveButton(cut).HasAttribute("disabled").Should().BeTrue("year 4 still has no rung");

        cut.Find("#edit-stage-4").Change("4");
        SaveButton(cut).HasAttribute("disabled").Should().BeFalse();
        ClickButton(cut, "Save");

        var command = sender.Updates.Should().ContainSingle().Subject;
        command.ScaleId.Should().Be(FakeSender.OrScale.Id);
        command.MinimumLevelOrder.Should().Be(4);
        CurriculumItem.ParseStageOverrides(command.MinimumLevelByStageJson).Should().Equal(
            new Dictionary<int, int> { [1] = 2, [2] = 3, [3] = 3, [4] = 4 });
    }

    // ---- The scale round trip ----

    [Fact]
    public void ReturningToTheLadderTheMinimaWerePickedOn_BringsThemBack_AndNothingCrossesLadders()
    {
        // On Windows each arrow key on a closed select is a change, so Down then Up on the Scale cell is O-R and back.
        // That must not wipe PAED-001's year curve: what was picked on a ladder comes back on that ladder, and only
        // there. An edit made before leaving comes back too, not the stored value.
        var sender = Catalogue();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");
        cut.Find("#edit-minimum").Change("5");

        cut.Find("#edit-scale").Change(FakeSender.OrScale.Id.ToString());
        SelectedValue(cut, "#edit-minimum").Should().BeEmpty();
        cut.Find("#edit-minimum").Change("2");

        cut.Find("#edit-scale").Change(FakeSender.CpsaScale.Id.ToString());
        SelectedValue(cut, "#edit-minimum").Should().Be("5", "the edit made on v11.1 before leaving it");
        new[] { 1, 2, 3, 4 }.Select(year => SelectedValue(cut, $"#edit-stage-{year}")).Should().Equal("3", "4", "5", "6");
        SaveButton(cut).HasAttribute("disabled").Should().BeFalse();
        cut.Find("#edit-minima-status").TextContent.Trim().Should().BeEmpty("nothing is missing, so nothing is said");

        // And the O-R pick waits on O-R, not on v11.1.
        cut.Find("#edit-scale").Change(FakeSender.OrScale.Id.ToString());
        SelectedValue(cut, "#edit-minimum").Should().Be("2");
        SelectedValue(cut, "#edit-stage-1").Should().BeEmpty("no year was picked on O-R");

        cut.Find("#edit-scale").Change(FakeSender.CpsaScale.Id.ToString());
        ClickButton(cut, "Save");

        var command = sender.Updates.Should().ContainSingle().Subject;
        command.ScaleId.Should().Be(FakeSender.CpsaScale.Id);
        command.MinimumLevelOrder.Should().Be(5);
        CurriculumItem.ParseStageOverrides(command.MinimumLevelByStageJson).Should().Equal(
            new Dictionary<int, int> { [1] = 3, [2] = 4, [3] = 5, [4] = 6 });
    }

    // ---- The year-by-year editor ----

    [Fact]
    public void AYearBeyondFour_SurvivesAnUnchangedSave_AsJsonTheRealValidatorAccepts()
    {
        // The rows are the item's own keys, not years 1 to 4, so year 5 is a row like any other and an unchanged save
        // writes it back. Checked against the command's real validator, not only the fake sender, which skips it.
        var sender = new FakeSender(
        [
            FakeSender.Item(21, 1, 3, QuotaPeriod.Semester, null, 6, """{"1":3,"5":6}""", FakeSender.CpsaScale)
        ])
        { Scales = [FakeSender.CpsaScale, FakeSender.OrScale] };
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");

        cut.FindAll("[id^='edit-stage-']").Select(element => element.Id).Should().Equal("edit-stage-1", "edit-stage-5");

        ClickButton(cut, "Save");

        var command = sender.Updates.Should().ContainSingle().Subject;
        CurriculumItem.ParseStageOverrides(command.MinimumLevelByStageJson).Should().Equal(
            new Dictionary<int, int> { [1] = 3, [5] = 6 });
        new UpdateCurriculumItemCommandValidator().Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void AStoredEntryThatIsNotAYearAndARung_IsListed_AndSaveIsOffUntilItIsRemoved()
    {
        // "final" is not a training year. The editor does not drop it (that would change the item unasked), and it
        // does not offer a save the validator is bound to refuse, whatever else the operator changed: it lists the
        // entry, says saving is off until it is removed, and waits.
        var sender = new FakeSender(
        [
            FakeSender.Item(21, 1, 3, QuotaPeriod.Semester, null, 6, """{"1":3,"5":6,"final":6}""", FakeSender.CpsaScale)
        ])
        { Scales = [FakeSender.CpsaScale, FakeSender.OrScale] };
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");

        cut.Find("tbody td[colspan]").TextContent.Should().Contain("final");
        SaveButton(cut).HasAttribute("disabled").Should().BeTrue();
        cut.Find("#edit-minima-status").TextContent.Should().Contain(
            "Saving is off until each stored entry under Minimum by training year that is not a year and a rung is removed.");

        cut.FindAll("button").First(button => button.GetAttribute("aria-label") == "Remove the stored entry final").Click();
        SaveButton(cut).HasAttribute("disabled").Should().BeFalse();
        ClickButton(cut, "Save");

        var command = sender.Updates.Should().ContainSingle().Subject;
        CurriculumItem.ParseStageOverrides(command.MinimumLevelByStageJson).Should().Equal(
            new Dictionary<int, int> { [1] = 3, [5] = 6 });

        // Why the page holds back: the validator refuses the entry, so a save carrying it could only ever fail.
        var validator = new UpdateCurriculumItemCommandValidator();
        validator.Validate(command).IsValid.Should().BeTrue();
        validator.Validate(command with { MinimumLevelByStageJson = """{"final":6,"1":3,"5":6}""" }).IsValid.Should().BeFalse();
    }

    [Fact]
    public void AddYear_AddsTheNextYearEmpty_AndRemoveYear_DropsOne()
    {
        var sender = Catalogue();
        var cut = RenderPage(sender);

        BeginEdit(cut, "PAED-001");
        ClickButton(cut, "Add year 5");

        SelectedValue(cut, "#edit-stage-5").Should().BeEmpty("a new year starts with no rung");
        SaveButton(cut).HasAttribute("disabled").Should().BeTrue("the new year has no rung yet");

        cut.Find("#edit-stage-5").Change("6");
        cut.FindAll("button").First(button => button.GetAttribute("aria-label") == "Remove year 1").Click();
        ClickButton(cut, "Save");

        CurriculumItem.ParseStageOverrides(sender.Updates.Should().ContainSingle().Subject.MinimumLevelByStageJson)
            .Should().Equal(new Dictionary<int, int> { [2] = 4, [3] = 5, [4] = 6, [5] = 6 });
    }

    // ---- T136: the refusal ----

    [Fact]
    public void ARefusedSave_ShowsTheRefusalBesideTheEditedRow()
    {
        // The server's refusal (T109) stays; the pickers are guidance. When it fires, it is shown in the sub-row under
        // the row being edited, which is where the operator is looking, and the row stays open.
        const string refusal = "Minimum level 6 (rung 5) is not a rung on O-R Scale, which has 5 rungs.";
        var cut = RenderPage(Catalogue(new InvalidOperationException(refusal)));

        BeginEdit(cut, "PAED-002");
        ClickButton(cut, "Save");

        var editRow = cut.Find("#edit-minimum").Closest("tr")!;
        var subRow = editRow.NextElementSibling!;
        subRow.QuerySelector("td[colspan]").Should().NotBeNull("the refusal belongs to the edited item's own sub-row");
        subRow.QuerySelector("[role=alert]")!.TextContent.Trim().Should().Be(refusal);
    }

    // ---- Read mode ----

    [Fact]
    public void ReadMode_ShowsEachMinimumAsTheRungItIsOnItsLadder()
    {
        var cut = RenderPage(Catalogue());

        Cell(cut, "PAED-001", "Minimum level").Should().Be("5", "Order 6 is rung 5 on v11.1, not 'level 6'");
        CellLines(cut, "PAED-001", "Minimum by training year").Should().Equal(
            "Year 1: 3a", "Year 2: 3b", "Year 3: 4", "Year 4: 5");

        Cell(cut, "PAED-003", "Minimum level").Should().Be("Independent");
        CellLines(cut, "PAED-003", "Minimum by training year").Should().Equal("Year 1: Direct supervision");

        // Unpinned: no ladder, so the ordinal, as before.
        Cell(cut, "PAED-002", "Minimum level").Should().Be("4");
        Cell(cut, "PAED-002", "Minimum by training year").Should().Be("—");
    }

    [Fact]
    public void AStoredValueThatIsNotARungOnTheItemsLadder_IsShownAsSuch_AndCannotBeSavedUntilReplaced()
    {
        // Only the seeder's boot-time pin or direct SQL can store this: an item on the five-rung O-R Scale whose
        // minimum is 6. Read mode and the picker both say so rather than print a bare 6 (on a ladder named "1" to "5"
        // that would read as a real rung), and the item cannot be saved as it is.
        var sender = new FakeSender([FakeSender.Item(23, 3, 2, QuotaPeriod.Semester, null, 6, """{"1":6}""", FakeSender.OrScale)])
        {
            Scales = [FakeSender.CpsaScale, FakeSender.OrScale]
        };
        var cut = RenderPage(sender);

        Cell(cut, "PAED-003", "Minimum level").Should().Be("6 (not a rung on O-R Scale)");
        CellLines(cut, "PAED-003", "Minimum by training year").Should().Equal("Year 1: 6 (not a rung on O-R Scale)");

        BeginEdit(cut, "PAED-003");

        var picker = cut.Find("#edit-minimum");
        SelectedValue(cut, "#edit-minimum").Should().Be("6", "what is stored is shown, so it can be seen and replaced");
        picker.QuerySelectorAll("option").Single(option => option.HasAttribute("selected")).TextContent.Trim()
            .Should().Be("6 (not a rung on O-R Scale)");
        picker.GetAttribute("aria-invalid").Should().Be("true");
        picker.ClassList.Should().Contain("input-validation-error");
        SaveButton(cut).HasAttribute("disabled").Should().BeTrue("a value off the ladder does not count as picked");

        cut.Find("#edit-minimum").Change("4");
        SaveButton(cut).HasAttribute("disabled").Should().BeTrue("year 1 is still off the ladder");
        cut.Find("#edit-stage-1").Change("2");
        SaveButton(cut).HasAttribute("disabled").Should().BeFalse();
        cut.Find("#edit-minimum").GetAttribute("aria-invalid").Should().Be("false");
    }

    // ---- The Add form ----

    [Fact]
    public void TheAddForm_StartsOnTheLadderEveryItemShares_WithTheMinimumEmpty_AndWaitsForARung()
    {
        var sender = new FakeSender(
        [
            FakeSender.Item(21, 1, 3, QuotaPeriod.Semester, null, 6, null, FakeSender.CpsaScale),
            FakeSender.Item(22, 2, 3, QuotaPeriod.Semester, null, 5, null, FakeSender.CpsaScale)
        ])
        { Scales = [FakeSender.CpsaScale, FakeSender.OrScale], SubSpecialityDefaultScaleId = FakeSender.OrScale.Id };
        var cut = RenderPage(sender);

        SelectedValue(cut, "#curriculum-item-scale").Should().Be(FakeSender.CpsaScale.Id.ToString(),
            "the siblings agree, and that outranks the sub-speciality default");
        SelectedValue(cut, "#curriculum-item-level").Should().BeEmpty("not '4', which is 3b on this ladder");
        cut.Find("#curriculum-item-level").GetAttribute("aria-invalid").Should().Be("false",
            "an untouched required picker is not an error");
        cut.FindAll("#curriculum-item-level option").Skip(1).Select(option => option.TextContent.Trim()).Should().Equal(CpsaRungs);
        AddButton(cut).HasAttribute("disabled").Should().BeTrue();

        cut.Find("#curriculum-item-level").Change("5");
        AddButton(cut).HasAttribute("disabled").Should().BeFalse();
        cut.Find("form").Submit();

        var command = sender.Adds.Should().ContainSingle().Subject;
        command.ScaleId.Should().Be(FakeSender.CpsaScale.Id);
        command.MinimumLevelOrder.Should().Be(5);
        command.MinimumLevelByStageJson.Should().BeNull();
    }

    [Fact]
    public void TheAddForm_FallsBackToTheSubSpecialityDefault_WhenTheItemsDoNotShareALadder()
    {
        // Catalogue() mixes v11.1, unpinned and O-R items.
        var cut = RenderPage(Catalogue(subSpecialityDefaultScaleId: FakeSender.CpsaScale.Id));

        SelectedValue(cut, "#curriculum-item-scale").Should().Be(FakeSender.CpsaScale.Id.ToString());
    }

    [Fact]
    public void TheAddForm_IsUnpinned_WhenNothingSuggestsALadder()
    {
        var cut = RenderPage(Catalogue());

        SelectedValue(cut, "#curriculum-item-scale").Should().BeEmpty();
        cut.Find("#curriculum-item-level").GetAttribute("type").Should().Be("number");
        cut.Find("#curriculum-item-level").GetAttribute("value").Should().BeEmpty();
    }

    [Fact]
    public void ChangingTheAddFormsScale_EmptiesTheMinimum_AndDisablesAdd_UntilItIsRePicked()
    {
        // The Add form's half of T125's hazard: 3b picked on v11.1 is Order 4, which is "Independent" on the O-R Scale.
        var sender = SharedLadderCatalogue();
        var cut = RenderPage(sender);

        cut.Find("#curriculum-item-level").Change("4");
        AddButton(cut).HasAttribute("disabled").Should().BeFalse();
        var status = cut.Find("#add-minima-status");
        status.GetAttribute("role").Should().Be("status", "a live region the screen reader knows before it is filled");
        status.TextContent.Trim().Should().BeEmpty();

        cut.Find("#curriculum-item-scale").Change(FakeSender.OrScale.Id.ToString());

        SelectedValue(cut, "#curriculum-item-level").Should().BeEmpty("3b is not carried to the O-R Scale as 'Independent'");
        AddButton(cut).HasAttribute("disabled").Should().BeTrue();
        AddButton(cut).GetAttribute("aria-describedby").Should().Be("add-minima-status");
        cut.Find("#add-minima-status").TextContent.Should().Contain(
            "The scale is now O-R Scale, so the minima picked on the previous ladder have been cleared");
        cut.Find("form").TextContent.Should().NotContain("Suggested because", "the operator has picked another ladder");

        cut.Find("#curriculum-item-scale").Change(FakeSender.CpsaScale.Id.ToString());
        SelectedValue(cut, "#curriculum-item-level").Should().Be("4", "3b comes back on the ladder it was picked on");
        cut.Find("#add-minima-status").TextContent.Trim().Should().BeEmpty();
    }

    [Fact]
    public void ARefusedAdd_ShowsTheRefusalInsideTheAddForm()
    {
        const string refusal = "Minimum level 6 is not a rung on O-R Scale, which has 5 rungs.";
        var sender = new FakeSender(
        [
            FakeSender.Item(21, 1, 3, QuotaPeriod.Semester, null, 6, null, FakeSender.CpsaScale)
        ])
        {
            Scales = [FakeSender.CpsaScale, FakeSender.OrScale],
            AddFailure = new InvalidOperationException(refusal)
        };
        var cut = RenderPage(sender);

        cut.Find("#curriculum-item-level").Change("6");
        cut.Find("form").Submit();

        sender.Adds.Should().ContainSingle();
        var alert = cut.FindAll("[role=alert]").Should().ContainSingle(
            "shown where the operator is looking, not also at the top of the page").Subject;
        alert.Closest("form").Should().NotBeNull();
        alert.TextContent.Trim().Should().Be(refusal);
        SelectedValue(cut, "#curriculum-item-level").Should().Be("6", "a refusal keeps what the operator picked");
    }

    [Fact]
    public void TheAddFormsReason_FollowsTheItems_WhenAnEditMeansTheyNoLongerShareALadder()
    {
        // "Suggested because every item is pinned to it" is only true while it is. Re-pinning a sibling makes it false,
        // and the reason goes; the form keeps the ladder it shows, which the operator can still change.
        var sender = SharedLadderCatalogue();
        var cut = RenderPage(sender);

        cut.Find("form").TextContent.Should().Contain("Suggested because every item on this curriculum is pinned to it.");

        BeginEdit(cut, "PAED-002");
        cut.Find("#edit-scale").Change(FakeSender.OrScale.Id.ToString());
        cut.Find("#edit-minimum").Change("4");
        ClickButton(cut, "Save");
        sender.Updates.Should().ContainSingle();

        cut.Find("form").TextContent.Should().NotContain("Suggested because");
        SelectedValue(cut, "#curriculum-item-scale").Should().Be(FakeSender.CpsaScale.Id.ToString());
    }

    // ---- helpers ----

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    // The option a select shows. Blazor renders a select's value as `selected` on the matching option.
    private static string? SelectedValue(IRenderedComponent<CurriculumItemsEdit> cut, string selector)
        => cut.Find(selector).QuerySelectorAll("option")
            .Where(option => option.HasAttribute("selected"))
            .Select(option => option.GetAttribute("value"))
            .SingleOrDefault() ?? string.Empty;

    private static IReadOnlyList<string> HeaderTexts(IRenderedComponent<CurriculumItemsEdit> cut)
        => cut.FindAll("thead th").Select(header => header.TextContent.Trim()).ToList();

    private static IElement DisplayRow(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelector("td[colspan]") is null && row.QuerySelector("#edit-minimum") is null)
            .First(row => row.QuerySelectorAll("td").FirstOrDefault()?.TextContent.Trim().StartsWith(epaCode, StringComparison.Ordinal) == true);

    private static IElement CellElement(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode, string header)
        => DisplayRow(cut, epaCode).QuerySelectorAll("td").ElementAt(HeaderTexts(cut).ToList().IndexOf(header));

    private static string Cell(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode, string header)
        => CellElement(cut, epaCode, header).TextContent.Trim();

    private static IReadOnlyList<string> CellLines(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode, string header)
        => CellElement(cut, epaCode, header).QuerySelectorAll("li").Select(item => item.TextContent.Trim()).ToList();

    private static void BeginEdit(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => DisplayRow(cut, epaCode).QuerySelectorAll("button")
            .First(button => button.TextContent.Trim() == "Edit")
            .Click();

    private static IElement SaveButton(IRenderedComponent<CurriculumItemsEdit> cut)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == "Save");

    private static IElement AddButton(IRenderedComponent<CurriculumItemsEdit> cut)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == "Add item");

    private static void ClickButton(IRenderedComponent<CurriculumItemsEdit> cut, string label)
        => cut.FindAll("button")
            .First(button => string.Equals(button.TextContent.Trim(), label, StringComparison.Ordinal))
            .Click();
}
