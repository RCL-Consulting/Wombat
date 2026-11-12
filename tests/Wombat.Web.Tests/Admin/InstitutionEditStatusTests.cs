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
/// policy admits, the rule of the two commands behind them. T400 (flow 18, R3-I11–I16): the Administrator's are a Status
/// section's Deactivate… and Reactivate…, each asking in ConfirmDialog's action mode (M1); there is no Active box. An InstitutionalAdmin reads the state as text. T291 item 7:
/// her Cancel leads home, not to the Administrator's institutions list, and T376 (flow 12, I01) gives her no Back link:
/// the trail's Home is her way back. T264: Deactivate asks first.
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
        PageButtons(cut).Select(Text).Should().Equal(["Save changes"], "neither Deactivate nor Reactivate is hers");
        cut.FindAll("dialog").Should().BeEmpty("she is offered nothing to confirm");
    }

    [Fact]
    public void AnInstitutionalAdmin_IsSentHome_ByCancel_NotToTheInstitutionsList_AndHasNoBackLink()
    {
        SignInAsInstitutionalAdmin();
        var cut = RenderPage(new FakeSender());

        Links(cut).Should().Equal(("Cancel", "/"));
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
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be(
            "Kgosi Kgari Teaching Hospital saved. Its contact email is now hod.paediatrics@kgk.wombat.local.");
        Text(cut.Find("#institution-status")).Should().Be("Active");
        JSInterop.VerifyNotInvoke("wombatDialog.showModal");
    }

    // ─── The Administrator (T400, flow 18, lane A; R3-I11–I16, M1, T11, K17) ───

    [Fact]
    public void AnAdministrator_IsShownAStatusSection_WithDeactivate_AndNoActiveBox()
    {
        SignInAsAdministrator();
        var cut = RenderPage(new FakeSender());

        cut.FindAll("input[type=checkbox]").Should().BeEmpty("T400: there is no Active box; Save never asks");
        cut.FindAll("#institution-status").Should().BeEmpty("the Status section says it");
        var section = cut.Find("section.user-section");
        section.GetAttribute("aria-labelledby").Should().Be("institution-status-heading");
        Text(section.QuerySelector("h2")!).Should().Be("Status");
        Text(section.QuerySelector("p")!).Should().Be("Active A new invitation can name it.");
        section.QuerySelector("p .badge")!.ClassList.Should().Contain("badge-completed");

        var deactivate = PageButton(cut, "Deactivate…");
        deactivate.ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger",
            "the red button belongs only in the dialog's footer (DESIGN.md § Button system)");
        deactivate.GetAttribute("aria-haspopup").Should().Be("dialog");
        deactivate.Closest("form").Should().BeNull("the trigger is the Status section's, not the form's");
        PageButtons(cut).Select(Text).Should().Equal("Save", "Deactivate…");
        Links(cut).Should().Equal(("Cancel", "/admin/institutions"));
        Text(cut.Find("h1")).Should().Be("Kgosi Kgari Teaching Hospital");
        Text(cut.Find(".page-subtitle")).Should().Be("KGK · Active");
    }

    [Fact]
    public void AnInactiveInstitution_OffersReactivate_NeverDeactivate()
    {
        SignInAsAdministrator();
        var cut = RenderPage(new FakeSender(isActive: false));

        PageButtons(cut).Select(Text).Should().Equal("Save", "Reactivate…");
        Text(cut.Find("section.user-section p")).Should().Be(
            "Inactive No new invitation can name it. Its people, their records and the invitations already sent are kept.");
        cut.Find("section.user-section p .badge").ClassList.Should().Contain("badge-withdrawn");
        Text(cut.Find(".page-subtitle")).Should().Be("KGK · Inactive");
    }

    /// <summary>M1: both dialogs are ConfirmDialog's action mode, Cancel first and autofocused, the action after it.</summary>
    [Theory]
    [InlineData(true, "Deactivate Kgosi Kgari Teaching Hospital?", "Deactivate", "btn-danger",
        "No new invitation can be issued for it until it is reactivated. Its people, their records and the invitations already sent are kept. You can reactivate it on this page.")]
    [InlineData(false, "Reactivate Kgosi Kgari Teaching Hospital?", "Reactivate", "btn-primary",
        "A new invitation can name it again. Its people and their records are as they were.")]
    public void EachTrigger_AsksInTheActionMode_OpeningOnCancel_AndSendsNothingUntilConfirmed(
        bool isActive, string title, string confirm, string confirmClass, string body)
    {
        SignInAsAdministrator();
        var sender = new FakeSender(isActive);
        var cut = RenderPage(sender);

        PageButton(cut, isActive ? "Deactivate…" : "Reactivate…").Click();

        var dialog = cut.FindAll("dialog").Single(candidate => Text(candidate.QuerySelector("h2")!) == title);
        dialog.QuerySelector("form").Should().BeNull("a dialog that does not post is the action mode (M1)");
        dialog.QuerySelector(".form-container > h2").Should().NotBeNull();
        Text(dialog.QuerySelector(".form-container > p")!).Should().Be(body);
        var buttons = dialog.QuerySelectorAll(".form-actions button").ToList();
        buttons.Select(Text).Should().Equal("Cancel", confirm);
        buttons[0].HasAttribute("autofocus").Should().BeTrue("it opens on Cancel");
        buttons[1].ClassList.Should().Contain(confirmClass);
        JSInterop.VerifyInvoke("wombatDialog.showModal").Arguments[1].Should().Be(
            isActive ? "institution-deactivate" : "institution-reactivate", "Cancel and Escape return to the trigger");
        sender.Commands.Should().BeEmpty();
    }

    [Fact]
    public void ConfirmingDeactivate_SendsItsOwnCommand_SaysSoByName_AndOffersReactivate()
    {
        SignInAsAdministrator();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        PageButton(cut, "Deactivate…").Click();
        ConfirmButton(cut, "Deactivate").Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<DeactivateInstitutionCommand>().Which.Id.Should().Be(KgkId);
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be(
            "Kgosi Kgari Teaching Hospital deactivated. A new invitation can no longer name it.");
        PageButtons(cut).Select(Text).Should().Equal("Save", "Reactivate…");
        Text(cut.Find(".page-subtitle")).Should().Be("KGK · Inactive");
        cut.WaitForAssertion(() => FocusCalls().Should().NotBeEmpty("the result takes the focus once the dialog has closed"));
    }

    [Fact]
    public void ConfirmingReactivate_SendsItsOwnCommand_AndSaysSoByName()
    {
        SignInAsAdministrator();
        var sender = new FakeSender(isActive: false);
        var cut = RenderPage(sender);

        PageButton(cut, "Reactivate…").Click();
        ConfirmButton(cut, "Reactivate").Click();

        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<ReactivateInstitutionCommand>();
        Text(cut.Find(".action-result .alert.alert-success")).Should().Be(
            "Kgosi Kgari Teaching Hospital reactivated. A new invitation can name it again.");
        PageButtons(cut).Select(Text).Should().Equal("Save", "Deactivate…");
    }

    /// <summary>K17: a status change leaves typing not yet saved in the fields, unsaved.</summary>
    [Fact]
    public void AStatusChange_KeepsUnsavedTyping_InTheFields()
    {
        SignInAsAdministrator();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#institution-email").Change("typed-not-saved@kgk.wombat.local");
        PageButton(cut, "Deactivate…").Click();
        ConfirmButton(cut, "Deactivate").Click();

        cut.Find("#institution-email").GetAttribute("value").Should().Be("typed-not-saved@kgk.wombat.local");
        sender.Commands.Should().NotContain(command => command is UpdateInstitutionCommand);
    }

    [Fact]
    public void Save_NeverAsks_AndSaysTheContactEmailByName()
    {
        SignInAsAdministrator();
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#institution-email").Change("paediatrics@kgk.wombat.local");
        cut.Find("form").Submit();

        JSInterop.VerifyNotInvoke("wombatDialog.showModal");
        sender.Commands.Should().ContainSingle().Which.Should().BeOfType<UpdateInstitutionCommand>();
        var result = cut.Find(".action-result .alert.alert-success");
        Text(result).Should().Be(
            "Kgosi Kgari Teaching Hospital saved. Its contact email is now paediatrics@kgk.wombat.local.");
        result.QuerySelector("strong").Should().BeNull("R3-I12 bolds nothing in a success");
    }

    [Fact]
    public void AFailedStatusChange_IsT23sWords_NeverTheExceptions_AndTheStateIsAsItWas()
    {
        SignInAsAdministrator();
        var cut = RenderPage(new FakeSender { StateRefusal = new InvalidOperationException("Npgsql: the database is down") });

        PageButton(cut, "Deactivate…").Click();
        ConfirmButton(cut, "Deactivate").Click();

        var failure = cut.Find(".action-result .alert.alert-danger");
        Text(failure).Should().Be(
            "Could not deactivate the institution. Nothing has changed. Try again, or come back in a few minutes.");
        Text(failure.QuerySelector("strong")!).Should().Be("Could not deactivate the institution.");
        cut.Markup.Should().NotContain("Npgsql");
        PageButtons(cut).Select(Text).Should().Equal("Save", "Deactivate…");
    }

    [Fact]
    public void TheAdministratorPolicy_WithoutTheRole_IsShownTheStatusSection()
    {
        _auth.SetAuthorized("devadmin@test");
        _auth.SetPolicies(AdministratorPolicy);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "devadmin"));
        var cut = RenderPage(new FakeSender());

        cut.FindAll("section.user-section").Should().ContainSingle();
    }

    [Fact]
    public void TheAdministratorRole_WithoutThePolicy_IsShownStatusAsText()
    {
        _auth.SetAuthorized("devadmin@test");
        _auth.SetRoles(WombatRoles.Administrator);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "devadmin"));
        var cut = RenderPage(new FakeSender());

        cut.FindAll("section.user-section").Should().BeEmpty();
        Text(cut.Find("#institution-status")).Should().Be("Active");
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
        cut.WaitForState(() => cut.FindAll("#institution-name").Count > 0);
        return cut;
    }

    private IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    private static List<(string Text, string? Href)> Links(IRenderedFragment cut)
        => cut.FindAll("a").Select(link => (Text(link), link.GetAttribute("href"))).ToList();

    private static IElement PageButton(IRenderedFragment cut, string label) => PageButtons(cut).Single(button => Text(button) == label);

    private static IReadOnlyList<IElement> PageButtons(IRenderedFragment cut)
        => cut.FindAll("button").Where(button => button.Closest("dialog") is null).ToList();

    private static IElement ConfirmButton(IRenderedFragment cut, string label)
        => cut.FindAll("dialog button").Single(button => Text(button) == label);

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
