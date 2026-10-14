namespace Wombat.Application.Features.CommitteeDecisions;

/// <summary>
/// A committee handler's own refusal, in Wombat's words, meant for the person who asked: the schedule page and a panel's
/// page show its message (T364, round 3 S4), and nothing else's (T364, build review R4).
/// </summary>
/// <remarks>
/// Until R4 these handlers threw bare <see cref="InvalidOperationException" />s, and the pages printed the message of any
/// exception of that type. The framework throws the same type (a sequence with no element, EF Core's own complaints), so a
/// fault's text could reach the page (T338). A page now shows this type's message, and treats any other
/// <see cref="InvalidOperationException" /> as an unexpected failure: a fixed sentence, logged. Derived from
/// <see cref="InvalidOperationException" /> so that nothing that already catches a refusal changes, and the audit row keeps
/// its message.
/// </remarks>
public class CommitteeRefusedException : InvalidOperationException
{
    public CommitteeRefusedException(string message)
        : base(message)
    {
    }

    /// <param name="message">The refusal, as the page shows it.</param>
    /// <param name="innerException">The database's refusal underneath (a race), kept for the audit pipeline (T201).</param>
    public CommitteeRefusedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Which exceptions from a committee command are its refusals, whose words a page shows (T366, R1).</summary>
public static class CommitteeRefusals
{
    /// <summary>
    /// The validator's (<see cref="FluentValidation.ValidationException" />), a scope refusal
    /// (<see cref="UnauthorizedAccessException" />, in the handler's words), or the committee's own
    /// (<see cref="CommitteeRefusedException" />). Anything else is an unexpected failure: a fixed sentence, logged.
    /// </summary>
    public static bool IsRefusal(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is FluentValidation.ValidationException or UnauthorizedAccessException or CommitteeRefusedException;
    }
}
