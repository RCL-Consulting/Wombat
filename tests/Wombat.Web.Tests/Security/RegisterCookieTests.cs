using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Wombat.Application.Common.Options;
using Wombat.Application.Features.Invitations;
using Wombat.Web.Security;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// T373 (flow 11, S1, S2, F5; the nit on the cookie's Secure): the mail's link opens the start path, the register cookie
/// holds the invitation for 30 minutes from then and never a password, and sign-out comes back to Register only for that
/// one path.
/// </summary>
public sealed class RegisterCookieTests
{
    private static readonly DateTimeOffset Opened = new(2026, 10, 19, 7, 30, 0, TimeSpan.Zero);

    [Fact]
    public void TheMailsLink_OpensTheStartPath_WhichIsNotThePage()
    {
        var link = new Uri(InvitationLinks.RegistrationUrl(new WombatOptions { BaseUrl = "https://wombat.test/" }, "a+b/c"));

        link.AbsolutePath.Should().Be(RegisterOutcome.StartPath).And.Be("/account/register/start");
        link.Query.Should().Be("?token=a%2Bb%2Fc");
        RegisterOutcome.StartPath.Should().NotBe(RegisterOutcome.PagePath, "S2: the page and the GET never share a route");
        RegisterOutcome.StartPath.Should().StartWith(RegisterOutcome.PagePath + "/", "the cookie's path covers it");
        RegisterOutcome.SubmitPath.Should().StartWith(RegisterOutcome.PagePath + "/");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void TheCookie_IsHttpOnly_Lax_OnTheRegisterPaths_ThirtyMinutes_AndSecureOnlyOverHttps(bool https, bool secure)
    {
        var (cookie, _) = NewCookie();
        var context = new DefaultHttpContext();
        context.Request.Scheme = https ? "https" : "http";

        cookie.Open(context, "the-token");

        var header = context.Response.Headers.SetCookie.ToString();
        var attributes = header.Split(';', StringSplitOptions.TrimEntries).Skip(1).Select(part => part.ToLowerInvariant()).ToList();
        attributes.Should().Contain("httponly").And.Contain("samesite=lax").And.Contain("path=/account/register")
            .And.Contain("max-age=1800");
        attributes.Contains("secure").Should().Be(secure, "Secure follows the request's scheme, so the http replay registers");
        header.Should().NotContain("the-token", "the value is protected");
    }

    /// <summary>B1: the form's binding to its invitation: stable, sixteen hex characters, and not the token.</summary>
    [Fact]
    public void TheFingerprint_IsStable_Short_AndDiffersByToken()
    {
        var one = RegisterCookie.Fingerprint("the-token");

        one.Should().MatchRegex("^[0-9a-f]{16}$").And.Be(RegisterCookie.Fingerprint("the-token"));
        RegisterCookie.Fingerprint("another-token").Should().NotBe(one);
        RegisterCookie.Fingerprints("the-token", one).Should().BeTrue();
        RegisterCookie.Fingerprints("the-token", one.ToUpperInvariant()).Should().BeFalse();
        RegisterCookie.Fingerprints("the-token", null).Should().BeFalse("a post with no binding came from no form of ours");
        RegisterCookie.Fingerprints("the-token", "").Should().BeFalse();
    }

    /// <summary>B7: a token far past the 43 characters one is, is no token: the start path clears the cookie instead.</summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("  ", false)]
    [InlineData("abc", true)]
    public void ATokenIsHeld_OnlyWhenItIsOne(string? token, bool held)
        => RegisterCookie.CanHold(token).Should().Be(held);

    [Fact]
    public void ATokenLongerThanTheBound_IsNotHeld()
    {
        RegisterCookie.CanHold(new string('a', RegisterCookie.LongestToken)).Should().BeTrue();
        RegisterCookie.CanHold(new string('a', RegisterCookie.LongestToken + 1)).Should().BeFalse();
        RegisterCookie.LongestToken.Should().Be(256);
    }

    [Fact]
    public void WhatTheCookieHolds_IsReadBack_UntilItsThirtyMinutesHavePassed()
    {
        var (cookie, clock) = NewCookie();
        var written = new DefaultHttpContext();
        cookie.Open(written, "the-token");
        var value = RegisterCookieValues.ValueIn(written.Response.Headers.SetCookie.ToString());

        Read(cookie, value).Should().BeEquivalentTo(new RegisterState("the-token", [], null, null, false, Opened + RegisterCookie.Lifetime));

        clock.Now = Opened.AddMinutes(29);
        Read(cookie, value).Should().NotBeNull();
        clock.Now = Opened.AddMinutes(30);
        Read(cookie, value).Should().BeNull("G16: the invitation is held for 30 minutes from when its link was opened");
    }

    [Fact]
    public void ARefusalsRewrite_KeepsTheEndTheLinkWasOpenedWith()
    {
        var (cookie, clock) = NewCookie();
        var opened = new DefaultHttpContext();
        cookie.Open(opened, "the-token");
        var state = Read(cookie, RegisterCookieValues.ValueIn(opened.Response.Headers.SetCookie.ToString()))!;

        clock.Now = Opened.AddMinutes(20);
        var refused = new DefaultHttpContext();
        cookie.Write(refused, state with { Codes = [RegisterOutcome.ConfirmationMismatch], FirstName = "Mohammed" });

        refused.Response.Headers.SetCookie.ToString().ToLowerInvariant().Should().Contain("max-age=600",
            "the 30 minutes run from the link, not from the refusal (G14's words)");
        Read(cookie, RegisterCookieValues.ValueIn(refused.Response.Headers.SetCookie.ToString()))!.FirstName.Should().Be("Mohammed");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-protected")]
    [InlineData("CfDJ8forged")]
    public void AValueNotWrittenHere_ReadsAsNoCookie(string value)
    {
        var (cookie, _) = NewCookie();

        Read(cookie, value).Should().BeNull();
    }

    [Fact]
    public void ACookieFromAnotherPurpose_ReadsAsNoCookie()
    {
        var provider = new EphemeralDataProtectionProvider();
        var (cookie, _) = NewCookie(provider);
        var forged = provider.CreateProtector("Wombat.Web.Navigation.ActingRoleSwitchResults.v1").Protect("{\"Token\":\"x\"}");

        Read(cookie, forged).Should().BeNull();
    }

    [Theory]
    [InlineData("/account/register", "/account/register")]
    [InlineData(null, "/account/login?error=SignedOut")]
    [InlineData("", "/account/login?error=SignedOut")]
    [InlineData("/account/register/", "/account/login?error=SignedOut")]
    [InlineData("/ACCOUNT/REGISTER", "/account/login?error=SignedOut")]
    [InlineData("/account/register?x=1", "/account/login?error=SignedOut")]
    [InlineData("//evil.example/account/register", "/account/login?error=SignedOut")]
    [InlineData("https://evil.example/", "/account/login?error=SignedOut")]
    [InlineData("/progress", "/account/login?error=SignedOut")]
    public void SignOut_ComesBackToRegister_ForThatOnePathAlone(string? returnTo, string landing)
    {
        SignOutOutcome.LandingFor(returnTo).Should().Be(landing, "F5: returnTo is never an open redirect");
        SignOutOutcome.SignedOutUrl.Should().Be("/account/login?error=SignedOut");
    }

    private static RegisterState? Read(RegisterCookie cookie, string value)
    {
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = $"{RegisterCookie.CookieName}={value}";
        return cookie.Read(context);
    }

    private static (RegisterCookie Cookie, MovableClock Clock) NewCookie(IDataProtectionProvider? provider = null)
    {
        var clock = new MovableClock { Now = Opened };
        return (new RegisterCookie(provider ?? new EphemeralDataProtectionProvider(), clock), clock);
    }

    private sealed class MovableClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; }

        public override DateTimeOffset GetUtcNow() => Now;
    }
}
