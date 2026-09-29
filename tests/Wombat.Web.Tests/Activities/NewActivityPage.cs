using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Wombat.Web.Components.Pages.Activities;

namespace Wombat.Web.Tests.Activities;

/// <summary>
/// How a test opens Log an activity on a type's form (T342, flow 03, Q1). Since flow 03 a type is chosen by a link to
/// <c>/activities/new?type=&lt;key&gt;</c>, not a select: the page with no <c>?type</c> is the instrument picker.
/// </summary>
internal static class NewActivityPage
{
    /// <summary>Moves the address to the type's form (and to a declined request's copy, with <paramref name="from" />).</summary>
    public static void NavigateTo(TestContext context, string? typeKey, int? from = null)
    {
        var navigation = context.Services.GetRequiredService<NavigationManager>();
        var query = new List<string>();
        if (typeKey is not null)
        {
            query.Add($"type={Uri.EscapeDataString(typeKey)}");
        }

        if (from is not null)
        {
            query.Add($"from={from}");
        }

        navigation.NavigateTo(query.Count == 0 ? "/activities/new" : $"/activities/new?{string.Join('&', query)}");
    }

    /// <summary>Renders the page on the type's form and waits for <paramref name="readySelector" /> to be drawn.</summary>
    public static IRenderedComponent<NewActivity> Open(TestContext context, string typeKey, string readySelector)
    {
        NavigateTo(context, typeKey);
        var cut = context.RenderComponent<NewActivity>();
        cut.WaitForState(() => cut.FindAll(readySelector).Count > 0);
        return cut;
    }

    /// <summary>The action bar's buttons, the move first.</summary>
    public static IReadOnlyList<IElement> MoveButtons(IRenderedFragment cut)
        => cut.FindAll(".form-actions--moves button").ToList();

    /// <summary>The primary move's button: the first in the bar.</summary>
    public static IElement Primary(IRenderedFragment cut) => MoveButtons(cut)[0];

    /// <summary>
    /// A move's button by the words a test knows it by: "Submit" is the primary move whatever it reads now ("Submit to
    /// Dr One", "Submitting…"; T342, C3), and "Save draft" is Save draft while it runs too ("Saving…").
    /// </summary>
    public static IElement FindMove(IRenderedFragment cut, string label)
        => label switch
        {
            "Submit" => Primary(cut),
            "Save draft" => SaveDraft(cut) ?? throw new InvalidOperationException("The page offers no Save draft."),
            _ => MoveButtons(cut).First(button => button.TextContent.Trim() == label)
        };

    /// <summary>Save draft, when the page offers it.</summary>
    public static IElement? SaveDraft(IRenderedFragment cut)
        => MoveButtons(cut).FirstOrDefault(button => button.TextContent.Trim() is "Save draft" or "Saving…");
}
