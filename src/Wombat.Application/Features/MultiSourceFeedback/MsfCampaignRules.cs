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
    /// Refuses to act on a campaign about a trainee outside the caller's institution. (T121)
    /// </summary>
    /// <remarks>
    /// <para>
    /// Applied at creation AND at release, because release is where the consequence is: it writes
    /// activities scope-stamped from the SUBJECT's profile, so an out-of-scope release plants another
    /// institution's oversight trail permanently in that trainee's portfolio. Checking only at creation
    /// would have left the door open, since neither <c>ListMsfCampaignsForCoordinatorQuery</c> nor
    /// <c>GetCampaignAggregateReportQuery</c> filters by principal - a coordinator can reach any
    /// campaign's report page by id today (recorded on [T121] as adjacent defect 2).
    /// </para>
    /// <para>
    /// Deliberately NOT <c>ClaimsPrincipalExtensions.CanAccessInstitution</c>. That helper answers a
    /// narrower question - global Administrator, or InstitutionalAdmin whose claim matches - and returns
    /// false for a Coordinator whatever their institution. Coordinators are the people who run MSF
    /// campaigns, so using it here would lock out the only role that needs these pages. The rule below
    /// is the one <c>ActivityService.IsScopedOverseerOf</c> already applies to the activities a release
    /// creates: the trainee's institution, or a global Administrator.
    /// </para>
    /// <para>
    /// This is narrower than the T056 adoption the whole MSF folder still needs. It is here because
    /// [T121] gave a cross-institution campaign teeth it did not have when it was an inert row.
    /// </para>
    /// </remarks>
    public static async Task EnsureSubjectIsInScopeAsync(
        IApplicationDbContext dbContext,
        ClaimsPrincipal principal,
        string subjectUserId,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(principal);

        if (principal.IsAdministrator())
        {
            return;
        }

        var trimmedSubjectUserId = subjectUserId.Trim();

        var subjectInstitutionId = await dbContext.Set<TraineeProfile>()
            .AsNoTracking()
            .Where(entity => entity.UserId == trimmedSubjectUserId)
            .OrderByDescending(entity => entity.IsActive)
            .ThenByDescending(entity => entity.ProgrammeStartDate)
            .Select(entity => (int?)entity.InstitutionId)
            .FirstOrDefaultAsync(cancellationToken);

        var callerInstitutionId = principal.GetInstitutionId();

        if (subjectInstitutionId is null ||
            callerInstitutionId is null ||
            subjectInstitutionId.Value != callerInstitutionId.Value)
        {
            throw new UnauthorizedAccessException(
                "A multi-source feedback campaign can only be run for a trainee admitted to your own institution.");
        }
    }

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
