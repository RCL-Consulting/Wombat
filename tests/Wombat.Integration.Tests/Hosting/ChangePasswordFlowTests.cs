extern alias WombatWeb;

using System.Net;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Infrastructure.Identity;
using WombatWeb::Wombat.Web.Security;
using Wombat.Integration.Tests.MultiSourceFeedback;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T265 end to end: a signed-in user changes their password through the page's own form, and stays signed in, on a
/// cookie issued again with the account's new security stamp; the cookie they held before the change is refused.
/// </summary>
/// <remarks>
/// <para>
/// The whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a PostgreSQL schema of its own, hosted by the MSF respondent
/// page's fixture (<see cref="MsfRespondPageFlowTests.WebHost" />): Identity, the fallback policy, antiforgery and the
/// Razor-components endpoint. A browser is a client with its own cookies and its own address, so its sign-in throttle
/// is its own.
/// </para>
/// <para>
/// Until T265 the page changed the password in its circuit and then called <c>RefreshSignInAsync</c> there, which threw
/// "Headers are read-only" (confirmed in the browser, 2026-09-25): the circuit's response had started long before. The
/// password was changed and the page crashed. A cookie can only be issued by an HTTP request, so the form now posts to
/// <c>/account/change-password/submit</c>, which changes the password and issues the cookie in the one request.
/// </para>
/// </remarks>
public sealed class ChangePasswordFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string PagePath = "/account/change-password";
    private const string SubmitPath = "/account/change-password/submit";
    private const string SignInCookie = ".AspNetCore.Identity.Application";
    private const string NewPassword = "Changed-Pa55word!";

    private static readonly Uri Origin = new("http://localhost");

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public ChangePasswordFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    [Fact]
    public async Task ChangingAPassword_IssuesTheSignInCookieAgain_SoTheUserStaysSignedIn_AndTheOldCookieIsRefused()
    {
        // The security stamp checked on every request, not once in thirty minutes (Identity's default), so a stale
        // cookie is refused at once rather than at the next check.
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero)));

        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, cookies) = NewBrowser(app);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var cookieBefore = SignInCookieOf(cookies);
            var stampBefore = await StampAsync(email);

            using var submit = await PostTheFormAsync(browser, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword);

            submit.StatusCode.Should().Be(HttpStatusCode.Redirect);
            submit.Headers.Location!.ToString().Should().Be($"{PagePath}?status=updated");
            submit.Headers.GetValues("Set-Cookie").Should().Contain(
                cookie => cookie.StartsWith($"{SignInCookie}=", StringComparison.Ordinal),
                "the sign-in cookie is issued again in the response to the change");
            (await StampAsync(email)).Should().NotBe(stampBefore, "changing the password changes the security stamp");
            SignInCookieOf(cookies).Should().NotBe(cookieBefore);

            // Still signed in: the page the redirect names loads, and says what happened.
            using var answer = await browser.GetAsync(submit.Headers.Location);
            answer.StatusCode.Should().Be(HttpStatusCode.OK, $"not sent to sign in again ({answer.Headers.Location})");
            var page = Parse(await answer.Content.ReadAsStringAsync());
            page.QuerySelector(".action-result .alert-success")!.TextContent.Trim().Should().Be("Password updated.");
            page.QuerySelector(".top-row.auth")!.TextContent.Should().Contain(email, "signed in, as themselves");

            // The cookie from before the change carries the old stamp, and is refused.
            using var stale = app.CreateDefaultClient(Origin);
            using var staleRequest = new HttpRequestMessage(HttpMethod.Get, PagePath);
            staleRequest.Headers.Add("Cookie", cookieBefore);
            using var refused = await stale.SendAsync(staleRequest);
            refused.StatusCode.Should().Be(HttpStatusCode.Redirect, "the old security stamp no longer signs anyone in");
            refused.Headers.Location!.ToString().Should().Contain("/account/login");
        }

        // The new password signs in, and the old one no longer does.
        var (fresh, _) = NewBrowser(app);
        using (fresh)
        {
            (await SignInAsync(fresh, email, MsfRespondPageFlowTests.WebHost.SignInPassword, expectSuccess: false))
                .Should().Contain("/account/login?error=", "the old password is refused");
            await SignInAsync(fresh, email, NewPassword);
        }
    }

    [Theory]
    [InlineData("Not-the-Pa55word!", NewPassword, NewPassword, "PasswordMismatch", "Incorrect password.")]
    [InlineData(MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, "Changed-Pa55word?", "ConfirmationMismatch",
        "The password confirmation does not match.")]
    [InlineData(MsfRespondPageFlowTests.WebHost.SignInPassword, "short1A!", "short1A!", "PasswordTooShort",
        "Passwords must be at least 12 characters.")]
    [InlineData(" ", " ", " ", "FieldsMissing",
        "Enter your current password, a new password, and the new password again to confirm it.")]
    public async Task ARefusedChange_IsSaidOnThePage_TheUserStaysSignedIn_AndNothingChanges(
        string current,
        string replacement,
        string confirmation,
        string code,
        string sentence)
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(_host.Factory);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var stamp = await StampAsync(email);

            using var submit = await PostTheFormAsync(browser, current, replacement, confirmation);

            submit.StatusCode.Should().Be(HttpStatusCode.Redirect);
            submit.Headers.Location!.ToString().Should().Be($"{PagePath}?error={code}");
            (await StampAsync(email)).Should().Be(stamp, "a refused change changes nothing");

            using var answer = await browser.GetAsync(submit.Headers.Location);
            answer.StatusCode.Should().Be(HttpStatusCode.OK);
            Parse(await answer.Content.ReadAsStringAsync()).QuerySelector(".action-result .alert-danger")!.TextContent.Trim()
                .Should().Be(sentence);
        }

        var (fresh, _) = NewBrowser(_host.Factory);
        using (fresh)
        {
            await SignInAsync(fresh, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
        }
    }

    [Fact]
    public async Task APostWithoutThePagesAntiforgeryToken_IsRefused_AndChangesNothing()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(_host.Factory);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var stamp = await StampAsync(email);

            // Another site's form, posted with the user's cookies: it cannot read the page, so it has no token.
            using var forged = await browser.PostAsync(SubmitPath, new FormUrlEncodedContent(
            [
                new("CurrentPassword", MsfRespondPageFlowTests.WebHost.SignInPassword),
                new("NewPassword", NewPassword),
                new("ConfirmPassword", NewPassword)
            ]));

            forged.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            (await StampAsync(email)).Should().Be(stamp);
        }
    }

    [Fact]
    public async Task AVisitorWhoHasNotSignedIn_IsSentToSignIn_AndComesBackToThePage()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(_host.Factory);
        using (browser)
        {
            // The page left open past the cookie's expiry, then pressed: the post is not signed in.
            using var post = await browser.PostAsync(SubmitPath, new FormUrlEncodedContent(
            [
                new("CurrentPassword", "x"),
                new("NewPassword", NewPassword),
                new("ConfirmPassword", NewPassword)
            ]));

            post.StatusCode.Should().Be(HttpStatusCode.Redirect);
            var challenge = post.Headers.Location!;
            challenge.AbsolutePathOf().Should().Be("/account/login");
            var returnUrl = QueryValue(challenge, "ReturnUrl");
            returnUrl.Should().Be(SubmitPath, "the sign-in page is sent where the post was going");

            // Signing in sends the browser there with a GET, which lands on the page, not on a blank 405 (T265 review).
            var afterSignIn = await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword, returnUrl: returnUrl);
            afterSignIn.Should().Be(SubmitPath);
            using var get = await browser.GetAsync(afterSignIn);
            get.StatusCode.Should().Be(HttpStatusCode.Redirect);
            get.Headers.Location!.ToString().Should().Be(PagePath);
            using var page = await browser.GetAsync(get.Headers.Location);
            page.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task ASessionEndedByLockingTheAccount_CannotChangeThePassword_AndStaysEnded()
    {
        // Identity's own interval: the stamp is checked once in thirty minutes, on a clock the test moves.
        var clock = new MovableClock();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Configure<SecurityStampValidatorOptions>(options => options.TimeProvider = clock)));

        var email = NewEmail();
        var user = await _host.CreateAssessorAsync(email);
        var (browser, cookies) = NewBrowser(app);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var cookieBefore = SignInCookieOf(cookies);

            // An administrator's "Lock out user": the account is deactivated and its stamp changed, which ends the session
            // at the stamp validator's next check. Until then the cookie still signs the browser in (guard, in the form's
            // load below).
            await using (var scope = app.Services.CreateAsyncScope())
            {
                await scope.ServiceProvider.GetRequiredService<IUserAdministrationService>().SetLockoutAsync(user.Id, locked: true);
            }

            var stampAfterLock = await StampAsync(email);

            // Whoever holds the session, and knows the password, changes it before that check.
            using var submit = await PostTheFormAsync(browser, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword);

            // Past the check, neither the cookie the browser now holds nor the one from before signs anyone in. Until the
            // T265 review the change went through, the cookie was issued again with the new stamp, and the session outlived
            // the lock, sliding on for as long as it was used.
            clock.MoveForward(TimeSpan.FromMinutes(31));
            (await SignedInAsync(browser)).Should().BeFalse("a locked account's session ends at the next check, change or no change");
            (await SignedInAsync(app, cookieBefore)).Should().BeFalse();

            submit.StatusCode.Should().Be(HttpStatusCode.Redirect);
            submit.Headers.Location!.ToString().Should().Be(
                "/account/login?error=SessionEnded&returnUrl=" + Uri.EscapeDataString(PagePath),
                "a code, never the words (T285)");
            (await SignInPageRefusalAsync(browser, submit)).Should().Be("Your session has ended. Please sign in again.");
            IssuedSignInCookies(submit).Should().BeEmpty("no cookie is issued to a session that has ended");
            (await StampAsync(email)).Should().Be(stampAfterLock, "nothing changed");
            (await PasswordIsAsync(email, MsfRespondPageFlowTests.WebHost.SignInPassword)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task ASessionThatAChangeElsewhereHasEnded_IsNotRenewedByAChangeOfItsOwn()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (first, _) = NewBrowser(_host.Factory);
        var (second, _) = NewBrowser(_host.Factory);
        using (first)
        using (second)
        {
            await SignInAsync(first, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            await SignInAsync(second, email, MsfRespondPageFlowTests.WebHost.SignInPassword);

            // The first browser's change ends every other session: their cookies carry the old stamp.
            using (var changed = await PostTheFormAsync(first, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword))
            {
                changed.Headers.Location!.ToString().Should().Be($"{PagePath}?status=updated", "guard: the first change went through");
            }

            var stamp = await StampAsync(email);

            // Inside the validator's thirty minutes the second browser still loads the page. It knows the new password, as
            // anyone who watched it typed would; its change would hand its ended session the new stamp.
            using var submit = await PostTheFormAsync(second, NewPassword, "Another-Pa55word!", "Another-Pa55word!");

            submit.StatusCode.Should().Be(HttpStatusCode.Redirect);
            submit.Headers.Location!.AbsolutePathOf().Should().Be("/account/login");
            IssuedSignInCookies(submit).Should().BeEmpty();
            (await StampAsync(email)).Should().Be(stamp);
            (await PasswordIsAsync(email, NewPassword)).Should().BeTrue("the second browser's change was refused");
            (await SignedInAsync(second)).Should().BeFalse("its cookie was taken away");
        }
    }

    [Fact]
    public async Task FiveWrongCurrentPasswords_LockTheAccount_AndThenEvenTheRightOneIsRefused_WhileTheSessionGoesOn()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(_host.Factory);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var stamp = await StampAsync(email);

            var codes = new List<string>();
            for (var attempt = 1; attempt <= 5; attempt++)
            {
                using var wrong = await PostTheFormAsync(browser, $"Guess-{attempt}-Pa55word!", NewPassword, NewPassword);
                codes.Add(wrong.Headers.Location!.ToString());
            }

            // Identity's lockout, as the sign-in and link pages apply it: the fifth failure locks the account.
            codes.Should().Equal(
                $"{PagePath}?error=PasswordMismatch",
                $"{PagePath}?error=PasswordMismatch",
                $"{PagePath}?error=PasswordMismatch",
                $"{PagePath}?error=PasswordMismatch",
                $"{PagePath}?error={ChangePasswordOutcome.LockedOut}");

            using var right = await PostTheFormAsync(browser, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword);
            right.Headers.Location!.ToString().Should().Be($"{PagePath}?error={ChangePasswordOutcome.LockedOut}",
                "a locked account's password is not checked");
            (await StampAsync(email)).Should().Be(stamp);
            (await PasswordIsAsync(email, MsfRespondPageFlowTests.WebHost.SignInPassword)).Should().BeTrue();

            // A lockout the typing caused is not a deactivation: the session goes on, and the page says why.
            using var answer = await browser.GetAsync(right.Headers.Location);
            answer.StatusCode.Should().Be(HttpStatusCode.OK);
            Parse(await answer.Content.ReadAsStringAsync()).QuerySelector(".action-result .alert-danger")!.TextContent.Trim()
                .Should().Be(ChangePasswordOutcome.LockedOutMessage);
        }
    }

    [Fact]
    public async Task ThePasswordCheck_SharesTheSignInThrottle_AndItsRefusalComesBackToThePage()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(_host.Factory);
        using (browser)
        {
            // Neither costs anything: the throttle counts failed password checks only (T156).
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            using (var changed = await PostTheFormAsync(browser, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword))
            {
                changed.Headers.Location!.ToString().Should().Be(ChangePasswordOutcome.UpdatedUrl, "guard: a change that succeeds");
            }

            var codes = new List<string>();
            string? retryAfter = null;
            for (var attempt = 1; attempt <= 11; attempt++)
            {
                using var guess = await PostTheFormAsync(browser, $"Guess-{attempt}-Pa55word!", NewPassword, NewPassword);
                codes.Add(guess.Headers.Location!.ToString());
                retryAfter = guess.Headers.RetryAfter?.ToString();
            }

            // The fifth locks the account, and the ones after it fail on the lock: failures all the same.
            codes.Take(10).Should().NotContain(code => code.Contains(ChangePasswordOutcome.TooManyAttempts), "guard: ten are checked");
            codes[10].Should().Be($"{PagePath}?error={ChangePasswordOutcome.TooManyAttempts}",
                "a signed-in user is sent back to the page they were on, not to the sign-in page");
            int.Parse(retryAfter!).Should().BeInRange(1, 300, "the seconds left of the address's five minutes");

            using var answer = await browser.GetAsync(codes[10]);
            answer.StatusCode.Should().Be(HttpStatusCode.OK);
            Parse(await answer.Content.ReadAsStringAsync()).QuerySelector(".action-result .alert-danger")!.TextContent.Trim()
                .Should().Be(ChangePasswordOutcome.TooManyAttemptsMessage);

            // Shared with the sign-in page: the same address is refused there too, before any password is checked.
            var signInRefused = await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword, expectSuccess: false);
            signInRefused.Should().Be(SignInOutcome.Url(SignInOutcome.TooManyAttempts)).And.Be("/account/login?error=TooManyAttempts");
            using var signInPage = await browser.GetAsync(signInRefused);
            Parse(await signInPage.Content.ReadAsStringAsync()).QuerySelector(".alert-danger")!.TextContent.Trim()
                .Should().Be("Too many failed sign-in attempts from this network. Please wait a few minutes and try again.");
        }
    }

    [Fact]
    public async Task AnAccountThatSignsInThroughItsInstitution_HasNoPasswordToChangeHere()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(_host.Factory);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);

            // Moved to institutional sign-in while signed in. Its password no longer signs anyone in, and an SSO-provisioned
            // account has none, so checking one could only count failures towards a lockout that blocks its SSO sign-in.
            await using (var scope = _host.Factory.Services.CreateAsyncScope())
            {
                var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
                var user = await users.FindByEmailAsync(email);
                user!.AllowLocalPassword = false;
                (await users.UpdateAsync(user)).Succeeded.Should().BeTrue();
            }

            var stamp = await StampAsync(email);

            using var submit = await PostTheFormAsync(browser, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword);

            submit.Headers.Location!.ToString().Should().Be($"{PagePath}?error={ChangePasswordOutcome.InstitutionalSignIn}");
            (await StampAsync(email)).Should().Be(stamp);
            (await PasswordIsAsync(email, MsfRespondPageFlowTests.WebHost.SignInPassword)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task AFaultBeforeTheChange_IsSaidOnThePage_AndChangesNothing()
    {
        var fault = new Fault();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IPasswordHasher<WombatIdentityUser>>();
            services.AddScoped<IPasswordHasher<WombatIdentityUser>>(provider => new FaultingPasswordHasher(
                new PasswordHasher<WombatIdentityUser>(provider.GetService<IOptions<PasswordHasherOptions>>()), fault));
        }));

        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var stamp = await StampAsync(email);

            using var load = await browser.GetAsync(PagePath);
            fault.On = true;
            using var submit = await PostTheLoadedFormAsync(browser, load, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword);
            fault.On = false;

            submit.StatusCode.Should().Be(HttpStatusCode.Redirect, "not the error page");
            submit.Headers.Location!.ToString().Should().Be($"{PagePath}?error={ChangePasswordOutcome.Failed}");
            (await StampAsync(email)).Should().Be(stamp);

            using var answer = await browser.GetAsync(submit.Headers.Location);
            answer.StatusCode.Should().Be(HttpStatusCode.OK, "still signed in");
            Parse(await answer.Content.ReadAsStringAsync()).QuerySelector(".action-result .alert-danger")!.TextContent.Trim()
                .Should().Be(ChangePasswordOutcome.GeneralRefusal);
        }
    }

    [Fact]
    public async Task AFaultAfterTheChange_SaysThePasswordWasChanged_AndAsksForASignIn()
    {
        var fault = new Fault();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IUserClaimsPrincipalFactory<WombatIdentityUser>>();
            services.AddScoped<WombatUserClaimsPrincipalFactory>();
            services.AddScoped<IUserClaimsPrincipalFactory<WombatIdentityUser>>(provider =>
                new FaultingPrincipalFactory(provider.GetRequiredService<WombatUserClaimsPrincipalFactory>(), fault));
        }));

        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var stamp = await StampAsync(email);

            // The password changes; issuing the cookie again then fails, as it would with the database gone mid-request.
            using var load = await browser.GetAsync(PagePath);
            fault.On = true;
            using var submit = await PostTheLoadedFormAsync(browser, load, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword);
            fault.On = false;

            submit.StatusCode.Should().Be(HttpStatusCode.Redirect, "not the error page");
            submit.Headers.Location!.ToString().Should().Be("/account/login?error=PasswordChanged", "a code, never the words (T285)");
            (await SignInPageRefusalAsync(browser, submit))
                .Should().Be("Your password was changed. Please sign in with your new password.");
            IssuedSignInCookies(submit).Should().BeEmpty();
            (await StampAsync(email)).Should().NotBe(stamp, "the password was changed, and the answer must not say otherwise");
            (await PasswordIsAsync(email, NewPassword)).Should().BeTrue();
            (await SignedInAsync(browser)).Should().BeFalse("the cookie, whose stamp the change made stale, was taken away");
        }
    }

    /// <summary>Loads the change-password page as the signed-in browser, and posts its form as the browser does.</summary>
    private static async Task<HttpResponseMessage> PostTheFormAsync(
        HttpClient browser,
        string current,
        string replacement,
        string confirmation)
    {
        using var load = await browser.GetAsync(PagePath);
        return await PostTheLoadedFormAsync(browser, load, current, replacement, confirmation);
    }

    /// <summary>Posts the form of a change-password page already loaded, as the browser does.</summary>
    private static async Task<HttpResponseMessage> PostTheLoadedFormAsync(
        HttpClient browser,
        HttpResponseMessage load,
        string current,
        string replacement,
        string confirmation)
    {
        load.StatusCode.Should().Be(HttpStatusCode.OK, $"guard: the page loads for a signed-in user ({load.Headers.Location})");
        var form = Parse(await load.Content.ReadAsStringAsync()).QuerySelector($"form[action='{SubmitPath}']")!;
        form.GetAttribute("method").Should().Be("post");

        var fields = new List<KeyValuePair<string, string>>
        {
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!)
        };
        string[] values = [current, replacement, confirmation];
        var inputs = form.QuerySelectorAll("input[type=password]").ToList();
        inputs.Select(input => input.GetAttribute("name")).Should().Equal("CurrentPassword", "NewPassword", "ConfirmPassword");
        fields.AddRange(inputs.Select((input, index) => new KeyValuePair<string, string>(input.GetAttribute("name")!, values[index])));

        return await browser.PostAsync(form.GetAttribute("action"), new FormUrlEncodedContent(fields));
    }

    /// <summary>Signs a browser in through the sign-in page's own form, and returns where it was sent.</summary>
    private static async Task<string> SignInAsync(
        HttpClient browser,
        string email,
        string password,
        bool expectSuccess = true,
        string? returnUrl = null)
    {
        using var loginPage = await browser.GetAsync(
            returnUrl is null ? "/account/login" : $"/account/login?ReturnUrl={Uri.EscapeDataString(returnUrl)}");
        var form = Parse(await loginPage.Content.ReadAsStringAsync()).QuerySelector("form[action='/account/login/submit']")!;

        using var signIn = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", form.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
            new("Email", email),
            new("Password", password),
            new("ReturnUrl", form.QuerySelector("input[name=ReturnUrl]")?.GetAttribute("value") ?? string.Empty)
        ]));

        signIn.StatusCode.Should().Be(HttpStatusCode.Redirect);
        var location = signIn.Headers.Location!.ToString();
        if (expectSuccess)
        {
            location.Should().NotContain("/account/login", "guard: the sign-in succeeded");
        }

        return location;
    }

    /// <summary>
    /// Follows a redirect to the sign-in page and returns what its refusal says: the words the page chose for the code it
    /// was sent (T285).
    /// </summary>
    private static async Task<string?> SignInPageRefusalAsync(HttpClient browser, HttpResponseMessage redirect)
    {
        using var page = await browser.GetAsync(redirect.Headers.Location);
        page.StatusCode.Should().Be(HttpStatusCode.OK, "guard: the sign-in page loads");
        return Parse(await page.Content.ReadAsStringAsync()).QuerySelector(".alert-danger")?.TextContent.Trim();
    }

    /// <summary>Whether the browser's cookie, as it holds it now, loads the page signed in.</summary>
    private static async Task<bool> SignedInAsync(HttpClient browser)
    {
        using var load = await browser.GetAsync(PagePath);
        return Answered(load);
    }

    /// <summary>Whether <paramref name="cookie" />, sent alone, loads the page signed in.</summary>
    private static async Task<bool> SignedInAsync<TEntryPoint>(WebApplicationFactory<TEntryPoint> app, string cookie)
        where TEntryPoint : class
    {
        using var client = app.CreateDefaultClient(Origin);
        using var request = new HttpRequestMessage(HttpMethod.Get, PagePath);
        request.Headers.Add("Cookie", cookie);
        using var load = await client.SendAsync(request);
        return Answered(load);
    }

    private static bool Answered(HttpResponseMessage load)
    {
        if (load.StatusCode == HttpStatusCode.OK)
        {
            return true;
        }

        load.StatusCode.Should().Be(HttpStatusCode.Redirect);
        load.Headers.Location!.AbsolutePathOf().Should().Be("/account/login", "guard: refused by being sent to sign in");
        return false;
    }

    /// <summary>The sign-in cookies a response sets with a value; a sign-out sets them empty, to delete them.</summary>
    private static IReadOnlyList<string> IssuedSignInCookies(HttpResponseMessage response)
        => (response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : [])
            .Where(cookie => cookie.StartsWith(SignInCookie, StringComparison.Ordinal))
            .Where(cookie => cookie.IndexOf('=') is var equals && equals + 1 < cookie.Length && cookie[equals + 1] != ';')
            .ToList();

    private static string? QueryValue(Uri address, string name)
    {
        var query = (address.IsAbsoluteUri ? address : new Uri(Origin, address)).Query;
        return query.TrimStart('?').Split('&')
            .Select(pair => pair.Split('=', 2))
            .Where(pair => string.Equals(pair[0], name, StringComparison.OrdinalIgnoreCase))
            .Select(pair => Uri.UnescapeDataString(pair.Length > 1 ? pair[1] : string.Empty))
            .SingleOrDefault();
    }

    private async Task<bool> PasswordIsAsync(string email, string password)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var user = await users.FindByEmailAsync(email);
        return await users.CheckPasswordAsync(user!, password);
    }

    private async Task<string?> StampAsync(string email)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        var user = await users.FindByEmailAsync(email);
        return user!.SecurityStamp;
    }

    /// <summary>
    /// A browser of its own: its own cookies, which the test can read, and its own address, so the sign-in throttle it
    /// meets is its own. Redirects are not followed.
    /// </summary>
    private static (HttpClient Browser, CookieContainerHandler Cookies) NewBrowser<TEntryPoint>(WebApplicationFactory<TEntryPoint> factory)
        where TEntryPoint : class
    {
        var cookies = new CookieContainerHandler();
        var browser = factory.CreateDefaultClient(Origin, cookies);
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.235.{Interlocked.Increment(ref _nextSubnet)}.7");
        return (browser, cookies);
    }

    /// <summary>The sign-in cookie a browser holds, as its Cookie header would carry it (every chunk, if it is split).</summary>
    private static string SignInCookieOf(CookieContainerHandler cookies)
    {
        var header = string.Join("; ", cookies.Container.GetCookies(Origin).Cast<Cookie>()
            .Where(cookie => cookie.Name.StartsWith(SignInCookie, StringComparison.Ordinal))
            .Select(cookie => $"{cookie.Name}={cookie.Value}"));
        header.Should().NotBeEmpty("guard: the browser is signed in");
        return header;
    }

    private static string NewEmail() => $"change-password-{Guid.NewGuid():N}@example.test";

    private static IHtmlDocument Parse(string html) => new HtmlParser().ParseDocument(html);

    /// <summary>The system clock, moved forward by as much as a test says.</summary>
    private sealed class MovableClock : TimeProvider
    {
        private TimeSpan _ahead;

        public void MoveForward(TimeSpan by) => _ahead += by;

        public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + _ahead;
    }

    /// <summary>A switch a test turns on around the one request it means to fail.</summary>
    private sealed class Fault
    {
        private volatile bool _on;

        public bool On
        {
            get => _on;
            set => _on = value;
        }

        public void ThrowIfOn()
        {
            if (_on)
            {
                throw new InvalidOperationException("A fault the test put here: the database has gone.");
            }
        }
    }

    /// <summary>Identity's hasher, failing while the fault is on: a fault in the password check, before any change.</summary>
    private sealed class FaultingPasswordHasher(IPasswordHasher<WombatIdentityUser> inner, Fault fault)
        : IPasswordHasher<WombatIdentityUser>
    {
        public string HashPassword(WombatIdentityUser user, string password)
        {
            fault.ThrowIfOn();
            return inner.HashPassword(user, password);
        }

        public PasswordVerificationResult VerifyHashedPassword(WombatIdentityUser user, string hashedPassword, string providedPassword)
        {
            fault.ThrowIfOn();
            return inner.VerifyHashedPassword(user, hashedPassword, providedPassword);
        }
    }

    /// <summary>
    /// The app's principal factory, failing while the fault is on. In a change nothing calls it but issuing the cookie
    /// again, so it fails after the password has changed.
    /// </summary>
    private sealed class FaultingPrincipalFactory(IUserClaimsPrincipalFactory<WombatIdentityUser> inner, Fault fault)
        : IUserClaimsPrincipalFactory<WombatIdentityUser>
    {
        public Task<System.Security.Claims.ClaimsPrincipal> CreateAsync(WombatIdentityUser user)
        {
            fault.ThrowIfOn();
            return inner.CreateAsync(user);
        }
    }
}

internal static class ChangePasswordFlowUriExtensions
{
    /// <summary>The path of a redirect's address, relative or absolute.</summary>
    public static string AbsolutePathOf(this Uri address)
        => (address.IsAbsoluteUri ? address : new Uri(new Uri("http://localhost"), address)).AbsolutePath;
}

