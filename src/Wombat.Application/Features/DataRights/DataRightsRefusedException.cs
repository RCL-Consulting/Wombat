namespace Wombat.Application.Features.DataRights;

/// <summary>
/// Why a data-rights command refused, as the pages map it to their own words (T387): a typed reason, never the message's
/// text (T324, T329). The message is Wombat's own fallback for the audit row and the log, and names nobody.
/// </summary>
public enum DataRightsRefusal
{
    /// <summary>The requester withdrew it before the decision (F8, S13; checked first, and on a lost race).</summary>
    Withdrawn,

    /// <summary>Someone else decided it while the page was open (S13); nothing more was done.</summary>
    DecidedMeanwhile,

    /// <summary>The erasure of Wombat's only Administrator (F9): nobody would be left to run it.</summary>
    LastAdministrator,

    /// <summary>Erasure is not set up on this server: <c>Wombat__PseudonymSalt</c> is missing (N1; named in the log only).</summary>
    ErasureNotConfigured,

    /// <summary>Something about the person changed while they were being erased (the executor's PersonChanged).</summary>
    PersonChanged,

    /// <summary>A correction recorded on, or a completion of, a request that is not an approved Rectification.</summary>
    NotAnApprovedRectification,

    /// <summary>Complete request with no correction recorded (T316).</summary>
    NoCorrectionRecorded
}

/// <summary>
/// The refusals' sentences a page shows as they are (T387, review P9): here, beside the reasons, so that the request page's
/// words read Application, not the executor in Infrastructure that throws them.
/// </summary>
public static class DataRightsRefusalSentences
{
    /// <summary>
    /// Something about the person changed while they were being erased: nothing is erased, and the request stays as it was,
    /// to be approved again (T258 review). R3-D11 draws it.
    /// </summary>
    public const string PersonChanged =
        "Something about this person changed while they were being erased: a review, a feedback campaign or their account " +
        "was changed elsewhere at the same moment. Nothing has been erased, and the request has not been approved. " +
        "Approve it again.";
}

/// <summary>A data-rights command's own refusal, with its reason (T387). Derived from InvalidOperationException, as before.</summary>
public sealed class DataRightsRefusedException : InvalidOperationException
{
    public DataRightsRefusedException(DataRightsRefusal reason, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Reason = reason;
    }

    public DataRightsRefusal Reason { get; }
}
