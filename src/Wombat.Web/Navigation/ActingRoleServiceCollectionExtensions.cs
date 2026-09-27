using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Wombat.Web.Navigation;

/// <summary>What the acting role needs registered, for <c>Program.cs</c> and for every host that renders <c>App</c>.</summary>
public static class ActingRoleServiceCollectionExtensions
{
    public static IServiceCollection AddActingRoleSwitch(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // One per process: the nonces of the switch results already shown are held here (ActingRoleSwitchResults).
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ActingRoleSwitchResults>();
        return services;
    }
}
