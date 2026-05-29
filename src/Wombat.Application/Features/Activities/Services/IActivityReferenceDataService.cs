using System.Security.Claims;

namespace Wombat.Application.Features.Activities.Services;

public interface IActivityReferenceDataService
{
    Task<IReadOnlyList<ActivityCatalogueOption>> GetCatalogueOptionsAsync(
        string catalogueKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// EPAs the caller may reference on a form, scoped to their sub-speciality/speciality.
    /// A global Administrator sees all active EPAs. Value is the EPA id; Label is "Code — Title".
    /// </summary>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
        ClaimsPrincipal principal,
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
    /// id or name). Value is the level order; Label is "order. label". Empty when unresolved.
    /// </summary>
    Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleLevelOptionsAsync(
        string? scaleKey,
        CancellationToken cancellationToken = default);
}

public sealed record ActivityCatalogueOption(
    string Value,
    string Label);
