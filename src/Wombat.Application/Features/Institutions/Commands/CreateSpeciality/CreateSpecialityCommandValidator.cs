using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.CreateSpeciality;

public sealed class CreateSpecialityCommandValidator : AbstractValidator<CreateSpecialityCommand>
{
    public CreateSpecialityCommandValidator()
    {
        RuleFor(command => command.InstitutionId).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(2000);
    }
}
