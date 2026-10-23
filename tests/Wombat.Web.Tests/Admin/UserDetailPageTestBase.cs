using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Common.Security;
using Wombat.Application.Features.Institutions.Queries.GetAdoptedDisciplinesForInstitution;
using Wombat.Application.Features.Users;
using Wombat.Application.Features.Users.Queries.GetRequestsWaitingForUser;
using Wombat.Application.Features.Users.Queries.GetUserById;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Users;
using Wombat.Web.Navigation;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// The user page's fixture (T376, flow 12; boards P01–P36): Nolwazi Mbatha, Institutional admin at Kgosi Kgari Teaching
/// Hospital (or the Administrator), opens a person's page, whose reads and commands a scripted sender answers.
/// </summary>
public abstract class UserDetailPageTestBase : TestContext
{
    protected const int Kgk = 1;
    protected const string KgkName = "Kgosi Kgari Teaching Hospital";
    protected const string Mbatha = "mbatha";

    /// <summary>What ElementReference.FocusAsync calls: an ActionResult taking the focus.</summary>
    protected const string ElementFocus = "Blazor._internal.domWrapper.focus";

    private readonly TestAuthorizationContext _auth;

    protected UserDetailPageTestBase()
    {
        _auth = this.AddTestAuthorization();
        Sender = new UserPageSender();
        Services.AddSingleton<IScopedSender>(Sender);

        // Dialogs, the password toggle and the focus calls go to the browser.
        JSInterop.Mode = JSRuntimeMode.Loose;
        SignIn(WombatRoles.InstitutionalAdmin);
    }

    protected UserPageSender Sender { get; }

    /// <summary>Signs in as Mbatha in <paramref name="roles" />; the Administrator carries no institution.</summary>
    protected void SignIn(params string[] roles)
    {
        _auth.SetAuthorized("mbatha@kgk.wombat.local");
        _auth.SetRoles(roles);
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, Mbatha) };
        if (!roles.Contains(WombatRoles.Administrator))
        {
            claims.Add(new Claim(WombatClaimTypes.InstitutionId, Kgk.ToString()));
        }

        _auth.SetClaims(claims.ToArray());
    }

    /// <summary>Opens <paramref name="person" />'s page at <paramref name="query" /> (as "?section=training"), once it has loaded.</summary>
    protected IRenderedComponent<UserDetail> Open(UserDetailDto? person, string query = "", string userId = "zulu")
    {
        Sender.User = person;
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo($"/admin/users/{person?.UserId ?? userId}{query}");
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var cut = RenderComponent<UserDetail>(parameters => parameters.Add(page => page.UserId, person?.UserId ?? userId));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    /// <summary>
    /// Opens the page as the shell does, with her acting role, her menu and the route cascaded, so the header draws its
    /// trail (the owner table: Home › Users › the person).
    /// </summary>
    protected IRenderedComponent<UserDetail> OpenWithTrail(UserDetailDto person, string query = "")
    {
        Sender.User = person;
        Services.GetRequiredService<FakeNavigationManager>().NavigateTo($"/admin/users/{person.UserId}{query}");
        SetRendererInfo(new RendererInfo("Server", isInteractive: true));
        var role = WombatRoles.InstitutionalAdmin;
        var acting = ActingRoleResolver.Resolve(role, [role]);
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "Test"));
        var cut = RenderComponent<UserDetail>(parameters => parameters
            .Add(page => page.UserId, person.UserId)
            .AddCascadingValue(acting)
            .AddCascadingValue(NavItems.For(acting, user))
            .AddCascadingValue(new RouteData(typeof(UserDetail), new Dictionary<string, object?> { ["UserId"] = person.UserId })));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    /// <summary>
    /// Thandi Zulu at 2.13: a Committee member kept to Paediatrics, who signs in with a password. Each test changes what
    /// its board draws.
    /// </summary>
    protected static UserDetailDto Zulu(params string[] roles) => new(
        "zulu", "zulu@kgk.wombat.local", "Thandi", "Zulu", Kgk, KgkName, [10], [],
        roles.Length == 0 ? [WombatRoles.CommitteeMember] : roles, false)
    {
        Scope = "Paediatrics",
        SpecialityNames = ["Paediatrics"],
        SubSpecialityNames = []
    };

    /// <summary>The scope Zulu holds once Assessor is added in Paediatrics / Paediatrics (P05).</summary>
    protected static UserDetailDto ZuluAssessor() => Zulu(WombatRoles.CommitteeMember, WombatRoles.Assessor) with
    {
        SubSpecialityIds = [20],
        Scope = "Paediatrics / Paediatrics",
        SubSpecialityNames = ["Paediatrics"]
    };

    protected static AdoptedDisciplinesDto KgkAdopted { get; } = new(
        [new AdoptedSpecialityDto(10, "Paediatrics")],
        [new AdoptedSubSpecialityDto(20, 10, "Paediatrics")]);

    protected static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();

    protected static IElement Button(IRenderedFragment cut, string words)
        => cut.FindAll("button").Single(button => Text(button) == words && button.Closest("dialog") is null);

    protected static IElement Named(IRenderedFragment cut, string name)
        => cut.FindAll("button").Single(button => button.GetAttribute("aria-label") == name);

    protected static IElement DialogButton(IRenderedFragment cut, string words)
        => cut.FindAll("dialog button").Single(button => Text(button) == words);

    /// <summary>The open dialog: the one whose title is <paramref name="title" />.</summary>
    protected static IElement Dialog(IRenderedFragment cut, string title)
        => cut.FindAll("dialog").Single(dialog => Text(dialog.QuerySelector("h2")!) == title);

    /// <summary>A part of Sign-in and access, by its h3.</summary>
    protected static IElement Part(IRenderedFragment cut, string heading)
        => cut.FindAll(".section-part").Single(part => Text(part.QuerySelector("h3")!) == heading);

    /// <summary>A section, by its h2.</summary>
    protected static IElement Section(IRenderedFragment cut, string heading)
        => cut.FindAll("section.user-section").Single(section => Text(section.QuerySelector("h2")!) == heading);

    protected int ElementFocusCalls() => JSInterop.Invocations.Count(invocation => invocation.Identifier == ElementFocus);

    protected IReadOnlyList<string> FocusedIds()
        => JSInterop.Invocations
            .Where(invocation => invocation.Identifier == "wombat.focusById")
            .Select(invocation => (string)invocation.Arguments[0]!)
            .ToList();
}

/// <summary>Answers the user page's reads from what a test sets, records every command, and refuses when told to.</summary>
public sealed class UserPageSender : IScopedSender
{
    public UserDetailDto? User { get; set; }

    /// <summary>The person as a command leaves them, read again after it: null keeps <see cref="User" />.</summary>
    public Func<object, UserDetailDto?>? After { get; set; }

    public AdoptedDisciplinesDto Adopted { get; set; } = AdoptedDisciplinesDto.None;

    public WaitingRequestsDto Waiting { get; set; } = WaitingRequestsDto.None;

    public Exception? ReadFault { get; set; }

    public Exception? Refusal { get; set; }

    public List<object> Received { get; } = [];

    public List<object> Commands { get; } = [];

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        Received.Add(request);
        object? answer = request switch
        {
            GetUserByIdQuery when ReadFault is not null => throw ReadFault,
            GetUserByIdQuery => User,
            GetAdoptedDisciplinesForInstitutionQuery => Adopted,
            GetRequestsWaitingForUserQuery => Waiting,
            _ => throw new NotSupportedException($"Unhandled request: {request.GetType().Name}")
        };

        return Task.FromResult((TResponse)answer!);
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        Received.Add(request);
        Commands.Add(request);
        if (Refusal is not null)
        {
            return Task.FromException(Refusal);
        }

        if (After?.Invoke(request) is { } after)
        {
            User = after;
        }

        return Task.CompletedTask;
    }
}
