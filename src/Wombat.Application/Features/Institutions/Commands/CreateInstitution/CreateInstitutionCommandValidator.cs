using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.CreateInstitution;

public sealed class CreateInstitutionCommandValidator : AbstractValidator<CreateInstitutionCommand>
{
    public CreateInstitutionCommandValidator()
    {
        RuleFor(command => command.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.ShortCode).NotEmpty().MaximumLength(32);
        RuleFor(command => command.ContactEmail).MaximumLength(320);
    }
}
