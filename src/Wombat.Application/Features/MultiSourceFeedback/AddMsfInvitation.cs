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
/// </remarks>
public sealed record AddMsfInvitationCommand(
    int CampaignId,
    [property: Redact] string RespondentEmail,
    MsfRespondentCategory RespondentCategory,
    ClaimsPrincipal Principal) : IRequest<int>;

public sealed class AddMsfInvitationCommandValidator : AbstractValidator<AddMsfInvitationCommand>
{
    public AddMsfInvitationCommandValidator()
    {
        RuleFor(command => command.CampaignId).GreaterThan(0);
        RuleFor(command => command.RespondentEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(command => command.Principal).NotNull();
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

        if (!campaign.Template.AllowPatientResponses && request.RespondentCategory == MsfRespondentCategory.Patient)
        {
            throw new InvalidOperationException("This template does not allow patient responses.");
        }

        var invitation = new MsfInvitation
        {
            CampaignId = campaign.Id,
            RespondentEmail = request.RespondentEmail.Trim(),
            RespondentCategory = request.RespondentCategory,
            TokenHash = _tokenService.HashToken(_tokenService.GenerateToken()),
            IssuedOn = DateTime.UtcNow,
            ExpiresOn = campaign.ClosesOn.AddDays(7)
        };

        _dbContext.Set<MsfInvitation>().Add(invitation);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return invitation.Id;
    }
}
