using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Wombat.Application.Features.Invitations;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// T285: the addresses the sign-in, link and register endpoints redirect to carry codes, and only codes; each code the
/// pages know has its one sentence, and nothing an exception says reaches an address.
/// </summary>
public sealed class AccountRefusalCodeTests
{
    /// <summary>The lockout AddInfrastructure sets.</summary>
    private static readonly TimeSpan Lockout = TimeSpan.FromMinutes(15);

    [Fact]
    public void TheSignInPagesAddress_CarriesTheCode_AndTheReturnAddress()
    {
        SignInOutcome.Url(SignInOutcome.Refused).Should().Be("/account/login?error=Refused");
        SignInOutcome.Url(SignInOutcome.SessionEnded, "/account/change-password")
            .Should().Be("/account/login?error=SessionEnded&returnUrl=%2Faccount%2Fchange-password");
        SignInOutcome.Url(ExternalLoginRefusal.EmailInUse, " ").Should().Be("/account/login?error=SsoEmailInUse");
    }

    [Fact]
    public void ARefusedSignIn_PointsAtTheInstitutionalButton_OnlyWhereThePageOffersOne()
    {
        SignInOutcome.Describe(SignInOutcome.Refused, institutionalSignInOffered: false, Lockout)
            .Should().Be(SignInMessages.InvalidCredentials);
        SignInOutcome.Describe(SignInOutcome.Refused, institutionalSignInOffered: true, Lockout)
            .Should().Be(SignInMessages.InvalidCredentialsOrInstitutional);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void NoCode_IsNoRefusal(string? code)
    {
        SignInOutcome.Describe(code, institutionalSignInOffered: false, Lockout).Should().BeNull();
        LinkExternalOutcome.Describe(code).Should().BeNull();
    }

    [Theory]
    [InlineData("Call 012")]
    [InlineData("refused")]
    [InlineData("SSOEMAILINUSE")]
    [InlineData("Invalid email or password.")]
    public void ACodeThePagesDoNotKnow_ReadsAsTheirGeneralSentence(string code)
    {
        SignInOutcome.Describe(code, institutionalSignInOffered: false, Lockout).Should().Be(SignInOutcome.GeneralRefusal);
        LinkExternalOutcome.Describe(code).Should().Be(LinkExternalOutcome.GeneralRefusal);
        RegisterOutcome.IsFieldCode(code).Should().BeFalse("the register page shows a code it does not know as G14 (T373)");
        RegisterOutcome.InvitationRefusalOf(code).Should().BeNull();
    }

    [Fact]
    public void EveryInstitutionalSignInCode_HasOneSentence_TheSameOnTheSignInAndLinkPages()
    {
        ExternalLoginRefusal.All.Should().OnlyHaveUniqueItems().And.OnlyContain(code => code.StartsWith("Sso", StringComparison.Ordinal),
            "a prefix of their own, so none can mean something else on a page with codes of its own");

        foreach (var code in ExternalLoginRefusal.All)
        {
            var sentence = ExternalLoginRefusal.Describe(code);
            sentence.Should().NotBeNullOrWhiteSpace(code);
            SignInOutcome.Describe(code, institutionalSignInOffered: false, Lockout).Should().Be(sentence);
            LinkExternalOutcome.Describe(code).Should().Be(sentence);
        }

        ExternalLoginRefusal.Describe("SsoSomethingElse").Should().BeNull();
    }

    [Fact]
    public void TheLinkPagesAddress_CarriesTheReturnAddress_AndACodeWhenThereIsOne()
    {
        LinkExternalOutcome.Url(null, null).Should().Be("/account/link-external?returnUrl=%2F");
        LinkExternalOutcome.Url("/progress", LinkExternalOutcome.PasswordRequired)
            .Should().Be("/account/link-external?returnUrl=%2Fprogress&error=PasswordRequired");
        LinkExternalOutcome.Url("/", ExternalLoginRefusal.LinkRefused)
            .Should().Be("/account/link-external?returnUrl=%2F&error=SsoLinkRefused");
    }

    [Theory]
    [InlineData(InvitationRefusal.Invalid, "InvitationInvalid")]
    [InlineData(InvitationRefusal.Revoked, "InvitationRevoked")]
    [InlineData(InvitationRefusal.Used, "InvitationUsed")]
    [InlineData(InvitationRefusal.Expired, "InvitationExpired")]
    [InlineData(InvitationRefusal.AccountExists, "AccountExists")]
    [InlineData(InvitationRefusal.AddressNotAccepted, "AddressNotAccepted")]
    public void AnInvitationsRefusal_TravelsAsItsCode_AndReadsAsItsSentence(InvitationRefusal reason, string code)
    {
        var refusal = new InvitationRefusedException(reason);

        RegisterOutcome.IsRefusal(refusal).Should().BeTrue();
        RegisterOutcome.CodesFor(refusal).Should().Equal(code);
        RegisterOutcome.InvitationRefusalOf(code).Should().Be(reason, "the page shows the refusal's own card (T373)");
    }

    /// <summary>
    /// A refusal added later with no code of its own would travel as <see cref="RegisterOutcome.Failed" /> and read as the
    /// general sentence. Only Identity's refusal of the account travels as Identity's codes instead.
    /// </summary>
    [Fact]
    public void EveryInvitationRefusal_ButIdentitysOwn_HasACodeOfItsOwn()
    {
        foreach (var reason in Enum.GetValues<InvitationRefusal>().Where(reason => reason != InvitationRefusal.AccountNotCreated))
        {
            var codes = RegisterOutcome.CodesFor(new InvitationRefusedException(reason));

            codes.Should().ContainSingle().Which.Should().NotBe(RegisterOutcome.Failed, $"{reason} has a code of its own");
            RegisterOutcome.InvitationRefusalOf(codes[0]).Should().Be(reason);
        }
    }

    [Fact]
    public void AnAccountIdentityRefused_TravelsAsIdentitysCodes_NeverItsWords()
    {
        var refusal = new InvitationRefusedException(
            InvitationRefusal.AccountNotCreated,
            "Identity refused the new account (PasswordTooShort, InvalidEmail).",
            ["PasswordTooShort", "InvalidEmail"]);

        RegisterOutcome.CodesFor(refusal).Should().Equal("PasswordTooShort", "InvalidEmail");
        RegisterOutcome.PasswordRulesMessage(RegisterOutcome.CodesFor(refusal), new() { RequiredLength = 12 })
            .Should().Be("The new password needs: At least 12 characters.", "G04: Password's own message (T373)");

        RegisterOutcome.CodesFor(new InvitationRefusedException(InvitationRefusal.AccountNotCreated, "no codes", []))
            .Should().Equal(RegisterOutcome.Failed);
    }

    /// <summary>
    /// T373 (flow 11, S15, G06): a validator's refusal travels as one code for each field it refused, so the page puts each
    /// message under its field. Until T373 every one was DetailsInvalid, one sentence above the form.
    /// </summary>
    [Fact]
    public void AValidatorsRefusal_TravelsAsACodeForEachFieldItRefused()
    {
        var refusal = new ValidationException(
        [
            new ValidationFailure("FirstName", "'First Name' must not be empty.") { ErrorCode = "NotEmptyValidator" },
            new ValidationFailure("LastName", "too long") { ErrorCode = "MaximumLengthValidator" },
            new ValidationFailure("LastName", "too long again") { ErrorCode = "MaximumLengthValidator" },
            new ValidationFailure("Password", "'Password' must not be empty.") { ErrorCode = "NotEmptyValidator" }
        ]);

        RegisterOutcome.IsRefusal(refusal).Should().BeTrue();
        RegisterOutcome.CodesFor(refusal).Should().Equal(
            RegisterOutcome.FirstNameMissing, RegisterOutcome.LastNameTooLong, RegisterOutcome.PasswordMissing);
        RegisterOutcome.CodesFor(refusal).Should().OnlyContain(code => RegisterOutcome.IsFieldCode(code));
        RegisterOutcome.CodesFor(new ValidationException([new ValidationFailure("Token", "empty")]))
            .Should().Equal(new[] { RegisterOutcome.Failed }, "the token is no field of the form's");
    }

    [Fact]
    public void AnythingElse_TravelsAsFailed_AndItsMessageGoesNowhere()
    {
        var fault = new InvalidOperationException("Npgsql: connection to 10.0.0.5 refused. Call 012 345 6789");

        RegisterOutcome.IsRefusal(fault).Should().BeFalse("the caller logs it");
        RegisterOutcome.CodesFor(fault).Should().Equal(RegisterOutcome.Failed);
    }
}
