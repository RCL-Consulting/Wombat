using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Services;

/// <summary>
/// The circuit's own services are registered scoped: one instance per connected user, shared by every page they visit
/// in that circuit.
/// </summary>
/// <remarks>
/// A wrong lifetime fails quietly. Transient, and <see cref="ActivityNotices" /> hands <c>/activities/{id}</c> a
/// different instance from the one <c>/activities/new</c> posted to, so a refused submit lands on its draft without the
/// word that it was saved (T127). Singleton, and one user's notice can be taken by another user's circuit. Every bUnit
/// test registers the service itself, so none of them would notice either.
/// </remarks>
public sealed class CircuitServiceCollectionExtensionsTests
{
    [Theory]
    [InlineData(typeof(ActivityNotices))]
    [InlineData(typeof(IScopedSender))]
    public void TheService_IsRegisteredOnce_AndScoped(Type serviceType)
    {
        var services = new ServiceCollection().AddWombatCircuitServices();

        services.Where(descriptor => descriptor.ServiceType == serviceType)
            .Should().ContainSingle()
            .Which.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void ANoticePosted_IsTakenInTheSameCircuit_AndInNoOther()
    {
        using var provider = new ServiceCollection().AddWombatCircuitServices().BuildServiceProvider();
        using var circuit = provider.CreateScope();
        using var otherCircuit = provider.CreateScope();

        circuit.ServiceProvider.GetRequiredService<ActivityNotices>().Post(41, "success", "Filed. It is now Requested.");

        otherCircuit.ServiceProvider.GetRequiredService<ActivityNotices>().Take(41).Should().BeNull(
            "another user's circuit must never see it");
        circuit.ServiceProvider.GetRequiredService<ActivityNotices>().Take(41).Should().Be(
            new ActivityNotice("success", "Filed. It is now Requested."),
            "the page that takes it resolves the same instance as the page that posted it");
    }
}
