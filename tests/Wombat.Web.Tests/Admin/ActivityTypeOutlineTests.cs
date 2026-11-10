using System.Text;
using Bunit;
using FluentAssertions;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Identity;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T398 (flow 17, Q4, Q8, S13, S27, T23, M2(b); round 3 check item 1): the Form tab's outline is a list of sections and
/// their fields; a field's or section's editor opens in place under its row, one at a time, Edit then Done; Add field opens
/// the new row's editor; Delete is an outline button that asks nothing; above 20 fields each section folds.
/// </summary>
public sealed class ActivityTypeOutlineTests : TestContext
{
    public ActivityTypeOutlineTests() => BuilderKit.SignIn(this, WombatRoles.InstitutionalAdmin);

    [Fact]
    public void TheOutline_IsAListOfRows_EachByLabel_WithItsTypeKeyAndRequired()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        var section = cut.Find(".outline-section");
        section.QuerySelector("h3")!.TextContent.Should().Be("Teaching session");
        section.QuerySelector(".outline-section-head .field-row-meta code")!.TextContent.Should().Be("session");

        var rows = cut.FindAll("ul.list-unstyled > li.field-row");
        rows.Select(row => row.QuerySelector(".field-row-label")!.TextContent).Should().Equal(
            "Topic", "EPA", "Date delivered", "Audience", "Learning objectives", "Supervising consultant");
        rows.ElementAt(0).QuerySelector(".field-row-meta")!.TextContent.Should().Be("Text · topic · Required");
        rows.ElementAt(1).QuerySelector(".field-row-meta")!.TextContent.Should().Be("EPA · epa_id · Required", "a type by its label (T324)");
        rows.ElementAt(5).QuerySelector(".field-row-meta")!.TextContent.Should().Be("User · Names an Assessor · supervisor_user_id");
        rows.ElementAt(0).QuerySelectorAll(".actions-cell button").Select(button => button.GetAttribute("aria-label"))
            .Should().Equal("Edit field Topic", "Move field Topic up", "Move field Topic down", "Delete field Topic");
        rows.ElementAt(0).QuerySelector("button[aria-label='Move field Topic up']")!.HasAttribute("disabled").Should().BeTrue();
        cut.FindAll(".field-editor-inplace").Should().BeEmpty("nothing opens with the page");
    }

    [Fact]
    public void Edit_OpensTheEditorUnderItsRow_AnLiWithNoRole_AndDoneClosesIt()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        BuilderKit.Named(cut, "Edit field Audience").Click();

        var row = cut.Find("li.field-row.is-editing");
        var editor = row.NextElementSibling!;
        editor.TagName.Should().Be("LI");
        editor.ClassList.Should().Contain("field-editor-inplace");
        editor.HasAttribute("role").Should().BeFalse("an li is no group (round 3 check, item 1)");
        editor.HasAttribute("aria-labelledby").Should().BeFalse();
        editor.QuerySelector("h4")!.TextContent.Should().Be("Field Audience");
        editor.TextContent.Should().Contain("* marks a field you must fill in.");
        cut.Find("#field-options").GetAttribute("aria-required").Should().Be("true", "a Choice's options are starred (M1)");

        var done = BuilderKit.Named(cut, "Done with field Audience");
        done.TextContent.Trim().Should().Be("Done");
        done.GetAttribute("aria-expanded").Should().Be("true");
        done.GetAttribute("aria-controls").Should().Be(editor.Id, "N22");
        BuilderKit.FocusedIds(this).Should().Equal(["field-key"], "Edit focuses the editor's first input");

        BuilderKit.Named(cut, "Edit field Topic").Click();
        cut.FindAll(".field-editor-inplace").Should().ContainSingle("one at a time").Which.QuerySelector("h4")!.TextContent.Should().Be("Field Topic");

        BuilderKit.Named(cut, "Done with field Topic").Click();
        cut.FindAll(".field-editor-inplace").Should().BeEmpty();
        BuilderKit.FocusedIds(this).Last().Should().Be("row-0-0-edit", "Done gives the focus back to the row's Edit");
    }

    [Fact]
    public void ASectionsEditor_OpensUnderItsHead()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        BuilderKit.Named(cut, "Edit section Teaching session").Click();

        cut.Find(".outline-section-head").ClassList.Should().Contain("is-editing");
        cut.Find("#se-0 h4").TextContent.Should().Be("Section Teaching session");
        cut.Find("#section-key").GetAttribute("value").Should().Be("session");
        BuilderKit.FocusedIds(this).Should().Equal(["section-key"]);
    }

    [Fact]
    public void AddField_OpensTheNewRowsEditor_ItsKeyFocused()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        BuilderKit.Named(cut, "Add field to section Teaching session").Click();

        var rows = cut.FindAll("li.field-row");
        rows.Last().QuerySelector(".field-row-label")!.TextContent.Should().Be("New field");
        rows.Last().ClassList.Should().Contain("is-editing");
        cut.Find("#field-key").GetAttribute("value").Should().Be("field_1");
        BuilderKit.FocusedIds(this).Should().Equal(["field-key"], "S13: no Edit is pressed for a new field");
    }

    [Fact]
    public void Delete_IsAnOutlineButton_ThatAsksNothing_TheFocusToTheNextRowsEdit()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));
        var delete = BuilderKit.Named(cut, "Delete field Topic");
        delete.ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger");
        delete.HasAttribute("aria-haspopup").Should().BeFalse();

        delete.Click();

        cut.FindAll("li.field-row .field-row-label").Select(label => label.TextContent).Should().NotContain("Topic");
        BuilderKit.FocusedIds(this).Should().Equal(["row-0-0-edit"], "the next row, EPA, is now the first");

        BuilderKit.Named(cut, "Delete field Supervising consultant").Click();
        BuilderKit.FocusedIds(this).Last().Should().Be("add-field-0", "with no next row, Add field");
    }

    [Fact]
    public void UpAndDown_MoveTheRow_TheFocusStaysWithIt()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        BuilderKit.Named(cut, "Move field EPA up").Click();

        cut.FindAll("li.field-row .field-row-label").ElementAt(0).TextContent.Should().Be("EPA");
        BuilderKit.FocusedIds(this).Should().Equal(["row-0-0-down"], "it is first now, so its Up is disabled: Down takes the focus");
    }

    [Fact]
    public void AboveTwentyFields_EachSectionFolds_NoAriaExpanded()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk(draftSchema: Schema(sections: 3, fieldsEach: 7))));

        var fold = BuilderKit.Named(cut, "Hide fields of section Section 2");
        fold.TextContent.Trim().Should().Be("Hide fields");
        fold.GetAttribute("aria-controls").Should().Be("fields-1");
        fold.HasAttribute("aria-expanded").Should().BeFalse("M2(b): the words say which way it goes");
        cut.FindAll("ul[hidden]").Should().BeEmpty("every section starts open");

        fold.Click();

        cut.Find("#fields-1").HasAttribute("hidden").Should().BeTrue();
        cut.Find("#fields-1").Children.Should().BeEmpty();
        BuilderKit.Named(cut, "Show fields of section Section 2").TextContent.Trim().Should().Be("Show fields");
        cut.FindAll("button[aria-label='Add field to section Section 2']").Should().BeEmpty();
    }

    [Fact]
    public void AtTwentyFields_NothingFolds()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk(draftSchema: Schema(sections: 2, fieldsEach: 10))));

        cut.FindAll("button").Where(button => button.TextContent.Trim() is "Hide fields" or "Show fields").Should().BeEmpty();
    }

    [Fact]
    public void TheFormSettings_ReadByLabel_WithTheKeyAfter()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        cut.FindAll("#observation-date-field option").Select(option => option.TextContent.Trim())
            .Should().Equal("None — this form has no single encounter date", "Date delivered (delivered_on)");
        cut.FindAll("#evidence-epa-field option").Select(option => option.TextContent.Trim())
            .Should().Equal("None — this form is about no single EPA", "EPA (epa_id)");
    }

    [Fact]
    public void TheNamesASelect_OffersRolesByTheirLabels()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));
        BuilderKit.Named(cut, "Edit field Supervising consultant").Click();

        cut.FindAll("#field-role option").Select(option => option.TextContent.Trim()).Should()
            .Contain(["Assessor (default)", "Institutional admin", "Coordinator", "Committee member", "Trainee"])
            .And.NotContain(["InstitutionalAdmin", "CommitteeMember"]);
    }

    [Fact]
    public void ThePreview_IsAnH2_ItsSectionsH3_WithThePersonLine()
    {
        var cut = BuilderKit.Render(this, new BuilderSender(BuilderKit.Kgk()));

        var preview = cut.Find(".builder-two-col").Children.ElementAt(1);
        preview.QuerySelector("h2")!.TextContent.Should().Be("Live preview");
        preview.QuerySelectorAll("h3").Select(heading => heading.TextContent.Trim()).Should().Contain("Teaching session", "S8: level 3");
        preview.TextContent.Should().Contain(
            "In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar.");
    }

    private static string Schema(int sections, int fieldsEach)
    {
        var builder = new StringBuilder("""{ "version": 1, "sections": [""");
        var n = 0;
        for (var s = 1; s <= sections; s++)
        {
            builder.Append(s > 1 ? "," : string.Empty).Append($$"""{ "key": "s{{s}}", "title": "Section {{s}}", "fields": [""");
            for (var f = 1; f <= fieldsEach; f++)
            {
                n++;
                builder.Append(f > 1 ? "," : string.Empty).Append($$"""{ "key": "f{{n}}", "type": "text", "label": "Field {{n}}" }""");
            }

            builder.Append("]}");
        }

        var json = builder.Append("]}").ToString();
        FormSchemaParser.Parse(json);
        return json;
    }
}
