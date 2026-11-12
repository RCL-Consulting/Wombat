using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Audit;
using Wombat.Application.Features.Audit.Queries.ListAuditEntries;
using Wombat.Domain.Audit;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Audit;
using Wombat.Web.Services;
using Wombat.Web.Tests.Accessibility;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The audit log (T400, flow 18, lane C; boards R3-L01–L13, R2-L14, L15): the filters' order and words, Show and Clear
/// filters after them, the address read and written, the window line, the count line and its focus, the pager and its
/// focus on "Actions", the Person cell's kinds, the Detail's name in SAST, the empty state, loading, and a failed read in
/// fixed words.
/// </summary>
public sealed class AuditListPageTests : WombatTestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";
    private const string FocusById = "wombat.focusById";
    private const string Secret = "Npgsql: the connection string was rejected.";

    // 2026-11-10 22:24 SAST.
    private static readonly DateTime Now = new(2026, 11, 10, 20, 24, 30, DateTimeKind.Utc);

    private readonly ScriptedSender _sender = new();
    private readonly TestAuthorizationContext _auth;

    public AuditListPageTests()
    {
        _auth = this.AddTestAuthorization();
        SignInAs(WombatRoles.InstitutionalAdmin);
        Services.AddSingleton<IScopedSender>(_sender);
        Services.AddSingleton<TimeProvider>(new FixedClock(Now));
        JSInterop.Mode = JSRuntimeMode.Loose;
        _sender.Answer = query => Page(query, [Login(new DateTime(2026, 11, 10, 18, 24, 10, DateTimeKind.Utc))], total: 166);
    }

    [Fact]
    public void L04_OnArrival_TheFiltersInOrder_ShowAfterThem_NoClear_AndTheLast24Hours()
    {
        var cut = Render();

        Text(cut.Find("h1")).Should().Be("Audit log");
        Text(cut.Find(".page-subtitle")).Should().Be($"Every consequential action at {Kgk}, newest first.");
        cut.FindAll(".header-container button, .header-container a").Should().BeEmpty("no Apply filters in the header (T328)");

        var form = cut.Find("form.search-container");
        form.GetAttribute("role").Should().Be("search");
        form.GetAttribute("aria-label").Should().Be("Filter the audit log");
        form.QuerySelectorAll("label").Select(Text).Should().Equal(
            "Category", "Action", "Person's user id", "Subject type", "From (SAST)", "To (SAST)", "Result");
        cut.FindAll("#a-cat option").Select(Text).Should().Equal(
            "All categories", "Command", "Authentication", "Permission", "Committee", "MSF", "Export", "Data rights", "Job",
            "Activity type");
        cut.FindAll("#a-result option").Select(Text).Should().Equal("All results", "Success only", "Failures only");
        Text(cut.Find("#a-action-help")).Should().Be("Part of a name: \"Msf\" finds the MSF commands.");
        cut.Find("#a-action").GetAttribute("aria-describedby").Should().Be("a-action-help");
        Text(cut.Find("#a-to-help")).Should().Be("Empty: up to when you press Show.");
        cut.Find("#a-from").GetAttribute("type").Should().Be("datetime-local");
        cut.FindAll("#a-inst").Should().BeEmpty("the Institution filter is the Administrator's");

        cut.FindAll("form .filter-actions button").Select(Text).Should().Equal(["Show"], "no filter is applied yet");

        var query = _sender.SentOf<ListAuditEntriesQuery>().Single();
        query.To.Should().Be(Now, "the window is fixed when the page opens");
        query.From.Should().BeNull("the query's 24 hours before To");
        query.Page.Should().Be(1);
        query.PageSize.Should().Be(50);

        Text(cut.Find("h2#audit-actions")).Should().Be("Actions");
        cut.Find("h2#audit-actions").GetAttribute("tabindex").Should().Be("-1");
        var count = cut.Find("p#audit-count");
        Text(count).Should().Be("166 actions.");
        count.GetAttribute("tabindex").Should().Be("-1");
        count.HasAttribute("role").Should().BeFalse("the count line is no live region (N2)");
        Text(count.NextElementSibling!).Should().Be(
            "Read from 2026-11-09 22:24 SAST to 22:24 SAST: the last 24 hours, when you opened the page. Turning a page keeps this window.");
    }

    [Fact]
    public void L01_TheTable_ItsColumns_TheTimeInSast_AndEachDetailNamedForItsRow()
    {
        var cut = Render();

        Text(cut.Find("caption")).Should().Be("Actions, newest first");
        cut.Find("table").ClassList.Should().Contain("clinic-table--stack-wide");
        cut.Find(".people-page").ClassList.Should().Contain("audit-page");
        cut.FindAll("thead th").Select(Text).Should().Equal("When", "Person", "Category", "Action", "Result", "Detail");
        cut.Find("thead th:last-child span").ClassList.Should().Contain("visually-hidden");

        var row = cut.Find("tbody tr");
        row.QuerySelectorAll("td[data-label=When] .nowrap").Select(Text).Should().Equal("2026-11-10", "20:24:10 SAST");
        Text(row.QuerySelector("td[data-label=Person]")!).Should().Be("Nolwazi Mbatha");
        Text(row.QuerySelector("td[data-label=Category]")!).Should().Be("Authentication");
        row.QuerySelector("td[data-label=Action]")!.ClassList.Should().Contain("col-wrap");
        Text(row.QuerySelector("td[data-label=Result] .badge")!).Should().Be("Success");
        row.QuerySelectorAll("th[scope=row]").Should().BeEmpty("the audit rows have no row header (S7)");

        var detail = row.QuerySelector("a.btn")!;
        Text(detail).Should().Be("Detail");
        detail.GetAttribute("aria-label").Should().Be("Detail: Login at 2026-11-10 20:24:10 SAST");
        var href = detail.GetAttribute("href")!;
        href.Should().StartWith("/admin/audit/");
        AuditLinks.SafeLogReturn(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri("http://x" + href).Query)["back"])
            .Should().Be("/admin/audit?opened=2026-11-10T20%3A24%3A30.0000000Z", "the way back carries the window it was read in");

        Text(cut.Find(".pager .pager-info")).Should().Be("Showing 1–50 of 166");
        cut.Markup.Should().NotContain("(UTC)").And.NotContain("Apply filters").And.NotContain("Actor user ID");
    }

    [Fact]
    public void L05_TheAdministrator_TheInstitutionFilter_AndEachPersonsInstitutionUnderTheName()
    {
        SignInAs(WombatRoles.Administrator);
        _sender.Answer = query => Page(
            query,
            [
                Login(Now.AddMinutes(-1)) with { PersonName = "Demo Administrator", InstitutionId = null, InstitutionName = null },
                Login(Now.AddMinutes(-2)),
                Row(Now.AddMinutes(-3), "SubmitMsfResponseCommand", AuditActorKind.MsfRespondent, null, null),
                Row(Now.AddMinutes(-4), "LoginFailed", AuditActorKind.None, null, null, success: false, error: "Invalid sign-in attempt."),
            ],
            total: 174,
            everyInstitution: true);

        var cut = Render();

        Text(cut.Find(".page-subtitle")).Should().Be(
            "Every consequential action across the institutions, and those with no institution, newest first.");
        cut.FindAll("form label").Select(Text).Last().Should().Be("Institution");
        cut.FindAll("#a-inst option").Select(Text).Should().Equal(
            "All institutions", "No institution", "Demo Institution", Kgk);
        cut.FindAll("tbody td[data-label=Person]").Select(Text).Should().Equal(
            "Demo AdministratorNo institution",
            $"Nolwazi Mbatha{Kgk}",
            "MSF respondent (no account)No institution",
            "No signed-in personNo institution");
        cut.FindAll("tbody td[data-label=Person] .person-cell span").Select(Text).Should().Equal(
            "No institution", Kgk, "No institution", "No institution");
        Text(cut.FindAll("tbody tr").Last().QuerySelector("td[data-label=Result]")!).Should().Be("FailedInvalid sign-in attempt.");
    }

    [Fact]
    public void TheAddress_IsRead_IntoTheFieldsAndTheQuery_AndTheWindowSaysShow()
    {
        var cut = Render(
            "/admin/audit?category=Command&action=Msf&person=sipho&subject=Activity&from=2026-11-10T18%3A58&result=failure" +
            "&shown=2026-11-10T18%3A00%3A00.0000000Z&page=2&size=20");

        var query = _sender.SentOf<ListAuditEntriesQuery>().Single();
        query.Category.Should().Be(AuditCategory.Command);
        query.Action.Should().Be("Msf");
        query.ActorUserId.Should().Be("sipho");
        query.SubjectType.Should().Be("Activity");
        query.SuccessOnly.Should().BeFalse();
        query.From.Should().Be(new DateTime(2026, 11, 10, 16, 58, 0, DateTimeKind.Utc), "18:58 on the South African clock");
        query.To.Should().Be(new DateTime(2026, 11, 10, 18, 0, 0, DateTimeKind.Utc), "To empty: when Show was pressed");
        query.Page.Should().Be(2);
        query.PageSize.Should().Be(20);

        cut.Find("#a-cat").GetAttribute("value").Should().Be("Command");
        cut.Find("#a-action").GetAttribute("value").Should().Be("Msf");
        cut.Find("#a-from").GetAttribute("value").Should().Be("2026-11-10T18:58");
        cut.Find("#a-to").GetAttribute("value").Should().BeNullOrEmpty();
        cut.Find("#a-result").GetAttribute("value").Should().Be("failure");
        cut.FindAll("form .filter-actions button").Select(Text).Should().Equal("Show", "Clear filters");
        Text(cut.Find("p#audit-count").NextElementSibling!).Should().Be(
            "Read from 2026-11-10 18:58 SAST to 20:00 SAST, when you pressed Show. Turning a page keeps this window.");
    }

    [Fact]
    public void AnInstitutionsAdministrator_IsNotFilteredByAnInstitutionInTheAddress()
    {
        Render("/admin/audit?institution=2");

        _sender.SentOf<ListAuditEntriesQuery>().Single().Institution.Should().BeNull();
    }

    [Theory]
    [InlineData("none", null)]
    [InlineData("2", 2)]
    public void TheAdministratorsInstitution_IsReadFromTheAddress_InItsThreeKinds(string value, int? one)
    {
        SignInAs(WombatRoles.Administrator);

        var cut = Render($"/admin/audit?institution={value}");

        _sender.SentOf<ListAuditEntriesQuery>().Single().Institution.Should().Be(
            one is int id ? AuditInstitutionFilter.One(id) : AuditInstitutionFilter.NoInstitution);
        cut.Find("#a-inst").GetAttribute("value").Should().Be(value);
    }

    [Fact]
    public void Show_WritesTheAddress_WithTheWindowFixedNow_AndFocusesTheCountLine()
    {
        var cut = Render();

        cut.Find("#a-cat").Change("Msf");
        cut.Find("#a-action").Change("Msf");
        cut.Find("#a-from").Change("2026-11-10T18:58");
        cut.Find("#a-result").Change("failure");
        _sender.SentOf<ListAuditEntriesQuery>().Should().HaveCount(1, "a field's change sends nothing");

        cut.Find("form.search-container").Submit();

        var address = new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery;
        address.Should().Be(
            "/admin/audit?category=Msf&action=Msf&from=2026-11-10T18%3A58&result=failure&shown=2026-11-10T20%3A24%3A30.0000000Z");
        cut.WaitForState(() => _sender.SentOf<ListAuditEntriesQuery>().Count() == 2);
        _sender.SentOf<ListAuditEntriesQuery>().Last().Category.Should().Be(AuditCategory.Msf);
        cut.WaitForAssertion(() => FocusedIds().Should().Equal("audit-count"));
    }

    [Fact]
    public void ATurnOfPage_KeepsTheWindow_AndFocusesActions()
    {
        var cut = Render();

        cut.FindAll(".pager button").Single(button => Text(button) == "Next").Click();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be(
            "/admin/audit?opened=2026-11-10T20%3A24%3A30.0000000Z&page=2");
        cut.WaitForState(() => _sender.SentOf<ListAuditEntriesQuery>().Count() == 2);
        var second = _sender.SentOf<ListAuditEntriesQuery>().Last();
        second.Page.Should().Be(2);
        second.To.Should().Be(Now, "the window fixed on arrival (T277)");
        cut.WaitForAssertion(() => FocusedIds().Should().Equal("audit-actions"));
    }

    [Fact]
    public void ClearFilters_GoesToTheBareAddress_AndFocusesCategory()
    {
        var cut = Render("/admin/audit?result=failure&shown=2026-11-10T18%3A00%3A00.0000000Z");

        cut.FindAll("form .filter-actions button").Single(button => Text(button) == "Clear filters").Click();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be("/admin/audit");
        cut.WaitForState(() => _sender.SentOf<ListAuditEntriesQuery>().Count() == 2);
        _sender.SentOf<ListAuditEntriesQuery>().Last().SuccessOnly.Should().BeNull();
        cut.WaitForAssertion(() => FocusedIds().Should().Equal("a-cat"));
    }

    [Fact]
    public void L13_BackFromAnEntryByItsTrail_TheSameRows_AndItsDetailTakesTheFocus()
    {
        var left = Login(new DateTime(2026, 11, 10, 17, 1, 41, DateTimeKind.Utc));
        _sender.Answer = query => Page(query, [Login(Now.AddMinutes(-1)), left], total: 4);

        var cut = Render($"/admin/audit?result=failure&shown=2026-11-10T18%3A00%3A00.0000000Z&entry={left.Id}");

        _sender.SentOf<ListAuditEntriesQuery>().Single().SuccessOnly.Should().BeFalse();
        cut.WaitForAssertion(() => FocusedIds().Should().Equal($"detail-{left.Id}"));
    }

    [Fact]
    public void L11_NothingMatches_SaysSo_AndOffersToClearTheFilters()
    {
        _sender.Answer = query => Page(query, [], total: 0);

        var cut = Render("/admin/audit?action=NoSuchCommand&shown=2026-11-10T18%3A00%3A00.0000000Z");

        Text(cut.Find("p#audit-count")).Should().Be("No actions match these filters.");
        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be("No actions match");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            "Nothing matches these filters between these times. Widen From and To, or clear the filters.");
        var clear = cut.Find(".detail-card--empty button");
        Text(clear).Should().Be("Clear the filters");
        cut.FindAll("table").Should().BeEmpty();

        clear.Click();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be("/admin/audit");
    }

    [Fact]
    public void L10_OneAction_IsSingular()
    {
        _sender.Answer = query => Page(query, [Login(Now.AddMinutes(-1))], total: 1);

        var cut = Render();

        Text(cut.Find("p#audit-count")).Should().Be("1 action.");
        Text(cut.Find(".pager .pager-info")).Should().Be("Showing 1–1 of 1");
    }

    [Fact]
    public void L02_AFailedRow_ShowsItsStoredWordsUnderItsBadge()
    {
        _sender.Answer = query => Page(
            query,
            [Row(Now.AddMinutes(-1), "TransitionActivityCommand", AuditActorKind.Person, "Fatima Khumalo", "khumalo", success: false, error: "Decline requires a note.")],
            total: 4);

        var cut = Render();

        var result = cut.Find("tbody td[data-label=Result]");
        Text(result.QuerySelector(".badge")!).Should().Be("Failed");
        Text(result.QuerySelector(".text-sm")!).Should().Be("Decline requires a note.");
    }

    [Fact]
    public void TwoActionsInOneSecond_HaveDistinctDetailNames()
    {
        var at = Now.AddMinutes(-1);
        _sender.Answer = query => Page(
            query,
            [Login(at), Login(at) with { Id = Guid.NewGuid(), PersonName = "Pieter Smit" }],
            total: 2);

        var cut = Render();

        cut.FindAll("tbody a.btn").Select(link => link.GetAttribute("aria-label")).Should().Equal(
            "Detail: Login at 2026-11-10 22:23:30 SAST by Nolwazi Mbatha",
            "Detail: Login at 2026-11-10 22:23:30 SAST by Pieter Smit");
    }

    [Fact]
    public void R2L14_WhileLoading_TheFiltersStay_ShowWaits_AndTheLineSaysWhat()
    {
        _sender.Held = request => request is ListAuditEntriesQuery;

        var cut = Render(wait: false);

        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading the audit log.");
        var show = cut.FindAll("form .filter-actions button").Single(button => Text(button) == "Show");
        show.GetAttribute("aria-disabled").Should().Be("true");
        show.HasAttribute("disabled").Should().BeFalse();
        cut.Find("form.search-container").Submit();
        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be("/admin/audit", "a press sends nothing");

        _sender.Gate.SetResult();
        cut.WaitForState(() => cut.FindAll("p#audit-count").Count == 1, AsyncWorkTimeout);
    }

    [Fact]
    public void R2L15_AFailedRead_IsTheHouseSentence_WithTryAgain_NeverTheExceptionsText()
    {
        _sender.Answer = _ => new InvalidOperationException(Secret);

        var cut = Render(waitFor: "Could not load");

        Text(cut.Find(".alert-row-text")).Should().Be(
            "Could not load the audit log. Nothing has changed. Try again, or come back in a few minutes.");
        cut.FindAll(".alert-row button").Select(Text).Should().Equal("Try again");
        cut.Markup.Should().NotContain(Secret);
        cut.FindAll("form.search-container").Should().ContainSingle("the filters stay drawn");
    }

    [Fact]
    public void ThePage_HasNoBrokenIdReference_AndNoUnnamedControl()
    {
        SignInAs(WombatRoles.Administrator);
        _sender.Answer = query => Page(query, [Login(Now.AddMinutes(-1))], total: 1, everyInstitution: true);

        var cut = Render("/admin/audit?result=failure&shown=2026-11-10T18%3A00%3A00.0000000Z");

        IdReferences.Broken(cut).Should().BeEmpty();
        AccessibleNames.Unnamed(cut).Should().BeEmpty();
    }

    [Fact]
    public void AddressValuesThatDoNotRead_AreNoFilter()
    {
        var cut = Render("/admin/audit?category=0&from=yesterday&result=maybe&page=-3&size=7&opened=never");

        var query = _sender.SentOf<ListAuditEntriesQuery>().Single();
        query.Category.Should().BeNull();
        query.From.Should().BeNull();
        query.SuccessOnly.Should().BeNull();
        query.Page.Should().Be(1);
        query.PageSize.Should().Be(50);
        query.To.Should().Be(Now);
        cut.FindAll("form .filter-actions button").Select(Text).Should().Equal("Show");
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<AuditList> Render(string address = AuditLinks.Log, bool wait = true, string? waitFor = null)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        var cut = RenderComponent<AuditList>();
        if (wait)
        {
            cut.WaitForState(
                () => waitFor is null ? cut.FindAll("p#audit-count").Count == 1 : cut.Markup.Contains(waitFor),
                AsyncWorkTimeout);
        }

        return cut;
    }

    private void SignInAs(string role)
    {
        _auth.SetAuthorized("mbatha@kgk.wombat.local");
        _auth.SetRoles(role);
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "mbatha") };
        if (role != WombatRoles.Administrator)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, "1"));
        }

        _auth.SetClaims(claims.ToArray());
    }

    private IReadOnlyList<string> FocusedIds()
        => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == FocusById)
            .Select(invocation => (string)invocation.Arguments[0]!)
            .ToList();

    private static object Page(object request, IReadOnlyList<AuditLogRowDto> rows, int total, bool everyInstitution = false)
    {
        var query = (ListAuditEntriesQuery)request;
        var to = query.To ?? Now;
        return new PagedAuditResult(
            rows,
            total,
            query.Page,
            query.PageSize,
            query.From ?? to.AddHours(-24),
            to,
            everyInstitution,
            everyInstitution ? [new AuditInstitutionOptionDto(2, "Demo Institution"), new AuditInstitutionOptionDto(1, Kgk)] : [],
            everyInstitution ? null : Kgk);
    }

    private static AuditLogRowDto Login(DateTime at)
        => Row(at, "Login", AuditActorKind.Person, "Nolwazi Mbatha", "mbatha", category: AuditCategory.Authentication);

    private static AuditLogRowDto Row(
        DateTime at,
        string action,
        AuditActorKind kind,
        string? name,
        string? userId,
        bool success = true,
        string? error = null,
        AuditCategory category = AuditCategory.Command)
        => new(Guid.NewGuid(), at, category, action, success, error, kind, name, userId,
            kind == AuditActorKind.Person ? 1 : null, kind == AuditActorKind.Person ? Kgk : null);

    private static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    private sealed class FixedClock(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    /// <summary>Answers each request from <see cref="Answer" />, and keeps what was sent. An exception answered is thrown.</summary>
    private sealed class ScriptedSender : IScopedSender
    {
        public Func<object, object?> Answer { get; set; } = _ => null;

        public List<object> Sent { get; } = [];

        public Func<object, bool>? Held { get; set; }

        public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public IEnumerable<T> SentOf<T>() => Sent.OfType<T>();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            if (Held?.Invoke(request) == true)
            {
                await Gate.Task;
            }

            var answer = Answer(request);
            if (answer is Exception exception)
            {
                throw exception;
            }

            return answer is null ? default! : (TResponse)answer;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            return Task.CompletedTask;
        }
    }
}
