using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Web.Security;

namespace Wombat.Web.Tests.Security;

/// <summary>
/// T290: a circuit with no browser connected is not navigated when its sign-in ends. The navigation had nothing to receive
/// it, and the framework logged "Navigation failed" and "Unhandled exception in circuit" at error level for a tab nobody
/// was looking at (a role change within three minutes of signing out). It leaves when the browser reconnects instead.
/// </summary>
public sealed class EndedSessionExitConnectionTests : TestContext
{
    [Fact]
    public async Task ADisconnectedCircuit_IsNotNavigated_AndLeavesOnceTheBrowserReconnects()
    {
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        navigation.NavigateTo("/trainee/progress");
        var connection = new CircuitConnection();
        var exit = new EndedSessionExit(navigation, connection);
        var before = navigation.History.Count;

        await connection.OnConnectionDownAsync(null!, CancellationToken.None);
        exit.Leave();

        exit.Leaving.Should().BeTrue("a second ask must not queue a second exit");
        navigation.History.Count.Should().Be(before, "there is no browser to receive a navigation");

        exit.Leave();
        await connection.OnConnectionUpAsync(null!, CancellationToken.None);

        navigation.History.Count.Should().Be(before + 1);
        var leave = navigation.History.First();
        leave.Uri.Should().StartWith(SessionEnd.Path);
        leave.Options.ForceLoad.Should().BeTrue();
    }

    [Fact]
    public void AConnectedCircuit_LeavesAtOnce()
    {
        var navigation = Services.GetRequiredService<FakeNavigationManager>();
        var exit = new EndedSessionExit(navigation, new CircuitConnection());

        exit.Leave();

        navigation.History.First().Uri.Should().StartWith(SessionEnd.Path);
    }
}
