using System.Security.Claims;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages;
using Wombat.Web.Components.Pages.Activities;
using Wombat.Web.Components.Pages.Admin.ActivityTypes;
using Wombat.Web.Components.Pages.Admin.Adoptions;
using Wombat.Web.Components.Pages.Admin.Assessors;
using Wombat.Web.Components.Pages.Admin.Audit;
using Wombat.Web.Components.Pages.Admin.Colleges;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Components.Pages.Admin.DataRights;
using Wombat.Web.Components.Pages.Admin.EntrustmentScales;
using Wombat.Web.Components.Pages.Admin.Epas;
using Wombat.Web.Components.Pages.Admin.Institutions;
using Wombat.Web.Components.Pages.Admin.Invitations;
using Wombat.Web.Components.Pages.Admin.Jobs;
using Wombat.Web.Components.Pages.Admin.Sso;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Components.Pages.CommitteeDecisions;
using Wombat.Web.Components.Pages.MultiSourceFeedback;
using Wombat.Web.Components.Pages.Portfolio;
using Wombat.Web.Components.Pages.Profile;

namespace Wombat.Web.Navigation;

/// <summary>
/// One page the navigation offers (T335, flow 01; R2-Rules, A-Spec). Its label is its page's heading and the stem of its
/// tab, in sentence case (D10). <see cref="Page" /> is the component its address opens, which is how the shell knows the
/// item is current (<see cref="NavOwners" />).
/// </summary>
/// <param name="Href">The address, a page's route.</param>
/// <param name="Label">What the link says: its page's heading.</param>
/// <param name="Icon">An <c>Icon.razor</c> name, a file under <c>wwwroot/icons</c>.</param>
/// <param name="Page">The routed component <paramref name="Href" /> opens.</param>
public sealed record NavItem(string Href, string Label, string Icon, Type Page);

/// <summary>Links under a heading that is not a link, or, with no heading, the list's ungrouped head.</summary>
public sealed record NavGroup(string? Heading, IReadOnlyList<NavItem> Items);

/// <summary>
/// What the navigation shows one signed-in person: their acting role's links, Home first, then the personal links under a
/// rule. Never the union of their roles (R2-Rules § 1).
/// </summary>
public sealed record NavMenuModel(IReadOnlyList<NavGroup> Groups, IReadOnlyList<NavItem> Personal)
{
    /// <summary>Every link, in the order the menu shows them.</summary>
    public IEnumerable<NavItem> Items => Groups.SelectMany(group => group.Items).Concat(Personal);

    /// <summary>Whether the menu offers <paramref name="item" />.</summary>
    public bool Offers(NavItem? item) => item is not null && Items.Contains(item);
}

/// <summary>
/// The navigation, written out once (T178's one declaration per page, kept): every item, and which of them each acting role
/// is offered, grouped. DESIGN.md § The NavMenu tabulates it and <c>NavMenuAuthorizationTests</c> holds the two together.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><b>One role at a time.</b> The acting role's links, never the union of the roles held: access is the union and
/// never changes, so a page outside the acting role's links still opens (R2-Rules § 1).</item>
/// <item><b>Grouped only when long.</b> Up to <see cref="FlatLimit" /> links, Home and the personal links counted, the list
/// is flat. Longer (the Administrator's 17 and the Institutional admin's 16), it is grouped under headings that are not
/// links.</item>
/// <item><b>The personal links</b>, last and under a rule, belong to the person rather than the role, so they are shown
/// whatever the role: My progress to anyone holding a trainee record (the claim sign-in issues, T252, D8) or the Trainee
/// role, a graduate acting as Assessor included; My data rights to everyone signed in.</item>
/// <item><b>Nothing unbuilt.</b> The nav never links to a page that does not exist. The five placeholders (Recent
/// activities, Stalled activities, Programme trainees, STAR review queue, System) went with the placeholder page: flows
/// 06 and 09 design the ones kept, and the others were dropped (the flow 01 pick).</item>
/// <item><b>No Sign out and no My account:</b> the account row carries both.</item>
/// </list>
/// </remarks>
public static class NavItems
{
    /// <summary>Up to this many links in all, the list is flat; more, it is grouped.</summary>
    public const int FlatLimit = 8;

    public static readonly NavItem Home = new("/", "Home", "home", typeof(Home));

    // The trainee's own work.
    public static readonly NavItem LogAnActivity = new("/activities/new", "Log an activity", "circle-plus", typeof(NewActivity));
    public static readonly NavItem MyActivities = new("/activities/mine", "My activities", "list", typeof(MyActivities));
    public static readonly NavItem MsfReports = new("/msf/my-reports", "MSF reports", "message-square", typeof(MyMsfReports));
    public static readonly NavItem MyCommitteeReviews = new("/committee/my-reviews", "My committee reviews", "file-text", typeof(MyReviews));
    public static readonly NavItem ExportPortfolio = new("/portfolio/export", "Export portfolio", "download", typeof(Components.Pages.Portfolio.ExportPortfolio));

    // Assessing and reviewing.
    public static readonly NavItem ActivityInbox = new("/activities/inbox", "Activity inbox", "inbox", typeof(ActivityInbox));
    public static readonly NavItem DecisionsDue = new("/committee/decisions-due", "Decisions due", "calendar-check", typeof(DecisionsDue));
    public static readonly NavItem CommitteeReviews = new("/committee/reviews", "Committee reviews", "file-text", typeof(ReviewsSchedule));
    public static readonly NavItem DecisionPanels = new("/committee/panels", "Decision panels", "scale", typeof(PanelsList));
    public static readonly NavItem MsfCampaigns = new("/msf/campaigns", "MSF campaigns", "message-square", typeof(CampaignsList));

    // The catalogue.
    public static readonly NavItem Specialities = new("/admin/specialities", "Specialities", "stethoscope", typeof(MySpecialitiesRedirect));
    public static readonly NavItem Epas = new("/admin/epas", "EPAs", "list-checks", typeof(EpasList));
    public static readonly NavItem Curricula = new("/admin/curricula", "Curricula", "book-open", typeof(CurriculaList));
    public static readonly NavItem CurriculumAdoptions = new("/admin/adoptions", "Curriculum adoptions", "book-check", typeof(AdoptionsList));
    public static readonly NavItem ActivityTypes = new("/admin/activity-types", "Activity types", "clipboard-list", typeof(ActivityTypesList));
    public static readonly NavItem EntrustmentScales = new("/admin/entrustment-scales", "Entrustment scales", "gauge", typeof(EntrustmentScalesList));

    // People and organisations.
    public static readonly NavItem Institutions = new("/admin/institutions", "Institutions", "building", typeof(InstitutionsList));
    public static readonly NavItem Colleges = new("/admin/colleges", "Colleges", "graduation-cap", typeof(CollegesList));
    public static readonly NavItem Users = new("/admin/users", "Users", "users", typeof(UsersList));
    public static readonly NavItem Invitations = new("/admin/invitations", "Invitations", "mail", typeof(InvitationsList));
    public static readonly NavItem Trainees = new("/admin/trainees", "Trainees", "user-round", typeof(PendingTraineesList));
    public static readonly NavItem Assessors = new("/admin/assessors", "Assessors", "user-check", typeof(AssessorsList));

    // The platform.
    public static readonly NavItem ScheduledJobs = new("/admin/jobs", "Scheduled jobs", "clock", typeof(ScheduledJobsList));
    public static readonly NavItem AuditLog = new("/admin/audit", "Audit log", "history", typeof(AuditList));
    public static readonly NavItem SsoMappings = new("/admin/sso/group-mappings", "SSO mappings", "key", typeof(GroupMappings));
    public static readonly NavItem DataRightsRequests = new("/admin/data-rights", "Data rights requests", "shield-check", typeof(RequestsList));

    // The personal links.
    public static readonly NavItem MyProgress = new("/portfolio/progress", "My progress", "trending-up", typeof(MyProgress));
    public static readonly NavItem MyDataRights = new("/account/data-rights", "My data rights", "lock", typeof(DataRights));

    /// <summary>
    /// Each role's links, grouped, Home and the personal links aside. A role whose links, with Home and the personal links,
    /// come to <see cref="FlatLimit" /> or fewer has one group, whose heading is never shown.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<NavGroup>> ByRole =
        new Dictionary<string, IReadOnlyList<NavGroup>>(StringComparer.Ordinal)
        {
            [WombatRoles.Administrator] =
            [
                new("Platform", [ScheduledJobs, AuditLog, SsoMappings, DataRightsRequests]),
                new("Organisations", [Institutions, Colleges]),
                new("People", [Users, Invitations]),
                new("Catalogue", [Epas, Curricula, ActivityTypes, EntrustmentScales]),
                new("Reviews", [DecisionsDue, CommitteeReviews, DecisionPanels]),
            ],
            [WombatRoles.InstitutionalAdmin] =
            [
                new("People", [Invitations, Trainees, Assessors, Users]),
                new("Curriculum", [CurriculumAdoptions, Curricula, Epas, ActivityTypes, EntrustmentScales]),
                new("Reviews", [DecisionsDue, CommitteeReviews, DecisionPanels]),
                new("Access and audit", [SsoMappings, AuditLog]),
            ],

            // T300: the College writes its disciplines' activity types (T091), and the builder admits it.
            [WombatRoles.CollegeAdmin] = [new(null, [Specialities, Epas, Curricula, ActivityTypes])],

            // T131 slice 6: Decisions due is for the roles that schedule reviews, so not for a Committee member.
            [WombatRoles.SpecialityAdmin] = [new(null, [DecisionsDue, CommitteeReviews, DecisionPanels])],
            [WombatRoles.SubSpecialityAdmin] = [new(null, [DecisionsDue, CommitteeReviews, DecisionPanels])],
            [WombatRoles.CommitteeMember] = [new(null, [CommitteeReviews, DecisionPanels])],

            // Not Invitations: its page admits only an Administrator or an Institutional admin (T178).
            [WombatRoles.Coordinator] = [new(null, [DecisionsDue, MsfCampaigns, CommitteeReviews, DataRightsRequests])],
            [WombatRoles.Assessor] = [new(null, [ActivityInbox])],

            // T154: the portfolio review asks the trainee to export the period and hand the PDF to the reviewer.
            [WombatRoles.Trainee] = [new(null, [LogAnActivity, MyActivities, MsfReports, MyCommitteeReviews, ExportPortfolio])],

            // Only the pages that admit a pending trainee (T141).
            [WombatRoles.PendingTrainee] = [new(null, [LogAnActivity, MyActivities])],
        };

    /// <summary>The roles the navigation has links for: every role there is.</summary>
    public static IEnumerable<string> Roles => ByRole.Keys;

    /// <summary>The navigation of <paramref name="user" />, acting as <paramref name="acting" />.</summary>
    public static NavMenuModel For(ActingRole acting, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(acting);
        ArgumentNullException.ThrowIfNull(user);

        var groups = acting.Role is { } role && ByRole.TryGetValue(role, out var own) ? own : [];
        var personal = PersonalFor(user);

        var count = 1 + groups.Sum(group => group.Items.Count) + personal.Count;
        IReadOnlyList<NavGroup> shown = count <= FlatLimit
            ? [new NavGroup(null, [Home, .. groups.SelectMany(group => group.Items)])]
            : [new NavGroup(null, [Home]), .. groups];

        return new NavMenuModel(shown, personal);
    }

    /// <summary>
    /// The links that are the person's rather than the role's, whatever the acting role (D8): My progress for a holder of
    /// a trainee record, current or ended, and My data rights for everyone. My progress is offered to exactly whom its
    /// page's policy, <c>TraineeOrFormerTrainee</c>, admits: the Trainee role held, or the record's claim (T252).
    /// </summary>
    private static IReadOnlyList<NavItem> PersonalFor(ClaimsPrincipal user)
        => user.IsInRole(WombatRoles.Trainee) || user.HasClaim(claim => claim.Type == WombatClaimTypes.TraineeRecord)
            ? [MyProgress, MyDataRights]
            : [MyDataRights];
}
