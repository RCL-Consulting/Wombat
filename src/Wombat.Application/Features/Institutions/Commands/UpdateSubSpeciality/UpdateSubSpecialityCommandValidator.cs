using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;

public sealed class UpdateSubSpecialityCommandValidator : AbstractValidator<UpdateSubSpecialityCommand>
{
    public UpdateSubSpecialityCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.SpecialityId).GreaterThan(0);

        // T394 (flow 16, N1, T324): in the label's words, shown under the field they are about.
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(SpecialityFieldMessages.NameRequired)
            .MaximumLength(SpecialityFieldMessages.NameMaxLength).WithMessage(SpecialityFieldMessages.NameTooLong);
        RuleFor(command => command.Description)
            .MaximumLength(SpecialityFieldMessages.DescriptionMaxLength).WithMessage(SpecialityFieldMessages.DescriptionTooLong);
    }
}
