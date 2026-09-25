using Wombat.Web.Security;

namespace Wombat.Web.Services;

/// <summary>
/// The Web services whose lifetime must be the circuit's: scoped, one instance per connected user.
/// </summary>
/// <remarks>
/// Kept apart from <c>Program.cs</c> so the lifetimes can be tested. Getting one wrong fails quietly, not loudly:
/// registered transient, <see cref="ActivityNotices" /> gives the page that takes a notice a different instance from the
/// page that posted it, and a refused submit lands on its draft with no word that it was saved (T127); registered as a
/// singleton, it would hand one user's notice to another's circuit. A missing registration, by contrast, fails the first
/// render of any page that injects the service.
/// </remarks>
public static class CircuitServiceCollectionExtensions
{
    public static IServiceCollection AddWombatCircuitServices(this IServiceCollection services)
    {
        // A fresh DI scope per request, so no DbContext outlives one MediatR call inside a long-lived circuit.
        services.AddScoped<IScopedSender, ScopedSender>();

        // /activities/new hands /activities/{id} what became of the activity it just created (T127).
        services.AddScoped<ActivityNotices>();

        // A circuit whose sign-in has ended leaves by a full page load, once (the T279 review).
        services.AddScoped<EndedSessionExit>();

        return services;
    }
}
