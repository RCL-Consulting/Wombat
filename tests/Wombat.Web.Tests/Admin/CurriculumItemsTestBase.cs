using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Bunit;
using Bunit.TestDoubles;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Curricula;
using Wombat.Web.Navigation;
using Wombat.Web.Services;
using FakeSender = Wombat.Web.Tests.Admin.CurriculumItemsFakeSender;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// What every test of a curriculum's items page and an item's page of its own needs (T392, flow 15, Q14): a caller, the two
/// pages rendered on <see cref="FakeSender" />, and the list's rows read by their EPA.
/// </summary>
public abstract partial class CurriculumItemsTestBase : TestContext
{
    /// <summary>What <c>ElementReference.FocusAsync</c> calls.</summary>
    protected const string FocusIdentifier = "Blazor._internal.domWrapper.focus";

    private readonly TestAuthorizationContext _auth;
    private string _role = WombatRoles.CollegeAdmin;

    protected CurriculumItemsTestBase()
    {
        // The dialog and the focus calls go to the browser; a test that reads them reads the invocations.
        JSInterop.Mode = JSRuntimeMode.Loose;
        _auth = this.AddTestAuthorization();
        SignIn(WombatRoles.CollegeAdmin);
    }

    /// <summary>The caller, before the page is rendered; the College admin unless a test says otherwise.</summary>
    protected void SignIn(string role)
    {
        _role = role;
        _auth.SetAuthorized($"{role}@test");
        _auth.SetRoles(role);
        _auth.SetClaims(new Claim(ClaimTypes.NameIdentifier, $"{role}-1"));
    }

    /// <summary>The items list, at <paramref name="uri" /> when a test arrives with a token, drawn once its read is in.</summary>
    private protected IRenderedComponent<CurriculumItemsEdit> RenderList(FakeSender sender, string? uri = null)
    {
        Services.AddSingleton<IScopedSender>(sender);
        if (uri is not null)
        {
            Services.GetRequiredService<NavigationManager>().NavigateTo(uri);
        }

        var cut = RenderComponent<CurriculumItemsEdit>(parameters => Shell(parameters, typeof(CurriculumItemsEdit))
            .Add(page => page.Id, FakeSender.CurriculumId));
        cut.WaitForState(() => cut.FindAll("h1").Count > 0 && cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    /// <summary>An item's page: the add when <paramref name="itemId" /> is null, else that item's.</summary>
    private protected IRenderedComponent<CurriculumItemPage> RenderItem(FakeSender sender, int? itemId = null)
    {
        Services.AddSingleton<IScopedSender>(sender);
        var cut = RenderComponent<CurriculumItemPage>(parameters => Shell(parameters, typeof(CurriculumItemPage))
            .Add(page => page.Id, FakeSender.CurriculumId)
            .Add(page => page.ItemId, itemId));
        cut.WaitForState(() => cut.FindAll(".loading-line").Count == 0);
        return cut;
    }

    /// <summary>What the shell cascades, so the header draws its trail from the owner table (NavOwners).</summary>
    private ComponentParameterCollectionBuilder<TPage> Shell<TPage>(ComponentParameterCollectionBuilder<TPage> parameters, Type page)
        where TPage : IComponent
    {
        var acting = ActingRoleResolver.Resolve(_role, [_role]);
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, $"{_role}-1"), new Claim(ClaimTypes.Role, _role)], "Test"));
        return parameters
            .AddCascadingValue(acting)
            .AddCascadingValue(NavItems.For(acting, user))
            .AddCascadingValue(new RouteData(page, new Dictionary<string, object?>()));
    }

    protected NavigationManager Navigation => Services.GetRequiredService<NavigationManager>();

    /// <summary>The list's row whose EPA cell starts with <paramref name="epaCode" />.</summary>
    protected static IElement Row(IRenderedFragment cut, string epaCode)
        => cut.FindAll("tbody tr").Single(row => Text(row.QuerySelector("th[scope=row]")!).StartsWith(epaCode, StringComparison.Ordinal));

    /// <summary>A row's cell by its column's label (<c>data-label</c>).</summary>
    protected static IElement Cell(IRenderedFragment cut, string epaCode, string column)
        => Row(cut, epaCode).QuerySelectorAll("td").Single(cell => cell.GetAttribute("data-label") == column);

    /// <summary>The words of a row's actions cell: its links' and buttons' visible words, then its note.</summary>
    protected static IReadOnlyList<string> Actions(IElement row)
        => row.QuerySelectorAll(".actions-cell a, .actions-cell button, .actions-cell span").Select(Text).ToList();

    protected static string Text(IElement element) => Spaces().Replace(element.TextContent, " ").Trim();

    protected static string Id(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>The focus calls made with an element reference (an ActionResult's).</summary>
    protected IReadOnlyList<JSRuntimeInvocation> FocusCalls()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == FocusIdentifier).ToList();

    /// <summary>The ids focused by <c>PageFocus.FocusByIdAsync</c>.</summary>
    protected IReadOnlyList<string> FocusedIds()
        => JSInterop.Invocations.Where(invocation => invocation.Identifier == "wombat.focusById")
            .Select(invocation => (string)invocation.Arguments[0]!).ToList();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Spaces();
}
