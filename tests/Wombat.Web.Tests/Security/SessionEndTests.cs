using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Web.Components.Shared;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// The T279 review: a tab whose circuit's sign-in has ended leaves the circuit by a full page load of
/// <see cref="SessionEnd" />, which signs out a session the account no longer accepts, whatever the cookie's age, and sends
/// the browser to a sign-in page that loads signed out.
/// </summary>
/// <remarks>
/// <para>
/// Until the review the router sent such a tab to the sign-in page inside the circuit (<c>RedirectToLogin</c>,
/// <c>forceLoad: false</c>). That page's form carried the antiforgery token the circuit was given on its first page, naming
/// the old user; the post arrived with the old cookie, which the stamp validator refused, so the post was anonymous, the
/// token named someone else, and the sign-in endpoint answered 400 with an empty page. Every user the check signed out met
/// it. The integration suite's <c>SessionRevalidationFlowTests</c> runs the whole way back in over HTTP; these hold each
/// part to its half.
/// </para>
/// </remarks>
public sealed class SessionEndTests : TestContext
{
    private const string ReviewPage = "/committee/reviews/7?tab=evidence";

    private static readonly string SessionEndedUrl =
        $"{SessionEnd.Path}?returnUrl={Uri.EscapeDataString(ReviewPage)}";

    private static readonly string SignInAgainUrl =
        $"{SignInOutcome.PagePath}?error={SignInOutcome.SessionEnded}&returnUrl={Uri.EscapeDataString(ReviewPage)}";

    public SessionEndTests()
    {
        Services.AddScoped<EndedSessionExit>();
    }

    // ---- the tab leaves the circuit ----------------------------------------------------------------------------------

    [Fact]
    public void InACircuit_RedirectToLogin_LeavesByAFullLoadOfTheSessionEndedPage_ComingBackHere()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var navigation = At(ReviewPage);

        RenderComponent<RedirectToLogin>();

        var leave = navigation.History.First();
        leave.Uri.Should().Be(SessionEndedUrl);
        leave.Options.ForceLoad.Should().BeTrue(
            "the sign-in page inside the old circuit would post the old user's antiforgery token, and be refused with a 400");
    }

    [Fact]
    public void RenderedStatically_RedirectToLogin_SendsTheRequestToSignIn_ComingBackHere()
    {
        // The control: a request, with no circuit to leave and no token of the old user's, goes to the sign-in page as before.
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        var navigation = At(ReviewPage);

        RenderComponent<RedirectToLogin>();

        var redirect = navigation.History.First();
        redirect.Uri.Should().Be($"{SignInOutcome.PagePath}?returnUrl={Uri.EscapeDataString(ReviewPage)}");
        redirect.Options.ForceLoad.Should().BeFalse();
    }

    [Fact]
    public void InACircuit_TheMomentItsSignInEnds_TheTabLeaves_WhateverPageItIsOn_AndLeavesOnce()
    {
        // On a page open to anyone (the sign-in page itself, the not-found page) the router renders no RedirectToLogin, so
        // LeaveEndedSession, in Routes.razor, is what takes the tab out of the anonymous circuit. On a page the circuit may no
        // longer see both ask, and the tab is sent once.
        var authorization = this.AddTestAuthorization();
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        authorization.SetAuthorized("member@test");
        var navigation = At(ReviewPage);
        var before = navigation.History.Count;

        RenderComponent<LeaveEndedSession>();
        navigation.History.Count.Should().Be(before, "guard: nothing happens while the circuit is signed in");

        authorization.SetNotAuthorized();

        navigation.History.Count.Should().Be(before + 1);
        navigation.History.First().Uri.Should().Be(SessionEndedUrl);
        navigation.History.First().Options.ForceLoad.Should().BeTrue();

        RenderComponent<RedirectToLogin>();
        navigation.History.Count.Should().Be(before + 1, "the tab is already leaving");
    }

    [Fact]
    public void Routes_Listens_OnEveryPage_OutsideTheRouter()
    {
        // LeaveEndedSession only works where it is rendered. Routes.razor is the root every signed-in page renders in its
        // circuit, and outside the Router it is there whatever the page, the not-found page included.
        var routes = File.ReadAllText(WebFile("Components", "Routes.razor"));

        var router = routes.IndexOf("<Router", StringComparison.Ordinal);
        router.Should().BeGreaterThan(0, "guard: Routes.razor holds the router");
        routes[..router].Should().Contain("<LeaveEndedSession />");
    }

    [Fact]
    public void RenderedStatically_LeaveEndedSession_DoesNothing()
    {
        var authorization = this.AddTestAuthorization();
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        authorization.SetAuthorized("member@test");
        var navigation = At(ReviewPage);
        var before = navigation.History.Count;

        RenderComponent<LeaveEndedSession>();
        authorization.SetNotAuthorized();

        navigation.History.Count.Should().Be(before, "a request has no circuit to leave");
    }

    [Fact]
    public void ALeaveEndedSessionTakenAway_NoLongerListens()
    {
        var authorization = this.AddTestAuthorization();
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        authorization.SetAuthorized("member@test");
        var navigation = At(ReviewPage);
        var before = navigation.History.Count;

        RenderComponent<LeaveEndedSession>();
        DisposeComponents();
        authorization.SetNotAuthorized();

        navigation.History.Count.Should().Be(before);
    }

    [Theory]
    [InlineData("/committee/reviews", "/committee/reviews")]
    [InlineData("/committee/reviews/7?tab=evidence", "/committee/reviews/7?tab=evidence")]
    [InlineData("/", "/")]
    [InlineData("//evil.example/x", null)]
    [InlineData("/\\evil.example", null)]
    [InlineData("https://evil.example/", null)]
    [InlineData("committee/reviews", null)]
    [InlineData("/committee\t/reviews", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void OnlyAPathOnThisSite_IsCarriedBack(string? returnUrl, string? carried)
    {
        SessionEnd.LocalOrNull(returnUrl).Should().Be(carried);
        SessionEnd.Url(returnUrl).Should().Be(
            carried is null ? SessionEnd.Path : $"{SessionEnd.Path}?returnUrl={Uri.EscapeDataString(carried)}");
    }

    // ---- the endpoint ------------------------------------------------------------------------------------------------

    [Fact]
    public async Task ASessionTheAccountNoLongerAccepts_IsSignedOut_AndSentToSignIn_WhateverTheCookiesAge()
    {
        // The cookie handler lets a cookie issued less than a minute ago through unchecked, so the browser can arrive still
        // signed in as the old user. The endpoint asks the account itself.
        await using var identity = await IdentityFixture.CreateAsync();
        await identity.ChangeAsync(users => users.RemoveRoleAsync(identity.UserId, WombatRoles.Assessor));

        var (result, signOuts) = await identity.EndAsync(identity.Principal, ReviewPage);

        result.Url.Should().Be(SignInAgainUrl);
        signOuts.Should().Be(1);
    }

    [Fact]
    public async Task ASessionTheAccountStillAccepts_GoesBackToItsPage_StillSignedIn()
    {
        // Another session than the circuit's: the user signed in again in another tab, or the circuit ended on faults alone.
        // A GET here never signs it out, so a link to the endpoint cannot be used to.
        await using var identity = await IdentityFixture.CreateAsync();

        var (result, signOuts) = await identity.EndAsync(identity.Principal, ReviewPage);

        result.Url.Should().Be(ReviewPage);
        signOuts.Should().Be(0);
    }

    [Fact]
    public async Task AVisitorTheCookieHandlerAlreadySignedOut_IsSentToSignIn()
    {
        await using var identity = await IdentityFixture.CreateAsync();

        var (result, signOuts) = await identity.EndAsync(new ClaimsPrincipal(new ClaimsIdentity()), ReviewPage);

        result.Url.Should().Be(SignInAgainUrl);
        signOuts.Should().Be(1, "whatever the browser still holds is taken away");
    }

    [Fact]
    public async Task ASessionThatCannotBeChecked_IsSignedOut()
    {
        // The circuit that sent the browser here had already stopped trusting the session; a check that cannot be made
        // does not restore it.
        await using var identity = await IdentityFixture.CreateAsync();

        var (result, signOuts) = await identity.EndAsync(identity.Principal, ReviewPage, faulting: true);

        result.Url.Should().Be(SignInAgainUrl);
        signOuts.Should().Be(1);
    }

    [Theory]
    [InlineData("//evil.example/x")]
    [InlineData("https://evil.example/")]
    public async Task AnAddressOffThisSite_IsNeverFollowed(string returnUrl)
    {
        await using var identity = await IdentityFixture.CreateAsync();

        (await identity.EndAsync(identity.Principal, returnUrl)).Result.Url.Should().Be("/");
        (await identity.EndAsync(new ClaimsPrincipal(new ClaimsIdentity()), returnUrl)).Result.Url
            .Should().Be($"{SignInOutcome.PagePath}?error={SignInOutcome.SessionEnded}");
    }

    private static string WebFile(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && directory.GetFiles("Wombat.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        var root = directory?.FullName ?? throw new InvalidOperationException("Could not find Wombat.sln.");
        return Path.Combine([root, "src", "Wombat.Web", .. parts]);
    }

    private FakeNavigationManager At(string address)
    {
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        navigation.NavigateTo(address);
        return navigation;
    }

    /// <summary>An assessor's account over Identity as AddInfrastructure wires it, and the principal a sign-in built for it.</summary>
    private sealed class IdentityFixture : IAsyncDisposable
    {
        private readonly ServiceProvider _services;

        private IdentityFixture(ServiceProvider services, string userId, ClaimsPrincipal principal)
        {
            _services = services;
            UserId = userId;
            Principal = principal;
        }

        public string UserId { get; }

        public ClaimsPrincipal Principal { get; }

        public static async Task<IdentityFixture> CreateAsync()
        {
            var databaseName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpContextAccessor();
            services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName));
            services.AddIdentity<WombatIdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddClaimsPrincipalFactory<WombatUserClaimsPrincipalFactory>()
                .AddDefaultTokenProviders();
            services.AddScoped<IUserAdministrationService, UserAdministrationService>();
            var provider = services.BuildServiceProvider();

            await using var scope = provider.CreateAsyncScope();
            (await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().CreateAsync(new IdentityRole(WombatRoles.Assessor)))
                .Succeeded.Should().BeTrue();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            var user = new WombatIdentityUser
            {
                UserName = "assessor@example.test",
                Email = "assessor@example.test",
                FirstName = "Anna",
                LastName = "Assessor"
            };
            (await users.CreateAsync(user, "Assessor-Pa55word!")).Succeeded.Should().BeTrue();
            (await users.AddToRoleAsync(user, WombatRoles.Assessor)).Succeeded.Should().BeTrue();
            var principal = await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<WombatIdentityUser>>()
                .CreateAsync(user);

            return new IdentityFixture(provider, user.Id, principal);
        }

        public async Task ChangeAsync(Func<IUserAdministrationService, Task> change)
        {
            await using var scope = _services.CreateAsyncScope();
            await change(scope.ServiceProvider.GetRequiredService<IUserAdministrationService>());
        }

        /// <summary>The endpoint, for a request signed in as <paramref name="user" />: where it sends the browser, and how often it signed out.</summary>
        public async Task<(RedirectHttpResult Result, int SignOuts)> EndAsync(
            ClaimsPrincipal user,
            string? returnUrl,
            bool faulting = false)
        {
            await using var scope = _services.CreateAsyncScope();
            var signIns = ActivatorUtilities.CreateInstance<RecordingSignInManager>(scope.ServiceProvider);
            signIns.Faulting = faulting;
            var context = new DefaultHttpContext { RequestServices = scope.ServiceProvider, User = user };

            var result = await SessionEnd.HandleAsync(context, signIns, NullLoggerFactory.Instance, returnUrl);

            return (result.Should().BeOfType<RedirectHttpResult>().Subject, signIns.SignOuts);
        }

        public ValueTask DisposeAsync() => _services.DisposeAsync();
    }

    /// <summary>Identity's sign-in manager, counting its sign-outs, and failing its check as the database would when told to.</summary>
    private sealed class RecordingSignInManager(
        UserManager<WombatIdentityUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<WombatIdentityUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<WombatIdentityUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<WombatIdentityUser> confirmation)
        : SignInManager<WombatIdentityUser>(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
        public bool Faulting { get; set; }

        public int SignOuts { get; private set; }

        public override Task SignOutAsync()
        {
            SignOuts++;
            return Task.CompletedTask;
        }

        public override Task<WombatIdentityUser?> ValidateSecurityStampAsync(ClaimsPrincipal? principal)
            => Faulting
                ? throw new InvalidOperationException("A fault the test put here: the database has gone.")
                : base.ValidateSecurityStampAsync(principal);
    }
}
