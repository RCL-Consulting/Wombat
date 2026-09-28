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
        RegisterOutcome.Describe([code], new(), new()).Should().Equal(RegisterOutcome.GeneralRefusal);
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

    [Fact]
    public void TheRegisterPagesAddress_CarriesTheTokenAndEachCodeOnce_AndAFailureWithNoCodeStillSaysSomething()
    {
        RegisterOutcome.Url("a+b/c", [RegisterOutcome.ConfirmationMismatch])
            .Should().Be("/account/register?token=a%2Bb%2Fc&error=ConfirmationMismatch");
        RegisterOutcome.Url("t", ["PasswordTooShort", "PasswordRequiresDigit", "PasswordTooShort", " "])
            .Should().Be("/account/register?token=t&error=PasswordTooShort&error=PasswordRequiresDigit");
        RegisterOutcome.Url("t", []).Should().Be("/account/register?token=t&error=Failed");
        RegisterOutcome.Url(null, [RegisterOutcome.TokenMissing]).Should().Be("/account/register?error=TokenMissing");
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
        RegisterOutcome.Describe([code], new(), new()).Should().Equal(InvitationRefusals.Describe(reason));
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
            RegisterOutcome.Describe(codes, new(), new()).Should().Equal(InvitationRefusals.Describe(reason));
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
        RegisterOutcome.Describe(RegisterOutcome.CodesFor(refusal), new(), new() { RequiredLength = 12 })
            .Should().Equal(RegisterOutcome.GeneralRefusal, "The new password needs:", "At least 12 characters.");

        RegisterOutcome.CodesFor(new InvitationRefusedException(InvitationRefusal.AccountNotCreated, "no codes", []))
            .Should().Equal(RegisterOutcome.Failed);
    }

    [Fact]
    public void AValidatorsRefusal_TravelsAsDetailsInvalid()
    {
        var refusal = new ValidationException([new ValidationFailure("FirstName", "'First Name' must not be empty.")]);

        RegisterOutcome.IsRefusal(refusal).Should().BeTrue();
        RegisterOutcome.CodesFor(refusal).Should().Equal(RegisterOutcome.DetailsInvalid);
    }

    [Fact]
    public void AnythingElse_TravelsAsFailed_AndItsMessageGoesNowhere()
    {
        var fault = new InvalidOperationException("Npgsql: connection to 10.0.0.5 refused. Call 012 345 6789");

        RegisterOutcome.IsRefusal(fault).Should().BeFalse("the caller logs it");
        RegisterOutcome.CodesFor(fault).Should().Equal(RegisterOutcome.Failed);
        RegisterOutcome.Url("t", RegisterOutcome.CodesFor(fault)).Should().Be("/account/register?token=t&error=Failed");
    }
}
