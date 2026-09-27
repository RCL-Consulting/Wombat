using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Accounts;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Components.Shared;
using Wombat.Web.Security;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Account;

/// <summary>
/// My account (T335, flow 01; the review of the t335 branch): the name is saved by the browser's post of the page's form
/// to an endpoint, which issues the sign-in cookie again, and the page shows what the endpoint sent it back with. It never
/// saves the name in its circuit.
/// </summary>
/// <remarks>
/// <para>
/// Until the review the page renamed the account in its circuit. The shell names the person from the sign-in cookie's
/// <c>display_name</c> claim, which a circuit cannot write, so the account row went on showing the old name, in this
/// circuit and in the cookie, until the stamp validator next rebuilt it. The integration suite's
/// <c>Hosting/ProfileFlowTests</c> runs the post end to end, to the account row's new name; these hold the page to its half,
/// as <c>ChangePasswordPageTests</c> holds change password's (T265).
/// </para>
/// </remarks>
public sealed class ProfilePageTests : TestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private static readonly string[] FieldIds = ["profile-first-name", "profile-last-name"];

    private readonly StubSender _sender = new();

    public ProfilePageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("thandi@kgk.test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "thandi"));
    }

    [Fact]
    public void TheForm_IsPostedByTheBrowserToTheEndpoint_NotHandledInTheCircuit_WithTheNamesAsStored()
    {
        var cut = Render(ProfileOutcome.PagePath);

        var form = cut.Find("form");
        form.GetAttribute("method").Should().Be("post");
        form.GetAttribute("action").Should().Be(ProfileOutcome.SubmitPath).And.Be("/account/profile/submit");
        form.HasAttribute("data-submit-once").Should().BeTrue("a second press while the post is on its way sends nothing");
        form.HasAttribute("blazor:onsubmit").Should().BeFalse("the circuit must not take the submit: it cannot set a cookie");
        cut.FindComponents<Microsoft.AspNetCore.Components.Forms.AntiforgeryToken>().Should().ContainSingle(
            "the post carries its token (bUnit renders it empty; ProfileFlowTests posts the one the page gives)");

        cut.FindAll("form input[name]:not([type=hidden])")
            .Select(input => (input.Id, input.GetAttribute("name"), input.GetAttribute("value"), input.GetAttribute("autocomplete")))
            .Should().Equal(
                ("profile-first-name", "FirstName", "Thandi", "given-name"),
                ("profile-last-name", "LastName", "Mokoena", "family-name"));
        FieldIds.Should().OnlyContain(id => cut.Find($"#{id}").HasAttribute("required")
            && cut.Find($"#{id}").GetAttribute("maxlength") == "100");
        cut.Find("#profile-email").GetAttribute("value").Should().Be("thandi@kgk.test");
        cut.Find("#profile-email").HasAttribute("name").Should().BeFalse("the email is shown, never posted");

        var submit = cut.Find("form button[type=submit]");
        submit.TextContent.Trim().Should().Be("Save profile");
        submit.HasAttribute("blazor:onclick").Should().BeFalse();
        _sender.Sent.OfType<UpdateCurrentUserProfileCommand>().Should().BeEmpty("the page renames no one");
    }

    [Fact]
    public void WithNothingToReport_ThePageShowsTheFormAlone_AndMovesNoFocus()
    {
        var cut = Render(ProfileOutcome.PagePath);

        cut.FindAll(".alert").Should().BeEmpty();
        cut.Find(".action-result").HasAttribute("autofocus").Should().BeFalse();
        FocusCalls().Should().BeEmpty();
        TabTitle.Of(this, cut).Should().Be("My account · Wombat");
    }

    [Fact]
    public void OnceTheEndpointHasSavedTheName_ThePageSaysSo_AndTheResultTakesTheFocus()
    {
        var cut = Render(ProfileOutcome.SavedUrl);

        var done = cut.Find(".action-result .alert.alert-success");
        done.TextContent.Trim().Should().Be("Profile saved.");
        done.GetAttribute("role").Should().Be("status");
        cut.FindAll(".alert-danger").Should().BeEmpty();

        cut.Find(".action-result").HasAttribute("autofocus").Should().BeTrue("the result arrives with the page");
        var region = cut.FindComponent<ActionResult>();
        FocusCalls().Should().ContainSingle("the circuit's render replaces the HTML the browser focused, so it focuses again")
            .Which.Arguments[0].Should().BeOfType<ElementReference>().Which.Id.Should().Be(region.Instance.Element.Id);
    }

    [Theory]
    [InlineData("?error=NameMissing", "Enter your first name and your last name.")]
    [InlineData("?error=NameTooLong", "A first name or a last name can be at most 100 characters.")]
    [InlineData("?error=Failed", ProfileOutcome.GeneralRefusal)]
    [InlineData("?error=ConcurrencyFailure", ProfileOutcome.GeneralRefusal)]
    public void ARefusal_IsShownInThePagesOwnWords_ReadAtOnce_AndNamedByEachField(string query, string expected)
    {
        var cut = Render(ProfileOutcome.PagePath + query);

        var refusal = cut.Find(".action-result .alert.alert-danger");
        refusal.TextContent.Trim().Should().Be(expected);
        refusal.GetAttribute("role").Should().Be("alert");
        refusal.Id.Should().Be(Profile.RefusalId);
        cut.FindAll(".alert-success").Should().BeEmpty();

        FieldIds.Should().OnlyContain(id => cut.Find($"#{id}").GetAttribute("aria-describedby") == Profile.RefusalId,
            "an alert already on the page when it loads is not reliably announced; each field reads it");
        cut.Find(".action-result").HasAttribute("autofocus").Should().BeTrue();
        TabTitle.Of(this, cut).Should().Be("Error: My account · Wombat", "after a refused post the title starts \"Error: \" (G88)");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AnAddressCarryingWordsOfItsOwn_PutsNoneOfThemOnThePage()
    {
        var cut = Render(ProfileOutcome.PagePath + "?error=Your%20account%20is%20locked.%20Call%20012%20345%206789");

        cut.Markup.Should().NotContain("Call 012", "a refusal travels as a code, and the page chooses the words");
        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be(ProfileOutcome.GeneralRefusal);
    }

    [Fact]
    public void ARefusalAndAStatusTogether_ShowTheRefusalOnly()
    {
        var cut = Render(ProfileOutcome.SavedUrl + "&error=NameMissing");

        cut.FindAll(".alert").Should().ContainSingle().Which.ClassList.Should().Contain("alert-danger");
    }

    private IRenderedComponent<Profile> Render(string address)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        return RenderComponent<Profile>();
    }

    private List<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    /// <summary>Answers the profile's read as stored, and records what the page sends.</summary>
    private sealed class StubSender : IScopedSender
    {
        public List<object> Sent { get; } = [];

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            return request is GetCurrentUserProfileQuery
                ? Task.FromResult((TResponse)(object)new UserProfileDto("thandi", "thandi@kgk.test", "Thandi", "Mokoena", [WombatRoles.Trainee]))
                : throw new InvalidOperationException($"The page sent {request.GetType().Name}, which it has no business sending.");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"The page sent {request.GetType().Name}.");
    }
}
