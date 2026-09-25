using FluentAssertions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Infrastructure.Identity;

namespace Wombat.Infrastructure.Tests.Identity;

/// <summary>
/// T156: the external cookie, as the host composes it. Identity registers the cookie with its own options, and
/// AddInfrastructure's must come after them to count. The cookie's behaviour in a request, that it does not slide and
/// what a browser is sent, is driven in the web suite (<c>ExternalSignInCookieHostingTests</c>).
/// </summary>
public sealed class ExternalSignInCookieTests
{
    [Fact]
    public void TheHostsExternalCookie_DoesNotSlide_AndIsAHostOnlySecureCookie()
    {
        // No connection is opened: the options are all that is read.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=t156_never_opened"
            })
            .Build();

        using var provider = new ServiceCollection()
            .AddLogging()
            .AddInfrastructure(configuration)
            .BuildServiceProvider();

        var options = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ExternalScheme);

        options.SlidingExpiration.Should().BeFalse("a provider's sign-in ends five minutes after its callback, however it is used");
        options.ExpireTimeSpan.Should().Be(TimeSpan.FromMinutes(5));
        options.Cookie.Name.Should().Be("__Host-Identity.External");
        options.Cookie.SecurePolicy.Should().Be(CookieSecurePolicy.Always, "a __Host- cookie must be Secure, or a browser drops it");
        options.Cookie.Path.Should().Be("/", "a __Host- cookie is for the whole site");
        options.Cookie.Domain.Should().BeNull("a __Host- cookie names no domain");
        options.Cookie.HttpOnly.Should().BeTrue();

        // The application cookie is not this one, and keeps its own name and sliding eight hours.
        var application = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);
        application.Cookie.Name.Should().NotStartWith("__Host-");
        application.SlidingExpiration.Should().BeTrue();
    }
}
