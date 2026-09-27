using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Wombat.Web.Services;

namespace Wombat.Web.Components.Shared;

/// <summary>
/// A role dashboard on Home that reads one summary (DESIGN.md § Dashboard page; T335, flow 01, S22(c); T329): it draws
/// its cards inside a <see cref="DashboardFrame" />, each a title and a skeleton until <see cref="Summary" /> has come, and
/// one alert with Try again, and no cards, if the read fails.
/// </summary>
/// <remarks>
/// <para>
/// The frame renders before the summary, so a card's parameters are read while <see cref="Summary" /> is still null: a
/// card's title, stripe or badge that reads it guards the read (<c>Summary?.…</c>), and its content is drawn only once the
/// summary is there (<c>@if (Summary is { } summary)</c>). Until T335 each dashboard drew nothing but a list of skeletons
/// while it read, and <c>Warning="@(_vm.StalledRequests.Count &gt; 0)"</c> and a coverage card's title were safe only because
/// no card rendered before the read returned.
/// </para>
/// <para>
/// The failure is logged, with the exception, and never shown: until T335 each dashboard printed the exception's message,
/// which for a database failure is EF's own text (T272).
/// </para>
/// </remarks>
public abstract class RoleDashboard<TSummary> : ComponentBase
    where TSummary : class
{
    [Inject] private IScopedSender Sender { get; set; } = default!;

    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;

    /// <summary>What the dashboard read; null until it has, and after a failed read.</summary>
    protected TSummary? Summary { get; private set; }

    /// <summary>The last read failed.</summary>
    protected bool Failed { get; private set; }

    /// <summary>The read has not returned yet.</summary>
    protected bool IsLoading => Summary is null && !Failed;

    /// <summary>The dashboard's query, for the signed-in user.</summary>
    protected abstract IRequest<TSummary> QueryFor(ClaimsPrincipal user);

    protected override Task OnInitializedAsync() => LoadAsync();

    /// <summary>Reads the summary, or reads it again after a failure (Try again): the frame shows its skeleton meanwhile.</summary>
    protected async Task LoadAsync()
    {
        Summary = null;
        Failed = false;

        try
        {
            var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            Summary = await Sender.Send(QueryFor(authState.User));
        }
        catch (Exception exception)
        {
            LoggerFactory.CreateLogger(GetType()).LogError(exception, "Home's {Dashboard} could not be read.", GetType().Name);
            Failed = true;
        }
    }
}
