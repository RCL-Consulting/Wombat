using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Audit;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Commands.TransitionActivity;

/// <remarks>
/// <c>DataPatchJson</c> and <c>Note</c> are redacted from the audit summary. The
/// AuditPipelineBehavior audits every request whose type name ends in "Command" and
/// AuditPayloadSerializer writes its properties into SummaryJson; from T070 onward the patch
/// carries the assessor's clinical narrative (strengths / improvements / plan) on every
/// completion, and the note carries the decline reason. Neither belongs in the audit log. (T070)
/// </remarks>
public sealed record TransitionActivityCommand(
    int ActivityId,
    string TransitionKey,
    string ActorUserId,
    ClaimsPrincipal Principal,
    [property: Redact] string? DataPatchJson = null,
    [property: Redact] string? Note = null) : IRequest<ActivityDto>;

public sealed class TransitionActivityCommandValidator : AbstractValidator<TransitionActivityCommand>
{
    public TransitionActivityCommandValidator()
    {
        RuleFor(command => command.ActivityId).GreaterThan(0);
        RuleFor(command => command.TransitionKey).NotEmpty();
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
    }
}

public sealed class TransitionActivityCommandHandler : IRequestHandler<TransitionActivityCommand, ActivityDto>
{
    private readonly IActivityService _activityService;

    public TransitionActivityCommandHandler(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public Task<ActivityDto> Handle(TransitionActivityCommand request, CancellationToken cancellationToken)
        => _activityService.TransitionAsync(
            new TransitionActivityInput(
                request.ActivityId,
                request.TransitionKey,
                request.ActorUserId,
                request.Principal,
                request.DataPatchJson,
                request.Note),
            cancellationToken);
}
