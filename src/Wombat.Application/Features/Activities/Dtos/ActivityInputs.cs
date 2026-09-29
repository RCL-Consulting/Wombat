using System.Security.Claims;

namespace Wombat.Application.Features.Activities.Dtos;

public sealed record CreateActivityInput(
    int ActivityTypeId,
    string SubjectUserId,
    string CreatedByUserId,
    string InitialDataJson,
    ClaimsPrincipal Principal);

/// <summary>
/// A batch of activities to create and drive straight to a terminal state in one unit of work. (T121)
/// </summary>
/// <param name="ActivityTypeKey">
/// The seeded type's key, not its id. The caller is a feature that knows which instrument it is writing
/// evidence for ("msf_cpsa"), never which row that became in this database.
/// </param>
/// <param name="CreatedByUserId">
/// The person whose act produced these records — the releasing reviewer, not the subject. They must
/// satisfy the transition's actor rule, which is what stops a trainee reaching this path.
/// </param>
/// <param name="DataJsonPerActivity">
/// One complete form payload per activity. The batch exists because one campaign covers many EPAs and
/// <c>curriculum_item_match</c> reads a single integer out of <c>DataJson</c>, so the coverage has to be
/// one row per EPA rather than one row naming many.
/// </param>
public sealed record RecordCompletedActivitiesInput(
    string ActivityTypeKey,
    string SubjectUserId,
    string CreatedByUserId,
    string TransitionKey,
    IReadOnlyList<string> DataJsonPerActivity,
    ClaimsPrincipal Principal);

public sealed record TransitionActivityInput(
    int ActivityId,
    string TransitionKey,
    string ActorUserId,
    ClaimsPrincipal Principal,
    string? DataPatchJson,
    string? Note);

/// <summary>
/// A draft's data saved without a move (T342, B5, E3): <c>IActivityService.SaveDraftAsync</c>.
/// </summary>
/// <param name="DataPatchJson">
/// The fields to write, as a JSON object, merged over the stored data as a move's patch is. The page may send the whole
/// form or only what changed: a field the caller may not write is refused only when its value differs from the stored one.
/// </param>
public sealed record SaveActivityDraftInput(
    int ActivityId,
    string ActorUserId,
    ClaimsPrincipal Principal,
    string DataPatchJson);

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
