using System.Data.Common;

namespace Wombat.Application.Common.Persistence;

/// <summary>
/// The PostgreSQL error codes (SQLSTATEs) a refused save is recognised by, where more than one code means the same
/// refusal. Read through <see cref="DbException.SqlState" />, so the Application layer names no provider type.
/// </summary>
/// <remarks>
/// <para>
/// <b>A foreign-key refusal has two codes (T243).</b> PostgreSQL 18 reports a delete or update refused by an
/// <c>ON DELETE RESTRICT</c> or <c>ON UPDATE RESTRICT</c> foreign key as <c>23001</c> (<c>restrict_violation</c>).
/// PostgreSQL 17 and earlier report the same refusal as <c>23503</c> (<c>foreign_key_violation</c>), and every version
/// still reports <c>23503</c> for an insert or update naming a missing row and for a <c>NO ACTION</c> key. Measured on
/// 2026-09-25: the dev server (16.10) and <c>postgres:17</c> (17.11) gave 23503 for all three; <c>postgres:18</c> (18.6)
/// gave 23001 for the RESTRICT delete and 23503 for the other two (<c>ForeignKeySqlStatePostgresTests</c>).
/// </para>
/// <para>
/// EF Core's <c>DeleteBehavior.Restrict</c> is <c>ON DELETE RESTRICT</c>, and production runs PostgreSQL 18, so a check
/// on <c>23503</c> alone misses exactly the refusal it was written for there while passing on dev. Ask
/// <see cref="IsForeignKeyViolation(string?)" /> instead of naming either code. <c>ForeignKeyErrorCodeTests</c>
/// (Architecture) fails when any other type in <c>src</c> names one, Npgsql's <c>PostgresErrorCodes</c> constants
/// included.
/// </para>
/// </remarks>
public static class PostgresErrors
{
    /// <summary><c>foreign_key_violation</c>: a missing referenced row, or a <c>NO ACTION</c> key (and RESTRICT before 18).</summary>
    public const string ForeignKeyViolation = "23503";

    /// <summary><c>restrict_violation</c>: a delete or update refused by a <c>RESTRICT</c> foreign key, from PostgreSQL 18.</summary>
    public const string RestrictViolation = "23001";

    /// <summary>Every code a foreign-key refusal can carry, for an assertion that names the code it got instead.</summary>
    public static IReadOnlyList<string> ForeignKeyViolationStates { get; } = [ForeignKeyViolation, RestrictViolation];

    /// <summary>Whether <paramref name="sqlState" /> is a foreign key refusing a write, on any PostgreSQL version.</summary>
    public static bool IsForeignKeyViolation(string? sqlState)
        => sqlState is ForeignKeyViolation or RestrictViolation;

    /// <summary>
    /// Whether <paramref name="exception" /> is a foreign key refusing a write: the provider's exception itself, or any
    /// exception carrying it at any depth. A refused <c>SaveChanges</c> wraps it in a <c>DbUpdateException</c>, and a
    /// handler that translates a refusal wraps that again (<c>CreateDecisionPanel</c> throws an
    /// <c>InvalidOperationException</c> carrying the unique-index refusal it translates), so the whole
    /// <see cref="Exception.InnerException" /> chain is read, as <c>AuditPipelineBehavior</c> reads it for a refused save.
    /// </summary>
    public static bool IsForeignKeyViolation(Exception? exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is DbException refusal && IsForeignKeyViolation(refusal.SqlState))
            {
                return true;
            }
        }

        return false;
    }
}
