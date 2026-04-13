using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record OpenMsfCampaignCommand(int CampaignId, string BaseRespondUrl) : IRequest;

public sealed class OpenMsfCampaignCommandHandler : IRequestHandler<OpenMsfCampaignCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IInvitationTokenService _tokenService;

    public OpenMsfCampaignCommandHandler(IApplicationDbContext dbContext, IEmailSender emailSender, IInvitationTokenService tokenService)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _tokenService = tokenService;
    }

    public async Task Handle(OpenMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _dbContext.Set<MsfCampaign>()
            .Include(candidate => candidate.Template)
            .Include(candidate => candidate.Invitations)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.CampaignId, cancellationToken)
            ?? throw new InvalidOperationException("The MSF campaign could not be found.");

        if (campaign.Invitations.Count == 0)
        {
            throw new InvalidOperationException("At least one respondent invitation is required before opening a campaign.");
        }

        campaign.Open(DateTime.UtcNow);

        foreach (var invitation in campaign.Invitations.Where(candidate => !string.IsNullOrWhiteSpace(candidate.RespondentEmail)))
        {
            var token = _tokenService.GenerateToken();
            invitation.TokenHash = _tokenService.HashToken(token);

            await _emailSender.SendAsync(
                invitation.RespondentEmail!,
                $"MSF request: {campaign.Template.Name}",
                $"""
                You have been invited to provide anonymous multi-source feedback.

                Submit your response:
                {request.BaseRespondUrl}?token={Uri.EscapeDataString(token)}

                This link expires on {invitation.ExpiresOn:yyyy-MM-dd}.
                """,
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
