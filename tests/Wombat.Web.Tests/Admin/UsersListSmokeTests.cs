using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Institutions.Queries.GetMyInstitutionName;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Queries.ListUsers;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Components.Shared;
using Wombat.Web.Services;
using Wombat.Web.Tests.TestSupport;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// Users (T376, flow 12, lane U; boards R3-U01–U14): every state the boards draw. Ten at 2.12 with no pager (F9), the
/// filter card (S21) and its address (S6), the heading's count and the hidden live count with the focus on the heading,
/// no match, the No role cell with and without an ended programme (S2), the Locked out badge (K25), Manage's way back (S6),
/// the heavy set's pager, loading and a failed read with Try again (S9, T329), and the Administrator's view (U14).
/// </summary>
public sealed class UsersListSmokeTests : WombatTestContext
{
    private const string Kgk = "Kgosi Kgari Teaching Hospital";
    private const string FocusById = "wombat.focusById";
    private const string Secret = "Npgsql: the connection string was rejected.";

    private readonly FakeSender _sender = new();
    private readonly TestAuthorizationContext _auth;

    public UsersListSmokeTests()
    {
        _auth = this.AddTestAuthorization();
        SignInAs(WombatRoles.InstitutionalAdmin);
        Services.AddSingleton<IScopedSender>(_sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ─── U01: ten at 2.12 ────────────────────────────────────────────────────

    [Fact]
    public void U01_Ten_TheHeaderTheFilterCardTheCountedHeadingAndTheRows_WithNoPager()
    {
        _sender.Users = _ => List(Ten(), total: 10);

        var cut = Render();

        Text(cut.Find("h1")).Should().Be("Users");
        Text(cut.Find(".page-subtitle")).Should().Be(
            $"Everyone with an account at {Kgk}: the roles each holds, their scope, and whether they can sign in.");
        var invite = cut.Find(".header-container a.btn-primary");
        invite.GetAttribute("href").Should().Be(OnboardingLinks.InviteAPerson);
        Text(invite).Should().Be("Invite a person");

        var form = cut.Find("form.detail-card.people-filter");
        form.GetAttribute("role").Should().Be("search");
        form.GetAttribute("aria-label").Should().Be("Filter people");
        Text(cut.Find("label[for=pf-q]")).Should().Be("Name or email");
        cut.Find("#pf-q").GetAttribute("aria-describedby").Should().Be("pf-q-h");
        Text(cut.Find("#pf-q-h")).Should().Be("A name in either order, part of one, or an address.");
        cut.FindAll("#pf-r option").Select(Text).Should().Equal(
            "Any role", "Institutional admin", "Speciality admin", "Sub-speciality admin", "Coordinator", "Committee member",
            "Assessor", "Pending trainee", "Trainee", "No role");
        cut.FindAll("#pf-s option").Select(Text).Should().Equal("Any status", "Active", "Locked out");
        cut.Find(".filter-actions button[type=submit].btn-primary").TextContent.Trim().Should().Be("Show");
        cut.Find(".filter-actions button[type=button].btn-outline").TextContent.Trim().Should().Be("Clear");

        var heading = cut.Find("h2#people-heading.list-section-title");
        Text(heading).Should().Be("People 10");
        heading.GetAttribute("tabindex").Should().Be("-1");
        cut.Find("section.list-section").GetAttribute("aria-labelledby").Should().Be("people-heading");
        Text(cut.Find("p.visually-hidden[role=status]")).Should().BeEmpty("nothing is filtered");

        var table = cut.Find("table");
        table.ClassList.Should().Contain(["clinic-table--stack", "clinic-table--stack-wide"]);
        table.GetAttribute("role").Should().Be("table");
        Text(table.QuerySelector("caption")!).Should().Be(
            $"At {Kgk}, by last name: each person's roles, one scope for all of them, and whether they can sign in.");
        table.QuerySelectorAll("thead th").Select(Text).Should().Equal("Name", "Roles", "Scope", "Status", "Actions");
        cut.FindAll("nav.pager, .pager").Should().BeEmpty("ten fit on one page of 20 (F9)");
        cut.FindAll("[style]").Should().BeEmpty("no inline style (S18)");
    }

    [Fact]
    public void U01_ARow_TheNameOverTheAddressBreakingAfterItsAt_RolesOneALine_ScopeStatusAndManage()
    {
        _sender.Users = _ => List(Ten(), total: 10);

        var cut = Render();

        var zulu = RowOf(cut, "Thandi Zulu");
        var header = zulu.QuerySelector("th[scope=row]")!;
        header.GetAttribute("role").Should().Be("rowheader");
        header.ClassList.Should().NotContain("col-wrap", "check Left 3: the row header has no .col-wrap");
        header.QuerySelector(".person-cell > span")!.InnerHtml.Should().Be("zulu@<wbr>kgk.wombat.local");
        zulu.QuerySelectorAll("td[data-label=Roles] ul.role-cell li").Select(Text).Should().Equal("Committee member", "Assessor");
        Text(zulu.QuerySelector("td[data-label=Scope]")!).Should().Be("Paediatrics / Paediatrics");
        Text(zulu.QuerySelector("td[data-label=Status]")!).Should().Be("Active");
        var manage = zulu.QuerySelector("td.col-actions a.btn.btn-outline.btn-sm")!;
        Text(manage).Should().Be("Manage");
        manage.GetAttribute("aria-label").Should().Be("Manage Thandi Zulu");
        manage.GetAttribute("href").Should().Be(PeopleLinks.User("zulu", back: PeopleLinks.Users));
        cut.FindAll("td[data-label=Institution]").Should().BeEmpty("her list has no Institution column (Q6)");
    }

    // ─── U02, U02b, U09: the filter, read from the address ───────────────────

    [Fact]
    public void U02_AFilterInTheAddress_IsSent_CountedInTheHeadingAndTheLiveRegion_AndTheHeadingTakesTheFocus()
    {
        _sender.Users = _ => List([Zulu()], total: 10);

        var cut = Render(PeopleLinks.UsersFiltered(q: "zulu"));

        var query = _sender.Received.OfType<ListUsersQuery>().Single();
        query.Search.Should().Be("zulu");
        query.Page.Should().Be(1);
        query.PageSize.Should().Be(20);
        cut.Find("#pf-q").GetAttribute("value").Should().Be("zulu");
        Text(cut.Find("h2#people-heading")).Should().Be("People 1 of 10 match “zulu”");
        Text(cut.Find("p.visually-hidden[role=status]")).Should().Be("1 of 10 people match “zulu”.");
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == FocusById && (string)call.Arguments[0]! == "people-heading"));
        RowOf(cut, "Thandi Zulu").QuerySelector("a.btn")!.GetAttribute("href").Should().Be(
            PeopleLinks.User("zulu", back: PeopleLinks.UsersFiltered(q: "zulu")), "the way back keeps the filter (S6)");
    }

    [Fact]
    public void U09_NoRole_IsSent_ItsHelpShown_AndTheCountSaysWhoHoldsNone()
    {
        _sender.Users = _ => List([DuPlessis(ended: true), Molefe()], total: 14);

        var cut = Render(PeopleLinks.UsersFiltered(role: PeopleLinks.NoRole));

        _sender.Received.OfType<ListUsersQuery>().Single().Role.Should().Be(UsersFilterValues.NoRole);
        PeopleLinks.NoRole.Should().Be(UsersFilterValues.NoRole);
        cut.Find("#pf-r").GetAttribute("aria-describedby").Should().Be("pf-r-h");
        Text(cut.Find("#pf-r-h")).Should().Be("A completed or withdrawn registrar keeps an account and its records.");
        Text(cut.Find("h2#people-heading")).Should().Be("People 2 of 14 hold no role");
        Text(cut.Find("p.visually-hidden[role=status]")).Should().Be("2 of 14 people hold no role.");
    }

    [Theory]
    [InlineData("Administrator")]
    [InlineData("CollegeAdmin")]
    [InlineData("Wizard")]
    public void ARoleNotOfferedToHer_InACraftedAddress_ReadsAsAnyRole(string role)
    {
        _sender.Users = _ => List(Ten(), total: 10);

        var cut = Render(PeopleLinks.UsersFiltered(role: role));

        _sender.Received.OfType<ListUsersQuery>().Single().Role.Should().BeNull();
        Text(cut.Find("h2#people-heading")).Should().Be("People 10");
    }

    [Fact]
    public void Show_GoesToThePagesOwnAddress_WithTheValuesChosen_AndNothingFiltersPerKeystroke()
    {
        _sender.Users = _ => List(Ten(), total: 10);
        var cut = Render();

        cut.Find("#pf-q").Change("thandi zulu");
        cut.Find("#pf-r").Change(WombatRoles.CommitteeMember);
        cut.Find("#pf-s").Change(UsersFilterValues.Active);
        _sender.Received.OfType<ListUsersQuery>().Should().HaveCount(1, "a field's change sends nothing");

        cut.Find("form.people-filter").Submit();

        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        new Uri(navigation.Uri).PathAndQuery.Should().Be(
            PeopleLinks.UsersFiltered(q: "thandi zulu", role: WombatRoles.CommitteeMember, status: UsersFilterValues.Active));
    }

    [Fact]
    public void Clear_GoesToTheBareAddress()
    {
        _sender.Users = _ => List([Zulu()], total: 10);
        var cut = Render(PeopleLinks.UsersFiltered(q: "zulu", status: UsersFilterValues.Locked));

        cut.Find(".filter-actions button[type=button]").Click();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be(PeopleLinks.Users);
    }

    // ─── U03: no match ───────────────────────────────────────────────────────

    [Fact]
    public void U03_NoMatch_SaysWhatWasAsked_AndHowToSeeEveryone()
    {
        _sender.Users = _ => List([], total: 10);

        var cut = Render(PeopleLinks.UsersFiltered(q: "zzz"));

        Text(cut.Find("h2#people-heading")).Should().Be("People 0 of 10 match “zzz”");
        Text(cut.Find("p.visually-hidden[role=status]")).Should().Be("No one matches “zzz”.");
        Text(cut.Find(".detail-card--empty .state-panel-title")).Should().Be("No one matches “zzz”");
        Text(cut.Find(".detail-card--empty .state-panel-copy")).Should().Be(
            $"Nobody at {Kgk} has “zzz” in their name or address. Press Clear to see all 10 people.");
        cut.FindAll("table").Should().BeEmpty();
    }

    // ─── U07, U08: No role and Locked out ────────────────────────────────────

    [Fact]
    public void U07_NoRole_WithTheSentence_OnlyForAnEndedProgramme()
    {
        _sender.Users = _ => List([DuPlessis(ended: true), DuPlessis(ended: false) with { UserId = "vr", FirstName = "John", LastName = "van Rensburg" }], total: 14);

        var cut = Render();

        var ended = RowOf(cut, "Pieter du Plessis").QuerySelector("td[data-label=Roles] .no-role-cell")!;
        ended.ChildNodes.First().TextContent.Should().Be("No role");
        Text(ended.QuerySelector(".field-help")!).Should().Be("A completed or withdrawn registrar keeps an account and its records.");
        var none = RowOf(cut, "John van Rensburg").QuerySelector("td[data-label=Roles] .no-role-cell")!;
        Text(none).Should().Be("No role");
        none.QuerySelector(".field-help").Should().BeNull("no ended programme, no sentence (S2)");
    }

    [Fact]
    public void U08_LockedOut_IsTheBadgeWithTheLockIcon()
    {
        _sender.Users = _ => List([Zulu() with { UserId = "patel", FirstName = "Mohammed", LastName = "Patel", IsDeactivated = true }], total: 10);

        var cut = Render();

        var badge = RowOf(cut, "Mohammed Patel").QuerySelector("td[data-label=Status] span.badge.badge-declined.status-locked")!;
        Text(badge).Should().Be("Locked out");
        badge.QuerySelector("svg.icon").Should().NotBeNull();
    }

    // ─── U10: the heavy set ──────────────────────────────────────────────────

    [Fact]
    public void U10_ThePager_OnlyWhenTheMatchesRunPastOnePage()
    {
        var rows = Enumerable.Range(1, 20).Select(n => Zulu() with { UserId = $"p{n}", FirstName = $"Person{n}" }).ToArray();
        _sender.Users = _ => List(rows, total: 200, match: 200);

        var cut = Render();

        var pager = cut.Find("nav.pager");
        pager.GetAttribute("aria-label").Should().Be("Pages of people");
        Text(pager.QuerySelector(".pager-info")!).Should().Be("Showing 1–20 of 200");
        Text(pager.QuerySelector("label.pager-page-size-label")!).Should().Be("Rows per page");
        Text(pager).Should().NotContain("Page 1 of");
    }

    [Fact]
    public void U10_ANextPage_GoesToItsAddress()
    {
        var rows = Enumerable.Range(1, 20).Select(n => Zulu() with { UserId = $"p{n}", FirstName = $"Person{n}" }).ToArray();
        _sender.Users = _ => List(rows, total: 45, match: 45);
        var cut = Render();

        cut.FindAll(".pager-actions button").Single(button => Text(button) == "Next").Click();

        new Uri(Services.GetRequiredService<FakeNavigationManager>().Uri).PathAndQuery.Should().Be(PeopleLinks.UsersFiltered(page: 2));
    }

    [Theory]
    [InlineData(15)]
    [InlineData(20)]
    public void F9_NoPager_OnFifteenOrTwenty(int count)
    {
        var rows = Enumerable.Range(1, count).Select(n => Zulu() with { UserId = $"p{n}", FirstName = $"Person{n}" }).ToArray();
        _sender.Users = _ => List(rows, total: count);

        var cut = Render();

        cut.FindAll(".pager").Should().BeEmpty();
    }

    // ─── U11, U12: loading and a failed read ─────────────────────────────────

    [Fact]
    public void U11_Loading_TheSubtitleWithoutTheName_TheLoadingLine_AndNoInviteYet()
    {
        var pending = new TaskCompletionSource<UserListDto>();
        _sender.Pending = pending.Task;

        var cut = Render(wait: false);

        Text(cut.Find(".page-subtitle")).Should().Be(
            "Everyone with an account at your institution: the roles each holds, their scope, and whether they can sign in.");
        Text(cut.Find("p.loading-line[role=status]")).Should().Be("Loading the users.");
        cut.Find(".people-page").GetAttribute("aria-busy").Should().Be("true");
        cut.FindAll(".header-container a.btn-primary").Should().BeEmpty();
        cut.FindAll("[style]").Should().BeEmpty("a loading card is raw .skeleton, not Skeleton's inline size (S18)");

        pending.SetResult(List(Ten(), total: 10));
        cut.WaitForAssertion(() => cut.FindAll("table").Should().HaveCount(1), AsyncWorkTimeout);
    }

    [Fact]
    public void U12_AFailedRead_FixedWordsWithTryAgain_NeverTheExceptionsText_AndTryAgainsAnswerTakesTheFocus()
    {
        _sender.Fail = true;

        var cut = Render(waitFor: "Could not load the users.");

        Text(cut.Find(".alert-danger")).Should().Contain(
            "Could not load the users. Nothing has changed. Try again, or come back in a few minutes.");
        cut.Markup.Should().NotContain(Secret);
        Text(cut.Find(".page-subtitle")).Should().Be(
            "Everyone with an account at your institution: the roles each holds, their scope, and whether they can sign in.");
        cut.FindAll(".header-container a.btn-primary").Should().BeEmpty();

        _sender.Fail = false;
        _sender.Users = _ => List(Ten(), total: 10);
        cut.Find(".alert-danger button").Click();

        cut.WaitForAssertion(() => cut.FindAll("table").Should().HaveCount(1), AsyncWorkTimeout);
        cut.WaitForAssertion(() => JSInterop.Invocations.Should().Contain(call =>
            call.Identifier == FocusById && (string)call.Arguments[0]! == "people-heading"), AsyncWorkTimeout);
    }

    // ─── U13: the Trainee note ───────────────────────────────────────────────

    [Fact]
    public void U13_ATraineeHoldingAdmin_GetsTheNote_NoSubtitle_AndNothingIsRead()
    {
        SignInAs(WombatRoles.InstitutionalAdmin, WombatRoles.Trainee);

        var cut = RenderComponent<UsersList>();

        cut.Find("#users-trainee-note").GetAttribute("role").Should().BeNull("a standing notice is not a live region (S23)");
        cut.FindAll(".page-subtitle").Should().BeEmpty();
        cut.FindAll("form.people-filter").Should().BeEmpty();
        _sender.Received.Should().BeEmpty();
    }

    // ─── U14: the Administrator ──────────────────────────────────────────────

    [Fact]
    public void U14_TheAdministrator_HasTheInstitutionColumn_HisTwoRoles_AndHisOwnWords()
    {
        SignInAs(WombatRoles.Administrator);
        _sender.Users = _ => List(
            [Zulu(), Zulu() with { UserId = "devadmin", FirstName = "Demo", LastName = "Administrator", InstitutionId = null, InstitutionName = null, Roles = [WombatRoles.Administrator], Scope = "—" }],
            total: 18);

        var cut = Render();

        Text(cut.Find(".page-subtitle")).Should().Be(
            "Every account in Wombat: the institution, roles and scope of each, and whether they can sign in.");
        cut.FindAll("#pf-r option").Select(Text).Take(3).Should().Equal("Any role", "Administrator", "College admin");
        Text(cut.Find("caption")).Should().Be("Every institution, by last name: each person's institution, roles, scope and status.");
        cut.FindAll("thead th").Select(Text).Should().Equal("Name", "Institution", "Roles", "Scope", "Status", "Actions");
        Text(RowOf(cut, "Demo Administrator").QuerySelector("td[data-label=Institution]")!).Should().Be("Global (no institution)");
        Text(RowOf(cut, "Thandi Zulu").QuerySelector("td[data-label=Institution]")!).Should().Be(Kgk);
        _sender.Received.OfType<GetMyInstitutionNameQuery>().Should().BeEmpty("he has no institution to name");
    }

    // ─── helpers ─────────────────────────────────────────────────────────────

    private IRenderedComponent<UsersList> Render(string address = PeopleLinks.Users, bool wait = true, string? waitFor = null)
    {
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo(address);
        var cut = RenderComponent<UsersList>();
        if (wait)
        {
            cut.WaitForState(() => waitFor is null ? cut.FindAll("h2#people-heading").Count == 1 : cut.Markup.Contains(waitFor), AsyncWorkTimeout);
        }

        return cut;
    }

    private void SignInAs(params string[] roles)
    {
        _auth.SetAuthorized("mbatha@kgk.wombat.local");
        _auth.SetRoles(roles);
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "mbatha") };
        if (!roles.Contains(WombatRoles.Administrator))
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, "1"));
        }

        _auth.SetClaims(claims.ToArray());
    }

    private static IElement RowOf(IRenderedFragment cut, string name)
        => cut.FindAll("tbody tr").Single(row => row.QuerySelector("th[scope=row] .person-cell")!.ChildNodes.First().TextContent == name);

    private static string Text(IElement element) => string.Join(' ', element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static UserListDto List(IReadOnlyList<UserSummaryDto> rows, int total, int? match = null)
        => new(rows, total, match ?? rows.Count, 1, UsersFilterValues.DefaultPageSize);

    private static UserSummaryDto Zulu()
        => new("zulu", "zulu@kgk.wombat.local", "Thandi", "Zulu", 1, Kgk,
            [WombatRoles.CommitteeMember, WombatRoles.Assessor], "Paediatrics / Paediatrics", IsDeactivated: false);

    private static UserSummaryDto DuPlessis(bool ended)
        => new("duplessis", "duplessis@kgk.wombat.local", "Pieter", "du Plessis", 1, Kgk, [], "Paediatrics / Paediatrics",
            IsDeactivated: false, HasEndedProgramme: ended);

    private static UserSummaryDto Molefe()
        => DuPlessis(ended: true) with { UserId = "molefe", FirstName = "Lerato", LastName = "Molefe", Email = "molefe@kgk.wombat.local" };

    private static IReadOnlyList<UserSummaryDto> Ten()
        =>
        [
            Zulu() with { UserId = "botha", FirstName = "Sarah", LastName = "Botha", Email = "botha@kgk.wombat.local", Roles = [WombatRoles.CommitteeMember], Scope = "Paediatrics" },
            Zulu() with { UserId = "khumalo", FirstName = "Fatma", LastName = "Khumalo", Email = "khumalo@kgk.wombat.local", Roles = [WombatRoles.Assessor] },
            Zulu() with { UserId = "mbatha", FirstName = "Nolwazi", LastName = "Mbatha", Email = "mbatha@kgk.wombat.local", Roles = [WombatRoles.InstitutionalAdmin], Scope = Kgk },
            Zulu() with { UserId = "mokoena", FirstName = "Refilwe", LastName = "Mokoena", Email = "mokoena@kgk.wombat.local", Roles = [WombatRoles.SpecialityAdmin], Scope = "Paediatrics" },
            Zulu() with { UserId = "naidoo", FirstName = "David", LastName = "Naidoo", Email = "naidoo@kgk.wombat.local", Roles = [WombatRoles.CommitteeMember], Scope = "Paediatrics" },
            Zulu() with { UserId = "patel", FirstName = "Mohammed", LastName = "Patel", Email = "patel@kgk.wombat.local", Roles = [WombatRoles.Assessor] },
            Zulu() with { UserId = "sithole", FirstName = "Kabelo", LastName = "Sithole", Email = "sithole@kgk.wombat.local", Roles = [WombatRoles.SubSpecialityAdmin] },
            Zulu() with { UserId = "smit", FirstName = "Pieter", LastName = "Smit", Email = "smit@kgk.wombat.local", Roles = [WombatRoles.Coordinator], Scope = "Any speciality" },
            Zulu() with { UserId = "vanrensburg", FirstName = "John", LastName = "van Rensburg", Email = "vanrensburg@sun.wombat.local", Roles = [WombatRoles.CommitteeMember], Scope = "Any speciality" },
            Zulu()
        ];

    private sealed class FakeSender : IScopedSender
    {
        public Func<ListUsersQuery, UserListDto> Users { get; set; } = _ => new UserListDto([], 0, 0, 1, 20);

        public Task<UserListDto>? Pending { get; set; }

        public bool Fail { get; set; }

        public List<object> Received { get; } = [];

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Received.Add(request);
            switch (request)
            {
                case GetMyInstitutionNameQuery:
                    return (TResponse)(object)Kgk;
                case ListUsersQuery query:
                    if (Fail)
                    {
                        throw new InvalidOperationException(Secret);
                    }

                    var answer = Pending is { } pending ? await pending : Users(query);
                    return (TResponse)(object)answer;
                default:
                    throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
            }
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
