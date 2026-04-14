using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Common.Interfaces;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;

namespace Wombat.Application.Features.DataRights.Commands;

public sealed record SubmitDataRightsRequestCommand(
    DataRightsRequestType Type,
    string Reason,
    ClaimsPrincipal Principal) : IRequest<DataRightsRequestDto>;

public sealed class SubmitDataRightsRequestCommandValidator : AbstractValidator<SubmitDataRightsRequestCommand>
{
    public SubmitDataRightsRequestCommandValidator()
    {
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(4000);
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.Type).IsInEnum();
    }
}

public sealed class SubmitDataRightsRequestCommandHandler : IRequestHandler<SubmitDataRightsRequestCommand, DataRightsRequestDto>
{
    private readonly IApplicationDbContext _dbContext;

    public SubmitDataRightsRequestCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DataRightsRequestDto> Handle(SubmitDataRightsRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = request.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User identity is required.");

        var displayName = request.Principal.FindFirst(ClaimTypes.Name)?.Value
            ?? request.Principal.FindFirst(ClaimTypes.Email)?.Value
            ?? userId;

        // Block erasure if the user has an unratified committee review in progress
        if (request.Type == DataRightsRequestType.Erasure)
        {
            var hasActiveReview = _dbContext.Set<Domain.CommitteeDecisions.CommitteeReview>()
                .Any(review => review.TraineeUserId == userId &&
                    (review.State == Domain.CommitteeDecisions.CommitteeReviewState.Scheduled ||
                     review.State == Domain.CommitteeDecisions.CommitteeReviewState.InProgress ||
                     review.State == Domain.CommitteeDecisions.CommitteeReviewState.Decided ||
                     review.State == Domain.CommitteeDecisions.CommitteeReviewState.UnderAppeal));

            if (hasActiveReview)
                throw new InvalidOperationException(
                    "You cannot request erasure while you have an active committee review. " +
                    "Please wait until all reviews are ratified or finalized.");
        }

        var utcNow = DateTime.UtcNow;
        var entity = DataRightsRequest.Create(userId, displayName, request.Type, request.Reason, utcNow);

        _dbContext.Set<DataRightsRequest>().Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(entity);
    }

    private static DataRightsRequestDto ToDto(DataRightsRequest entity) => new(
        entity.Id,
        entity.RequesterUserId,
        entity.RequesterDisplayName,
        entity.RequestedOn,
        entity.Type,
        entity.Status,
        entity.Reason,
        entity.DecisionNote,
        entity.DecidedByUserId,
        entity.DecidedOn,
        entity.CompletedOn);
}
