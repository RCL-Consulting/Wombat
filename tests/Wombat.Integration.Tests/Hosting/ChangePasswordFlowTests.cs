extern alias WombatWeb;

using System.Net;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.Audit;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using WombatWeb::Wombat.Web.Navigation;
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
        // The security stamp checked on every request, not once a minute (SessionRevalidation, T279), so a stale
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
            submit.Headers.Location!.ToString().Should().Be("/account/profile?status=password-updated",
                "a change goes to My account, which says so (T339, flow 02)");
            submit.Headers.GetValues("Set-Cookie").Should().Contain(
                cookie => cookie.StartsWith($"{SignInCookie}=", StringComparison.Ordinal),
                "the sign-in cookie is issued again in the response to the change");
            (await StampAsync(email)).Should().NotBe(stampBefore, "changing the password changes the security stamp");
            SignInCookieOf(cookies).Should().NotBe(cookieBefore);

            // Still signed in: the page the redirect names loads. What My account says for the status is its own (T339,
            // flow 02: "Password updated."), held by its page and flow tests.
            using var answer = await browser.GetAsync(submit.Headers.Location);
            answer.StatusCode.Should().Be(HttpStatusCode.OK, $"not sent to sign in again ({answer.Headers.Location})");
            var page = Parse(await answer.Content.ReadAsStringAsync());
            // The account row names them by the display-name claim sign-in issued (T335, flow 01): "Signed Assessor".
            page.QuerySelector(".account-row .account-link")!.TextContent.Trim().Should().Be("Signed Assessor", "signed in, as themselves");

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
    [InlineData("Not-the-Pa55word!", NewPassword, NewPassword, "PasswordMismatch",
        "Your password was not changed. Incorrect password.")]
    [InlineData(MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, "Changed-Pa55word?", "ConfirmationMismatch",
        "Your password was not changed. The password confirmation does not match.")]
    [InlineData(MsfRespondPageFlowTests.WebHost.SignInPassword, "short1A!", "short1A!", "PasswordTooShort",
        "Your password was not changed. The new password needs: At least 12 characters.")]
    [InlineData(" ", " ", " ", "FieldsMissing",
        "Your password was not changed. Enter your current password, a new password, and the new password again to confirm it.")]
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
            RefusalWords(Parse(await answer.Content.ReadAsStringAsync()))
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
        // The app's own interval: the stamp is checked once a minute (SessionRevalidation, T279), on a clock the test
        // moves.
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
            clock.MoveForward(SessionRevalidation.Interval + TimeSpan.FromSeconds(5));
            (await SignedInAsync(browser)).Should().BeFalse("a locked account's session ends at the next check, change or no change");
            (await SignedInAsync(app, cookieBefore)).Should().BeFalse();

            submit.StatusCode.Should().Be(HttpStatusCode.Redirect);
            submit.Headers.Location!.ToString().Should().Be(
                "/account/login?error=SessionEnded&returnUrl=" + Uri.EscapeDataString(PagePath),
                "a code, never the words (T285)");
            (await SignInPageMessageAsync(browser, submit)).Should().Be("Your session has ended. Sign in again.");
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
                changed.Headers.Location!.ToString().Should().Be(ChangePasswordOutcome.UpdatedUrl, "guard: the first change went through");
            }

            var stamp = await StampAsync(email);

            // Inside the validator's minute the second browser still loads the page. It knows the new password, as
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

    /// <summary>
    /// T339, flow 02, E2: the fifth wrong current password locks the account, as the sign-in page's does, and ends the
    /// session that guessed, every copy of its cookie included, so whoever holds it cannot wait the lock out and guess five
    /// more. The sign-in page says why, with the lockout Identity is configured with. Until T339 the lockout came back to
    /// this page as a refusal and the session went on.
    /// </summary>
    [Fact]
    public async Task FiveWrongCurrentPasswords_LockTheAccount_AndSignTheSessionOut_SayingWhy()
    {
        // The security stamp checked on every request, so a cookie the lock ended is refused at once.
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero)));

        var email = NewEmail();
        var account = await _host.CreateAssessorAsync(email);
        var (browser, cookies) = NewBrowser(app);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var cookieBefore = SignInCookieOf(cookies);

            var codes = new List<string>();
            HttpResponseMessage? fifth = null;
            for (var attempt = 1; attempt <= 5; attempt++)
            {
                using var load = await browser.GetAsync(PagePath);
                if (attempt == 5)
                {
                    // A switch's word not yet shown, left after the page loaded (a load takes it): the sign-out takes it too
                    // (T317; the review of the t339 branch).
                    cookies.Container.Add(Origin, new Cookie(ActingRoleSwitchResults.CookieName, "a-word-not-yet-shown"));
                }

                var wrong = await PostTheLoadedFormAsync(browser, load, $"Guess-{attempt}-Pa55word!", NewPassword, NewPassword);
                codes.Add(wrong.Headers.Location!.ToString());
                if (attempt == 5)
                {
                    fifth = wrong;
                }
                else
                {
                    wrong.Dispose();
                }
            }

            using (fifth)
            {
                // Identity's lockout, as the sign-in and link pages apply it: the fifth failure locks the account.
                codes.Should().Equal(
                    $"{PagePath}?error=PasswordMismatch",
                    $"{PagePath}?error=PasswordMismatch",
                    $"{PagePath}?error=PasswordMismatch",
                    $"{PagePath}?error=PasswordMismatch",
                    "/account/login?error=LockedSignedOut");
                IssuedSignInCookies(fifth!).Should().BeEmpty("the session is signed out, not issued a cookie");
                SetCookies(fifth!).Should().Contain(cookie => cookie.StartsWith(ActingRoleSwitchResults.CookieName + "=;", StringComparison.Ordinal),
                    "the acting role's word goes with the sign-out");

                (await SignInPageMessageAsync(browser, fifth!)).Should().Be(
                    "Your current password was entered incorrectly too many times, so your account is locked for 15 minutes " +
                    "and you have been signed out. Wait 15 minutes, then sign in again.");
            }

            (await SignedInAsync(browser)).Should().BeFalse("the browser's session is signed out");
            (await SignedInAsync(app, cookieBefore)).Should().BeFalse(
                "a copy of the cookie is ended too: the stamp it carries is not the account's any more");
            (await PasswordIsAsync(email, MsfRespondPageFlowTests.WebHost.SignInPassword)).Should().BeTrue("nothing was changed");

            // Signing in during the lock is refused as a wrong password is (T287).
            (await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword, expectSuccess: false))
                .Should().Be("/account/login?error=Refused");

            // Recorded, as the sign-in page's lockout is: stamped with the account's institution, never the password (the
            // review of the t339 branch).
            var row = (await AuditRowsAsync(account.Id, "ChangePasswordLockedOut")).Should().ContainSingle().Subject;
            row.Success.Should().BeFalse();
            row.Category.Should().Be(AuditCategory.Authentication);
            row.InstitutionId.Should().Be(account.InstitutionId);
            (row.SummaryJson + row.ErrorMessage).Should().NotContain("Guess-5-Pa55word!", "never the password");
        }
    }

    /// <summary>
    /// The review of the t339 branch: an account locked already, by a stranger's five wrong passwords at the sign-in page,
    /// is refused here with the wait, and nothing else changes. Identity answers a locked account's check with a lockout
    /// without checking the password, so until the review the owner's own change, with the right current password, was
    /// taken for the fifth guess: every session of the account ended, and the sign-in page told the owner they had typed
    /// their password wrong too many times.
    /// </summary>
    [Fact]
    public async Task AnAccountLockedAlready_IsRefusedWithTheWait_AndTheSessionGoesOn()
    {
        var email = NewEmail();
        var account = await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(_host.Factory);
        var (stranger, _) = NewBrowser(_host.Factory);
        using (browser)
        using (stranger)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            for (var guess = 1; guess <= 5; guess++)
            {
                (await SignInAsync(stranger, email, $"Stranger-{guess}-Pa55word!", expectSuccess: false))
                    .Should().Be("/account/login?error=Refused", "guard: a stranger's guess");
            }

            var stampBefore = await StampAsync(email);
            using var post = await PostTheFormAsync(browser, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword);

            post.Headers.Location!.ToString().Should().Be($"{PagePath}?error=AccountLocked&minutes=15");
            SetCookies(post).Should().NotContain(cookie => cookie.StartsWith(SignInCookie, StringComparison.Ordinal),
                "the session is neither signed out nor issued a cookie");

            using var answer = await browser.GetAsync(post.Headers.Location);
            answer.StatusCode.Should().Be(HttpStatusCode.OK, "still signed in");
            RefusalWords(Parse(await answer.Content.ReadAsStringAsync()))
                .Should().Be("Your account is locked. Wait 15 minutes, then try again.");

            (await StampAsync(email)).Should().Be(stampBefore, "no session of the account was ended");
            (await PasswordIsAsync(email, MsfRespondPageFlowTests.WebHost.SignInPassword)).Should().BeTrue("nothing was changed");
            (await AuditRowsAsync(account.Id, "ChangePasswordLockedOut")).Should().BeEmpty("this post tripped no lock");
        }
    }

    /// <summary>
    /// The review of the t339 branch: a refused change of stamp at the lockout is tried once more, and the browser is signed
    /// out whatever came of it. Until then the result was not read. A user validator refuses every write to the account
    /// once the lock is saved, as a change made elsewhere would (ConcurrencyFailure).
    /// </summary>
    [Fact]
    public async Task ALockoutWhoseStampCannotBeChanged_IsTriedTwice_AndStillSignsTheBrowserOut()
    {
        var writesWhileLocked = new WriteCounter();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            services.AddScoped<IUserValidator<WombatIdentityUser>>(_ => new RefusesWritesAfterTheLock(writesWhileLocked))));

        var email = NewEmail();
        var account = await _host.CreateAssessorAsync(email);
        var (browser, _) = NewBrowser(app);
        using (browser)
        {
            await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword);
            var stampBefore = await StampAsync(email);

            HttpResponseMessage? fifth = null;
            for (var attempt = 1; attempt <= 5; attempt++)
            {
                var wrong = await PostTheFormAsync(browser, $"Guess-{attempt}-Pa55word!", NewPassword, NewPassword);
                if (attempt == 5)
                {
                    fifth = wrong;
                }
                else
                {
                    wrong.Dispose();
                }
            }

            using (fifth)
            {
                fifth!.Headers.Location!.ToString().Should().Be("/account/login?error=LockedSignedOut");
                SetCookies(fifth).Should().Contain(cookie => cookie.StartsWith(SignInCookie + "=;", StringComparison.Ordinal),
                    "this browser is signed out whatever came of the stamp");
            }

            writesWhileLocked.Count.Should().Be(3, "the lock's own write, then the change of stamp and its one retry");
            (await StampAsync(email)).Should().Be(stampBefore, "both changes were refused, and neither was saved by a later write");
            (await AuditRowsAsync(account.Id, "ChangePasswordLockedOut")).Should().ContainSingle("the lockout is still recorded");
            (await SignedInAsync(browser)).Should().BeFalse();
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

            // Six failed sign-ins from this address, to addresses no account has, then four wrong current passwords here:
            // ten failed checks, one short of the account's lockout, which since T339 would end the session (E2). The
            // sign-ins are another tab's, signed out, from the same address: since T339 the sign-in page sends a signed-in
            // visitor Home (E4), so this browser has no sign-in form to post.
            var (signedOut, _) = NewBrowser(_host.Factory);
            using var signedOutTab = signedOut;
            signedOut.DefaultRequestHeaders.Remove("X-Forwarded-For");
            signedOut.DefaultRequestHeaders.Add("X-Forwarded-For", browser.DefaultRequestHeaders.GetValues("X-Forwarded-For").Single());
            for (var attempt = 1; attempt <= 6; attempt++)
            {
                (await SignInAsync(signedOut, NewEmail(), NewPassword, expectSuccess: false))
                    .Should().Be("/account/login?error=Refused", "guard: a failed check the throttle counts");
            }

            var codes = new List<string>();
            string? retryAfter = null;
            for (var attempt = 1; attempt <= 5; attempt++)
            {
                using var guess = await PostTheFormAsync(browser, $"Guess-{attempt}-Pa55word!", NewPassword, NewPassword);
                codes.Add(guess.Headers.Location!.ToString());
                retryAfter = guess.Headers.RetryAfter?.ToString();
            }

            codes.Take(4).Should().OnlyContain(code => code == $"{PagePath}?error=PasswordMismatch", "guard: four are checked");
            codes[4].Should().Be($"{PagePath}?error={ChangePasswordOutcome.TooManyAttempts}",
                "a signed-in user is sent back to the page they were on, not to the sign-in page");
            int.Parse(retryAfter!).Should().BeInRange(1, 300, "the seconds left of the address's five minutes");

            using var answer = await browser.GetAsync(codes[4]);
            answer.StatusCode.Should().Be(HttpStatusCode.OK);
            RefusalWords(Parse(await answer.Content.ReadAsStringAsync()))
                .Should().Be(ChangePasswordOutcome.TooManyAttemptsMessage);

            // Shared with the sign-in page: the same address is refused there too, before any password is checked.
            var signInRefused = await SignInAsync(signedOut, email, MsfRespondPageFlowTests.WebHost.SignInPassword, expectSuccess: false);
            signInRefused.Should().Be(SignInOutcome.Url(SignInOutcome.TooManyAttempts)).And.Be("/account/login?error=TooManyAttempts");
            using var signInPage = await signedOut.GetAsync(signInRefused);
            Parse(await signInPage.Content.ReadAsStringAsync()).QuerySelector(".alert-danger")!.TextContent.Trim()
                .Should().Be("Too many failed sign-in attempts from this network. Wait a few minutes and try again.");
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
            using var loadedBefore = await browser.GetAsync(PagePath);

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

            // Since T339 (flow 02, C9) the page itself offers such an account no form, on its first visit: the notice and
            // the way back. The endpoint still refuses a post, from a page loaded before the move, or a crafted one.
            using var page = await browser.GetAsync(PagePath);
            page.StatusCode.Should().Be(HttpStatusCode.OK);
            var shown = Parse(await page.Content.ReadAsStringAsync());
            shown.QuerySelector($"form[action='{SubmitPath}']").Should().BeNull("there is no password to change here");
            shown.QuerySelector("main .alert-info")!.TextContent.Trim().Should().Be(ChangePasswordOutcome.InstitutionalSignInMessage);

            using var submit = await PostTheLoadedFormAsync(
                browser, loadedBefore, MsfRespondPageFlowTests.WebHost.SignInPassword, NewPassword, NewPassword);

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
            RefusalWords(Parse(await answer.Content.ReadAsStringAsync()))
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
            (await SignInPageMessageAsync(browser, submit))
                .Should().Be("Your password was changed. Sign in with your new password.");
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
    /// Follows a redirect to the sign-in page and returns what its one message says, refusal or notice: the words the page
    /// chose for the code it was sent (T285). Which kind of alert it is, is the page's (T339, flow 02).
    /// </summary>
    private static async Task<string?> SignInPageMessageAsync(HttpClient browser, HttpResponseMessage redirect)
    {
        using var page = await browser.GetAsync(redirect.Headers.Location);
        page.StatusCode.Should().Be(HttpStatusCode.OK, "guard: the sign-in page loads");
        return Parse(await page.Content.ReadAsStringAsync()).QuerySelectorAll(".alert").Should().ContainSingle()
            .Which.TextContent.Trim();
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

    private static IReadOnlyList<string> SetCookies(HttpResponseMessage response)
        => response.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies.ToList() : [];

    private async Task<List<AuditEntry>> AuditRowsAsync(string userId, string action)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Set<AuditEntry>()
            .AsNoTracking()
            .Where(entry => entry.ActorUserId == userId && entry.Action == action)
            .ToListAsync();
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

    /// <summary>
    /// The page's refusal as it reads (T339, flow 02): it stands in the form's card now, not in an action-result region,
    /// and a rule broken is a list item under the heading, read as its own sentence.
    /// </summary>
    private static string RefusalWords(IHtmlDocument page)
    {
        var refusal = page.QuerySelector("#change-password-error.alert-danger")!;
        var list = refusal.QuerySelector("ul");
        var before = list is null
            ? refusal.TextContent
            : refusal.TextContent[..refusal.TextContent.IndexOf(list.TextContent, StringComparison.Ordinal)];
        var rules = refusal.QuerySelectorAll("li").Select(item => item.TextContent.Trim());
        return System.Text.RegularExpressions.Regex.Replace(string.Join(" ", [before.Trim(), .. rules]), @"\s+", " ").Trim();
    }

    /// <summary>The system clock, moved forward by as much as a test says.</summary>
    private sealed class MovableClock : TimeProvider
    {
        private TimeSpan _ahead;

        public void MoveForward(TimeSpan by) => _ahead += by;

        public override DateTimeOffset GetUtcNow() => base.GetUtcNow() + _ahead;
    }

    private sealed class WriteCounter
    {
        private int _count;

        public int Count => Volatile.Read(ref _count);

        public int Add() => Interlocked.Increment(ref _count);
    }

    /// <summary>
    /// Lets the write that saves an account's lock through, and refuses every later write while it is locked: a change of
    /// stamp that cannot be made.
    /// </summary>
    private sealed class RefusesWritesAfterTheLock(WriteCounter writes) : IUserValidator<WombatIdentityUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<WombatIdentityUser> manager, WombatIdentityUser user)
            => Task.FromResult(user.LockoutEnd is { } end && end > DateTimeOffset.UtcNow && writes.Add() > 1
                ? IdentityResult.Failed(new IdentityErrorDescriber().ConcurrencyFailure())
                : IdentityResult.Success);
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

