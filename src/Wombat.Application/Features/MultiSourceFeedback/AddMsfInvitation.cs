using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Audit;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <remarks>
/// <c>RespondentEmail</c> is redacted from the audit summary (T184). The AuditPipelineBehavior audits every request
/// whose type name ends in "Command", and AuditPayloadSerializer writes its properties into SummaryJson. Closing a
/// campaign anonymises its respondents (<c>MsfCampaign.Close</c>), but not the audit trail, which is the log kept
/// longest: every address invited would outlive the anonymising there, beside the campaign it was invited to. The row
/// still records the campaign, the category and who added the invitation.
/// <para>
/// <c>TeachingContext</c> is where a <see cref="MsfRespondentCategory.Learner" /> was taught (T164): required for a
/// learner and refused for anyone else. It stays in the audit row: it names a teaching session, not a person.
/// </para>
/// </remarks>
public sealed record AddMsfInvitationCommand(
    int CampaignId,
    [property: Redact] string RespondentEmail,
    MsfRespondentCategory RespondentCategory,
    ClaimsPrincipal Principal,
    string? TeachingContext = null) : IRequest<int>;

public sealed class AddMsfInvitationCommandValidator : AbstractValidator<AddMsfInvitationCommand>
{
    public AddMsfInvitationCommandValidator()
    {
        RuleFor(command => command.CampaignId).GreaterThan(0);
        RuleFor(command => command.RespondentEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.RespondentCategory).IsInEnum();

        // Where a learner was taught is what EPA 15's "two teaching contexts" is counted from, so a learner is never
        // invited without one; nobody else is invited with one, because no other group is counted by it. (T164)
        RuleFor(command => command.TeachingContext)
            .Must(context => MsfTeachingContexts.Normalize(context) is not null)
            .When(command => command.RespondentCategory == MsfRespondentCategory.Learner)
            .WithMessage("Say where this learner was taught: the teaching context is how the feedback's contexts are counted.");
        RuleFor(command => command.TeachingContext)
            .Must(context => MsfTeachingContexts.Normalize(context) is null)
            .When(command => command.RespondentCategory != MsfRespondentCategory.Learner)
            .WithMessage("Only a learner is invited with a teaching context.");
        RuleFor(command => command.TeachingContext)
            .Must(context => (MsfTeachingContexts.Normalize(context)?.Length ?? 0) <= MsfTeachingContexts.MaximumLength)
            .WithMessage($"A teaching context is at most {MsfTeachingContexts.MaximumLength} characters.");
    }
}

public sealed class AddMsfInvitationCommandHandler : IRequestHandler<AddMsfInvitationCommand, int>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IInvitationTokenService _tokenService;

    public AddMsfInvitationCommandHandler(IApplicationDbContext dbContext, IInvitationTokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    public async Task<int> Handle(AddMsfInvitationCommand request, CancellationToken cancellationToken)
    {
        // Before anything is loaded to be touched, and before the state checks, whose messages would describe another
        // institution's campaign to someone who may not see it. (T113)
        await MsfCampaignRules.EnsureCampaignIsInScopeAsync(
            _dbContext, request.Principal, request.CampaignId, cancellationToken);

        var campaign = await _dbContext.Set<MsfCampaign>()
            .Include(candidate => candidate.Template)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.CampaignId, cancellationToken)
            ?? throw new InvalidOperationException("The MSF campaign could not be found.");

        if (campaign.State != MsfCampaignState.Draft)
        {
            throw new InvalidOperationException("Invitations can only be added while a campaign is in draft.");
        }

        // One rule for who answers which questionnaire (MsfTemplate.Accepts), the one the invitee picker offers from.
        // (T164)
        if (!campaign.Template.Accepts(request.RespondentCategory))
        {
            throw new InvalidOperationException(RefusalFor(campaign.Template, request.RespondentCategory));
        }

        var invitation = new MsfInvitation
        {
            CampaignId = campaign.Id,
            RespondentEmail = request.RespondentEmail.Trim(),
            RespondentCategory = request.RespondentCategory,
            TeachingContext = MsfTeachingContexts.Normalize(request.TeachingContext),
            // No link yet: opening the campaign issues one (MsfInvitation.IssueLink). Until then the invitee holds no
            // selector, so no link finds this row, and the hash is of a token nobody was given. (T163)
            TokenHash = _tokenService.HashToken(_tokenService.GenerateToken()),
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = campaign.ClosesOn.AddDays(7)
        };

        _dbContext.Set<MsfInvitation>().Add(invitation);

        // The campaign is marked modified, though nothing about it changes, so that this save writes the campaign row
        // and is checked against its xmin token (MsfCampaignConfiguration). Until T206 an invitee added from another tab
        // while an open was running was stored on a campaign that then opened without mailing them, and there is no
        // resend. Now either side of that race is refused whole: an open that read the invitations before this save
        // commits is refused at its own save and, opened again, mails this invitee too; and this save, if the campaign
        // was opened, withdrawn or given another invitee after it was read above, is refused here with nothing stored.
        // (MsfInviteDuringOpenRacePostgresTests)
        _dbContext.Set<MsfCampaign>().Entry(campaign).Property(candidate => candidate.State).IsModified = true;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            // Carried as the inner exception, so the audit pipeline still sees a refused save and writes its row alone
            // (T201).
            throw new InvalidOperationException(CampaignChanged, exception);
        }

        return invitation.Id;
    }

    private static string RefusalFor(MsfTemplate template, MsfRespondentCategory category)
        => template.Kind == MsfTemplateKind.LearnerFeedback
            ? "Learner feedback is answered by learners only: invite each respondent as a Learner."
            : category == MsfRespondentCategory.Learner
                ? "A learner answers learner feedback, not multi-source feedback: run a learner-feedback campaign for them."
                : "This template does not allow patient responses.";

    /// <summary>
    /// The refusal when the campaign changed between being read and the save: it was opened or withdrawn, or another
    /// invitee was added at the same moment. Nothing is stored. (T206)
    /// </summary>
    public const string CampaignChanged =
        "The campaign changed while this invitee was being added: it was opened or withdrawn, or another invitee was " +
        "added at the same moment. The invitee has not been added. If the campaign is still a draft, add them again.";
}
