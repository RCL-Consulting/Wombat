namespace Wombat.Application.Features.Activities.Services;

public interface IActivityReferenceDataService
{
    Task<IReadOnlyList<ActivityCatalogueOption>> GetCatalogueOptionsAsync(
        string catalogueKey,
        CancellationToken cancellationToken = default);
}

public sealed record ActivityCatalogueOption(
    string Value,
    string Label);
