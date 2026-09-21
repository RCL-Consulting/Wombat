using System.Security.Claims;

namespace Wombat.Application.Features.Activities.Services;

public interface IActivityReferenceDataService
{
    Task<IReadOnlyList<ActivityCatalogueOption>> GetCatalogueOptionsAsync(
        string catalogueKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// EPAs the caller may reference on a form. Value is the EPA id; Label is "Code — Title".
    /// </summary>
    /// <param name="principal">The authenticated viewer — who is often NOT the subject.</param>
    /// <param name="scope">
    /// What the caller knows about the field being rendered. <c>null</c>, or a scope that cannot be
    /// resolved, keeps the claims-based behaviour: sub-speciality/speciality scope, with a global
    /// Administrator seeing every active EPA.
    /// </param>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
        ClaimsPrincipal principal,
        EpaOptionScope? scope = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The EPAs on the SUBJECT's own curriculum, with no permissive fallback. (T121)
    /// </summary>
    /// <param name="subjectUserId">The trainee the evidence will be about.</param>
    /// <remarks>
    /// <para>
    /// Same predicate as <see cref="GetEpaOptionsAsync" />'s narrowing arm — a curriculum item on the
    /// subject's curriculum whose <c>OwningInstitutionId</c> is null or theirs — and deliberately
    /// <b>without</b> its three fallbacks to the viewer's claims. Those fallbacks exist to stop a
    /// required form field becoming unsubmittable; this list is not a form field. It answers "which EPAs
    /// may this campaign declare itself evidence for", and the release re-applies exactly the same
    /// predicate before creating anything. Offering an EPA here that release would then drop would be a
    /// picker that lies.
    /// </para>
    /// <para>
    /// Empty means empty, and the caller must say so rather than substitute something else.
    /// </para>
    /// </remarks>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
        string subjectUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// The rungs of the ladder a named activity type rates on. Value is the rung's ORDER, Label is the
    /// rung as the College prints it. Empty when the type declares no rating or the ladder is
    /// unresolvable. (T121)
    /// </summary>
    /// <param name="activityTypeKey">The seeded key, e.g. <c>msf_cpsa</c>.</param>
    /// <remarks>
    /// For a surface that has to collect an entrustment level OUTSIDE the activity form - the MSF
    /// release page is the first - and must therefore resolve the same ladder the form would have.
    /// Value and Label are different numbers on the CPSA ladder: order 5 is rung "4" (T100). A bare
    /// number box here would have stored the order a reviewer typed as a rung they did not mean.
    /// </remarks>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetRatedLevelOptionsForActivityTypeAsync(
        string activityTypeKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assessor users the caller may reference (e.g. the named assessor on a Mini-CEX), scoped to
    /// their institution. A global Administrator sees all assessors. Value is the user id.
    /// </summary>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetAssessorOptionsAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Entrustment scales available to bind to a Scale field. Used by the activity-type builder's
    /// Scale picker. Value is the scale id; Label is the scale name.
    /// </summary>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleOptionsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ordered levels of the entrustment scale identified by <paramref name="scaleKey"/> (matched by
    /// id or name). Value is the level order — that is what is stored and compared. Label is the rung
    /// label alone, which is what a clinician reads. Empty when unresolved.
    /// </summary>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleLevelOptionsAsync(
        string? scaleKey,
        CancellationToken cancellationToken = default);
}

public sealed record ActivityCatalogueOption(
    string Value,
    string Label);

/// <summary>
/// What an EPA picker knows about the field it is rendering, so the option list can mirror what
/// <c>CreditApplier</c> would actually do rather than guess (T108).
/// </summary>
/// <param name="SubjectUserId">
/// The trainee the activity is ABOUT, not the person looking at it. On the detail page these differ:
/// the assessor is the principal, the registrar is the subject, and credit follows the subject.
/// </param>
/// <param name="NarrowToCreditable">
/// True only when this field's key is the one the pinned version's credit rules actually read
/// (<c>epa_field</c>). A reflective note tags an EPA and credits nothing by design; an MSF form
/// credits a fixed <c>curriculum_item_id</c> and never reads its EPA field. Narrowing either of
/// those would hide EPAs whose selection changes nothing.
/// </param>
/// <param name="CurrentValue">
/// The EPA id already stored in this field, if any. It is always offered back, whatever the
/// narrowing decides — an <c>epa</c> field renders as a <c>select</c>, so dropping a stored value
/// from the options would erase recorded evidence from the page rather than merely hide a choice.
/// </param>
public sealed record EpaOptionScope(
    string? SubjectUserId,
    bool NarrowToCreditable,
    string? CurrentValue = null);
