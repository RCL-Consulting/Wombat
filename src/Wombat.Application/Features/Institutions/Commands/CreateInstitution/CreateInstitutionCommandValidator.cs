using FluentValidation;
using Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

namespace Wombat.Application.Features.Institutions.Commands.CreateInstitution;

/// <remarks>
/// T400 (flow 18, T378, K14): the record's own words (<see cref="UpdateInstitutionCommandValidator" />), never a
/// property's name, so a refusal the create page did not catch before sending still reads as the board draws it. The
/// page checks the same before it sends and puts each under its field.
/// </remarks>
public sealed class CreateInstitutionCommandValidator : AbstractValidator<CreateInstitutionCommand>
{
    public CreateInstitutionCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty().WithMessage(UpdateInstitutionCommandValidator.NameMissing)
            .MaximumLength(200).WithMessage(UpdateInstitutionCommandValidator.NameTooLong);
        RuleFor(command => command.ShortCode)
            .NotEmpty().WithMessage(UpdateInstitutionCommandValidator.ShortCodeMissing)
            .MaximumLength(32).WithMessage(UpdateInstitutionCommandValidator.ShortCodeTooLong);
        RuleFor(command => command.ContactEmail)
            .EmailAddress().WithMessage(UpdateInstitutionCommandValidator.ContactEmailMalformed)
            .MaximumLength(320).WithMessage(UpdateInstitutionCommandValidator.ContactEmailMalformed)
            .When(command => !string.IsNullOrWhiteSpace(command.ContactEmail));
    }
}
