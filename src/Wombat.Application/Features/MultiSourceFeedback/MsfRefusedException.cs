namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// An MSF campaign handler's own refusal, in Wombat's words, meant for the coordinator or administrator who asked: the
/// campaign pages show its message (T369, build review R1).
/// </summary>
/// <remarks>
/// Until R1 these handlers threw bare <see cref="InvalidOperationException" />s, and the pages printed the message of any
/// exception of that type. The framework throws the same type (a sequence with no element, EF Core's own complaints), so a
/// fault's text could reach the page (T329), as flow 09 found on the committee's pages (T366, R1;
/// <c>CommitteeRefusedException</c>). A page now shows this type's message, and treats any other
/// <see cref="InvalidOperationException" /> as an unexpected failure: a fixed sentence, logged. Derived from
/// <see cref="InvalidOperationException" /> so that nothing that already catches a refusal changes, and the audit row keeps
/// its message.
/// </remarks>
public sealed class MsfRefusedException : InvalidOperationException
{
    public MsfRefusedException(string message)
        : base(message)
    {
    }

    /// <param name="message">The refusal, as the page shows it.</param>
    /// <param name="innerException">The refusal underneath (a refused save, a domain rule), kept for the audit pipeline (T201).</param>
    public MsfRefusedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>Which exceptions from an MSF command or query are its refusals, whose words a page shows (T369, R1).</summary>
public static class MsfRefusals
{
    /// <summary>
    /// The validator's (<see cref="FluentValidation.ValidationException" />), a scope refusal
    /// (<see cref="UnauthorizedAccessException" />, in the handler's words), or the campaign's own
    /// (<see cref="MsfRefusedException" />, or <see cref="MsfResponseRefusedException" /> on a respondent's link). Anything
    /// else is an unexpected failure: a fixed sentence, logged.
    /// </summary>
    public static bool IsRefusal(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception is FluentValidation.ValidationException or UnauthorizedAccessException or MsfRefusedException
            or MsfResponseRefusedException;
    }
}
