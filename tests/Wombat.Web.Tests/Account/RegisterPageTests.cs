using System.Collections.Concurrent;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wombat.Application.Features.Invitations;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Security;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;

namespace Wombat.Web.Tests.Account;

/// <summary>
/// T285: the register page shows a fixed sentence for each refusal code the register endpoint sends it, and never the
/// words of the address itself or of an exception.
/// </summary>
/// <remarks>
/// Until T285 the endpoint redirected with the sentence in <c>?error=</c>, and its catch with the message of whatever it
/// caught; the page printed either as it arrived, and hid the form. The expected sentences are written out, so a changed
/// word fails here. The integration suite's <c>Hosting/AccountRefusalFlowTests</c> runs the endpoint end to end.
/// </remarks>
public sealed class RegisterPageTests : TestContext
{
    private const string Token = "invitation-token";

    private static readonly string[] FieldIds =
        ["register-first-name", "register-last-name", "register-password", "register-confirm-password"];

    private readonly PreviewSender _sender = new();
    private readonly CapturingLogger _logger = new();

    public RegisterPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<ILogger<Register>>(_logger);
        Services.AddSingleton(new IdentityErrorDescriber());

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

    [Fact]
    public void AFirstVisit_ShowsTheInvitationAndTheForm_AndNamesNoRefusal()
    {
        var cut = Render($"{RegisterOutcome.PagePath}?token={Token}");

        cut.Find(".alert-info").TextContent.Trim().Should().Be("Registering invitee@hospital.test as Coordinator.");
        cut.FindAll(".alert-danger").Should().BeEmpty();
        cut.Find("form").GetAttribute("action").Should().Be(RegisterOutcome.SubmitPath).And.Be("/account/register/submit");
        cut.Find("input[name=Token]").GetAttribute("value").Should().Be(Token);
        FieldIds.Should().OnlyContain(id => !cut.Find($"#{id}").HasAttribute("aria-describedby"));
        cut.Find("#register-first-name").HasAttribute("autofocus").Should().BeFalse(
            "a first visit reads from the top, where the invitation is named");
    }

    [Theory]
    [InlineData("ConfirmationMismatch", "The password confirmation does not match.")]
    [InlineData("UserNotLoaded", "The invited user could not be loaded after registration.")]
    [InlineData("InvitationInvalid", "This invitation is invalid.")]
    [InlineData("InvitationRevoked", "This invitation has been revoked.")]
    [InlineData("InvitationUsed", "This invitation has already been used.")]
    [InlineData("InvitationExpired", "This invitation has expired.")]
    [InlineData("DetailsInvalid", "Enter your first name and last name, each of at most 100 characters, and a password.")]
    [InlineData("Failed", "Registration could not be completed. Please try again.")]
    [InlineData("PasswordTooShort", "Passwords must be at least 12 characters.")]
    [InlineData("PasswordRequiresNonAlphanumeric", "Passwords must have at least one non alphanumeric character.")]
    [InlineData("PasswordRequiresDigit", "Passwords must have at least one digit ('0'-'9').")]
    [InlineData("PasswordRequiresUpper", "Passwords must have at least one uppercase ('A'-'Z').")]
    [InlineData("PasswordRequiresLower", "Passwords must have at least one lowercase ('a'-'z').")]
    [InlineData("PasswordRequiresUniqueChars", "Passwords must use at least 4 different characters.")]
    public void ARefusal_ReadsAsItDidBefore_AboveTheForm_AndIsNamedByEachField(string code, string expected)
    {
        var cut = Render($"{RegisterOutcome.PagePath}?token={Token}&error={code}");

        var refusal = cut.Find(".alert.alert-danger");
        refusal.TextContent.Trim().Should().Be(expected);
        refusal.GetAttribute("role").Should().Be("alert");
        refusal.Id.Should().Be(Register.RefusalId);

        cut.FindAll("form").Should().ContainSingle("the invitation can still be used, so the person puts it right here");
        FieldIds.Should().OnlyContain(id => cut.Find($"#{id}").GetAttribute("aria-describedby") == Register.RefusalId,
            "an alert already on the page when it loads is not reliably announced; each field reads it");
        cut.Find("#register-first-name").HasAttribute("autofocus").Should().BeTrue("the page reloads with the focus in the form");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void SeveralPasswordRules_ReadEachOnce_InTheOrderSent()
    {
        var cut = Render($"{RegisterOutcome.PagePath}?token={Token}" +
                         "&error=PasswordTooShort&error=PasswordRequiresDigit&error=PasswordTooShort");

        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be(
            "Passwords must be at least 12 characters. Passwords must have at least one digit ('0'-'9').");
    }

    [Theory]
    [InlineData("Call%20012", "Call 012")]
    [InlineData("Your%20account%20is%20locked.%20Call%20012%20345%206789", "012 345 6789")]
    [InlineData("A%20user%20with%20this%20email%20address%20already%20exists.%20Call%20012", "Call 012")]
    public void WordsOfTheAddressesOwn_NeverReachThePage(string error, string words)
    {
        var cut = Render($"{RegisterOutcome.PagePath}?token={Token}&error={error}");

        cut.Markup.Should().NotContain(words, "a refusal travels as a code, and the page chooses the words");
        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be(RegisterOutcome.GeneralRefusal)
            .And.Be("Registration could not be completed. Please try again.");
    }

    [Fact]
    public void WithNoToken_ThePageSaysSo_WhateverTheAddressCarries()
    {
        var cut = Render($"{RegisterOutcome.PagePath}?error=Call%20012");

        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be("The invitation token is missing.");
        cut.Markup.Should().NotContain("Call 012");
        cut.FindAll("form").Should().BeEmpty();
        _sender.Previews.Should().Be(0, "there is nothing to look up");
    }

    [Theory]
    [InlineData(InvitationRefusal.Revoked, "This invitation has been revoked.")]
    [InlineData(InvitationRefusal.Used, "This invitation has already been used.")]
    [InlineData(InvitationRefusal.Expired, "This invitation has expired.")]
    [InlineData(InvitationRefusal.Invalid, "This invitation is invalid.")]
    [InlineData(InvitationRefusal.AccountExists, "A user with this email address already exists.")]
    [InlineData(InvitationRefusal.AddressNotAccepted,
        "This email address cannot be used for an account. Ask your administrator for an invitation to another address.")]
    public void AnInvitationThatCannotBeUsed_SaysWhy_AndOffersNoForm(InvitationRefusal reason, string expected)
    {
        _sender.Refusal = new InvitationRefusedException(reason);

        var cut = Render($"{RegisterOutcome.PagePath}?token={Token}&error=ConfirmationMismatch");

        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be(expected,
            "the invitation's own refusal, as before T285, over the one the address carries");
        cut.FindAll("form").Should().BeEmpty();
        _logger.Errors.Should().BeEmpty("a refusal is not a fault");
    }

    /// <summary>
    /// T285 review: an invitation to an address an account holds. The submit is refused as AccountExists, and the page it
    /// reloads says so once and offers no form, because the preview refuses the same address: no submit could succeed.
    /// Until the review the preview answered, and every submit came back to the form, refused again.
    /// </summary>
    [Fact]
    public void AnAddressAnAccountHolds_AfterItsSubmitIsRefused_ReadsOnce_AndOffersNoFormToSubmitAgain()
    {
        _sender.Refusal = new InvitationRefusedException(InvitationRefusal.AccountExists);

        var cut = Render($"{RegisterOutcome.PagePath}?token={Token}&error={RegisterOutcome.AccountExists}");

        cut.FindAll(".alert.alert-danger").Should().ContainSingle()
            .Which.TextContent.Trim().Should().Be("A user with this email address already exists.");
        cut.FindAll("form").Should().BeEmpty();
        cut.FindAll("input").Should().BeEmpty();
        _logger.Errors.Should().BeEmpty("a refusal is not a fault");
    }

    [Fact]
    public void AFaultInThePreview_IsLogged_AndNeverShownInItsOwnWords()
    {
        var fault = new InvalidOperationException("Npgsql: connection to 10.0.0.5 refused. Call 012 345 6789");
        _sender.Refusal = fault;

        var cut = Render($"{RegisterOutcome.PagePath}?token={Token}");

        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be("Registration could not be completed. Please try again.");
        cut.Markup.Should().NotContain("Npgsql").And.NotContain("012 345 6789");
        cut.FindAll("form").Should().BeEmpty();
        _logger.Errors.Should().ContainSingle().Which.Should().BeSameAs(fault);
    }

    private IRenderedComponent<Register> Render(string address)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        return RenderComponent<Register>();
    }

    /// <summary>Answers the page's one query: the invitation's preview, or the refusal a test sets.</summary>
    private sealed class PreviewSender : IScopedSender
    {
        public Exception? Refusal { get; set; }

        public int Previews { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            if (request is not GetInvitationPreviewQuery { Token: Token })
            {
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            Previews++;
            return Refusal is null
                ? Task.FromResult((TResponse)(object)new InvitationPreviewDto("invitee@hospital.test", "Coordinator"))
                : Task.FromException<TResponse>(Refusal);
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
