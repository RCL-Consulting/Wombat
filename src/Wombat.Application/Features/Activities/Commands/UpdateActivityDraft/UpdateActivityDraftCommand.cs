using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Commands.UpdateActivityDraft;

public sealed record UpdateActivityDraftCommand(
    int ActivityId,
    string ActorUserId,
    string NewDataJson,
    ClaimsPrincipal Principal) : IRequest<ActivityDto>;

public sealed class UpdateActivityDraftCommandValidator : AbstractValidator<UpdateActivityDraftCommand>
{
    public UpdateActivityDraftCommandValidator()
    {
        RuleFor(command => command.ActivityId).GreaterThan(0);
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.NewDataJson).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class UpdateActivityDraftCommandHandler : IRequestHandler<UpdateActivityDraftCommand, ActivityDto>
{
    private readonly IActivityService _activityService;

    public UpdateActivityDraftCommandHandler(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public Task<ActivityDto> Handle(UpdateActivityDraftCommand request, CancellationToken cancellationToken)
        => _activityService.UpdateDraftAsync(
            new UpdateActivityDraftInput(
                request.ActivityId,
                request.ActorUserId,
                request.NewDataJson,
                request.Principal),
            cancellationToken);
}
