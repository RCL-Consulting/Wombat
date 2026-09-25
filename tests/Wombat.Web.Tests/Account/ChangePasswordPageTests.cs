using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Components.Shared;
using Wombat.Web.Security;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Account;

/// <summary>
/// T265: the change-password page posts its form to an endpoint and shows what the endpoint sent it back with. It never
/// changes the password, or the sign-in cookie, in its circuit.
/// </summary>
/// <remarks>
/// <para>
/// Until T265 the page changed the password in its circuit and then called <c>RefreshSignInAsync</c> there, which threw
/// "Headers are read-only": the circuit's response had started long before. The password was changed, the button stayed
/// on "Saving...", and Blazor's error bar appeared. The integration suite's <c>Hosting/ChangePasswordFlowTests</c> runs
/// the post end to end, from the form to the cookie issued again; these tests hold the page to its half.
/// </para>
/// </remarks>
public sealed class ChangePasswordPageTests : TestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private static readonly string[] FieldIds = ["current-password", "new-password", "confirm-password"];

    public ChangePasswordPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton(new IdentityErrorDescriber());

        // The rules AddInfrastructure sets.
        Services.AddSingleton(Options.Create(new IdentityOptions
        {
            Password = new PasswordOptions
            {
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
                RequiredLength = 12,
                RequiredUniqueChars = 4
            }
        }));
    }

    [Fact]
    public void TheForm_IsPostedByTheBrowserToTheEndpoint_NotHandledInTheCircuit()
    {
        var cut = Render(ChangePasswordOutcome.PagePath);

        var form = cut.Find("form");
        form.GetAttribute("method").Should().Be("post");
        form.GetAttribute("action").Should().Be(ChangePasswordOutcome.SubmitPath).And.Be("/account/change-password/submit");
        form.HasAttribute("data-submit-once").Should().BeTrue("a second press while the post is on its way sends nothing");
        form.HasAttribute("blazor:onsubmit").Should().BeFalse("the circuit must not take the submit: it cannot set a cookie");

        cut.FindAll("form input:not([type=hidden])").Select(input => (input.Id, input.GetAttribute("name"), input.GetAttribute("autocomplete")))
            .Should().Equal(
                ("current-password", "CurrentPassword", "current-password"),
                ("new-password", "NewPassword", "new-password"),
                ("confirm-password", "ConfirmPassword", "new-password"));
        cut.FindAll("form input:not([type=hidden])").Should().OnlyContain(input => input.GetAttribute("type") == "password")
            .And.OnlyContain(input => input.HasAttribute("required"));

        var submit = cut.Find("form button[type=submit]");
        submit.TextContent.Trim().Should().Be("Change password");
        submit.HasAttribute("blazor:onclick").Should().BeFalse();

        // The page stays interactive, so each field's Show button still works.
        cut.FindAll(".password-toggle-btn").Should().HaveCount(3).And.OnlyContain(button => button.HasAttribute("blazor:onclick"));
    }

    [Fact]
    public void WithNothingToReport_ThePageShowsTheFormAlone_AndMovesNoFocus()
    {
        var cut = Render(ChangePasswordOutcome.PagePath);

        cut.FindAll(".alert").Should().BeEmpty();
        cut.Find(".action-result").HasAttribute("autofocus").Should().BeFalse();
        FocusCalls().Should().BeEmpty();
        FieldIds.Should().OnlyContain(id => !cut.Find($"#{id}").HasAttribute("aria-describedby"));
    }

    [Fact]
    public void OnceTheEndpointHasChangedThePassword_ThePageSaysSo_AndTheResultTakesTheFocus()
    {
        var cut = Render(ChangePasswordOutcome.UpdatedUrl);

        var done = cut.Find(".action-result .alert.alert-success");
        done.TextContent.Trim().Should().Be("Password updated.");
        done.GetAttribute("role").Should().Be("status");
        cut.FindAll(".alert-danger").Should().BeEmpty();

        cut.Find(".action-result").HasAttribute("autofocus").Should().BeTrue("the result arrives with the page");
        var region = cut.FindComponent<ActionResult>();
        FocusCalls().Should().ContainSingle("the circuit's render replaces the HTML the browser focused, so it focuses again")
            .Which.Arguments[0].Should().BeOfType<ElementReference>().Which.Id.Should().Be(region.Instance.Element.Id);
        FieldIds.Should().OnlyContain(id => string.IsNullOrEmpty(cut.Find($"#{id}").GetAttribute("value")), "the fields are empty");
    }

    [Theory]
    [InlineData("?error=PasswordMismatch", "Incorrect password.")]
    [InlineData("?error=ConfirmationMismatch", "The password confirmation does not match.")]
    [InlineData("?error=FieldsMissing", "Enter your current password, a new password, and the new password again to confirm it.")]
    [InlineData("?error=PasswordTooShort&error=PasswordRequiresDigit",
        "Passwords must be at least 12 characters. Passwords must have at least one digit ('0'-'9').")]
    [InlineData("?error=ConcurrencyFailure", ChangePasswordOutcome.GeneralRefusal)]
    [InlineData("?error=LockedOut", "Too many incorrect passwords, so the account is locked for a few minutes. Please try again later.")]
    [InlineData("?error=TooManyAttempts", "Too many attempts from this network. Please wait a few minutes and try again.")]
    [InlineData("?error=InstitutionalSignIn", "This account signs in through your institution, so it has no password to change here.")]
    public void ARefusal_IsShownInTheServersOwnWords_ReadAtOnce_AndNamedByEachField(string query, string expected)
    {
        var cut = Render(ChangePasswordOutcome.PagePath + query);

        var refusal = cut.Find(".action-result .alert.alert-danger");
        refusal.TextContent.Trim().Should().Be(expected);
        refusal.GetAttribute("role").Should().Be("alert");
        refusal.Id.Should().Be(ChangePassword.RefusalId);
        cut.FindAll(".alert-success").Should().BeEmpty();

        FieldIds.Should().OnlyContain(id => cut.Find($"#{id}").GetAttribute("aria-describedby") == ChangePassword.RefusalId,
            "an alert already on the page when it loads is not reliably announced; each field reads it");
        cut.Find(".action-result").HasAttribute("autofocus").Should().BeTrue();
        FocusCalls().Should().ContainSingle();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AnAddressCarryingWordsOfItsOwn_PutsNoneOfThemOnThePage()
    {
        var cut = Render(ChangePasswordOutcome.PagePath +
                         "?error=Your%20account%20is%20locked.%20Call%20012%20345%206789&error=PasswordMismatch");

        cut.Markup.Should().NotContain("Call 012", "a refusal travels as a code, and the page chooses the words");
        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be($"{ChangePasswordOutcome.GeneralRefusal} Incorrect password.");
    }

    [Fact]
    public void ARefusalAndAStatusTogether_ShowTheRefusalOnly()
    {
        var cut = Render(ChangePasswordOutcome.UpdatedUrl + "&error=PasswordMismatch");

        cut.FindAll(".alert-success").Should().BeEmpty("a link cannot say both");
        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be("Incorrect password.");
    }

    [Fact]
    public void TheEndpointsAddresses_CarryCodes_EachOnce_AndAFailureWithNoCodeStillSaysSomething()
    {
        ChangePasswordOutcome.UpdatedUrl.Should().Be("/account/change-password?status=updated");
        ChangePasswordOutcome.RefusedUrl(["PasswordTooShort", "PasswordRequiresDigit", "PasswordTooShort", " "])
            .Should().Be("/account/change-password?error=PasswordTooShort&error=PasswordRequiresDigit");
        ChangePasswordOutcome.RefusedUrl([]).Should().Be("/account/change-password?error=Failed");
        ChangePasswordOutcome.RefusedUrl(["a b&c"]).Should().Be("/account/change-password?error=a%20b%26c");

        var describer = new IdentityErrorDescriber();
        ChangePasswordOutcome.Describe(["Failed", "Unknown", "DefaultError"], describer, new PasswordOptions())
            .Should().Equal([ChangePasswordOutcome.GeneralRefusal], "one general sentence, however many codes it covers");
        ChangePasswordOutcome.Describe(null, describer, new PasswordOptions()).Should().BeEmpty();
    }

    private IRenderedComponent<ChangePassword> Render(string address)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        return RenderComponent<ChangePassword>();
    }

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();
}
