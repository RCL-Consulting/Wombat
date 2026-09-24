using FluentAssertions;
using Wombat.Application.Common.Options;

namespace Wombat.Application.Tests.Common.Options;

/// <summary>
/// The link every MSF invitation and reminder carries (T132, T205). Unset, it is the web app's own respondent page;
/// set, it may point nowhere but the web app's origin, because a wrong value opens a campaign without complaint and mails
/// every respondent a link nobody can use.
/// </summary>
public sealed class WombatOptionsExtensionsTests
{
    [Theory]
    [InlineData("https://wombat.example.test", "https://wombat.example.test/msf/respond")]
    [InlineData("https://wombat.example.test/", "https://wombat.example.test/msf/respond")]
    [InlineData("http://localhost:5080", "http://localhost:5080/msf/respond")]
    public void Unset_ItIsTheWebAppsOwnPage_OnTheBaseUrl(string baseUrl, string expected)
    {
        new WombatOptions { BaseUrl = baseUrl, MsfRespondUrl = "  " }.RequireMsfRespondUrl().Should().Be(expected);
        new WombatOptions { BaseUrl = baseUrl }.RequireMsfRespondUrl().Should().Be(expected);
    }

    [Theory]
    [InlineData("https://wombat.example.test", "https://wombat.example.test/msf/respond/", "https://wombat.example.test/msf/respond")]
    [InlineData("https://WOMBAT.example.test", "https://wombat.example.test:443/msf/respond", "https://wombat.example.test/msf/respond")]
    [InlineData(null, "http://localhost/msf/respond", "http://localhost/msf/respond")]
    [InlineData("", "https://wombat.example.test/msf/respond", "https://wombat.example.test/msf/respond")]
    public void Set_OnTheBaseUrlsOrigin_OrWithNoBaseUrl_ItIsUsedAsGiven(string? baseUrl, string respondUrl, string expected)
        => new WombatOptions { BaseUrl = baseUrl, MsfRespondUrl = respondUrl }.RequireMsfRespondUrl().Should().Be(expected);

    /// <summary>
    /// The mistakes a deploy makes: the Api host's address (port 5090, where dev pointed until T205), http for an https
    /// site, and another host. Each opened the campaign and mailed a dead link; each is now refused, and says what to do.
    /// </summary>
    [Theory]
    [InlineData("https://wombat.example.test:5090/msf/respond")]
    [InlineData("http://wombat.example.test/msf/respond")]
    [InlineData("https://elsewhere.example.test/msf/respond")]
    public void Set_OffTheBaseUrlsOrigin_IsRefused_AndTheRefusalSaysWhatToDo(string respondUrl)
    {
        var require = () => new WombatOptions { BaseUrl = "https://wombat.example.test", MsfRespondUrl = respondUrl }
            .RequireMsfRespondUrl();

        require.Should().Throw<InvalidOperationException>()
            .WithMessage("*must be on Wombat:BaseUrl's scheme, host and port (https://wombat.example.test)*")
            .WithMessage("*Leave it unset to use https://wombat.example.test/msf/respond*");
    }

    [Theory]
    [InlineData("/msf/respond")]
    [InlineData("msf/respond")]
    [InlineData("ftp://wombat.example.test/msf/respond")]
    public void Set_ToAnythingButAnAbsoluteWebUrl_IsRefused(string respondUrl)
    {
        var require = () => new WombatOptions { MsfRespondUrl = respondUrl }.RequireMsfRespondUrl();

        require.Should().Throw<InvalidOperationException>().WithMessage("*must be an absolute http or https URL*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/relative")]
    public void NeitherItNorAWebBaseUrl_IsRefused(string? baseUrl)
    {
        var require = () => new WombatOptions { BaseUrl = baseUrl }.RequireMsfRespondUrl();

        require.Should().Throw<InvalidOperationException>().WithMessage("*Wombat:MsfRespondUrl, or Wombat:BaseUrl*");
    }
}
