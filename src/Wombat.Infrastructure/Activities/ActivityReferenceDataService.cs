using Microsoft.EntityFrameworkCore;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Infrastructure.Persistence;

namespace Wombat.Infrastructure.Activities;

public sealed class ActivityReferenceDataService : IActivityReferenceDataService
{
    public const string ProcedureCatalogueKey = "procedure_catalogue";

    private readonly ApplicationDbContext _dbContext;

    public ActivityReferenceDataService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ActivityCatalogueOption>> GetCatalogueOptionsAsync(
        string catalogueKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogueKey);

        if (!string.Equals(catalogueKey, ProcedureCatalogueKey, StringComparison.Ordinal))
        {
            return [];
        }

        return await _dbContext.Set<ProcedureCatalogueEntry>()
            .OrderBy(entity => entity.Category)
            .ThenBy(entity => entity.Name)
            .Select(entity => new ActivityCatalogueOption(entity.Key, entity.Name))
            .ToListAsync(cancellationToken);
    }
}
