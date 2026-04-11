using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.CreateSubSpeciality;

public sealed class CreateSubSpecialityCommandValidator : AbstractValidator<CreateSubSpecialityCommand>
{
    public CreateSubSpecialityCommandValidator()
    {
        RuleFor(command => command.SpecialityId).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(2000);
    }
}
