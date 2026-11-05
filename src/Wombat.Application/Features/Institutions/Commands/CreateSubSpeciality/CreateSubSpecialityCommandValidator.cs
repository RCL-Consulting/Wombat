using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;

public sealed class CreateSubSpecialityCommandValidator : AbstractValidator<CreateSubSpecialityCommand>
{
    public CreateSubSpecialityCommandValidator()
    {
        RuleFor(command => command.SpecialityId).GreaterThan(0);

        // T394 (flow 16, N1, T324): in the label's words, shown under the field they are about.
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(SpecialityFieldMessages.NameRequired)
            .MaximumLength(SpecialityFieldMessages.NameMaxLength).WithMessage(SpecialityFieldMessages.NameTooLong);
        RuleFor(command => command.Description)
            .MaximumLength(SpecialityFieldMessages.DescriptionMaxLength).WithMessage(SpecialityFieldMessages.DescriptionTooLong);

        // An existing scale is the handler's to read, before it stages anything (T394, Q1); an id is at least positive.
        RuleFor(command => command.DefaultEntrustmentScaleId).GreaterThan(0).When(command => command.DefaultEntrustmentScaleId.HasValue);
    }
}
