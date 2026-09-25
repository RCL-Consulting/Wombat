using System.Globalization;
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
using Wombat.Web.Tests.Accessibility;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T176: the curriculum item edit reads in full at a laptop's width, and every control on the page has a name.
/// </summary>
/// <remarks>
/// <para>
/// The edit used to put its controls in the item's own table cells. A select squeezed into a column showed a fragment
/// of its label ("CPSA", "Cho…", "per"), and Save and Cancel sat off-screen until the administrator scrolled sideways:
/// the table was 1018px in a 907px container, all but a few pixels of it from nine columns of cell padding and text.
/// Three of the controls had no name at all: the EPA select, the Completion window and the Weight.
/// </para>
/// <para>
/// A width cannot be measured in bUnit, so these tests pin the structure that makes the width right: the item's row
/// stays read-only, every control is in the row below it, whose one cell spans the table, and the table uses the
/// compact cell padding. The 1280px check itself is a browser check.
/// </para>
/// </remarks>
public sealed class CurriculumItemsEditLayoutTests : TestContext
{
    public CurriculumItemsEditLayoutTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
    }

    public enum Ladder
    {
        // PAED-001 as the catalogue seeds it, with a stored tool key the vocabulary no longer holds. Its siblings share
        // v11.1, so the Add form starts pinned too: both minimum pickers are selects.
        Pinned,

        // An unpinned item with a stored entry that is not a training year, among items on no shared ladder: both
        // minimum pickers are typed numbers, and the entry has its own Remove button.
        Unpinned
    }

    // ---- Every control has a name ----

    [Theory]
    [InlineData(Ladder.Pinned)]
    [InlineData(Ladder.Unpinned)]
    public void EveryControlOnThePage_HasAnAccessibleName_WithAnItemOpenForEdit(Ladder ladder)
    {
        var cut = RenderPage(Catalogue(ladder));

        BeginEdit(cut, "PAED-001");
        ClickButton(cut, "Add year 1"); // the Add form's, so its year pickers are checked too

        AccessibleNames.Unnamed(cut).Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty("a label pointing at no control names nothing");

        // The three that had none, and the rest of the edit, named by the labels the Add form uses.
        NameOf(cut, "edit-epa").Should().Be("EPA required");
        NameOf(cut, "edit-window").Should().Be("Completion window (months)");
        NameOf(cut, "edit-weight").Should().Be("Weight");
        NameOf(cut, "edit-count").Should().Be("Target");
        NameOf(cut, "edit-period").Should().Be("Per");
        NameOf(cut, "edit-scale").Should().Be("Entrustment scale");
        NameOf(cut, "edit-minimum").Should().Be("Minimum level required");
        NameOf(cut, "edit-stage-1").Should().Be("Year 1");
        NameOf(cut, "add-stage-1").Should().Be("Year 1");
        NameOf(cut, "curriculum-item-level").Should().Be("Minimum level required");

        cut.Find("#edit-minimum").TagName.Should().BeEquivalentTo(ladder == Ladder.Pinned ? "select" : "input");
        cut.Find("#curriculum-item-level").TagName.Should().BeEquivalentTo(ladder == Ladder.Pinned ? "select" : "input");
    }

    // ---- The layout ----

    [Fact]
    public void TheEditedItemsOwnRow_StaysReadOnly_AndEveryControlIsInTheFullWidthRowBelowIt()
    {
        var cut = RenderPage(Catalogue(Ladder.Pinned));

        BeginEdit(cut, "PAED-001");

        var itemRow = ItemRow(cut, "PAED-001");
        itemRow.QuerySelectorAll("input, select, textarea, button").Should().BeEmpty(
            "a control in a table cell is squeezed to the column's width and widens the table past its container");
        itemRow.QuerySelectorAll("td").Last().TextContent.Trim().Should().Be("Editing below");
        itemRow.QuerySelectorAll("td")[2].TextContent.Trim().Should().Be(FakeSender.CpsaScale.Name,
            "what is stored stays in view above what is being changed");

        var formRow = itemRow.NextElementSibling!;
        var cell = formRow.QuerySelectorAll("td").Should().ContainSingle().Subject;
        cell.GetAttribute("colspan").Should().Be(
            cut.FindAll("thead th").Count.ToString(CultureInfo.InvariantCulture), "the form spans the whole table");
        cell.QuerySelector("legend")!.TextContent.Trim().Should().Be("Edit PAED-001");

        foreach (var id in new[] { "edit-epa", "edit-count", "edit-period", "edit-scale", "edit-minimum", "edit-window", "edit-weight" })
        {
            cell.QuerySelector($"#{id}").Should().NotBeNull($"#{id} belongs to the edit form");
        }

        cell.QuerySelectorAll("button").Select(button => button.TextContent.Trim())
            .Should().Contain(new[] { "Cancel", "Save" });

        // Nothing else in the table is a control but the other items' Edit and Remove.
        cut.FindAll("tbody input, tbody select, tbody textarea")
            .Should().OnlyContain(control => control.Closest("tr") == formRow);
        cut.FindAll("tbody button").Where(button => button.Closest("tr") != formRow)
            .Select(button => button.TextContent.Trim())
            .Should().OnlyContain(label => label == "Edit" || label == "Remove");

        cut.FindAll("tbody tr.is-editing").Should().Equal(new[] { itemRow, formRow },
            "the stripe ties the item to its form, and marks no other row");
    }

    [Fact]
    public void TheEditForm_OffersTheAddFormsFields_InTheSameOrder_WithTheSameHelp()
    {
        // The two forms are written out twice. Before this held them together the edit form had drifted: its Tools
        // help was an older wording, and it had no help at all under the scale, though an edit is exactly when a
        // change of ladder clears the minima already stored.
        var cut = RenderPage(Catalogue(Ladder.Pinned));

        BeginEdit(cut, "PAED-001");

        var edit = FieldsOf(cut, cut.Find("tbody td[colspan] .form-grid"));
        var add = FieldsOf(cut, cut.Find(".form-container .form-grid"));

        edit.Should().Equal(add);
        edit.Select(field => field.Name).Should().Equal(
            "EPA required", "Target", "Per",
            // T131: Annexure B's decision cells follow its observation cells.
            "Decision cadence", "Decided by", "Opportunistic Decided as rotation or opportunity allows",
            "Entrustment scale", "Minimum level required",
            "Minimum by training year", "Completion window (months)", "Weight", "Tools");
        edit.Single(field => field.Name == "Decision cadence").Help.Should().StartWith("How often a committee takes the entrustment decision");
        edit.Single(field => field.Name == "Entrustment scale").Help.Should().StartWith("The ladder the minima are picked on.");
        edit.Single(field => field.Name == "Completion window (months)").Help.Should().StartWith("Only used to suggest");
    }

    [Theory]
    [InlineData(Ladder.Pinned)]
    [InlineData(Ladder.Unpinned)]
    public void EveryFieldOfBothForms_IsAFormGroup_StraightInTheGrid(Ladder ladder)
    {
        // T188. The minimum by training year and Tools were plain fieldsets among the fields, so each legend was a
        // section title (1.1rem, semibold) in a grid of labels, and the year list sat in a bare div.full-width. Every
        // field of either form is now a form group, a FormField's div or a group's fieldset, straight in the grid.
        var cut = RenderPage(Catalogue(ladder));

        BeginEdit(cut, "PAED-001");

        foreach (var grid in new[] { cut.Find("tbody td[colspan] .form-grid"), cut.Find(".form-container .form-grid") })
        {
            grid.Children.Should().OnlyContain(field => field.ClassList.Contains("form-group"),
                "a field of the form reads as one: its label or legend at a label's size");
            grid.Children.Where(field => field.LocalName == "fieldset")
                .Select(group => Collapse(group.QuerySelector("legend")!.TextContent))
                .Should().Equal(["Minimum by training year", "Tools"], "both groups are fields of their own");
        }
    }

    // ---- Every help is read with its field (T193) ----

    [Theory]
    [InlineData(Ladder.Pinned)]
    [InlineData(Ladder.Unpinned)]
    public void EveryFieldsHelp_IsNamedFirst_ByItsControl_OrByItsGroup(Ladder ladder)
    {
        // The help was written into FormField's slot as a bare <small>, which nothing named, so a screen reader read
        // "Target, spin button" and never what a target is.
        var cut = RenderPage(Catalogue(ladder));

        BeginEdit(cut, "PAED-001");

        foreach (var grid in new[] { cut.Find("tbody td[colspan] .form-grid"), cut.Find(".form-container .form-grid") })
        {
            var helped = HelpedFieldsOf(cut, grid);
            helped.Select(field => field.Name).Should().Equal(
                "EPA required", "Target", "Per", "Decision cadence", "Decided by",
                "Opportunistic Decided as rotation or opportunity allows", "Entrustment scale",
                "Minimum by training year", "Completion window (months)", "Tools");

            foreach (var (name, help, described) in helped)
            {
                help.Id.Should().NotBeNullOrEmpty($"the help of {name} has an id to be named by");
                DescribedBy(described).FirstOrDefault().Should().Be(help.Id, $"{name} names its help, first");
            }
        }

        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Theory]
    [InlineData(Ladder.Pinned, "curriculum-item-scale-help curriculum-item-scale-reason")]
    [InlineData(Ladder.Unpinned, "curriculum-item-scale-help")]
    public void TheAddFormsScale_NamesWhyItWasSuggested_WhileItIs(Ladder ladder, string describedBy)
    {
        // Pinned: every sibling shares v11.1, so the Add form starts on it and says why. Unpinned: no suggestion.
        var cut = RenderPage(Catalogue(ladder));

        cut.Find("#curriculum-item-scale").GetAttribute("aria-describedby").Should().Be(describedBy);
        if (ladder == Ladder.Pinned)
        {
            cut.Find("#curriculum-item-scale-reason").TextContent.Should().StartWith("Suggested because");
        }

        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheStandingWarning_IsNotALiveRegion()
    {
        // It is there on every visit. As a status it would be read out each time the page finished loading (T193).
        var cut = RenderPage(Catalogue(Ladder.Pinned));

        var warning = cut.Find(".alert.alert-warning");
        warning.TextContent.Should().Contain("A target and its period are read live.");
        warning.HasAttribute("role").Should().BeFalse();
    }

    [Fact]
    public void TheItemsTable_UsesTheCompactCellPadding()
    {
        // Measured on a static render of the fifteen v11.1 items with app.css (T176): the read-only rows alone needed
        // 1018px against a 907px container at 1280px, 288px of it cell padding. The compact padding brings the table to
        // 874px, so the form row below an item, and its Save, fit without scrolling sideways.
        var cut = RenderPage(Catalogue(Ladder.Pinned));

        cut.Find(".table-container table").ClassList.Should().Contain(new[] { "clinic-table", "clinic-table--compact" });
    }

    [Fact]
    public void TheEpaAndToolsColumns_WrapInsideAWord_RatherThanWidenTheTable()
    {
        // T198: with the compact padding the table had 33px to spare at 1280px (T176), its narrowest width being set by
        // two words, "neurodevelopmental" in the EPA column and "observation," in Tools. A longer title or tool name, or
        // another column, brought the sideways scroll back. Those two columns now give way first: they break inside a word
        // (overflow-wrap: anywhere, on every cell of the column) down to a floor their headers set, and the headers ask
        // for a share of the table. On a static render of the fifteen v11.1 items with app.css, the narrowest width went
        // from 883px to 794px, against the 907px container at 1280px. The price is that "neurodevelopmental" breaks just
        // below 1280px, where the table used to fit (DESIGN.md § Table system). The live 1280px check is a browser check.
        var cut = RenderPage(Catalogue(Ladder.Pinned));
        BeginEdit(cut, "PAED-001");

        var headers = cut.FindAll("thead th");
        headers.Select(header => (header.TextContent.Trim(), string.Join(' ', header.ClassList)))
            .Should().Equal(
                ("EPA", "col-wrap col-wrap--wide"), ("Target and decision", ""), ("Scale", ""), ("Minimum level", ""),
                ("Minimum by training year", ""), ("Completion window (months)", ""), ("Weight", ""), ("Tools", "col-wrap"),
                // Named for a screen reader only (T222), and not .col-actions, whose 12rem this table has no room for.
                ("Actions", ""));

        var itemRows = cut.FindAll("tbody tr").Where(row => row.QuerySelector("td[colspan]") is null).ToList();
        itemRows.Should().HaveCount(2);
        foreach (var row in itemRows)
        {
            row.Children.Select(cell => cell.ClassList.Contains("col-wrap"))
                .Should().Equal(true, false, false, false, false, false, false, true, false);
        }

        cut.Find("tbody td[colspan]").ClassList.Should().NotContain("col-wrap", "the edit form keeps its words whole");
        cut.FindAll("th[style], td[style]").Should().BeEmpty("a column's width is a class in app.css");
    }

    [Fact]
    public void TheReasonSaveIsOff_SitsBesideSave()
    {
        // It used to be in the sub-row's first line, above the year editor and the tool list, far from the button.
        var cut = RenderPage(Catalogue(Ladder.Pinned));

        BeginEdit(cut, "PAED-001");
        cut.Find("#edit-scale").Change(FakeSender.OrScale.Id.ToString(CultureInfo.InvariantCulture));

        var save = SaveButton(cut);
        save.HasAttribute("disabled").Should().BeTrue();

        var status = cut.Find("#edit-minima-status");
        status.TextContent.Should().Contain("The scale is now O-R Scale");
        status.ParentElement.Should().BeSameAs(save.ParentElement, "the reason is beside the button it explains");
        save.ParentElement!.ClassList.Should().Contain("form-actions");
        save.GetAttribute("aria-describedby").Should().Be("edit-minima-status");
    }

    [Fact]
    public void CancellingAnEdit_ReturnsTheRowToItsActions_AndRemovesTheForm()
    {
        var cut = RenderPage(Catalogue(Ladder.Pinned));

        BeginEdit(cut, "PAED-001");
        ClickButton(cut, "Cancel");

        cut.FindAll("tbody td[colspan]").Should().BeEmpty();
        cut.FindAll("tbody tr.is-editing").Should().BeEmpty();
        ItemRow(cut, "PAED-001").QuerySelectorAll("button").Select(button => button.TextContent.Trim())
            .Should().Equal("Edit", "Remove");
    }

    // ---- helpers ----

    private static FakeSender Catalogue(Ladder ladder)
        => ladder == Ladder.Pinned
            ? new FakeSender(
            [
                FakeSender.Item(21, 1, 3, QuotaPeriod.Semester, """["cbd","legacy_tool"]""", 6, """{"1":3,"2":4,"3":5,"4":6}""", FakeSender.CpsaScale),
                FakeSender.Item(22, 2, 3, QuotaPeriod.AcademicYear, null, 5, null, FakeSender.CpsaScale)
            ])
            { Scales = [FakeSender.CpsaScale, FakeSender.OrScale] }
            : new FakeSender(
            [
                FakeSender.Item(21, 1, 3, QuotaPeriod.Semester, null, 4, """{"1":3,"final":6}"""),
                FakeSender.Item(22, 2, 3, QuotaPeriod.AcademicYear, null, 4, null, FakeSender.OrScale)
            ])
            { Scales = [FakeSender.CpsaScale, FakeSender.OrScale] };

    private IRenderedComponent<CurriculumItemsEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => parameters.Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);

        return cut;
    }

    // The item's own row: the one whose first cell names it, not the form row below it.
    private static IElement ItemRow(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => cut.FindAll("tbody tr")
            .Where(row => row.QuerySelector("td[colspan]") is null)
            .Single(row => row.QuerySelector("td")!.TextContent.Trim().StartsWith(epaCode, StringComparison.Ordinal));

    private static void BeginEdit(IRenderedComponent<CurriculumItemsEdit> cut, string epaCode)
        => ItemRow(cut, epaCode).QuerySelectorAll("button")
            .First(button => button.TextContent.Trim() == "Edit")
            .Click();

    private static IElement SaveButton(IRenderedComponent<CurriculumItemsEdit> cut)
        => cut.FindAll("button").First(button => button.TextContent.Trim() == "Save");

    private static void ClickButton(IRenderedComponent<CurriculumItemsEdit> cut, string label)
        => cut.FindAll("button")
            .First(button => string.Equals(button.TextContent.Trim(), label, StringComparison.Ordinal))
            .Click();

    private static string NameOf(IRenderedComponent<CurriculumItemsEdit> cut, string id)
        => AccessibleNames.NameOf(cut, cut.Find($"#{id}"));

    /// <summary>
    /// A form grid's fields as the administrator reads them, in order: each one's name (its control's accessible name, or
    /// a group's legend) and the help printed with it.
    /// </summary>
    /// <remarks>
    /// Only the first help of a field is its help. The Add form's note on why it starts on a ladder comes before it and
    /// is the Add form's own (an edit starts on the item's stored ladder), and "No year has its own minimum." after the
    /// year list's help describes the item, not the field.
    /// </remarks>
    private static IReadOnlyList<(string Name, string? Help)> FieldsOf(IRenderedComponent<CurriculumItemsEdit> cut, IElement grid)
        => grid.Children.Select(field =>
        {
            var group = field.LocalName == "fieldset" ? field : field.QuerySelector("fieldset") ?? field;
            var title = group.QuerySelector("label, legend")!;
            var name = title.LocalName == "label"
                ? NameOf(cut, title.GetAttribute("for")!)
                : Collapse(title.TextContent);
            var help = HelpElementOf(group);
            return (name, help is null ? null : Collapse(help.TextContent));
        }).ToList();

    /// <summary>
    /// The fields of a form grid that carry help, each with its help element and the element that must name it: the
    /// control its label is for, or the fieldset its legend names.
    /// </summary>
    private static IReadOnlyList<(string Name, IElement Help, IElement Described)> HelpedFieldsOf(
        IRenderedComponent<CurriculumItemsEdit> cut, IElement grid)
        => grid.Children.Select(field =>
        {
            var group = field.LocalName == "fieldset" ? field : field.QuerySelector("fieldset") ?? field;
            var title = group.QuerySelector("label, legend")!;
            var (name, described) = title.LocalName == "label"
                ? (NameOf(cut, title.GetAttribute("for")!), cut.Find($"#{title.GetAttribute("for")}"))
                : (Collapse(title.TextContent), group);
            return (Name: name, Help: HelpElementOf(group), Described: described);
        })
        .Where(field => field.Help is not null)
        .Select(field => (field.Name, field.Help!, field.Described))
        .ToList();

    /// <summary>
    /// A field's help: the first muted line under it that is not the Add form's note on why it starts on a ladder.
    /// FormField renders its help as <c>small.page-subtitle</c>; a group's is a <c>p.page-subtitle</c> under its legend (T188).
    /// </summary>
    private static IElement? HelpElementOf(IElement group)
        => group.Children
            .Where(child => child.LocalName is "small" or "p" &&
                            (child.ClassList.Contains("muted") || child.ClassList.Contains("page-subtitle")))
            .FirstOrDefault(child => !Collapse(child.TextContent).StartsWith("Suggested because", StringComparison.Ordinal));

    private static IReadOnlyList<string> DescribedBy(IElement element)
        => (element.GetAttribute("aria-describedby") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    private static string Collapse(string text)
        => string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
}
