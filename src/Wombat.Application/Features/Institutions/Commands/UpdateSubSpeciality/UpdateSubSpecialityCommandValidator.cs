using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.UpdateSubSpeciality;

public sealed class UpdateSubSpecialityCommandValidator : AbstractValidator<UpdateSubSpecialityCommand>
{
    public UpdateSubSpecialityCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.SpecialityId).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Description).MaximumLength(2000);
    }
}
