using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Options;
using Wombat.Web.Security;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The sign-in page as a browser receives it (T193; T339, flow 02, R3-SI-*). Its fields say what they hold, so a password
/// manager fills the saved account's email and password; its one message slot is a refusal or a notice, and the focus
/// rule (C6) decides who reads it as the page loads.
/// </summary>
/// <remarks>
/// <para>
/// Served through <see cref="AppTestHost" /> because the page is static HTML for a visitor who has not signed in (T181):
/// what matters is the markup of the full load, not a component rendered in isolation.
/// </para>
/// <para>
/// Changed deliberately by T339 (flow 02): the heading is the page's &lt;h1&gt;; a refusal takes the focus itself, where
/// the email took it and named the refusal (C6); the notices are info alerts, not refusals; the tab says "Error:" after
/// a refusal; the institutions' buttons stand above the form; the forgotten-password link is one link, not "Reset it".
/// </para>
/// </remarks>
public sealed class SignInPageFieldTests
{
    private const string SignInPath = "/account/login";

    private const string Kgk = "Kgosi Kgari Teaching Hospital";

    [Fact]
    public async Task TheEmailAndPassword_SayWhatTheyAre_ForAPasswordManager()
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync(SignInPath);

        document.GetElementById("login-email")!.GetAttribute("autocomplete").Should().Be(
            "username", "the email is the account's user name, which a password manager pairs with the saved password");
        document.GetElementById("login-password")!.GetAttribute("autocomplete").Should().Be(
            "current-password", "a sign-in asks for the saved password, not a new one");
    }

    [Fact]
    public async Task TheCard_IsTheLockup_ThePurpose_TheHeading_TheForm_AndOneLinkForAForgottenPassword()
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync(SignInPath);

        var card = document.QuerySelector(".account-form-container")!;
        card.QuerySelector(".account-brand .account-brand-text")!.TextContent.Should().Be("Wombat");
        card.QuerySelector(".account-purpose")!.TextContent.Trim().Should().Be("Work-based assessment for specialist training.");
        document.QuerySelectorAll("h1").Should().ContainSingle().Which.TextContent.Trim().Should().Be("Sign in");
        card.QuerySelectorAll("h2").Should().BeEmpty("the card's heading is the page's h1 (R3-Spec § Typography)");
        document.Title.Should().Be("Sign in · Wombat");

        var forgotten = card.QuerySelector("a.account-link")!;
        forgotten.TextContent.Trim().Should().Be("Forgotten your password?");
        forgotten.GetAttribute("href").Should().Be("/account/forgot-password");
        card.TextContent.Should().NotContain("Reset it");

        card.QuerySelectorAll("label span[aria-hidden]").Should().BeEmpty("every field is required, so none is marked * (R3-Spec)");
        card.QuerySelector(".sso-providers").Should().BeNull("this host offers no institutional sign-in");
        card.QuerySelector(".sso-divider").Should().BeNull();
    }

    [Fact]
    public async Task ThePassword_HasItsShowToggle_HiddenUntilWombatJsDrivesIt()
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync(SignInPath);

        PasswordToggleMarkup.ShouldBeTheToggle(document, "login-password", "Show password", PasswordToggleMarkup.Driven.ByScript);
    }

    [Fact]
    public async Task ARefusedSignIn_IsAnAlert_ThatTakesTheFocus_AndBothFieldsNameIt()
    {
        // The sign-in endpoint redirects a refusal back here with its code. The page reloads, and the refusal is the one
        // thing on it a screen reader must say first: it takes the focus itself, and both fields name it (C6).
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error=Refused");

        var alert = document.QuerySelector(".alert.alert-danger")!;
        alert.TextContent.Trim().Should().Be("Invalid email or password.");
        alert.GetAttribute("role").Should().Be("alert");
        alert.Id.Should().Be("login-error");
        alert.GetAttribute("tabindex").Should().Be("-1");
        alert.HasAttribute("autofocus").Should().BeTrue();
        alert.PreviousElementSibling!.TagName.Should().Be("H1", "one slot, under the heading");

        document.QuerySelectorAll("[autofocus]").Should().ContainSingle("the refusal's autofocus replaces the email's");
        document.GetElementById("login-email")!.GetAttribute("aria-describedby").Should().Be("login-error");
        document.GetElementById("login-password")!.GetAttribute("aria-describedby").Should().Be("login-error");
        document.Title.Should().Be("Error: Sign in · Wombat");
    }

    /// <summary>
    /// Each refusal code the endpoints send the sign-in page reads as round 3's Spec has it (T339, flow 02; T285 before it).
    /// Written out, so a changed word fails here. This host offers no institutional sign-in, so a refused sign-in says only
    /// that the email or password is wrong, and the institutional sign-in's refusals point at the email and password.
    /// </summary>
    [Theory]
    [InlineData("FieldsMissing", "Enter your email and your password.")]
    [InlineData("Refused", "Invalid email or password.")]
    [InlineData("TooManyAttempts", "Too many failed sign-in attempts from this network. Wait a few minutes and try again.")]
    [InlineData("LockedOut", "Invalid email or password.")]
    [InlineData("ExternalLoginUnavailable", "Your institution's sign-in did not complete. Sign in with your email and password.")]
    [InlineData("ExternalSessionExpired",
        "Your institution's sign-in took too long and has expired. Sign in with your email and password.")]
    [InlineData("SsoFailed",
        "Wombat could not sign you in through your institution this time. Try again in a few minutes, or sign in with your " +
        "email and password.")]
    [InlineData("SsoUnknownProvider", "That institution's sign-in is not set up in Wombat. Sign in with your email and password.")]
    [InlineData("SsoNoEmail",
        "Your institution's sign-in did not give Wombat your email address, so Wombat cannot find your account. Sign in " +
        "with your email and password, or ask your administrator for help.")]
    [InlineData("SsoEmailNotVerified",
        "Your institution's sign-in did not confirm your email address, so Wombat cannot use it to find your account or " +
        "create one. Sign in with your password if you have one, or ask your administrator for an invitation.")]
    [InlineData("SsoAccountLocked", "Wombat could not sign you in through your institution. Contact your administrator.")]
    [InlineData("SsoAdministrator", "An administrator account signs in with its password, not through institutional sign-in.")]
    [InlineData("SsoWrongInstitution",
        "This account is not registered at the institution this sign-in belongs to. Contact your administrator.")]
    [InlineData("SsoEmailInUse",
        "A Wombat account already uses this email address, so a new one cannot be created. Contact your administrator.")]
    [InlineData("SsoAccountNotCreated",
        "Wombat could not create an account from your institution's sign-in. Contact your administrator.")]
    public async Task EachRefusal_ReadsAsTheSpecHasIt(string code, string expected)
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error={code}&returnUrl=%2Fprogress");

        var alert = document.QuerySelector(".alert.alert-danger")!;
        alert.TextContent.Trim().Should().Be(expected);
        alert.Id.Should().Be("login-error");
        document.QuerySelector("input[name=ReturnUrl]")!.GetAttribute("value").Should().Be("/progress",
            "the code travels beside the return address, which the form still carries");
    }

    /// <summary>
    /// The four notices read as the Spec has them, the lock's with the lockout Identity is configured with (T339, flow 02),
    /// each an info alert with role="status": nothing the person did here was refused. The tab carries no "Error:".
    /// </summary>
    [Theory]
    [InlineData("SessionEnded", "Your session has ended. Sign in again.")]
    [InlineData("PasswordChanged", "Your password was changed. Sign in with your new password.")]
    [InlineData("SignedOut", "You have signed out.")]
    [InlineData("LockedSignedOut",
        "Your current password was entered incorrectly too many times, so your account is locked for 15 minutes and you " +
        "have been signed out. Wait 15 minutes, then sign in again.")]
    public async Task EachNotice_IsAnInfoStatus_InTheSpecsWords(string code, string expected)
    {
        await using var host = await AppTestHost.StartAsync(services => services.Configure<IdentityOptions>(
            options => options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15)));

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error={code}");

        var notice = document.QuerySelectorAll(".alert").Should().ContainSingle().Which;
        notice.TextContent.Trim().Should().Be(expected);
        notice.ClassList.Should().Contain("alert-info");
        notice.GetAttribute("role").Should().Be("status");
        notice.Id.Should().Be("login-notice");
        document.Title.Should().Be("Sign in · Wombat");
    }

    [Fact]
    public async Task ANotice_WithNoInstitutionsButton_LeavesTheFocusOnEmail_WhichNamesIt()
    {
        // C6: with no button to reach first, Email takes the focus as it would with nothing to say, and reads the notice.
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error=SessionEnded");

        var notice = document.GetElementById("login-notice")!;
        notice.HasAttribute("tabindex").Should().BeFalse();
        notice.HasAttribute("autofocus").Should().BeFalse();
        var email = document.GetElementById("login-email")!;
        email.HasAttribute("autofocus").Should().BeTrue();
        email.GetAttribute("aria-describedby").Should().Be("login-notice");
        document.GetElementById("login-password")!.HasAttribute("aria-describedby").Should().BeFalse();
        document.QuerySelectorAll("[autofocus]").Should().ContainSingle();
    }

    [Fact]
    public async Task ANotice_BesideTheInstitutionsButtons_TakesTheFocusItself()
    {
        // C6: the notice is announced as it takes the focus, and the next Tab reaches the first institution.
        await using var host = await AppTestHost.StartAsync(OfferInstitutions);

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error=SessionEnded");

        var notice = document.GetElementById("login-notice")!;
        notice.GetAttribute("role").Should().Be("status");
        notice.GetAttribute("tabindex").Should().Be("-1");
        notice.HasAttribute("autofocus").Should().BeTrue();
        document.QuerySelectorAll("[autofocus]").Should().ContainSingle();
        document.GetElementById("login-email")!.HasAttribute("aria-describedby").Should().BeFalse();
        notice.NextElementSibling!.ClassList.Should().Contain("sso-providers", "the first institution is the next stop");
    }

    [Fact]
    public async Task ARefusal_BesideTheInstitutionsButtons_TakesTheFocus_AndSaysWhereTheButtonsAre()
    {
        await using var host = await AppTestHost.StartAsync(OfferInstitutions);

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error=Refused");

        var alert = document.GetElementById("login-error")!;
        alert.TextContent.Trim().Should().Be(
            "Invalid email or password. If your institution signs you in, use its sign-in button below.");
        alert.HasAttribute("autofocus").Should().BeTrue();
        document.QuerySelectorAll("[autofocus]").Should().ContainSingle();
    }

    [Fact]
    public async Task WithNothingToSay_EmailTakesTheFocus_WhenThereIsNoButton()
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync(SignInPath);

        document.QuerySelectorAll("[autofocus]").Should().ContainSingle().Which.Id.Should().Be("login-email");
    }

    [Fact]
    public async Task WithNothingToSay_NothingTakesTheFocus_BesideTheInstitutionsButtons()
    {
        // C6: an autofocused Email would carry a keyboard user past the buttons meant for them.
        await using var host = await AppTestHost.StartAsync(OfferInstitutions);

        var (_, _, document) = await host.LoadAsync(SignInPath);

        document.QuerySelectorAll("[autofocus]").Should().BeEmpty();
    }

    [Fact]
    public async Task TheInstitutionsButtons_StandAboveTheForm_UnderADividerThatSaysWhatTheFormIsFor()
    {
        // Q1: the buttons first, full width with the building icon, then "or sign in with your email and password".
        await using var host = await AppTestHost.StartAsync(OfferInstitutions);

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?returnUrl=%2Fprogress");

        var card = document.QuerySelector(".account-form-container")!;
        var blocks = card.Children.Select(child => string.IsNullOrEmpty(child.ClassName) ? child.TagName.ToLowerInvariant() : child.ClassName).ToList();
        blocks.IndexOf("sso-providers").Should().BeLessThan(blocks.IndexOf("sso-divider"));
        blocks.IndexOf("sso-divider").Should().BeLessThan(blocks.IndexOf("form"));

        var buttons = card.QuerySelectorAll(".sso-providers a.btn.btn-outline.sso-button");
        buttons.Select(button => button.TextContent.Trim()).Should().Equal($"Sign in with {Kgk}", "Sign in with Marula Test Hospital");
        buttons[0].GetAttribute("href").Should().Be("/account/sso-challenge/kgk?returnUrl=%2Fprogress");
        buttons.Should().OnlyContain(button => button.QuerySelector("svg use")!.GetAttribute("href") == "/icons/building-2.svg#i");
        card.QuerySelector(".sso-divider")!.TextContent.Trim().Should().Be("or sign in with your email and password");
    }

    /// <summary>
    /// Until T285 the page printed its <c>?error=</c> as it arrived: <c>?error=Call%20012</c> put "Call 012" on Wombat's
    /// own sign-in page, in its refusal's red. Now any code the page does not know reads as one general sentence.
    /// </summary>
    [Theory]
    [InlineData("Call%20012", "Call 012")]
    [InlineData("Your%20account%20is%20locked.%20Call%20012%20345%206789", "012 345 6789")]
    [InlineData("Invalid%20email%20or%20password.%20Reset%20it%20at%20evil.example", "evil.example")]
    [InlineData("%3Cscript%3Ealert(1)%3C%2Fscript%3E", "alert(1)")]
    public async Task WordsOfTheAddressesOwn_NeverReachThePage(string error, string words)
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, html, document) = await host.LoadAsync($"{SignInPath}?error={error}");

        document.QuerySelector(".alert.alert-danger")!.TextContent.Trim().Should().Be(SignInOutcome.GeneralRefusal)
            .And.Be("Sign-in could not be completed. Try again.");
        html.Should().NotContain(words);
        document.GetElementById("login-email")!.GetAttribute("aria-describedby").Should().Be("login-error");
    }

    [Theory]
    [InlineData("")]
    [InlineData("%20")]
    public async Task AnEmptyCode_NamesNoRefusal(string error)
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error={error}");

        document.QuerySelector(".alert").Should().BeNull();
        document.GetElementById("login-email")!.HasAttribute("aria-describedby").Should().BeFalse();
        document.Title.Should().Be("Sign in · Wombat");
    }

    [Fact]
    public async Task AFirstVisit_NamesNoRefusal()
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync(SignInPath);

        document.QuerySelector(".alert").Should().BeNull();
        document.GetElementById("login-email")!.HasAttribute("aria-describedby").Should().BeFalse(
            "there is no refusal for it to name");
        document.GetElementById("login-password")!.HasAttribute("aria-describedby").Should().BeFalse();
    }

    /// <summary>
    /// E4: the notices are for someone who has just stopped being signed in. A signed-in visitor who opens the page, with a
    /// notice's code or none, is sent Home and shown nothing.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("?error=SignedOut")]
    [InlineData("?error=SessionEnded&returnUrl=%2Fprogress")]
    public async Task ASignedInVisitor_IsSentHome(string query)
    {
        await using var host = await AppTestHost.StartAsync(SignedInVisitor.Register);

        using var response = await host.Client.GetAsync(SignInPath + query);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = response.Headers.Location!;
        (location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString).Should().Be("/");
    }

    private static void OfferInstitutions(IServiceCollection services) => services.Configure<SsoOptions>(options =>
        options.Providers =
        [
            new SsoProviderOptions { Key = "kgk", DisplayName = Kgk, InstitutionId = 1 },
            new SsoProviderOptions { Key = "marula", DisplayName = "Marula Test Hospital", InstitutionId = 2 }
        ]);

}
