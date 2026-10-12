namespace Wombat.Web.Components.Pages.CommitteeDecisions.Review;

/// <summary>
/// What the chair has typed into one agenda line's staging form, kept by the page while another line is open (T361, E9:
/// "Draft kept"), and while the form is swapped for the evidence reader (C3: Back restores the form as typed). It lives in
/// the circuit only: no navigation, no storage. Staging, Cancel or leaving the page ends it.
/// </summary>
public sealed class LineDraft
{
    /// <summary>The EPA staged on: the line's, or the foot form's choice (0 until chosen).</summary>
    public int EpaId { get; set; }

    /// <summary>The rung chosen, 0 until one is.</summary>
    public int AuthorisedLevelId { get; set; }

    /// <summary>Today on the South African calendar when the form opened (T325).</summary>
    public DateOnly IssuedOn { get; set; }

    public DateOnly? ExpiresOn { get; set; }

    public string Rationale { get; set; } = string.Empty;

    /// <summary>The ticked snapshot lines, in the order they were ticked (round 3 item 30 names them so).</summary>
    public List<int> EvidenceItemIds { get; } = [];

    /// <summary>Whether "Show the other N snapshot lines" is open (20).</summary>
    public bool OthersShown { get; set; }

    /// <summary>The refusal the last stage got, shown at the form's top, or null (4.18, 4.20).</summary>
    public string? Refusal { get; set; }

    /// <summary>
    /// The fields the last refusal names, by <see cref="Fields" /> key, each with its own message, marked on the field
    /// (4.18; T324: a field's message, never the model's property name).
    /// </summary>
    public Dictionary<string, string> FieldRefusals { get; } = new(StringComparer.Ordinal);

    /// <summary>Clears the last refusal, before the next press.</summary>
    public void ClearRefusal()
    {
        Refusal = null;
        FieldRefusals.Clear();
    }

    /// <summary>The keys of <see cref="FieldRefusals" />.</summary>
    public static class Fields
    {
        public const string Epa = "epa";
        public const string Level = "level";
        public const string Expires = "expires";
        public const string Rationale = "rationale";
    }

    public void Tick(int evidenceId, bool ticked)
    {
        EvidenceItemIds.Remove(evidenceId);
        if (ticked)
        {
            EvidenceItemIds.Add(evidenceId);
        }
    }
}
