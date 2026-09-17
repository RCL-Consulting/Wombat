using System.Security.Claims;
using Wombat.Domain.Activities;
using Wombat.Domain.Activities.Schema;
using Wombat.Domain.Activities.Workflow;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// Resolves which schema fields the supplied principal may write on an activity right now.
/// </summary>
/// <remarks>
/// T070. Ownership is declared in the schema, not inferred from <c>requires_fields</c>
/// (which is a validation list that only ever widens the required set — see SchemaValidator).
/// The effective permission is a conjunction: the workflow state must let this actor write,
/// AND the field (or, failing that, its section) must let this actor write.
///
/// Both declarations are optional and default to <c>subject|creator</c>, which reproduces the
/// pre-T070 <c>CanEditDraft</c> behaviour exactly — so every already-published
/// <c>ActivityTypeVersion</c> keeps working without a republish.
/// </remarks>
public interface IFieldPermissionEvaluator
{
    /// <param name="ignoreStateGate">
    /// Skips the state-level gate (terminal check and the state's own <c>editable_by</c> rule),
    /// leaving only the section/field rules. Used solely by activity creation: types such as
    /// <c>procedure_log</c> and <c>journal_club</c> declare a terminal initial state and would
    /// otherwise be uncreatable.
    /// </param>
    IReadOnlySet<string> GetWritableFieldKeys(
        FormSchema schema,
        Workflow workflow,
        Activity activity,
        ClaimsPrincipal principal,
        bool ignoreStateGate = false);
}
