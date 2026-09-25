using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Design;

/// <summary>
/// A refusal is shown in its own words (T213): a validator's as its messages, never FluentValidation's log format, and any
/// other exception's message as it is.
/// </summary>
public sealed class RefusalTextTests
{
    [Fact]
    public void AValidatorsRefusal_IsItsMessages_EachOnce_InTheValidatorsOrder()
    {
        var refusal = new ValidationException(
        [
            new ValidationFailure("PresentUserIds", "A committee decision needs a quorum."),
            new ValidationFailure("Rationale", "'Rationale' must not be empty."),
            new ValidationFailure("PresentUserIds", "A committee decision needs a quorum.")
        ]);

        refusal.Message.Should().StartWith("Validation failed:", "the exception itself is unchanged, for the audit row");
        RefusalText.Of(refusal).Should().Be("A committee decision needs a quorum. 'Rationale' must not be empty.");
    }

    [Fact]
    public void AValidatorsRefusalWithNoFailures_FallsBackToItsMessage()
        => RefusalText.Of(new ValidationException("The request is not valid.")).Should().Be("The request is not valid.");

    [Fact]
    public void AnyOtherRefusal_IsItsMessage()
        => RefusalText.Of(new InvalidOperationException("Only the panel's chair can do this."))
            .Should().Be("Only the panel's chair can do this.");
}
