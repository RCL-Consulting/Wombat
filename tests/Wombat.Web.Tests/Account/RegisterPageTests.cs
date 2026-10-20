using System.Collections.Concurrent;
using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Domain.Identity;
using Wombat.Application.Features.Invitations;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Security;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Account;

/// <summary>
/// Complete registration (T373, flow 11; R3-G01–G18): the form and each card, read from the register cookie, never the
/// address. A refusal sits under its field, which is marked, described by it and focused (S15); the names are kept and
/// the passwords never are; each card is its own h1 and words.
/// </summary>
/// <remarks>
/// Rewritten by T373. Until then the page read <c>?token=</c> and <c>?error=</c> from its address, put every refusal in
/// one alert above the form that every field named, dropped the names, and took the token out of the address bar with a
/// script after load (T315). The expected sentences are written out, so a changed word fails here. The integration
/// suite's <c>Hosting/AccountRefusalFlowTests</c> runs the start path, the submit and sign-out end to end.
/// </remarks>
public sealed class RegisterPageTests : TestContext
{
    private const string Token = "invitation-token";

    private static readonly DateTimeOffset Now = new(2026, 10, 19, 7, 30, 0, TimeSpan.Zero);

    private readonly PreviewSender _sender = new();
    private readonly CapturingLogger _logger = new();
    private readonly RegisterCookie _cookie = new(new EphemeralDataProtectionProvider(), new FixedClock(Now));

    public RegisterPageTests()
    {
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<ILogger<Register>>(_logger);
        Services.AddSingleton(_cookie);

        // The rules AddInfrastructure sets.
        Services.AddSingleton(Options.Create(new IdentityOptions
        {
            Password = new PasswordOptions
            {
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
                RequireNonAlphanumeric = true,
                RequiredLength = 12,
                RequiredUniqueChars = 4
            }
        }));
    }

    // ─── The form (G01–G03, G17) ─────────────────────────────────────────────

    [Theory]
    [InlineData("smit@kgk.wombat.local", WombatRoles.Coordinator, "Kgosi Kgari Teaching Hospital", null, null, null,
        "This invitation is for smit@kgk.wombat.local, as Coordinator at Kgosi Kgari Teaching Hospital.")]
    [InlineData("kruger@cmsa.wombat.local", WombatRoles.CollegeAdmin, null, "College of Paediatricians of South Africa", null, null,
        "This invitation is for kruger@cmsa.wombat.local, as College admin at College of Paediatricians of South Africa.")]
    [InlineData("mbatha@kgk.wombat.local", WombatRoles.InstitutionalAdmin, "Kgosi Kgari Teaching Hospital", null, null, null,
        "This invitation is for mbatha@kgk.wombat.local, as Institutional admin at Kgosi Kgari Teaching Hospital.")]
    [InlineData("patel@kgk.wombat.local", WombatRoles.Assessor, "Kgosi Kgari Teaching Hospital", null, "Paediatrics", "Paediatrics",
        "This invitation is for patel@kgk.wombat.local, as Assessor in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital.")]
    public void TheForm_NamesTheAddressAndTheRoleAndScopeInWords_AndTheLinksLastDay(
        string email, string role, string? institution, string? college, string? speciality, string? subSpeciality, string expected)
    {
        _sender.Preview = new InvitationPreviewDto(email, role, institution, college, speciality, subSpeciality, new DateOnly(2026, 11, 2));

        var (cut, _) = Render(Opened());

        var notice = cut.Find(".alert.alert-info");
        notice.TextContent.Trim().Should().Be(expected, "Steps 1.8, 1.10, 2.8 (R3-0-steps): the role's label, never its key");
        notice.HasAttribute("role").Should().BeFalse("standing page content, present on arrival (P8)");
        notice.QuerySelector(".account-email")!.InnerHtml.Should().Contain("@<wbr>", "the address breaks after its @ (M7)");
        cut.Find("p.account-note").TextContent.Trim().Should().Be(
            "Not expecting this invitation? Ignore it: its link works until 2026-11-02.");
        cut.FindAll("h1").Should().ContainSingle().Which.TextContent.Trim().Should().Be("Complete registration");
    }

    [Fact]
    public void TheForm_IsTheLockup_ReadOnlyEmail_TheNames_ThePasswords_AndRegisterInFormActions_WithNoMarksAndNoMaxlength()
    {
        var (cut, _) = Render(Opened());

        var card = cut.Find(".account-form-container");
        card.QuerySelector(".account-brand .account-brand-text")!.TextContent.Should().Be("Wombat");
        card.QuerySelector(".account-purpose")!.TextContent.Trim().Should().Be("Work-based assessment for specialist training.");

        var form = cut.Find("form");
        form.GetAttribute("action").Should().Be(RegisterOutcome.SubmitPath).And.Be("/account/register/submit");
        form.GetAttribute("method").Should().Be("post");
        form.HasAttribute("novalidate").Should().BeFalse("Q6: the browser's own check names a blank field");
        cut.FindAll("input[name=Token]").Should().BeEmpty("the token is the cookie's, never the form's (S1)");
        var binding = form.QuerySelector("input[type=hidden][name=Invitation]")!;
        binding.GetAttribute("value").Should().Be(RegisterCookie.Fingerprint(Token),
            "B1: the form names the invitation it shows, so a cookie another link replaced registers nothing");
        cut.Markup.Should().NotContain(Token, "the binding is a fingerprint, not the token");

        var email = cut.Find($"#{Register.EmailId}");
        email.HasAttribute("readonly").Should().BeTrue();
        email.HasAttribute("disabled").Should().BeFalse("a read-only field is read and focusable; a disabled one is skipped");
        email.HasAttribute("name").Should().BeFalse("the address is the invitation's, not the form's");
        email.GetAttribute("value").Should().Be("invitee@hospital.test");
        cut.Find($"#{email.GetAttribute("aria-describedby")}").TextContent.Trim().Should().Be(
            "The address this invitation is for. It cannot be changed here.");

        cut.Find($"#{Register.FirstNameId}").GetAttribute("autocomplete").Should().Be("given-name");
        cut.Find($"#{Register.LastNameId}").GetAttribute("autocomplete").Should().Be("family-name");
        cut.FindAll("input[maxlength]").Should().BeEmpty("S21: a long name is typed in full, then refused in words");
        cut.FindAll(".required-mark").Should().BeEmpty("Q6: every field is required, so none is marked");
        cut.FindAll("label").Should().OnlyContain(label => !label.TextContent.Contains('*'));
        cut.FindAll("[style]").Should().BeEmpty("S20: no inline style");
        cut.FindAll(".mb-3").Should().BeEmpty();

        cut.Find(".form-actions button[type=submit]").TextContent.Trim().Should().Be("Register");
        cut.FindAll("[aria-invalid]").Should().BeEmpty();
        cut.FindAll("[autofocus]").Should().BeEmpty("a first visit reads from the top, where the invitation is named");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    /// <summary>T339 (flow 02, E11), kept: the six rules under the password, which names them; each password its toggle.</summary>
    [Fact]
    public void ThePasswordFields_HaveTheirToggles_AndThePasswordNamesTheSixRules()
    {
        var (cut, _) = Render(Opened());

        cut.Find($"#{Register.PasswordId}").GetAttribute("aria-describedby").Should().Be(Register.RulesId);
        var rules = cut.Find($"#{Register.RulesId}");
        rules.QuerySelector("p")!.TextContent.Trim().Should().Be("The new password needs:");
        rules.QuerySelectorAll("li").Should().HaveCount(6);
        PasswordToggleMarkup.ShouldBeTheToggle(cut, Register.PasswordId, "Show password", PasswordToggleMarkup.Driven.ByScript);
        PasswordToggleMarkup.ShouldBeTheToggle(cut, Register.ConfirmId, "Show confirm password", PasswordToggleMarkup.Driven.ByScript);
    }

    // ─── A message per field (G04–G06, S15) ──────────────────────────────────

    [Fact]
    public void ABrokenPasswordRule_IsPasswordsMessage_DescribingIt_WithTheRules_TheNamesKept_AndTheFocusThere()
    {
        var (cut, context) = Render(Refused(["PasswordTooShort"], "Mohammed", "Patel"));

        var password = cut.Find($"#{Register.PasswordId}");
        password.GetAttribute("aria-invalid").Should().Be("true");
        password.GetAttribute("aria-describedby").Should().Be($"{Register.PasswordMessageId} {Register.RulesId}");
        password.HasAttribute("autofocus").Should().BeTrue();
        password.HasAttribute("value").Should().BeFalse("a password is never kept");
        cut.Find($"#{Register.PasswordMessageId}").TextContent.Trim().Should().Be("The new password needs: At least 12 characters.");
        cut.Find($"#{Register.PasswordMessageId}").ClassList.Should().Contain("validation-message");

        cut.Find($"#{Register.FirstNameId}").GetAttribute("value").Should().Be("Mohammed", "T317's registration half: the names kept");
        cut.Find($"#{Register.LastNameId}").GetAttribute("value").Should().Be("Patel");
        cut.FindAll(".alert-danger").Should().BeEmpty("never one alert for every field (S15)");
        OnlyFocused(cut).Should().Be(Register.PasswordId);
        ConsumedCodes(context).Should().BeTrue("the refusal is shown once; a reload shows the form with the names");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void SeveralBrokenRules_ReadInTheRulesOwnOrder_WhateverOrderTheyWereSent()
    {
        var (cut, _) = Render(Refused(["PasswordRequiresDigit", "PasswordTooShort", "PasswordRequiresUpper"]));

        cut.Find($"#{Register.PasswordMessageId}").TextContent.Trim().Should().Be(
            "The new password needs: At least 12 characters. A digit (0 to 9). An upper-case letter.");
    }

    [Fact]
    public void ARefusedConfirmation_IsConfirmPasswordsMessage_BothPasswordsEmpty_AndTheFocusOnPassword()
    {
        var (cut, _) = Render(Refused([RegisterOutcome.ConfirmationMismatch], "Mohammed", "Patel"));

        var confirm = cut.Find($"#{Register.ConfirmId}");
        confirm.GetAttribute("aria-invalid").Should().Be("true");
        confirm.GetAttribute("aria-describedby").Should().Be(Register.ConfirmMessageId);
        cut.Find($"#{Register.ConfirmMessageId}").TextContent.Trim().Should().Be(
            "The password confirmation does not match. Type the password again in both fields.");

        var password = cut.Find($"#{Register.PasswordId}");
        password.HasAttribute("aria-invalid").Should().BeFalse("G05 marks the confirmation, not the password");
        password.GetAttribute("aria-describedby").Should().Be($"{Register.ConfirmMessageId} {Register.RulesId}",
            "Password is described by the confirmation's message too (G05)");
        password.HasAttribute("value").Should().BeFalse();
        confirm.HasAttribute("value").Should().BeFalse();
        OnlyFocused(cut).Should().Be(Register.PasswordId, "typing starts again in Password");
        cut.Find($"#{Register.FirstNameId}").GetAttribute("value").Should().Be("Mohammed");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Theory]
    [InlineData(RegisterOutcome.LastNameTooLong, Register.LastNameId, Register.LastNameMessageId, "Last name: enter 100 characters or fewer.")]
    [InlineData(RegisterOutcome.FirstNameTooLong, Register.FirstNameId, Register.FirstNameMessageId, "First name: enter 100 characters or fewer.")]
    [InlineData(RegisterOutcome.FirstNameMissing, Register.FirstNameId, Register.FirstNameMessageId, "Enter your first name.")]
    [InlineData(RegisterOutcome.LastNameMissing, Register.LastNameId, Register.LastNameMessageId, "Enter your last name.")]
    public void ANameTheValidatorRefused_IsThatFieldsMessage_AndTheFocusIsThere(
        string code, string fieldId, string messageId, string expected)
    {
        var longName = new string('a', 101);
        var (cut, _) = Render(Refused([code], "Mohammed", longName));

        var field = cut.Find($"#{fieldId}");
        field.GetAttribute("aria-invalid").Should().Be("true");
        field.GetAttribute("aria-describedby").Should().Be(messageId);
        cut.Find($"#{messageId}").TextContent.Trim().Should().Be(expected);
        OnlyFocused(cut).Should().Be(fieldId);
        cut.Find($"#{Register.LastNameId}").GetAttribute("value").Should().Be(longName, "G06: the name as typed, to shorten");
        cut.FindAll("[aria-invalid]").Should().ContainSingle();
    }

    [Fact]
    public void BothNamesRefused_TakeTheFocusToTheFirst()
    {
        var (cut, _) = Render(Refused([RegisterOutcome.FirstNameTooLong, RegisterOutcome.LastNameTooLong]));

        cut.FindAll("[aria-invalid]").Should().HaveCount(2);
        OnlyFocused(cut).Should().Be(Register.FirstNameId);
    }

    // ─── The cards (G07–G16, G18) ────────────────────────────────────────────

    [Theory]
    [InlineData(InvitationRefusal.Used, "Invitation already used",
        "This invitation has already been used: an account was registered from it.|If it was yours, sign in with that account.")]
    [InlineData(InvitationRefusal.Revoked, "Invitation revoked",
        "This invitation has been revoked.|Ask the person who invited you for a new invitation.")]
    [InlineData(InvitationRefusal.Invalid, "Invitation link not recognised",
        "This invitation is invalid.|The link may be incomplete, or a newer invitation email may have replaced it. Open the link in " +
        "the most recent invitation email, or ask the person who invited you for a new invitation.")]
    [InlineData(InvitationRefusal.AccountExists, "Account already exists",
        "A user with this email address already exists.|Sign in with that account.")]
    [InlineData(InvitationRefusal.AddressNotAccepted, "Address not accepted",
        "This email address cannot be used for an account. Ask your administrator for an invitation to another address.")]
    public void AnInvitationThatCannotBeUsed_IsItsCard_WithItsWords_AndNoForm(InvitationRefusal reason, string heading, string sentences)
    {
        _sender.Failure = new InvitationRefusedException(reason);

        var (cut, _) = Render(Opened());

        CardShouldRead(cut, heading, sentences.Split('|'));
        _logger.Errors.Should().BeEmpty("a refusal is not a fault");
    }

    [Theory]
    [InlineData(InvitationRefusal.Used)]
    [InlineData(InvitationRefusal.AccountExists)]
    public void TheUsedAndExistingAccountCards_OfferSignIn_AndAForgottenPassword(InvitationRefusal reason)
    {
        _sender.Failure = new InvitationRefusedException(reason);

        var (cut, _) = Render(Opened());

        var signIn = cut.Find(".form-actions a.btn.btn-primary");
        signIn.TextContent.Trim().Should().Be("Sign in");
        signIn.GetAttribute("href").Should().Be("/account/login");
        cut.Find("a.account-link").GetAttribute("href").Should().Be("/account/forgot-password");
        cut.Find("a.account-link").TextContent.Trim().Should().Be("Forgotten your password?");
    }

    [Fact]
    public void AnExpiredLink_SaysTheDayItWorkedUntil()
    {
        _sender.Failure = new InvitationRefusedException(InvitationRefusal.Expired) { LastDay = new DateOnly(2026, 11, 2) };

        var (cut, _) = Render(Opened());

        CardShouldRead(cut, "Invitation expired",
            "This invitation has expired: its link worked until 2026-11-02.", "Ask the person who invited you for a new invitation.");
    }

    [Fact]
    public void WithNoCookie_ThePageSaysTheLinkIsIncomplete_AndAsksNothing()
    {
        var (cut, _) = Render(cookie: null);

        CardShouldRead(cut, "Invitation link incomplete",
            "The invitation token is missing.",
            "Open the link from the invitation email again. If the email split it over two lines, copy all of it into the address bar.");
        _sender.Previews.Should().Be(0, "there is nothing to look up");
    }

    [Fact]
    public void ACookieThatCannotBeRead_IsNoCookie()
    {
        var (cut, _) = Render(cookie: "forged-value");

        cut.Find("h1").TextContent.Trim().Should().Be("Invitation link incomplete");
        _sender.Previews.Should().Be(0);
    }

    [Fact]
    public void ASubmitAfterTheThirtyMinutes_IsTimedOut()
    {
        var (cut, _) = Render(CookieOf(new RegisterState(null, [], null, null, TimedOut: true, Until: Now.AddMinutes(30))));

        CardShouldRead(cut, "Invitation timed out",
            "This page held the invitation for 30 minutes, and that time has passed. Nothing was saved.",
            "Try again: open the link in the invitation email once more.");
        _sender.Previews.Should().Be(0);
    }

    [Fact]
    public void AFaultAtTheSubmit_IsNotCompleted_AndTryAgainReopensTheFormFromTheCookie()
    {
        var (cut, context) = Render(Refused([RegisterOutcome.Failed], "Mohammed", "Patel"));

        CardShouldRead(cut, "Registration not completed",
            "Registration could not be completed. Nothing was saved.",
            "The invitation is still open in this browser for 30 minutes from when you opened its link.");
        var tryAgain = cut.Find(".form-actions a.btn.btn-outline");
        tryAgain.TextContent.Trim().Should().Be("Try again");
        tryAgain.GetAttribute("href").Should().Be("/account/register");
        ConsumedCodes(context).Should().BeTrue("so Try again shows the form, not this card again");
    }

    [Fact]
    public void AFaultInThePreview_IsLogged_AndNeverShownInItsOwnWords()
    {
        var fault = new InvalidOperationException("Npgsql: connection to 10.0.0.5 refused. Call 012 345 6789");
        _sender.Failure = fault;

        var (cut, _) = Render(Opened());

        cut.Find("h1").TextContent.Trim().Should().Be("Registration not completed");
        cut.Markup.Should().NotContain("Npgsql").And.NotContain("012 345 6789");
        cut.FindAll("form").Should().BeEmpty();
        _logger.Errors.Should().ContainSingle().Which.Should().BeSameAs(fault);
    }

    [Fact]
    public void AnAccountCreatedButNotLoaded_SaysSo_OffersSignIn_AndForgetsTheInvitation()
    {
        var (cut, context) = Render(Refused([RegisterOutcome.UserNotLoaded]));

        CardShouldRead(cut, "Registered, but not signed in",
            "The invited user could not be loaded after registration.",
            "Your account was created. Sign in with this email address and the password you chose.");
        cut.Find(".form-actions a.btn.btn-primary").GetAttribute("href").Should().Be("/account/login");
        _sender.Previews.Should().Be(0, "the invitation is used now, and the preview would say so instead");
        DeletedCookie(context).Should().BeTrue();
    }

    [Fact]
    public void ASignedInVisitor_IsToldWhoIsSignedIn_AndOfferedSignOutAndContinue_WithNoForm()
    {
        var (cut, _) = Render(Opened(), signedInAs: "Pieter Smit");

        CardShouldRead(cut, "Signed in as someone else",
            "You are signed in as Pieter Smit. This invitation is for invitee@hospital.test.",
            "To register it, sign out first. Your own account is not changed.");
        cut.Find(".account-form-container p:not(.account-purpose)").InnerHtml.Should().Contain("@<wbr>");

        var form = cut.FindAll("form").Should().ContainSingle().Which;
        form.GetAttribute("action").Should().Be(SignOutOutcome.SubmitPath);
        form.GetAttribute("method").Should().Be("post");
        var returnTo = form.QuerySelector("input[type=hidden][name=returnTo]")!;
        returnTo.GetAttribute("value").Should().Be(RegisterOutcome.PagePath, "F5: back to Register after signing out");
        form.QuerySelector("button[type=submit]")!.TextContent.Trim().Should().Be("Sign out and continue");
        var home = cut.Find("a.account-link");
        home.TextContent.Trim().Should().Be("Go to Home");
        home.GetAttribute("href").Should().Be("/");
        cut.FindAll("input[name=Password]").Should().BeEmpty();
    }

    /// <summary>
    /// B1: a form posted after another invitation's link was opened in this browser registers nothing, and the page says
    /// so, naming the invitation the cookie now holds; Continue shows its form.
    /// </summary>
    [Fact]
    public void AFormForAnotherInvitation_IsItsCard_NamingTheOneHeldNow_AndContinueShowsItsForm()
    {
        var (cut, context) = Render(Refused([RegisterOutcome.OtherInvitation]));

        CardShouldRead(cut, "Another invitation opened",
            "Another invitation's link was opened in this browser after this form was shown, so nothing was registered.",
            "The invitation open in this browser now is for invitee@hospital.test. Continue to register it, or open the link in your own invitation email again.");
        cut.FindAll(".account-form-container > p:not(.account-purpose)").ElementAt(1).InnerHtml.Should().Contain("@<wbr>");
        var go = cut.Find(".form-actions a.btn.btn-primary");
        go.TextContent.Trim().Should().Be("Continue");
        go.GetAttribute("href").Should().Be("/account/register");
        ConsumedCodes(context).Should().BeTrue("so Continue shows the form");
    }

    /// <summary>A signed-in visitor's submit comes back with no codes (M4); the card is the same whatever the cookie holds.</summary>
    [Fact]
    public void ASignedInVisitor_SeesTheCard_EvenWithARefusalInTheCookie()
    {
        var (cut, _) = Render(Refused([RegisterOutcome.ConfirmationMismatch]), signedInAs: "Pieter Smit");

        cut.Find("h1").TextContent.Trim().Should().Be("Signed in as someone else");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static void CardShouldRead(IRenderedComponent<Register> cut, string heading, params string[] sentences)
    {
        cut.FindAll("h1").Should().ContainSingle().Which.TextContent.Trim().Should().Be(heading);
        cut.FindAll(".account-form-container > p:not(.account-purpose)").Select(paragraph => paragraph.TextContent.Trim())
            .Should().Equal(sentences);
        cut.FindAll("form[action='/account/register/submit']").Should().BeEmpty("a card offers no form");
        cut.FindAll(".alert").Should().BeEmpty("a card's words are the card's, not an alert's");
    }

    /// <summary>The id of the one field with autofocus; null when none has it.</summary>
    private static string? OnlyFocused(IRenderedComponent<Register> cut)
    {
        var focused = cut.FindAll("[autofocus]");
        focused.Count.Should().BeLessThanOrEqualTo(1);
        return focused.SingleOrDefault()?.Id;
    }

    private string Opened() => CookieOf(new RegisterState(Token, [], null, null, TimedOut: false, Until: Now.AddMinutes(30)));

    private string Refused(IReadOnlyList<string> codes, string? firstName = null, string? lastName = null)
        => CookieOf(new RegisterState(Token, codes, firstName, lastName, TimedOut: false, Until: Now.AddMinutes(25)));

    /// <summary>The cookie's value as the endpoint writes it.</summary>
    private string CookieOf(RegisterState state)
    {
        var writer = new DefaultHttpContext();
        _cookie.Write(writer, state);
        return RegisterCookieValues.ValueIn(writer.Response.Headers.SetCookie.ToString());
    }

    private (IRenderedComponent<Register> Cut, HttpContext Context) Render(string? cookie, string? signedInAs = null)
    {
        var context = new DefaultHttpContext();
        if (cookie is not null)
        {
            context.Request.Headers.Cookie = $"{RegisterCookie.CookieName}={cookie}";
        }

        if (signedInAs is not null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, signedInAs)], "Identity.Application"));
        }

        // A visitor's page is static (ExcludeFromInteractiveRouting), so PasswordField's toggles are wombat.js's.
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));
        var cut = RenderComponent<Register>(parameters => parameters.AddCascadingValue<HttpContext>(context));
        return (cut, context);
    }

    /// <summary>Whether the page rewrote the cookie without its codes, so they are shown once.</summary>
    private bool ConsumedCodes(HttpContext context)
    {
        var written = context.Response.Headers.SetCookie.ToString();
        if (!written.Contains($"{RegisterCookie.CookieName}=", StringComparison.Ordinal))
        {
            return false;
        }

        var reader = new DefaultHttpContext();
        reader.Request.Headers.Cookie = $"{RegisterCookie.CookieName}={RegisterCookieValues.ValueIn(written)}";
        return _cookie.Read(reader) is { Codes.Count: 0, Token: Token };
    }

    private static bool DeletedCookie(HttpContext context)
        => context.Response.Headers.SetCookie.ToString().StartsWith($"{RegisterCookie.CookieName}=;", StringComparison.Ordinal);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>Answers the page's one query: the invitation's preview, or the failure a test sets.</summary>
    private sealed class PreviewSender : IScopedSender
    {
        public InvitationPreviewDto Preview { get; set; } = new(
            "invitee@hospital.test", WombatRoles.Coordinator, "Kgosi Kgari Teaching Hospital", null, null, null, new DateOnly(2026, 11, 2));

        public Exception? Failure { get; set; }

        public int Previews { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is not GetInvitationPreviewQuery { Token: Token })
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            Previews++;
            return Failure is null
                ? Task.FromResult((TResponse)(object)Preview)
                : Task.FromException<TResponse>(Failure);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }

    private sealed class CapturingLogger : ILogger<Register>
    {
        private readonly ConcurrentQueue<Exception?> _errors = new();

        public IReadOnlyList<Exception?> Errors => _errors.ToList();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel >= LogLevel.Error)
            {
                _errors.Enqueue(exception);
            }
        }
    }
}
