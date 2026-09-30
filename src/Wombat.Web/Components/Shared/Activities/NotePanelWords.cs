using Wombat.Application.Features.Activities.Dtos;

namespace Wombat.Web.Components.Shared.Activities;

/// <summary>
/// The note panel's words, one pattern for every move that needs a note (T350, note 4; round 1, E2; Spec § 1): "Decline
/// this request" or "Return this reflection", "Note for Sipho Ndlovu", "Decline with this note", "Keep the request". The
/// part is what the form's first section calls the author's part, by its first word (<see cref="PartOf" />):
/// "request", "reflection", "review".
/// </summary>
public static class NotePanelWords
{
    /// <summary>
    /// The part a heading and Keep name: the first word of <see cref="ActivityPageModel.AuthorPart" /> ("review request"
    /// is "review"), or "activity" when there is none.
    /// </summary>
    public static string PartOf(string? authorPart)
    {
        var first = (authorPart ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrEmpty(first) ? "activity" : first;
    }

    /// <summary>The panel's heading: "Decline this request", "Return this reflection", "Return this review".</summary>
    public static string Heading(ActivityActionDto action, string part)
    {
        ArgumentNullException.ThrowIfNull(action);

        return $"{action.Label} this {part}";
    }

    /// <summary>The quiet button that closes the panel and keeps the work as it is: "Keep the request".</summary>
    public static string Keep(string part) => $"Keep the {part}";

    /// <summary>The note's label, for both moves: "Note for Sipho Ndlovu".</summary>
    public static string Label(string subjectName) => $"Note for {subjectName}";

    /// <summary>The send: "Decline with this note", "Return with this note".</summary>
    public static string Send(ActivityActionDto action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return $"{action.Label} with this note";
    }

    /// <summary>
    /// The send's weight: <c>btn-danger</c> only for a move that ends the activity (<see cref="ActivityActionDto.TargetIsFinal" />,
    /// a decline), else <c>btn-primary</c> (a return, which hands it back; round 1, E2).
    /// </summary>
    public static string SendClass(ActivityActionDto action)
    {
        ArgumentNullException.ThrowIfNull(action);

        return action.TargetIsFinal ? "btn-danger" : "btn-primary";
    }

    /// <summary>The note's help: "Sipho Ndlovu reads it on the activity's page. It is kept with the activity's history."</summary>
    public static string Help(string subjectName)
        => $"{subjectName} reads it on the activity's page. It is kept with the activity's history.";
}
