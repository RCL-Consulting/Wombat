using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Features.Accounts;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Security;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Account;

/// <summary>
/// Change password (T265; T339, flow 02, R3-CP-*): a static page in the shell whose form the browser posts to an endpoint,
/// which shows what the endpoint sent it back with. It never changes the password, or the sign-in cookie, itself.
/// </summary>
/// <remarks>
/// <para>
/// Until T265 the page changed the password in its circuit and then called <c>RefreshSignInAsync</c> there, which threw
/// "Headers are read-only". The integration suite's <c>Hosting/ChangePasswordFlowTests</c> runs the post end to end; these
/// tests hold the page to its half. It renders as it does on the server: static, with no circuit.
/// </para>
/// <para>
/// Changed deliberately by T339 (flow 02): the page is static, so its toggles are wombat.js's (hidden, data-password-toggle)
/// and nothing is focused by a circuit, the refusal taking the focus with autofocus; "Password updated." is My account's
/// now (the endpoint goes there), so this page has no success state; a refusal stands in the card above its action row and
/// marks the field it concerns, which names it, and no longer every field; the six rules stand under New password before
/// anything is typed; the institution-only account sees an info state in place of the form, on its first visit.
/// </para>
/// </remarks>
public sealed class ChangePasswordPageTests : TestContext
{
    private static readonly string[] FieldIds = ["current-password", "new-password", "confirm-password"];

    private readonly StubSender _sender = new();

    public ChangePasswordPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        Services.AddSingleton(new IdentityErrorDescriber());
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<ILogger<ChangePassword>>(NullLogger<ChangePassword>.Instance);

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

        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("fatima@kgk.test");
        auth.SetRoles(WombatRoles.Assessor);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "fatima"));

        // Static, as the server renders it (App.razor): PasswordField reads it. Last, since it builds the services.
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
    }

    [Fact]
    public void TheForm_IsPostedByTheBrowserToTheEndpoint_WithNoCircuit()
    {
        var cut = Render(ChangePasswordOutcome.PagePath);

        var form = cut.Find("form");
        form.GetAttribute("method").Should().Be("post");
        form.GetAttribute("action").Should().Be(ChangePasswordOutcome.SubmitPath).And.Be("/account/change-password/submit");
        form.HasAttribute("data-submit-once").Should().BeTrue("a second press while the post is on its way sends nothing");
        form.HasAttribute("blazor:onsubmit").Should().BeFalse("no circuit takes the submit: it cannot set a cookie");

        cut.FindAll("form input:not([type=hidden])").Select(input => (input.Id, input.GetAttribute("name"), input.GetAttribute("autocomplete")))
            .Should().Equal(
                ("current-password", "CurrentPassword", "current-password"),
                ("new-password", "NewPassword", "new-password"),
                ("confirm-password", "ConfirmPassword", "new-password"));
        cut.FindAll("form input:not([type=hidden])").Should().OnlyContain(input => input.GetAttribute("type") == "password")
            .And.OnlyContain(input => input.HasAttribute("required"))
            .And.OnlyContain(input => string.IsNullOrEmpty(input.GetAttribute("value")), "the fields are always empty");
        cut.FindAll("label span[aria-hidden]").Should().BeEmpty("every field is required, so none is marked * (R3-Spec)");
        cut.FindAll("label").Select(label => label.TextContent.Trim())
            .Should().Equal("Current password", "New password", "Confirm new password");

        cut.FindAll(".form-actions .btn").Select(button => button.TextContent.Trim()).Should().Equal("Cancel", "Change password");
        cut.Find(".form-actions a.btn").GetAttribute("href").Should().Be("/account/profile");
        cut.Find("form button[type=submit]").HasAttribute("blazor:onclick").Should().BeFalse();
    }

    [Fact]
    public void EachField_HasItsShowToggle_NamedByItsLabel_ForWombatJsToDrive()
    {
        var cut = Render(ChangePasswordOutcome.PagePath);

        PasswordToggleMarkup.ShouldBeTheToggle(cut, "current-password", "Show current password", PasswordToggleMarkup.Driven.ByScript);
        PasswordToggleMarkup.ShouldBeTheToggle(cut, "new-password", "Show new password", PasswordToggleMarkup.Driven.ByScript);
        PasswordToggleMarkup.ShouldBeTheToggle(cut, "confirm-password", "Show confirm new password", PasswordToggleMarkup.Driven.ByScript);
    }

    [Fact]
    public void Blank_ThePageShowsTheFormAndTheSixRules_WhichNewPasswordNames_AndFocusesNothing()
    {
        var cut = Render(ChangePasswordOutcome.PagePath);

        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("Choose a new password for signing in to Wombat.");
        cut.FindAll(".alert").Should().BeEmpty();
        cut.FindAll("[autofocus]").Should().BeEmpty("a full load, so the focus is at the top of the page");
        TabTitle.Of(this, cut).Should().Be("Change password · Wombat");

        var rules = cut.Find($"#{ChangePassword.RulesId}");
        rules.ClassList.Should().Contain("password-rules");
        rules.QuerySelector("p")!.TextContent.Trim().Should().Be("The new password needs:");
        rules.QuerySelectorAll("li").Select(rule => rule.TextContent.Trim()).Should().Equal(
            "At least 12 characters.",
            "At least 4 different characters.",
            "A digit (0 to 9).",
            "An upper-case letter.",
            "A lower-case letter.",
            "A symbol, such as ! or #.");
        cut.Find("#new-password").GetAttribute("aria-describedby").Should().Be(ChangePassword.RulesId);
        cut.Find("#current-password").HasAttribute("aria-describedby").Should().BeFalse();
        cut.Find("#confirm-password").HasAttribute("aria-describedby").Should().BeFalse();
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Theory]
    [InlineData("?error=PasswordMismatch", "Your password was not changed. Incorrect password.", "current-password", "Incorrect password.")]
    [InlineData("?error=ConfirmationMismatch", "Your password was not changed. The password confirmation does not match.",
        "confirm-password", "The password confirmation does not match.")]
    [InlineData("?error=PasswordRequiresDigit&error=PasswordTooShort",
        "Your password was not changed. The new password needs: At least 12 characters. A digit (0 to 9).",
        "new-password", "The new password does not meet the rules below.")]
    public void ARefusalAboutOneField_MarksIt_AndItNamesItsMessageAndTheRefusal(
        string query, string expected, string fieldId, string fieldMessage)
    {
        var cut = Render(ChangePasswordOutcome.PagePath + query);

        var refusal = TheRefusal(cut);
        Words(refusal).Should().Be(expected);

        var field = cut.Find($"#{fieldId}");
        field.GetAttribute("aria-invalid").Should().Be("true");
        var named = field.GetAttribute("aria-describedby")!.Split(' ');
        named.Should().Equal($"{fieldId}-message", ChangePassword.RefusalId);
        cut.Find($"#{fieldId}-message").TextContent.Trim().Should().Be(fieldMessage);

        FieldIds.Where(id => id != fieldId).Should().OnlyContain(id => !cut.Find($"#{id}").HasAttribute("aria-invalid"));
        cut.Find("#new-password").GetAttribute("aria-describedby").Should().NotContain(
            ChangePassword.RulesId, "while a refusal is shown the field names it, not the list (A15)");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void TheRulesBroken_AreListedInTheRefusal_UnderTheHeading_InTheOneOrder()
    {
        var cut = Render(ChangePasswordOutcome.PagePath + "?error=PasswordRequiresUpper&error=PasswordTooShort&error=PasswordRequiresDigit");

        var refusal = TheRefusal(cut);
        refusal.QuerySelector("strong")!.TextContent.Should().Be("Your password was not changed.");
        refusal.QuerySelectorAll("ul.password-rules-broken li").Select(rule => rule.TextContent.Trim())
            .Should().Equal("At least 12 characters.", "A digit (0 to 9).", "An upper-case letter.");
    }

    [Theory]
    [InlineData("?error=FieldsMissing",
        "Your password was not changed. Enter your current password, a new password, and the new password again to confirm it.")]
    [InlineData("?error=ConcurrencyFailure", ChangePasswordOutcome.GeneralRefusal)]
    [InlineData("?error=LockedOut", ChangePasswordOutcome.GeneralRefusal)]
    [InlineData("?error=TooManyAttempts", "Too many attempts from this network. Wait a few minutes and try again.")]
    public void ARefusalAboutNoOneField_MarksNone_AndTheFieldsNameNothing(string query, string expected)
    {
        var cut = Render(ChangePasswordOutcome.PagePath + query);

        Words(TheRefusal(cut)).Should().Be(expected);
        FieldIds.Should().OnlyContain(id => !cut.Find($"#{id}").HasAttribute("aria-invalid"));
        FieldIds.Should().OnlyContain(id => !cut.Find($"#{id}").HasAttribute("aria-describedby"),
            "the refusal takes the focus itself, so it is read as the page loads");
    }

    [Fact]
    public void ARefusal_StandsInTheCardAboveItsActionRow_TakesTheFocus_AndTheTabSaysError()
    {
        var cut = Render(ChangePasswordOutcome.PagePath + "?error=PasswordMismatch");

        var refusal = TheRefusal(cut);
        refusal.GetAttribute("role").Should().Be("alert");
        refusal.GetAttribute("tabindex").Should().Be("-1");
        refusal.HasAttribute("autofocus").Should().BeTrue("the result arrives with the page");
        cut.FindAll("[autofocus]").Should().ContainSingle();
        refusal.Closest("form").Should().NotBeNull("in the card");
        refusal.NextElementSibling!.ClassList.Should().Contain("form-actions", "above the action row");
        TabTitle.Of(this, cut).Should().Be("Error: Change password · Wombat");
    }

    [Fact]
    public void AnAddressCarryingWordsOfItsOwn_PutsNoneOfThemOnThePage()
    {
        var cut = Render(ChangePasswordOutcome.PagePath +
                         "?error=Your%20account%20is%20locked.%20Call%20012%20345%206789&error=PasswordMismatch");

        cut.Markup.Should().NotContain("Call 012", "a refusal travels as a code, and the page chooses the words");
        Words(TheRefusal(cut)).Should().Be($"Your password was not changed. {ChangePasswordOutcome.GeneralRefusal} Incorrect password.");
    }

    [Fact]
    public void AStatusInTheAddress_SaysNothing_ForMyAccountSaysThePasswordWasUpdated()
    {
        // The endpoint sends a change to My account since T339 (flow 02), so this page has no success state to show.
        var cut = Render($"{ChangePasswordOutcome.PagePath}?status={ChangePasswordOutcome.Updated}");

        cut.FindAll(".alert").Should().BeEmpty();
        cut.Markup.Should().NotContain("Password updated.");
    }

    [Fact]
    public void AnAccountWithNoPassword_SeesWhy_AndTheWayBack_InPlaceOfTheForm_OnItsFirstVisit()
    {
        _sender.Profile = _sender.Profile with { HasLocalPassword = false };

        var cut = Render(ChangePasswordOutcome.PagePath);

        AssertInstitutionOnly(cut);
        _sender.Sent.Should().ContainSingle().Which.Should().BeOfType<GetCurrentUserProfileQuery>()
            .Which.Principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be("fatima", "the caller's own account");
    }

    [Fact]
    public void TheEndpointsInstitutionOnlyRefusal_ReadsAsTheSameInfoState()
    {
        var cut = Render(ChangePasswordOutcome.PagePath + "?error=InstitutionalSignIn");

        AssertInstitutionOnly(cut);
    }

    [Fact]
    public void AFaultReadingTheAccount_OffersTheForm_ForTheEndpointChecksTheAccountItself()
    {
        _sender.FailReads = true;

        var cut = Render(ChangePasswordOutcome.PagePath);

        cut.FindAll("form").Should().ContainSingle();
        cut.FindAll(".alert").Should().BeEmpty();
    }

    [Fact]
    public void TheEndpointsAddresses_CarryCodes_EachOnce_AndAFailureWithNoCodeStillSaysSomething()
    {
        ChangePasswordOutcome.UpdatedUrl.Should().Be("/account/profile?status=password-updated");
        ChangePasswordOutcome.RefusedUrl(["PasswordTooShort", "PasswordRequiresDigit", "PasswordTooShort", " "])
            .Should().Be("/account/change-password?error=PasswordTooShort&error=PasswordRequiresDigit");
        ChangePasswordOutcome.RefusedUrl([]).Should().Be("/account/change-password?error=Failed");
        ChangePasswordOutcome.RefusedUrl(["a b&c"]).Should().Be("/account/change-password?error=a%20b%26c");

        var describer = new IdentityErrorDescriber();
        ChangePasswordOutcome.Describe(["Failed", "Unknown", "DefaultError"], describer, new PasswordOptions())
            .Should().Equal([ChangePasswordOutcome.GeneralRefusal], "one general sentence, however many codes it covers");
        ChangePasswordOutcome.Describe(null, describer, new PasswordOptions()).Should().BeEmpty();
    }

    private void AssertInstitutionOnly(IRenderedComponent<ChangePassword> cut)
    {
        cut.FindAll("form").Should().BeEmpty("there is no password to change here");
        cut.Find("h1").TextContent.Trim().Should().Be("Change password");
        cut.FindAll(".page-subtitle").Should().BeEmpty("\"Choose a new password\" would contradict the notice (C9)");

        var notice = cut.Find(".alert");
        notice.ClassList.Should().Contain("alert-info");
        notice.GetAttribute("role").Should().Be("status");
        notice.TextContent.Trim().Should().Be("This account signs in through your institution, so it has no password to change here.");

        var back = cut.Find(".change-password > a.btn");
        back.TextContent.Trim().Should().Be("Back to My account");
        back.GetAttribute("href").Should().Be("/account/profile");
        TabTitle.Of(this, cut).Should().Be("Change password · Wombat", "information, not an error");
    }

    private static IElement TheRefusal(IRenderedComponent<ChangePassword> cut)
    {
        var refusal = cut.Find($"#{ChangePassword.RefusalId}");
        refusal.ClassList.Should().Contain(["alert", "alert-danger"]);
        return refusal;
    }

    /// <summary>
    /// The element's words as they read: its text, with each list item read as its own sentence. Razor writes no space
    /// between elements, so the text of adjacent list items runs together in TextContent; the words before the list must
    /// carry their own spaces, which the tests with no list hold exactly.
    /// </summary>
    private static string Words(IElement element)
    {
        var listItems = element.QuerySelectorAll("li").Select(item => item.TextContent.Trim()).ToList();
        var list = element.QuerySelector("ul");
        var before = list is null ? element.TextContent : element.TextContent[..element.TextContent.IndexOf(list.TextContent, StringComparison.Ordinal)];
        return Regex.Replace(string.Join(" ", [before.Trim(), .. listItems]), @"\s+", " ").Trim();
    }

    private IRenderedComponent<ChangePassword> Render(string address)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        return RenderComponent<ChangePassword>();
    }

    /// <summary>Answers the account's read, and records what the page sends.</summary>
    private sealed class StubSender : IScopedSender
    {
        public List<object> Sent { get; } = [];

        public UserProfileDto Profile { get; set; } =
            new("fatima", "fatima@kgk.test", "Fatima", "Khumalo", [WombatRoles.Assessor]) { HasLocalPassword = true };

        public bool FailReads { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            if (request is not GetCurrentUserProfileQuery)
            {
                throw new InvalidOperationException($"The page sent {request.GetType().Name}, which it has no business sending.");
            }

            return FailReads
                ? throw new InvalidOperationException("The database is down.")
                : Task.FromResult((TResponse)(object)Profile);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"The page sent {request.GetType().Name}.");
    }
}
