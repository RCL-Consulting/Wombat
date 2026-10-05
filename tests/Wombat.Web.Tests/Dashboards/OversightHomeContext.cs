using System.Security.Claims;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Dashboards;

/// <summary>
/// Renders one oversight Home's dashboard with the summary given, or one that never comes or fails (T358, flow 06, lane
/// B): what CommitteeHomeTests, ProgrammeAdminHomeTests and CoordinatorHomeTests share.
/// </summary>
public abstract class OversightHomeContext : TestContext
{
    private readonly TestAuthorizationContext _auth;

    protected OversightHomeContext()
    {
        _auth = this.AddTestAuthorization();
        _auth.SetAuthorized("staff@kgk.test");
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, "staff-1"));
    }

    protected IRenderedComponent<TDashboard> RenderDashboard<TDashboard>(string role, object? summary, Reads reads = Reads.Answer)
        where TDashboard : Microsoft.AspNetCore.Components.IComponent
    {
        _auth.SetRoles(role);
        Services.AddSingleton<IScopedSender>(new Sender(summary, reads));
        var cut = RenderComponent<TDashboard>();
        if (reads == Reads.Answer)
        {
            cut.WaitForState(() => cut.FindAll(".skeleton").Count == 0 && cut.FindAll(".detail-card").Count > 0);
        }
        else if (reads == Reads.Throw)
        {
            cut.WaitForState(() => cut.FindAll(".alert-danger").Count > 0);
        }

        return cut;
    }

    protected static List<string> Titles(IRenderedFragment cut)
        => cut.FindAll(".dashboard-card-title > span:not(.badge)").Select(title => title.TextContent.Trim()).ToList();

    protected static IElement Section(IRenderedFragment cut, string headingId)
        => cut.Find($"section[aria-labelledby='{headingId}']");

    protected static string Text(IElement element)
        => string.Join(" ", element.TextContent.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public enum Reads
    {
        Answer,
        Hang,
        Throw
    }

    private sealed class Sender(object? summary, Reads reads) : IScopedSender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            => reads switch
            {
                Reads.Hang => new TaskCompletionSource<TResponse>().Task,
                Reads.Throw => Task.FromException<TResponse>(new InvalidOperationException("a fake failure")),
                _ => Task.FromResult((TResponse)summary!)
            };

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
