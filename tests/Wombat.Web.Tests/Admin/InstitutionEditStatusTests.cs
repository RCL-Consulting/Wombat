using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.DeactivateInstitution;
using Wombat.Application.Features.Institutions.Commands.ReactivateInstitution;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T302: the institution page offers the controls that change an institution's state only to those the Administrator
/// policy admits, the rule of the two commands behind them. An InstitutionalAdmin reads the state as text. T291 item 7:
/// her Back and Cancel lead home, not to the Administrator's institutions list. T264: Deactivate asks first.
/// </summary>
/// <remarks>
/// Before T302 every caller saw the Active box on an existing record, and the update sent it: Prof Mbatha, KGK's
/// InstitutionalAdmin, unticked it and saved KGK inactive, beside a Deactivate that refused her (the T295 replay, Steps
/// 1.23 and A.6.3). Back and Cancel sent her to Access denied.
/// </remarks>
public sealed class InstitutionEditStatusTests : TestContext
{
    private const int KgkId = 2;
    private const string AdministratorPolicy = "Administrator";

    /// <summary>What <see cref="Microsoft.AspNetCore.Components.ElementReference" />.FocusAsync calls.</summary>
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private readonly TestAuthorizationContext _auth;

    public InstitutionEditStatusTests()
    {
        _auth = this.AddTestAuthorization();
        JSInterop.SetupVoid("wombatDialog.showModal", _ => true).SetVoidResult();
        JSInterop.SetupVoid("wombatDialog.close", _ => true).SetVoidResult();
    }

    // ─── The InstitutionalAdmin ─────────────────────────────────────────────

    [Theory]
    [InlineData(true, "Active")]
    [InlineData(false, "Inactive")]
    public void AnInstitutionalAdmin_IsShownStatusAsText_WithNoActiveBox_AndNoDeactivateOrReactivate(bool isActive, string status)
    {
        SignInAsInstitutionalAdmin();
        var cut = RenderPage(new FakeSender(isActive));

        cut.FindAll("input[type=checkbox]").Should().BeEmpty("the state is the Administrator's to change");
        cut.FindAll("#institution-active").Should().BeEmpty();
        Text(cut.Find("#institution-status")).Should().Be(status);
        Text(cut.Find("#institution-status").Closest("dl")!.QuerySelector("dt")!).Should().Be("Status");
        PageButtons(cut).Select(Text).Should().Equal(["Save"], "neither Deactivate nor Reactivate is hers");
        cut.FindAll("dialog").Should().BeEmpty("she is offered nothing to confirm");
    }

    [Fact]
    public void AnInstitutionalAdmin_IsSentHome_ByBackAndCancel_NotToTheInstitutionsList()
    {
        SignInAsInstitutionalAdmin();
        var cut = RenderPage(new FakeSender());

        Links(cut).Should().Equal(("Back to home", "/"), ("Cancel", "/"));
    }

    [Fact]
    public void HerSave_SendsHerFieldsAndNoState()
    {
        SignInAsInstitutionalAdmin();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#institution-email").Change("hod.paediatrics@kgk.wombat.local");
        cut.Find("form").Submit();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateInstitutionCommand>()
            .Which.ContactEmail.Should().Be("hod.paediatrics@kgk.wombat.local");
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Institution saved.");
        Text(cut.Find("#institution-status")).Should().Be("Active");
        JSInterop.VerifyNotInvoke("wombatDialog.showModal");
    }

    // ─── The Administrator ──────────────────────────────────────────────────

    [Fact]
    public void AnAdministrator_IsShownTheActiveBox_AndDeactivate()
    {
        SignInAsAdministrator();
        var cut = RenderPage(new FakeSender());

        cut.Find("#institution-active").HasAttribute("checked").Should().BeTrue();
        cut.Find("#institution-active").GetAttribute("aria-describedby").Should().Be("institution-active-help");
        Text(cut.Find("#institution-active-help")).Should().Contain("cannot be named on a new invitation");
        cut.FindAll("#institution-status").Should().BeEmpty("the box says it");

        var deactivate = PageButtons(cut).Single(button => Text(button) == "Deactivate");
        deactivate.ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger",
            "the red button belongs only in the dialog's footer (DESIGN.md § Button system)");
        Links(cut).Should().Equal(("Back to institutions", "/admin/institutions"), ("Cancel", "/admin/institutions"));
    }

    [Fact]
    public void AnInactiveInstitution_IsNotOfferedDeactivate_AndIsReactivatedByTickingActiveAndSaving()
    {
        SignInAsAdministrator();
        var sender = new FakeSender(isActive: false);
        var cut = RenderPage(sender);

        PageButtons(cut).Select(Text).Should().Equal(["Save"], "an inactive institution has nothing to deactivate");
        cut.Find("#institution-active").HasAttribute("checked").Should().BeFalse();

        cut.Find("#institution-active").Change(true);
        cut.Find("form").Submit();

        JSInterop.VerifyNotInvoke("wombatDialog.showModal");
        sender.Commands.Select(command => command.GetType()).Should().Equal(
            typeof(UpdateInstitutionCommand), typeof(ReactivateInstitutionCommand));
        sender.Commands.OfType<ReactivateInstitutionCommand>().Single().Id.Should().Be(KgkId);
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Institution saved and reactivated.");
        PageButtons(cut).Select(Text).Should().Equal(["Deactivate", "Save"], "the institution is active again");
    }

    [Fact]
    public void Deactivate_OpensADialogNamingTheInstitution_AndSendsNothingUntilConfirmed()
    {
        SignInAsAdministrator();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        PageButton(cut, "Deactivate").Click();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty("opening the dialog must not deactivate the institution");
        Text(cut.Find("dialog h2")).Should().Be("Deactivate Kgosi Kgari Teaching Hospital?");
        Text(cut.Find("dialog p")).Should().Contain("No new invitation can be issued for it until it is reactivated.");
    }

    [Fact]
    public void ConfirmingDeactivate_DeactivatesTheInstitution_AndDeactivateIsNoLongerOffered()
    {
        SignInAsAdministrator();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        PageButton(cut, "Deactivate").Click();
        ConfirmButton(cut).Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<DeactivateInstitutionCommand>()
            .Which.Id.Should().Be(KgkId);
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Institution deactivated.");
        cut.Find("#institution-active").HasAttribute("checked").Should().BeFalse();
        PageButtons(cut).Select(Text).Should().Equal(["Save"], "the institution is inactive now");

        // Deactivate had the focus, and it is gone, so the result takes it, once the dialog has closed.
        cut.WaitForAssertion(() => FocusCalls().Should().ContainSingle());
        var calls = JSInterop.Invocations.Select(invocation => invocation.Identifier).ToList();
        calls.IndexOf("wombatDialog.close").Should().BeGreaterThan(-1)
            .And.BeLessThan(calls.IndexOf(FocusIdentifier), "while the modal is open nothing outside it can take the focus");
    }

    [Fact]
    public void UntickingActiveAndSaving_AsksFirst_ThenSavesAndDeactivates()
    {
        SignInAsAdministrator();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#institution-name").Change("Kgosi Kgari Hospital");
        cut.Find("#institution-active").Change(false);
        cut.Find("form").Submit();

        JSInterop.VerifyInvoke("wombatDialog.showModal");
        sender.Commands.Should().BeEmpty("unticking Active and saving deactivates the institution, so it asks first");
        Text(cut.Find("dialog h2")).Should().Be("Deactivate Kgosi Kgari Teaching Hospital?", "it names the record as stored");

        ConfirmButton(cut).Click();

        sender.Commands.Select(command => command.GetType()).Should().Equal(
            typeof(UpdateInstitutionCommand), typeof(DeactivateInstitutionCommand));
        sender.Commands.OfType<UpdateInstitutionCommand>().Single().Name.Should().Be("Kgosi Kgari Hospital");
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Institution saved and deactivated.");
        PageButtons(cut).Select(Text).Should().Equal(["Save"]);
    }

    [Fact]
    public void UntickingActiveAndSaving_ThenCancelling_SendsNothing()
    {
        SignInAsAdministrator();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#institution-active").Change(false);
        cut.Find("form").Submit();
        cut.FindAll("dialog button").Single(button => Text(button) == "Cancel").Click();

        sender.Commands.Should().BeEmpty();
        cut.FindAll(".alert.alert-success").Should().BeEmpty();
    }

    [Fact]
    public void SavingAnActiveInstitutionThatStaysActive_SendsOnlyTheUpdate()
    {
        SignInAsAdministrator();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("form").Submit();

        JSInterop.VerifyNotInvoke("wombatDialog.showModal");
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateInstitutionCommand>();
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Institution saved.");
    }

    // ─── Another Administrator changed the state after the page loaded ──────

    [Fact]
    public void AnUntouchedUntickedBox_OnAnInstitutionReactivatedElsewhere_SendsOnlyTheUpdate_AndShowsItActive()
    {
        // Loaded inactive; another Administrator reactivates it; this one edits the email and saves, box untouched.
        SignInAsAdministrator();
        var sender = new FakeSender(isActive: false) { StateAtSave = true };
        var cut = RenderPage(sender);

        cut.Find("#institution-email").Change("hod.paediatrics@kgk.wombat.local");
        cut.Find("form").Submit();

        JSInterop.VerifyNotInvoke("wombatDialog.showModal");
        sender.Commands.Should().ContainSingle("the box was not changed, so no state is sent, and no deactivation goes unasked")
            .Which.Should().BeOfType<UpdateInstitutionCommand>();
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Institution saved.");
        cut.Find("#institution-active").HasAttribute("checked").Should().BeTrue("the box shows the state as stored");
        PageButtons(cut).Select(Text).Should().Equal(["Deactivate", "Save"]);
    }

    [Fact]
    public void AnUntouchedTickedBox_OnAnInstitutionDeactivatedElsewhere_SendsOnlyTheUpdate_AndShowsItInactive()
    {
        SignInAsAdministrator();
        var sender = new FakeSender(isActive: true) { StateAtSave = false };
        var cut = RenderPage(sender);

        cut.Find("#institution-name").Change("Kgosi Kgari Hospital");
        cut.Find("form").Submit();

        sender.Commands.Should().ContainSingle("the box was not changed, so the other Administrator's deactivation stands")
            .Which.Should().BeOfType<UpdateInstitutionCommand>();
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be("Institution saved.");
        cut.Find("#institution-active").HasAttribute("checked").Should().BeFalse();
        PageButtons(cut).Select(Text).Should().Equal(["Save"]);
    }

    [Fact]
    public void AConfirmedUntick_OnAnInstitutionAlreadyDeactivatedElsewhere_SendsOnlyTheUpdate()
    {
        SignInAsAdministrator();
        var sender = new FakeSender(isActive: true) { StateAtSave = false };
        var cut = RenderPage(sender);

        cut.Find("#institution-active").Change(false);
        cut.Find("form").Submit();
        ConfirmButton(cut).Click();

        sender.Commands.Should().ContainSingle("it is already what she asked for").Which.Should().BeOfType<UpdateInstitutionCommand>();
        cut.Find("#institution-active").HasAttribute("checked").Should().BeFalse();
        PageButtons(cut).Select(Text).Should().Equal(["Save"]);
    }

    [Fact]
    public void ARefusedStateCommand_AfterALandedUpdate_SaysBothInOneAlert_AndLeavesTheBoxAsSet()
    {
        SignInAsAdministrator();
        var sender = new FakeSender { StateRefusal = new InvalidOperationException("Institution 2 was not found.") };
        var cut = RenderPage(sender);

        cut.Find("#institution-active").Change(false);
        cut.Find("form").Submit();
        ConfirmButton(cut).Click();

        sender.Commands.Select(command => command.GetType()).Should().Equal(
            typeof(UpdateInstitutionCommand), typeof(DeactivateInstitutionCommand));
        cut.FindAll(".alert.alert-success").Should().BeEmpty("a success beside a refusal says two things at once");
        Text(cut.Find(".action-result .alert.alert-danger")).Should().Be(
            "Institution saved, but not deactivated: Institution 2 was not found.");
        cut.Find("#institution-active").HasAttribute("checked").Should().BeFalse("the untick stays on the form, unsaved");
        PageButtons(cut).Select(Text).Should().Equal(["Deactivate", "Save"], "the institution is still active as stored");
    }

    // ─── The Administrator policy decides, not the role ─────────────────────

    [Fact]
    public void TheAdministratorPolicy_WithoutTheRole_IsShownTheStateControls()
    {
        _auth.SetAuthorized("policy-only@test");
        _auth.SetRoles(WombatRoles.InstitutionalAdmin);
        _auth.SetPolicies(AdministratorPolicy);
        var cut = RenderPage(new FakeSender());

        cut.FindAll("#institution-active").Should().ContainSingle();
        PageButtons(cut).Select(Text).Should().Equal(["Deactivate", "Save"]);
        Links(cut).Should().Equal(("Back to institutions", "/admin/institutions"), ("Cancel", "/admin/institutions"));
    }

    [Fact]
    public void TheAdministratorRole_WithoutThePolicy_IsShownStatusAsText()
    {
        _auth.SetAuthorized("role-only@test");
        _auth.SetRoles(WombatRoles.Administrator);
        var cut = RenderPage(new FakeSender());

        cut.FindAll("#institution-active").Should().BeEmpty("the page asks the policy (DESIGN.md § Table system, T211)");
        Text(cut.Find("#institution-status")).Should().Be("Active");
        PageButtons(cut).Select(Text).Should().Equal(["Save"]);
        Links(cut).Should().Equal(("Back to home", "/"), ("Cancel", "/"));
    }

    [Fact]
    public void ARefusedDeactivate_LeavesTheInstitutionActive_AndDeactivateOffered()
    {
        SignInAsAdministrator();
        var cut = RenderPage(new FakeSender { StateRefusal = new InvalidOperationException("Institution 2 was not found.") });

        PageButton(cut, "Deactivate").Click();
        ConfirmButton(cut).Click();

        Text(cut.Find(".action-result .alert.alert-danger")).Should().Be("Institution 2 was not found.");
        cut.Find("#institution-active").HasAttribute("checked").Should().BeTrue();
        PageButton(cut, "Deactivate").Should().NotBeNull("the dialog hands the focus back to it");
        FocusCalls().Should().BeEmpty("a refusal leaves the focus where it was");
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private void SignInAsInstitutionalAdmin()
    {
        _auth.SetAuthorized("mbatha@test");
        _auth.SetRoles(WombatRoles.InstitutionalAdmin);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"));
    }

    private void SignInAsAdministrator()
    {
        _auth.SetAuthorized("devadmin@test");
        _auth.SetRoles(WombatRoles.Administrator);
        _auth.SetPolicies(AdministratorPolicy);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "devadmin"));
    }

    private IRenderedComponent<InstitutionEdit> RenderPage(FakeSender sender)
    {
        Services.AddSingleton<IScopedSender>(sender);

        var cut = RenderComponent<InstitutionEdit>(parameters => parameters.Add(page => page.Id, KgkId));
        cut.WaitForState(() => cut.FindAll("#institution-name").Count > 0 && cut.FindAll(".header-container a").Count > 0);
        return cut;
    }

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    private static List<(string Text, string? Href)> Links(IRenderedFragment cut)
        => cut.FindAll("a").Select(link => (Text(link), link.GetAttribute("href"))).ToList();

    private static IElement PageButton(IRenderedFragment cut, string label) => PageButtons(cut).Single(button => Text(button) == label);

    private static IReadOnlyList<IElement> PageButtons(IRenderedFragment cut)
        => cut.FindAll("button").Where(button => button.Closest("dialog") is null).ToList();

    private static IElement ConfirmButton(IRenderedFragment cut)
        => cut.FindAll("dialog button").Single(button => Text(button) == "Deactivate");

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender(bool isActive = true) : IScopedSender
    {
        private InstitutionDto _institution = new(
            KgkId, "Kgosi Kgari Teaching Hospital", "KGK", "paeds-admin@kgk.wombat.local", isActive,
            new DateTime(2026, 9, 26, 8, 0, 0, DateTimeKind.Utc));

        /// <summary>Every command the page sent, in order. Queries are answered and not recorded.</summary>
        public List<object> Commands { get; } = [];

        /// <summary>When set, a deactivation or a reactivation is refused with it.</summary>
        public Exception? StateRefusal { get; init; }

        /// <summary>
        /// When set, the state another Administrator left the institution in after the page loaded: the update answers
        /// with it, as the handler answers with the record as stored.
        /// </summary>
        public bool? StateAtSave { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response = request switch
            {
                GetInstitutionByIdQuery query when query.Id == KgkId => _institution,
                UpdateInstitutionCommand command => Record(command, _institution = _institution with
                {
                    Name = command.Name,
                    ShortCode = command.ShortCode,
                    ContactEmail = command.ContactEmail,
                    IsActive = StateAtSave ?? _institution.IsActive
                }),
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Commands.Add(request);
            if (StateRefusal is not null)
            {
                return Task.FromException(StateRefusal);
            }

            _institution = request switch
            {
                DeactivateInstitutionCommand => _institution with { IsActive = false },
                ReactivateInstitutionCommand => _institution with { IsActive = true },
                _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
            };

            return Task.CompletedTask;
        }

        private T Record<T>(object command, T response)
        {
            Commands.Add(command);
            return response;
        }
    }
}
