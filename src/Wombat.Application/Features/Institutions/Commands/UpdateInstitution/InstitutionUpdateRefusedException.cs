namespace Wombat.Application.Features.Institutions.Commands.UpdateInstitution;

/// <summary>Why an institution's record was not saved (T376, flow 12: her record's refusals, I08).</summary>
public enum InstitutionUpdateRefusal
{
    /// <summary>Another institution holds the name or the short code (the unique indexes).</summary>
    Duplicate,

    /// <summary>The institution is not there.</summary>
    NotFound
}

/// <summary>The field of the record a refusal sits under (T376, S22: beside its field, focused, never a summary).</summary>
public enum InstitutionRecordField
{
    /// <summary>No one field: the page shows the refusal in its result.</summary>
    None,
    Name,
    ShortCode,
    ContactEmail
}

/// <summary>
/// A refusal of <see cref="UpdateInstitutionCommand" /> the page puts beside its field by <see cref="Field" />, never by
/// its text (T376, flow 12, I08). Its message is the built words ("An institution with the same name or short code already
/// exists."), which name no other institution.
/// </summary>
/// <remarks>
/// Derived from <see cref="InvalidOperationException" />, as <c>InvitationIssueRefusedException</c> is, so the audit row
/// keeps its message and nothing that caught the bare exception before changes. Any other exception is, to the page, an
/// unexpected failure: a fixed sentence, logged (T329).
/// </remarks>
public sealed class InstitutionUpdateRefusedException : InvalidOperationException
{
    public InstitutionUpdateRefusedException(InstitutionUpdateRefusal reason, InstitutionRecordField field, string message, Exception? inner = null)
        : base(message, inner)
    {
        Reason = reason;
        Field = field;
    }

    public InstitutionUpdateRefusal Reason { get; }

    public InstitutionRecordField Field { get; }
}

/// <summary>Which exceptions from the record's save are refusals, whose words the page shows (T376, T329).</summary>
public static class InstitutionUpdateRefusals
{
    /// <summary>
    /// The validator's (<see cref="FluentValidation.ValidationException" />), a scope refusal
    /// (<see cref="UnauthorizedAccessException" />), or the record's own (<see cref="InstitutionUpdateRefusedException" />).
    /// </summary>
    public static bool IsRefusal(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is FluentValidation.ValidationException or UnauthorizedAccessException
            or InstitutionUpdateRefusedException;
    }
}
