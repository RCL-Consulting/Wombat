using FluentValidation;

namespace Wombat.Application.Features.Epas.Commands.CreateEntrustmentScale;

public sealed class CreateEntrustmentScaleCommandValidator : AbstractValidator<CreateEntrustmentScaleCommand>
{
    public CreateEntrustmentScaleCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(2000);

        RuleFor(command => command.Levels)
            .NotEmpty().WithMessage("At least two levels are required.")
            .Must(levels => levels.Count >= 2).WithMessage("At least two levels are required.")
            .Must(BeContiguousFromOne).WithMessage("Level orders must form a contiguous range starting at 1.")
            .Must(HaveDistinctLabels).WithMessage("Level labels must be unique within a scale.");

        RuleForEach(command => command.Levels).ChildRules(level =>
        {
            level.RuleFor(entry => entry.Order).GreaterThan(0);
            level.RuleFor(entry => entry.Label).NotEmpty().MaximumLength(200);
            level.RuleFor(entry => entry.Description).MaximumLength(2000);
        });
    }

    private static bool BeContiguousFromOne(IReadOnlyList<EntrustmentLevelInput> levels)
    {
        if (levels.Count == 0) return false;
        var orders = levels.Select(level => level.Order).OrderBy(order => order).ToList();
        return orders.SequenceEqual(Enumerable.Range(1, levels.Count));
    }

    private static bool HaveDistinctLabels(IReadOnlyList<EntrustmentLevelInput> levels)
        => levels.Select(level => level.Label?.Trim() ?? string.Empty)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() == levels.Count;
}
