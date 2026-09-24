using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Extensions;
using Wombat.Application.Common.Security;
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
    /// Refuses to act on a campaign the caller does not run, before anything of it is loaded to be changed. A campaign
    /// id that names nothing is refused with exactly the same exception and message as one about a trainee at another
    /// institution. (T113)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Invite, open, close, withdraw and release run this first; create runs <see cref="EnsureSubjectIsInScopeAsync" />
    /// on the subject it is given. The rule is <see cref="IsSubjectInScopeAsync" />, asked of the campaign's subject.
    /// </para>
    /// <para>
    /// The two refusals are one because the campaign editor prints the refusal: "could not be found" for a missing id
    /// beside a scope refusal for a real one would let a Coordinator walk the ids and learn which campaigns other
    /// institutions run, which is the census <see cref="GetCampaignAggregateReportQuery" /> already denies by
    /// returning null for both. An Administrator runs every campaign, so for them "not found" means only that.
    /// </para>
    /// </remarks>
    public static async Task EnsureCampaignIsInScopeAsync(
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

        if (subjectUserId is null && principal.IsAdministrator())
        {
            throw new InvalidOperationException("The MSF campaign could not be found.");
        }

        if (subjectUserId is null ||
            !await IsSubjectInScopeAsync(dbContext, principal, subjectUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException(CampaignNotRunByCaller);
        }
    }

    /// <summary>The one refusal for a campaign id the caller may not act on, whether or not it exists. (T113)</summary>
    internal const string CampaignNotRunByCaller =
        "The MSF campaign could not be found among the campaigns you run.";

    /// <summary>
    /// Refuses to create a campaign about a trainee the caller does not run campaigns for. (T121, T113)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every campaign command runs this rule before it touches anything: create through this method, and invite, open,
    /// close, withdraw and release through <see cref="EnsureCampaignIsInScopeAsync" />. Release is where the
    /// consequence is (it writes activities scope-stamped from the SUBJECT's profile, so an out-of-scope release plants
    /// another institution's oversight trail permanently in that trainee's portfolio), but a close anonymises the
    /// respondents for good and an open mails every one of them, so none of the others is harmless either. The audit
    /// pipeline commits a failed handler's pending mutation, so the check has to come before the first one, not merely
    /// somewhere in the handler.
    /// </para>
    /// <para>
    /// The rule is <see cref="IsSubjectInScopeAsync" />.
    /// </para>
    /// </remarks>
    public static async Task EnsureSubjectIsInScopeAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        if (!await IsSubjectInScopeAsync(dbContext, principal, subjectUserId, cancellationToken))
        {
            throw new UnauthorizedAccessException(
                "A multi-source feedback campaign can only be run for a trainee admitted to your own institution.");
        }
    }

    /// <summary>
    /// Whether this caller runs multi-source feedback for this trainee: a global Administrator, or a Coordinator whose
    /// institution is the one the trainee trains at. (T121, T113)
    /// </summary>
    /// <remarks>
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
    /// about trainees at their own institution for a Coordinator, and none for anyone else. The list-shaped half of
    /// <see cref="IsSubjectInScopeAsync" />, as a SQL-translatable predicate. (T113)
    /// </summary>
    /// <remarks>
    /// A campaign carries no institution of its own; its subject's preferred profile does. Resolving each campaign's
    /// subject one by one would mean materialising every campaign in the country to decide which few a coordinator
    /// may see, so the resolver's preferred-profile set is joined here instead. It is the same definition
    /// <see cref="TraineeScopeResolver.ResolveAsync" /> reads, which is what keeps this list and the single-campaign
    /// check from disagreeing about a trainee who holds more than one profile.
    /// </remarks>
    public static IQueryable<MsfCampaign> WhereRunBy(
        this IQueryable<MsfCampaign> campaigns,
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(campaigns);
        ArgumentNullException.ThrowIfNull(principal);

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
    /// <c>CommitteeDecisionAuthorization.DemandReviewAccess</c> puts its trainee arm first.
    /// </para>
    /// <para>
    /// Everyone else reads it only if they run campaigns for the trainee (<see cref="IsSubjectInScopeAsync" />).
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

        var callerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(callerUserId) &&
            string.Equals(callerUserId, subjectUserId, StringComparison.Ordinal))
        {
            return state == MsfCampaignState.Released;
        }

        return await IsSubjectInScopeAsync(dbContext, principal, subjectUserId, cancellationToken);
    }

    /// <summary>
    /// The institution this caller runs campaigns at, or null when they run none: a Coordinator's own institution.
    /// A global Administrator is answered before this is asked.
    /// </summary>
    private static int? CampaignInstitutionOf(ClaimsPrincipal principal)
        => principal.IsInRole(WombatRoles.Coordinator) ? principal.GetInstitutionId() : null;

    public static async Task<MsfInvitation> GetActiveInvitationByTokenAsync(
        IApplicationDbContext dbContext,
        string rawToken,
        IInvitationTokenService tokenService,
        CancellationToken cancellationToken)
    {
        var invitations = await dbContext.Set<MsfInvitation>()
            .Include(invitation => invitation.Campaign)
                .ThenInclude(campaign => campaign.Template)
                    .ThenInclude(template => template.Questions)
            .ToListAsync(cancellationToken);

        var invitation = invitations.SingleOrDefault(candidate => tokenService.VerifyToken(rawToken, candidate.TokenHash))
            ?? throw new InvalidOperationException("The response link is invalid or has already been used.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (invitation.ExpiresOn < today)
        {
            throw new InvalidOperationException("The response link has expired.");
        }

        if (invitation.RevokedOn is not null)
        {
            throw new InvalidOperationException("The response link has been revoked.");
        }

        if (invitation.RespondedOn is not null)
        {
            throw new InvalidOperationException("The response link has already been used.");
        }

        if (invitation.Campaign.State != MsfCampaignState.Open)
        {
            throw new InvalidOperationException("This campaign is not currently accepting responses.");
        }

        return invitation;
    }

    public static void ValidateResponsePayload(MsfTemplate template, IReadOnlyCollection<SubmitMsfResponseAnswerItem> answers)
    {
        foreach (var question in template.Questions)
        {
            var answer = answers.SingleOrDefault(candidate => candidate.QuestionId == question.Id);
            if (question.Required && answer is null)
            {
                throw new InvalidOperationException($"A response is required for '{question.Prompt}'.");
            }

            if (answer is null)
            {
                continue;
            }

            if (question.Type == MsfQuestionType.Scale && !answer.ScaleValue.HasValue)
            {
                throw new InvalidOperationException($"A scale value is required for '{question.Prompt}'.");
            }

            if (question.Type == MsfQuestionType.LongText && string.IsNullOrWhiteSpace(answer.LongText))
            {
                throw new InvalidOperationException($"A comment is required for '{question.Prompt}'.");
            }
        }
    }

    public static void AnonymizeInvitations(IEnumerable<MsfInvitation> invitations)
    {
        var utcNow = DateTime.UtcNow;

        foreach (var invitation in invitations.Where(candidate => !string.IsNullOrWhiteSpace(candidate.RespondentEmail)))
        {
            invitation.RespondentEmailHash = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(invitation.RespondentEmail!.Trim().ToUpperInvariant())));
            invitation.RespondentEmail = null;
            invitation.AnonymizedOn = utcNow;
        }
    }
}
