namespace Wombat.Domain.Activities.Schema;

/// <param name="ObservationDateField">
/// The key of the <see cref="FieldType.Date" /> field that records WHEN THE ENCOUNTER HAPPENED, as
/// distinct from when the paperwork was filed. Null when the form has no single encounter date — a
/// reflection, or a QI project spanning months. (T119)
/// </param>
/// <param name="RatedLevelField">
/// The key of the <see cref="FieldType.Scale" /> field carrying THE entrustment rating for this form,
/// as distinct from the component scales rated alongside it. Null when the form asserts no entrustment
/// level — a reflection, a procedure log, a journal club. (T126)
/// </param>
/// <param name="EvidenceEpaField">
/// The key of the <see cref="FieldType.Epa" /> field naming THE EPA this activity is evidence for. It is
/// what <c>Activity.EpaId</c> is stamped from, so a list, a committee snapshot or any other reader can say
/// which EPA a row is about without re-parsing <c>DataJson</c>. Null when the form is about no single EPA —
/// a journal club, a procedure log, a QI project. (T137)
/// <para>
/// Named for what it answers rather than after the credit directive's <c>epa_field</c>, because the two are
/// different properties that must name the SAME field: this one holds for every type, including the ones
/// that credit nothing (<c>msf_cpsa</c>, the reflections), while a directive's <c>epa_field</c> only says
/// where credit looks. <c>EvidenceEpa.EnsureCreditAgrees</c> refuses, at save and at publish, a type
/// whose credit reads its EPA from any other field, so the EPA a list shows and the EPA credit lands on
/// cannot differ.
/// </para>
/// </param>
/// <remarks>
/// A root pointer rather than a per-field flag, because it is single by construction: a flag would let
/// two fields claim the role and force the parser to police it. On the schema rather than the credit
/// rules, because a date drives the portfolio export, the committee review window and the sampling
/// warnings even for types that credit nothing.
/// <para>
/// <see cref="RatedLevelField" /> follows the same shape for the same reason, and cannot be inferred:
/// the generic <c>mini_cex</c>, <c>dops</c> and <c>acat</c> seeds each declare six scale fields and
/// <c>cbd</c> five, of which exactly one is the overall entrustment judgement. Before T126 the only
/// thing that knew which was the credit rules' <c>minimum_level_field</c> — so a type that credits
/// nothing had no stated rated field at all, and a reader holding an ordinal could not say which
/// ladder it sat on. Declaring it here makes the answer a property of the form rather than of the
/// rules that happen to score it.
/// </para>
/// </remarks>
public sealed record FormSchema(
    int Version,
    IReadOnlyList<FormSection> Sections,
    string? ObservationDateField = null,
    string? RatedLevelField = null,
    string? EvidenceEpaField = null)
{
    /// <summary>
    /// The words a user sees for the field <paramref name="fieldKey" />: its label, followed by its section's title in
    /// brackets when another field in the form carries the same label, or the key itself when the schema declares no
    /// such field or the field's label is blank.
    /// </summary>
    /// <remarks>
    /// The one place a refusal or a reason turns a field key into what the form shows (T172). A validation refusal, a
    /// disabled action's reason (T107), the nominee gate (T102), the tool gate (T122) and the subject-as-actor refusal all
    /// name fields through it, so no two of them can call the same field by different names. The parser refuses a blank
    /// label, so the key is what a user sees only for a schema built in code, or a key the schema does not declare.
    /// Keys are unique once a type can be saved (<c>ActorFieldRules.EnsurePublishable</c>), so the first match is the
    /// field.
    /// <para>
    /// Labels are not unique: <c>qi_project</c> has a "Plan", "Do", "Study" and "Act" under each of its three PDSA
    /// cycles, and the builder lets any type do the same. The form tells them apart by the section each sits under, so a
    /// repeated label is named with that section's title, "Plan (PDSA cycle 1)", which is what the user sees; a label
    /// used once is named alone. Labels are compared as a reader sees them: trimmed and ignoring case. The title follows
    /// in brackets rather than after a colon, because a refusal already uses the colon between the field and what is
    /// wrong with it.
    /// </para>
    /// </remarks>
    public string FieldLabel(string fieldKey)
    {
        FormSection? owner = null;
        FormField? field = null;

        foreach (var section in Sections)
        {
            field = section.Fields.FirstOrDefault(candidate => string.Equals(candidate.Key, fieldKey, StringComparison.Ordinal));
            if (field is not null)
            {
                owner = section;
                break;
            }
        }

        if (field is null || owner is null || string.IsNullOrWhiteSpace(field.Label))
        {
            return fieldKey;
        }

        var label = field.Label.Trim();

        var repeated = Sections
            .SelectMany(section => section.Fields)
            .Any(other => !string.Equals(other.Key, fieldKey, StringComparison.Ordinal) &&
                          string.Equals(other.Label?.Trim(), label, StringComparison.OrdinalIgnoreCase));

        return repeated && !string.IsNullOrWhiteSpace(owner.Title)
            ? $"{label} ({owner.Title.Trim()})"
            : label;
    }
}
