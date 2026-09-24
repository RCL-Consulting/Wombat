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
    string? EvidenceEpaField = null);
