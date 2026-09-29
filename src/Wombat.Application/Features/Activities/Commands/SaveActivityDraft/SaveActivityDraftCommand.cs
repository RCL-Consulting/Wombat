using System.Security.Claims;
using FluentValidation;
using MediatR;
using Wombat.Application.Audit;
using Wombat.Application.Features.Activities.Dtos;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Application.Features.Activities.Commands.SaveActivityDraft;

/// <summary>
/// Saves a draft's data without a move (T342, B5, E3): "Save draft" on an existing draft's page. A reflection is written
/// over days, and until this command the only way to keep an edit was to submit it (T106 item 1).
/// </summary>
/// <remarks>
/// Audited as every command is (<c>AuditPipelineBehavior</c>). <c>DataPatchJson</c> is redacted from the audit summary,
/// as a move's patch is: it carries the registrar's clinical narrative.
/// </remarks>
public sealed record SaveActivityDraftCommand(
    int ActivityId,
    string ActorUserId,
    ClaimsPrincipal Principal,
    [property: Redact] string DataPatchJson) : IRequest<ActivityDto>;

public sealed class SaveActivityDraftCommandValidator : AbstractValidator<SaveActivityDraftCommand>
{
    public SaveActivityDraftCommandValidator()
    {
        RuleFor(command => command.ActivityId).GreaterThan(0);
        RuleFor(command => command.ActorUserId).NotEmpty();
        RuleFor(command => command.Principal).NotNull();
        RuleFor(command => command.DataPatchJson).NotEmpty();
    }
}

public sealed class SaveActivityDraftCommandHandler : IRequestHandler<SaveActivityDraftCommand, ActivityDto>
{
    private readonly IActivityService _activityService;

    public SaveActivityDraftCommandHandler(IActivityService activityService)
    {
        _activityService = activityService;
    }

    public Task<ActivityDto> Handle(SaveActivityDraftCommand request, CancellationToken cancellationToken)
        => _activityService.SaveDraftAsync(
            new SaveActivityDraftInput(
                request.ActivityId,
                request.ActorUserId,
                request.Principal,
                request.DataPatchJson),
            cancellationToken);
}
