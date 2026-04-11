namespace Wombat.Domain.Activities.Schema;

public sealed record FieldValidation(
    decimal? Min,
    decimal? Max,
    string? Regex,
    int? MinLength,
    int? MaxLength);
