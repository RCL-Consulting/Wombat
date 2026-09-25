using FluentAssertions;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// The sign-in page as a browser receives it (T193). Its fields say what they hold, so a password manager fills the
/// saved account's email and password, and a refused sign-in is announced as the page loads.
/// </summary>
/// <remarks>
/// Served through <see cref="AppTestHost" /> because the page is static HTML for a visitor who has not signed in (T181):
/// what matters is the markup of the full load, not a component rendered in isolation.
/// </remarks>
public sealed class SignInPageFieldTests
{
    private const string SignInPath = "/account/login";

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
    public async Task ARefusedSignIn_IsAnAlert()
    {
        // The sign-in endpoint redirects a refusal back here with the reason in the query. The page reloads, and the
        // reason is the one thing on it a screen reader must say first.
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error=Refused");

        var alert = document.QuerySelector(".alert.alert-danger")!;
        alert.TextContent.Should().Contain("Invalid email or password.");
        alert.GetAttribute("role").Should().Be("alert");
    }

    [Fact]
    public async Task ARefusedSignIn_IsReadWithTheFieldThatTakesFocus()
    {
        // An alert already on the page when it loads is not reliably announced, and the email field takes focus at once,
        // so a reader would say "Email, edit" and nothing else. Both fields name the refusal, so it is read with them.
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error=Refused");

        var alert = document.QuerySelector(".alert.alert-danger")!;
        alert.Id.Should().Be("login-error");
        document.GetElementById("login-email")!.HasAttribute("autofocus").Should().BeTrue();
        document.GetElementById("login-email")!.GetAttribute("aria-describedby").Should().Be("login-error");
        document.GetElementById("login-password")!.GetAttribute("aria-describedby").Should().Be("login-error");
    }

    /// <summary>
    /// Each code the endpoints send the sign-in page reads as the refusal did before T285, when they sent the words.
    /// Written out, so a changed word fails here. This host offers no institutional sign-in, so a refused sign-in says only
    /// that the email or password is wrong.
    /// </summary>
    [Theory]
    [InlineData("FieldsMissing", "Email and password are required.")]
    [InlineData("Refused", "Invalid email or password.")]
    [InlineData("TooManyAttempts", "Too many failed sign-in attempts from this network. Please wait a few minutes and try again.")]
    [InlineData("LockedOut", "Too many failed sign-in attempts. Please try again later or reset your password.")]
    [InlineData("SessionEnded", "Your session has ended. Please sign in again.")]
    [InlineData("PasswordChanged", "Your password was changed. Please sign in with your new password.")]
    [InlineData("ExternalLoginUnavailable", "External login information was not available.")]
    [InlineData("ExternalSessionExpired", "External login session expired. Please try again.")]
    [InlineData("SsoFailed", "SSO login failed.")]
    [InlineData("SsoUnknownProvider", "Unknown SSO provider.")]
    [InlineData("SsoNoEmail", "The identity provider did not supply an email address.")]
    [InlineData("SsoEmailNotVerified",
        "Your institution's sign-in did not confirm your email address, so Wombat cannot use it to find your account or " +
        "create one. Sign in with your password if you have one, or ask your administrator for an invitation.")]
    [InlineData("SsoAccountLocked", "This account is locked. Contact your administrator.")]
    [InlineData("SsoAdministrator", "An administrator account signs in with its password, not through institutional sign-in.")]
    [InlineData("SsoWrongInstitution",
        "This account is not registered at the institution this sign-in belongs to. Contact your administrator.")]
    [InlineData("SsoEmailInUse",
        "A Wombat account already uses this email address, so a new one cannot be created. Contact your administrator.")]
    [InlineData("SsoAccountNotCreated",
        "Wombat could not create an account from your institution's sign-in. Contact your administrator.")]
    public async Task EachRefusal_ReadsAsItDidBefore(string code, string expected)
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
            .And.Be("Sign-in could not be completed. Please try again.");
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
}
