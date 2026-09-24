using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Wombat.Application.Common.Email;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Options;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

using Wombat.Application.Common;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// No validator: carries a non-nullable int ID and the caller; the handler authorises the caller against the
/// campaign's subject (T113) and validates the state transition.
/// </summary>
[NoValidator]
public sealed record OpenMsfCampaignCommand(int CampaignId, ClaimsPrincipal Principal) : IRequest;

public sealed class OpenMsfCampaignCommandHandler : IRequestHandler<OpenMsfCampaignCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly IInvitationTokenService _tokenService;
    private readonly WombatOptions _options;

    public OpenMsfCampaignCommandHandler(
        IApplicationDbContext dbContext,
        IEmailSender emailSender,
        IInvitationTokenService tokenService,
        IOptions<WombatOptions> options)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _tokenService = tokenService;
        _options = options.Value;
    }

    public async Task Handle(OpenMsfCampaignCommand request, CancellationToken cancellationToken)
    {
        var respondUrl = _options.RequireMsfRespondUrl();

        // Before anything is loaded to be touched: opening rotates every invitation token and mails each respondent.
        // (T113)
        await MsfCampaignRules.EnsureCampaignIsInScopeAsync(
            _dbContext, request.Principal, request.CampaignId, cancellationToken);

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

            var submitUrl = $"{respondUrl}?token={Uri.EscapeDataString(token)}";
            await _emailSender.SendAsync(new EmailMessage(
                To: invitation.RespondentEmail!,
                Subject: $"MSF request: {campaign.Template.Name}",
                HtmlBody: $"""
                    <p>You have been invited to provide anonymous multi-source feedback for <strong>{System.Net.WebUtility.HtmlEncode(campaign.Template.Name)}</strong>.</p>
                    <p><a href="{System.Net.WebUtility.HtmlEncode(submitUrl)}">Submit your response</a></p>
                    <p>This link expires on <strong>{invitation.ExpiresOn:yyyy-MM-dd}</strong>.</p>
                    """,
                TextBody: $"""
                    You have been invited to provide anonymous multi-source feedback.

                    Submit your response:
                    {submitUrl}

                    This link expires on {invitation.ExpiresOn:yyyy-MM-dd}.
                    """,
                Tags: ["msf-invite", $"campaign:{campaign.Id}"]),
                cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
