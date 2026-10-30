using System.Globalization;
using Wombat.Application.Features.DataRights;
using Wombat.Domain.DataRights;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Profile;

/// <summary>
/// The requester's words on My data rights (T387, flow 14, boards R3-M01 to M18; build-contracts § 3): the page's own, so
/// a test holds each sentence to the boards without rendering the page. The words two data-rights pages share (a type,
/// a status, a moment in SAST) are <see cref="DataRightsWords" />.
/// </summary>
public static class MyDataRightsWords
{
    public const string Title = "My data rights";

    public const string Subtitle =
        "Ask for a copy of your data, a correction or erasure, and read the answers. Stop the regular reminder emails.";

    // ---- the sections (Q4: one page, three h2 sections in this order; Q24: Reminder emails) ----

    public const string YourRequests = "Your requests";
    public const string MakeARequest = "Make a request";
    public const string ReminderEmails = "Reminder emails";

    // ---- loading and the load failure (M14, M15; T329: never the exception's text) ----

    public const string Loading = "Loading your data rights.";

    public const string LoadFailed =
        "Could not load your data rights. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- Your requests (M01, M03–M10, M17) ----

    public const string NoRequestsTitle = "No requests yet.";

    public const string NoRequestsBody =
        "You have not asked for your data, a correction or erasure. Make a request below.";

    /// <summary>The table's caption, which does not repeat the section's heading (T280).</summary>
    public const string Caption = "Your data rights requests, newest first";

    public const string Submitted = "Request submitted. You will be notified when it is reviewed.";
    public const string Withdrawn = "Request withdrawn.";

    /// <summary>
    /// Withdraw refused because the request was decided first (Q14; lane K's <see cref="DataRightsRefusal.DecidedMeanwhile" />):
    /// the operator's words of 2026-10-30, drawn on no board.
    /// </summary>
    public const string DecidedBeforeWithdrawn =
        "This request was decided before it could be withdrawn. Nothing was withdrawn.";

    /// <summary>Withdraw failed for any other reason (T329): NEW, no board draws it; M16's shape.</summary>
    public const string WithdrawFailed =
        "The request could not be withdrawn just now. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- Make a request (M01, M02, M16, M17, M18) ----

    /// <summary>The intro, naming who decides (S3, M18): staff at the requester's institution, or the Administrator.</summary>
    public static string Intro(string? institutionName) => institutionName is null
        ? "Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. " +
          "The Wombat administrator decides it; the answer shows under Your requests."
        : "Under data protection law you may ask for a copy of your personal data, its correction, or its erasure. " +
          $"Staff at {institutionName} decide it; the answer shows under Your requests.";

    /// <summary>What happens once it is sent (Q5's promise, Q14's Withdraw), under the reason.</summary>
    public static string NextStep(string? institutionName) => institutionName is null
        ? "What happens next: the Wombat administrator reads it and approves or rejects it with a note, and you are " +
          "emailed when it is decided. While it waits you can withdraw it under Your requests."
        : $"What happens next: staff at {institutionName} read it and approve or reject it with a note, and you are " +
          "emailed when it is decided. While it waits you can withdraw it under Your requests.";

    public const string TypeLegend = "Request type";
    public const string ReasonLabel = "Reason";
    public const string SubmitLabel = "Submit request";

    /// <summary>A type's description under its radio (Q20: Access and Export kept apart, each said honestly).</summary>
    public static string TypeHelp(DataRightsRequestType type) => type switch
    {
        DataRightsRequestType.Access =>
            "Everything Wombat holds about you, as a download, once the request is approved.",
        DataRightsRequestType.Export =>
            "The same download, to take your data elsewhere: machine-readable data with a PDF summary.",
        DataRightsRequestType.Rectification =>
            "Something about you is recorded wrongly. Say what it is and what it should be.",
        DataRightsRequestType.Erasure =>
            "Your account and personal data are removed; your training records are kept under a name that identifies " +
            "nobody. Once approved it cannot be undone, and you can no longer sign in.",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "My data rights offers no such type (Q8).")
    };

    /// <summary>The reason's limit, the validator's (<c>SubmitDataRightsRequestCommandValidator</c>, S24).</summary>
    public const int ReasonMaxLength = 4000;

    /// <summary>The counter, the reason's <c>.field-help</c> (S24): "0 of 4,000 characters".</summary>
    public static string Counter(int length)
        => string.Create(CultureInfo.InvariantCulture, $"{length:N0} of {ReasonMaxLength:N0} characters");

    /// <summary>The built refusal of an empty reason (build-contracts § 5), now under Reason (M02).</summary>
    public const string ReasonRequired = "Please provide a reason for your request.";

    /// <summary>A reason past the limit: NEW (no board), the lodge block's words (<c>LodgeAppealCommand.ReasonTooLong</c>).</summary>
    public const string ReasonTooLong = "Give your reason in 4,000 characters or fewer.";

    /// <summary>
    /// Submit with no type chosen: NEW (no board). The boards draw no radio checked as the page opens (M01, M03), so the
    /// form asks for one rather than sending a type the person did not choose.
    /// </summary>
    public const string TypeRequired = "Choose the request type.";

    /// <summary>The submit failed for any reason but a refused field (M16; T329).</summary>
    public const string SubmitFailed =
        "The request could not be sent just now. Nothing was sent. Try again, or come back in a few minutes.";

    // ---- Reminder emails (Q6, Q7, Q24, Q27, F7, N3; M11, M12) ----

    public const string RemindersIntro =
        "Wombat processes what your training programme needs (your record, assessments and what regulation requires), " +
        "and that cannot be objected to. You can stop its regular reminders.";

    public const string StopRemindersLabel = "Stop the regular reminder emails";
    public const string SaveLabel = "Save";

    /// <summary>
    /// What the opt-out stops, and what it does not (T240; Q27, F7). The three jobs are the ones that ask
    /// <c>ReminderRecipientPolicy</c>; the nudge's days are its setting (<c>DashboardThresholds.AssessorNudgeDays</c>, T358).
    /// The examples of mail still sent were checked against <c>IEmailSender</c>'s and <c>AccountNotices</c>' callers on
    /// 2026-10-30: an MSF invitation (OpenMsfCampaign, ResendMsfLinks), a coordinator's reminder about one activity
    /// (SendActivityReminder), and the entrustment decision expiry notices (EntrustmentDecisionExpiryJob, which the
    /// opt-out does not reach: F7, T389). "A request to assess" and "a committee or entrustment decision", which no code
    /// mails, are gone (T320).
    /// </summary>
    public static string RemindersHelp(int assessorNudgeDays)
        => "Stops three regular reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 " +
           "days, and the reminder of activities that have waited more than " +
           $"{assessorNudgeDays.ToString(CultureInfo.InvariantCulture)} days for you to assess. Email about one " +
           "particular thing is still sent, such as an invitation to give feedback, a coordinator's reminder about one " +
           "activity, or a notice that an entrustment decision is about to expire or has expired.";

    /// <summary>What the save did (Q24), or that it changed nothing (N3), read from the state before (ReminderEmailsSaved).</summary>
    public static string Saved(bool optedOut, bool changed) => (optedOut, changed) switch
    {
        (true, true) => "Reminder emails stopped.",
        (false, true) => "Reminder emails will be sent again.",
        (true, false) => "Nothing changed: reminder emails are still stopped.",
        (false, false) => "Nothing changed: reminder emails are still sent."
    };

    /// <summary>The save failed (T329): NEW, no board draws it; M16's shape.</summary>
    public const string SaveFailed =
        "The reminder emails could not be saved just now. Nothing has changed. Try again, or come back in a few minutes.";

    // ---- a row (Q5, T319; M03–M10, M17) ----

    /// <summary>
    /// The row's answer (Q5, T319): the decision, its South African day and its note, never who decided. A note that
    /// ends its own sentence closes the quote with no second full stop (M04: “… to the data subject.”).
    /// </summary>
    public static string Answer(MyDataRightsRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Status switch
        {
            DataRightsRequestStatus.Submitted => "Waiting for a decision.",
            DataRightsRequestStatus.Withdrawn => "You withdrew it before a decision.",
            DataRightsRequestStatus.Rejected => Decided("Rejected", request.DecidedOn, request.DecisionNote),
            DataRightsRequestStatus.Approved when request.Type == DataRightsRequestType.Rectification =>
                Decided("Approved", request.DecidedOn, request.DecisionNote) +
                " Being corrected; it reads Completed once done.",
            DataRightsRequestStatus.Approved => Decided("Approved", request.DecidedOn, request.DecisionNote),
            DataRightsRequestStatus.Completed when request.Type == DataRightsRequestType.Rectification =>
                Corrected(request),
            // Access, Export and Erasure complete as they are approved (ApproveDataRightsRequest): the answer is the
            // approval (M04).
            DataRightsRequestStatus.Completed =>
                Decided("Approved", request.DecidedOn ?? request.CompletedOn, request.DecisionNote),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request.Status, "A status with no answer.")
        };
    }

    private static string Decided(string verb, DateTime? on, string? note)
    {
        var day = on is { } utc ? $" on {DataRightsWords.DayOf(utc)}" : string.Empty;
        if (string.IsNullOrWhiteSpace(note))
        {
            return $"{verb}{day}.";
        }

        var trimmed = note.Trim();
        return $"{verb}{day}: “{trimmed}”" + (EndsASentence(trimmed) ? string.Empty : ".");
    }

    private static string Corrected(MyDataRightsRequestDto request)
    {
        var day = request.CompletedOn is { } utc ? $" on {DataRightsWords.DayOf(utc)}" : string.Empty;
        return request.CorrectedRecords.Count == 0
            ? $"Completed{day}."
            : $"Completed{day}. Corrected: {string.Join(", ", request.CorrectedRecords.Select(DataRightsWords.Record))}.";
    }

    private static bool EndsASentence(string text) => text[^1] is '.' or '!' or '?';

    /// <summary>
    /// When each request was made, as its actions name it (T239; N4): "2026-10-28 00:01 SAST", or to the second,
    /// "2026-10-28 00:01:07 SAST", where two requests of one type share the minute and their names would read alike.
    /// </summary>
    public static IReadOnlyDictionary<Guid, string> Made(IReadOnlyList<MyDataRightsRequestDto> requests)
    {
        ArgumentNullException.ThrowIfNull(requests);
        var shared = requests
            .GroupBy(request => (request.Type, Minute: DataRightsWords.When(request.RequestedOn)))
            .Where(group => group.Count() > 1)
            .SelectMany(group => group)
            .Select(request => request.Id)
            .ToHashSet();

        return requests.ToDictionary(
            request => request.Id,
            request => shared.Contains(request.Id) ? ToTheSecond(request.RequestedOn) : DataRightsWords.When(request.RequestedOn));
    }

    /// <summary>A stored UTC moment in SAST to the second (N4). South Africa keeps UTC+2 all year (ProgrammeCalendar).</summary>
    public static string ToTheSecond(DateTime utc)
        => (DateTime.SpecifyKind(utc, DateTimeKind.Utc) + TimeSpan.FromHours(2))
            .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " SAST";

    /// <summary>A row as its actions name it: "the Export request made 2026-10-28 00:01 SAST" (T239).</summary>
    public static string RowName(DataRightsRequestType type, string made)
        => $"the {DataRightsWords.Type(type)} request made {made}";

    /// <summary>Withdraw's name: "Withdraw the Export request made 2026-10-28 00:01 SAST" (T239).</summary>
    public static string WithdrawName(DataRightsRequestType type, string made) => $"Withdraw {RowName(type, made)}";

    /// <summary>
    /// What Download downloads, between the verb and the row (N12): the built Export words, "the export of", and the
    /// Access words the operator approved on 2026-10-30, "the copy of your data for".
    /// </summary>
    public static string DownloadObject(DataRightsRequestType type)
        => type == DataRightsRequestType.Access ? "the copy of your data for" : "the export of";

    /// <summary>
    /// Download's name, each starting with the visible word (N12): "Download the export of the Export request made …",
    /// "Download the copy of your data for the Access request made …".
    /// </summary>
    public static string DownloadName(DataRightsRequestType type, string made)
        => $"Download {DownloadObject(type)} {RowName(type, made)}";

    /// <summary>A Completed Access or Export offers Download (Program.cs's download route admits only those).</summary>
    public static bool OffersDownload(MyDataRightsRequestDto request)
        => request.Status == DataRightsRequestStatus.Completed &&
           request.Type is DataRightsRequestType.Access or DataRightsRequestType.Export;

    /// <summary>Withdraw is offered only while the request waits for a decision (Q14).</summary>
    public static bool OffersWithdraw(MyDataRightsRequestDto request)
        => request.Status == DataRightsRequestStatus.Submitted;
}
