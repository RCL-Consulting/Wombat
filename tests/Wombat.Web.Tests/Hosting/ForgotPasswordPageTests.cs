using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Options;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// Forgotten password as a browser receives it (T339, flow 02; R3-FG, R3-FG-390; Q2 A): who to ask, three steps and the
/// way back, with no field. Worded for an account with an institution and one without (C4); the line about an
/// institution's own sign-in only where the sign-in page offers one.
/// </summary>
public sealed class ForgotPasswordPageTests
{
    private const string Path = "/account/forgot-password";

    [Fact]
    public void ThisIsTheHostsAnonymousPage()
        => AppTestHost.AnonymousPage.Should().Be(Path, "guard: the tests below read the page the host names");

    [Fact]
    public async Task ThePage_SaysWhoToAsk_InThreeSteps_WithNoField_AndTheWayBack()
    {
        await using var host = await AppTestHost.StartAsync();

        var (_, _, document) = await host.LoadAsync(Path);

        document.Title.Should().Be("Forgotten password · Wombat");
        var card = document.QuerySelector(".account-form-container")!;
        card.QuerySelector("h1")!.TextContent.Trim().Should().Be("Forgotten password");
        card.QuerySelector("h1 + p")!.TextContent.Trim().Should().Be("A Wombat administrator can set a new password for you.");
        card.QuerySelectorAll("ol.account-steps > li").Select(step => Collapse(step.TextContent)).Should().Equal(
            "Ask your Wombat administrator for a new password: the one at your institution or, if your account belongs to no " +
            "institution, the platform's administrator. They give it to you themselves; Wombat does not email it.",
            "Sign in with it.",
            "Choose your own password on My account, under Change password.");
        card.QuerySelectorAll("input:not([type=hidden])").Should().BeEmpty();
        card.QuerySelector(".alert").Should().BeNull("nothing is refused or reported here");

        var back = card.QuerySelector(".form-actions a.btn.btn-outline")!;
        back.TextContent.Trim().Should().Be("Back to sign in");
        back.GetAttribute("href").Should().Be("/account/login");
        back.QuerySelector("svg use")!.GetAttribute("href").Should().Be("/icons/arrow-left.svg#i");

        card.QuerySelector(".account-note").Should().BeNull("no institution signs anyone in on this host");
    }

    [Fact]
    public async Task WhereAnInstitutionSignsPeopleIn_ThePageSaysItsPasswordIsTheInstitutions()
    {
        await using var host = await AppTestHost.StartAsync(services => services.Configure<SsoOptions>(options =>
            options.Providers = [new SsoProviderOptions { Key = "kgk", DisplayName = "Kgosi Kgari Teaching Hospital", InstitutionId = 1 }]));

        var (_, _, document) = await host.LoadAsync(Path);

        Collapse(document.QuerySelector(".account-note")!.TextContent).Should().Be(
            "If your institution signs you in, use its button on the sign-in page. Your institution resets that password, not Wombat.");
    }

    private static string Collapse(string text) => System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
}
