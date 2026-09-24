using Bunit;
using FluentAssertions;

namespace Wombat.Web.Tests.Accessibility;

/// <summary>
/// The page checks lean on <see cref="AccessibleNames" /> to say every control is named, so each way of naming a
/// control is shown counting here, and each way of failing to is shown being caught. A helper that named everything
/// would pass every page.
/// </summary>
public sealed class AccessibleNamesTests : TestContext
{
    [Fact]
    public void EachWayOfNamingAControl_Counts()
    {
        var cut = RenderMarkup("""
            <label for="for">Target</label><input id="for" type="number" />
            <select id="aria" aria-label="Scale"></select>
            <label>Weight <input id="wrapped" /></label>
            <p id="heading">Minimum</p><input id="labelledby" aria-labelledby="heading" />
            <button id="text">Save</button>
            <button id="icon" aria-label="Remove year 2"><svg aria-hidden="true"></svg></button>
            """);

        AccessibleNames.Unnamed(cut).Should().BeEmpty();
        NameOf(cut, "for").Should().Be("Target");
        NameOf(cut, "aria").Should().Be("Scale");
        NameOf(cut, "wrapped").Should().Be("Weight");
        NameOf(cut, "labelledby").Should().Be("Minimum");
        NameOf(cut, "text").Should().Be("Save");
        NameOf(cut, "icon").Should().Be("Remove year 2");
    }

    [Fact]
    public void AControlWithNoLabel_IsUnnamed()
    {
        // T176's defect: the edit row's EPA select and its Completion window and Weight inputs, bare in a table cell.
        var cut = RenderMarkup("""
            <select id="edit-epa"><option>PAED-001</option></select>
            <input id="window" type="number" />
            <textarea></textarea>
            """);

        AccessibleNames.Unnamed(cut).Should().HaveCount(3)
            .And.Contain(problem => problem.Contains("<select>") && problem.Contains("\"edit-epa\""))
            .And.Contain(problem => problem.Contains("type=\"number\"") && problem.Contains("\"window\""))
            .And.Contain(problem => problem.Contains("<textarea>") && problem.Contains("no id"));
    }

    [Fact]
    public void ALabelForAnotherControl_DoesNotNameThisOne()
    {
        var cut = RenderMarkup("""<label for="other">Weight</label><input id="weight" /><input id="other" />""");

        AccessibleNames.Unnamed(cut).Should().ContainSingle().Which.Should().Contain("\"weight\"");
    }

    [Fact]
    public void WhatIsHiddenFromAScreenReader_OrOnlyAHint_IsNotAName()
    {
        // A label whose only text is aria-hidden, a blank aria-label, an aria-labelledby naming nothing, and a
        // placeholder or a title: each looks like a name in the markup and is none to a screen reader user.
        var cut = RenderMarkup("""
            <label for="star"><span aria-hidden="true">*</span></label><input id="star" />
            <input id="blank" aria-label="  " />
            <input id="missing" aria-labelledby="nothing" />
            <input id="hint" placeholder="Weight" title="Weight" />
            <button id="empty"><svg aria-hidden="true"></svg></button>
            """);

        AccessibleNames.Unnamed(cut).Should().HaveCount(5);
    }

    [Fact]
    public void AHiddenInput_IsNotAControl()
    {
        var cut = RenderMarkup("""<input type="hidden" name="__RequestVerificationToken" value="x" />""");

        AccessibleNames.Controls(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheRequiredMarker_IsReadAsARequiredLabel()
    {
        // FormField's label: a visible "*" hidden from screen readers, and "required" hidden from sight.
        var cut = RenderMarkup("""
            <label for="epa">EPA <span aria-hidden="true">*</span> <span class="visually-hidden">required</span></label>
            <select id="epa"></select>
            """);

        NameOf(cut, "epa").Should().Be("EPA required");
    }

    private IRenderedFragment RenderMarkup(string markup)
        => Render(builder => builder.AddMarkupContent(0, markup));

    private static string NameOf(IRenderedFragment cut, string id)
        => AccessibleNames.NameOf(cut, cut.Find($"#{id}"));
}
