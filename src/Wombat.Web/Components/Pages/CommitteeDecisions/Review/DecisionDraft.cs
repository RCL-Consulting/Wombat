using Wombat.Domain.CommitteeDecisions;

namespace Wombat.Web.Components.Pages.CommitteeDecisions.Review;

/// <summary>
/// The committee's decision as the chair types it (T361, flow 07; R2-Spec "Decision form"): kept by the page across the
/// re-reads after every action, so a refusal or a Refresh never loses a word (T324).
/// </summary>
public sealed class DecisionDraft
{
    /// <summary>The progression category, or null until chosen; an entrustment-only review takes none (T131 slice 5).</summary>
    public CommitteeDecisionCategory? Category { get; set; }

    public string Rationale { get; set; } = string.Empty;

    /// <summary>The decision's one sentence about the EPAs it does not decide (D53, E1), optional.</summary>
    public string? UndecidedNote { get; set; }

    public string? Conditions { get; set; }

    /// <summary>The members ticked as present, other than the chair, who always is (T165).</summary>
    public HashSet<string> PresentUserIds { get; } = new(StringComparer.Ordinal);

    /// <summary>The refusal at the form's top, before any dialog or from the server, or null (T324).</summary>
    public string? Refusal { get; set; }

    /// <summary>The fields that refusal names, each with its own message: "category", "rationale".</summary>
    public Dictionary<string, string> FieldRefusals { get; } = new(StringComparer.Ordinal);

    public void ClearRefusal()
    {
        Refusal = null;
        FieldRefusals.Clear();
    }

    public void Tick(string userId, bool present)
    {
        if (present)
        {
            PresentUserIds.Add(userId);
        }
        else
        {
            PresentUserIds.Remove(userId);
        }
    }

    /// <summary>The keys of <see cref="FieldRefusals" />.</summary>
    public static class Fields
    {
        public const string Category = "category";
        public const string Rationale = "rationale";
    }
}
