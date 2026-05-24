using FluentAssertions;
using Wombat.Application.Features.Epas;
using Wombat.Application.Tests.TestHelpers;

namespace Wombat.Application.Tests.Features.Epas;

public sealed class CreateEpaCommandValidatorTests
{
    [Fact]
    public void Validate_WhenRequiredFieldsMissing_ReturnsValidationErrors()
    {
        var validator = new CreateEpaCommandValidator();
        var command = new CreateEpaCommand(0, string.Empty, string.Empty, null, null, TestPrincipals.Administrator());

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateEpaCommand.SubSpecialityId));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateEpaCommand.Code));
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateEpaCommand.Title));
    }
}
