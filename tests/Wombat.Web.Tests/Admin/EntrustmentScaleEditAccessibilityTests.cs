using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Epas;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T198: every control on the entrustment scale editor has a name. Each level's Label and Description inputs sat bare in
/// their cells, so a screen reader announced six rows of "edit text" and nothing to tell them apart: a column header
/// names no input. They are named for their column and their level ("Label, level 3"), and a level's Up, Down and
/// Remove for the level they act on.
/// </summary>
public sealed class EntrustmentScaleEditAccessibilityTests : TestContext
{
    private const int ScaleId = 901;

    /// <summary>The v11.1 ladder: level 3 is the College's rung "3a", so a level's name is its order, not its label.</summary>
    private static readonly EntrustmentScaleDto CpsaScale = new(ScaleId, "CPSA Paediatric Entrustment Scale v11.1", null,
        new[] { "1", "2", "3a", "3b", "4", "5" }
            .Select((label, index) => new EntrustmentLevelDto(9010 + index, index + 1, label, $"Rung {label}."))
            .ToList());

    public EntrustmentScaleEditAccessibilityTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
        Services.AddSingleton<IScopedSender>(new ScaleSender());
    }

    [Fact]
    public void EveryControlOnTheEditor_HasAnAccessibleName_AndEachLevelsAreItsOwn()
    {
        var cut = RenderEditor(ScaleId);

        AccessibleNames.Unnamed(cut).Should().BeEmpty();
        IdReferences.Broken(cut).Should().BeEmpty();

        LevelControlNames(cut).Should().BeEquivalentTo(
            Enumerable.Range(1, 6).Select(order => new[]
            {
                $"Label, level {order}", $"Description, level {order}",
                $"Move level {order} up", $"Move level {order} down", $"Remove level {order}"
            }),
            options => options.WithStrictOrdering());

        var names = AccessibleNames.Controls(cut).Select(control => control.Name).ToList();
        names.Should().OnlyHaveUniqueItems("a screen reader user tells one level's controls from another's by name");
    }

    [Fact]
    public void ANewScale_StartsWithTwoLevels_EachNamed()
    {
        var cut = RenderEditor(null);

        AccessibleNames.Unnamed(cut).Should().BeEmpty();
        LevelControlNames(cut).Select(row => row[0] + " | " + row[1]).Should().Equal(
            "Label, level 1 | Description, level 1",
            "Label, level 2 | Description, level 2");
    }

    [Fact]
    public void ALevelsName_FollowsItsPlace_WhenItMoves_OrOneIsAdded()
    {
        var cut = RenderEditor(ScaleId);

        ClickNamed(cut, "Move level 3 up");

        NameOf(cut, InputHolding(cut, "3a")).Should().Be("Label, level 2", "the level the College calls 3a is now second");
        NameOf(cut, InputHolding(cut, "2")).Should().Be("Label, level 3");

        ClickNamed(cut, "Add level");

        LevelControlNames(cut).Last().Should().Equal(
            "Label, level 7", "Description, level 7", "Move level 7 up", "Move level 7 down", "Remove level 7");
        AccessibleNames.Unnamed(cut).Should().BeEmpty();
        AccessibleNames.Controls(cut).Select(control => control.Name).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void TheLevelsTable_IsAClinicTable_WhoseColumnWidthsAreClassesInAppCss()
    {
        // The Order and actions headers carried style="width:6rem" and "width:12rem", which DESIGN.md forbids, on a
        // table whose class app.css did not define. Order is now as narrow as its content. The buttons' column asks for
        // room to hold them on one line and lets them wrap when the card is narrow (T198 review): held on one line at
        // every width, they pushed Remove out of view at 390px.
        var cut = RenderEditor(ScaleId);

        cut.FindAll("[style]").Should().BeEmpty("a column's width is a class in app.css (DESIGN.md § Table system)");

        var table = cut.Find(".table-container > table");
        table.ClassList.Should().Contain(new[] { "clinic-table", "clinic-table--compact", "clinic-table--inputs" });

        var headers = table.QuerySelectorAll("thead th");
        headers.Select(header => string.Join(' ', header.ClassList)).Should().Equal("col-fit", "", "", "col-actions");
        headers[0].TextContent.Trim().Should().Be("Order");
        headers[3].QuerySelector(".visually-hidden")!.TextContent.Should().Be("Actions", "a header with no text names nothing");

        foreach (var row in table.QuerySelectorAll("tbody tr"))
        {
            row.Children.Select(cell => string.Join(' ', cell.ClassList)).Should().Equal("col-fit", "", "", "col-actions");
        }
    }

    // ---- helpers ----

    private IRenderedComponent<EntrustmentScaleEdit> RenderEditor(int? id)
    {
        var cut = RenderComponent<EntrustmentScaleEdit>(parameters => parameters.Add(page => page.Id, id));
        cut.WaitForState(() => cut.FindAll("tbody tr").Count > 0);
        return cut;
    }

    /// <summary>Each level's row: the accessible names of its controls, inputs first, in the order they appear.</summary>
    private static IReadOnlyList<string[]> LevelControlNames(IRenderedComponent<EntrustmentScaleEdit> cut)
        => cut.FindAll("tbody tr")
            .Select(row => row.QuerySelectorAll("input, button").Select(control => NameOf(cut, control)).ToArray())
            .ToList();

    private static IElement InputHolding(IRenderedComponent<EntrustmentScaleEdit> cut, string value)
        => cut.FindAll("tbody input").Single(input => input.GetAttribute("value") == value);

    private static void ClickNamed(IRenderedComponent<EntrustmentScaleEdit> cut, string name)
        => cut.FindAll("button").Single(button => NameOf(cut, button) == name).Click();

    private static string NameOf(IRenderedComponent<EntrustmentScaleEdit> cut, IElement control)
        => AccessibleNames.NameOf(cut, control);

    /// <summary>The editor's server: it loads one scale. Nothing here saves.</summary>
    private sealed class ScaleSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetEntrustmentScaleByIdQuery { Id: ScaleId }
                ? Task.FromResult((TResponse)(object)CpsaScale)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
