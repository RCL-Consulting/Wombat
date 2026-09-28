using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Accounts;
using Wombat.Domain.Identity;
using Wombat.Web.Security;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Hosting;

/// <summary>
/// Change password as a signed-in browser receives it (T339, flow 02; R3-CP-*): static in the shell. Until T339 it was the
/// one account page a signed-in person reached that stayed interactive, for its Show buttons; now it is excluded from
/// interactive routing (App.razor), so the full load carries no interactive root, and the toggles are wombat.js's.
/// </summary>
public sealed class ChangePasswordPageHostingTests
{
    [Fact]
    public async Task ThePage_IsStatic_InTheShell_WithNoCircuit()
    {
        await using var host = await StartAsync(hasPassword: true);

        var (_, html, document) = await host.LoadAsync(ChangePasswordOutcome.PagePath);

        ServerComponentMarker.IsIn(html).Should().BeFalse("[ExcludeFromInteractiveRouting]: the form is an HTTP post");
        document.QuerySelector("nav[aria-label='Main']").Should().NotBeNull("the shell works without a circuit, as /Error shows");
        document.QuerySelectorAll("nav[aria-label='Breadcrumb'] li").Select(item => item.TextContent.Trim())
            .Should().Equal("Home", "My account", "Change password");
        document.QuerySelector("main h1")!.TextContent.Trim().Should().Be("Change password");
        document.Title.Should().Be("Change password · Wombat");
        document.QuerySelectorAll("[autofocus]").Should().BeEmpty("a full load: the focus is at the top of the page");

        PasswordToggleMarkup.ShouldBeTheToggle(document, "current-password", "Show current password", PasswordToggleMarkup.Driven.ByScript);
        PasswordToggleMarkup.ShouldBeTheToggle(document, "new-password", "Show new password", PasswordToggleMarkup.Driven.ByScript);
        PasswordToggleMarkup.ShouldBeTheToggle(document, "confirm-password", "Show confirm new password", PasswordToggleMarkup.Driven.ByScript);
        document.QuerySelector("form input[name=__RequestVerificationToken]").Should().NotBeNull("the post carries its token");
    }

    [Fact]
    public async Task AnAccountWithNoPassword_GetsTheInfoState_OnItsFirstVisit()
    {
        await using var host = await StartAsync(hasPassword: false);

        var (_, html, document) = await host.LoadAsync(ChangePasswordOutcome.PagePath);

        ServerComponentMarker.IsIn(html).Should().BeFalse();
        document.QuerySelector("form[action='/account/change-password/submit']").Should().BeNull();
        document.QuerySelector("main .alert.alert-info")!.TextContent.Trim()
            .Should().Be("This account signs in through your institution, so it has no password to change here.");
        document.QuerySelector("main .page-subtitle").Should().BeNull("no subtitle on this state (C9)");
    }

    private static Task<AppTestHost> StartAsync(bool hasPassword)
        => AppTestHost.StartAsync(services =>
        {
            SignedInVisitor.Register(services);
            services.AddSingleton(new IdentityErrorDescriber());
            services.AddSingleton<IScopedSender>(new ProfileSender(hasPassword));
        });

    /// <summary>Answers the page's one read: the account, with or without a password of its own.</summary>
    private sealed class ProfileSender(bool hasPassword) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetCurrentUserProfileQuery
                ? Task.FromResult((TResponse)(object)new UserProfileDto(
                    "signed-in-visitor", "visitor@hospital.test", "Signed-in", "Visitor", [WombatRoles.Assessor])
                    {
                        HasLocalPassword = hasPassword
                    })
                : throw new InvalidOperationException($"The page sent {request.GetType().Name}.");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"The page sent {request.GetType().Name}.");
    }
}
