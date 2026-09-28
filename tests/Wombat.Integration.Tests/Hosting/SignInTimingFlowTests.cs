extern alias WombatWeb;

using System.Net;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Wombat.Infrastructure.Identity;
using Wombat.Integration.Tests.MultiSourceFeedback;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T287 in T339, flow 02 (the round 2 review's A1, A2 and E5): every refused sign-in is sent to the same address and costs
/// one password check, whatever refused it: a wrong password, an address no account has, an account that signs in only
/// through its institution, or a locked account. So neither the address nor a stopwatch says which addresses have
/// accounts, or which are locked.
/// </summary>
/// <remarks>
/// The time is not measured: a wall clock in a test is noise. What costs the time is one verification of a password hash,
/// so the app's hasher is wrapped to count them, and each refusal must verify exactly one, as a wrong password does. Until
/// T339 the unknown, institutional and locked paths verified none, and a lockout was sent to <c>?error=LockedOut</c>.
/// </remarks>
public sealed class SignInTimingFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string WrongPassword = "Not-the-Pa55word!";
    private const string Refused = "/account/login?error=Refused";

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public SignInTimingFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task EveryRefusal_IsSentToTheSameAddress_AfterOnePasswordCheck()
    {
        var checks = new CheckCounter();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPasswordHasher<WombatIdentityUser>>();
            services.AddScoped<IPasswordHasher<WombatIdentityUser>>(provider => new CountingPasswordHasher(
                new PasswordHasher<WombatIdentityUser>(provider.GetService<IOptions<PasswordHasherOptions>>()), checks));
        }));

        // A wrong password: the one check every refusal is measured against.
        var active = await NewUserAsync();
        (await RefusalAsync(app, checks, active, WrongPassword)).Should().Be((Refused, 1));

        // An address no account has.
        (await RefusalAsync(app, checks, $"nobody-{Guid.NewGuid():N}@example.test", WrongPassword)).Should().Be((Refused, 1));

        // An account that signs in only through its institution: the endpoint refuses it before any check of its own.
        var institutional = await NewUserAsync(user => user.AllowLocalPassword = false);
        (await RefusalAsync(app, checks, institutional, MsfRespondPageFlowTests.WebHost.SignInPassword))
            .Should().Be((Refused, 1));

        // An account an administrator has deactivated: Identity answers the lock before it checks any password, even the
        // right one.
        var deactivated = await NewUserAsync(user => user.LockoutEnd = UserDeactivation.IndefiniteLockoutEnd);
        (await RefusalAsync(app, checks, deactivated, MsfRespondPageFlowTests.WebHost.SignInPassword))
            .Should().Be((Refused, 1));

        // An account the fifth wrong password locks: the fifth checks its password, and the lock that follows costs one.
        var guessed = await NewUserAsync();
        for (var guess = 1; guess <= 4; guess++)
        {
            (await RefusalAsync(app, checks, guessed, WrongPassword)).Should().Be((Refused, 1));
        }

        (await RefusalAsync(app, checks, guessed, WrongPassword)).Should().Be((Refused, 1), "the fifth trips the lock");
        (await RefusalAsync(app, checks, guessed, MsfRespondPageFlowTests.WebHost.SignInPassword))
            .Should().Be((Refused, 1), "locked now, the right password is refused as a wrong one, after one check's time");
    }

    /// <summary>Loads the sign-in page and posts its form from a new browser; where it was sent, and how many hashes it checked.</summary>
    private static async Task<(string Location, int Checks)> RefusalAsync(
        WebApplicationFactory<WombatWeb::Program> app,
        CheckCounter checks,
        string email,
        string password)
    {
        using var browser = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.240.{Interlocked.Increment(ref _nextSubnet)}.7");

        using var page = await browser.GetAsync("/account/login");
        var token = new HtmlParser().ParseDocument(await page.Content.ReadAsStringAsync())
            .QuerySelector("form[action='/account/login/submit'] input[name=__RequestVerificationToken]")!.GetAttribute("value")!;

        var before = checks.Count;
        using var answer = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", token),
            new("Email", email),
            new("Password", password)
        ]));

        answer.StatusCode.Should().Be(HttpStatusCode.Redirect);
        return (answer.Headers.Location!.ToString(), checks.Count - before);
    }

    private async Task<string> NewUserAsync(Action<WombatIdentityUser>? change = null)
    {
        var email = $"timing-{Guid.NewGuid():N}@example.test";
        var user = await _host.CreateAssessorAsync(email);
        if (change is not null)
        {
            await using var scope = _host.Factory.Services.CreateAsyncScope();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
            var stored = (await users.FindByIdAsync(user.Id))!;
            change(stored);
            (await users.UpdateAsync(stored)).Succeeded.Should().BeTrue("guard: the account is as the test needs it");
        }

        return email;
    }

    private sealed class CheckCounter
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public void Add() => Interlocked.Increment(ref _count);
    }

    /// <summary>The app's hasher, counting each password it verifies.</summary>
    private sealed class CountingPasswordHasher(IPasswordHasher<WombatIdentityUser> inner, CheckCounter checks)
        : IPasswordHasher<WombatIdentityUser>
    {
        public string HashPassword(WombatIdentityUser user, string password) => inner.HashPassword(user, password);

        public PasswordVerificationResult VerifyHashedPassword(WombatIdentityUser user, string hashedPassword, string providedPassword)
        {
            checks.Add();
            return inner.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }
    }
}
