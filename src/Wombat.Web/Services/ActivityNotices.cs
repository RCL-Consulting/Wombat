namespace Wombat.Web.Services;

/// <summary>
/// A one-shot message about an activity, carried across a navigation: <c>/activities/new</c> posts what became of
/// the activity it just created, and <c>/activities/{id}</c> takes it on arrival (T127, T143, T148).
/// </summary>
/// <remarks>
/// <para>
/// Scoped, so one instance lives as long as the circuit. That is enough only because <c>Routes</c> is rendered
/// InteractiveServer for the whole app (<c>App.razor</c>): a <c>NavigateTo</c> without <c>forceLoad</c> stays in the
/// same circuit, so the page that takes the notice resolves the same instance as the page that posted it. A reload, a
/// new tab or a forced load starts a new circuit and the notice is lost. That is acceptable, because the activity page
/// states the activity's state either way.
/// </para>
/// <para>
/// The message never travels in the query string. A link is something anyone can craft, and a page that printed
/// whatever the link said would display any text under the product's name.
/// </para>
/// </remarks>
public sealed class ActivityNotices
{
    private readonly Dictionary<int, ActivityNotice> _notices = [];

    /// <summary>Leaves a notice for the activity, replacing any it had not yet shown.</summary>
    /// <param name="refusedFieldKeys">The fields a refusal the notice reports named, so the activity's page marks them (T263).</param>
    public void Post(int activityId, string kind, string message, IReadOnlyList<string>? refusedFieldKeys = null)
        => _notices[activityId] = new ActivityNotice(kind, message, refusedFieldKeys);

    /// <summary>Returns the activity's notice and removes it, so it is shown once; null when there is none.</summary>
    public ActivityNotice? Take(int activityId)
        => _notices.Remove(activityId, out var notice) ? notice : null;
}

/// <summary>A notice's <see cref="Kind" /> is an <c>Alert</c> kind: info, success, warning or danger.</summary>
/// <param name="RefusedFieldKeys">
/// The fields named by the refusal the notice reports, by schema key: the submit refused straight after the create (T127),
/// whose notice asks the author to fix the fields below. The activity's page marks them (T263). Null for any other notice,
/// and for a refusal that names no field.
/// </param>
public sealed record ActivityNotice(string Kind, string Message, IReadOnlyList<string>? RefusedFieldKeys = null);
