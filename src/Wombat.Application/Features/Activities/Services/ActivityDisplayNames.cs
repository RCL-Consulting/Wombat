using System.Globalization;

namespace Wombat.Application.Features.Activities.Services;

/// <summary>
/// An activity's name as the h1, the browser tab, the last crumb and My activities' link read it: "Type · EPA · date"
/// (T342, E7, E9). Pure, so the lists and the detail name an activity alike.
/// </summary>
/// <remarks>
/// <para>
/// A segment is left out when the form has no such field: a type about no single EPA (a journal club) has no EPA
/// segment, and one with no encounter date field no date segment. A form that has the field but no value yet says so
/// (E9): "Mini-CEX (Paediatrics) · no EPA yet · no date yet" for a draft saved with neither, "· no date" when only the
/// date is missing.
/// </para>
/// <para>
/// Two of one registrar's activities can share all three, a declined request and its re-filing to someone else. Then
/// each is told apart by its nominee (<see cref="WithNominee" />), and only then (E7): the second line of the link
/// already names the nominee on every other row.
/// </para>
/// </remarks>
public static class ActivityDisplayNames
{
    /// <summary>What joins the segments.</summary>
    public const string Separator = " · ";

    /// <summary>"Type · EPA · date", each segment the form has.</summary>
    /// <param name="epaCode">The stamped EPA's code, or null when none is stamped.</param>
    /// <param name="observedOnDeclared">False when nobody stated the encounter date (it is only the creation day).</param>
    /// <param name="formHasEpa">Whether the pinned form has an EPA it is evidence for (<c>evidence_epa_field</c>).</param>
    /// <param name="formHasDate">Whether the pinned form has an encounter date field (<c>observation_date_field</c>).</param>
    public static string Compose(
        string typeName,
        string? epaCode,
        DateOnly observedOn,
        bool observedOnDeclared,
        bool formHasEpa = true,
        bool formHasDate = true)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        var segments = new List<string>(3) { typeName };
        var epaMissing = formHasEpa && string.IsNullOrWhiteSpace(epaCode);
        if (formHasEpa)
        {
            segments.Add(epaMissing ? "no EPA yet" : epaCode!);
        }

        if (formHasDate)
        {
            segments.Add(observedOnDeclared
                ? observedOn.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : epaMissing ? "no date yet" : "no date");
        }

        return string.Join(Separator, segments);
    }

    /// <summary>
    /// The name with " · nominee" when <paramref name="sharesTheRest" /> and there is a nominee to add; the name alone
    /// otherwise.
    /// </summary>
    public static string WithNominee(string name, string? nomineeName, bool sharesTheRest)
        => sharesTheRest && !string.IsNullOrWhiteSpace(nomineeName) ? name + Separator + nomineeName : name;

    /// <summary>
    /// What two of a subject's activities must share to be named alike, and so to need their nominee (E7): the subject, the
    /// type, the stamped EPA and the stated encounter date. The composed name is a function of these (and of the pinned
    /// form, which one type's versions share in practice), so the key is read from columns and needs no form.
    /// </summary>
    public static (string SubjectUserId, int ActivityTypeId, int? EpaId, DateOnly? ObservedOn) CollisionKey(
        string subjectUserId, int activityTypeId, int? epaId, DateOnly observedOn, bool observedOnDeclared)
        => (subjectUserId, activityTypeId, epaId, observedOnDeclared ? observedOn : null);
}
