using FluentAssertions;

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

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error=Invalid%20email%20or%20password.");

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

        var (_, _, document) = await host.LoadAsync($"{SignInPath}?error=Invalid%20email%20or%20password.");

        var alert = document.QuerySelector(".alert.alert-danger")!;
        alert.Id.Should().Be("login-error");
        document.GetElementById("login-email")!.HasAttribute("autofocus").Should().BeTrue();
        document.GetElementById("login-email")!.GetAttribute("aria-describedby").Should().Be("login-error");
        document.GetElementById("login-password")!.GetAttribute("aria-describedby").Should().Be("login-error");
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
