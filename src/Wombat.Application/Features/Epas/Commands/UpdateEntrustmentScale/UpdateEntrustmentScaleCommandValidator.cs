using FluentValidation;
using Wombat.Domain.Epas;

namespace Wombat.Application.Features.Epas.Commands.UpdateEntrustmentScale;

public sealed class UpdateEntrustmentScaleCommandValidator : AbstractValidator<UpdateEntrustmentScaleCommand>
{
    public UpdateEntrustmentScaleCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        // T394 (flow 16, lane B; T324, N1): every failure in its field's words, and a rung's under that rung's own key
        // (Levels[3].Label), so the page puts it under that rung's Label. The page asks the same before it sends.
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(EntrustmentScaleBindings.NameRequired)
            .MaximumLength(200).WithMessage(EntrustmentScaleBindings.NameTooLong)
            .Must(name => !ScaleBinding.IsReservedName(name)).WithMessage(EntrustmentScaleBindings.ReservedNameRefusal);
        RuleFor(command => command.Description).MaximumLength(2000).WithMessage(EntrustmentScaleBindings.DescriptionTooLong);

        RuleFor(command => command.Levels)
            .Must(levels => levels is { Count: >= 2 }).WithMessage(EntrustmentScaleBindings.TooFewRungs)
            .Must(BeContiguousFromOne).WithMessage("Level orders must form a contiguous range starting at 1.")
            .Custom(RefuseRepeatedLabels);

        RuleForEach(command => command.Levels).ChildRules(level =>
        {
            level.RuleFor(entry => entry.Order).GreaterThan(0);
            level.RuleFor(entry => entry.Label)
                .NotEmpty().WithMessage(EntrustmentScaleBindings.LabelRequired)
                .MaximumLength(200).WithMessage(EntrustmentScaleBindings.LabelTooLong);
            level.RuleFor(entry => entry.Description).MaximumLength(2000).WithMessage(EntrustmentScaleBindings.DescriptionTooLong);
        });
    }

    private static bool BeContiguousFromOne(IReadOnlyList<EntrustmentLevelUpdate> levels)
    {
        if (levels.Count == 0) return false;
        var orders = levels.Select(level => level.Order).OrderBy(order => order).ToList();
        return orders.SequenceEqual(Enumerable.Range(1, levels.Count));
    }

    /// <summary>A label a lower rung already has (ignoring case and the ends' spaces), refused under the later rung (R3-C03b).</summary>
    private static void RefuseRepeatedLabels(IReadOnlyList<EntrustmentLevelUpdate> levels, ValidationContext<UpdateEntrustmentScaleCommand> context)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < levels.Count; index++)
        {
            var label = levels[index].Label?.Trim() ?? string.Empty;
            if (label.Length > 0 && !seen.Add(label))
            {
                context.AddFailure(EntrustmentScaleBindings.RungField(index, "Label"), EntrustmentScaleBindings.LabelTaken(label));
            }
        }
    }
}
