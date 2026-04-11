namespace Wombat.Domain.Activities.Schema;

public sealed record FormSchema(
    int Version,
    IReadOnlyList<FormSection> Sections);
