using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages;
using Wombat.Web.Components.Pages.Account;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Components.Pages.Admin.Assessors;
using Wombat.Web.Components.Pages.Admin.Audit;
using Wombat.Web.Components.Pages.Admin.Colleges;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Components.Pages.Admin.CurriculumProgress;
using Wombat.Web.Components.Pages.Admin.DataRights;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Pages.Admin.Jobs;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Pages.Portfolio;
using EntrustmentDecisionsPage = Wombat.Web.Components.Pages.Admin.EntrustmentDecisions.Index;

namespace Wombat.Web.Navigation;

/// <summary>Which item the navigation lights, and how (R2-Rules § 3).</summary>
public enum NavCurrent
{
    /// <summary>Not lit.</summary>
    None,

    /// <summary>The page shown is the item's own page: <c>aria-current="page"</c>.</summary>
    Page,

    /// <summary>The page shown is under the item, its owner for the acting role: <c>aria-current="true"</c>.</summary>
    Owner
}

/// <summary>One crumb of a page's trail: its words, and where it goes (none for the page itself).</summary>
public sealed record Crumb(string Label, string? Href = null);

/// <summary>
/// The owner table (T335, flow 01; R2-Rules § 3; review S9; T331): for a page the navigation does not link to, the item it
/// is under for each acting role. The shell lights that item, and the page's breadcrumbs follow it (D5).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b>A list lights itself</b>, with <c>aria-current="page"</c>: the page shown is the page of an item the acting
/// role's navigation offers (<see cref="NavItem.Page" />).</item>
/// <item><b>A page under a list lights its owner</b> for the acting role, with <c>aria-current="true"</c>: the first
/// entry of <see cref="Table" /> naming the role. At most one item is lit, and never one the navigation does not show.</item>
/// <item><b>Where the acting role has no owner, nothing is lit.</b> "Lights Home" is retired. Following a link never
/// switches the role, so a page outside the acting role's navigation opens with nothing lit.</item>
/// <item><b>An owner is named only for a role the page admits.</b> So a page drawn as Access denied, which admits none of
/// the roles held, has no owner for the acting role, and the refusal lights nothing
/// (<c>ActiveNavItemTests.EveryOwner_IsNamedOnlyForARoleThePageAdmits_AndIsInThatRolesMenu</c> holds every entry to its
/// page's <c>[Authorize]</c>).</item>
/// <item><b>Pages outside the rule</b> (<see cref="Outside" />) light nothing and draw no trail: the account pages, the
/// sign-in pages, the anonymous static pages and the failure pages. On My account the account row's name carries
/// <c>aria-current="page"</c> instead.</item>
/// </list>
/// Every routable page is a navigation item's page, in <see cref="Table" /> or in <see cref="Outside" />;
/// <c>ActiveNavItemTests</c> enumerates them and fails on one that is none of these.
/// </remarks>
public static class NavOwners
{
    /// <summary>An owner: the item a page is under, for these acting roles.</summary>
    public sealed record Owner(IReadOnlyList<string> Roles, NavItem Item);

    private static readonly string[] CatalogueAuthors = [WombatRoles.Administrator, WombatRoles.CollegeAdmin, WombatRoles.InstitutionalAdmin];
    private static readonly string[] PanelKeepers = [WombatRoles.Administrator, WombatRoles.InstitutionalAdmin, WombatRoles.SpecialityAdmin, WombatRoles.SubSpecialityAdmin];

    /// <summary>
    /// The College's own pages, under the College admin's Specialities and the Administrator's Colleges (T331: until
    /// T335 the College admin's list lit nothing, since his item's address only redirects there).
    /// </summary>
    private static readonly Owner[] UnderTheCollege =
    [
        new([WombatRoles.CollegeAdmin], NavItems.Specialities),
        new([WombatRoles.Administrator], NavItems.Colleges),
    ];

    /// <summary>
    /// Each page under a list, and its owners in order. An empty list is a page under no list: nothing is lit and its
    /// trail is Home › its title.
    /// </summary>
    public static readonly IReadOnlyDictionary<Type, IReadOnlyList<Owner>> Table = new Dictionary<Type, IReadOnlyList<Owner>>
    {
        // An activity is opened from the inbox, from My activities, from Home and from a review. Its trail follows the
        // acting role's list (D5); a Committee member, a Coordinator or an administrator has none (R2-Rules § 3).
        [typeof(ActivityView)] =
        [
            new([WombatRoles.Assessor], NavItems.ActivityInbox),
            new([WombatRoles.Trainee, WombatRoles.PendingTrainee], NavItems.MyActivities),
        ],

        // A committee review. The trainee reads theirs on My committee reviews; this page does not admit them.
        [typeof(ReviewDetail)] =
        [
            new(
                [
                    WombatRoles.CommitteeMember, WombatRoles.Coordinator, WombatRoles.SpecialityAdmin,
                    WombatRoles.SubSpecialityAdmin, WombatRoles.InstitutionalAdmin, WombatRoles.Administrator
                ],
                NavItems.CommitteeReviews),
        ],
        [typeof(PanelEdit)] = [new(PanelKeepers, NavItems.DecisionPanels)],

        // T331: the MSF report and the coverage are reached from the campaigns, though they live outside /msf/campaigns.
        [typeof(CampaignEdit)] = [new([WombatRoles.Coordinator], NavItems.MsfCampaigns)],
        [typeof(CampaignReport)] = [new([WombatRoles.Coordinator], NavItems.MsfCampaigns)],
        [typeof(ProgrammeCoverage)] = [new([WombatRoles.Coordinator], NavItems.MsfCampaigns)],

        [typeof(RequestDetail)] = [new([WombatRoles.Coordinator, WombatRoles.Administrator], NavItems.DataRightsRequests)],

        [typeof(SpecialitiesList)] = UnderTheCollege,
        [typeof(SpecialityEdit)] = UnderTheCollege,
        [typeof(SubSpecialitiesList)] = UnderTheCollege,
        [typeof(SubSpecialityEdit)] = UnderTheCollege,
        [typeof(CollegeEdit)] = [new([WombatRoles.Administrator], NavItems.Colleges)],
        [typeof(InstitutionEdit)] = [new([WombatRoles.Administrator], NavItems.Institutions)],

        [typeof(EpaEdit)] = [new(CatalogueAuthors, NavItems.Epas)],
        [typeof(ActivityTypeEdit)] = [new(CatalogueAuthors, NavItems.ActivityTypes)],
        [typeof(CurriculumItemsEdit)] = [new(CatalogueAuthors, NavItems.Curricula)],
        [typeof(CurriculumEdit)] = [new([WombatRoles.Administrator, WombatRoles.CollegeAdmin], NavItems.Curricula)],

        // Linked from Curricula, now that the Administrator's Maintenance card is dropped (R2-Shell-Admin).
        [typeof(CurriculumProgressRebuild)] = [new([WombatRoles.Administrator], NavItems.Curricula)],
        [typeof(EntrustmentScaleEdit)] = [new([WombatRoles.Administrator], NavItems.EntrustmentScales)],

        [typeof(UserDetail)] = [new([WombatRoles.Administrator, WombatRoles.InstitutionalAdmin], NavItems.Users)],
        [typeof(TraineeProfileEdit)] = [new([WombatRoles.InstitutionalAdmin], NavItems.Trainees)],
        [typeof(AssessorProfileEdit)] = [new([WombatRoles.InstitutionalAdmin], NavItems.Assessors)],
        [typeof(AuditDetail)] = [new([WombatRoles.Administrator, WombatRoles.InstitutionalAdmin], NavItems.AuditLog)],
        [typeof(ScheduledJobRunsList)] = [new([WombatRoles.Administrator], NavItems.ScheduledJobs)],

        // A trainee's STARs are part of their record, reached from Home.
        [typeof(MyAuthorisations)] = [new([WombatRoles.Trainee], NavItems.MyProgress)],

        // Reached from the Institutional admin's Home only. Flow 09 places the entrustment work; until then it is under no list.
        [typeof(EntrustmentDecisionsPage)] = [],
    };

    /// <summary>The pages outside the rule, each with why: nothing is lit on them, and they draw no trail but their own.</summary>
    public static readonly IReadOnlyDictionary<Type, string> Outside = new Dictionary<Type, string>
    {
        [typeof(Profile)] = "My account: the account row's name is the current item",
        [typeof(ChangePassword)] = "an account page: its trail is Home › My account › Change password",
        [typeof(Login)] = "a sign-in page, in the sign-in layout",
        [typeof(Logout)] = "a sign-in page, in the sign-in layout",
        [typeof(Register)] = "a sign-in page, in the sign-in layout",
        [typeof(ForgotPassword)] = "a sign-in page, in the sign-in layout",
        [typeof(LinkExternalLogin)] = "a sign-in page, in the sign-in layout",
        [typeof(MsfRespond)] = "an anonymous static page, in the sign-in layout",
        [typeof(VerifyExport)] = "an anonymous static page anyone holding a portfolio PDF opens",
        [typeof(AccessDenied)] = "a failure page",
        [typeof(NotFound)] = "a failure page",
        [typeof(Error)] = "a failure page",
    };

    /// <summary>My account: the page on which the account row's name, not a nav item, is the current one.</summary>
    public static readonly Type MyAccountPage = typeof(Profile);

    /// <summary>The trail of the change password page, the one account page that has one (R2-Rules § 3).</summary>
    private static readonly Crumb[] UnderMyAccount = [new(NavItems.Home.Label, NavItems.Home.Href), new("My account", "/account/profile")];

    /// <summary>The item the navigation lights on <paramref name="page" />, and how, for this menu and acting role.</summary>
    public static (NavItem? Item, NavCurrent Current) Lit(Type? page, string? role, NavMenuModel? menu)
    {
        if (page is null || menu is null || Outside.ContainsKey(page))
        {
            return (null, NavCurrent.None);
        }

        if (menu.Items.FirstOrDefault(item => item.Page == page) is { } own)
        {
            return (own, NavCurrent.Page);
        }

        return OwnerFor(page, role) is { } owner && menu.Offers(owner) ? (owner, NavCurrent.Owner) : (null, NavCurrent.None);
    }

    /// <summary>The item <paramref name="page" /> is under for <paramref name="role" /> in the table, or null.</summary>
    public static NavItem? OwnerFor(Type page, string? role)
        => role is not null && Table.TryGetValue(page, out var owners)
            ? owners.FirstOrDefault(owner => owner.Roles.Contains(role, StringComparer.Ordinal))?.Item
            : null;

    /// <summary>
    /// The crumbs before a page's own (D5; R2-Detail-*), or null when the page draws no trail: Home, a list the acting
    /// role's navigation offers, and the pages outside the rule. Under an owner: Home › the owner. A page with no owner for
    /// the acting role, in the table or a list the role's navigation does not offer: Home.
    /// </summary>
    /// <remarks>
    /// A list's own "Home › the list" is the trail its pages inherit; the list draws none itself, as R2-ErrorBar shows.
    /// </remarks>
    public static IReadOnlyList<Crumb>? TrailTo(Type? page, string? role, NavMenuModel? menu)
    {
        if (page is null || menu is null)
        {
            return null;
        }

        if (page == typeof(ChangePassword))
        {
            return UnderMyAccount;
        }

        if (Outside.ContainsKey(page) || page == NavItems.Home.Page)
        {
            return null;
        }

        var (item, current) = Lit(page, role, menu);
        return current switch
        {
            NavCurrent.Page => null,
            NavCurrent.Owner => [new(NavItems.Home.Label, NavItems.Home.Href), new(item!.Label, item.Href)],
            _ => [new(NavItems.Home.Label, NavItems.Home.Href)],
        };
    }
}
