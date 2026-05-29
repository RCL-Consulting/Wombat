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
        ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetAssessorOptionsAsync(
        ClaimsPrincipal principal, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleOptionsAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);

    public virtual Task<IReadOnlyList<ActivityCatalogueOption>> GetEntrustmentScaleLevelOptionsAsync(
        string? scaleKey, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ActivityCatalogueOption>>([]);
}
