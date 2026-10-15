using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Wombat.Web.Security;

/// <summary>
/// Whether this circuit has a browser at the other end now. (T290)
/// </summary>
/// <remarks>
/// A tab that signs out, or closes, leaves its circuit disconnected but retained for a few minutes in case it comes back.
/// A role change in that time ends the circuit's sign-in, and <see cref="EndedSessionExit" /> used to send it to the
/// sign-in page all the same: with no browser to receive it the navigation was cancelled, and the framework logged
/// "Navigation failed" and "Unhandled exception in circuit" at error level for a circuit nobody was looking at. The exit
/// asks this first, and leaves on reconnection instead.
/// </remarks>
public sealed class CircuitConnection : CircuitHandler
{
    private Action? _onReconnected;

    /// <summary>Whether the browser is connected. True until the framework says otherwise.</summary>
    public bool IsConnected { get; private set; } = true;

    /// <summary>Runs <paramref name="action" /> once, when the browser next connects.</summary>
    public void WhenReconnected(Action action) => _onReconnected = action;

    public override Task OnConnectionDownAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        IsConnected = false;
        return Task.CompletedTask;
    }

    public override Task OnConnectionUpAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        IsConnected = true;
        var action = _onReconnected;
        _onReconnected = null;
        action?.Invoke();
        return Task.CompletedTask;
    }
}
