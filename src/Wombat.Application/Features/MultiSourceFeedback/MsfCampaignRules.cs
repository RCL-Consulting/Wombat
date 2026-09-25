using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Identity;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public static class MsfCampaignRules
{
    public static async Task<MsfCampaign> GetCampaignGraphAsync(IApplicationDbContext dbContext, int campaignId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<MsfCampaign>()
            .Include(campaign => campaign.Template)
                .ThenInclude(template => template.Questions)
            .Include(campaign => campaign.CoveredEpas)
                .ThenInclude(covered => covered.Epa)
            .Include(campaign => campaign.Invitations)
            .Include(campaign => campaign.Responses)
                .ThenInclude(response => response.Invitation)
            .Include(campaign => campaign.Responses)
                .ThenInclude(response => response.Answers)
            .SingleOrDefaultAsync(campaign => campaign.Id == campaignId, cancellationToken)
            ?? throw new InvalidOperationException("The MSF campaign could not be found.");
    }

    /// <summary>
    /// The EPAs a campaign of this kind about this trainee may be declared evidence for, as ids. (T121, T164)
    /// </summary>
    /// <remarks>
    /// The one predicate the create command and the release apply, and the campaign form's picker asks the same service
    /// with the same key: the EPAs on the trainee's own curriculum, and for learner feedback only those whose tool list
    /// names it (<see cref="MsfEvidenceKinds.CoverageToolKeyFor" />). Create refuses an EPA outside it; release drops one
    /// that has left it since, with a log line.
    /// </remarks>
    public static async Task<HashSet<int>> CoverableEpaIdsAsync(
        IActivityReferenceDataService referenceDataService,
        string subjectUserId,
        MsfTemplateKind kind,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(referenceDataService);

        return (await referenceDataService
                .GetSubjectCurriculumEpaOptionsAsync(subjectUserId, MsfEvidenceKinds.CoverageToolKeyFor(kind), cancellationToken))
            .Select(option => int.Parse(option.Value, System.Globalization.CultureInfo.InvariantCulture))
            .ToHashSet();
    }

    /// <summary>
    /// Refuses to act on a campaign the caller does not run, before anything of it is loaded to be changed. A campaign
    /// id that names nothing is refused with exactly the same exception and message as one about a trainee at another
    /// institution. (T113)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Close, withdraw, release, resend and removing an invitee run this first; invite and open run it through
    /// <see cref="EnsureCampaignTakesNewWorkAsync" />, which then asks for a current trainee (T284); create runs
    /// <see cref="EnsureSubjectIsInScopeAsync" /> on the subject it is given. The rule is
    /// <see cref="IsSubjectInScopeAsync" />, asked of the campaign's subject.
    /// </para>
    /// <para>
    /// The two refusals are one because the campaign editor prints the refusal: "could not be found" for a missing id
    /// beside a scope refusal for a real one would let a Coordinator walk the ids and learn which campaigns other
    /// institutions run, which is the census <see cref="GetCampaignAggregateReportQuery" /> already denies by
    /// returning null for both. An Administrator runs every campaign but those about themselves, so for them "not found"
    /// means only that; one about them is refused as a Coordinator's out-of-scope campaign is, which tells them only that
    /// a campaign about them exists. An Administrator who also holds Trainee runs none
    /// (<see cref="IsKeptFromCampaignsAbout" />), so they are told what anyone else is told. (T224)
    /// </para>
    /// </remarks>
    public static Task EnsureCampaignIsInScopeAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        int campaignId,
        CancellationToken cancellationToken)
        => SubjectOfCampaignInScopeAsync(dbContext, principal, campaignId, cancellationToken);

    /// <summary>
    /// Refuses new work on a campaign: <see cref="EnsureCampaignIsInScopeAsync" />, then a refusal unless the campaign's
    /// trainee is still a current trainee (<see cref="TraineeScopeResolver.ResolveCurrentAsync" />), in the words create
    /// refuses a subject who is not (<see cref="SubjectNotCurrentTrainee" />). What adding an invitee and opening a draft
    /// run first. (T284)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Opening a draft is new work: it mails every respondent a request for feedback about the trainee, as creating the
    /// campaign would have. So it asks what create asks (<see cref="MayStartCampaignAboutAsync" />), and adding an invitee
    /// to the draft does too, since an invitee is only ever added to be mailed at the open. Until T284 only create asked,
    /// so a draft written before the trainee was locked, completed their programme, withdrew or lost the Trainee role could
    /// still be opened, and mailed everyone on it about someone no longer in a programme.
    /// </para>
    /// <para>
    /// The scope refusal comes first and is unchanged, so an outsider learns nothing about the trainee from this. Past it,
    /// the caller runs the campaign and already knows whom it is about, so the reason is said plainly to a Coordinator as
    /// to an Administrator: the one create gives an Administrator. Both checks only read, so the refusal leaves nothing for
    /// the audit pipeline's save to commit.
    /// </para>
    /// <para>
    /// A campaign already open carries on: its reminders (<c>MsfInvitationExpiryReminderJob</c>) and its Resend
    /// (<see cref="ResendMsfLinksCommand" />) reach people already asked, and a close, release or withdraw finishes it, as
    /// a review already scheduled stays one its panel can finish. Removing an invitee from a draft is not new work either.
    /// </para>
    /// </remarks>
    public static async Task EnsureCampaignTakesNewWorkAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        int campaignId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(users);

        var subjectUserId = await SubjectOfCampaignInScopeAsync(dbContext, principal, campaignId, cancellationToken);

        if (await TraineeScopeResolver.ResolveCurrentAsync(dbContext, users, subjectUserId.Trim(), cancellationToken) is null)
        {
            throw new UnauthorizedAccessException(SubjectNotCurrentTrainee);
        }
    }

    /// <summary>
    /// <see cref="EnsureCampaignIsInScopeAsync" />'s rule, answering the campaign's subject once the caller is admitted.
    /// </summary>
    private static async Task<string> SubjectOfCampaignInScopeAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        int campaignId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var subjectUserId = await dbContext.Set<MsfCampaign>()
            .AsNoTracking()
            .Where(campaign => campaign.Id == campaignId)
            .Select(campaign => campaign.SubjectUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (subjectUserId is null && principal.IsAdministrator() && !RunsNoCampaigns(principal))
        {
            throw new InvalidOperationException("The MSF campaign could not be found.");
        }

        if (subjectUserId is null ||
            !await IsSubjectInScopeAsync(dbContext, principal, subjectUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException(CampaignNotRunByCaller);
        }

        return subjectUserId;
    }

    /// <summary>The one refusal for a campaign id the caller may not act on, whether or not it exists. (T113)</summary>
    internal const string CampaignNotRunByCaller =
        "The MSF campaign could not be found among the campaigns you run.";

    /// <summary>
    /// Refuses to create a campaign about a trainee the caller may not start one about
    /// (<see cref="MayStartCampaignAboutAsync" />). (T121, T113, T238)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every campaign command runs this rule before it touches anything: create through this method, and invite, open,
    /// close, withdraw and release through <see cref="EnsureCampaignIsInScopeAsync" /> (invite and open through
    /// <see cref="EnsureCampaignTakesNewWorkAsync" />, which asks for a current trainee too, T284). Release is where the
    /// consequence is (it writes activities scope-stamped from the SUBJECT's profile, so an out-of-scope release plants
    /// another institution's oversight trail permanently in that trainee's portfolio), but a close anonymises the
    /// respondents for good and an open mails every one of them, so none of the others is harmless either. The audit
    /// pipeline commits a failed handler's pending mutation, so the check has to come before the first one, not merely
    /// somewhere in the handler.
    /// </para>
    /// <para>
    /// The rule is <see cref="MayStartCampaignAboutAsync" />. A caller it keeps out whoever the subject is
    /// (<see cref="IsKeptFromCampaignsAbout" />) is told why, before anything is read: the reason is their own id or their
    /// own role, so it says nothing about the id they gave. Create runs this before it reads the template too, so an
    /// out-of-scope caller learns nothing from any id they send. (T224)
    /// </para>
    /// <para>
    /// So is a caller who runs no campaign by their roles (<see cref="EnsureRunsCampaigns" />, T248): an InstitutionalAdmin,
    /// or a Coordinator at no institution, is told whom campaigns are run by, not something about the subject.
    /// </para>
    /// <para>
    /// Anyone but an Administrator gets one refusal for every other subject the rule keeps out (an id that names nobody,
    /// a trainee elsewhere, an erased trainee's pseudonym, a trainee whose programme has ended, one who no longer holds
    /// Trainee, and one whose account an administrator has locked, T268), so it confirms nothing about the id. An
    /// Administrator runs campaigns at every institution, so the one reason left is said plainly. (T238)
    /// </para>
    /// </remarks>
    public static async Task EnsureSubjectIsInScopeAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(subjectUserId);

        if (IsCaller(principal, subjectUserId))
        {
            throw new UnauthorizedAccessException(OwnCampaignRefused);
        }

        EnsureRunsCampaigns(principal);

        if (!await MayStartCampaignAboutAsync(dbContext, users, principal, subjectUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException(
                principal.IsAdministrator() ? SubjectNotCurrentTrainee : SubjectNotRunByCaller);
        }
    }

    /// <summary>The refusal, at create, of a subject the caller may not start a campaign about, whoever it names. (T238)</summary>
    internal const string SubjectNotRunByCaller =
        "A multi-source feedback campaign can only be run for a trainee in a programme at your own institution.";

    /// <summary>
    /// The refusal, at create, to an Administrator, of a subject who is not a current trainee: no active profile, or an
    /// account that is gone, no longer holds Trainee (T238), or is locked (T268). Also the refusal, to anyone who runs the
    /// campaign, of adding an invitee to it or opening it once its trainee is not current
    /// (<see cref="EnsureCampaignTakesNewWorkAsync" />, T284).
    /// </summary>
    internal const string SubjectNotCurrentTrainee =
        "A multi-source feedback campaign can only be run for a trainee in a programme now: someone whose trainee " +
        "profile is active, who still holds the Trainee role, and who has not been locked out by an administrator.";

    /// <summary>
    /// Whether this caller may start a campaign about this trainee: they run campaigns for the trainee
    /// (<see cref="IsSubjectInScopeAsync" />), and the trainee is a current trainee
    /// (<see cref="TraineeScopeResolver.ResolveCurrentAsync" />): an active profile, on an account that exists, still
    /// holds Trainee and has not been locked out by an administrator (T268). What create asks, and what the campaign
    /// form's picker offers by (<see cref="CampaignSubjectsAsync" />). (T238)
    /// </summary>
    /// <remarks>
    /// <para>
    /// New work asks for a current trainee: a new campaign, and since T284 an invitee added to a draft and the open that
    /// mails them (<see cref="EnsureCampaignTakesNewWorkAsync" />). Until T258 an erased trainee's profile stayed active
    /// under a pseudonym no account holds (<c>ErasureExecutor</c>), so until T238 a crafted create could name it; and the
    /// form offered graduates and trainees who had withdrawn. None of them is on a programme to plan feedback for, and the
    /// programme's coverage page (<see cref="GetMsfProgrammeCoverageQuery" />) counts none of them.
    /// </para>
    /// <para>
    /// A campaign already open is acted on by <see cref="IsSubjectInScopeAsync" /> alone, which reads the trainee's
    /// preferred profile, active or not: a trainee who completes while their feedback is being collected leaves a
    /// campaign their coordinator can still resend, close, release or withdraw, and whose respondents are still reminded,
    /// as a review already scheduled stays one its panel can finish (<c>CommitteeTraineeScope</c>). A draft about them can
    /// be withdrawn, or have an invitee removed, but not opened (T284).
    /// </para>
    /// <para>
    /// A current trainee's active profile is their preferred one, so this is <see cref="IsSubjectInScopeAsync" />'s answer
    /// with the current-trainee half added, from one resolve. The answer is <see cref="MayStartCampaignAbout" />'s, the
    /// predicate the picker filters by too (T248).
    /// </para>
    /// </remarks>
    public static async Task<bool> MayStartCampaignAboutAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(subjectUserId);

        if (IsKeptFromCampaignsAbout(principal, subjectUserId))
        {
            return false;
        }

        var trainee = await TraineeScopeResolver.ResolveCurrentAsync(
            dbContext, users, subjectUserId.Trim(), cancellationToken);

        return MayStartCampaignAbout(principal, subjectUserId, trainee);
    }

    /// <summary>
    /// Whether this caller may start a campaign about this trainee, given where the trainee trains if they are a current
    /// trainee (<see cref="TraineeScopeResolver.ResolveCurrentAsync" />) and null if not. The one predicate the create
    /// (<see cref="MayStartCampaignAboutAsync" />) and the campaign form's picker (<see cref="CampaignSubjectsAsync" />)
    /// both ask, as T102's nominee gate and picker share theirs, so the two cannot drift apart. Reads nothing. (T248)
    /// </summary>
    /// <remarks>
    /// The exclusions first (<see cref="IsKeptFromCampaignsAbout" />), then a current trainee, then the caller's scope: a
    /// global Administrator runs a campaign about a current trainee anywhere, a Coordinator about one at their own
    /// institution, and nobody else about anyone.
    /// </remarks>
    public static bool MayStartCampaignAbout(ClaimsPrincipal principal, string subjectUserId, TraineeScope? trainee)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(subjectUserId);

        return !IsKeptFromCampaignsAbout(principal, subjectUserId) &&
               trainee is not null &&
               (principal.IsAdministrator() || CampaignInstitutionOf(principal) == trainee.InstitutionId);
    }

    /// <summary>
    /// The trainees this caller may start a campaign about: the set form of <see cref="MayStartCampaignAboutAsync" />,
    /// which the campaign form's trainee picker offers from (<see cref="ListMsfCampaignSubjectsQuery" />), so it offers
    /// exactly whom create accepts. (T238)
    /// </summary>
    /// <remarks>
    /// A Coordinator's are the current trainees at their institution, an Administrator's every current trainee; never the
    /// caller, and nobody for a caller who runs no campaign (<see cref="RunsCampaigns" />). One resolve of the current
    /// trainees (<see cref="TraineeScopeResolver.ResolveAllCurrentAsync" />), each kept only if
    /// <see cref="MayStartCampaignAbout" />, the create's own predicate, keeps it (T248). The institution the resolve is
    /// narrowed to drops nobody the predicate would keep, since it demands that institution of a Coordinator's subjects.
    /// </remarks>
    public static async Task<IReadOnlyList<string>> CampaignSubjectsAsync(
        IApplicationDbContext dbContext,
        IUserAdministrationService users,
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (!RunsCampaigns(principal))
        {
            return [];
        }

        int? institutionId = null;
        if (!principal.IsAdministrator())
        {
            if (CampaignInstitutionOf(principal) is not int callerInstitutionId)
            {
                return [];
            }

            institutionId = callerInstitutionId;
        }

        var trainees = await TraineeScopeResolver.ResolveAllCurrentAsync(dbContext, users, institutionId, cancellationToken);

        return trainees
            .Where(trainee => MayStartCampaignAbout(principal, trainee.Key, trainee.Value))
            .Select(trainee => trainee.Key)
            .ToArray();
    }

    /// <summary>
    /// Refuses a caller who runs no campaign at all, before anything is written: anyone who holds Trainee, in
    /// <see cref="TraineeRunsNoCampaigns" />'s words, and anyone who is neither an Administrator nor a Coordinator at an
    /// institution. What creating a questionnaire template asks, since a template is about no trainee, and what a campaign
    /// create asks before its subject. (T248)
    /// </summary>
    /// <remarks>
    /// Until T248 the template command took no caller at all, so anyone the campaign page admitted created templates,
    /// someone who holds Trainee among them (T224's rule), and the templates are every institution's.
    /// </remarks>
    public static void EnsureRunsCampaigns(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (RunsNoCampaigns(principal))
        {
            throw new UnauthorizedAccessException(TraineeRunsNoCampaigns);
        }

        if (!RunsCampaigns(principal))
        {
            throw new UnauthorizedAccessException(RunsCampaignsRoles);
        }
    }

    /// <summary>
    /// The refusal of a caller who is neither an Administrator nor a Coordinator at an institution; also what the campaign
    /// page says to such a caller as standing content. (T248)
    /// </summary>
    public const string RunsCampaignsRoles =
        "Multi-source feedback campaigns and their templates are run by a coordinator at an institution, or by an " +
        "administrator.";

    /// <summary>
    /// Whether this caller runs any campaign at all: an Administrator, or a Coordinator at an institution, who does not
    /// hold Trainee (<see cref="RunsNoCampaigns" />). Answered from the caller's claims, so it reads nothing. (T248)
    /// </summary>
    /// <remarks>
    /// A Coordinator at no institution is reachable: an Administrator can add the role to an account that has none.
    /// </remarks>
    public static bool RunsCampaigns(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return !RunsNoCampaigns(principal) &&
               (principal.IsAdministrator() || CampaignInstitutionOf(principal) is not null);
    }

    /// <summary>The refusal, at create, of a campaign about the caller themselves. (T224)</summary>
    internal const string OwnCampaignRefused =
        "A multi-source feedback campaign cannot be run by the trainee it is about. Another coordinator at your " +
        "institution can run yours.";

    /// <summary>
    /// The refusal, at create, of any campaign to a caller who holds the Trainee role; also what the campaign pages say to
    /// such a caller as standing content, so the page and the refusal give one reason. (T224, T185)
    /// </summary>
    public const string TraineeRunsNoCampaigns =
        "You hold the Trainee role, so you cannot run multi-source feedback campaigns, whatever other role you hold.";

    /// <summary>
    /// Whether this caller runs multi-source feedback for this trainee: a global Administrator, or a Coordinator whose
    /// institution is the one the trainee trains at; never the trainee themselves, and never anyone who holds Trainee.
    /// (T121, T113, T224)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The exclusions come first (<see cref="IsKeptFromCampaignsAbout" />), before the Administrator arm, as the subject
    /// arm comes first in <see cref="CanReadReportAsync" />. Whoever runs a campaign adds its invitees, opens it, watches
    /// its per-group counts come in before release, closes it, and writes the narrative the trainee is released, so the
    /// trainee running their own would defeat the review step. Until T224 only the report refused them; the campaign
    /// page's counts, the list's totals and every command admitted a subject who also coordinates.
    /// </para>
    /// <para>
    /// Deliberately NOT <c>ClaimsPrincipalExtensions.CanAccessInstitution</c>. That helper answers a narrower
    /// question - global Administrator, or InstitutionalAdmin whose claim matches - and returns false for a
    /// Coordinator whatever their institution. Coordinators are the people who run MSF campaigns, so using it here
    /// would lock out the only role that needs these pages.
    /// </para>
    /// <para>
    /// The Coordinator role is required, not only a matching institution claim (T113). Every user carries an
    /// institution claim, trainees included, and <see cref="CanReadReportAsync" /> is reached from a page trainees
    /// open: without the role, a trainee could read a classmate's unreleased feedback by putting its campaign id in
    /// the address bar. The campaign pages admit Coordinators and Administrators only, so this is the page gate
    /// restated where the data is, not a narrowing anyone was relying on.
    /// </para>
    /// <para>
    /// Where the trainee trains is <see cref="TraineeScopeResolver" />'s answer, the same one stamped on every
    /// activity a release writes. Until T113 this read its own copy that broke ties by programme start date, so a
    /// trainee with no current profile and two past ones could resolve to one institution for their campaigns and
    /// another for the evidence those campaigns recorded. A trainee with no profile is in nobody's scope but an
    /// Administrator's.
    /// </para>
    /// </remarks>
    public static async Task<bool> IsSubjectInScopeAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(subjectUserId);

        if (IsKeptFromCampaignsAbout(principal, subjectUserId))
        {
            return false;
        }

        if (principal.IsAdministrator())
        {
            return true;
        }

        if (CampaignInstitutionOf(principal) is not int callerInstitutionId)
        {
            return false;
        }

        var scope = await TraineeScopeResolver.ResolveAsync(dbContext, subjectUserId.Trim(), cancellationToken);
        return scope is not null && scope.InstitutionId == callerInstitutionId;
    }

    /// <summary>
    /// Confines a set of campaigns to the ones this caller runs: every campaign for an Administrator, the campaigns
    /// about trainees at their own institution for a Coordinator, and none for anyone else; never one about the caller,
    /// and none at all for a caller who holds Trainee. The list-shaped half of <see cref="IsSubjectInScopeAsync" />, as a
    /// SQL-translatable predicate. (T113, T224)
    /// </summary>
    /// <remarks>
    /// <para>
    /// A campaign carries no institution of its own; its subject's preferred profile does. Resolving each campaign's
    /// subject one by one would mean materialising every campaign in the country to decide which few a coordinator
    /// may see, so the resolver's preferred-profile set is joined here instead. It is the same definition
    /// <see cref="TraineeScopeResolver.ResolveAsync" /> reads, which is what keeps this list and the single-campaign
    /// check from disagreeing about a trainee who holds more than one profile.
    /// </para>
    /// <para>
    /// The exclusions are <see cref="IsKeptFromCampaignsAbout" />'s, applied first here as they are there: the list shows
    /// each campaign's invitee and response totals before release, which the subject may not watch. (T224)
    /// </para>
    /// </remarks>
    public static IQueryable<MsfCampaign> WhereRunBy(
        this IQueryable<MsfCampaign> campaigns,
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(campaigns);
        ArgumentNullException.ThrowIfNull(principal);

        if (RunsNoCampaigns(principal))
        {
            return campaigns.Where(_ => false);
        }

        if (CallerUserIdOf(principal) is { } callerUserId)
        {
            campaigns = campaigns.Where(campaign => campaign.SubjectUserId != callerUserId);
        }

        if (principal.IsAdministrator())
        {
            return campaigns;
        }

        if (CampaignInstitutionOf(principal) is not int institutionId)
        {
            return campaigns.Where(_ => false);
        }

        var preferredProfiles = TraineeScopeResolver.PreferredProfiles(dbContext);

        return campaigns.Where(campaign => preferredProfiles.Any(profile =>
            profile.UserId == campaign.SubjectUserId &&
            profile.InstitutionId == institutionId));
    }

    /// <summary>
    /// Whether this caller may read a campaign's aggregate report. (T113)
    /// </summary>
    /// <remarks>
    /// <para>
    /// The subject of the feedback reads it once it has been released, and not before, whatever other role they
    /// hold: the report before release is the coordinator's working copy, and the point of the review step is that
    /// the trainee sees what the reviewer released rather than what came in. That arm is first for the reason
    /// <c>CommitteeDecisionAuthorization.DemandReviewAccessAsync</c> puts its trainee arm first.
    /// </para>
    /// <para>
    /// Everyone else reads it only if they run campaigns for the trainee (<see cref="IsSubjectInScopeAsync" />), which
    /// nobody who holds Trainee does (T224): a registrar who also coordinates reads their own released report here and no
    /// peer's, as <see cref="TraineeScopeResolver.MayReadAsync" /> answers for the rest of a trainee's record (T185).
    /// Before T113 the trainee's page authorised the read itself, after the handler had already handed the whole
    /// report back, and the coordinator's page did not authorise it at all.
    /// </para>
    /// </remarks>
    public static async Task<bool> CanReadReportAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        string subjectUserId,
        MsfCampaignState state,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (IsCaller(principal, subjectUserId))
        {
            return state == MsfCampaignState.Released;
        }

        return await IsSubjectInScopeAsync(dbContext, principal, subjectUserId, cancellationToken);
    }

    /// <summary>
    /// Whether this caller is kept from running any campaign about this trainee, whatever role and scope would admit
    /// them: the trainee themselves, and anyone who holds Trainee (<see cref="TraineeScopeResolver.ActsAsTrainee" />).
    /// Answered from the caller's own id and roles, so it reads nothing. (T224)
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="IsSubjectInScopeAsync" /> and <see cref="WhereRunBy" /> apply it first, and through them every campaign
    /// command, the campaign page and the campaign list. The campaign form's trainee picker applies it too, so it never
    /// offers the caller, and offers nobody to a caller who holds Trainee. Since T238 the picker offers exactly whom the
    /// create accepts (<see cref="CampaignSubjectsAsync" />, <see cref="MayStartCampaignAboutAsync" />). Until then it
    /// listed every trainee profile at the caller's institution, ended ones included, and a trainee whose current profile
    /// was elsewhere was offered and refused.
    /// </para>
    /// <para>
    /// The trainee is kept out whatever other roles they hold, the Administrator role included, for the reason
    /// <see cref="CanReadReportAsync" /> keeps them from the report before release. Holding Trainee keeps a caller out of
    /// every campaign (<see cref="RunsNoCampaigns" />), T185's rung: a registrar who also coordinates runs no peer's
    /// feedback, as they read no peer's progress, committee review or entrustment standing.
    /// </para>
    /// </remarks>
    public static bool IsKeptFromCampaignsAbout(ClaimsPrincipal principal, string subjectUserId)
    {
        ArgumentNullException.ThrowIfNull(principal);
        ArgumentNullException.ThrowIfNull(subjectUserId);

        return RunsNoCampaigns(principal) || IsCaller(principal, subjectUserId);
    }

    /// <summary>
    /// Whether this caller runs no campaign at all, whoever it is about: anyone who holds Trainee
    /// (<see cref="TraineeScopeResolver.ActsAsTrainee" />), whatever other role brought them to the campaign pages.
    /// Answered from the caller's roles, so it reads nothing. (T224, T185)
    /// </summary>
    /// <remarks>
    /// The one half of <see cref="IsKeptFromCampaignsAbout" /> that does not depend on the subject, so the campaign pages
    /// ask it too: the list offers no "New campaign", the create form is not shown, and both pages say why in
    /// <see cref="TraineeRunsNoCampaigns" />'s words, the ones a create refusal gives.
    /// </remarks>
    public static bool RunsNoCampaigns(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        return TraineeScopeResolver.ActsAsTrainee(principal);
    }

    /// <summary>
    /// Whether a campaign in this state can be withdrawn: any campaign not yet released, and not withdrawn already, as
    /// <see cref="MsfCampaign.Withdraw" /> accepts. The withdraw command refuses the rest, and the campaign list and the
    /// campaign page offer Withdraw on exactly these. (T206; T199 added a closed campaign and one under review)
    /// </summary>
    /// <remarks>
    /// Withdrawing a campaign under review is the decision never to release its report. Whoever may release it may take
    /// that decision instead: the release and the withdraw ask the same caller rule
    /// (<see cref="EnsureCampaignIsInScopeAsync" />). Until T199 only a draft or an open campaign could be withdrawn, so a
    /// campaign whose report nobody would release stayed under review for good, and a committee review kept saying it
    /// was awaiting release (T173's notice).
    /// </remarks>
    public static bool IsWithdrawable(MsfCampaignState state)
        => state is not (MsfCampaignState.Released or MsfCampaignState.Withdrawn);

    /// <summary>
    /// Whether withdrawing a campaign in this state is the decision never to release its report: it has closed, which
    /// already stopped every link and removed every address, and it has not been released. Withdrawing a draft or an open
    /// campaign stops its links and removes its addresses instead. The two pages word the withdraw dialog and its result
    /// by this, and the command refuses a withdraw confirmed in the one's words when the campaign now means the other
    /// (<see cref="WithdrawMsfCampaignCommandHandler.ClosedSinceShown" />). (T199 review)
    /// </summary>
    public static bool WithdrawingForgoesRelease(MsfCampaignState state)
        => state is MsfCampaignState.Closed or MsfCampaignState.UnderReview;

    /// <summary>
    /// Whether the caller is this user; never for a caller with no id. The one answer to "is this the campaign's subject",
    /// for the scope rules here and for the report, which names teaching contexts to anyone else. (T224)
    /// </summary>
    internal static bool IsCaller(ClaimsPrincipal principal, string userId)
        => CallerUserIdOf(principal) is { } callerUserId &&
           string.Equals(callerUserId, userId.Trim(), StringComparison.Ordinal);

    private static string? CallerUserIdOf(ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value is { Length: > 0 } callerUserId ? callerUserId : null;

    /// <summary>
    /// The institution this caller runs campaigns at, or null when they run none: a Coordinator's own institution.
    /// A global Administrator is answered before this is asked.
    /// </summary>
    private static int? CampaignInstitutionOf(ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.Coordinator) ? principal.GetInstitutionId() : null;

    /// <summary>
    /// The refusal of a link a response has already been submitted through (T202). Also what the submit answers when a
    /// second submission through one link loses the race to the first (T205).
    /// </summary>
    public const string LinkUsedMessage =
        "This feedback link has already been used: a response was submitted through it, and each link takes one response.";

    /// <summary>
    /// The invitation a respondent's link names, when it can still take a response; otherwise a refusal written for
    /// the respondent (<see cref="MsfResponseRefusedException" />, T202).
    /// </summary>
    /// <remarks>
    /// A link names its invitation as the current link (<see cref="MsfInvitation.TokenSelector" />) or as the one a
    /// reminder replaced (<see cref="MsfInvitation.PreviousTokenSelector" />, T214). The previous link takes a response
    /// until the respondent's last day to respond and is refused as expired after it; otherwise both are judged alike.
    /// Either way the invitation takes one response: the first answer retires the previous link, and the current one is
    /// then refused as used (<see cref="LinkUsedMessage" />), as is the loser of two answers racing through both.
    /// </remarks>
    public static async Task<MsfInvitation> GetActiveInvitationByTokenAsync(
        IApplicationDbContext dbContext,
        string rawToken,
        IInvitationTokenService tokenService,
        CancellationToken cancellationToken)
    {
        // A link no token could be is refused before anything is read (T205), and so is a link mailed before T163, which
        // carries no selector.
        var selector = tokenService.SelectorOf(rawToken) ?? throw LinkNotRecognised();

        // The row by the selector, as the current link or as the one a reminder replaced (T214), each column under a
        // unique index of its own; then the whole token against that link's hash, in constant time. Until T163 the hash
        // was the only key, so every invitation there was, with its campaign and questionnaire, was loaded and hashed
        // against the token, on a public page, for every load and every submit (T163). At most two rows, one per column;
        // a selector is 96 random bits, so in practice one.
        var candidates = await dbContext.Set<MsfInvitation>()
            .Include(candidate => candidate.Campaign)
                .ThenInclude(campaign => campaign.Template)
                    .ThenInclude(template => template.Questions)
            .Where(candidate => candidate.TokenSelector == selector || candidate.PreviousTokenSelector == selector)
            .ToListAsync(cancellationToken);

        // A selector alone opens nothing: it is not a secret, and a token whose secret half is wrong is refused as a link
        // that names no invitation, in the same words as a selector that matched no row. The time taken can differ (a
        // match costs a joined read and a hash), which tells a guesser nothing usable: a selector is 96 random bits, and
        // whoever holds a real one holds the link it came in. Each link is checked against its own hash, never the other.
        var invitation = candidates.FirstOrDefault(candidate =>
            candidate.TokenSelector == selector && tokenService.VerifyToken(rawToken, candidate.TokenHash));
        var throughPreviousLink = false;
        if (invitation is null)
        {
            invitation = candidates.FirstOrDefault(candidate =>
                candidate.PreviousTokenSelector == selector &&
                candidate.PreviousTokenHash is { } previousHash &&
                tokenService.VerifyToken(rawToken, previousHash));
            throughPreviousLink = invitation is not null;
        }

        if (invitation is null)
        {
            throw LinkNotRecognised();
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // The link a reminder replaced lasts until the last day to respond, the one deadline the respondent was given,
        // and not a day past it while the campaign waits for the auto-close job (T214).
        if (invitation.ExpiresOn < today ||
            (throughPreviousLink && MsfInvitation.LastDayToRespond(invitation.Campaign.ClosesOn, invitation.ExpiresOn) < today))
        {
            throw new MsfResponseRefusedException(
                MsfResponseRefusal.LinkExpired,
                "This feedback link has expired, so it can no longer be used.");
        }

        if (invitation.RevokedOn is not null)
        {
            throw new MsfResponseRefusedException(
                MsfResponseRefusal.LinkRevoked,
                "This feedback link has been revoked, so it can no longer be used.");
        }

        if (invitation.RespondedOn is not null)
        {
            throw new MsfResponseRefusedException(MsfResponseRefusal.LinkUsed, LinkUsedMessage);
        }

        if (invitation.Campaign.State != MsfCampaignState.Open)
        {
            throw new MsfResponseRefusedException(
                MsfResponseRefusal.CampaignNotOpen,
                "This feedback request has closed and is no longer accepting responses.");
        }

        return invitation;
    }

    /// <summary>The refusal of a link that names no invitation: malformed, mistyped, or retired.</summary>
    /// <remarks>
    /// <para>
    /// The link that opened a campaign is replaced when a reminder re-issues it (T132), and since T214 it keeps working
    /// until the last day to respond, unless the respondent answers first or the campaign closes, either of which retires
    /// it. So a first link nobody recognises most likely belongs to a respondent who has answered, or to a campaign that
    /// has stopped taking responses, and the most recent email's link says which.
    /// </para>
    /// <para>
    /// So it says what that link will tell them (T214 review). A respondent who answered through their invitation's link
    /// and opens it again, as the thank-you and fault pages invite them to, is sent here, not to "already used": the
    /// answer retired the link they hold. Until then this page only told them to use another link, never that it would
    /// say whether their feedback had been recorded.
    /// </para>
    /// </remarks>
    private static MsfResponseRefusedException LinkNotRecognised()
        => new(
            MsfResponseRefusal.LinkNotRecognised,
            "This feedback link is not recognised. If you were sent a reminder about this request, use the link in the " +
            "most recent email: it opens the questionnaire, or says whether your feedback has already been recorded.");

    /// <summary>
    /// Refuses answers that do not complete the questionnaire, as a refusal the respondent is shown
    /// (<see cref="MsfResponseRefusedException" />, T202).
    /// </summary>
    /// <remarks>
    /// <para>
    /// A duplicate answer and an answer to a question not on the questionnaire are refused here too. The first used to
    /// throw from <c>SingleOrDefault</c> and the second from the save, both as faults (500) on input the respondent
    /// sent.
    /// </para>
    /// <para>
    /// So are a rating that is not one of its question's points (<paramref name="scalePoints" />, from
    /// <see cref="MsfRatingScale.ResolveAsync" />) and a comment longer than the column holds (T205). Before T205 any
    /// integer was stored as a rating, where it entered the category means, and an over-long comment failed the save as
    /// a fault.
    /// </para>
    /// </remarks>
    public static void ValidateResponsePayload(
        MsfTemplate template,
        IReadOnlyDictionary<int, IReadOnlyList<MsfScalePointDto>> scalePoints,
        IReadOnlyCollection<SubmitMsfResponseAnswerItem> answers)
    {
        ArgumentNullException.ThrowIfNull(scalePoints);

        if (answers.GroupBy(answer => answer.QuestionId).Any(group => group.Count() > 1))
        {
            throw Incomplete("Each question can be answered once.");
        }

        var questionIds = template.Questions.Select(question => question.Id).ToHashSet();
        if (answers.Any(answer => !questionIds.Contains(answer.QuestionId)))
        {
            throw Incomplete("An answer names a question that is not on this questionnaire.");
        }

        foreach (var question in template.Questions)
        {
            var answer = answers.SingleOrDefault(candidate => candidate.QuestionId == question.Id);
            if (question.Required && answer is null)
            {
                throw Incomplete($"A response is required for '{question.Prompt}'.", question.Id);
            }

            if (answer is null)
            {
                continue;
            }

            if (question.Type == MsfQuestionType.Scale && !answer.ScaleValue.HasValue)
            {
                throw Incomplete($"A scale value is required for '{question.Prompt}'.", question.Id);
            }

            if (question.Type == MsfQuestionType.Scale &&
                !(scalePoints.TryGetValue(question.Id, out var points) &&
                  points.Any(point => point.Value == answer.ScaleValue!.Value)))
            {
                throw Incomplete($"Choose one of the points on the scale for '{question.Prompt}'.", question.Id);
            }

            if (question.Type == MsfQuestionType.LongText && string.IsNullOrWhiteSpace(answer.LongText))
            {
                throw Incomplete($"A comment is required for '{question.Prompt}'.", question.Id);
            }

            if (question.Type == MsfQuestionType.LongText && answer.LongText!.Trim().Length > MsfResponseAnswer.LongTextMaxLength)
            {
                throw Incomplete(
                    $"The comment for '{question.Prompt}' is longer than {MsfResponseAnswer.LongTextMaxLength} characters. " +
                    "Please shorten it.",
                    question.Id);
            }
        }
    }

    private static MsfResponseRefusedException Incomplete(string message, int? questionId = null)
        => new(MsfResponseRefusal.AnswersIncomplete, message) { QuestionId = questionId };
}
