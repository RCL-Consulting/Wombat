using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.CreateSpeciality;

public sealed class CreateSpecialityCommandValidator : AbstractValidator<CreateSpecialityCommand>
{
    public CreateSpecialityCommandValidator()
    {
        RuleFor(command => command.CollegeId).GreaterThan(0);

        // T394 (flow 16, N1, T324): in the label's words, shown under the field they are about.
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(SpecialityFieldMessages.NameRequired)
            .MaximumLength(SpecialityFieldMessages.NameMaxLength).WithMessage(SpecialityFieldMessages.NameTooLong);
        RuleFor(command => command.Description)
            .MaximumLength(SpecialityFieldMessages.DescriptionMaxLength).WithMessage(SpecialityFieldMessages.DescriptionTooLong);
    }
}
