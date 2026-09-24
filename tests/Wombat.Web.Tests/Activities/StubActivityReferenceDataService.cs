using System.Security.Claims;
using Wombat.Application.Features.Activities.Services;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// No-op reference-data stub for bUnit ActivityForm renders. Returns no options for any field
/// type; tests that need populated pickers can subclass and override.
/// </summary>
internal class StubActivityReferenceDataService : IActivityReferenceDataService
{
    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetCatalogueOptionsAsync(
        string catalogueKey, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetEpaOptionsAsync(
        ClaimsPrincipal principal, EpaOptionScope? scope = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetSubjectCurriculumEpaOptionsAsync(
        string subjectUserId, string? permittedToolKey = null, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetRatedLevelOptionsForActivityTypeAsync(
        string activityTypeKey, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetNomineeOptionsAsync(
        NomineeOptionScope scope, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<ActivityCatalogueOption?> GetUserOptionAsync(
        string userId, CancellationToken cancellationToken = default)
        => Task.FromResult<ActivityCatalogueOption?>(null);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleOptionsAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleLevelOptionsAsync(
        string? scaleKey, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);
}
