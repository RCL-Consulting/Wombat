using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

public sealed class UpdateInstitutionCommandValidator : AbstractValidator<UpdateInstitutionCommand>
{
    public UpdateInstitutionCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.ShortCode).NotEmpty().MaximumLength(32);
        RuleFor(command => command.ContactEmail).MaximumLength(320);
    }
}
