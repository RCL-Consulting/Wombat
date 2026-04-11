using System.Security.Claims;

namespace Wombat.Application.Features.Activities.Dtos;

public sealed record CreateActivityInput(
    int ActivityTypeId,
    string SubjectUserId,
    string CreatedByUserId,
    string InitialDataJson,
    ClaimsPrincipal Principal);

public sealed record UpdateActivityDraftInput(
    int ActivityId,
    string ActorUserId,
    string NewDataJson,
    ClaimsPrincipal Principal);

public sealed record TransitionActivityInput(
    int ActivityId,
    string TransitionKey,
    string ActorUserId,
    ClaimsPrincipal Principal,
    string? DataPatchJson,
    string? Note);

public sealed record WorkflowEvaluationResult(
    bool Allowed,
    string? Reason)
{
    public static WorkflowEvaluationResult Allow() => new(true, null);

    public static WorkflowEvaluationResult Deny(string reason) => new(false, reason);
}

public enum SchemaValidationMode
{
    Draft = 0,
    Submit = 1
}
