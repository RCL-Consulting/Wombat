using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

public sealed record AddMsfInvitationCommand(
    int CampaignId,
    string RespondentEmail,
    MsfRespondentCategory RespondentCategory) : IRequest<int>;

public sealed class AddMsfInvitationCommandValidator : AbstractValidator<AddMsfInvitationCommand>
{
    public AddMsfInvitationCommandValidator()
    {
        RuleFor(command => command.CampaignId).GreaterThan(0);
        RuleFor(command => command.RespondentEmail).NotEmpty().EmailAddress().MaximumLength(320);
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
