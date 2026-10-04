using System.Security.Claims;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Identity;
using Wombat.Infrastructure.Identity;
using Wombat.Web.Components;
using Wombat.Web.Components.Layout;
using Wombat.Web.Components.Pages;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Application.Features.DataRights;
using Wombat.Domain.DataRights;
using Wombat.Web.Components.Pages.Admin.Colleges;
using Wombat.Web.Components.Pages.Admin.DataRights;
using Wombat.Web.Components.Pages.Admin.CurriculumProgress;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Shared;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using static Wombat.Web.Tests.Navigation.PageAccess;

namespace Wombat.Web.Tests.Navigation;

/// <summary>
/// The current item and the trail (T335, flow 01; R2-Rules § 3; review S9; T331; D5). A list lights itself with
/// <c>aria-current="page"</c>; a page under it lights its owner for the acting role with <c>aria-current="true"</c>; at most
/// one item is lit; a page with no owner for the acting role lights nothing. The page's breadcrumbs follow the same owner.
/// </summary>
public sealed class ActiveNavItemTests : TestContext
{
    public ActiveNavItemTests()
    {
        // NavMenu scrolls its lit item into view after it renders (wombat.revealCurrentNavItem).
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>Every acting role, and none (a former trainee), with the role held alone.</summary>
    public static TheoryData<string?> EveryActingRole()
    {
        var data = new TheoryData<string?> { (string?)null };
        foreach (var role in WombatRoles.All)
        {
            data.Add(role);
        }

        return data;
    }

    // ---- the registry covers every page ----

    [Fact]
    public void EveryRoutablePage_IsANavItemsPage_OrInTheOwnerTable_OrOutsideTheRule()
    {
        var itemPages = AllItems().Select(item => item.Page).ToHashSet();

        Pages.Select(page => page.Page).Distinct()
            .Where(page => !itemPages.Contains(page) && !NavOwners.Table.ContainsKey(page) && !NavOwners.Outside.ContainsKey(page))
            .Select(page => page.FullName)
            .Should().BeEmpty("a new page says which nav item it is under, or that it is under none (NavOwners)");
    }

    [Fact]
    public void TheRegistry_NamesOnlyRoutablePages_AndNoPageTwice()
    {
        var routable = Pages.Select(page => page.Page).ToHashSet();

        NavOwners.Table.Keys.Concat(NavOwners.Outside.Keys).Should().OnlyContain(page => routable.Contains(page));
        NavOwners.Table.Keys.Intersect(NavOwners.Outside.Keys).Should().BeEmpty();
        NavOwners.Table.Keys.Intersect(AllItems().Select(item => item.Page)).Should().BeEmpty(
            "a nav item's own page lights that item; the table is for the pages under one");
    }

    [Fact]
    public async Task EveryOwner_IsNamedOnlyForARoleThePageAdmits_AndIsInThatRolesMenu()
    {
        // So a page drawn as Access denied (it admits none of the roles held) never has an owner for the acting role, and
        // the refusal lights nothing (R2-Rules § 3: the failure pages light nothing).
        foreach (var (page, owners) in NavOwners.Table)
        {
            foreach (var owner in owners)
            {
                foreach (var role in owner.Roles)
                {
                    (await RefusalOf(page, Holder([role]))).Should().BeNull($"{page.Name} is under {owner.Item.Label} for a {role}");
                    MenuFor(role, TraineeRecord()).Offers(owner.Item).Should().BeTrue($"a {role}'s menu shows {owner.Item.Label}");
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(EveryActingRole))]
    public async Task APageTheActingRoleCannotOpen_LightsNothing(string? role)
    {
        // Access denied renders in place (D6), at the address of the page that refused, under its RouteData.
        var held = role is null ? Array.Empty<string>() : [role];
        foreach (var page in Pages.Select(page => page.Page).Distinct())
        {
            if (await RefusalOf(page, Holder(held)) is null)
            {
                continue;
            }

            NavOwners.Lit(page, role, MenuFor(role)).Current.Should().Be(NavCurrent.None, $"{page.Name} refuses a {role ?? "person with no role"}");
        }
    }

    // ---- at most one item, and aria-current ----

    [Theory]
    [MemberData(nameof(EveryActingRole))]
    public void OnEveryPage_AtMostOneItemIsLit_AndOnlyItCarriesAriaCurrent(string? role)
    {
        foreach (var page in Pages.Select(page => page.Page).Distinct())
        {
            var cut = RenderNav(role, page);

            var lit = cut.FindAll("a.active");
            lit.Should().HaveCountLessThanOrEqualTo(1, $"{page.Name}, acting as {role ?? "no role"}");
            cut.FindAll("a[aria-current]").Should().BeEquivalentTo(lit, "only the lit item says it is current");

            if (lit.Count == 1)
            {
                var item = AllItems().Single(item => item.Href == lit.First().GetAttribute("href"));
                lit.First().GetAttribute("aria-current").Should().Be(item.Page == page ? "page" : "true",
                    $"on {page.Name}, {item.Label} is {(item.Page == page ? "the list itself" : "its owner")}");
            }
        }
    }

    [Theory]
    [MemberData(nameof(EveryActingRole))]
    public void EveryItemInTheMenu_LightsItself_OnItsOwnPage(string? role)
    {
        foreach (var item in MenuFor(role).Items)
        {
            var lit = RenderNav(role, item.Page).FindAll("a.active").Should().ContainSingle($"{item.Label}'s own page").Which;
            lit.GetAttribute("href").Should().Be(item.Href);
            lit.GetAttribute("aria-current").Should().Be("page");
        }
    }

    // ---- the owner table's rows (R2-Rules § 3) and T331 ----

    [Theory]
    [InlineData(WombatRoles.Assessor, "Activity inbox")]
    [InlineData(WombatRoles.Trainee, "My activities")]
    [InlineData(WombatRoles.PendingTrainee, "My activities")]
    [InlineData(WombatRoles.CommitteeMember, null)]
    [InlineData(WombatRoles.Coordinator, null)]
    [InlineData(WombatRoles.Administrator, null)]
    public void AnActivity_LightsTheActingRolesList(string role, string? owner)
        => LitLabel(role, typeof(ActivityView)).Should().Be(owner);

    [Theory]
    [InlineData(WombatRoles.CollegeAdmin, "Specialities")]
    [InlineData(WombatRoles.Administrator, "Colleges")]
    public void ACollegesSpecialities_LightSpecialitiesForItsAdmin_AndCollegesForTheAdministrator(string role, string owner)
    {
        // T331: the College admin's Specialities item only redirects to /admin/colleges/{id}/specialities, so the prefix
        // match lit nothing there. One item, never two: the Administrator has no Specialities item.
        var cut = RenderNav(role, typeof(SpecialitiesList));

        var lit = cut.FindAll("a.active").Should().ContainSingle().Which;
        lit.TextContent.Trim().Should().Be(owner);
        lit.GetAttribute("aria-current").Should().Be("true");
    }

    [Theory]
    [InlineData(typeof(CampaignReport))]
    [InlineData(typeof(ProgrammeCoverage))]
    [InlineData(typeof(CampaignEdit))]
    public void TheMsfReportAndCoverage_LightMsfCampaigns_ForACoordinator(Type page)
        => LitLabel(WombatRoles.Coordinator, page).Should().Be("MSF campaigns", "T331: they are reached from the campaigns");

    [Theory]
    [InlineData(WombatRoles.CommitteeMember)]
    [InlineData(WombatRoles.Coordinator)]
    [InlineData(WombatRoles.SpecialityAdmin)]
    [InlineData(WombatRoles.InstitutionalAdmin)]
    [InlineData(WombatRoles.Administrator)]
    public void ACommitteeReview_LightsCommitteeReviews(string role)
        => LitLabel(role, typeof(ReviewDetail)).Should().Be("Committee reviews");

    [Fact]
    public void CurriculumProgress_LightsCurricula_ForTheAdministrator()
        => LitLabel(WombatRoles.Administrator, typeof(CurriculumProgressRebuild)).Should().Be("Curricula");

    // ---- a page under a personal link (T355, E3) ----

    /// <summary>
    /// The EPA page is under My progress whatever the acting role, and for a graduate who holds none: My progress is a
    /// personal link, offered to whoever holds the Trainee role or a trainee record, so a per-role entry would light nothing
    /// for a registrar acting as Assessor or for a graduate (E3).
    /// </summary>
    public static TheoryData<string?, bool> TraineeRecordHolders() => new()
    {
        { WombatRoles.Trainee, false },
        { WombatRoles.Assessor, true },
        { null, true }
    };

    [Theory]
    [MemberData(nameof(TraineeRecordHolders))]
    public void TheEpaPage_LightsMyProgress_WhateverTheActingRole_AndWithNone(string? role, bool withRecord)
    {
        var menu = withRecord ? MenuFor(role, TraineeRecord()) : MenuFor(role);

        NavOwners.Lit(typeof(EpaProgress), role, menu).Should().Be((NavItems.MyProgress, NavCurrent.Owner));
        var cut = RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(ActingAs(role))
            .AddCascadingValue(menu)
            .AddCascadingValue(new RouteData(typeof(EpaProgress), new Dictionary<string, object?>())));
        Lit(cut).Should().Equal(("My progress", "true"));
    }

    [Theory]
    [MemberData(nameof(TraineeRecordHolders))]
    public void TheEpaPagesTrail_IsHome_MyProgress_ThenItsCode(string? role, bool withRecord)
    {
        var cut = RenderComponent<PageHeader>(parameters => parameters
            .Add(header => header.Title, "PAED-001 — Providing paediatric emergency care to children")
            .Add(header => header.CurrentCrumb, "PAED-001")
            .AddCascadingValue(ActingAs(role))
            .AddCascadingValue(withRecord ? MenuFor(role, TraineeRecord()) : MenuFor(role))
            .AddCascadingValue(new RouteData(typeof(EpaProgress), new Dictionary<string, object?>())));

        Crumbs(cut).Should().Equal(("Home", "/"), ("My progress", "/portfolio/progress"), ("PAED-001", null));
    }

    [Fact]
    public void AnEntryNamingRoles_UnderAPersonalLink_KeepsItsRoles()
    {
        // T355, build review R7, G5: the any-role rule is for an entry declared UnderAPersonalLink, not for every entry
        // whose item is personal. My authorisations is under My progress for the Trainee alone, as its policy admits.
        NavOwners.OwnerFor(typeof(MyAuthorisations), WombatRoles.Trainee).Should().Be(NavItems.MyProgress);
        NavOwners.OwnerFor(typeof(MyAuthorisations), WombatRoles.Assessor).Should().BeNull();
        NavOwners.OwnerFor(typeof(MyAuthorisations), null).Should().BeNull();
        NavOwners.OwnerFor(typeof(EpaProgress), WombatRoles.Assessor).Should().Be(NavItems.MyProgress);
        NavOwners.OwnerFor(typeof(EpaProgress), null).Should().Be(NavItems.MyProgress);
    }

    [Fact]
    public async Task APageUnderAPersonalLink_AdmitsWhomTheLinkIsOfferedTo()
    {
        // The rule names no role, so EveryOwner_IsNamedOnlyForARoleThePageAdmits has none to check: the page's policy,
        // TraineeOrFormerTrainee, is held to My progress's offer here instead (T252).
        foreach (var (page, owners) in NavOwners.Table.Where(entry => entry.Value.Any(owner => NavOwners.IsPersonal(owner.Item))))
        {
            foreach (var owner in owners.Where(owner => owner.Roles.Count == 0))
            {
                (await RefusalOf(page, Holder([WombatRoles.Trainee]))).Should().BeNull($"{page.Name} admits a Trainee");
                (await RefusalOf(page, Holder([WombatRoles.Assessor], TraineeRecord()))).Should().BeNull($"{page.Name} admits a record holder");
                (await RefusalOf(page, Holder([], TraineeRecord()))).Should().BeNull($"{page.Name} admits a graduate with no role");
                (await RefusalOf(page, Holder([WombatRoles.Assessor]))).Should().NotBeNull($"{page.Name} refuses an Assessor with no record");
                MenuFor(WombatRoles.Assessor).Offers(owner.Item).Should().BeFalse("nor is its link offered to them");
            }
        }
    }

    [Theory]
    [InlineData(typeof(Profile))]
    [InlineData(typeof(ChangePassword))]
    [InlineData(typeof(AccessDenied))]
    [InlineData(typeof(NotFound))]
    [InlineData(typeof(Error))]
    public void MyAccount_ChangePassword_AndTheFailurePages_LightNothing(Type page)
    {
        foreach (var role in WombatRoles.All)
        {
            RenderNav(role, page).FindAll("a.active, a[aria-current]").Should().BeEmpty($"{page.Name}, acting as {role}");
        }
    }

    [Fact]
    public void APageOutsideTheActingRolesMenu_LightsNothing_WhateverAnotherHeldRoleWouldLight()
    {
        // Dr Zulu, acting as Committee member, opens a Mini-CEX (her Assessor role admits it): nothing is lit, and following
        // the link switched nothing (R2-Rules § 1, R2-Detail-Email's "after Switch back").
        var zulu = ActingRoleResolver.Resolve(WombatRoles.CommitteeMember, [WombatRoles.CommitteeMember, WombatRoles.Assessor]);
        var user = Holder([WombatRoles.CommitteeMember, WombatRoles.Assessor]);

        NavOwners.Lit(typeof(ActivityView), zulu.Role, NavItems.For(zulu, user)).Current.Should().Be(NavCurrent.None);
    }

    [Fact]
    public void NoRoute_LightsNothing()
        => RenderComponent<NavMenu>(parameters => parameters
                .AddCascadingValue(ActingAs(WombatRoles.Assessor))
                .AddCascadingValue(MenuFor(WombatRoles.Assessor)))
            .FindAll("a.active").Should().BeEmpty();

    // ---- the trail (D5; R2-Detail-*) ----

    [Fact]
    public void UnderAnOwner_TheTrailIsHome_TheOwner_ThenThePage_WhichIsCurrent()
    {
        var cut = RenderHeader(WombatRoles.Assessor, typeof(ActivityView), "Mini-CEX for Nomsa Mahlangu");

        var trail = cut.Find("nav[aria-label='Breadcrumb']");
        trail.NextElementSibling!.ClassList.Should().Contain("header-container", "the trail is above the header");
        Crumbs(cut).Should().Equal(("Home", "/"), ("Activity inbox", "/activities/inbox"), ("Mini-CEX for Nomsa Mahlangu", null));
        trail.QuerySelector("li:last-child [aria-current='page']")!.TextContent.Should().Be("Mini-CEX for Nomsa Mahlangu");
        trail.QuerySelectorAll("[aria-current]").Should().ContainSingle();
    }

    [Fact]
    public void WithNoOwnerForTheActingRole_TheTrailIsHome_ThenThePage()
        => Crumbs(RenderHeader(WombatRoles.CommitteeMember, typeof(ActivityView), "Mini-CEX for Anele Dlamini"))
            .Should().Equal(("Home", "/"), ("Mini-CEX for Anele Dlamini", null));

    [Fact]
    public void ChangePassword_IsUnderMyAccount()
        => Crumbs(RenderHeader(WombatRoles.Trainee, typeof(ChangePassword), "Change password"))
            .Should().Equal(("Home", "/"), ("My account", "/account/profile"), ("Change password", null));

    [Theory]
    [InlineData(typeof(Home), "Home")]
    [InlineData(typeof(ActivityInbox), "Activity inbox")]
    [InlineData(typeof(Profile), "My account")]
    [InlineData(typeof(AccessDenied), "Access denied")]
    [InlineData(typeof(NotFound), "Page not found")]
    [InlineData(typeof(Error), "Something went wrong")]
    public void Home_AListOfTheActingRoles_MyAccount_AndTheFailurePages_DrawNoTrail(Type page, string title)
        => RenderHeader(WombatRoles.Assessor, page, title).FindAll("nav[aria-label='Breadcrumb']").Should().BeEmpty();

    [Fact]
    public void AccessDenied_DrawnInPlaceOfARefusedPage_DrawsNoTrail()
    {
        // A Committee member at /committee/panels/5 (PanelEdit refuses him): the address routes to PanelEdit, whose owner
        // for his role would be Decision panels; the header names the page it heads.
        var cut = RenderHeader(WombatRoles.CommitteeMember, typeof(PanelEdit), "You cannot open this page", page: typeof(AccessDenied));

        cut.FindAll("nav[aria-label='Breadcrumb']").Should().BeEmpty();
    }

    [Fact]
    public void InTheRealRoutes_AccessDeniedAtARefusedAddress_LightsNothing_AndDrawsNoTrail()
    {
        // Dr Botha, a Committee member, types /committee/panels/5: the panel editor refuses her role, and its owner for the
        // roles it admits is Decision panels, which her menu also shows.
        Services.AddWombatCircuitServices();
        Services.AddSingleton<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>(
            new Microsoft.AspNetCore.DataProtection.EphemeralDataProtectionProvider());
        Services.AddActingRoleSwitch();
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("botha@kgk.wombat.local");
        auth.SetRoles(WombatRoles.CommitteeMember);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "botha"));
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo("/committee/panels/5");

        var cut = RenderComponent<Routes>(parameters => parameters.Add(routes => routes.Acting, ActingAs(WombatRoles.CommitteeMember)));

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("You cannot open this page"));
        cut.FindAll("a.nav-link").Select(a => a.TextContent.Trim()).Should().Contain("Decision panels", "guard: her menu shows the owner");
        cut.FindAll("a.active, a[aria-current]").Should().BeEmpty();
        cut.FindAll("nav[aria-label='Breadcrumb']").Should().BeEmpty();
    }

    // The review of the t335 branch: every test above cascades a RouteData by hand. Here the real Routes renders, signed in,
    // and its own cascade of the router's RouteData is what lights the nav and draws the trail, again at each navigation in
    // the circuit. Without that cascade Home is never lit and a page under a list has no trail. Changed deliberately by
    // T339 (flow 02): the page under a list was Change password, which is static now and never reached in a circuit, so it
    // is a new curriculum, under Curricula. Its reads fail here, which draws its header and trail all the same.
    [Fact]
    public void InTheRealRoutes_TheLitItemAndTheTrail_FollowEachNavigationInTheCircuit()
    {
        Services.AddSingleton<IScopedSender>(new FailingSender());
        Services.AddSingleton(TimeProvider.System);
        Services.AddWombatCircuitServices();
        Services.AddSingleton<Microsoft.AspNetCore.DataProtection.IDataProtectionProvider>(
            new Microsoft.AspNetCore.DataProtection.EphemeralDataProtectionProvider());
        Services.AddActingRoleSwitch();
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("m.dube@cmsa.wombat.local");
        auth.SetRoles(WombatRoles.CollegeAdmin);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "dube"));
        auth.SetPolicies("AdministratorOrCollegeAdmin"); // the new curriculum's page asks for it
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        navigation.NavigateTo("/");

        // The College admin's Home reads nothing (CollegeAdminDashboard), so the page is whole at once.
        var cut = RenderComponent<Routes>(parameters => parameters.Add(routes => routes.Acting, ActingAs(WombatRoles.CollegeAdmin)));

        cut.WaitForAssertion(() => cut.Find("main h1").TextContent.Trim().Should().Be("Home"));
        Lit(cut).Should().Equal([("Home", "page")], "Home is the page shown, and an item of the menu");
        cut.FindAll("nav[aria-label='Breadcrumb']").Should().BeEmpty("Home draws no trail");

        navigation.NavigateTo("/admin/curricula/new");

        cut.WaitForAssertion(() => cut.Find("main h1").TextContent.Trim().Should().Be("Create curriculum"));
        Lit(cut).Should().Equal([("Curricula", "true")], "the page is under Curricula, its owner");
        cut.FindAll("nav[aria-label='Breadcrumb'] li").Select(li => (li.TextContent.Trim(), li.QuerySelector("a")?.GetAttribute("href")))
            .Should().Equal(("Home", "/"), ("Curricula", "/admin/curricula"), ("Create curriculum", null));

        navigation.NavigateTo("/");

        cut.WaitForAssertion(() => cut.Find("main h1").TextContent.Trim().Should().Be("Home"));
        Lit(cut).Should().Equal([("Home", "page")], "back on Home, Home is lit again");
        cut.FindAll("nav[aria-label='Breadcrumb']").Should().BeEmpty();
    }

    // R2-Rules § 3: "Home › Data rights requests › the request (its id is shown here)". The review of the t335 branch: the
    // page passed no crumb, so its trail ended "Data rights request" whichever request it showed.
    [Fact]
    public void ADataRightsRequest_IsNamedInItsTrailByItsId_OnceItHasLoaded()
    {
        var id = Guid.Parse("3f2a9c01-b7d8-4e6f-9a0b-1c2d3e4f5061");
        var load = new TaskCompletionSource<DataRightsRequestDto>();
        Services.AddSingleton<IScopedSender>(new LoadingSender(load.Task));
        var auth = this.AddTestAuthorization();
        auth.SetAuthorized("coordinator@kgk.wombat.local");
        auth.SetRoles(WombatRoles.Coordinator);
        auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "coordinator"));

        var cut = RenderComponent<RequestDetail>(parameters => parameters
            .Add(page => page.Id, id)
            .AddCascadingValue(ActingAs(WombatRoles.Coordinator))
            .AddCascadingValue(MenuFor(WombatRoles.Coordinator))
            .AddCascadingValue(new RouteData(typeof(RequestDetail), new Dictionary<string, object?>())));

        TrailOf(cut).Should().Equal([("Home", "/"), ("Data rights requests", "/admin/data-rights"), ("Data rights request", null)],
            "while it loads, the page's title");

        load.SetResult(new DataRightsRequestDto(id, "requester-1", "Nomsa Mahlangu", new DateTime(2026, 9, 20, 8, 0, 0, DateTimeKind.Utc),
            DataRightsRequestType.Access, DataRightsRequestStatus.Submitted, "My record, please.", null, null, null, null));

        cut.WaitForAssertion(() => TrailOf(cut).Should().Equal(
            [("Home", "/"), ("Data rights requests", "/admin/data-rights"), ("3f2a9c01-b7d8-4e6f-9a0b-1c2d3e4f5061", null)]),
            Wombat.Web.Tests.TestSupport.WombatTestContext.AsyncWorkTimeout);
        cut.Find("nav[aria-label='Breadcrumb'] [aria-current='page']").TextContent.Trim().Should().Be(id.ToString());
        cut.Find("h1").TextContent.Trim().Should().Be("Data rights request", "the heading stays the page's");
    }

    [Fact]
    public void AListOutsideTheActingRolesMenu_IsUnderHome()
        => Crumbs(RenderHeader(WombatRoles.Coordinator, typeof(ActivityInbox), "Activity inbox"))
            .Should().Equal(("Home", "/"), ("Activity inbox", null));

    [Fact]
    public void APageAddsTheCrumbsTheOwnerTableNames_BetweenTheOwnerAndItself()
    {
        // R2-Rules § 3: Home › Colleges › the College › Specialities, for the Administrator.
        var cut = RenderHeader(WombatRoles.Administrator, typeof(SpecialitiesList), "Specialities",
            trail: [new Crumb("College of Paediatricians", "/admin/colleges/2")]);

        Crumbs(cut).Should().Equal(
            ("Home", "/"), ("Colleges", "/admin/colleges"), ("College of Paediatricians", "/admin/colleges/2"), ("Specialities", null));
    }

    [Fact]
    public void AtPhoneWidth_TheTrailFoldsToItsParent_OneLinkWithABackArrow()
    {
        var cut = RenderHeader(WombatRoles.CommitteeMember, typeof(ReviewDetail), "Paediatrics review, Semester 2, 2026");

        var parent = cut.Find("li.breadcrumb-parent a");
        parent.TextContent.Trim().Should().Be("Committee reviews");
        parent.QuerySelector("svg.breadcrumb-back use")!.GetAttribute("href").Should().Be("/icons/chevron-left.svg#i");
        cut.FindAll("li.breadcrumb-ancestor").Select(li => li.TextContent.Trim()).Should().Equal("Home");

        var css = Wombat.Web.Tests.Design.Stylesheet.AppCss();
        const string phone = "@media (max-width: 640.98px)";
        // As specific as ".breadcrumbs li", which displays every crumb: a bare class lost to it in the browser.
        css.Rules.Should().Contain(rule => rule.AtRule == phone && rule.Selectors.Contains(".breadcrumbs li.breadcrumb-ancestor")
            && rule.Selectors.Contains(".breadcrumbs li.breadcrumb-current") && rule.Selectors.Contains(".breadcrumbs .breadcrumb-separator")
            && rule.Value("display") == "none");
        css.Rules.Should().Contain(rule => rule.AtRule == phone && rule.Selector == ".breadcrumb-parent a"
            && rule.Value("min-height") == "44px");
    }

    // ---- the lit item scrolled into view is decoration ----

    // The review of the t335 branch: only JSDisconnectedException was caught, so a script error in the scroll, or the call
    // cancelled as the circuit wound down, went unhandled out of OnAfterRenderAsync and ended the circuit.
    public static TheoryData<string> FailuresOfTheScroll() => ["script error", "cancelled", "disconnected"];

    [Theory]
    [MemberData(nameof(FailuresOfTheScroll))]
    public void AScrollThatFails_LeavesTheMenuStanding(string failure)
    {
        JSInterop.Mode = JSRuntimeMode.Strict;
        var scroll = JSInterop.SetupVoid("wombat.revealCurrentNavItem");
        Exception thrown = failure switch
        {
            "script error" => new Microsoft.JSInterop.JSException("Cannot read properties of null (reading 'scrollIntoView')"),
            "cancelled" => new TaskCanceledException("The circuit is going."),
            _ => new Microsoft.JSInterop.JSDisconnectedException("The circuit has gone."),
        };
        scroll.SetException(thrown);

        var cut = RenderNav(WombatRoles.Assessor, typeof(ActivityInbox));

        cut.WaitForAssertion(() => JSInterop.VerifyInvoke("wombat.revealCurrentNavItem"));
        Renderer.UnhandledException.IsCompleted.Should().BeFalse("scrolling is decoration: its failure must not reach the renderer");
        cut.Find("a.active").TextContent.Trim().Should().Be("Activity inbox", "the menu still stands, its item lit");
    }

    // ---- helpers ----

    private IRenderedComponent<NavMenu> RenderNav(string? role, Type page)
        => RenderComponent<NavMenu>(parameters => parameters
            .AddCascadingValue(ActingAs(role))
            .AddCascadingValue(MenuFor(role))
            .AddCascadingValue(new RouteData(page, new Dictionary<string, object?>())));

    private IRenderedComponent<PageHeader> RenderHeader(string role, Type route, string title, IReadOnlyList<Crumb>? trail = null, Type? page = null)
        => RenderComponent<PageHeader>(parameters => parameters
            .Add(header => header.Title, title)
            .Add(header => header.Trail, trail)
            .Add(header => header.Page, page)
            .AddCascadingValue(ActingAs(role))
            .AddCascadingValue(MenuFor(role))
            .AddCascadingValue(new RouteData(route, new Dictionary<string, object?>())));

    private string? LitLabel(string role, Type page) => RenderNav(role, page).FindAll("a.active").SingleOrDefault()?.TextContent.Trim();

    private static List<(string Label, string? Href)> TrailOf(IRenderedFragment cut)
        => cut.FindAll("nav[aria-label='Breadcrumb'] li")
            .Select(li => (li.TextContent.Trim(), li.QuerySelector("a")?.GetAttribute("href")))
            .ToList();

    /// <summary>Answers a data-rights request's read with <paramref name="load" />, when the test lets it.</summary>
    private sealed class LoadingSender(Task<DataRightsRequestDto> load) : IScopedSender
    {
        public async Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => request is Wombat.Application.Features.DataRights.Queries.GetDataRightsRequestByIdQuery
                ? (TResponse)(object)await load
                : throw new InvalidOperationException($"The page sent {request.GetType().Name}.");

        public Task Send(MediatR.IRequest request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException($"The page sent {request.GetType().Name}.");
    }

    /// <summary>Every item of the main navigation that carries <c>aria-current</c>: its label, and the value.</summary>
    private static List<(string Label, string? Current)> Lit(IRenderedFragment cut)
        => cut.FindAll("nav[aria-label='Main'] a[aria-current]")
            .Select(link => (link.TextContent.Trim(), link.GetAttribute("aria-current")))
            .ToList();

    private static List<(string Label, string? Href)> Crumbs(IRenderedComponent<PageHeader> cut)
        => cut.FindAll("nav[aria-label='Breadcrumb'] li")
            .Select(li => (li.TextContent.Trim(), li.QuerySelector("a")?.GetAttribute("href")))
            .ToList();

    private static ActingRole ActingAs(string? role) => role is null ? ActingRole.None : ActingRoleResolver.Resolve(role, [role]);

    private static NavMenuModel MenuFor(string? role, params Claim[] claims)
        => NavItems.For(ActingAs(role), Holder(role is null ? [] : [role], claims));

    private static List<NavItem> AllItems()
        => WombatRoles.All.SelectMany(role => MenuFor(role, TraineeRecord()).Items).Distinct().ToList();

    private static ClaimsPrincipal Holder(string[] roles, params Claim[] claims)
        => new(new ClaimsIdentity(
            roles.SelectMany(role => PrincipalFor(role).Claims)
                .Where(claim => claim.Type != ClaimTypes.NameIdentifier)
                .DistinctBy(claim => (claim.Type, claim.Value))
                .Prepend(new Claim(ClaimTypes.NameIdentifier, "user"))
                .Concat(claims),
            authenticationType: "Test"));

    private static Claim TraineeRecord() => new(WombatClaims.TraineeRecord, "true");

    /// <summary>Fails every read: a page draws its header and trail whatever its reads do.</summary>
    private sealed class FailingSender : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(MediatR.IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("No reads here.");

        public Task Send(MediatR.IRequest request, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("No commands here.");
    }
}