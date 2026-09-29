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
    /// The curriculum predicate of <see cref="GetEpaOptionsAsync" />'s narrowing arm — a curriculum item on
    /// the subject's curriculum whose <c>OwningInstitutionId</c> is null or theirs — deliberately
    /// <b>without</b> its three fallbacks to the viewer's claims, and <b>without</b> the T122 tool
    /// intersection that arm applies (an MSF release covers EPAs whatever their tool lists say, and release
    /// must not start dropping declared EPAs because of one). The fallbacks exist to stop a
    /// required form field becoming unsubmittable; this list is not a form field. It answers "which EPAs
    /// may this campaign declare itself evidence for", and the release re-applies exactly the same
    /// predicate before creating anything. Offering an EPA here that release would then drop would be a
    /// picker that lies.
    /// </para>
    /// <para>
    /// Empty means empty, and the caller must say so rather than substitute something else.
    /// </para>
    /// </remarks>
    /// <param name="permittedToolKey">
    /// Null for MSF: the tool lists are not consulted. An instrument key (<c>learner_feedback</c>, T164) keeps only the
    /// items whose list does not refuse it (<see cref="ToolPermission.Evaluate" />: an item with no list is unrestricted,
    /// D21), with none of the picker's fallbacks: when no item permits it the answer is empty. A learner-feedback
    /// campaign's form, its create command and its release all ask with the same key
    /// (<c>MsfEvidenceKinds.CoverageToolKeyFor</c>), so the three agree on which EPAs it may cover.
    /// </param>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
        string subjectUserId,
        string? permittedToolKey = null,
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
    /// The people a <c>user</c> field may name on an activity (T102): users holding every required role, at the
    /// activity's institution, not deactivated, and not the subject. Value is the user id.
    /// </summary>
    /// <remarks>
    /// The same query the write path judges a nominee with, so the picker offers exactly who submitting accepts. It
    /// depends on the activity, never on who is looking: an Administrator is offered the trainee's institution's people,
    /// not the country's. A <see cref="NomineeOptionScope.StoredValue" /> outside the list is appended, labelled
    /// "(not on the current list)" without saying why, so a stored nominee never renders as an empty select.
    /// </remarks>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
        NomineeOptionScope scope,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// One user's display label, or null if the id names nobody. For a <c>user</c> field shown read-only, which needs
    /// the stored person's name and nothing else: a reader is never sent the list of everyone who could have been
    /// named (T102).
    /// </summary>
    /// <remarks>
    /// Unscoped by design, so call it only with a value STORED on an activity the caller has been authorised to read,
    /// never with a value from a form's working copy: that may be any user's id, typed into the page by hand, and this
    /// would answer with their name and email.
    /// </remarks>
    Task<ActivityCatalogueOption?> GetUserOptionAsync(
        string userId,
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

    /// <summary>
    /// <see cref="GetEntrustmentScaleLevelOptionsAsync" />'s rungs with each one's descriptor (T342, B12): the same ladder,
    /// resolved the same way, in order. <c>Order</c> is what is stored (5 on the CPSA ladder), <c>Label</c> what a person
    /// reads ("4"): the page maps a stored value to "Rated 4" through it. Empty when unresolved.
    /// </summary>
    Task<IReadOnlyList<Wombat.Application.Features.Epas.EntrustmentRung>> GetEntrustmentScaleRungsAsync(
        string? scaleKey,
        CancellationToken cancellationToken = default);
}

public sealed record ActivityCatalogueOption(
    string Value,
    string Label);

/// <summary>
/// What an EPA picker knows about the field it is rendering, so the option list offers what the write path
/// will accept and the credit engine will credit, rather than guessing (T108, T122).
/// </summary>
/// <param name="SubjectUserId">
/// The trainee the activity is ABOUT, not the person looking at it. On the detail page these differ:
/// the assessor is the principal, the registrar is the subject, and credit follows the subject.
/// </param>
/// <param name="NarrowToCreditable">
/// True only when this field's key is the one the pinned version's credit rules actually read
/// (<c>epa_field</c>), or, for a College instrument that credits nothing, the field its schema names as
/// its evidence EPA, which the write path holds to the instrument's list (T154;
/// <c>CreditRuleFields.ResolveNarrowedEpaFieldKeys</c>). A reflective note tags an EPA, credits nothing
/// and is no instrument; an MSF form of the legacy shape credits a fixed <c>curriculum_item_id</c> and
/// never reads its EPA field. Narrowing either of those would hide EPAs whose selection changes nothing.
/// </param>
/// <param name="CurrentValue">
/// The EPA id already stored in this field, if any. It is always offered back, whatever the
/// narrowing decides — an <c>epa</c> field renders as a <c>select</c>, so dropping a stored value
/// from the options would erase recorded evidence from the page rather than merely hide a choice.
/// A stored EPA the tool may no longer credit is therefore still shown, and refused at submit.
/// </param>
/// <param name="WbaToolKey">
/// The <c>WbaToolKey</c> of the activity type being filed or rendered — the type ROW's current key, which is
/// what the write path checks (T122). Read only when <paramref name="NarrowToCreditable" /> produces a
/// narrowing; the claims filter ignores it. Null means "not a recognised instrument", which is unrestricted
/// (D21). Defaulted only so the existing named constructions compile: a host that forgets it narrows less
/// than the write path accepts, which is the safe direction, because the write path still refuses.
/// </param>
public sealed record EpaOptionScope(
    string? SubjectUserId,
    bool NarrowToCreditable,
    string? CurrentValue = null,
    string? WbaToolKey = null);

/// <summary>
/// Whose nominees a <c>user</c> field's picker lists (T102).
/// </summary>
/// <param name="SubjectUserId">The person the activity is about. Never a nominee; null lists nobody.</param>
/// <param name="RequiredRoles">Every role the nominee must hold, from <c>ActorFieldRules.RequiredRolesForUserField</c>.</param>
/// <param name="ForExistingActivity">
/// True on an activity that exists: the institution is then <paramref name="ActivityInstitutionId" /> as stamped, and a
/// null stamp lists nobody, exactly as the write path judges it. False on the create page: the institution is resolved
/// from the subject the way the create will stamp it.
/// </param>
/// <param name="ActivityInstitutionId">The existing activity's stamped institution.</param>
/// <param name="StoredValue">
/// The value stored on the activity for this field, never the form's working copy. If it has fallen off the list it is
/// appended with its label, because it is already on the record every reader of the activity sees. A value the page
/// holds but the activity does not is never looked up: it may be anyone's id, typed into the page by hand.
/// </param>
public sealed record NomineeOptionScope(
    string? SubjectUserId,
    IReadOnlyList<string> RequiredRoles,
    bool ForExistingActivity,
    int? ActivityInstitutionId,
    string? StoredValue);
