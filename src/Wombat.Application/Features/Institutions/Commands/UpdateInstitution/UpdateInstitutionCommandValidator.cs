using FluentValidation;

namespace Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

/// <remarks>
/// T376 (flow 12; T324): every refusal in Wombat's words, never a property's name. Her record's page checks the same
/// before it sends (I03, I04), and puts each beside its field.
/// </remarks>
public sealed class UpdateInstitutionCommandValidator : AbstractValidator<UpdateInstitutionCommand>
{
    /// <summary>An empty name (T376).</summary>
    public const string NameMissing = "Type the institution's name. The record was not saved.";

    /// <summary>A name past the column's 200 characters (T376).</summary>
    public const string NameTooLong = "Keep the name to 200 characters. The record was not saved.";

    /// <summary>An empty short code (T376, I03).</summary>
    public const string ShortCodeMissing = "Type a short code. The record was not saved.";

    /// <summary>A short code past the column's 32 characters (T376).</summary>
    public const string ShortCodeTooLong = "Keep the short code to 32 characters. The record was not saved.";

    /// <summary>A malformed contact email (T376, I04, PROPOSED), Invite a person's words (flow 11, N14).</summary>
    public const string ContactEmailMalformed = "Enter a valid email address, for example name@example.org.";

    public UpdateInstitutionCommandValidator()
    {
        RuleFor(command => command.Id).GreaterThan(0);
        RuleFor(command => command.Name).NotEmpty().WithMessage(NameMissing).MaximumLength(200).WithMessage(NameTooLong);
        RuleFor(command => command.ShortCode).NotEmpty().WithMessage(ShortCodeMissing)
            .MaximumLength(32).WithMessage(ShortCodeTooLong);
        RuleFor(command => command.ContactEmail)
            .EmailAddress().WithMessage(ContactEmailMalformed)
            .MaximumLength(320).WithMessage(ContactEmailMalformed)
            .When(command => !string.IsNullOrWhiteSpace(command.ContactEmail));
    }
}
