using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
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
/// My account (T335, flow 01; T339, flow 02, R3-MA-*): three cards, Account, Your name and How you sign in. The name is
/// saved by the browser's post of the page's form to an endpoint, and an institutional sign-in removed by the post of a
/// ConfirmDialog's form; each endpoint issues the sign-in cookie again, and the page shows what it was sent back with. The
/// page saves nothing in its circuit.
/// </summary>
/// <remarks>
/// <para>
/// Until the t335 review the page renamed the account in its circuit. The shell names the person from the sign-in cookie's
/// <c>display_name</c> claim, which a circuit cannot write, so the account row went on showing the old name. The
/// integration suite's <c>Hosting/ProfileFlowTests</c> runs the posts end to end; these hold the page to its half.
/// </para>
/// <para>
/// Changed deliberately by T339 (flow 02): the words ("Save name", "Name saved.", the split name refusals); the email is
/// text in the Account card, no longer a disabled input; the result is focused once after the circuit's render, no longer
/// autofocused as well (C6, A16); a refusal of one name marks that field alone, empty, and the other keeps its name.
/// </para>
/// </remarks>
public sealed class ProfilePageTests : TestContext
{
    /// <summary>What <see cref="ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private const string Kgk = "Kgosi Kgari Teaching Hospital";

    private static readonly string[] FieldIds = ["profile-first-name", "profile-last-name"];

    private readonly StubSender _sender = new();

    public ProfilePageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton(Options.Create(new SsoOptions
        {
            Providers =
            [
                new SsoProviderOptions { Key = "kgk", DisplayName = Kgk, InstitutionId = 1 },
                new SsoProviderOptions { Key = "marula", DisplayName = "Marula Test Hospital", InstitutionId = 1 }
            ]
        }));
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("thandi@kgk.test");
        auth.SetRoles(WombatRoles.Trainee);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "thandi"));
    }

    // ---- Your name: the form ----

    [Fact]
    public void TheNameForm_IsPostedByTheBrowserToTheEndpoint_NotHandledInTheCircuit_WithTheNamesAsStored()
    {
        var cut = Render(ProfileOutcome.PagePath);

        var form = NameForm(cut);
        form.GetAttribute("method").Should().Be("post");
        form.GetAttribute("action").Should().Be(ProfileOutcome.SubmitPath).And.Be("/account/profile/submit");
        form.HasAttribute("data-submit-once").Should().BeTrue("a second press while the post is on its way sends nothing");
        form.HasAttribute("blazor:onsubmit").Should().BeFalse("the circuit must not take the submit: it cannot set a cookie");
        // bUnit renders the token empty, so it is counted as a component: every one on the page less the Remove dialog's
        // (ConfirmDialog's form carries its own). ProfileFlowTests posts the one the page gives.
        var tokens = cut.FindComponents<Microsoft.AspNetCore.Components.Forms.AntiforgeryToken>().Count;
        var dialogTokens = cut.FindComponent<ConfirmDialog>().FindComponents<Microsoft.AspNetCore.Components.Forms.AntiforgeryToken>().Count;
        (tokens - dialogTokens).Should().Be(1, "the name form's post carries its token, once");

        form.QuerySelectorAll("input[name]:not([type=hidden])")
            .Select(input => (input.Id, input.GetAttribute("name"), input.GetAttribute("value"), input.GetAttribute("autocomplete")))
            .Should().Equal(
                ("profile-first-name", "FirstName", "Thandi", "given-name"),
                ("profile-last-name", "LastName", "Mokoena", "family-name"));
        FieldIds.Should().OnlyContain(id => cut.Find($"#{id}").HasAttribute("required")
            && cut.Find($"#{id}").GetAttribute("maxlength") == "100");
        form.QuerySelectorAll("label span[aria-hidden]").Should().BeEmpty("every field here is required, so none is marked * (R3-Spec)");
        cut.FindAll("#profile-email").Should().BeEmpty("the email is text in the Account card, never a field");

        var submit = form.QuerySelector("button[type=submit]")!;
        submit.TextContent.Trim().Should().Be("Save name");
        submit.HasAttribute("blazor:onclick").Should().BeFalse();
        _sender.Sent.Should().AllBeOfType<GetCurrentUserProfileQuery>("the page renames no one and removes nothing");
    }

    [Fact]
    public void WithNothingToReport_ThePageShowsItsCards_AndMovesNoFocus()
    {
        var cut = Render(ProfileOutcome.PagePath);

        cut.Find(".page-subtitle").TextContent.Trim().Should().Be("Your name, your roles, and how you sign in.");
        cut.FindAll(".my-account-card > h2").Select(heading => heading.TextContent.Trim())
            .Should().Equal("Account", "Your name", "How you sign in");
        cut.FindAll(".alert").Should().BeEmpty();
        cut.FindAll("[autofocus]").Where(element => element.Closest("dialog") is null).Should().BeEmpty();
        FocusCalls().Should().BeEmpty();
        TabTitle.Of(this, cut).Should().Be("My account · Wombat");
    }

    [Fact]
    public void OnceTheEndpointHasSavedTheName_TheYourNameCardSaysSo_AboveItsActionRow_AndTheResultTakesTheFocusOnce()
    {
        var cut = Render(ProfileOutcome.SavedUrl);

        var done = NameForm(cut).QuerySelector(".action-result .alert.alert-success")!;
        done.TextContent.Trim().Should().Be("Name saved.");
        done.GetAttribute("role").Should().Be("status");
        done.Closest(".action-result")!.NextElementSibling!.ClassList.Should().Contain("form-actions", "above the action row (R3-MA-Saved)");
        cut.FindAll(".alert-danger").Should().BeEmpty();

        cut.FindAll(".action-result[autofocus]").Should().BeEmpty("focused once by the circuit, not announced twice (A16)");
        FocusCalls().Should().ContainSingle().Which.Arguments[0].Should().BeOfType<ElementReference>()
            .Which.Id.Should().Be(ResultHolding(cut, "Name saved.").Id);
    }

    [Theory]
    [InlineData("FirstNameMissing", "Your name was not saved. Enter your first name.", "profile-first-name", "Enter your first name.", "profile-last-name", "Mokoena")]
    [InlineData("LastNameMissing", "Your name was not saved. Enter your last name.", "profile-last-name", "Enter your last name.", "profile-first-name", "Thandi")]
    public void AOneNameRefusal_MarksThatField_Empty_NamingItsRefusal_AndTheOtherKeepsTheStoredName(
        string code, string expected, string refusedId, string fieldMessage, string keptId, string keptValue)
    {
        var cut = Render($"{ProfileOutcome.PagePath}?error={code}");

        var refusal = NameForm(cut).QuerySelector(".action-result .alert.alert-danger")!;
        refusal.TextContent.Trim().Should().Be(expected);
        refusal.GetAttribute("role").Should().Be("alert");

        var refused = cut.Find($"#{refusedId}");
        refused.GetAttribute("value").Should().BeNullOrEmpty("the refused field is empty (R3-MA-Refused)");
        refused.GetAttribute("aria-invalid").Should().Be("true");
        cut.Find($"#{refused.GetAttribute("aria-describedby")}").TextContent.Trim().Should().Be(fieldMessage);
        cut.Find($"#{refused.GetAttribute("aria-describedby")}").ClassList.Should().Contain("validation-message");

        var kept = cut.Find($"#{keptId}");
        kept.GetAttribute("value").Should().Be(keptValue);
        kept.HasAttribute("aria-invalid").Should().BeFalse();
        kept.HasAttribute("aria-describedby").Should().BeFalse();

        TabTitle.Of(this, cut).Should().Be("Error: My account · Wombat");
        FocusCalls().Should().ContainSingle().Which.Arguments[0].As<ElementReference>().Id.Should().Be(ResultHolding(cut, expected).Id);
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void BothNamesBlank_MarksBothFields()
    {
        var cut = Render($"{ProfileOutcome.PagePath}?error=NameMissing");

        cut.Find(".action-result .alert-danger").TextContent.Trim().Should().Be("Your name was not saved. Enter your first name and your last name.");
        FieldIds.Should().OnlyContain(id => cut.Find($"#{id}").GetAttribute("aria-invalid") == "true"
            && string.IsNullOrEmpty(cut.Find($"#{id}").GetAttribute("value")));
        cut.FindAll(".validation-message").Select(message => message.TextContent.Trim())
            .Should().Equal("Enter your first name.", "Enter your last name.");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Theory]
    [InlineData("?error=NameTooLong", "A first name or a last name can be at most 100 characters.")]
    [InlineData("?error=Failed", "Your name could not be saved. Try again.")]
    [InlineData("?error=ConcurrencyFailure", ProfileOutcome.GeneralRefusal)]
    public void ARefusalOfTheNameAsAWhole_IsNamedByBothFields_WhichKeepTheStoredName(string query, string expected)
    {
        var cut = Render(ProfileOutcome.PagePath + query);

        var refusal = cut.Find(".action-result .alert.alert-danger");
        refusal.TextContent.Trim().Should().Be(expected);
        refusal.GetAttribute("role").Should().Be("alert");
        refusal.Id.Should().Be(Profile.RefusalId);
        FieldIds.Should().OnlyContain(id => cut.Find($"#{id}").GetAttribute("aria-describedby") == Profile.RefusalId);
        cut.Find("#profile-first-name").GetAttribute("value").Should().Be("Thandi");
        TabTitle.Of(this, cut).Should().Be("Error: My account · Wombat", "after a refused post the title starts \"Error: \" (G88)");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void AnAddressCarryingWordsOfItsOwn_PutsNoneOfThemOnThePage()
    {
        var cut = Render(ProfileOutcome.PagePath + "?error=Your%20account%20is%20locked.%20Call%20012%20345%206789&provider=Call%20012");

        cut.Markup.Should().NotContain("Call 012", "a refusal travels as a code, and the page chooses the words");
        cut.Find(".alert.alert-danger").TextContent.Trim().Should().Be(ProfileOutcome.GeneralRefusal);
    }

    [Fact]
    public void ARefusalAndAStatusTogether_ShowTheRefusalOnly()
    {
        var cut = Render(ProfileOutcome.SavedUrl + "&error=NameTooLong");

        cut.FindAll(".alert").Should().ContainSingle().Which.ClassList.Should().Contain("alert-danger");
    }

    // ---- Account ----

    [Fact]
    public void TheAccountCard_ShowsTheEmailAsText_TheInstitution_AndTheRolesOnePerLineByLabel()
    {
        _sender.Profile = ProfileOf(roles: [WombatRoles.Assessor, WombatRoles.CommitteeMember]);

        var cut = Render(ProfileOutcome.PagePath);

        var list = cut.Find(".my-account-card dl.details-list.details-list--stacked");
        list.QuerySelectorAll("dt").Select(term => term.TextContent.Trim()).Should().Equal("Email", "Institution", "Roles");
        list.QuerySelectorAll("dd").ElementAt(0).TextContent.Trim().Should().Be("thandi@kgk.test");
        list.QuerySelectorAll("dd").ElementAt(1).TextContent.Trim().Should().Be(Kgk);
        list.QuerySelectorAll("dd li").Select(role => role.TextContent.Trim()).Should().Equal("Committee member", "Assessor");
    }

    [Fact]
    public void AnAccountWithNoInstitution_HasNoInstitutionRow_AtAll()
    {
        _sender.Profile = ProfileOf(institution: null, roles: [WombatRoles.Administrator]);

        var cut = Render(ProfileOutcome.PagePath);

        cut.FindAll("dt").Select(term => term.TextContent.Trim()).Should().Equal("Email", "Roles");
        cut.Markup.Should().NotContain("Institution<");
    }

    [Fact]
    public void APendingTrainee_IsLabelled_NotNamedByTheRolesKey()
    {
        _sender.Profile = ProfileOf(roles: [WombatRoles.PendingTrainee]);

        Render(ProfileOutcome.PagePath).Find(".my-account-roles li").TextContent.Trim().Should().Be("Pending trainee");
    }

    // ---- How you sign in ----

    [Fact]
    public void APasswordOnlyAccount_ShowsItsPassword_WithChangePassword_AndNoRemove()
    {
        var cut = Render(ProfileOutcome.PagePath);

        var rows = cut.FindAll(".signin-methods > li.signin-method");
        rows.Should().ContainSingle();
        rows.ElementAt(0).QuerySelector("strong")!.TextContent.Trim().Should().Be("Password");
        rows.ElementAt(0).QuerySelector(".signin-method-detail")!.TextContent.Trim().Should().Be("Your Wombat password.");
        rows.ElementAt(0).QuerySelector("a.btn")!.GetAttribute("href").Should().Be("/account/change-password");
        cut.FindAll("button").Select(button => button.TextContent.Trim()).Should().NotContain("Remove");
    }

    [Fact]
    public void ALinkedAccount_ShowsEachInstitutionalSignIn_ByName_WithALiveRemove()
    {
        _sender.Profile = ProfileOf(signIns: [new InstitutionalSignIn("kgk", Kgk)]);

        var cut = Render(ProfileOutcome.PagePath);

        var row = cut.FindAll(".signin-methods > li.signin-method").ElementAt(1);
        row.QuerySelector("strong")!.TextContent.Trim().Should().Be(Kgk);
        row.QuerySelector(".signin-method-detail")!.TextContent.Trim().Should().Be("Institutional sign-in.", "no link date (C1)");
        var remove = row.QuerySelector("button")!;
        remove.TextContent.Trim().Should().Be("Remove");
        remove.GetAttribute("aria-label").Should().Be($"Remove your {Kgk} sign-in");
        remove.HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void AnInstitutionOnlyAccount_SaysSo_OffersNoPasswordChange_AndItsLastWayInHasNoLiveRemove_ButItsReason()
    {
        _sender.Profile = ProfileOf(hasPassword: false, signIns: [new InstitutionalSignIn("kgk", Kgk)]);

        var cut = Render(ProfileOutcome.PagePath);

        var card = cut.FindAll(".my-account-card").ElementAt(2);
        card.QuerySelector("p")!.TextContent.Trim().Should().Be(
            "You sign in through your institution. This account has no Wombat password, so there is no password to change here.");
        card.TextContent.Should().NotContain("Change password");
        card.QuerySelectorAll("li.signin-method").Should().ContainSingle();
        var remove = card.QuerySelector("button")!;
        remove.HasAttribute("disabled").Should().BeTrue("the last way in's Remove is not a live action");
        cut.Find($"#{remove.GetAttribute("aria-describedby")}").TextContent.Trim().Should().Be("Remove: this is the only way you sign in.");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void Remove_OpensTheDialog_AFormPostedToTheEndpoint_NamingTheProvider_AndAskingForThePassword()
    {
        _sender.Profile = ProfileOf(signIns: [new InstitutionalSignIn("kgk", Kgk)]);
        var cut = Render(ProfileOutcome.PagePath);

        cut.Find($"button[aria-label='Remove your {Kgk} sign-in']").Click();

        JSInterop.Invocations.Should().Contain(invocation => invocation.Identifier == "wombatDialog.showModal");
        var dialog = cut.Find("dialog");
        var form = dialog.QuerySelector("form")!;
        form.GetAttribute("method").Should().Be("post");
        form.GetAttribute("action").Should().Be(SignInMethodEndpoints.RemovePath).And.Be("/account/external-logins/remove");
        form.QuerySelector("input[type=hidden][name=Provider]")!.GetAttribute("value").Should().Be("kgk");
        dialog.QuerySelector("h2")!.TextContent.Trim().Should().Be($"Remove your {Kgk} sign-in?");
        dialog.QuerySelector("p")!.TextContent.Trim().Should().Be(
            $"You will still sign in with your email and password. You can link it again the next time you sign in through {Kgk}.");

        var password = form.QuerySelector("input[name=Password]")!;
        password.GetAttribute("type").Should().Be("password");
        password.GetAttribute("autocomplete").Should().Be("current-password");
        password.HasAttribute("autofocus").Should().BeTrue("the password takes the focus as the dialog opens (R3-MA-Confirm)");
        cut.Find($"#{password.GetAttribute("aria-describedby")}").TextContent.Trim().Should().Be("Enter your password to confirm it is you.");
        PasswordToggleMarkup.ShouldBeTheToggle(cut, "remove-password", "Show password", PasswordToggleMarkup.Driven.ByCircuit);
        form.QuerySelector("button[type=submit]")!.TextContent.Trim().Should().Be("Remove sign-in");
    }

    // ---- Remove: the dialog opens on what it is about (the step F review) ----
    //
    // A press used to open the dialog in its own handler, before the render it had asked for, so showModal ran on the old
    // dialog: the first open said "Remove your  sign-in?", a second named the first sign-in, and after a refusal the
    // focus went to the refusal the render then removed. bUnit's markup is right either way, so these read the dialog as
    // it stands at the moment showModal is called.

    [Fact]
    public void Remove_OpensTheDialog_OnlyOnceItNamesTheSignIn()
    {
        _sender.Profile = ProfileOf(signIns: [new InstitutionalSignIn("kgk", Kgk)]);
        var opened = RecordDialogAtShowModal();
        var cut = Render(ProfileOutcome.PagePath);

        cut.Find($"button[aria-label='Remove your {Kgk} sign-in']").Click();

        opened.Should().ContainSingle().Which.Title.Should().Be($"Remove your {Kgk} sign-in?");
    }

    [Fact]
    public void RemoveForASecondSignIn_OpensTheDialogOnThatOne_NotTheFirst()
    {
        _sender.Profile = ProfileOf(signIns: [new InstitutionalSignIn("kgk", Kgk), new InstitutionalSignIn("marula", "Marula Test Hospital")]);
        var opened = RecordDialogAtShowModal();
        var cut = Render(ProfileOutcome.PagePath);

        cut.Find($"button[aria-label='Remove your {Kgk} sign-in']").Click();
        cut.Find("dialog").TriggerEvent("onclose", EventArgs.Empty);
        cut.Find("button[aria-label='Remove your Marula Test Hospital sign-in']").Click();

        opened.Select(dialog => dialog.Title).Should().Equal($"Remove your {Kgk} sign-in?", "Remove your Marula Test Hospital sign-in?");
    }

    [Fact]
    public void AfterARefusalReopenedTheDialog_CancelThenRemove_OpensItWithoutTheRefusal_ThePasswordTakingTheFocus()
    {
        _sender.Profile = ProfileOf(signIns: [new InstitutionalSignIn("kgk", Kgk)]);
        var opened = RecordDialogAtShowModal();
        var cut = Render(ProfileOutcome.RemoveRefusedUrl(ProfileOutcome.RemoveWrongPassword, "kgk"));
        opened.Should().ContainSingle("the refusal reopened it as the page was drawn");
        cut.Find("dialog .alert-danger").Should().NotBeNull();

        cut.Find("dialog").TriggerEvent("onclose", EventArgs.Empty);
        cut.Find($"button[aria-label='Remove your {Kgk} sign-in']").Click();

        opened.Should().HaveCount(2);
        opened[1].HasRefusal.Should().BeFalse("a stale refusal would take the focus, and the render would then remove it");
        opened[1].Autofocused.Should().Be("remove-password");
    }

    [Theory]
    [InlineData(false, "kgk", "You will still sign in through Marula Test Hospital. You can link it again the next time you sign in through Kgosi Kgari Teaching Hospital.")]
    [InlineData(true, "marula", "You will still sign in with your email and password, or through Kgosi Kgari Teaching Hospital. You can link it again the next time you sign in through Marula Test Hospital.")]
    public void TheDialog_SaysWhatRemains(bool hasPassword, string provider, string expected)
    {
        _sender.Profile = ProfileOf(hasPassword: hasPassword,
            signIns: [new InstitutionalSignIn("kgk", Kgk), new InstitutionalSignIn("marula", "Marula Test Hospital")]);
        var cut = Render(ProfileOutcome.PagePath);

        cut.Find($"button[aria-label^='Remove your'][aria-label*='{(provider == "kgk" ? "Kgosi" : "Marula")}']").Click();

        cut.Find("dialog p").TextContent.Trim().Should().Be(expected);
        cut.FindAll("dialog input[name=Password]").Should().HaveCount(hasPassword ? 1 : 0, "E3: a password only when the account has one");
    }

    [Fact]
    public void AWrongPassword_OpensTheDialogAgain_ItsRefusalFirstAndFocused_TheFieldEmptyAndMarked()
    {
        _sender.Profile = ProfileOf(signIns: [new InstitutionalSignIn("kgk", Kgk)]);

        var cut = Render(ProfileOutcome.RemoveRefusedUrl(ProfileOutcome.RemoveWrongPassword, "kgk"));

        JSInterop.Invocations.Should().ContainSingle(invocation => invocation.Identifier == "wombatDialog.showModal",
            "the page opens the dialog again as it is drawn (R3-MA-RemoveStates a)");
        var refusal = cut.Find("dialog .action-result[autofocus] .alert.alert-danger");
        refusal.TextContent.Trim().Should().Be("Your sign-in was not removed. Incorrect password.");
        var password = cut.Find("dialog input[name=Password]");
        password.GetAttribute("value").Should().BeNullOrEmpty();
        password.HasAttribute("autofocus").Should().BeFalse("the refusal takes the focus first");
        password.GetAttribute("aria-invalid").Should().Be("true");
        cut.Find($"#{password.GetAttribute("aria-describedby")}").TextContent.Trim().Should().Be("Incorrect password.");
        cut.FindAll(".my-account-card .alert").Should().BeEmpty("the refusal is inside the dialog, not in the card");
        TabTitle.Of(this, cut).Should().Be("Error: My account · Wombat");
        IdReferences.Broken(cut).Should().BeEmpty();
    }

    [Fact]
    public void ARemovedSignIn_IsSaid_InTheCard_ByTheInstitutionsConfiguredName_AndTakesTheFocus()
    {
        var cut = Render(ProfileOutcome.SignInRemovedUrl("kgk"));

        var card = cut.FindAll(".my-account-card").ElementAt(2);
        card.QuerySelector(".action-result .alert-success")!.TextContent.Trim().Should().Be($"{Kgk} sign-in removed.");
        FocusCalls().Should().ContainSingle().Which.Arguments[0].As<ElementReference>().Id
            .Should().Be(ResultHolding(cut, $"{Kgk} sign-in removed.").Id);
    }

    [Theory]
    [InlineData("RemoveLastSignIn", "Your Kgosi Kgari Teaching Hospital sign-in was not removed. It is the only way you sign in to Wombat.")]
    [InlineData("RemoveFailed", "Your Kgosi Kgari Teaching Hospital sign-in was not removed. Try again.")]
    public void ARemovalTheServerRefused_IsSaid_InTheCard(string code, string expected)
    {
        _sender.Profile = ProfileOf(hasPassword: false, signIns: [new InstitutionalSignIn("kgk", Kgk)]);

        var cut = Render(ProfileOutcome.RemoveRefusedUrl(code, "kgk"));

        cut.FindAll(".my-account-card").ElementAt(2).QuerySelector(".action-result .alert-danger")!.TextContent.Trim().Should().Be(expected);
        JSInterop.Invocations.Should().NotContain(invocation => invocation.Identifier == "wombatDialog.showModal");
        cut.Find("#profile-first-name").HasAttribute("aria-invalid").Should().BeFalse("the name was not refused");
        TabTitle.Of(this, cut).Should().Be("Error: My account · Wombat");
    }

    [Fact]
    public void AProviderThePageDoesNotKnow_IsNotNamed()
        => Render(ProfileOutcome.SignInRemovedUrl("unknown")).Find(".alert-success").TextContent.Trim()
            .Should().Be("Institutional sign-in removed.");

    // ---- Password updated, the load and its retry ----

    [Fact]
    public void PasswordUpdated_IsSaidUnderTheHeader_AndTakesTheFocus()
    {
        var cut = Render(ProfileOutcome.PasswordUpdatedUrl);

        var done = cut.Find(".my-account > .action-result .alert-success");
        done.TextContent.Trim().Should().Be("Password updated.");
        ProfileOutcome.PasswordUpdatedUrl.Should().Be("/account/profile?status=password-updated");
        FocusCalls().Should().ContainSingle().Which.Arguments[0].As<ElementReference>().Id.Should().Be(ResultHolding(cut, "Password updated.").Id);
    }

    [Fact]
    public void AFailedLoad_SaysSoInThePagesWords_AndTryAgainReadsAgain()
    {
        _sender.FailReads = 1;
        var cut = Render(ProfileOutcome.PagePath);

        var alert = cut.Find(".alert.alert-danger");
        alert.TextContent.Should().Contain("Could not load your account. Nothing has changed. Try again, or come back in a few minutes.");
        cut.FindAll(".my-account-card").Should().BeEmpty();

        alert.QuerySelector("button")!.Click();

        cut.FindAll(".my-account-card").Should().HaveCount(3);
        _sender.Sent.OfType<GetCurrentUserProfileQuery>().Should().HaveCount(2);
        FocusCalls().Should().ContainSingle("the content took the alert's place, so its first heading takes the focus")
            .Which.Arguments[0].As<ElementReference>().Id.Should().Be(ReferenceOf(cut.Find("#account-heading")));
    }

    [Fact]
    public void ARetryThatFailsAgain_FocusesTheAlert_DrawnAgain()
    {
        _sender.FailReads = 2;
        var cut = Render(ProfileOutcome.PagePath);

        cut.Find(".alert.alert-danger button").Click();

        cut.FindAll(".my-account-card").Should().BeEmpty();
        FocusCalls().Should().ContainSingle().Which.Arguments[0].As<ElementReference>().Id
            .Should().Be(cut.FindComponent<StatePanel>().FindComponent<ActionResult>().Instance.Element.Id);
    }

    [Theory]
    [InlineData("?status=saved", "Name saved.")]
    [InlineData("?status=password-updated", "Password updated.")]
    public void ARetryThatLoads_WithAResultToSay_FocusesTheResult_NotTheHeading(string query, string words)
    {
        // One owner for the focus (the step F review): the page's result, so it is read; StatePanel's move to the heading
        // would have taken the focus from it before it was announced.
        _sender.FailReads = 1;
        var cut = Render(ProfileOutcome.PagePath + query);

        cut.Find(".alert.alert-danger button").Click();

        FocusCalls().Should().ContainSingle().Which.Arguments[0].As<ElementReference>().Id.Should().Be(ResultHolding(cut, words).Id);
    }

    private IRenderedComponent<Profile> Render(string address)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        // PasswordField reads it (T339, flow 02): this page is interactive, so its toggles are the circuit's.
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        _rendered = RenderComponent<Profile>();
        return _rendered;
    }

    private static AngleSharp.Dom.IElement NameForm(IRenderedComponent<Profile> cut)
        => cut.Find($"form[action='{ProfileOutcome.SubmitPath}']");

    private static ElementReference ResultHolding(IRenderedComponent<Profile> cut, string words)
        => cut.FindComponents<ActionResult>().Single(result => result.Markup.Contains(words)).Instance.Element;

    /// <summary>
    /// What the dialog said, and which element held autofocus in it, each time the page called showModal: read from the
    /// renderer's markup at that moment, not after the render that follows.
    /// </summary>
    private List<(string Title, bool HasRefusal, string? Autofocused)> RecordDialogAtShowModal()
    {
        var opened = new List<(string Title, bool HasRefusal, string? Autofocused)>();
        Services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(new ShowModalRecorder(JSInterop.JSRuntime, () =>
        {
            // A dialog the page opens as it is first drawn is opened before RenderComponent returns: not read.
            if (_rendered is null)
            {
                opened.Add(("(first render)", false, null));
                return;
            }

            var dialog = _rendered.Find("dialog");
            opened.Add((dialog.QuerySelector("h2")!.TextContent.Trim(),
                dialog.QuerySelector(".alert-danger") is not null,
                dialog.QuerySelector("[autofocus]")?.Id));
        }));
        return opened;
    }

    // The page once RenderComponent has returned, for the recorder.
    private IRenderedComponent<Profile>? _rendered;

    private static string ReferenceOf(AngleSharp.Dom.IElement element) => element.GetAttribute("blazor:elementreference")!;

    private List<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    private static UserProfileDto ProfileOf(
        string? institution = Kgk,
        bool hasPassword = true,
        IReadOnlyList<InstitutionalSignIn>? signIns = null,
        IReadOnlyCollection<string>? roles = null)
        => new("thandi", "thandi@kgk.test", "Thandi", "Mokoena", roles ?? [WombatRoles.Trainee])
        {
            InstitutionName = institution,
            HasLocalPassword = hasPassword,
            InstitutionalSignIns = signIns ?? []
        };

    /// <summary>Calls <c>onShowModal</c> as the page calls <c>wombatDialog.showModal</c>, then bUnit's JS runtime.</summary>
    private sealed class ShowModalRecorder(Microsoft.JSInterop.IJSRuntime inner, Action onShowModal) : Microsoft.JSInterop.IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Record(identifier);
            return inner.InvokeAsync<TValue>(identifier, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Record(identifier);
            return inner.InvokeAsync<TValue>(identifier, cancellationToken, args);
        }

        private void Record(string identifier)
        {
            if (identifier == "wombatDialog.showModal")
            {
                onShowModal();
            }
        }
    }

    /// <summary>Answers the profile's read as stored, and records what the page sends.</summary>
    private sealed class StubSender : IScopedSender
    {
        public List<object> Sent { get; } = [];

        public UserProfileDto Profile { get; set; } = ProfilePageTests.ProfileOf();

        /// <summary>How many reads fail before one answers.</summary>
        public int FailReads { get; set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            if (request is not GetCurrentUserProfileQuery)
            {
                throw new InvalidOperationException($"The page sent {request.GetType().Name}, which it has no business sending.");
            }

            if (FailReads > 0)
            {
                FailReads--;
                throw new InvalidOperationException("The database is down.");
            }

            return Task.FromResult((TResponse)(object)Profile);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"The page sent {request.GetType().Name}.");
    }
}
