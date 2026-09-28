using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T284: the lock card on a user's page says what a lock does beyond sign-in, before the administrator presses Lock out.
/// </summary>
/// <remarks>
/// Until T284 it said only that locking "prevents sign-in immediately". Since T165, T240, T268 and T284 a lock also
/// takes the user off every committee panel, stops them being a current trainee (no review or feedback campaign started
/// about them, no draft about them opened, off the programme's lists), and stops their reminders; since T102 nobody can
/// name them as an assessor. An administrator locking a trainee who sits on a panel should know all of that first.
/// </remarks>
public sealed class UserDetailLockCardTests : TestContext
{
    public UserDetailLockCardTests()
    {
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("admin@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));

        // The reset-password field's show/hide toggle calls into the browser; it is not what these tests are about.
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void BeforeALock_TheCardNamesWhatALockDoes_BesideTheButton()
    {
        var card = LockoutCard(Render(User(isLockedOut: false)));

        Text(card.QuerySelector("p")!).Should().Be("Locking out the user prevents sign-in immediately. Until they are reactivated:");
        card.QuerySelectorAll("#lockout-effects li").Select(Text).Should().Equal(
            "They sit on no committee panel.",
            "They are not a current trainee. No committee review or feedback campaign can be started about them, a draft " +
            "feedback campaign about them cannot be opened, and the programme's lists of its trainees leave them out: the " +
            "speciality, sub-speciality and committee dashboards' trainee targets and counts, feedback coverage, decisions " +
            "due, and the weekly digest's inactive trainees.",
            "They are sent no activity reminders or weekly digests.",
            "They cannot be named as an activity's assessor.");
        Text(card).Should().Contain(
            "Their records stay as they are, and a review or feedback campaign already under way can still be finished. " +
            "Reactivating clears the lockout.");

        // The effects come before the button, so they are read before it is reached.
        var button = card.QuerySelectorAll("button").Single(candidate => Text(candidate) == "Lock out user");
        card.QuerySelector("#lockout-effects")!.CompareDocumentPosition(button)
            .Should().HaveFlag(DocumentPositions.Following);
    }

    [Fact]
    public void ForALockedUser_TheCardOffersReactivation_AndListsNoEffects()
    {
        var card = LockoutCard(Render(User(isLockedOut: true)));

        Text(card).Should().Contain("This user is currently locked out and cannot sign in.");
        card.QuerySelectorAll("button").Select(Text).Should().Equal("Reactivate user");
        card.QuerySelector("#lockout-effects").Should().BeNull();
    }

    [Fact]
    public void ForAnAdministrator_TheCardOffersNoLock_AndListsNoEffects()
    {
        var card = LockoutCard(Render(User(isLockedOut: false) with { Roles = [WombatRoles.Administrator] }));

        Text(card).Should().Contain("Global administrators cannot be locked out from this surface.");
        card.QuerySelector("#lockout-effects").Should().BeNull();
    }

    private IRenderedComponent<UserDetail> Render(UserDetailDto user)
    {
        Services.AddSingleton<IScopedSender>(new FakeSender(user));
        // PasswordField reads it (T339, flow 02): this page is interactive, so its toggles are the circuit's.
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var cut = RenderComponent<UserDetail>(parameters => parameters.Add(page => page.UserId, user.UserId));
        cut.WaitForState(() => cut.FindAll("section.detail-card h3").Any(heading => Text(heading) == "Lockout"));
        return cut;
    }

    private static IElement LockoutCard(IRenderedComponent<UserDetail> cut)
        => cut.FindAll("section.detail-card").Single(section => Text(section.QuerySelector("h3")!) == "Lockout");

    private static UserDetailDto User(bool isLockedOut) => new(
        "trainee-9", "registrar@hospital.test", "Rene", "Registrar", 4, "Groote Schuur Hospital", [], [],
        [WombatRoles.Trainee, WombatRoles.CommitteeMember], isLockedOut, []);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(UserDetailDto user) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetUserByIdQuery
                ? Task.FromResult((TResponse)(object)user)
                : throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
