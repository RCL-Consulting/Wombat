using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Wombat.Application.Common;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Admin;

/// <summary>
/// A refusal under its field (T392, flow 15, F4, S6, build-contracts § 0): the EPA's page, the item's page and the profile
/// read a command's <see cref="FieldRefusalException" /> and a validator's one-property failure alike; anything else is no
/// field's, and the page shows its own fixed failure sentence, never the exception's text (T329).
/// </summary>
public sealed class FieldRefusalTests
{
    [Fact]
    public void A_CommandsFieldRefusal_IsItsFieldAndItsSentence()
    {
        var refusal = FieldRefusal.Of(new FieldRefusalException("Code", "An EPA with the same code already exists for this sub-speciality."));

        refusal.Should().Be(new FieldRefusal("Code", "An EPA with the same code already exists for this sub-speciality."));
        refusal!.Is("Code").Should().BeTrue();
        refusal.Is("code").Should().BeFalse("the key is the command's property name, compared exactly");
    }

    [Fact]
    public void A_FieldRefusal_IsStillAnInvalidOperation_SoEveryCatchThatTookOneStillDoes()
    {
        Exception refused = new FieldRefusalException("SubSpecialityId", "Your institution has not adopted Paediatrics / Neonatology, so an EPA cannot be added to it.");

        refused.Should().BeAssignableTo<InvalidOperationException>();
        ((FieldRefusalException)refused).Field.Should().Be("SubSpecialityId");
    }

    [Fact]
    public void A_FieldRefusal_NeedsAFieldAndASentence()
    {
        var noField = () => new FieldRefusalException(" ", "A sentence.");
        var noSentence = () => new FieldRefusalException("Code", "");

        noField.Should().Throw<ArgumentException>();
        noSentence.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void A_ValidatorsFailuresOnOneProperty_AreThatFieldsRefusal_EachSentenceOnce()
    {
        var exception = new ValidationException(
        [
            new ValidationFailure("Code", "Enter the EPA's code."),
            new ValidationFailure("Code", "Enter the EPA's code."),
            new ValidationFailure("Code", "Write the code in 20 characters or fewer."),
        ]);

        FieldRefusal.Of(exception).Should().Be(new FieldRefusal("Code", "Enter the EPA's code. Write the code in 20 characters or fewer."));
    }

    [Fact]
    public void A_ValidatorsFailuresOnTwoProperties_AreNoOneFieldsRefusal()
        => FieldRefusal.Of(new ValidationException(
            [new ValidationFailure("Code", "Enter the EPA's code."), new ValidationFailure("Title", "Enter the EPA's title.")]))
            .Should().BeNull();

    [Fact]
    public void AnyOtherException_IsNoFieldsRefusal()
    {
        FieldRefusal.Of(new InvalidOperationException("An exception has been raised that is likely due to a transient failure.")).Should().BeNull();
        FieldRefusal.Of(new UnauthorizedAccessException("You do not have permission to update this EPA.")).Should().BeNull();
    }

    [Fact]
    public void TheControlIsDescribed_ByTheRefusalFirst_ThenItsHelp_ThenAnythingElse()
    {
        FieldRefusal.Id("epa-code").Should().Be("epa-code-refusal");
        FieldRefusal.Id("item epa").Should().Be("item-epa-refusal");

        FieldRefusal.DescribedBy("epa-code", refused: true, "The College's code.").Should().Be("epa-code-refusal epa-code-help");
        FieldRefusal.DescribedBy("epa-code", refused: false, "The College's code.").Should().Be("epa-code-help");
        FieldRefusal.DescribedBy("epa-code", refused: true, null, "epa-code-counter").Should().Be("epa-code-refusal epa-code-counter");
        FieldRefusal.DescribedBy("epa-code", refused: false, null).Should().BeNull();
    }
}
