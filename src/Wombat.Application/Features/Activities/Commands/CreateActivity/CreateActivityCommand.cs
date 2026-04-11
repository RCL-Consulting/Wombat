using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Commands.CreateActivity;

public sealed record CreateActivityCommand(
    int ActivityTypeId,
    string SubjectUserId,
    string CreatedByUserId,
    string InitialDataJson,
    ClaimsPrincipal Principal) : IRequest<ActivityDto>;

public sealed class CreateActivityCommandValidator : AbstractValidator<CreateActivityCommand>
{
    public CreateActivityCommandValidator()
    {
        RuleFor(command => command.ActivityTypeId).GreaterThan(0);
        RuleFor(command => command.SubjectUserId).NotEmpty();
        RuleFor(command => command.CreatedByUserId).NotEmpty();
        RuleFor(command => command.InitialDataJson).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IActivityService _activityService;

    public CreateActivityCommandHandler(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public Task<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
        => _activityService.CreateDraftAsync(
            new CreateActivityInput(
                request.ActivityTypeId,
                request.SubjectUserId,
                request.CreatedByUserId,
                request.InitialDataJson,
                request.Principal),
            cancellationToken);
}
