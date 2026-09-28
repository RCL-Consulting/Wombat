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
/// the field, which then names its own message and the refusal instead of the rules (A15). The card's other words are
/// flow 12's.
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
    public void ResetPassword_IsEnabledOnceAnythingIsTyped_WithNo8CharacterRule()
    {
        var cut = Render();

        ResetButton(cut).HasAttribute("disabled").Should().BeTrue("nothing typed yet");
        cut.Find("#reset-password-input").HasAttribute("minlength").Should().BeFalse();

        cut.Find("#reset-password-input").Input("short");

        ResetButton(cut).HasAttribute("disabled").Should().BeFalse("the rule is 12, the rules are listed, and a refusal names each broken");
    }

    [Fact]
    public void ARuleRefusal_MarksNewPassword_WithItsMessage_NamingTheMessageAndTheRefusal_NotTheRules()
    {
        _sender.ResetRefusal = RuleRefusal;
        var cut = Render();

        cut.Find("#reset-password-input").Input("short");
        ResetButton(cut).Click();

        var refusal = cut.Find(".action-result .alert.alert-danger");
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

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is GetUserByIdQuery
                ? Task.FromResult((TResponse)(object)new UserDetailDto(
                    "user-9", "registrar@hospital.test", "Rene", "Registrar", 4, "Groote Schuur Hospital", [], [],
                    [WombatRoles.CommitteeMember], false, []))
                : throw new NotSupportedException(request.GetType().Name);

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => request is ResetUserPasswordCommand
                ? ResetRefusal is null ? Task.CompletedTask : throw new InvalidOperationException(ResetRefusal)
                : throw new NotSupportedException(request.GetType().Name);
    }
}
