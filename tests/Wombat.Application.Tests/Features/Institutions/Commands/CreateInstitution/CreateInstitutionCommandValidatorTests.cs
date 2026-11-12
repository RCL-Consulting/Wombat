using FluentAssertions;
using Wombat.Application.Features.Institutions.Commands.CreateInstitution;
using Wombat.Application.Tests.TestHelpers;

namespace Wombat.Application.Tests.Features.Institutions.Commands.CreateInstitution;

public sealed class CreateInstitutionCommandValidatorTests
{
    [Fact]
    public void Validate_WhenRequiredFieldsMissing_ReturnsValidationErrors()
    {
        var validator = new CreateInstitutionCommandValidator();
        var command = new CreateInstitutionCommand(string.Empty, string.Empty, null, TestPrincipals.Administrator());

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateInstitutionCommand.Name));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateInstitutionCommand.ShortCode));
    }

    /// <summary>T400 (flow 18, T378, K14): the record's own words, never a property's name.</summary>
    [Fact]
    public void Validate_RefusesInTheRecordsWords()
    {
        var validator = new CreateInstitutionCommandValidator();

        validator.Validate(new CreateInstitutionCommand(" ", "", "not-an-address", TestPrincipals.Administrator()))
            .Errors.Select(error => error.ErrorMessage).Should().Equal(
                "Type the institution's name. The record was not saved.",
                "Type a short code. The record was not saved.",
                "Enter a valid email address, for example name@example.org.");
        validator.Validate(new CreateInstitutionCommand(new string('n', 201), new string('k', 33), null, TestPrincipals.Administrator()))
            .Errors.Select(error => error.ErrorMessage).Should().Equal(
                "Keep the name to 200 characters. The record was not saved.",
                "Keep the short code to 32 characters. The record was not saved.");
    }
}
