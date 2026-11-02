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

    /// <summary>
    /// T392 (flow 15, lane C, F4; T324): a refusal the page puts under its field says what to do in words, never the
    /// property's name ("'Sub Speciality Id' must be greater than '0'.").
    /// </summary>
    [Fact]
    public void Validate_SaysWhatIsMissingInWords()
    {
        var validator = new CreateEpaCommandValidator();
        var command = new CreateEpaCommand(0, string.Empty, new string('x', 201), null, null, TestPrincipals.Administrator());

        var result = validator.Validate(command);

        result.Errors.Select(error => error.ErrorMessage).Should().BeEquivalentTo(
            [EpaFieldMessages.SubSpecialityRequired, EpaFieldMessages.CodeRequired, EpaFieldMessages.TitleTooLong]);
    }
}
