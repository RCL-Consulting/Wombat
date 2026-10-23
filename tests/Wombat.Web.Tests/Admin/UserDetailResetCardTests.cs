using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Web.Services;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Commands.ResetUserPassword;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Security;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The administrator's reset card (T339, flow 02, E11; R3-CP-AdminReset): New password is a <c>PasswordField</c> in the
/// circuit, the six rules under it before anything is typed, no 8-character rule, and a refusal that breaks a rule marks
/// the field, which then names its own message and the refusal instead of the rules (A15). Since T376 (flow 12, P10, P11)
/// it is Sign-in and access's Password part: Reset password is never disabled, an empty field is refused beside it, and
/// the refusal and the result show in the part.
/// </summary>
public sealed class UserDetailResetCardTests : TestContext
{
    private const string RulesId = "reset-password-rules";

    private static readonly string RuleRefusal =
        $"The password was not reset. {PasswordRuleMessages.Heading} A digit (0 to 9). A symbol, such as ! or #.";

    private readonly StubSender _sender = new();

    public UserDetailResetCardTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
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
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("caller@test");
        auth.SetRoles(WombatRoles.Administrator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "admin-1"));
    }

    [Fact]
    public void F3_AnAccountThatMayNotUseAPassword_IsOfferedNone()
    {
        _sender.SsoOnly = true;
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var cut = RenderComponent<UserDetail>(parameters => parameters.Add(page => page.UserId, "user-9"));
        cut.WaitForState(() => cut.FindAll(".section-part").Count > 0);

        cut.FindAll("#reset-password-input").Should().BeEmpty();
        cut.FindAll("button").Should().NotContain(button => button.TextContent.Trim() == "Reset password");
        cut.Markup.Should().Contain("Rene Registrar signs in only through their institution. A password cannot be set for them.");
    }

    [Fact]
    public void NewPassword_HasItsCircuitToggle_AndNamesTheRules_ListedUnderIt()
    {
        var cut = Render();

        PasswordToggleMarkup.ShouldBeTheToggle(cut, "reset-password-input", "Show new password", PasswordToggleMarkup.Driven.ByCircuit);
        var input = cut.Find("#reset-password-input");
        input.GetAttribute("aria-describedby").Should().Be(RulesId);
        input.HasAttribute("aria-invalid").Should().BeFalse();
        var rules = cut.Find($"#{RulesId}");
        rules.ClassList.Should().Contain("password-rules");
        rules.QuerySelector("p")!.TextContent.Trim().Should().Be(PasswordRuleMessages.Heading);
        rules.QuerySelectorAll("li").Should().HaveCount(6);
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ResetPassword_IsNeverDisabled_WithNo8CharacterRule_AndAnEmptyFieldIsRefusedBesideIt()
    {
        var cut = Render();

        ResetButton(cut).HasAttribute("disabled").Should().BeFalse("an action is never natively disabled (T376)");
        cut.Find("#reset-password-input").HasAttribute("minlength").Should().BeFalse();

        ResetButton(cut).Click();

        var input = cut.Find("#reset-password-input");
        input.GetAttribute("aria-invalid").Should().Be("true");
        cut.Find("#reset-password-error").TextContent.Trim().Should().Be("Type the new password.");
        input.GetAttribute("aria-describedby")!.Split(' ').Should().Contain("reset-password-error");
        _sender.Resets.Should().Be(0, "nothing is sent");
    }

    [Fact]
    public void A45_TheReset_IsSaidInPassword_NamingThePerson()
    {
        var cut = Render();

        cut.Find("#reset-password-input").Input("A-long-password-1");
        ResetButton(cut).Click();

        var part = cut.FindAll(".section-part").Single(candidate => candidate.QuerySelector("h3")!.TextContent.Trim() == "Password");
        var alert = part.QuerySelector(".action-result .alert-success")!;
        alert.TextContent.Trim().Should().Be("Password reset for Rene Registrar. Share the new password with Rene Registrar out-of-band.");
        alert.QuerySelector("strong")!.TextContent.Trim().Should().Be("Password reset for Rene Registrar.");
        cut.Find("#reset-password-input").GetAttribute("value").Should().BeNullOrEmpty("the password is not left on the page");
    }

    [Fact]
    public void ARuleRefusal_MarksNewPassword_WithItsMessage_NamingTheMessageAndTheRefusal_NotTheRules()
    {
        _sender.ResetRefusal = RuleRefusal;
        var cut = Render();

        cut.Find("#reset-password-input").Input("short");
        ResetButton(cut).Click();

        var refusal = cut.Find(".section-part .action-result .alert.alert-danger");
        refusal.TextContent.Trim().Should().Be(RuleRefusal);
        var input = cut.Find("#reset-password-input");
        input.GetAttribute("aria-invalid").Should().Be("true");
        var described = input.GetAttribute("aria-describedby")!.Split(' ');
        described.Should().HaveCount(2).And.NotContain(RulesId, "A15: the refusal lists the rules broken; the list says them all");
        cut.Find($"#{described[0]}").TextContent.Trim().Should().Be(PasswordRuleMessages.FieldMessage);
        cut.Find($"#{described[0]}").ClassList.Should().Contain("validation-message");
        described[1].Should().Be(refusal.Id, "the refusal, which lists the rules broken");
        cut.FindAll($"#{RulesId}").Should().ContainSingle("the rules stay under the field");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ARefusalThatIsNoRules_DoesNotMarkTheField()
    {
        _sender.ResetRefusal = "You do not have permission to reset this user's password.";
        var cut = Render();

        cut.Find("#reset-password-input").Input("A-long-password-1");
        ResetButton(cut).Click();

        cut.Find(".alert-danger").TextContent.Trim().Should().Be(_sender.ResetRefusal);
        var input = cut.Find("#reset-password-input");
        input.HasAttribute("aria-invalid").Should().BeFalse();
        input.GetAttribute("aria-describedby").Should().Be(RulesId);
    }

    [Fact]
    public void AnotherActionAfterARuleRefusal_ClearsTheMark()
    {
        _sender.ResetRefusal = RuleRefusal;
        var cut = Render();
        cut.Find("#reset-password-input").Input("short");
        ResetButton(cut).Click();

        _sender.ResetRefusal = null;
        cut.Find("#reset-password-input").Input("A-long-password-1");
        ResetButton(cut).Click();

        cut.Find("#reset-password-input").HasAttribute("aria-invalid").Should().BeFalse();
        cut.Find("#reset-password-input").GetAttribute("aria-describedby").Should().Be(RulesId);
    }

    private IRenderedComponent<UserDetail> Render()
    {
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var cut = RenderComponent<UserDetail>(parameters => parameters.Add(page => page.UserId, "user-9"));
        cut.WaitForState(() => cut.FindAll("#reset-password-input").Count == 1);
        return cut;
    }

    private static AngleSharp.Dom.IElement ResetButton(IRenderedFragment cut)
        => cut.FindAll("button").Single(button => button.TextContent.Trim() == "Reset password");

    /// <summary>Answers the user's read; refuses the reset with <see cref="ResetRefusal" /> when it is set.</summary>
    private sealed class StubSender : IScopedSender
    {
        public string? ResetRefusal { get; set; }

        /// <summary>F3: the account may not use a Wombat password, and has no institutional sign-in listed.</summary>
        public bool SsoOnly { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetUserByIdQuery
                ? Task.FromResult((TResponse)(object)new UserDetailDto(
                    "user-9", "registrar@hospital.test", "Rene", "Registrar", 4, "Groote Schuur Hospital", [], [],
                    [WombatRoles.CommitteeMember], false) { AllowsLocalPassword = !SsoOnly, HasLocalPassword = !SsoOnly })
                : throw new NotSupportedException(request.GetType().Name);

        public int Resets { get; private set; }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (request is not ResetUserPasswordCommand)
            {
                throw new NotSupportedException(request.GetType().Name);
            }

            Resets++;
            return ResetRefusal is null ? Task.CompletedTask : throw new InvalidOperationException(ResetRefusal);
        }
    }
}
