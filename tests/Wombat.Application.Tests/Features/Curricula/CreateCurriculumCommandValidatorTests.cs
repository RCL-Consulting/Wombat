using FluentAssertions;
using Wombat.Application.Features.Curricula;
using Wombat.Application.Tests.TestHelpers;

namespace Wombat.Application.Tests.Features.Curricula;

public sealed class CreateCurriculumCommandValidatorTests
{
    [Fact]
    public void Validate_WhenEffectiveToPrecedesEffectiveFrom_ReturnsValidationError()
    {
        var validator = new CreateCurriculumCommandValidator();
        var command = new CreateCurriculumCommand(2, "Core", "2026.1", new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1), TestPrincipals.Administrator());

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateCurriculumCommand.EffectiveTo));
    }
}
