namespace Wombat.Domain.Activities.Schema;

/// <param name="ObservationDateField">
/// The key of the <see cref="FieldType.Date" /> field that records WHEN THE ENCOUNTER HAPPENED, as
/// distinct from when the paperwork was filed. Null when the form has no single encounter date — a
/// reflection, or a QI project spanning months. (T119)
/// </param>
/// <remarks>
/// A root pointer rather than a per-field flag, because it is single by construction: a flag would let
/// two fields claim the role and force the parser to police it. On the schema rather than the credit
/// rules, because a date drives the portfolio export, the committee review window and the sampling
/// warnings even for types that credit nothing.
/// </remarks>
public sealed record FormSchema(
    int Version,
    IReadOnlyList<FormSection> Sections,
    string? ObservationDateField = null);
