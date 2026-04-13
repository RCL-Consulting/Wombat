using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Scheduling;

namespace Wombat.Infrastructure.Scheduling;

public static class ScheduledJobServiceCollectionExtensions
{
    public static IServiceCollection AddScheduledJob<TJob>(this IServiceCollection services)
        where TJob : class, IScheduledJob
    {
        services.AddSingleton<TJob>();
        services.AddSingleton<IScheduledJob>(provider => provider.GetRequiredService<TJob>());
        return services;
    }
}
