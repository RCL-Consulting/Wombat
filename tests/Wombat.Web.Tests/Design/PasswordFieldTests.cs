using System.Text.RegularExpressions;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Wombat.Web.Components.Shared;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// PasswordField (T339, flow 02; R3-SI-Toggle; the round 2 review's A3-A5, B6, B7, E9): the label, the input and the Show
/// toggle, driven by wombat.js on a page with no circuit and by its own @onclick in one. Until T339 the toggle
/// (PasswordToggleButton) toggled over JS interop from a circuit, so on every static page it did nothing.
/// </summary>
public sealed class PasswordFieldTests : TestContext
{
    [Fact]
    public void Static_TheToggleIsHidden_ForWombatJsToShowAndDrive()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderField("Confirm new password", "confirm-password");

        cut.Find("label").GetAttribute("for").Should().Be("confirm-password");
        cut.Find("label").TextContent.Trim().Should().Be("Confirm new password");
        PasswordToggleMarkup.ShouldBeTheToggle(cut, "confirm-password", "Show confirm new password", PasswordToggleMarkup.Driven.ByScript);
    }

    [Fact]
    public void Interactive_TheToggleIsShown_AndTheCircuitDrivesIt()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));

        var cut = RenderField("Password", "remove-password");

        var toggle = PasswordToggleMarkup.ShouldBeTheToggle(cut, "remove-password", "Show password", PasswordToggleMarkup.Driven.ByCircuit);
        toggle.Click();

        var pressed = cut.Find("button.password-toggle");
        pressed.GetAttribute("aria-pressed").Should().Be("true");
        pressed.TextContent.Trim().Should().Be("Show", "pressed, the word stays (E9)");
        cut.Find("#remove-password").GetAttribute("type").Should().Be("text");

        pressed.Click();

        cut.Find("button.password-toggle").GetAttribute("aria-pressed").Should().Be("false");
        cut.Find("#remove-password").GetAttribute("type").Should().Be("password");
    }

    [Fact]
    public void TheInput_CarriesWhatThePageGivesIt()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var cut = RenderComponent<PasswordField>(parameters => parameters
            .Add(field => field.Label, "New password")
            .Add(field => field.Id, "new-password")
            .Add(field => field.Name, "NewPassword")
            .Add(field => field.Autocomplete, "new-password")
            .Add(field => field.Required, true)
            .Add(field => field.Autofocus, true)
            .Add(field => field.Invalid, true)
            .Add(field => field.DescribedBy, "new-password-message change-password-error")
            .AddChildContent("<div class=\"validation-message\" id=\"new-password-message\">The new password does not meet the rules below.</div>"));

        var input = cut.Find("#new-password");
        input.GetAttribute("name").Should().Be("NewPassword");
        input.GetAttribute("autocomplete").Should().Be("new-password");
        input.HasAttribute("required").Should().BeTrue();
        input.HasAttribute("autofocus").Should().BeTrue();
        input.GetAttribute("aria-invalid").Should().Be("true");
        input.GetAttribute("aria-describedby").Should().Be("new-password-message change-password-error");
        input.HasAttribute("value").Should().BeFalse("a posted form's field is always empty");
        cut.Find(".form-group > .validation-message").TextContent.Should().Contain("does not meet the rules");
    }

    [Fact]
    public void Blank_TheInputNamesNothing_AndIsNeitherInvalidNorFocused()
    {
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        var input = RenderField("Password", "login-password").Find("#login-password");

        input.HasAttribute("aria-describedby").Should().BeFalse();
        input.HasAttribute("aria-invalid").Should().BeFalse();
        input.HasAttribute("autofocus").Should().BeFalse();
        input.HasAttribute("required").Should().BeFalse();
    }

    [Fact]
    public void Bound_TheValueFollowsTheInput()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var value = string.Empty;

        var cut = RenderComponent<PasswordField>(parameters => parameters
            .Add(field => field.Label, "New password")
            .Add(field => field.Id, "reset-password-input")
            .Add(field => field.Value, value)
            .Add(field => field.ValueChanged, (string typed) => value = typed));

        cut.Find("#reset-password-input").Input("A-long-password-1");

        value.Should().Be("A-long-password-1");
        cut.Find("#reset-password-input").HasAttribute("blazor:oninput").Should().BeTrue();
    }

    /// <summary>
    /// A field nothing binds (the Remove dialog's password, posted by the browser) takes no input handler: in a circuit each
    /// keystroke of a password would otherwise cross to the server and render the field again (the step F review).
    /// </summary>
    [Fact]
    public void Unbound_InACircuit_TheInputHasNoHandler()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));

        var cut = RenderField("Password", "remove-password");

        cut.Find("#remove-password").Attributes.Should().NotContain(attribute => attribute.Name.StartsWith("blazor:on", StringComparison.Ordinal));
    }

    /// <summary>
    /// wombat.js holds the static half (A3-A5): one delegated click listener for [data-password-toggle]; the toggles shown
    /// as the page loads and after an enhanced navigation; every controlled field back to a password as its form is posted
    /// and when the page comes back from the back-forward cache. No inline handler: the CSP forbids one.
    /// </summary>
    [Fact]
    public void WombatJs_WiresTheStaticToggle()
    {
        var script = File.ReadAllText(Stylesheet.WebFile("wwwroot", "wombat.js"));

        Regex.Matches(script, @"document\.addEventListener\(""click""").Should().ContainSingle("one delegated listener");
        script.Should().Contain("closest(\"[data-password-toggle]\")");
        script.Should().Contain("getAttribute(\"aria-controls\")");
        script.Should().Contain("setAttribute(\"aria-pressed\"");
        script.Should().Contain("window.wombat.revealPasswordToggles();", "shown as the page loads");
        script.Should().Contain("addEventListener(\"enhancedload\", window.wombat.revealPasswordToggles)", "and after an enhanced navigation");
        Regex.IsMatch(script, @"addEventListener\(""submit"", function \(event\) \{\s+if \(event\.target instanceof HTMLFormElement\) \{\s+window\.wombat\.hidePasswords\(event\.target\);")
            .Should().BeTrue("a posted form's fields go back to passwords");
        Regex.IsMatch(script, @"addEventListener\(""pageshow"", function \(event\) \{\s+if \(event\.persisted\) \{\s+window\.wombat\.hidePasswords\(document\);")
            .Should().BeTrue("and a page restored from the back-forward cache's");
        script.Should().NotContain("togglePasswordVisibility", "the interop PasswordToggleButton called is gone");
    }

    private IRenderedComponent<PasswordField> RenderField(string label, string id)
        => RenderComponent<PasswordField>(parameters => parameters.Add(field => field.Label, label).Add(field => field.Id, id));
}
