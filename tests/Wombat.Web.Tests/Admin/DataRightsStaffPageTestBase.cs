using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Domain.Identity;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The fixture of the queue's and a request's tests (T387, flow 14, lane P): Pieter Smit, Coordinator at Kgosi Kgari
/// Teaching Hospital, or the Administrator, whose reads and commands a scripted sender answers. An answer that is an
/// exception is thrown.
/// </summary>
public abstract class DataRightsStaffPageTestBase : TestContext
{
    protected const string Kgk = "Kgosi Kgari Teaching Hospital";

    /// <summary>What ElementReference.FocusAsync calls: an ActionResult or a heading taking the focus.</summary>
    protected const string ElementFocus = "Blazor._internal.domWrapper.focus";

    protected static readonly DateTime Submitted = new(2026, 10, 27, 22, 1, 0, DateTimeKind.Utc);

    private readonly TestAuthorizationContext _auth;

    protected DataRightsStaffPageTestBase()
    {
        _auth = this.AddTestAuthorization();
        Sender = new ScriptedSender();
        Services.AddSingleton<IScopedSender>(Sender);
        JSInterop.Mode = JSRuntimeMode.Loose;
        SignIn(WombatRoles.Coordinator);
    }

    protected ScriptedSender Sender { get; }

    protected void SignIn(string role)
    {
        _auth.SetAuthorized("smit@kgk.wombat.local");
        _auth.SetRoles(role);
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "smit") };
        if (role != WombatRoles.Administrator)
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, "1"));
        }

        _auth.SetClaims(claims.ToArray());
    }

    protected static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    protected static IElement Button(IRenderedFragment cut, string words)
        => cut.FindAll("button").Single(button => Text(button) == words && button.Closest("dialog") is null);

    protected static IElement DialogButton(IRenderedFragment cut, string words)
        => cut.FindAll("dialog button").Single(button => Text(button) == words);

    protected static IElement Section(IRenderedFragment cut, string heading)
        => cut.FindAll("section.user-section").Single(section => Text(section.QuerySelector("h2")!) == heading);

    protected static IReadOnlyList<string> Headings(IRenderedFragment cut)
        => cut.FindAll("section.user-section h2").Select(Text).ToList();

    protected int ElementFocusCalls() => JSInterop.Invocations.Count(invocation => invocation.Identifier == ElementFocus);

    protected IReadOnlyList<string> FocusedIds()
        => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == "wombat.focusById")
            .Select(invocation => (string)invocation.Arguments[0]!)
            .ToList();

    protected int Invoked(string identifier) => JSInterop.Invocations.Count(invocation => invocation.Identifier == identifier);

    /// <summary>Answers each request from <see cref="Answer" />, and keeps what was sent. An exception answered is thrown.</summary>
    protected sealed class ScriptedSender : IScopedSender
    {
        public Func<object, object?> Answer { get; set; } = _ => null;

        public List<object> Sent { get; } = [];

        /// <summary>When set, a request it matches waits for <see cref="Gate" />.</summary>
        public Func<object, bool>? Held { get; set; }

        public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public IEnumerable<T> SentOf<T>() => Sent.OfType<T>();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            if (Held?.Invoke(request) == true)
            {
                await Gate.Task;
            }

            var answer = Answer(request);
            if (answer is Exception exception)
            {
                throw exception;
            }

            return answer is null ? default! : (TResponse)answer;
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            Sent.Add(request);
            return Task.CompletedTask;
        }
    }
}
