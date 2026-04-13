using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

internal static class MsfCampaignRules
{
    public static async Task<MsfCampaign> GetCampaignGraphAsync(IApplicationDbContext dbContext, int campaignId, CancellationToken cancellationToken)
    {
        return await dbContext.Set<MsfCampaign>()
            .Include(campaign => campaign.Template)
                .ThenInclude(template => template.Questions)
            .Include(campaign => campaign.Invitations)
            .Include(campaign => campaign.Responses)
                .ThenInclude(response => response.Invitation)
            .Include(campaign => campaign.Responses)
                .ThenInclude(response => response.Answers)
            .SingleOrDefaultAsync(campaign => campaign.Id == campaignId, cancellationToken)
            ?? throw new InvalidOperationException("The MSF campaign could not be found.");
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
