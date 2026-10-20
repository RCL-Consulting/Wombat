extern alias WombatWeb;

using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Security.Claims;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Invitations;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Domain.Invitations;
using Wombat.Infrastructure.Identity;
using Wombat.Infrastructure.Persistence;
using Wombat.Integration.Tests.MultiSourceFeedback;
using WombatWeb::Wombat.Web.Security;

namespace Wombat.Integration.Tests.Hosting;

/// <summary>
/// T285 end to end, through the whole of <c>Wombat.Web</c>'s <c>Program.cs</c> on a PostgreSQL schema of its own: the
/// sign-in, link and register pages show fixed sentences for the codes their endpoints send, never text from the address,
/// and every real refusal still reads as it did before.
/// </summary>
/// <remarks>
/// <para>
/// Until T285 each endpoint redirected with the sentence in <c>?error=</c> and each page printed it as it arrived, so a
/// crafted link could put any words on Wombat's own sign-in page, and the register endpoint's catch put any exception's
/// message into the address. The expected sentences below are written out, not read from the classes that now hold them,
/// so a changed word fails here.
/// </para>
/// <para>
/// A browser is a client with its own cookies and its own address (<see cref="MsfRespondPageFlowTests.WebHost.NewBrowser" />),
/// so the sign-in throttle each one meets is its own.
/// </para>
/// </remarks>
public sealed class AccountRefusalFlowTests : IClassFixture<MsfRespondPageFlowTests.WebHost>
{
    private const string Crafted = "Your account is locked. Call 012 345 6789";
    private const string WrongPassword = "Not-the-Pa55word!";
    private const string NewUserPassword = "Registered-Pa55word!";
    private const string ProviderKey = "kgk";

    private static int _nextSubnet;

    private readonly MsfRespondPageFlowTests.WebHost _host;

    public AccountRefusalFlowTests(MsfRespondPageFlowTests.WebHost host)
    {
        _host = host;
    }

    // ─── Sign-in ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task TheSignInPage_ShowsOnlyTheGeneralSentence_ForWordsOfTheAddressesOwn()
    {
        using var browser = NewBrowser();

        var page = await LoadAsync(browser, "/account/login?error=Call%20012");

        Refusal(page).Should().Be("Sign-in could not be completed. Try again.");
        page.DocumentElement.OuterHtml.Should().NotContain("Call 012");

        var longer = await LoadAsync(browser, "/account/login?error=" + Uri.EscapeDataString(Crafted));
        Refusal(longer).Should().Be("Sign-in could not be completed. Try again.");
        longer.DocumentElement.OuterHtml.Should().NotContain("012 345 6789");
    }

    [Fact]
    public async Task ARefusedSignIn_TravelsAsACode_AndALockoutAsAWrongPassword()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        using var browser = NewBrowser();

        var blank = await SignInAsync(browser, email, string.Empty);
        blank.Should().Be("/account/login?error=FieldsMissing");
        Refusal(await LoadAsync(browser, blank)).Should().Be("Enter your email and your password.");

        var wrong = await SignInAsync(browser, email, WrongPassword);
        wrong.Should().Be("/account/login?error=Refused");
        Refusal(await LoadAsync(browser, wrong)).Should().Be("Invalid email or password.");

        var unknown = await SignInAsync(browser, NewEmail(), WrongPassword);
        unknown.Should().Be(wrong, "an address no account has is refused as a wrong password is (T156)");

        // Identity's lockout trips at the account's fifth wrong password (one above, four here), and the lock refuses even
        // the right one. Each is sent back as a wrong password is, byte for byte (T287, T339, flow 02): until T339 the
        // lockout had a code of its own, which said the address had an account.
        var answers = new List<string>();
        for (var guess = 2; guess <= 5; guess++)
        {
            answers.Add(await SignInAsync(browser, email, WrongPassword));
        }

        answers.Add(await SignInAsync(browser, email, MsfRespondPageFlowTests.WebHost.SignInPassword));

        (await LockedOutAsync(email)).Should().BeTrue("guard: the fifth wrong password locked the account");
        answers.Should().AllBe(unknown);
        Refusal(await LoadAsync(browser, answers[^1])).Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task TheInstitutionalSignInsOwnRefusals_TravelAsCodes_AndReadAsTheSpecHasThem()
    {
        using var browser = NewBrowser();

        using var challenge = await browser.GetAsync("/account/sso-challenge/no-such-provider");
        challenge.Headers.Location!.ToString().Should().Be("/account/login?error=SsoUnknownProvider");
        Refusal(await LoadAsync(browser, challenge.Headers.Location.ToString()))
            .Should().Be("That institution's sign-in is not set up in Wombat. Sign in with your email and password.");

        // No institutional sign-in in progress: nothing to read at the callback, and nothing to link at the link submit.
        using var callback = await browser.GetAsync("/account/sso-callback?returnUrl=%2Fprogress");
        callback.Headers.Location!.ToString().Should().Be("/account/login?error=ExternalLoginUnavailable&returnUrl=%2Fprogress");
        Refusal(await LoadAsync(browser, callback.Headers.Location.ToString()))
            .Should().Be("Your institution's sign-in did not complete. Sign in with your email and password.",
                "this host offers no institution's button, so the words point at the email and password");

        var signInPage = await LoadAsync(browser, "/account/login");
        using var link = await browser.PostAsync("/account/link-external/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", AntiforgeryToken(signInPage)),
            new("Password", WrongPassword)
        ]));
        link.Headers.Location!.ToString().Should().Be("/account/login?error=ExternalSessionExpired");
        Refusal(await LoadAsync(browser, link.Headers.Location.ToString()))
            .Should().Be("Your institution's sign-in took too long and has expired. Sign in with your email and password.");
    }

    [Fact]
    public async Task ARefusedInstitutionalSignIn_TravelsAsItsCode_AndReadsAsItDidBefore()
    {
        // The provider does not verify the email, so no account is found by it or created from it (T155).
        await using var app = WithProvider(new ProviderCallback(NewEmail(), EmailVerified: false), await DemoInstitutionIdAsync());
        using var browser = NewHttpsBrowser(app);

        using (await browser.GetAsync(ProviderCallback.Path))
        {
        }

        using var callback = await browser.GetAsync("/account/sso-callback?returnUrl=%2F");
        callback.Headers.Location!.ToString().Should().Be("/account/login?error=SsoEmailNotVerified&returnUrl=%2F");
        Refusal(await LoadAsync(browser, callback.Headers.Location.ToString())).Should().Be(
            "Your institution's sign-in did not confirm your email address, so Wombat cannot use it to find your account or " +
            "create one. Sign in with your password if you have one, or ask your administrator for an invitation.");
    }

    // ─── Link ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task TheLinkPage_ShowsOnlyFixedSentences_AndARefusedLinkReadsAsItDidBefore()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        await using var app = WithProvider(new ProviderCallback(email, EmailVerified: true), await InstitutionOfAsync(email));
        using var browser = NewHttpsBrowser(app);

        using (await browser.GetAsync(ProviderCallback.Path))
        {
        }

        using var callback = await browser.GetAsync("/account/sso-callback?returnUrl=%2F");
        callback.Headers.Location!.ToString().Should().Be("/account/link-external?returnUrl=%2F",
            "guard: the account is offered a link, with no refusal");

        var crafted = await LoadAsync(browser, "/account/link-external?returnUrl=%2F&error=" + Uri.EscapeDataString(Crafted));
        Refusal(crafted).Should().Be("The account could not be linked. Try again.");
        crafted.DocumentElement.OuterHtml.Should().NotContain("012 345 6789");

        var blank = await PostLinkAsync(browser, string.Empty);
        blank.Should().Be("/account/link-external?returnUrl=%2F&error=PasswordRequired");
        Refusal(await LoadAsync(browser, blank)).Should().Be("Enter your password.");

        var wrong = await PostLinkAsync(browser, WrongPassword);
        wrong.Should().Be("/account/link-external?returnUrl=%2F&error=SsoLinkRefused");
        Refusal(await LoadAsync(browser, wrong)).Should().Be("The account could not be linked. Check your password and try again.");
    }

    // ─── Register (T373, flow 11) ────────────────────────────────────────────

    /// <summary>
    /// S2, S1 and the cookie nit: the mail's link opens the start path, which keeps the token in the register cookie and
    /// answers 303 to the page with no query; the page reads the invitation from the cookie. Until T373 the link opened the
    /// page with the token in its address, which a script took out after the load (T315).
    /// </summary>
    [Fact]
    public async Task TheMailsLink_KeepsTheTokenInTheCookie_AndSendsTheBrowserOnToThePageWithNoQuery()
    {
        var (token, email) = await InviteAsync();
        using var browser = NewBrowser();

        using var start = await browser.GetAsync(StartLink(token));

        start.StatusCode.Should().Be(HttpStatusCode.SeeOther);
        start.Headers.Location!.ToString().Should().Be("/account/register");
        start.Headers.CacheControl!.NoStore.Should().BeTrue("S2: the answer that held the token is never kept (B11)");
        var cookie = start.Headers.GetValues("Set-Cookie").Should().ContainSingle(value => value.StartsWith("wombat_register=")).Which;
        var attributes = cookie.Split(';', StringSplitOptions.TrimEntries).Skip(1).Select(part => part.ToLowerInvariant()).ToList();
        attributes.Should().Contain("httponly").And.Contain("samesite=lax").And.Contain("path=/account/register");
        attributes.Should().NotContain("secure", "the request was http, as the replay's is, so the browser sends it back");
        cookie.Should().NotContain(token, "the value is protected");

        var page = await LoadAsync(browser, "/account/register");
        page.QuerySelector("h1")!.TextContent.Trim().Should().Be("Complete registration");
        page.QuerySelector(".alert-info")!.TextContent.Trim().Should().Be(
            $"This invitation is for {email}, as Coordinator at {await DemoInstitutionNameAsync()}.");
        page.QuerySelector("form[action='/account/register/submit']").Should().NotBeNull();
        page.QuerySelector("input[name=Token]").Should().BeNull("the token is the cookie's");
        page.DocumentElement.OuterHtml.Should().NotContain(token);
    }

    [Fact]
    public async Task ARefusedRegistration_ComesBackToThePageWithNoQuery_KeepingTheNames_ThenTheRightOneRegisters()
    {
        var (token, email) = await InviteAsync();
        using var browser = NewBrowser();
        await OpenLinkAsync(browser, token);

        var mismatch = await RegisterAsync(browser, NewUserPassword, "Something-else-1!");
        mismatch.Should().Be("/account/register", "no redirect holds a name, a code or a token (S1)");
        var afterMismatch = await LoadAsync(browser, mismatch);
        FieldMessage(afterMismatch, "register-confirm-password").Should().Be(
            "The password confirmation does not match. Type the password again in both fields.");
        afterMismatch.GetElementById("register-first-name")!.GetAttribute("value").Should().Be("Refilwe", "the names are kept");
        afterMismatch.GetElementById("register-last-name")!.GetAttribute("value").Should().Be("Dlamini");
        afterMismatch.GetElementById("register-password")!.HasAttribute("autofocus").Should().BeTrue();

        // Identity's rules, under Password, in the one order they are listed in (T339, flow 02, E11).
        var weak = await RegisterAsync(browser, "short", "short");
        weak.Should().Be("/account/register");
        FieldMessage(await LoadAsync(browser, weak), "register-password").Should().Be(
            "The new password needs: At least 12 characters. A digit (0 to 9). An upper-case letter. A symbol, such as ! or #.");

        var reloaded = await LoadAsync(browser, "/account/register");
        reloaded.QuerySelectorAll("[aria-invalid]").Should().BeEmpty("a refusal is shown once");
        reloaded.GetElementById("register-first-name")!.GetAttribute("value").Should().Be("Refilwe");

        using (var registered = await PostRegisterAsync(browser, AntiforgeryToken(reloaded), InvitationField(reloaded), NewUserPassword, NewUserPassword))
        {
            registered.Headers.Location!.ToString().Should().Be("/", "guard: the right details register the account and sign it in");
            registered.Headers.GetValues("Set-Cookie").Should().Contain(value => value.StartsWith("wombat_register=;"),
                "the cookie is deleted on success");
        }

        (await FindUserAsync(email)).Should().NotBeNull();

        // The same link again, in another browser: the card says it was used.
        using var again = NewBrowser();
        await OpenLinkAsync(again, token);
        var used = await LoadAsync(again, "/account/register");
        used.QuerySelector("h1")!.TextContent.Trim().Should().Be("Invitation already used");
        used.QuerySelector("form[action='/account/register/submit']").Should().BeNull();
    }

    [Fact]
    public async Task ARevokedLink_ReadsAsItsCard_NoCookieAsAnIncompleteLink_AndASubmitWithoutOneAsTimedOut()
    {
        var (token, _) = await InviteAsync(revoked: true);
        using var browser = NewBrowser();

        var incomplete = await LoadAsync(browser, "/account/register");
        incomplete.QuerySelector("h1")!.TextContent.Trim().Should().Be("Invitation link incomplete");

        var formPage = await LoadAsync(browser, "/account/login");
        using (var lapsed = await PostRegisterAsync(browser, AntiforgeryToken(formPage), invitation: null, NewUserPassword, NewUserPassword))
        {
            lapsed.Headers.Location!.ToString().Should().Be("/account/register");
        }

        (await LoadAsync(browser, "/account/register")).QuerySelector("h1")!.TextContent.Trim().Should().Be("Invitation timed out");

        await OpenLinkAsync(browser, token);
        var revoked = await LoadAsync(browser, "/account/register");
        revoked.QuerySelector("h1")!.TextContent.Trim().Should().Be("Invitation revoked");
        revoked.QuerySelector(".account-form-container > p:not(.account-purpose)")!.TextContent.Trim()
            .Should().Be("This invitation has been revoked.");
    }

    /// <summary>
    /// T285 review, kept: an invitation to an address an account holds offers no form, and a form posted anyway is refused
    /// as the page was, with nothing registered.
    /// </summary>
    [Fact]
    public async Task AnInvitationToAnAddressAnAccountHolds_IsItsCard_BeforeOrAfterASubmit()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        var (token, _) = await InviteAsync(email: email);
        using var browser = NewBrowser();
        await OpenLinkAsync(browser, token);

        var page = await LoadAsync(browser, "/account/register");
        page.QuerySelector("h1")!.TextContent.Trim().Should().Be("Account already exists");
        page.QuerySelector("form[action='/account/register/submit']").Should().BeNull("no submit could succeed");

        // Posted with the binding the cookie's invitation would have had (B1), so it is the invitation's own refusal.
        var formPage = await LoadAsync(browser, "/account/login");
        using (var refused = await PostRegisterAsync(
                   browser, AntiforgeryToken(formPage), RegisterCookie.Fingerprint(token), NewUserPassword, NewUserPassword))
        {
            refused.Headers.Location!.ToString().Should().Be("/account/register");
        }

        (await LoadAsync(browser, "/account/register")).QuerySelector("h1")!.TextContent.Trim().Should().Be("Account already exists");
        (await InvitationUsedOnAsync(token)).Should().BeNull("nothing was registered");
    }

    /// <summary>
    /// T373 fix pass, B1: a form registers only the invitation it shows. The link for X is opened and its form loaded, then
    /// the link for Y is opened in the same browser (another tab), which the one cookie now holds. X's form, posted, accepts
    /// nothing, and the page says another invitation was opened, naming Y's address; Y's form then registers Y.
    /// </summary>
    [Fact]
    public async Task AFormPostedAfterAnotherInvitationsLinkWasOpened_AcceptsNothing_AndSaysSo()
    {
        var (first, firstEmail) = await InviteAsync();
        var (second, secondEmail) = await InviteAsync();
        using var browser = NewBrowser();
        await OpenLinkAsync(browser, first);
        var firstForm = await LoadAsync(browser, "/account/register");
        InvitationField(firstForm).Should().Be(RegisterCookie.Fingerprint(first), "guard: the form carries its invitation's binding");
        firstForm.DocumentElement.OuterHtml.Should().NotContain(first, "the binding is not the token");

        await OpenLinkAsync(browser, second);
        using (var refused = await PostRegisterAsync(
                   browser, AntiforgeryToken(firstForm), InvitationField(firstForm), NewUserPassword, NewUserPassword))
        {
            refused.Headers.Location!.ToString().Should().Be("/account/register");
        }

        (await FindUserAsync(firstEmail)).Should().BeNull("nothing was accepted");
        (await FindUserAsync(secondEmail)).Should().BeNull("the details typed for the first address never reach the second");
        (await InvitationUsedOnAsync(first)).Should().BeNull();
        (await InvitationUsedOnAsync(second)).Should().BeNull();

        var card = await LoadAsync(browser, "/account/register");
        card.QuerySelector("h1")!.TextContent.Trim().Should().Be("Another invitation opened");
        card.QuerySelectorAll(".account-form-container > p:not(.account-purpose)").Select(p => p.TextContent.Trim()).Should().Equal(
            "Another invitation's link was opened in this browser after this form was shown, so nothing was registered.",
            $"The invitation open in this browser now is for {secondEmail}. Continue to register it, or open the link in your own invitation email again.");

        var form = await LoadAsync(browser, card.QuerySelector(".form-actions a")!.GetAttribute("href")!);
        InvitationField(form).Should().Be(RegisterCookie.Fingerprint(second), "Continue shows the form of the invitation the cookie holds");
        using (var registered = await PostRegisterAsync(browser, AntiforgeryToken(form), InvitationField(form), NewUserPassword, NewUserPassword))
        {
            registered.Headers.Location!.ToString().Should().Be("/");
        }

        (await FindUserAsync(secondEmail)).Should().NotBeNull();
    }

    /// <summary>T373 fix pass, B10: a blind post after the 30 minutes leaves the timed-out cookie as it was, never extended.</summary>
    [Fact]
    public async Task ASecondPostAfterTheTimeOut_DoesNotRewriteTheTimedOutCookie()
    {
        using var browser = NewBrowser();
        var formPage = await LoadAsync(browser, "/account/login");
        using (var first = await PostRegisterAsync(browser, AntiforgeryToken(formPage), invitation: null, NewUserPassword, NewUserPassword))
        {
            first.Headers.GetValues("Set-Cookie").Should().Contain(value => value.StartsWith("wombat_register="),
                "guard: the first post leaves the timed-out state");
        }

        using (var second = await PostRegisterAsync(browser, AntiforgeryToken(formPage), invitation: null, NewUserPassword, NewUserPassword))
        {
            second.Headers.Location!.ToString().Should().Be("/account/register");
            (second.Headers.TryGetValues("Set-Cookie", out var cookies) ? cookies : [])
                .Should().NotContain(value => value.StartsWith("wombat_register="), "the window runs from the first, not the last");
        }

        (await LoadAsync(browser, "/account/register")).QuerySelector("h1")!.TextContent.Trim().Should().Be("Invitation timed out");
    }

    [Fact]
    public async Task AnInvitationToAnAddressIdentityWillNotTake_IsItsCard()
    {
        var address = $"o'brien-{Guid.NewGuid():N}@example.test";
        var (token, _) = await InviteAsync(email: address);
        using var browser = NewBrowser();
        await OpenLinkAsync(browser, token);

        var page = await LoadAsync(browser, "/account/register");
        page.QuerySelector("h1")!.TextContent.Trim().Should().Be("Address not accepted");
        page.QuerySelector(".account-form-container > p:not(.account-purpose)")!.TextContent.Trim().Should().Be(
            "This email address cannot be used for an account. Ask your administrator for an invitation to another address.");
        (await FindUserAsync(address)).Should().BeNull();
    }

    [Fact]
    public async Task AnExpiredLink_SaysTheDayItWorkedUntil()
    {
        var (token, _) = await InviteAsync(expiresOn: new DateOnly(2026, 1, 15));
        using var browser = NewBrowser();
        await OpenLinkAsync(browser, token);

        var page = await LoadAsync(browser, "/account/register");
        page.QuerySelector("h1")!.TextContent.Trim().Should().Be("Invitation expired");
        page.QuerySelector(".account-form-container > p:not(.account-purpose)")!.TextContent.Trim().Should().Be(
            "This invitation has expired: its link worked until 2026-01-15.");
    }

    /// <summary>
    /// A name of spaces passes the browser's <c>required</c>, and a 101-character name the field (no <c>maxlength</c>, S21):
    /// the command's own validator refuses each through the real pipeline, and the page says it under that field.
    /// </summary>
    [Theory]
    [InlineData("   ", "Dlamini", "register-first-name", "Enter your first name.")]
    [InlineData("Refilwe", null, "register-last-name", "Last name: enter 100 characters or fewer.")]
    public async Task ANameTheValidatorRefuses_IsSaidUnderItsField(string firstName, string? lastName, string field, string expected)
    {
        var (token, email) = await InviteAsync();
        using var browser = NewBrowser();
        await OpenLinkAsync(browser, token);

        var refused = await RegisterAsync(browser, NewUserPassword, NewUserPassword, firstName, lastName ?? new string('d', 101));

        refused.Should().Be("/account/register");
        var page = await LoadAsync(browser, refused);
        FieldMessage(page, field).Should().Be(expected);
        page.GetElementById(field)!.HasAttribute("autofocus").Should().BeTrue();
        (await FindUserAsync(email)).Should().BeNull();
    }

    [Fact]
    public async Task AFaultInRegistration_IsLogged_SaysNothingWasSaved_AndTryAgainReopensTheForm()
    {
        var (token, _) = await InviteAsync();
        var logs = new CapturingLoggerProvider();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IInvitedUserProvisioner>();
            services.AddScoped<IInvitedUserProvisioner, FaultingProvisioner>();
            services.AddSingleton<ILoggerProvider>(logs);
        }));
        using var browser = BrowserOf(app);
        await OpenLinkAsync(browser, token);

        var failed = await RegisterAsync(browser, NewUserPassword, NewUserPassword);

        failed.Should().Be("/account/register", "an exception's message is never put in an address");
        var page = await LoadAsync(browser, failed);
        page.QuerySelector("h1")!.TextContent.Trim().Should().Be("Registration not completed");
        page.DocumentElement.OuterHtml.Should().NotContain("012 345 6789");
        logs.Entries.Should().Contain(entry => entry.Category == RegisterOutcome.LogCategory
                                               && entry.Level == LogLevel.Error
                                               && entry.Exception != null
                                               && entry.Exception.Message == FaultingProvisioner.Message,
            "the fault is logged, where an operator can read it");

        var tryAgain = page.QuerySelector(".form-actions a")!.GetAttribute("href")!;
        (await LoadAsync(browser, tryAgain)).QuerySelector("form[action='/account/register/submit']").Should().NotBeNull(
            "the invitation is still in the cookie");
    }

    /// <summary>
    /// F10: a fault after the account is created, before the invitation is marked used, leaves no account and an unused
    /// invitation: one transaction. Until T373 the provisioner's own save had committed the account by then.
    /// </summary>
    [Fact]
    public async Task AFaultBetweenTheAccountAndTheAccept_LeavesNoAccount_AndTheInvitationUnused()
    {
        var (token, email) = await InviteAsync();
        await using var app = _host.Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IInvitedUserProvisioner>();
            services.AddScoped<InvitedUserProvisioner>();
            services.AddScoped<IInvitedUserProvisioner>(provider =>
                new FaultAfterProvisioning(provider.GetRequiredService<InvitedUserProvisioner>()));
        }));
        using var browser = BrowserOf(app);
        await OpenLinkAsync(browser, token);

        var failed = await RegisterAsync(browser, NewUserPassword, NewUserPassword);

        failed.Should().Be("/account/register");
        (await LoadAsync(browser, failed)).QuerySelector("h1")!.TextContent.Trim().Should().Be("Registration not completed");
        (await FindUserAsync(email)).Should().BeNull("the account's rows were rolled back with the accept");
        (await InvitationUsedOnAsync(token)).Should().BeNull(
            "the invitation is still open: the failed command's audit row, saved after the rollback, commits nothing of it");
    }

    /// <summary>
    /// M4 and G18: a form loaded while signed out, posted after someone signed in on the same browser (a stale tab), is
    /// refused back to the page, which says who is signed in. Nothing is accepted, no account is created, and the session
    /// is not replaced. Then F5: Sign out and continue lands back on Register, which still holds the invitation.
    /// </summary>
    [Fact]
    public async Task ASubmitWhileSignedIn_IsRefusedToTheSignedInCard_ThenSignOutAndContinueReturnsToTheForm()
    {
        var (token, email) = await InviteAsync();
        var someoneElse = NewEmail();
        await _host.CreateAssessorAsync(someoneElse);
        using var browser = NewBrowser();
        await OpenLinkAsync(browser, token);
        var staleForm = await LoadAsync(browser, "/account/register");

        await _host.SignInAsync(browser, someoneElse);
        using (var refused = await PostRegisterAsync(browser, AntiforgeryToken(staleForm), InvitationField(staleForm), NewUserPassword, NewUserPassword))
        {
            refused.StatusCode.Should().Be(HttpStatusCode.Redirect, "not a bare 400: the antiforgery check comes after this one");
            refused.Headers.Location!.ToString().Should().Be("/account/register");
            refused.Headers.Contains("Set-Cookie").Should().BeFalse("no session is replaced, and the invitation is kept");
        }

        (await FindUserAsync(email)).Should().BeNull("nothing was accepted");
        (await InvitationUsedOnAsync(token)).Should().BeNull();
        (await SignedInAsync(browser)).Should().BeTrue("the person signed in is still signed in");

        var card = await LoadAsync(browser, "/account/register");
        card.QuerySelector("h1")!.TextContent.Trim().Should().Be("Signed in as someone else");
        var said = card.QuerySelector(".account-form-container > p:not(.account-purpose)")!.TextContent.Trim();
        said.Should().StartWith("You are signed in as ").And.EndWith($". This invitation is for {email}.");

        var signOut = card.QuerySelector($"form[action='{SignOutOutcome.SubmitPath}']")!;
        using (var signedOut = await browser.PostAsync(SignOutOutcome.SubmitPath, new FormUrlEncodedContent(
               [
                   new("__RequestVerificationToken", signOut.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!),
                   new("returnTo", signOut.QuerySelector("input[name=returnTo]")!.GetAttribute("value")!)
               ])))
        {
            signedOut.Headers.Location!.ToString().Should().Be("/account/register", "F5");
        }

        (await SignedInAsync(browser)).Should().BeFalse();
        (await LoadAsync(browser, "/account/register")).QuerySelector("form[action='/account/register/submit']")
            .Should().NotBeNull("the person lands on Register with the form the cookie's token reads");
    }

    [Theory]
    [InlineData("/progress")]
    [InlineData("//evil.example/")]
    [InlineData("https://evil.example/account/register")]
    [InlineData("/ACCOUNT/REGISTER")]
    [InlineData("/account/register/")]
    [InlineData("/account/register?token=x")]
    public async Task SignOut_IgnoresAnyOtherReturnTo(string returnTo)
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, email);
        var page = await LoadAsync(browser, "/account/logout");

        using var signedOut = await browser.PostAsync(SignOutOutcome.SubmitPath, new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", AntiforgeryToken(page)),
            new("returnTo", returnTo)
        ]));

        signedOut.Headers.Location!.ToString().Should().Be("/account/login?error=SignedOut");
    }

    /// <summary>B11: two returnTo values, even both Register's, are read joined by a comma, and land where signing out does.</summary>
    [Fact]
    public async Task SignOut_IgnoresTwoReturnTos()
    {
        var email = NewEmail();
        await _host.CreateAssessorAsync(email);
        using var browser = NewBrowser();
        await _host.SignInAsync(browser, email);
        var page = await LoadAsync(browser, "/account/logout");

        using var signedOut = await browser.PostAsync(SignOutOutcome.SubmitPath, new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", AntiforgeryToken(page)),
            new("returnTo", RegisterOutcome.PagePath),
            new("returnTo", RegisterOutcome.PagePath)
        ]));

        signedOut.Headers.Location!.ToString().Should().Be("/account/login?error=SignedOut");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private HttpClient NewBrowser() => _host.NewBrowser(NextAddress());

    private static string NextAddress() => $"10.238.{Interlocked.Increment(ref _nextSubnet)}.7";

    private static string NewEmail() => $"refusal-{Guid.NewGuid():N}@example.test";

    private static async Task<IHtmlDocument> LoadAsync(HttpClient browser, string address)
    {
        using var response = await browser.GetAsync(address);
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"guard: {address} loads");
        return new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
    }

    /// <summary>What the page's refusal says; null when it shows none.</summary>
    private static string? Refusal(IHtmlDocument page) => page.QuerySelector(".alert-danger")?.TextContent.Trim();

    private static string AntiforgeryToken(IHtmlDocument page)
        => page.QuerySelector("input[name=__RequestVerificationToken]")!.GetAttribute("value")!;

    /// <summary>Loads the sign-in page and posts its form, as the browser does; returns where the answer sends it.</summary>
    private static async Task<string> SignInAsync(HttpClient browser, string email, string password)
    {
        var page = await LoadAsync(browser, "/account/login");
        using var answer = await browser.PostAsync("/account/login/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", AntiforgeryToken(page)),
            new("Email", email),
            new("Password", password)
        ]));

        answer.StatusCode.Should().Be(HttpStatusCode.Redirect, "guard: the form was accepted and answered");
        return answer.Headers.Location!.ToString();
    }

    /// <summary>Loads the link page, which holds the institutional sign-in in progress, and posts its form.</summary>
    private static async Task<string> PostLinkAsync(HttpClient browser, string password)
    {
        var page = await LoadAsync(browser, "/account/link-external?returnUrl=%2F");
        var form = page.QuerySelector("form[action='/account/link-external/submit']");
        form.Should().NotBeNull("guard: the institutional sign-in is still held, so the page offers the link");

        using var answer = await browser.PostAsync("/account/link-external/submit", new FormUrlEncodedContent(
        [
            new("__RequestVerificationToken", AntiforgeryToken(page)),
            new("ReturnUrl", form!.QuerySelector("input[name=ReturnUrl]")!.GetAttribute("value")!),
            new("Password", password)
        ]));

        answer.StatusCode.Should().Be(HttpStatusCode.Redirect);
        return answer.Headers.Location!.ToString();
    }

    /// <summary>The mail's link for <paramref name="token" />, as <c>InvitationLinks</c> writes it, without the host.</summary>
    private static string StartLink(string token) => InvitationLinks.RegistrationUrl(new WombatOptions(), token);

    /// <summary>Opens the mail's link, as the browser does: the cookie is set, and the answer is the page.</summary>
    private static async Task OpenLinkAsync(HttpClient browser, string token)
    {
        using var start = await browser.GetAsync(StartLink(token));
        start.StatusCode.Should().Be(HttpStatusCode.SeeOther, "guard: the start path answers 303");
        start.Headers.Location!.ToString().Should().Be("/account/register");
    }

    /// <summary>Loads the register page, which the cookie names the invitation of, and posts its form; returns where it sends.</summary>
    private static async Task<string> RegisterAsync(
        HttpClient browser,
        string password,
        string confirmation,
        string firstName = "Refilwe",
        string lastName = "Dlamini")
    {
        var page = await LoadAsync(browser, "/account/register");
        page.QuerySelector("form[action='/account/register/submit']").Should().NotBeNull(
            "guard: the invitation can be used, so the page offers the form");

        using var answer = await PostRegisterAsync(browser, AntiforgeryToken(page), InvitationField(page), password, confirmation, firstName, lastName);
        var location = answer.Headers.Location!.ToString();
        location.Should().NotContain("?", "S1: no redirect carries a query");
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            location.Should().NotContain(firstName, "S1: no redirect holds a name");
        }
        return location;
    }

    private static async Task<HttpResponseMessage> PostRegisterAsync(
        HttpClient browser,
        string antiforgeryToken,
        string? invitation,
        string password,
        string confirmation,
        string firstName = "Refilwe",
        string lastName = "Dlamini")
    {
        List<KeyValuePair<string, string>> fields = [new("__RequestVerificationToken", antiforgeryToken)];
        if (invitation is not null)
        {
            fields.Add(new("Invitation", invitation));
        }

        var answer = await browser.PostAsync("/account/register/submit", new FormUrlEncodedContent(
        [
            .. fields,
            new("FirstName", firstName),
            new("LastName", lastName),
            new("Password", password),
            new("ConfirmPassword", confirmation)
        ]));
        answer.StatusCode.Should().Be(HttpStatusCode.Redirect, "guard: the form was accepted and answered");
        return answer;
    }

    /// <summary>The form's binding to the invitation it shows (B1); null when the page offers no form.</summary>
    private static string? InvitationField(IHtmlDocument page)
        => page.QuerySelector("form[action='/account/register/submit'] input[type=hidden][name=Invitation]")?.GetAttribute("value");

    /// <summary>What the field <paramref name="fieldId" /> says under it, when it is refused and names its message.</summary>
    private static string? FieldMessage(IHtmlDocument page, string fieldId)
    {
        var field = page.GetElementById(fieldId)!;
        field.GetAttribute("aria-invalid").Should().Be("true", $"guard: {fieldId} is refused");
        var messageId = field.GetAttribute("aria-describedby")!.Split(' ')[0];
        return page.GetElementById(messageId)?.TextContent.Trim();
    }

    private static HttpClient BrowserOf(WebApplicationFactory<WombatWeb::Program> app)
    {
        var browser = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", NextAddress());
        return browser;
    }

    /// <summary>Whether the browser is signed in: My account answers, rather than sending it to sign in.</summary>
    private static async Task<bool> SignedInAsync(HttpClient browser)
    {
        using var response = await browser.GetAsync("/account/profile");
        return response.StatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    /// A Coordinator invitation to the demo institution, for <paramref name="email" /> or a new address, stored as issuing one
    /// stores it; returns its token.
    /// </summary>
    private async Task<(string Token, string Email)> InviteAsync(bool revoked = false, string? email = null, DateOnly? expiresOn = null)
    {
        var institutionId = await DemoInstitutionIdAsync();
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var tokens = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();

        var token = tokens.GenerateToken();
        email ??= NewEmail();
        dbContext.Invitations.Add(new Invitation
        {
            Email = email,
            TokenHash = tokens.HashToken(token),
            TargetRole = WombatRoles.Coordinator,
            InstitutionId = institutionId,
            IssuedByUserId = "coordinator-web-1",
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = expiresOn ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            RevokedOn = revoked ? DateTime.UtcNow : null
        });
        await dbContext.SaveChangesAsync();
        return (token, email);
    }

    /// <summary>When the invitation <paramref name="token" /> names was accepted; null while it has not been.</summary>
    private async Task<DateTime?> InvitationUsedOnAsync(string token)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var tokens = scope.ServiceProvider.GetRequiredService<IInvitationTokenService>();
        var invitations = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Invitations
            .AsNoTracking()
            .ToListAsync();
        return invitations.Single(invitation => tokens.VerifyToken(token, invitation.TokenHash)).UsedOn;
    }

    private async Task<WombatIdentityUser?> FindUserAsync(string email)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>().FindByEmailAsync(email);
    }

    private async Task<bool> LockedOutAsync(string email)
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<WombatIdentityUser>>();
        return await users.IsLockedOutAsync((await users.FindByEmailAsync(email))!);
    }

    private async Task<int> InstitutionOfAsync(string email) => (await FindUserAsync(email))!.InstitutionId!.Value;

    private async Task<string> DemoInstitutionNameAsync()
    {
        var id = await DemoInstitutionIdAsync();
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Institutions
            .Where(entity => entity.Id == id)
            .Select(entity => entity.Name)
            .SingleAsync();
    }

    private async Task<int> DemoInstitutionIdAsync()
    {
        await using var scope = _host.Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Institutions
            .Where(entity => entity.ShortCode == "DEMO")
            .Select(entity => entity.Id)
            .SingleAsync();
    }

    /// <summary>The web host with one institutional sign-in configured, and <paramref name="callback" /> ahead of it.</summary>
    private WebApplicationFactory<WombatWeb::Program> WithProvider(ProviderCallback callback, int institutionId)
        => _host.Factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Sso:Providers:0:Key", ProviderKey);
            builder.UseSetting("Sso:Providers:0:DisplayName", "KGK");
            builder.UseSetting("Sso:Providers:0:InstitutionId", institutionId.ToString(CultureInfo.InvariantCulture));
            builder.UseSetting("Sso:Providers:0:Authority", "https://idp.kgk.test");
            builder.UseSetting("Sso:Providers:0:ClientId", "wombat");
            builder.UseSetting("Sso:Providers:0:ClientSecret", "not-a-secret");
            builder.ConfigureTestServices(services => services.AddSingleton<IStartupFilter>(callback));
        });

    /// <summary>
    /// A browser over HTTPS, as behind Caddy: the external cookie is a <c>__Host-</c> cookie, which is Secure always.
    /// </summary>
    private static HttpClient NewHttpsBrowser(WebApplicationFactory<WombatWeb::Program> app)
    {
        var browser = app.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
        browser.DefaultRequestHeaders.Add("X-Forwarded-For", NextAddress());
        return browser;
    }

    /// <summary>
    /// What the provider's handler does at its callback, without a provider: puts an institutional sign-in that asserts
    /// <paramref name="Email" />, verified or not, into the external cookie. Ahead of the app's own pipeline.
    /// </summary>
    private sealed record ProviderCallback(string Email, bool EmailVerified) : IStartupFilter
    {
        public const string Path = "/test/provider-callback";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Map(Path, branch => branch.Run(context =>
            {
                List<Claim> claims =
                [
                    new(ClaimTypes.NameIdentifier, $"idp-subject-{Guid.NewGuid():N}"),
                    new(ClaimTypes.Email, Email)
                ];
                if (EmailVerified)
                {
                    claims.Add(new Claim("email_verified", "true"));
                }

                var properties = new AuthenticationProperties();
                properties.Items["LoginProvider"] = ProviderKey;
                return context.SignInAsync(
                    IdentityConstants.ExternalScheme,
                    new ClaimsPrincipal(new ClaimsIdentity(claims, ProviderKey)),
                    properties);
            }));
            next(app);
        };
    }

    /// <summary>A provisioner that fails as a fault does: with a message no one should ever read in an address.</summary>
    private sealed class FaultingProvisioner : IInvitedUserProvisioner
    {
        public const string Message = "Npgsql: connection to 10.0.0.5 refused. Call 012 345 6789";

        /// <summary>The address is free, so the page offers the form, and the fault comes at the submit.</summary>
        public Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
            => Task.FromResult(InvitedAddressStatus.Available);

        public Task<ProvisionedInvitationUser> ProvisionAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string targetRole,
            int? institutionId,
            int? collegeId,
            int? specialityId,
            int? subSpecialityId,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException(Message);

        public Task<T> InOneTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default)
            => work(cancellationToken);
    }

    /// <summary>
    /// The real provisioner, which creates the account, then a fault before the accept marks the invitation used (F10):
    /// the transaction is the real one's.
    /// </summary>
    private sealed class FaultAfterProvisioning(InvitedUserProvisioner inner) : IInvitedUserProvisioner
    {
        public Task<InvitedAddressStatus> GetAddressStatusAsync(string email, CancellationToken cancellationToken = default)
            => inner.GetAddressStatusAsync(email, cancellationToken);

        public async Task<ProvisionedInvitationUser> ProvisionAsync(
            string email,
            string password,
            string firstName,
            string lastName,
            string targetRole,
            int? institutionId,
            int? collegeId,
            int? specialityId,
            int? subSpecialityId,
            CancellationToken cancellationToken = default)
        {
            await inner.ProvisionAsync(
                email, password, firstName, lastName, targetRole, institutionId, collegeId, specialityId, subSpecialityId,
                cancellationToken);
            throw new InvalidOperationException("The database went away between the account and the accept.");
        }

        public Task<T> InOneTransactionAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken cancellationToken = default)
            => inner.InOneTransactionAsync(work, cancellationToken);
    }

    private sealed record LogEntry(string Category, LogLevel Level, Exception? Exception);

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<LogEntry> _entries = new();

        public IReadOnlyList<LogEntry> Entries => _entries.ToList();

        public ILogger CreateLogger(string categoryName) => new Logger(categoryName, _entries);

        public void Dispose()
        {
        }

        private sealed class Logger(string category, ConcurrentQueue<LogEntry> entries) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
                => entries.Enqueue(new LogEntry(category, logLevel, exception));
        }
    }
}
