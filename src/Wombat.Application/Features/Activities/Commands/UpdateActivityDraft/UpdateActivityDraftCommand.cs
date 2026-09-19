using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Audit;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Commands.UpdateActivityDraft;

/// <remarks>
/// <c>NewDataJson</c> is redacted from the audit summary, for the same reason
/// TransitionActivityCommand redacts its patch. The AuditPipelineBehavior audits every request
/// whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; this property is the whole form state on every keystroke-driven save, so leaving
/// it unmarked wrote more copies of the trainee's clinical data into the audit table than exist in
/// the activity table itself. The activity id and actor stay in the clear. (T101)
/// </remarks>
public sealed record UpdateActivityDraftCommand(
    int ActivityId,
    string ActorUserId,
    [property: Redact] string NewDataJson,
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
