using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// T339, flow 02: the sign-in and account pages' words as round 3's Spec has them, "Please" dropped everywhere, the
/// notices told apart from the refusals, a lockout's words the same as a wrong password's (T287), and the six password
/// rules in one order under one heading. The sentences are written out, so a changed word fails here.
/// </summary>
public sealed class AccountWordsTests
{
    private static readonly TimeSpan Lockout = TimeSpan.FromMinutes(15);

    /// <summary>The rules AddInfrastructure sets.</summary>
    private static readonly PasswordOptions Rules = new()
    {
        RequireDigit = true,
        RequireLowercase = true,
        RequireUppercase = true,
        RequireNonAlphanumeric = true,
        RequiredLength = 12,
        RequiredUniqueChars = 4
    };

    [Theory]
    [InlineData("FieldsMissing", false, "Enter your email and your password.")]
    [InlineData("Refused", false, "Invalid email or password.")]
    [InlineData("Refused", true, "Invalid email or password. If your institution signs you in, use its sign-in button below.")]
    [InlineData("LockedOut", false, "Invalid email or password.")]
    [InlineData("LockedOut", true, "Invalid email or password. If your institution signs you in, use its sign-in button below.")]
    [InlineData("TooManyAttempts", false, "Too many failed sign-in attempts from this network. Wait a few minutes and try again.")]
    [InlineData("ExternalLoginUnavailable", false,
        "Your institution's sign-in did not complete. Sign in with your email and password.")]
    [InlineData("ExternalLoginUnavailable", true,
        "Your institution's sign-in did not complete. Try your institution's button again, or sign in with your email and password.")]
    [InlineData("ExternalSessionExpired", true,
        "Your institution's sign-in took too long and has expired. Use your institution's button to start again.")]
    [InlineData("ExternalSessionExpired", false,
        "Your institution's sign-in took too long and has expired. Sign in with your email and password.")]
    [InlineData("SsoFailed", false,
        "Wombat could not sign you in through your institution this time. Try again in a few minutes, or sign in with your email and password.")]
    [InlineData("SsoUnknownProvider", false,
        "That institution's sign-in is not set up in Wombat. Sign in with your email and password.")]
    [InlineData("SsoNoEmail", false,
        "Your institution's sign-in did not give Wombat your email address, so Wombat cannot find your account. Sign in with your email and password, or ask your administrator for help.")]
    [InlineData("SsoAccountLocked", false, "Wombat could not sign you in through your institution. Contact your administrator.")]
    [InlineData("Anything else", false, "Sign-in could not be completed. Try again.")]
    [InlineData("SessionEnded", false, "Your session has ended. Sign in again.")]
    [InlineData("PasswordChanged", false, "Your password was changed. Sign in with your new password.")]
    [InlineData("SignedOut", false, "You have signed out.")]
    [InlineData("LockedSignedOut", false,
        "Your current password was entered incorrectly too many times, so your account is locked for 15 minutes and you have been signed out. Wait 15 minutes, then sign in again.")]
    public void EachSignInCode_ReadsAsTheSpecHasIt(string code, bool institutionsOffered, string expected)
        => SignInOutcome.Describe(code, institutionsOffered, Lockout).Should().Be(expected);

    /// <summary>
    /// The sign-in, link, register, change-password and My account sentences: every one the account pages write for a
    /// code (T339, flow 02, "Please" dropped everywhere). Not every sentence Wombat writes: the other pages' are not here.
    /// </summary>
    [Fact]
    public void NoAccountSentence_SaysPlease()
    {
        var codes = new[]
        {
            SignInOutcome.FieldsMissing, SignInOutcome.Refused, SignInOutcome.LockedOut, SignInOutcome.TooManyAttempts,
            SignInOutcome.SessionEnded, SignInOutcome.PasswordChanged, SignInOutcome.SignedOut, SignInOutcome.LockedSignedOut,
            SignInOutcome.ExternalLoginUnavailable, SignInOutcome.ExternalSessionExpired, SignInOutcome.SsoFailed, "Unknown"
        }.Concat(ExternalLoginRefusal.All);

        var sentences = codes
            .SelectMany(code => new[] { SignInOutcome.Describe(code, false, Lockout), SignInOutcome.Describe(code, true, Lockout) })
            .Concat(ExternalLoginRefusal.All.Select(LinkExternalOutcome.Describe))
            .Append(LinkExternalOutcome.Describe(LinkExternalOutcome.PasswordRequired))
            .Append(LinkExternalOutcome.GeneralRefusal)
            .Append(RegisterOutcome.GeneralRefusal)
            .Concat(ChangePasswordOutcome.Describe(
                ["FieldsMissing", "ConfirmationMismatch", "TooManyAttempts", "InstitutionalSignIn", "PasswordMismatch", "Failed",
                    ChangePasswordOutcome.AccountLocked],
                new IdentityErrorDescriber(),
                Rules))
            .Concat(new[]
            {
                ProfileOutcome.FirstNameMissing, ProfileOutcome.LastNameMissing, ProfileOutcome.NameMissing,
                ProfileOutcome.NameTooLong, ProfileOutcome.Failed, "Unknown"
            }.Select(ProfileOutcome.Describe))
            .Concat(new[]
            {
                ProfileOutcome.RemoveWrongPassword, ProfileOutcome.RemoveLastSignIn, ProfileOutcome.RemoveFailed,
                ProfileOutcome.RemoveTooManyAttempts, ProfileOutcome.RemoveAccountLocked
            }.SelectMany(code => new[]
            {
                ProfileOutcome.DescribeRemoval(code, null, Lockout), ProfileOutcome.DescribeRemoval(code, "Kgosi Hospital", Lockout)
            }))
            .Append(ProfileOutcome.SavedMessage)
            .Append(ProfileOutcome.PasswordUpdatedMessage)
            .Append(ProfileOutcome.FirstNameMissingMessage)
            .Append(ProfileOutcome.LastNameMissingMessage)
            .Append(ProfileOutcome.IncorrectPasswordMessage)
            .Append(ProfileOutcome.LoadErrorMessage)
            .Append(ProfileOutcome.SignInRemovedMessage(null))
            .Append(ProfileOutcome.SignInRemovedMessage("Kgosi Hospital"))
            .ToList();

        sentences.Should().NotContain(sentence => sentence!.Contains("Please", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>The page shows the four notices as information, and everything else, an unknown code included, as a refusal.</summary>
    [Theory]
    [InlineData("SessionEnded", true)]
    [InlineData("PasswordChanged", true)]
    [InlineData("SignedOut", true)]
    [InlineData("LockedSignedOut", true)]
    [InlineData("Refused", false)]
    [InlineData("LockedOut", false)]
    [InlineData("FieldsMissing", false)]
    [InlineData("TooManyAttempts", false)]
    [InlineData("SsoFailed", false)]
    [InlineData("SsoAccountLocked", false)]
    [InlineData("signedout", false)]
    [InlineData("Call 012", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void OnlyTheFourNotices_AreNotices(string? code, bool notice)
        => SignInOutcome.IsNotice(code).Should().Be(notice);

    [Fact]
    public void TheNoticeCodes_AreTheLiteralsTheEndpointsShare()
    {
        SignInOutcome.SignedOut.Should().Be("SignedOut");
        ChangePasswordOutcome.LockedSignedOut.Should().Be("LockedSignedOut",
            "My account's Remove endpoint redirects with it too (lane C's contract)");
        SignInOutcome.LockedSignedOut.Should().Be(ChangePasswordOutcome.LockedSignedOut);
        SignInOutcome.Url(ChangePasswordOutcome.LockedSignedOut, null).Should().Be("/account/login?error=LockedSignedOut");
        SignOutOutcome.SignedOutUrl.Should().Be("/account/login?error=SignedOut");
    }

    /// <summary>The lockout's length is the one Identity is configured with, never a number written into the words.</summary>
    [Theory]
    [InlineData(15, "locked for 15 minutes", "Wait 15 minutes,")]
    [InlineData(5, "locked for 5 minutes", "Wait 5 minutes,")]
    [InlineData(1, "locked for 1 minute ", "Wait 1 minute,")]
    public void TheLockNotice_SaysTheConfiguredLockout(int minutes, string locked, string wait)
        => SignInOutcome.Describe(SignInOutcome.LockedSignedOut, false, TimeSpan.FromMinutes(minutes))
            .Should().Contain(locked).And.Contain(wait);

    [Fact]
    public void AChangedPassword_GoesToMyAccount_SayingSo()
    {
        ChangePasswordOutcome.Updated.Should().Be("password-updated", "My account's contract with change password (lane C)");
        ChangePasswordOutcome.UpdatedUrl.Should().Be("/account/profile?status=password-updated");
    }

    [Fact]
    public void TheLinkPagesOwnWords_ReadAsTheSpecHasThem()
    {
        LinkExternalOutcome.Describe(LinkExternalOutcome.PasswordRequired).Should().Be("Enter your password.");
        LinkExternalOutcome.Describe("Call 012").Should().Be("The account could not be linked. Try again.");
        LinkExternalOutcome.Describe(ExternalLoginRefusal.LinkRefused)
            .Should().Be("The account could not be linked. Check your password and try again.");
        LinkExternalOutcome.Describe(ExternalLoginRefusal.LinkLockedOut)
            .Should().Be(LinkExternalOutcome.Describe(ExternalLoginRefusal.LinkRefused), "a lockout reads as any refused link (T287)");
        LinkExternalOutcome.Describe(ExternalLoginRefusal.LinkFailed)
            .Should().Be("Your institutional sign-in could not be linked this time. Try again later.");
    }

    // ─── The six password rules ─────────────────────────────────────────────

    [Fact]
    public void TheRules_AreSixSentences_InOneOrder_UnderOneHeading()
    {
        PasswordRuleMessages.Heading.Should().Be("The new password needs:");
        PasswordRuleMessages.FieldMessage.Should().Be("The new password does not meet the rules below.");
        PasswordRuleMessages.All(Rules).Should().Equal(
            "At least 12 characters.",
            "At least 4 different characters.",
            "A digit (0 to 9).",
            "An upper-case letter.",
            "A lower-case letter.",
            "A symbol, such as ! or #.");
    }

    [Fact]
    public void TheRulesList_ComesFromTheOptions_NotFromNumbersWrittenHere()
        => PasswordRuleMessages.All(new PasswordOptions
            {
                RequiredLength = 16,
                RequiredUniqueChars = 1,
                RequireDigit = true,
                RequireUppercase = false,
                RequireLowercase = false,
                RequireNonAlphanumeric = false
            })
            .Should().Equal("At least 16 characters.", "A digit (0 to 9).");

    [Fact]
    public void ARefusal_ListsOnlyTheRulesBroken_InTheListsOrder_WhateverOrderIdentitySentThem()
        => PasswordRuleMessages.Broken(
                ["PasswordRequiresNonAlphanumeric", "PasswordRequiresUpper", "PasswordMismatch", "PasswordTooShort", "PasswordRequiresDigit", "PasswordTooShort"],
                Rules)
            .Should().Equal("At least 12 characters.", "A digit (0 to 9).", "An upper-case letter.", "A symbol, such as ! or #.");

    [Fact]
    public void IdentitysOwnErrors_CarryTheSameSentences()
    {
        var describer = new WombatIdentityErrorDescriber();

        describer.PasswordTooShort(12).Description.Should().Be("At least 12 characters.");
        describer.PasswordRequiresUniqueChars(4).Description.Should().Be("At least 4 different characters.");
        describer.PasswordRequiresDigit().Description.Should().Be("A digit (0 to 9).");
        describer.PasswordRequiresUpper().Description.Should().Be("An upper-case letter.");
        describer.PasswordRequiresLower().Description.Should().Be("A lower-case letter.");
        describer.PasswordRequiresNonAlphanumeric().Description.Should().Be("A symbol, such as ! or #.");
        describer.PasswordRequiresDigit().Code.Should().Be("PasswordRequiresDigit", "the codes are Identity's own");
    }

    [Fact]
    public void ChangePasswordsRuleRefusal_SaysNotChanged_ThenTheHeading_ThenTheRulesBroken()
    {
        var refusal = ChangePasswordOutcome.Refusal(
            ["PasswordRequiresNonAlphanumeric", "PasswordRequiresDigit", "PasswordTooShort", "PasswordRequiresUpper"],
            new IdentityErrorDescriber(),
            Rules);

        refusal.Headline.Should().Be("Your password was not changed.");
        refusal.Sentences.Should().BeEmpty();
        refusal.BrokenRules.Should().Equal(
            "At least 12 characters.", "A digit (0 to 9).", "An upper-case letter.", "A symbol, such as ! or #.");
        string.Join(" ", refusal.Flat).Should().Be(
            "Your password was not changed. The new password needs: At least 12 characters. A digit (0 to 9). " +
            "An upper-case letter. A symbol, such as ! or #.");
    }

    [Theory]
    [InlineData("PasswordMismatch", "Your password was not changed. Incorrect password.")]
    [InlineData("ConfirmationMismatch", "Your password was not changed. The password confirmation does not match.")]
    [InlineData("FieldsMissing",
        "Your password was not changed. Enter your current password, a new password, and the new password again to confirm it.")]
    [InlineData("TooManyAttempts", "Too many attempts from this network. Wait a few minutes and try again.")]
    [InlineData("Failed", "The password could not be changed. Try again.")]
    [InlineData("Call 012", "The password could not be changed. Try again.")]
    [InlineData("LockedOut", "The password could not be changed. Try again.")]
    [InlineData("InstitutionalSignIn", "This account signs in through your institution, so it has no password to change here.")]
    public void ChangePasswordsOtherRefusals_ReadAsTheSpecHasThem(string code, string expected)
        => string.Join(" ", ChangePasswordOutcome.Describe([code], new IdentityErrorDescriber(), Rules)).Should().Be(expected);

    /// <summary>
    /// An account locked already when the form was posted (the review of the t339 branch): refused on the page, with the
    /// wait, and not a refusal the person can put right, so no "not changed" heading.
    /// </summary>
    [Theory]
    [InlineData(15, "Your account is locked. Wait 15 minutes, then try again.")]
    [InlineData(1, "Your account is locked. Wait 1 minute, then try again.")]
    public void AnAccountLockedAlready_IsRefusedWithTheWait(int minutes, string expected)
    {
        var refusal = ChangePasswordOutcome.Refusal([ChangePasswordOutcome.AccountLocked], new IdentityErrorDescriber(), Rules,
            TimeSpan.FromMinutes(minutes));

        refusal.Headline.Should().BeNull();
        refusal.Flat.Should().Equal(expected);
        ProfileOutcome.DescribeRemoval(ProfileOutcome.RemoveAccountLocked, "Kgosi Hospital", TimeSpan.FromMinutes(minutes))
            .Should().Be("Your sign-in was not removed. " + expected);
    }

    [Fact]
    public void TheLockedRefusals_TravelWithTheWaitInWholeMinutes_AndTheRemovalsIsShownInTheDialog()
    {
        ChangePasswordOutcome.AccountLocked.Should().Be("AccountLocked");
        ChangePasswordOutcome.LockedUrl(TimeSpan.FromSeconds(14 * 60 + 1))
            .Should().Be("/account/change-password?error=AccountLocked&minutes=15", "rounded up");
        ProfileOutcome.RemoveLockedUrl("kgk", TimeSpan.FromSeconds(30))
            .Should().Be("/account/profile?error=RemoveAccountLocked&provider=kgk&minutes=1", "never under one");
        ProfileOutcome.IsRemoveRefusal(ProfileOutcome.RemoveAccountLocked).Should().BeTrue();
        ProfileOutcome.IsShownInTheDialog(ProfileOutcome.RemoveAccountLocked).Should().BeTrue("the dialog opens again with it");
    }

    /// <summary>A crafted link can shorten the wait the page says, never lengthen it past the configured lockout.</summary>
    [Theory]
    [InlineData("7", 7)]
    [InlineData("15", 15)]
    [InlineData("16", 15)]
    [InlineData("9999", 15)]
    [InlineData("0", 15)]
    [InlineData("-3", 15)]
    [InlineData("seven", 15)]
    [InlineData(null, 15)]
    public void TheWaitAnAddressSays_IsHeldToTheConfiguredLockout(string? sent, int minutes)
        => ChangePasswordOutcome.LockedFor(sent, Lockout).Should().Be(TimeSpan.FromMinutes(minutes));

    [Fact]
    public void NoCodes_AreNoRefusal()
    {
        ChangePasswordOutcome.Describe(null, new IdentityErrorDescriber(), Rules).Should().BeEmpty();
        ChangePasswordOutcome.Refusal([" "], new IdentityErrorDescriber(), Rules).Any.Should().BeFalse();
    }

    [Fact]
    public void RegistersRuleRefusal_UsesTheSameHeadingOrderAndSentences()
        => RegisterOutcome.Describe(["PasswordRequiresUpper", "ConfirmationMismatch", "PasswordTooShort"], new IdentityErrorDescriber(), Rules)
            .Should().Equal(
                "The password confirmation does not match.",
                "The new password needs:",
                "At least 12 characters.",
                "An upper-case letter.");
}
