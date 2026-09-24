using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record SubmitMsfResponseAnswerItem(int QuestionId, int? ScaleValue, string? LongText);

/// <remarks>
/// Both properties are redacted from the audit summary. The AuditPipelineBehavior audits every
/// request whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson. <c>Answers</c> carries the respondent's free-text judgement of a named colleague,
/// which multi-source feedback only works because it is confidential and aggregated — the audit
/// table showed it un-aggregated and attributable. <c>Token</c> is the respondent's single-use link
/// and would let a reader re-open or overwrite their response. Nothing identifying the campaign is
/// lost: the row still records that a response was submitted, when, and from where. (T101)
///
/// Nor does the row name who sent it (<see cref="IAnonymousAuditedCommand" />, T205). The respondent's page is on the
/// signed-in app's own origin, so a respondent who is also a Wombat user can submit while signed in, and the row was
/// filled from that sign-in: their id, their email and their institution, beside the response's own timestamp.
/// </remarks>
public sealed record SubmitMsfResponseCommand(
    [property: Redact] string Token,
    [property: Redact] IReadOnlyList<SubmitMsfResponseAnswerItem> Answers) : IRequest, IAnonymousAuditedCommand;

public sealed class SubmitMsfResponseCommandValidator : AbstractValidator<SubmitMsfResponseCommand>
{
    public SubmitMsfResponseCommandValidator()
    {
        RuleFor(command => command.Token).NotEmpty();
        RuleFor(command => command.Answers).NotEmpty();
    }
}

public sealed class SubmitMsfResponseCommandHandler : IRequestHandler<SubmitMsfResponseCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;

    public SubmitMsfResponseCommandHandler(IApplicationDbContext dbContext, IInvitationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task Handle(SubmitMsfResponseCommand request, CancellationToken cancellationToken)
    {
        // Every refusal comes before the first mutation: the audit pipeline saves this request's context from its catch.
        var invitation = await MsfCampaignRules.GetActiveInvitationByTokenAsync(_dbContext, request.Token, _tokenService, cancellationToken);
        var scalePoints = await MsfRatingScale.ResolveAsync(_dbContext, invitation.Campaign.Template, cancellationToken);

        // Measured and stored as the respondent typed it. A browser posts each line break of a text box as CRLF, while
        // the box's maxlength counts it as one character, so a comment the box accepted was refused as too long (T205).
        var answers = request.Answers
            .Select(answer => answer with { LongText = MsfResponseAnswer.NormaliseLineBreaks(answer.LongText) })
            .ToList();
        MsfCampaignRules.ValidateResponsePayload(invitation.Campaign.Template, scalePoints, answers);

        var response = new MsfResponse
        {
            CampaignId = invitation.CampaignId,
            InvitationId = invitation.Id,
            SubmittedOn = DateTime.UtcNow,
            Answers = answers
                .Select(answer => new MsfResponseAnswer
                {
                    QuestionId = answer.QuestionId,
                    ScaleValue = answer.ScaleValue,
                    LongText = string.IsNullOrWhiteSpace(answer.LongText) ? null : answer.LongText.Trim()
                })
                .ToList()
        };

        // Through either link: the answer retires the link a reminder replaced, and the current one is then used (T214).
        invitation.RecordResponse(response.SubmittedOn);

        // Written even when this request read no previous link, so the retirement reaches the row whatever it holds now.
        // The reminder job may replace the link between the read above and this save, keeping the one it replaced; a
        // null written over a null read is no change to EF and would not be sent, and the row would be answered with a
        // live previous link, which the server refuses (CK_MsfInvitations_PreviousLinkUnanswered, T214 review).
        var entry = _dbContext.Set<MsfInvitation>().Entry(invitation);
        entry.Property(candidate => candidate.PreviousTokenSelector).IsModified = true;
        entry.Property(candidate => candidate.PreviousTokenHash).IsModified = true;

        _dbContext.Set<MsfResponse>().Add(response);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException refused)
        {
            // An invitation takes one response (a unique index on MsfResponses.InvitationId), so a second submission
            // through the same link that passed the check above before the first one committed (two tabs, or a post
            // repeated while the first was in flight) is refused here. It is the used link the check would have refused
            // a moment later, and it is answered as that, not as a fault (T205). Any other refused save stays a fault.
            // The refused insert is still tracked; the audit pipeline finds the DbUpdateException inside this refusal and
            // discards it before writing its row (T201).
            if (await HasResponseAsync(invitation.Id))
            {
                throw new MsfResponseRefusedException(MsfResponseRefusal.LinkUsed, MsfCampaignRules.LinkUsedMessage, refused);
            }

            throw;
        }
    }

    /// <summary>Whether a response through this invitation has been committed, read past anything this context holds.</summary>
    private Task<bool> HasResponseAsync(int invitationId)
        => _dbContext.Set<MsfResponse>()
            .AsNoTracking()
            .AnyAsync(response => response.InvitationId == invitationId, CancellationToken.None);
}
