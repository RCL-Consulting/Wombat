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
}
