using AngleSharp.Dom;
using Bunit;
using FluentAssertions;

namespace Wombat.Web.Tests.TestSupport;

/// <summary>
/// What a password field's Show toggle must be, on every page that has one (PasswordField; T339, flow 02, R3-SI-Toggle):
/// one shared assertion, so the sign-in, link, register, change-password, reset-card and Remove-dialog tests hold every
/// toggle to the same markup.
/// </summary>
internal static class PasswordToggleMarkup
{
    /// <summary>How the toggle is driven: by wombat.js on a page with no circuit, by its own @onclick in one.</summary>
    public enum Driven
    {
        /// <summary>Static: rendered hidden, with data-password-toggle, for wombat.js to show and drive.</summary>
        ByScript,

        /// <summary>Interactive: shown, with an @onclick and no data-password-toggle.</summary>
        ByCircuit
    }

    /// <summary>The toggle that controls <paramref name="inputId" />, the one there is.</summary>
    public static IElement For(IParentNode page, string inputId) => For(selector => page.QuerySelectorAll(selector), inputId);

    /// <summary>
    /// <see cref="ShouldBeTheToggle(IParentNode, string, string, Driven)" /> for a component bUnit rendered.
    /// </summary>
    public static IElement ShouldBeTheToggle(IRenderedFragment rendered, string inputId, string expectedName, Driven driven)
        => ShouldBeTheToggle(selector => rendered.FindAll(selector), inputId, expectedName, driven);

    private static IElement For(Func<string, IEnumerable<IElement>> selectAll, string inputId)
    {
        var toggles = selectAll($"button[aria-controls='{inputId}']").ToList();
        toggles.Should().ContainSingle($"one Show toggle controls #{inputId}");
        return toggles[0];
    }

    /// <summary>
    /// Holds the toggle for <paramref name="inputId" /> to its markup: a <c>.btn.btn-outline.password-toggle</c> of type
    /// button beside its input in <c>.password-field</c>, the word "Show" whether pressed or not (E9), named "Show" and the
    /// field's label lower-cased (C5), not pressed, controlling the input; the eye and eye-off icons; hidden with
    /// data-password-toggle only when <paramref name="driven" /> is <see cref="Driven.ByScript" />.
    /// </summary>
    public static IElement ShouldBeTheToggle(IParentNode page, string inputId, string expectedName, Driven driven)
        => ShouldBeTheToggle(selector => page.QuerySelectorAll(selector), inputId, expectedName, driven);

    private static IElement ShouldBeTheToggle(
        Func<string, IEnumerable<IElement>> selectAll, string inputId, string expectedName, Driven driven)
    {
        var toggle = For(selectAll, inputId);
        var input = selectAll($"#{inputId}").SingleOrDefault();
        input.Should().NotBeNull();
        input!.GetAttribute("type").Should().Be("password", "the field starts hidden");
        toggle.ParentElement!.ClassList.Should().Contain("password-field");
        input.ParentElement.Should().BeSameAs(toggle.ParentElement, "the toggle stands beside its input, in one .password-field");

        toggle.GetAttribute("type").Should().Be("button", "it must never submit the form");
        toggle.ClassList.Should().Contain(["btn", "btn-outline", "password-toggle"]);
        toggle.GetAttribute("aria-label").Should().Be(expectedName);
        toggle.GetAttribute("aria-pressed").Should().Be("false");
        toggle.TextContent.Trim().Should().Be("Show");
        toggle.QuerySelectorAll("svg use").Select(use => use.GetAttribute("href"))
            .Should().Equal("/icons/eye.svg#i", "/icons/eye-off.svg#i");
        toggle.QuerySelectorAll("svg").Should().OnlyContain(svg => svg.GetAttribute("aria-hidden") == "true");

        if (driven == Driven.ByScript)
        {
            toggle.HasAttribute("hidden").Should().BeTrue("with script blocked the page has no dead button");
            toggle.HasAttribute("data-password-toggle").Should().BeTrue("wombat.js shows it and drives it");
            toggle.Attributes.Should().NotContain(attribute => attribute.Name.StartsWith("blazor:", StringComparison.Ordinal));
        }
        else
        {
            toggle.HasAttribute("hidden").Should().BeFalse("the circuit drives it");
            toggle.HasAttribute("data-password-toggle").Should().BeFalse("wombat.js must not drive it too");
            toggle.HasAttribute("blazor:onclick").Should().BeTrue();
        }

        return toggle;
    }
}
