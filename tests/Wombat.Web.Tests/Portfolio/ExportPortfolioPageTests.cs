using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Features.Reporting;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;
using static Wombat.Web.Tests.Navigation.PageAccess;

namespace Wombat.Web.Tests.Portfolio;

/// <summary>
/// Export portfolio, own and staff (T379, flow 13, lane E; R3-E01–E20): the subject read before the form, the dates' defaults
/// on the South African calendar, the result above the form naming the file, the refusals under their field, the fixed
/// failure words, the one Unavailable page, and the staff trail through the owner the menu lights (M4).
/// </summary>
public sealed partial class ExportPortfolioPageTests : WombatTestContext
{
    private const string FocusIdentifier = "Blazor._internal.domWrapper.focus";
    private const string Secret = "Npgsql: the connection string was rejected.";

    // 2026-10-25 22:30 UTC is already 2026-10-26 in South Africa (T325): To opens on the 26th.
    private static readonly DateTimeOffset Now = new(2026, 10, 25, 22, 30, 0, TimeSpan.Zero);

    private readonly ScriptedSender _sender = new();
    private readonly TestAuthorizationContext _auth;

    public ExportPortfolioPageTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<TimeProvider>(new FixedClock(Now));
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("molefe@kgk.wombat.local");
        _auth.SetRoles(WombatRoles.Trainee);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "molefe"));
    }

    // ─── Her own (R3-E01, E07) ───────────────────────────────────────────────

    [Fact]
    public void HerOwn_OpensOnHerProgrammeStartAndTheSouthAfricanToday_WithNoTrail()
    {
        var cut = RenderOwn(Molefe());

        _sender.Sent.OfType<GetExportSubjectQuery>().Single().TraineeUserId.Should().BeNull("her own export reads her own");
        Text(cut.Find("h1")).Should().Be("Export portfolio");
        TabTitle.Of(this, cut).Should().Be("Export portfolio · Wombat");
        Text(cut.Find(".page-subtitle")).Should().Be(
            "A PDF of your portfolio for the dates you choose: your progress, your STARs, your committee's decisions and your activities.");
        cut.FindAll("nav[aria-label='Breadcrumb']").Should().BeEmpty("a page the menu offers draws no trail (F4)");

        Field(cut, "export-from").Should().Be(("From date *required", "2023-01-15", "Your programme started on 2023-01-15."));
        Field(cut, "export-to").Should().Be(("To date *required", "2026-10-26", "Today, on the South African calendar."));
        Text(cut.Find("form.form-container > p.export-note")).Should().Be(
            "Exporting the same dates again gives the same file, until the record changes.");
        cut.Find(".export-page > form.form-container").Should().NotBeNull("A2: the form is a child of .export-page");
        Text(cut.Find(".form-actions button[type=submit].btn-primary")).Should().Be("Export PDF");
        cut.Find("#export-from").GetAttribute("aria-describedby").Should().Be("export-from-help");
    }

    [Fact]
    public void HerOwn_AfterTheEnd_ClosesOnHerLastDay()
    {
        var cut = RenderOwn(Molefe() with { LastDay = new DateOnly(2026, 10, 26) });

        Field(cut, "export-to").Should().Be(("To date *required", "2026-10-26", "Your last day in the programme."));
    }

    // ─── Export PDF (R3-E02, E03) ────────────────────────────────────────────

    [Fact]
    public void ExportPdf_SendsTheDates_DownloadsTheFile_AndSaysSoAboveTheForm_Focused()
    {
        var cut = RenderOwn(Molefe());
        _sender.On<ExportPortfolioCommand>(_ => new PortfolioExportResult([1, 2, 3], "portfolio-aba6ce7943c2.pdf", "hash"));

        cut.Find("form.form-container").Submit();

        var command = _sender.Sent.OfType<ExportPortfolioCommand>().Single();
        (command.TraineeUserId, command.FromDate, command.ToDate).Should().Be(("molefe", new DateOnly(2023, 1, 15), new DateOnly(2026, 10, 26)));
        JSInterop.VerifyInvoke("wombatFileDownload");

        var result = cut.Find(".export-page > .action-result");
        result.NextElementSibling!.TagName.Should().Be("FORM", "the result sits above the form (Q18)");
        var alert = result.QuerySelector(".alert.alert-success[role=status]")!;
        Text(alert.QuerySelector("strong")!).Should().Be("Your portfolio exported: portfolio-aba6ce7943c2.pdf.");
        Text(alert).Should().Be("Your portfolio exported: portfolio-aba6ce7943c2.pdf. The download should start automatically. " +
            "Anyone holding the file can check it on Verify portfolio export.");
        var verify = alert.QuerySelector("a")!;
        verify.GetAttribute("href").Should().Be("/portfolio/verify", "a plain href, a full navigation to the static page (S13)");
        cut.WaitForAssertion(() => FocusedId().Should().Be(cut.FindComponent<ActionResult>().Instance.Element.Id));
    }

    [Fact]
    public void WhileThePdfIsMade_TheButtonSaysSo_IsAriaDisabled_AndTheStatusSpeaks()
    {
        var cut = RenderOwn(Molefe());
        var made = new TaskCompletionSource<object?>();
        _sender.OnAsync<ExportPortfolioCommand>(_ => made.Task);

        cut.Find("form.form-container").Submit();

        var button = cut.Find(".form-actions button");
        Text(button).Should().Be("Generating...");
        button.GetAttribute("aria-disabled").Should().Be("true");
        button.HasAttribute("disabled").Should().BeFalse();
        Statuses(cut).Should().Contain("Generating the PDF.");

        cut.Find("form.form-container").Submit();
        _sender.Sent.OfType<ExportPortfolioCommand>().Should().ContainSingle("a second press sends nothing");
        made.SetResult(new PortfolioExportResult([], "portfolio-aba6ce7943c2.pdf", "hash"));
    }

    // ─── Refusals under their field (R3-E11, E12) ────────────────────────────

    [Fact]
    public void FromAfterTo_IsRefusedUnderFrom_TheDatesKept_FromFocused()
    {
        var cut = RenderOwn(Molefe());
        _sender.OnAsync<ExportPortfolioCommand>(_ => Task.FromException<object?>(new FluentValidation.ValidationException(
            [new ValidationFailure(nameof(ExportPortfolioCommand.FromDate), "The start date must be before or equal to the end date.")])));
        cut.Find("#export-from").Change("2026-11-02");

        cut.Find("form.form-container").Submit();

        var from = cut.Find("#export-from");
        from.GetAttribute("value").Should().Be("2026-11-02");
        from.GetAttribute("aria-invalid").Should().Be("true");
        from.ClassList.Should().Contain("input-validation-error");
        from.GetAttribute("aria-describedby").Should().Be("export-from-error export-from-help", "the refusal first (P5)");
        var message = cut.Find("#export-from-error");
        message.ClassList.Should().Contain("validation-message");
        message.GetAttribute("role").Should().Be("alert");
        Text(message).Should().Be("The start date must be before or equal to the end date.");
        cut.Find("#export-to").GetAttribute("value").Should().Be("2026-10-26");
        cut.FindAll(".action-result .alert").Should().BeEmpty("one field's refusal stays beside it");
        JSInterop.VerifyInvoke(PageFocus.FocusByIdIdentifier).Arguments[0].Should().Be("export-from");
    }

    [Fact]
    public void AnEmptiedDate_IsAskedFor_UnderItsField_AndNothingIsSent()
    {
        var cut = RenderOwn(Molefe());

        cut.Find("#export-to").Change("");
        cut.Find("form.form-container").Submit();

        Text(cut.Find("#export-to-error")).Should().Be("Choose the to date.");
        cut.FindAll("#export-from-error").Should().BeEmpty();
        _sender.Sent.OfType<ExportPortfolioCommand>().Should().BeEmpty();
        JSInterop.VerifyInvoke(PageFocus.FocusByIdIdentifier).Arguments[0].Should().Be("export-to");

        cut.Find("#export-from").Change("");
        cut.Find("form.form-container").Submit();
        Text(cut.Find("#export-from-error")).Should().Be("Choose the from date.");
    }

    // ─── A failed export (R3-E13) ────────────────────────────────────────────

    [Fact]
    public void AnyOtherFailure_IsFixedWords_Focused_NeverTheExceptionsText()
    {
        var cut = RenderOwn(Molefe());
        _sender.OnAsync<ExportPortfolioCommand>(_ => Task.FromException<object?>(new InvalidOperationException(Secret)));

        cut.Find("form.form-container").Submit();

        var alert = cut.Find(".export-page > .action-result .alert.alert-danger");
        Text(alert.QuerySelector("strong")!).Should().Be("The portfolio could not be exported just now.");
        Text(alert).Should().Be(
            "The portfolio could not be exported just now. Nothing was downloaded. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain(Secret);
        Text(cut.Find(".form-actions button")).Should().Be("Export PDF", "offered again");
        cut.WaitForAssertion(() => FocusedId().Should().Be(cut.FindComponent<ActionResult>().Instance.Element.Id));
    }

    [Fact]
    public void ACommandRefusal_DrawsTheUnavailablePage_InFrontOfItsWords()
    {
        var cut = RenderOwn(Molefe());
        _sender.OnAsync<ExportPortfolioCommand>(_ => Task.FromException<object?>(
            new UnauthorizedAccessException("You are not authorized to export this portfolio.")));

        cut.Find("form.form-container").Submit();

        Text(cut.Find("h1")).Should().Be("Portfolio unavailable");
        cut.Markup.Should().NotContain("You are not authorized");
        cut.FindAll("form").Should().BeEmpty();
    }

    // ─── Unavailable, loading, the load error (R3-E09, E10, E14, E15, E19) ───

    [Fact]
    public void NoSubject_IsPortfolioUnavailable_OneBodyNoForm()
    {
        // R3-E19: David Naidoo, acting as Assessor, has no trainee profile.
        var cut = RenderOwn(null);

        Text(cut.Find("h1")).Should().Be("Portfolio unavailable");
        TabTitle.Of(this, cut).Should().Be("Portfolio unavailable · Wombat");
        cut.FindAll(".page-subtitle").Should().BeEmpty();
        var card = cut.Find(".detail-card.detail-card--empty");
        Text(card.QuerySelector(".state-panel-copy")!).Should().Be("This portfolio does not exist, or you cannot export it.");
        var home = card.QuerySelector(".form-actions a.btn.btn-primary")!;
        Text(home).Should().Be("Go to Home");
        home.GetAttribute("href").Should().Be("/");
        cut.FindAll("form").Should().BeEmpty();
    }

    [Fact]
    public void WhileTheSubjectIsRead_NoForm_AndTheStatusSpeaks()
    {
        _sender.OnAsync<GetExportSubjectQuery>(_ => new TaskCompletionSource<object?>().Task);

        var cut = RenderComponent<ExportPortfolio>();

        Text(cut.Find("h1")).Should().Be("Export portfolio");
        Statuses(cut).Should().Contain("Loading this export.");
        cut.FindAll("form").Should().BeEmpty();
        cut.FindAll(".export-page .skeleton.skel-title").Should().ContainSingle();
        cut.Markup.Should().NotContain("style=");
    }

    [Fact]
    public void AFailedRead_IsFixedWords_WithTryAgain()
    {
        _sender.OnAsync<GetExportSubjectQuery>(_ => Task.FromException<object?>(new InvalidOperationException(Secret)));

        var cut = RenderComponent<ExportPortfolio>();

        cut.WaitForAssertion(() => cut.Find(".alert.alert-danger"));
        Text(cut.Find(".alert-row-text")).Should().Be(
            "Could not load this export. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain(Secret);
        cut.FindAll("form").Should().BeEmpty();

        _sender.On<GetExportSubjectQuery>(_ => Molefe());
        cut.Find(".alert-row button").Click();
        cut.WaitForAssertion(() => cut.Find("form.form-container"));
    }

    // ─── The staff export (R3-E04–E06, E17, E18, E20; M4; check build note 6) ───────────

    [Fact]
    public void TheStaffExport_NamesTheRegistrar_InTheTitleSubtitleHintsAndResult()
    {
        SignIn(WombatRoles.InstitutionalAdmin, "mbatha");
        var cut = RenderStaff(Molefe(), WombatRoles.InstitutionalAdmin);

        _sender.Sent.OfType<GetExportSubjectQuery>().Single().TraineeUserId.Should().Be("molefe");
        Text(cut.Find("h1")).Should().Be("Export Lerato Molefe's portfolio");
        TabTitle.Of(this, cut).Should().Be("Export Lerato Molefe's portfolio · Wombat");
        Text(cut.Find(".page-subtitle")).Should().Be("Kgosi Kgari Teaching Hospital · Paediatric EPA Curriculum (11.1) · Training year 4");
        Field(cut, "export-from").Hint.Should().Be("Lerato Molefe's programme started on 2023-01-15.");
        Field(cut, "export-to").Hint.Should().Be("Today, on the South African calendar.");

        _sender.On<ExportPortfolioCommand>(_ => new PortfolioExportResult([], "portfolio-aba6ce7943c2.pdf", "hash"));
        cut.Find("form.form-container").Submit();

        _sender.Sent.OfType<ExportPortfolioCommand>().Single().TraineeUserId.Should().Be("molefe");
        Text(cut.Find(".action-result strong")).Should().Be("Lerato Molefe's portfolio exported: portfolio-aba6ce7943c2.pdf.");
    }

    [Fact]
    public void TheStaffExport_AfterTheEnd_ClosesOnTheRegistrarsLastDay()
    {
        // R3-E20: Pieter Smit after the end, from the review page.
        SignIn(WombatRoles.Coordinator, "smit");
        var cut = RenderStaff(Molefe() with { LastDay = new DateOnly(2026, 10, 26) }, WombatRoles.Coordinator);

        Field(cut, "export-to").Should().Be(("To date *required", "2026-10-26", "Lerato Molefe's last day in the programme."));
    }

    [Fact]
    public void TheStaffTrail_UnderTrainees_ReturnsToTheProfile()
    {
        SignIn(WombatRoles.InstitutionalAdmin, "mbatha");
        var cut = RenderStaff(Molefe(), WombatRoles.InstitutionalAdmin);

        Crumbs(cut).Should().Equal(
            ("Home", "/"), ("Trainees", "/admin/trainees"), ("Lerato Molefe's programme", "/admin/trainees/edit?id=2"),
            ("Export portfolio", null));
        cut.FindAll("[aria-current]").Should().ContainSingle("the trail's own crumb; the menu is not drawn here");
    }

    [Fact]
    public void TheStaffTrail_UnderProgrammeTrainees_ReturnsToTheRegistrarsPage()
    {
        SignIn(WombatRoles.Coordinator, "smit");
        var cut = RenderStaff(Molefe(), WombatRoles.Coordinator);

        Crumbs(cut).Should().Equal(
            ("Home", "/"), ("Programme trainees", "/programme/trainees"), ("Lerato Molefe", "/programme/trainees/2"),
            ("Export portfolio", null));
    }

    [Fact]
    public void TheStaffTrail_WithNoProfile_OrForTheAdministrator_HasNoMiddleCrumb()
    {
        SignIn(WombatRoles.InstitutionalAdmin, "mbatha");
        Crumbs(RenderStaff(Molefe() with { ProfileId = null }, WombatRoles.InstitutionalAdmin)).Should().Equal(
            ("Home", "/"), ("Trainees", "/admin/trainees"), ("Export portfolio", null));

        SignIn(WombatRoles.Administrator, "devadmin");
        Crumbs(RenderStaff(Molefe(), WombatRoles.Administrator)).Should().Equal(("Home", "/"), ("Export portfolio", null));
    }

    [Fact]
    public void TheStaffExport_WhileRead_IsExportPortfolio_UnderItsOwner_WithNoName()
    {
        // N12, R3-E14.
        SignIn(WombatRoles.InstitutionalAdmin, "mbatha");
        _sender.OnAsync<GetExportSubjectQuery>(_ => new TaskCompletionSource<object?>().Task);

        var cut = RenderStaff(null, WombatRoles.InstitutionalAdmin, answer: false);

        Text(cut.Find("h1")).Should().Be("Export portfolio");
        Crumbs(cut).Should().Equal(("Home", "/"), ("Trainees", "/admin/trainees"), ("Export portfolio", null));
    }

    [Fact]
    public void TheStaffExport_Refused_IsPortfolioUnavailable_WithNoTrail()
    {
        // R3-E09, E10: refused and unknown alike.
        SignIn(WombatRoles.InstitutionalAdmin, "mbatha");
        var cut = RenderStaff(null, WombatRoles.InstitutionalAdmin);

        Text(cut.Find("h1")).Should().Be("Portfolio unavailable");
        cut.FindAll("nav[aria-label='Breadcrumb']").Should().BeEmpty();
        Text(cut.Find(".state-panel-copy")).Should().Be("This portfolio does not exist, or you cannot export it.");
    }

    [Fact]
    public void TheStaffRoute_IsItsOwnComponent_SoTheMenuLightsItsOwner()
    {
        typeof(ExportTraineePortfolio).GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>()
            .Select(route => route.Template).Should().Equal("/portfolio/export/{TraineeUserId}");
        typeof(ExportPortfolio).GetCustomAttributes(typeof(RouteAttribute), false).Cast<RouteAttribute>()
            .Select(route => route.Template).Should().Equal("/portfolio/export");
    }

    // ─── The cast ────────────────────────────────────────────────────────────

    private static ExportSubjectDto Molefe() => new(
        "molefe", "Lerato Molefe", "Kgosi Kgari Teaching Hospital", "Paediatric EPA Curriculum", "11.1", 4,
        new DateOnly(2023, 1, 15), null, 2);

    private IRenderedComponent<ExportPortfolio> RenderOwn(ExportSubjectDto? subject)
    {
        _sender.On<GetExportSubjectQuery>(_ => subject);
        var cut = RenderComponent<ExportPortfolio>();
        cut.WaitForState(() => cut.FindAll("form, .detail-card--empty").Count > 0);
        return cut;
    }

    private IRenderedComponent<ExportTraineePortfolio> RenderStaff(ExportSubjectDto? subject, string role, bool answer = true)
    {
        if (answer)
        {
            _sender.On<GetExportSubjectQuery>(_ => subject);
        }

        var acting = ActingRoleResolver.Resolve(role, [role]);
        var cut = RenderComponent<ExportTraineePortfolio>(parameters => parameters
            .Add(page => page.TraineeUserId, "molefe")
            .AddCascadingValue(acting)
            .AddCascadingValue(NavItems.For(acting, PrincipalFor(role)))
            .AddCascadingValue(new RouteData(typeof(ExportTraineePortfolio), new Dictionary<string, object?>())));
        if (answer)
        {
            cut.WaitForState(() => cut.FindAll("form, .detail-card--empty").Count > 0);
        }

        return cut;
    }

    private void SignIn(string role, string userId)
    {
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, userId));
    }

    /// <summary>A date field's label, value and hint.</summary>
    private static (string Label, string Value, string? Hint) Field(IRenderedFragment cut, string id)
    {
        var input = cut.Find($"#{id}");
        var group = input.Closest(".form-group")!;
        input.GetAttribute("type").Should().Be("date");
        input.HasAttribute("required").Should().BeTrue();
        return (Text(group.QuerySelector($"label[for={id}]")!), input.GetAttribute("value") ?? string.Empty,
            group.QuerySelector("p.field-help") is { } help ? Text(help) : null);
    }

    private static List<(string Label, string? Href)> Crumbs(IRenderedFragment cut)
        => cut.FindAll("nav[aria-label='Breadcrumb'] li")
            .Select(li => (Text(li), li.QuerySelector("a")?.GetAttribute("href")))
            .ToList();

    private static IEnumerable<string> Statuses(IRenderedFragment cut)
        => cut.FindAll("p.visually-hidden[role=status]").Select(Text);

    private string? FocusedId()
        => JSInterop.Invocations.LastOrDefault(invocation => invocation.Identifier == FocusIdentifier).Arguments is [ElementReference reference, ..]
            ? reference.Id
            : null;

    private static string Text(IElement element) => Whitespace().Replace(element.TextContent, " ").Trim();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>Answers each request type it is told about, records every one, and refuses one nobody told it about.</summary>
    private sealed class ScriptedSender : IScopedSender
    {
        private readonly Dictionary<Type, Func<object, Task<object?>>> _answers = [];

        public List<object> Sent { get; } = [];

        public void On<TRequest>(Func<TRequest, object?> answer)
            => _answers[typeof(TRequest)] = request => Task.FromResult(answer((TRequest)request));

        public void OnAsync<TRequest>(Func<TRequest, Task<object?>> answer)
            => _answers[typeof(TRequest)] = request => answer((TRequest)request);

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            return _answers.TryGetValue(request.GetType(), out var answer)
                ? (TResponse)(await answer(request))!
                : throw new InvalidOperationException($"The page sent {request.GetType().Name}, which this test does not answer.");
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"The page sent {request.GetType().Name}.");
    }
}
