using System.Data.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Persistence;

namespace Wombat.Application.Tests.Common.Persistence;

/// <summary>
/// A foreign key refusing a write is recognised by either of its codes: 23503 everywhere, and 23001 for a RESTRICT key
/// on PostgreSQL 18, where production runs (T243). The codes as PostgreSQL itself reports them are pinned on a real
/// server by <c>ForeignKeySqlStatePostgresTests</c>.
/// </summary>
public sealed class PostgresErrorsTests
{
    [Theory]
    [InlineData("23503", "foreign_key_violation: a missing row, a NO ACTION key, and a RESTRICT key before PostgreSQL 18")]
    [InlineData("23001", "restrict_violation: a RESTRICT key on PostgreSQL 18")]
    public void BothForeignKeyCodes_AreAForeignKeyViolation(string sqlState, string because)
    {
        PostgresErrors.IsForeignKeyViolation(sqlState).Should().BeTrue(because);
        PostgresErrors.IsForeignKeyViolation(new Refusal(sqlState)).Should().BeTrue(because);
        PostgresErrors.ForeignKeyViolationStates.Should().Contain(sqlState, because);
    }

    [Theory]
    [InlineData("23505", "unique_violation")]
    [InlineData("23502", "not_null_violation")]
    [InlineData("23514", "check_violation")]
    [InlineData("23P01", "exclusion_violation")]
    [InlineData("23000", "integrity_constraint_violation, the class itself")]
    [InlineData("", "no code")]
    public void AnyOtherCode_IsNot(string sqlState, string because)
    {
        PostgresErrors.IsForeignKeyViolation(sqlState).Should().BeFalse(because);
        PostgresErrors.IsForeignKeyViolation(new Refusal(sqlState)).Should().BeFalse(because);
        PostgresErrors.ForeignKeyViolationStates.Should().NotContain(sqlState, because);
    }

    [Fact]
    public void NoCode_AndNoException_AreNot()
    {
        PostgresErrors.IsForeignKeyViolation((string?)null).Should().BeFalse();
        PostgresErrors.IsForeignKeyViolation((Exception?)null).Should().BeFalse();
        PostgresErrors.IsForeignKeyViolation(new Refusal(null)).Should().BeFalse();
    }

    [Theory]
    [InlineData("23503")]
    [InlineData("23001")]
    public void ARefusedSave_IsRecognisedThroughTheDbUpdateException_ThatWrapsTheProvidersRefusal(string sqlState)
    {
        var refusedSave = new DbUpdateException("An error occurred while saving the entity changes.", new Refusal(sqlState));

        PostgresErrors.IsForeignKeyViolation(refusedSave).Should().BeTrue();
    }

    [Theory]
    [InlineData("23503")]
    [InlineData("23001")]
    public void ARefusedSaveAHandlerTranslated_IsRecognisedAtAnyDepth(string sqlState)
    {
        // CreateDecisionPanel and SetDecisionPanelBody throw new InvalidOperationException(taken, exception) around the
        // refused save, so a caller above the handler (the audit pipeline, a page) sees the refusal two levels down.
        var refusedSave = new DbUpdateException("An error occurred while saving the entity changes.", new Refusal(sqlState));
        var translated = new InvalidOperationException("That panel is taken.", refusedSave);

        PostgresErrors.IsForeignKeyViolation(translated).Should().BeTrue("the refusal is two levels down");
        PostgresErrors.IsForeignKeyViolation(new InvalidOperationException("wrapped again", translated))
            .Should().BeTrue("the refusal is three levels down");
    }

    [Fact]
    public void ATranslatedRefusalOfAnotherKind_IsNot()
        => PostgresErrors.IsForeignKeyViolation(
                new InvalidOperationException("That panel is taken.",
                    new DbUpdateException("a unique index refused it", new Refusal("23505"))))
            .Should().BeFalse("only a foreign-key code is a foreign-key refusal, at any depth");

    [Fact]
    public void AnExceptionThatCarriesNoDatabaseRefusal_IsNot()
    {
        PostgresErrors.IsForeignKeyViolation(new InvalidOperationException("23503")).Should().BeFalse(
            "a message that happens to hold the code is not a refusal");
        PostgresErrors.IsForeignKeyViolation(new DbUpdateException("no inner exception")).Should().BeFalse();
        PostgresErrors.IsForeignKeyViolation(
                new DbUpdateException("a refusal of another kind", new Refusal("23505")))
            .Should().BeFalse();
    }

    [Fact]
    public void TheCodesAreExactlyTheTwo_SoAnAssertionOnTheListAcceptsNothingElse()
        => PostgresErrors.ForeignKeyViolationStates.Should().BeEquivalentTo(["23503", "23001"]);

    /// <summary>A provider's refusal carrying only its SQLSTATE, as Npgsql's <c>PostgresException</c> does.</summary>
    private sealed class Refusal(string? sqlState) : DbException("refused")
    {
        public override string? SqlState { get; } = sqlState;
    }
}
