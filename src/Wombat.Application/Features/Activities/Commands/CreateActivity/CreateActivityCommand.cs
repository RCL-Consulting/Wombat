using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Audit;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Commands.CreateActivity;

/// <remarks>
/// <c>InitialDataJson</c> is redacted from the audit summary, for the same reason
/// TransitionActivityCommand redacts its patch. The AuditPipelineBehavior audits every request
/// whose type name ends in "Command" and AuditPayloadSerializer writes its properties into
/// SummaryJson; this property is the activity's form submission in full — the case detail, the
/// patient context, the narrative — so leaving it unmarked copied every trainee's clinical data
/// into an admin-readable table that AuditDetail renders raw. The ids and the actor stay in the
/// clear: they are what makes the audit row useful. (T101)
/// </remarks>
public sealed record CreateActivityCommand(
    int ActivityTypeId,
    string SubjectUserId,
    string CreatedByUserId,
    [property: Redact] string InitialDataJson,
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
