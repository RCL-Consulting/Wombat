using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionsList;
using Wombat.Application.Features.Institutions.Queries.GetSpecialitiesList;
using Wombat.Application.Features.Institutions.Queries.GetSubSpecialitiesList;
using Wombat.Application.Features.Sso;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Sso;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T376 (flow 12, boards R3-O01–O10): the SSO mappings page. No provider for her institution is O01's card and nothing
/// else (Q11); with one, the form reads out Provider and Institution (M8) and asks Speciality and Sub-speciality only once
/// the role needs them (S4); a refusal of a posted role is beside Wombat role (O06); Delete… confirms in F8's words (T264);
/// a gone provider's mappings are read only for her (S5).
/// </summary>
public sealed class GroupMappingsPageTests : TestContext
{
    private const int Kgk = 2;
    private const string KgkName = "Kgosi Kgari Teaching Hospital";

    private readonly TestAuthorizationContext _auth;

    public GroupMappingsPageTests()
    {
        _auth = this.AddTestAuthorization();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── O01, O02: no provider ──────────────────────────────────────────────

    [Fact]
    public void O01_NoProviderForHerInstitution_IsTheNotSetUpCard_InHerTerms_WithNoFormAndNoList()
    {
        SignInAsHer();
        var cut = Render(new FakeSender(), providers: []);

        var card = cut.Find("section.detail-card.not-set-up");
        Text(card.QuerySelector("h2")!).Should().Be("Institutional sign-in is not set up");
        Text(card).Should().Contain($"Institutional sign-in is not set up for {KgkName}. A Wombat administrator sets it up;");
        Text(card).Should().Contain("Until then, your people sign in with an email and a password");
        card.HasAttribute("role").Should().BeFalse("a standing notice is not a live region (S23)");
        cut.FindAll("form").Should().BeEmpty();
        cut.FindAll("table").Should().BeEmpty();
        Text(cut.Find(".page-subtitle")).Should().Be(SsoMappingWords.Subtitle);
        cut.Markup.Should().NotContain("Sso:Providers", "the setting's name is the Administrator's");
    }

    [Fact]
    public void O02_TheAdministrator_WithNoProvider_IsShownTheSettingsName_AsBuilt()
    {
        SignInAsAdministrator();
        var cut = Render(new FakeSender(), providers: []);

        cut.Find("code").TextContent.Should().Be("Sso:Providers");
        cut.FindAll(".not-set-up").Should().BeEmpty();
        Text(cut.Find(".page-subtitle")).Should().Be(SsoMappingWords.AdministratorSubtitle);
    }

    // ─── O04, O04b: the form ────────────────────────────────────────────────

    [Fact]
    public void O04_TheForm_ReadsOutProviderAndInstitution_AndAsksNoScopeUntilARoleNeedsIt()
    {
        SignInAsHer();
        var cut = Render(new FakeSender());

        var form = cut.Find("form.form-container");
        Text(cut.Find("#sso-add-heading")).Should().Be("Add a mapping");
        form.GetAttribute("aria-labelledby").Should().Be("sso-add-heading");
        var readOuts = cut.FindAll("dl.form-group");
        Text(readOuts.First().QuerySelector("dd.readonly-value")!).Should().Be("KGK staff sign-in");
        Text(readOuts.First().QuerySelector("dd.field-help")!).Should().Be($"The one provider set up for {KgkName}.");
        Text(cut.Find("#mapping-institution")).Should().Be(KgkName);
        cut.FindAll("#mapping-speciality, #mapping-sub-speciality").Should().BeEmpty("S4: none until a role needs them");

        cut.FindAll("#mapping-role option").Select(Text).Should().Equal(
            "Choose a role", "Speciality admin", "Sub-speciality admin", "Coordinator", "Committee member", "Assessor");
        cut.Find("#mapping-role").GetAttribute("aria-describedby").Should().Be("mapping-role-help");
        cut.FindAll(".validation-summary-errors").Should().BeEmpty();
    }

    [Fact]
    public void TheAdministrator_IsOfferedInstitutionalAdmin()
    {
        SignInAsAdministrator();
        var cut = Render(new FakeSender());

        cut.FindAll("#mapping-role option").Select(option => option.GetAttribute("value")).Should().Contain(WombatRoles.InstitutionalAdmin);
    }

    [Fact]
    public void O04b_AssessorChosen_WithOneAdopted_ReadsOutSpecialityAndSubSpeciality()
    {
        SignInAsHer();
        var cut = Render(new FakeSender());

        cut.Find("#mapping-role").Change(WombatRoles.Assessor);

        Text(cut.Find("#mapping-speciality")).Should().Be("Paediatrics");
        Text(cut.Find("#mapping-sub-speciality")).Should().Be("Paediatrics");
        cut.Find("#mapping-sub-speciality").ClassList.Should().Contain("readonly-value");
        Text(cut.Find("#mapping-sub-speciality").ParentElement!.QuerySelector("dd.field-help")!)
            .Should().Be($"The only ones {KgkName} has adopted.");
    }

    [Fact]
    public void ACommitteeMember_MayBeKeptToASpeciality_OrLeftAtAny()
    {
        SignInAsHer();
        var cut = Render(new FakeSender());

        cut.Find("#mapping-role").Change(WombatRoles.CommitteeMember);

        cut.FindAll("#mapping-speciality option").Select(Text).Should().Equal("Any speciality", "Paediatrics");
        cut.FindAll("#mapping-sub-speciality").Should().BeEmpty();
    }

    // ─── O05: added ─────────────────────────────────────────────────────────

    [Fact]
    public void O05_Added_SendsTheProvidersInstitutionAndTheReadOutScope_AndTheResultSaysWhatTheGroupGets()
    {
        SignInAsHer();
        var sender = new FakeSender();
        var cut = Render(sender);

        cut.Find("#mapping-group-id").Change("kgk-consultants");
        cut.Find("#mapping-group-name").Change("KGK consultants");
        cut.Find("#mapping-role").Change(WombatRoles.Assessor);
        cut.Find("form").Submit();

        var command = sender.Creates.Should().ContainSingle().Subject;
        command.Should().BeEquivalentTo(new
        {
            ProviderKey = "kgk",
            ExternalGroupId = "kgk-consultants",
            ExternalGroupDisplayName = "KGK consultants",
            WombatRole = WombatRoles.Assessor,
            InstitutionId = Kgk,
            SpecialityId = (int?)7,
            SubSpecialityId = (int?)70
        }, options => options.ExcludingMissingMembers());

        var result = cut.Find(".action-result .alert-success");
        Text(result).Should().Be("Mapping added. People in KGK consultants get Assessor in Paediatrics / Paediatrics at their next sign-in.");
        Text(result.QuerySelector("strong")!).Should().Be("Mapping added.");
        cut.Find("#mapping-group-id").GetAttribute("value").Should().BeNullOrEmpty("the form is empty again");
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call => call.Identifier == "Blazor._internal.domWrapper.focus"));
    }

    [Fact]
    public void TheGroupAndTheRole_AreRequired_InWombatsWords_TheFirstFocused()
    {
        SignInAsHer();
        var sender = new FakeSender();
        var cut = Render(sender);

        cut.Find("form").Submit();

        sender.Creates.Should().BeEmpty();
        Text(cut.Find("#mapping-group-id-msg")).Should().Be(SsoMappingWords.GroupIdMissing);
        Text(cut.Find("#mapping-group-name-msg")).Should().Be(SsoMappingWords.GroupNameMissing);
        Text(cut.Find("#mapping-role-msg")).Should().Be(SsoMappingWords.RoleMissing);
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == PageFocus.FocusByIdIdentifier && Equals(call.Arguments[0], "mapping-group-id")));
    }

    // ─── O06: a posted role the select does not offer ───────────────────────

    [Fact]
    public void O06_ARefusalOfTheRole_IsBesideWombatRole_Focused_WithNoSummary()
    {
        SignInAsHer();
        var sender = new FakeSender
        {
            CreateFailure = new SsoMappingRefusedException(SsoMappingRefusal.InstitutionalAdminNotMappable, SsoMappingField.Role,
                CreateSsoGroupMappingCommandHandler.InstitutionalAdminNotMappable)
        };
        var cut = Render(sender);

        cut.Find("#mapping-group-id").Change("g");
        cut.Find("#mapping-group-name").Change("Group");
        cut.Find("#mapping-role").Change(WombatRoles.Coordinator);
        cut.Find("form").Submit();

        var role = cut.Find("#mapping-role");
        role.GetAttribute("aria-invalid").Should().Be("true");
        role.GetAttribute("aria-describedby").Should().Be("mapping-role-help mapping-role-msg");
        Text(cut.Find("#mapping-role-msg")).Should().Be("Only a Wombat administrator can map a group to Institutional admin.");
        cut.FindAll(".alert").Should().BeEmpty();
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == PageFocus.FocusByIdIdentifier && Equals(call.Arguments[0], "mapping-role")));
    }

    [Fact]
    public void AnUnexpectedFailure_IsFixedWords()
    {
        SignInAsHer();
        var cut = Render(new FakeSender { CreateFailure = new InvalidOperationException("Npgsql: deadlock") });

        cut.Find("#mapping-group-id").Change("g");
        cut.Find("#mapping-group-name").Change("Group");
        cut.Find("#mapping-role").Change(WombatRoles.Coordinator);
        cut.Find("form").Submit();

        Text(cut.Find(".action-result .alert-danger")).Should().Be(SsoMappingWords.ActionFailed);
        cut.Markup.Should().NotContain("Npgsql");
    }

    // ─── The list, O07, O08 ─────────────────────────────────────────────────

    [Fact]
    public void TheList_IsAStackWideTable_EachDeleteAnOutlineTriggerNamedForItsRow()
    {
        SignInAsHer();
        var cut = Render(new FakeSender { Mappings = [Live()] });

        Text(cut.Find("#sso-live-heading")).Should().Be("Current mappings 1");
        cut.Find("table").ClassList.Should().Contain("clinic-table--stack-wide");
        Text(cut.Find("table caption")).Should().Be($"At {KgkName}. Each applies at a person's next sign-in.");
        var header = cut.Find("tbody th[scope=row]");
        header.ClassList.Should().NotContain("col-wrap", "check Left 3: no .col-wrap on a row header");
        Text(header).Should().Be("KGK consultants kgk-consultants");
        Text(cut.Find("td[data-label='Wombat role and scope']")).Should().Be("Assessor Paediatrics / Paediatrics");
        var delete = cut.Find("tbody button");
        Text(delete).Should().Be("Delete…");
        delete.GetAttribute("aria-label").Should().Be("Delete the mapping of KGK consultants to Assessor");
        delete.ClassList.Should().Contain("btn-outline").And.NotContain("btn-danger", "the red button is the dialog's (S22)");
    }

    [Fact]
    public void O07_Delete_OpensF8sDialog_AndSendsNothingUntilConfirmed()
    {
        SignInAsHer();
        var sender = new FakeSender { Mappings = [Live()] };
        var cut = Render(sender);

        cut.Find("tbody button").Click();

        sender.Deletes.Should().BeEmpty();
        Text(cut.Find("dialog h2")).Should().Be("Delete this mapping?");
        cut.FindAll("dialog .dialog-body p").Select(Text).Should().Equal(
            "Delete the mapping of KGK consultants to Assessor (Paediatrics / Paediatrics)?",
            "From each person's next sign-in, belonging to KGK consultants no longer gives Assessor.",
            "The scope it gave stays.");
        cut.FindAll("dialog button").Select(Text).Should().Equal("Keep the mapping", "Delete the KGK consultants mapping");
        cut.FindAll("dialog button.btn-danger").Select(Text).Should().Equal("Delete the KGK consultants mapping");
    }

    [Fact]
    public void O08_Deleted_TheResultSaysWhatNoLongerHappens_AndTheListIsEmpty()
    {
        SignInAsHer();
        var sender = new FakeSender { Mappings = [Live()] };
        var cut = Render(sender);

        cut.Find("tbody button").Click();
        cut.FindAll("dialog button").Single(button => Text(button) == "Delete the KGK consultants mapping").Click();

        sender.Deletes.Should().ContainSingle().Which.Id.Should().Be(31);
        var result = cut.Find(".action-result .alert-success");
        Text(result).Should().Be("Mapping deleted. From each person's next sign-in, KGK consultants no longer gives Assessor.");
        Text(result.QuerySelector("strong")!).Should().Be("Mapping deleted.");
        Text(cut.Find("#sso-live-heading")).Should().Be("Current mappings 0");
        Text(cut.Find(".detail-card--empty")).Should().Be("No group gives a role yet. Use Add a mapping to give one.");
    }

    // ─── O03, S5: a gone provider's mappings ────────────────────────────────

    [Fact]
    public void O03_AGoneProvidersMappings_AreReadOnlyForHer_BesideTheNotSetUpCard()
    {
        SignInAsHer();
        var cut = Render(new FakeSender { Mappings = [Gone()] }, providers: []);

        cut.FindAll(".not-set-up").Should().ContainSingle();
        cut.FindAll("form").Should().BeEmpty();
        Text(cut.Find("#sso-gone-heading")).Should().Be("Mappings left from a removed provider 1");
        Text(cut.Find("table caption")).Should().Contain("Only a Wombat administrator can delete them.");
        cut.FindAll("tbody button").Should().BeEmpty("S5: Delete is the Administrator's alone");
        Text(cut.Find("tbody .actions-cell")).Should().Be("Read only: its provider is no longer set up.");
        Text(cut.Find("td[data-label=Provider]")).Should().Be("old-kgk");
    }

    [Fact]
    public void TheAdministrator_MayDeleteAGoneProvidersMapping()
    {
        SignInAsAdministrator();
        var cut = Render(new FakeSender { Mappings = [Gone()] });

        Text(cut.Find("#sso-gone-heading")).Should().Be("Mappings left from a removed provider 1");
        cut.Find("section[aria-labelledby=sso-gone-heading] tbody button").TextContent.Should().Contain("Delete…");
    }

    // ─── O09, O10 ───────────────────────────────────────────────────────────

    [Fact]
    public void O10_AFailedRead_IsFixedWords_WithTryAgain()
    {
        SignInAsHer();
        var cut = Render(new FakeSender { ReadFailure = new InvalidOperationException("Npgsql: refused") });

        Text(cut.Find(".alert-danger .alert-row-text")).Should().Be(
            "Could not load the SSO mappings. Nothing has changed. Try again, or come back in a few minutes.");
        cut.FindAll(".alert-danger button").Select(Text).Should().Equal("Try again");
        cut.Markup.Should().NotContain("Npgsql");
    }

    [Fact]
    public void P7_ATryAgainThatLoads_FocusesAddAMapping()
    {
        SignInAsHer();
        var sender = new FakeSender { ReadFailure = new InvalidOperationException("Npgsql: refused") };
        var cut = Render(sender);

        sender.ReadFailure = null;
        cut.Find(".alert-danger button").Click();

        cut.WaitForAssertion(() => cut.Find("#sso-add-heading").GetAttribute("tabindex").Should().Be("-1"));
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == "wombat.focusById" && (string)call.Arguments[0]! == "sso-add-heading"));
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private static SsoGroupMappingDto Live() => new(
        31, "kgk", "kgk-consultants", "KGK consultants", WombatRoles.Assessor, Kgk, KgkName,
        7, "Paediatrics", 70, "Paediatrics", "KGK staff sign-in");

    private static SsoGroupMappingDto Gone() => new(
        32, "old-kgk", "kgk-committee", "KGK committee", WombatRoles.CommitteeMember, Kgk, KgkName,
        7, "Paediatrics", null, null, "old-kgk", ProviderGone: true);

    private IRenderedComponent<GroupMappings> Render(FakeSender sender, IReadOnlyList<SsoProviderOptions>? providers = null)
    {
        Services.AddSingleton(Options.Create(new SsoOptions
        {
            Providers = providers?.ToList()
                ?? [new SsoProviderOptions { Key = "kgk", DisplayName = "KGK staff sign-in", InstitutionId = Kgk }]
        }));
        Services.AddSingleton<IScopedSender>(sender);
        return RenderComponent<GroupMappings>();
    }

    private void SignInAsHer()
    {
        _auth.SetAuthorized("mbatha@test");
        _auth.SetRoles(WombatRoles.InstitutionalAdmin);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"), new Claim(WombatClaimTypes.InstitutionId, Kgk.ToString()));
    }

    private void SignInAsAdministrator()
    {
        _auth.SetAuthorized("devadmin@test");
        _auth.SetRoles(WombatRoles.Administrator);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "devadmin"));
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender : IScopedSender
    {
        public List<SsoGroupMappingDto> Mappings { get; init; } = [];

        public List<CreateSsoGroupMappingCommand> Creates { get; } = [];

        public List<DeleteSsoGroupMappingCommand> Deletes { get; } = [];

        public Exception? ReadFailure { get; set; }

        public Exception? CreateFailure { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            object response;
            switch (request)
            {
                case GetInstitutionsListQuery when ReadFailure is not null:
                    return Task.FromException<TResponse>(ReadFailure);
                case GetInstitutionsListQuery:
                    response = new[] { new InstitutionDto(Kgk, KgkName, "KGK", null, true, DateTime.UtcNow) };
                    break;
                case GetSpecialitiesListQuery:
                    response = new[] { new SpecialityDto(7, 1, "Paediatrics", null, true) };
                    break;
                case GetSubSpecialitiesListQuery:
                    response = new[] { new SubSpecialityDto(70, 7, "Paediatrics", null, true, null) };
                    break;
                case ListSsoGroupMappingsQuery:
                    response = Mappings.ToArray();
                    break;
                case CreateSsoGroupMappingCommand command:
                    Creates.Add(command);
                    if (CreateFailure is not null)
                    {
                        return Task.FromException<TResponse>(CreateFailure);
                    }

                    response = 99;
                    break;
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }

            return Task.FromResult((TResponse)response);
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            if (request is DeleteSsoGroupMappingCommand delete)
            {
                Deletes.Add(delete);
                Mappings.RemoveAll(mapping => mapping.Id == delete.Id);
                return Task.CompletedTask;
            }

            throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }
}
