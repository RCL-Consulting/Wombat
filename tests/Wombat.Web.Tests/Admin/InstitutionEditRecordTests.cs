using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Institutions;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;
using Wombat.Application.Features.Institutions.Queries.GetInstitutionById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.Navigation;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// T376 (flow 12, boards R3-I01–I08): her institution's record. h1 and tab the institution's name, "Institution" before
/// the read and on a failed one (S9); Status read out (M8); a refusal under its field, marked, described and focused, with
/// no summary (S22); the result above the form names the contact email as saved; and S12, the create route the
/// Administrator's alone.
/// </summary>
public sealed class InstitutionEditRecordTests : TestContext
{
    private const int KgkId = 2;
    private const string KgkName = "Kgosi Kgari Teaching Hospital";

    private readonly TestAuthorizationContext _auth;

    public InstitutionEditRecordTests()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("mbatha@test");
        _auth.SetRoles(WombatRoles.InstitutionalAdmin);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "mbatha"));
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── I01: the record ────────────────────────────────────────────────────

    [Fact]
    public void I01_TheHeadingIsTheInstitutionsName_AndStatusIsAReadOut_OverWhoSetsIt()
    {
        var cut = RenderPage(new FakeSender());

        Text(cut.Find("h1")).Should().Be(KgkName);
        Text(cut.Find(".page-subtitle")).Should().Be("Your institution's name, short code and contact email.");
        cut.Find(".record-page form.form-container").GetAttribute("aria-label").Should().Be(KgkName);
        cut.Find(".record-page form").GetAttribute("novalidate").Should().NotBeNull();
        Text(cut.Find("form > p.field-help")).Should().Be("* marks a field you must fill in.");

        var status = cut.Find("dl.form-group");
        Text(status.QuerySelector("dt")!).Should().Be("Status");
        status.QuerySelector("dd.readonly-value")!.TextContent.Should().Be("Active");
        Text(status.QuerySelector("dd.field-help")!).Should().Be("Set by a Wombat administrator.");

        cut.Find("#institution-email").GetAttribute("type").Should().Be("email");
        cut.FindAll(".validation-summary-errors").Should().BeEmpty("S22: no summary");
        cut.FindAll("[style]").Should().BeEmpty("S18: no inline style");
    }

    [Fact]
    public void I05_BeforeTheRead_TheHeadingIsInstitution_AndTheLoadingLineSaysWhat()
    {
        var cut = RenderPage(new FakeSender { HoldRead = new TaskCompletionSource<InstitutionDto?>() }, waitForForm: false);

        Text(cut.Find("h1")).Should().Be("Institution");
        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading the institution.");
        cut.FindAll("form").Should().BeEmpty();
    }

    [Fact]
    public void I06_AFailedRead_IsFixedWords_WithTryAgain_AndTheHeadingStaysInstitution()
    {
        var cut = RenderPage(new FakeSender { ReadFailure = new InvalidOperationException("Npgsql: connection refused") }, waitForForm: false);

        cut.WaitForAssertion(() => cut.FindAll(".alert.alert-danger").Should().ContainSingle());
        Text(cut.Find("h1")).Should().Be("Institution");
        Text(cut.Find(".alert.alert-danger .alert-row-text")).Should().Be(
            "Could not load the institution. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql", "an exception's text is never shown (T329)");
        cut.FindAll(".alert-danger button").Select(Text).Should().Equal("Try again");
        cut.FindAll("form").Should().BeEmpty();
    }

    // ─── I02, I07: the result ───────────────────────────────────────────────

    [Fact]
    public void I02_Saved_TheResultNamesTheInstitutionInBold_AndTheAddressAsStored_AndTakesTheFocus()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#institution-email").Change("hod.paediatrics@kgk.wombat.local");
        cut.Find("form").Submit();

        var result = cut.Find(".record-page > .action-result .alert.alert-success");
        Text(result).Should().Be($"{KgkName} saved. Its contact email is now hod.paediatrics@kgk.wombat.local.");
        Text(result.QuerySelector("strong")!).Should().Be($"{KgkName} saved.", "only the first sentence is bold");
        result.InnerHtml.Should().Contain("@<wbr>", "an address breaks after its @ (flow 11's M7)");
        result.GetAttribute("role").Should().Be("status");
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call => call.Identifier == "Blazor._internal.domWrapper.focus"));
    }

    [Fact]
    public void I07_AClearedEmail_IsSaidSo()
    {
        var cut = RenderPage(new FakeSender());

        cut.Find("#institution-email").Change("");
        cut.Find("form").Submit();

        Text(cut.Find(".action-result .alert-success")).Should().Be($"{KgkName} saved. It has no contact email now.");
    }

    // ─── I03, I04, I08: refusals beside their field ─────────────────────────

    [Fact]
    public void I03_AnEmptiedShortCode_IsRefusedUnderIt_Focused_AndNothingIsSent()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#institution-short-code").Change("");
        cut.Find("form").Submit();

        sender.Updates.Should().BeEmpty();
        AssertRefusedUnder(cut, "institution-short-code", "Type a short code. The record was not saved.");
        cut.FindAll(".alert").Should().BeEmpty("the refusal is beside its field, with no summary (S22)");
    }

    [Fact]
    public void I04_AMalformedEmail_IsRefusedUnderIt_AndWhatWasTypedIsKept()
    {
        var sender = new FakeSender();
        var cut = RenderPage(sender);

        cut.Find("#institution-email").Change("paeds-admin.kgk.wombat.local");
        cut.Find("form").Submit();

        sender.Updates.Should().BeEmpty();
        AssertRefusedUnder(cut, "institution-email", "Enter a valid email address, for example name@example.org.");
        cut.Find("#institution-email").GetAttribute("value").Should().Be("paeds-admin.kgk.wombat.local");
    }

    [Fact]
    public void ARefusedField_IsCleared_OnceItIsChanged()
    {
        var cut = RenderPage(new FakeSender());

        cut.Find("#institution-short-code").Change("");
        cut.Find("form").Submit();
        cut.Find("#institution-short-code").Change("KGK");

        cut.FindAll(".validation-message").Should().BeEmpty();
        cut.Find("#institution-short-code").HasAttribute("aria-invalid").Should().BeFalse();
    }

    [Theory]
    [InlineData(InstitutionRecordField.ShortCode, "institution-short-code")]
    [InlineData(InstitutionRecordField.Name, "institution-name")]
    public void I08_ANameOrShortCodeAnotherInstitutionHolds_IsRefusedOnItsField_ByTheTypedField(InstitutionRecordField field, string fieldId)
    {
        var sender = new FakeSender
        {
            UpdateFailure = new InstitutionUpdateRefusedException(
                InstitutionUpdateRefusal.Duplicate, field, UpdateInstitutionCommandHandler.Duplicate)
        };
        var cut = RenderPage(sender);

        cut.Find("#institution-short-code").Change("DEMO");
        cut.Find("form").Submit();

        AssertRefusedUnder(cut, fieldId, "An institution with the same name or short code already exists.");
        cut.FindAll(".alert").Should().BeEmpty();
    }

    [Fact]
    public void AnUnexpectedFailure_IsFixedWords_NeverTheExceptions()
    {
        var cut = RenderPage(new FakeSender { UpdateFailure = new InvalidOperationException("Npgsql: deadlock detected") });

        cut.Find("form").Submit();

        Text(cut.Find(".action-result .alert-danger")).Should().Be(
            "Could not save the institution. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain("Npgsql");
    }

    // ─── S12: the create route is the Administrator's alone ─────────────────

    [Fact]
    public async Task S12_TheCreateRoute_IsAPageOfItsOwn_ThatAdmitsTheAdministrator_AndNotHer()
    {
        PageAccess.PageFor("/admin/institutions/new").Should().Be(typeof(InstitutionCreate));
        PageAccess.PageFor("/admin/institutions/2").Should().Be(typeof(InstitutionEdit));
        typeof(InstitutionEdit).GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>()
            .Select(route => route.Template).Should().Equal("/admin/institutions/{Id:int}");

        (await PageAccess.RefusalOf(typeof(InstitutionCreate), WombatRoles.InstitutionalAdmin)).Should().NotBeNull(
            "her /admin/institutions/new is Access denied, not a form whose save she is then refused");
        (await PageAccess.RefusalOf(typeof(InstitutionCreate), WombatRoles.Administrator)).Should().BeNull();
        (await PageAccess.RefusalOf(typeof(InstitutionEdit), WombatRoles.InstitutionalAdmin)).Should().BeNull("her record stays hers");
        typeof(InstitutionCreate).GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>()
            .Single().Policy.Should().Be("Administrator");
    }

    // ─── Helpers ────────────────────────────────────────────────────────────

    private void AssertRefusedUnder(IRenderedComponent<InstitutionEdit> cut, string fieldId, string words)
    {
        var input = cut.Find($"#{fieldId}");
        input.GetAttribute("aria-invalid").Should().Be("true");
        input.ClassList.Should().Contain("input-validation-error");
        var messageId = input.GetAttribute("aria-describedby");
        messageId.Should().NotBeNullOrEmpty();
        Text(cut.Find($"#{messageId}")).Should().Be(words);
        cut.Find($"#{messageId}").ClassList.Should().Contain("validation-message");
        input.ParentElement!.QuerySelector(".validation-message").Should().NotBeNull("the refusal is under its own field");
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == PageFocus.FocusByIdIdentifier && Equals(call.Arguments[0], fieldId)));
    }

    private IRenderedComponent<InstitutionEdit> RenderPage(FakeSender sender, bool waitForForm = true)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<InstitutionEdit>(parameters => parameters.Add(page => page.Id, KgkId));
        if (waitForForm)
        {
            cut.WaitForState(() => cut.FindAll("#institution-name").Count > 0);
        }

        return cut;
    }

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FakeSender : IScopedSender
    {
        private InstitutionDto _institution = new(
            KgkId, KgkName, "KGK", "paeds-admin@kgk.wombat.local", true, new DateTime(2026, 9, 26, 8, 0, 0, DateTimeKind.Utc));

        public List<UpdateInstitutionCommand> Updates { get; } = [];

        public Exception? ReadFailure { get; init; }

        public Exception? UpdateFailure { get; init; }

        public TaskCompletionSource<InstitutionDto?>? HoldRead { get; init; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            switch (request)
            {
                case GetInstitutionByIdQuery when HoldRead is not null:
                    return HoldRead.Task.ContinueWith(task => (TResponse)(object)task.Result!, cancellationToken);
                case GetInstitutionByIdQuery when ReadFailure is not null:
                    return Task.FromException<TResponse>(ReadFailure);
                case GetInstitutionByIdQuery:
                    return Task.FromResult((TResponse)(object)_institution);
                case UpdateInstitutionCommand command:
                    Updates.Add(command);
                    if (UpdateFailure is not null)
                    {
                        return Task.FromException<TResponse>(UpdateFailure);
                    }

                    _institution = _institution with
                    {
                        Name = command.Name.Trim(),
                        ShortCode = command.ShortCode.Trim(),
                        ContactEmail = string.IsNullOrWhiteSpace(command.ContactEmail) ? null : command.ContactEmail.Trim()
                    };
                    return Task.FromResult((TResponse)(object)_institution);
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
    }
}
