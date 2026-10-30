using System.Globalization;
using Wombat.Application.Features.CommitteeDecisions;
using Wombat.Application.Features.DataRights;
using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.Identity;
using Wombat.Web.Components.Pages.Admin.Trainees;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Components.Pages.Admin.DataRights;

/// <summary>
/// A sentence an alert shows with its first sentence in bold (BRIEF § 11, flow 11: a result bolds only its first
/// sentence where the board bolds one): <see cref="Lead" />, then <see cref="Rest" />, when there is more.
/// </summary>
public sealed record DataRightsMessage(string Lead, string? Rest = null)
{
    /// <summary>The whole message as one string: a test's, and an alert's text once read.</summary>
    public override string ToString() => Rest is null ? Lead : $"{Lead} {Rest}";
}

/// <summary>
/// The words of the queue and of a request's page (T387, flow 14, lane P; boards R3-Q01–Q13, D01–D22, C01–C11, K01, K02,
/// U01, U02): one place, so the page, its row names and its tests cannot drift. Shared words (a type, a status, a moment
/// in SAST, the title) are <see cref="DataRightsWords" />'s. Every sentence that names the requester takes the name the
/// read gives (F1: live, or the pseudonym once erased); none names a person by a pronoun (Q19, M4).
/// </summary>
public static class DataRightsRequestWords
{
    // ---- the queue (R3-Q01–Q13, K02) ----

    /// <summary>The queue's h1 and tab.</summary>
    public const string QueueHeading = "Data rights requests";

    /// <summary>The subtitle while the scope is not yet known (the first read, Q11) and for a Coordinator set to no institution (S4).</summary>
    public const string SubtitleNoInstitution =
        "Requests about personal data: decide each, and carry an approved correction through.";

    /// <summary>The Administrator's subtitle (Q05).</summary>
    public const string SubtitleEveryInstitution =
        "Requests from people at every institution about their personal data: decide each, and carry an approved correction through.";

    /// <summary>A scoped reviewer's subtitle (Q01): "Requests from people at Kgosi Kgari Teaching Hospital about …".</summary>
    public static string SubtitleAt(string institution)
        => $"Requests from people at {institution} about their personal data: decide each, and carry an approved correction through.";

    /// <summary>The queue's subtitle for the scope its read gave.</summary>
    public static string Subtitle(PagedDataRightsResult? result) => result switch
    {
        null => SubtitleNoInstitution,
        { EveryInstitution: true } => SubtitleEveryInstitution,
        { InstitutionName: { } institution } => SubtitleAt(institution),
        _ => SubtitleNoInstitution
    };

    /// <summary>The filter bar's name (Q01's form aria-label).</summary>
    public const string FilterLabel = "Filter Data rights requests";

    public const string TypeLabel = "Type";
    public const string StatusLabel = "Status";
    public const string AllTypes = "All types";
    public const string AllStatuses = "All statuses";
    public const string Show = "Show";
    public const string ClearFilters = "Clear filters";

    /// <summary>The list's heading, which Show, Clear filters and a page turn focus: "Requests (3)".</summary>
    public static string ListHeading(int count) => $"Requests ({count.ToString(CultureInfo.InvariantCulture)})";

    /// <summary>PagerControls' landmark (Q09).</summary>
    public const string PagerLabel = "Requests, pages";

    /// <summary>PagerControls' page-size select (Q09, F10).</summary>
    public const string PageSizeLabel = "Requests per page";

    public const string Loading = "Loading the data rights requests.";

    public const string LoadFailed =
        "Could not load the data rights requests. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>
    /// The caption (T280: not the heading again; S12: the filter's tail). "Kgosi Kgari Teaching Hospital's data rights
    /// requests, newest first: Export, Submitted".
    /// </summary>
    public static string Caption(PagedDataRightsResult result, DataRightsRequestType? type, DataRightsRequestStatus? status)
    {
        var whose = result.EveryInstitution
            ? "Every institution's"
            : result.InstitutionName is { } institution ? $"{institution}'s" : "Your institution's";
        var tail = string.Join(", ", new[] { type is { } t ? DataRightsWords.Type(t) : null, status is { } s ? DataRightsWords.Status(s) : null }
            .Where(part => part is not null));
        return tail.Length == 0
            ? $"{whose} data rights requests, newest first"
            : $"{whose} data rights requests, newest first: {tail}";
    }

    /// <summary>A filtered answer with nothing in it (Q02): its title.</summary>
    public const string NoMatchTitle = "No requests match";

    /// <summary>An unfiltered answer with nothing in it, and the no-institution Coordinator's (S4): its title.</summary>
    public const string NoneTitle = "No requests";

    /// <summary>The Coordinator set to no institution (S4).</summary>
    public const string NoInstitutionBody = "Your account is not set to an institution, so no request is yours to decide.";

    /// <summary>
    /// What a filtered answer with nothing in it asked (S12, Q02): "No Erasure request at Kgosi Kgari Teaching Hospital.
    /// Clear filters to see every request.", "… at any institution." for the Administrator.
    /// </summary>
    public static string NoMatchBody(PagedDataRightsResult result, DataRightsRequestType? type, DataRightsRequestStatus? status)
    {
        var where = result.EveryInstitution || result.InstitutionName is null ? "any institution" : result.InstitutionName;
        var asked = (type, status) switch
        {
            ({ } t, { } s) => $"No {DataRightsWords.Type(t)} request is {DataRightsWords.Status(s)} at {where}.",
            ({ } t, null) => $"No {DataRightsWords.Type(t)} request at {where}.",
            (null, { } s) => $"No request is {DataRightsWords.Status(s)} at {where}.",
            _ => $"No request at {where}."
        };
        return $"{asked} Clear filters to see every request.";
    }

    /// <summary>An unfiltered answer with nothing in it (NEW: no board draws an empty queue but S4's).</summary>
    public static string NoneBody(PagedDataRightsResult result) => result switch
    {
        { EveryInstitution: true } => "Nobody has made a data rights request yet.",
        { InstitutionName: { } institution } => $"Nobody at {institution} has made a data rights request yet.",
        _ => NoInstitutionBody
    };

    /// <summary>The Administrator's Institution cell for a request with no institution (Q09).</summary>
    public const string NoInstitution = "None";

    /// <summary>The second line of the Requester cell once erased (N5, N13): "Erased 2026-10-28".</summary>
    public static string ErasedLine(DateOnly day) => $"Erased {DataWordsDay(day)}";

    public const string Review = "Review";

    /// <summary>
    /// What each row's Review names after its word (T239, N4): "the Export request from Anele Dlamini, 2026-10-28 00:01
    /// SAST", so the button reads "Review the Export request …". Two of one type from one person in one minute carry the
    /// seconds (T6), and a tie the seconds leave is numbered by <see cref="RowNames.Distinct" />.
    /// </summary>
    public static IReadOnlyDictionary<Guid, string> ReviewNames(IReadOnlyList<DataRightsRequestSummaryDto> rows)
    {
        string Name(DataRightsRequestSummaryDto row, string when)
            => $"the {DataRightsWords.Type(row.Type)} request from {row.RequesterName}, {when}";

        var byMinute = rows.ToDictionary(row => row.Id, row => Name(row, DataRightsWords.When(row.RequestedOn)));
        var shared = byMinute.Values.GroupBy(name => name, StringComparer.Ordinal).Where(group => group.Count() > 1)
            .Select(group => group.Key).ToHashSet(StringComparer.Ordinal);
        var chosen = rows.ToDictionary(
            row => row.Id,
            row => shared.Contains(byMinute[row.Id]) ? Name(row, WhenToTheSecond(row.RequestedOn)) : byMinute[row.Id]);
        return RowNames.Distinct(rows, row => row.Id, row => chosen[row.Id]);
    }

    /// <summary>A moment to the second, for two requests in one minute (N4): "2026-10-28 00:03:17 SAST".</summary>
    public static string WhenToTheSecond(DateTime utc)
        => new DateTimeOffset(DateTime.SpecifyKind(utc, DateTimeKind.Utc)).ToOffset(TimeSpan.FromHours(2))
            .ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + " SAST";

    // ---- a request: the page (R3-D01–D22, K01, U01, U02) ----

    /// <summary>The h1 and tab while the request is read, and when the read failed (D17, D18).</summary>
    public const string RequestHeading = "Data rights request";

    /// <summary>The h1, tab and crumb for a null read: an unknown id and another institution's alike (U01 = U02, Q13, Q17).</summary>
    public const string UnavailableHeading = "Request unavailable";

    public const string UnavailableBody = "This data rights request does not exist, or you cannot open it.";

    public const string GoToQueue = "Go to Data rights requests";

    public const string LoadingRequest = "Loading the request.";

    public const string RequestLoadFailed =
        "Could not load this request. Nothing has changed. Try again, or come back in a few minutes.";

    /// <summary>The subtitle (S8): "Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital", no institution for none.</summary>
    public static string RequestSubtitle(DataRightsRequestDetailDto request)
        => request.InstitutionName is { } institution
            ? $"Submitted {DataRightsWords.When(request.RequestedOn)} · {institution}"
            : $"Submitted {DataRightsWords.When(request.RequestedOn)}";

    /// <summary>
    /// The standing notice on an erased person's request (K01, D09); on another of their requests, which an erasure
    /// rewrote but did not ask for (F3, K02), its own words (NEW).
    /// </summary>
    public static string ErasedNotice(DataRightsRequestDetailDto request, DateOnly erasedOn)
        => request.Type == DataRightsRequestType.Erasure
            ? $"This person's personal data was erased on {DataWordsDay(erasedOn)}, at this request. The request is kept as the record of it, under the pseudonym."
            : $"This person's personal data was erased on {DataWordsDay(erasedOn)}. The request is kept under the pseudonym.";

    /// <summary>
    /// A withdrawn request's line where Decide would be (D04): "Pieter du Plessis withdrew this request before it was
    /// decided. There is nothing to decide." An erased person's (K02's Access) says nothing of who withdrew it, since an
    /// erasure withdraws the person's other open requests too (F3; NEW).
    /// </summary>
    public static string WithdrawnLine(DataRightsRequestDetailDto request)
        => request.ErasedOn is null
            ? $"{request.RequesterName} withdrew this request before it was decided. There is nothing to decide."
            : "This request was withdrawn before it was decided. There is nothing to decide.";

    /// <summary>The line in Decide's place on the caller's own request (F9, D15, D20).</summary>
    public static string? OwnRequestLine(DataRightsRequestDetailDto request) => request.OwnRequest switch
    {
        DataRightsOwnRequest.AnotherCoordinatorDecides => request.InstitutionName is { } institution
            ? $"You cannot decide your own request: another Coordinator at {institution}, or the Wombat administrator, decides it."
            : "You cannot decide your own request: another Coordinator, or the Wombat administrator, decides it.",
        DataRightsOwnRequest.AnotherAdministratorDecides => "You cannot decide your own request: another Administrator decides it.",
        _ => null
    };

    /// <summary>Decided by: the name over the id (D03); the bare id when no name is read (D19).</summary>
    public const string DecisionApproved = "Approved";

    public const string DecisionRejected = "Rejected";

    // ---- Decide (D01, D06) ----

    public const string DecideHeading = "Decide";

    public const string NoteLabel = "Decision note";

    /// <summary>The note refused before any send, and the validator's (built; D02).</summary>
    public const string NoteRequired = "A decision note is required.";

    /// <summary>The note's limit, the validators' <c>MaximumLength(4000)</c> (Approve, Reject).</summary>
    public const int NoteMaxLength = 4000;

    /// <summary>
    /// A note past the limit (T387 review P4): NEW (no board), the reason's sentence on My data rights. Until the review a
    /// pasted 4,001-character note was refused as "A decision note is required.".
    /// </summary>
    public const string NoteTooLong = "Give your note in 4,000 characters or fewer.";

    /// <summary>The note's counter (P4), as My data rights' reason counts: "120 of 4,000 characters".</summary>
    public static string NoteCounter(int length)
        => string.Create(CultureInfo.InvariantCulture, $"{length:N0} of {NoteMaxLength:N0} characters");

    /// <summary>
    /// Decide's help by type (D01, D06). Access and Rectification have no board: theirs follow Export's and the
    /// Rectification's approval result (NEW). "You cannot download it yourself." where the caller may not (N2).
    /// </summary>
    public static string DecideHelp(DataRightsRequestDetailDto request)
    {
        var name = request.RequesterName;
        var yourself = request.MayDownload ? string.Empty : " You cannot download it yourself.";
        return request.Type switch
        {
            DataRightsRequestType.Export =>
                $"Approving makes the export ready at once: {name} downloads it from My data rights, and it is built at that moment.{yourself}",
            DataRightsRequestType.Access =>
                $"Approving makes the copy of the data ready at once: {name} downloads it from My data rights, and it is built at that moment.{yourself}",
            DataRightsRequestType.Erasure =>
                $"Approving erases {name}'s personal data at once and cannot be undone. You are asked to confirm, with what it ends and keeps.",
            DataRightsRequestType.Rectification =>
                $"Approving opens the Correction section: record each correction as it is made, then complete the request. {name} is emailed at each step.",
            _ => $"{name} is emailed the decision."
        };
    }

    /// <summary>The note's help (D01, D06): an erasure's requester never reads it.</summary>
    public static string NoteHelp(DataRightsRequestDetailDto request)
        => request.Type == DataRightsRequestType.Erasure
            ? $"Kept with the request. {request.RequesterName} will not read it: approving ends the account."
            : $"{request.RequesterName} reads this note with the decision on My data rights, and is emailed that it was decided. Who decided is not shown.";

    public const string Approve = "Approve";

    /// <summary>An Erasure's Approve: an outline trigger opening the dialog (Q19).</summary>
    public const string ApproveAndErase = "Approve and erase…";

    public const string Reject = "Reject";

    // ---- the results (D03, D05, C01, D09, D22) ----

    /// <summary>An approval's result, by type, read after it (the name the request now reads by).</summary>
    public static DataRightsMessage Approved(DataRightsRequestDetailDto request) => request.Type switch
    {
        DataRightsRequestType.Export => new("Request approved.", $"{request.RequesterName} is emailed, and can download the export from My data rights."),
        DataRightsRequestType.Access => new("Request approved.", $"{request.RequesterName} is emailed, and can download the copy from My data rights."),
        DataRightsRequestType.Rectification => new("Request approved.", $"{request.RequesterName} is emailed. Record each correction below, then complete the request."),
        _ => new("Request approved.", $"{request.RequesterName} is emailed.")
    };

    public static DataRightsMessage Rejected(DataRightsRequestDetailDto request)
        => new("Request rejected.", $"{request.RequesterName} is emailed, and reads your note on My data rights.");

    /// <summary>
    /// The erasure's result (D09, D22; S6): what it ended, counted where each was ended, clause by clause, then the
    /// pseudonym. "1 committee review was withdrawn and the programme ended today."; with nothing, "Nothing else was open
    /// to end."
    /// </summary>
    public static DataRightsMessage Erased(ErasureOutcome outcome)
    {
        var clauses = new List<string>();
        if (outcome.ReviewsWithdrawn > 0)
        {
            clauses.Add(Count(outcome.ReviewsWithdrawn, "committee review was withdrawn", "committee reviews were withdrawn"));
        }

        if (outcome.CampaignsWithdrawn > 0)
        {
            clauses.Add(Count(outcome.CampaignsWithdrawn, "MSF campaign was withdrawn", "MSF campaigns were withdrawn"));
        }

        if (outcome.RequestsWithdrawn > 0)
        {
            clauses.Add(Count(outcome.RequestsWithdrawn, "other data rights request was withdrawn", "other data rights requests were withdrawn"));
        }

        if (outcome.ProfilesEnded == 1)
        {
            clauses.Add("the programme ended today");
        }
        else if (outcome.ProfilesEnded > 1)
        {
            clauses.Add($"{outcome.ProfilesEnded.ToString(CultureInfo.InvariantCulture)} programmes ended today");
        }

        var ended = clauses.Count == 0 ? "Nothing else was open to end." : Capitalise(JoinAnd(clauses)) + ".";
        return new(
            "The personal data is erased.",
            $"The request is approved and completed. {ended} The account is now {outcome.Pseudonym}; its open session ends within a minute.");
    }

    public static DataRightsMessage Completed(DataRightsRequestDetailDto request)
        => new("Request completed.", $"{request.RequesterName} is emailed, and reads it as Completed on My data rights.");

    // ---- the refusals (D10, D11, D12, D16, D21, S13) ----

    /// <summary>A decision refused by the command, by its reason (never its message; T329). The request is read again first.</summary>
    public static DataRightsMessage DecisionRefused(DataRightsRefusal reason, string requesterName) => reason switch
    {
        DataRightsRefusal.Withdrawn => new($"{requesterName} withdrew this request.", "Nothing was decided or erased."),
        DataRightsRefusal.DecidedMeanwhile => new("This request was decided while this page was open.", "Nothing more was done."),
        DataRightsRefusal.ErasureNotConfigured => new(
            "Nothing has been erased.",
            "Erasure is not set up on this server. The request is still Submitted. Ask whoever runs the server to set it up, then approve again."),
        DataRightsRefusal.LastAdministrator => new(
            "Nothing has been erased.",
            $"{requesterName} is Wombat's only Administrator, and erasing the account would leave nobody to run it. Add another Administrator first, then approve again."),
        // D11 draws the executor's sentence whole, with no bold lead.
        DataRightsRefusal.PersonChanged => new(DataRightsRefusalSentences.PersonChanged),
        _ => DecisionFailed
    };

    /// <summary>A decision that failed for any other cause (D16); the cause goes to the log.</summary>
    public static readonly DataRightsMessage DecisionFailed =
        new("The decision could not be saved just now. Nothing has changed. Try again, or come back in a few minutes.");

    // ---- the erasure dialog (D07, D13, D14; Q3, Q19, F5, M2, N8) ----

    public static string DialogTitle(string name) => $"Erase {name}'s personal data?";

    public const string DialogCannotBeUndone = "This cannot be undone.";

    public const string DialogInOneStep = "Approving erases it now, in one step, and completes the request.";

    public const string DialogEnds = "It ends:";

    public const string DialogCancel = "Cancel";

    public const string DialogConfirm = "Erase and approve";

    /// <summary>The forecast could not be read: the dialog is not opened, and Decide says so (NEW).</summary>
    public static readonly DataRightsMessage ForecastFailed =
        new("What the erasure would end could not be read just now. Nothing has changed. Try again, or come back in a few minutes.");

    /// <summary>"It ends:"'s lines, from the forecast, in D13's order; a line for what is not there only for campaigns (D07).</summary>
    public static IReadOnlyList<string> DialogEndsLines(ErasureForecastDto forecast)
    {
        var name = forecast.PersonName;
        var lines = new List<string>
        {
            $"The account and its sign-in. {name} can no longer sign in, and an open session ends within a minute."
        };

        if (forecast.StaffRoleKeys.Count == 1)
        {
            lines.Add($"The role {WombatRoleLabels.For(forecast.StaffRoleKeys[0])} and its scope.");
        }
        else if (forecast.StaffRoleKeys.Count > 1)
        {
            lines.Add($"The roles {JoinAnd(forecast.StaffRoleKeys.Select(WombatRoleLabels.For).ToList())} and their scopes.");
        }

        var reviews = forecast.OpenReviews.Count + forecast.OpenReviewsElsewhere;
        if (reviews > 0)
        {
            var named = string.Join("; ", forecast.OpenReviews.Select(OpenReview));
            var elsewhere = forecast.OpenReviewsElsewhere switch
            {
                0 => null,
                1 => "1 open review at another institution",
                var n => $"{n.ToString(CultureInfo.InvariantCulture)} open reviews at other institutions"
            };
            var list = (named.Length, elsewhere) switch
            {
                (> 0, null) => named,
                (> 0, { } counted) => $"{named}, and {counted}",
                (_, { } counted) => counted,
                _ => string.Empty
            };
            lines.Add(reviews == 1
                ? $"1 open committee review: {list}. It is withdrawn, and nothing more is decided at it."
                : $"{reviews.ToString(CultureInfo.InvariantCulture)} open committee reviews: {list}. Each is withdrawn, and nothing more is decided at it.");
        }

        lines.Add(forecast.OpenCampaigns.Count switch
        {
            0 => $"No MSF campaign: none about {name} is open.",
            1 => $"1 MSF campaign not yet released: {forecast.OpenCampaigns[0].Name}.",
            var n => $"{n.ToString(CultureInfo.InvariantCulture)} MSF campaigns not yet released: {string.Join("; ", forecast.OpenCampaigns.Select(campaign => campaign.Name))}."
        });

        if (forecast.OtherOpenRequests.Count > 0)
        {
            var requests = string.Join("; ", forecast.OtherOpenRequests.Select(request =>
                $"{DataRightsWords.Type(request.Type)}, made {DataRightsWords.When(request.RequestedOn)}"));
            lines.Add(forecast.OtherOpenRequests.Count == 1
                ? $"1 other open data rights request: {requests}. It is withdrawn."
                : $"{forecast.OtherOpenRequests.Count.ToString(CultureInfo.InvariantCulture)} other open data rights requests: {requests}. Each is withdrawn.");
        }

        if (forecast.ProgrammeName is { } programme)
        {
            lines.Add($"The programme, {programme}, which ends today as Withdrawn.");
        }

        return lines;
    }

    /// <summary>
    /// "It keeps …" (D07, D14, N8): the settled reviews by number, the records every erasure keeps, this request, and a
    /// review under appeal last, which can still be answered.
    /// </summary>
    public static string DialogKeeps(ErasureForecastDto forecast)
    {
        var settled = forecast.KeptReviews.Where(review => review.State != CommitteeReviewState.UnderAppeal).Select(Kept).ToList();
        var appeals = forecast.KeptReviews.Where(review => review.State == CommitteeReviewState.UnderAppeal).ToList();
        var parts = new List<string>(settled) { "the activities, progress and audit entries" };
        if (appeals.Count == 0)
        {
            parts.Add("and this request");
        }
        else
        {
            parts.Add("this request");
            var named = string.Join("; ", appeals.Select(review => $"{review.TypeLabel} #{review.Id.ToString(CultureInfo.InvariantCulture)}, {review.PanelName}"));
            parts.Add(appeals.Count == 1
                ? $"and the review under appeal, which can still be answered ({named})"
                : $"and the reviews under appeal, which can still be answered ({named})");
        }

        return $"It keeps, under a pseudonym that names nobody: {string.Join(", ", parts)}.";
    }

    /// <summary>An open review as the dialog names it: "Formative check-in #8, Paed Annual Review Panel, Scheduled".</summary>
    private static string OpenReview(ErasureForecastReview review)
        => $"{review.TypeLabel} #{review.Id.ToString(CultureInfo.InvariantCulture)}, {review.PanelName}, {CommitteeDecisionWording.StateLabel(review.State)}";

    /// <summary>A settled review as "It keeps" names it (N8): "the ratified review #5", "closed check-in #6".</summary>
    private static string Kept(ErasureForecastReview review)
    {
        var id = review.Id.ToString(CultureInfo.InvariantCulture);
        var state = CommitteeDecisionWording.StateLabel(review.State).ToLowerInvariant();
        return string.Equals(review.TypeLabel, CommitteeDecisionWording.FormativeTypeLabel, StringComparison.Ordinal)
            ? $"{state} check-in #{id}"
            : $"the {state} review #{id}";
    }

    // ---- the Correction section (C01–C11; Q26, S2, S11, F6) ----

    public const string CorrectionHeading = "Correction";

    public const string CorrectionIntro = "Record each change as it is made on the page that holds the data.";

    public const string NoCorrection = "No correction is recorded yet.";

    public const string CorrectionsCaption = "Corrections recorded on this request";

    public const string RecordLegend = "Record";

    public const string OtherLabel = "Other";

    public const string OtherHelp = "Name the record in the field under it.";

    public const string OtherNameLabel = "Record's name";

    public const string BeforeLabel = "Value before";

    public const string AfterLabel = "Value after";

    public const string RecordCorrection = "Record correction";

    public const string CompleteRequest = "Complete request";

    public const string ChooseTheRecord = "Choose the record.";

    public const string EnterBefore = "Enter the value before.";

    public const string EnterAfter = "Enter the value after.";

    public const string NameTheRecord = "Name the record.";

    public static readonly string NameTooLong =
        $"Name the record in {DataRightsRecords.OtherNameMaxLength.ToString(CultureInfo.InvariantCulture)} characters or fewer.";

    /// <summary>Complete request with nothing recorded (T316, C03), the page's check and the command's alike.</summary>
    public const string RecordOneFirst = "Record at least one correction before completing the request.";

    /// <summary>A record whose request is no longer an approved Rectification (decided or completed meanwhile; NEW).</summary>
    public const string NoLongerApproved = "This request is no longer waiting for a correction. Nothing was recorded.";

    public static readonly DataRightsMessage CorrectionFailed =
        new("The correction could not be recorded just now. Nothing has changed. Try again, or come back in a few minutes.");

    /// <summary>Complete request failed for a cause that is not a refusal (NEW, as C10's).</summary>
    public static readonly DataRightsMessage CompleteFailed =
        new("The request could not be completed just now. Nothing has changed. Try again, or come back in a few minutes.");

    public static DataRightsMessage CorrectionRecorded(string targetType) => new($"Correction recorded: {DataRightsWords.Record(targetType)}.");

    /// <summary>Complete request's own line (C01).</summary>
    public static string CompleteHelp(string name)
        => $"Once every correction is made and recorded, complete the request. It then reads Completed here and on {name}'s My data rights.";

    /// <summary>A name's help: changed on My account (C01), or, from an institution's sign-in, not by Wombat (C11, F6).</summary>
    public static string NameHelp(DataRightsRequestDetailDto request)
        => request.NameFromSignIn is { } institution
            ? $"This name comes from {institution}'s sign-in; the institution corrects it."
            : $"Changed by {request.RequesterName} on My account. No staff page changes a name.";

    /// <summary>The programme start's help (C01, C07, C08; S2): archived, a link for one who may open it, or who does.</summary>
    public static string ProgrammeStartHelp(DataRightsRequestDetailDto request, ProgrammeStartRecordDto record)
    {
        if (record.Ended)
        {
            return TraineeProfileWords.EndedLine;
        }

        if (record.MayOpen)
        {
            return $"Changed on {request.RequesterName}'s programme page.";
        }

        return request.InstitutionName is { } institution
            ? $"Changed on {request.RequesterName}'s programme page, which an Institutional admin at {institution} opens; you cannot."
            : $"Changed on {request.RequesterName}'s programme page, which an Institutional admin opens; you cannot.";
    }

    /// <summary>C07's link, for the Administrator or an Institutional admin of the profile's institution.</summary>
    public static string OpenProgramme(string name) => $"Open {name}'s programme";

    /// <summary>The Recorded cell (C05): "Pieter Smit · 2026-10-28 00:05 SAST".</summary>
    public static string Recorded(DataRightsCorrectionDto correction)
        => $"{correction.AppliedByName} · {DataRightsWords.When(correction.AppliedOn)}";

    // ---- helpers ----

    private static string DataWordsDay(DateOnly day) => DataRightsWords.Day(day);

    private static string Count(int n, string one, string many)
        => n == 1 ? $"1 {one}" : $"{n.ToString(CultureInfo.InvariantCulture)} {many}";

    private static string Capitalise(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

    /// <summary>"a", "a and b", "a, b and c".</summary>
    private static string JoinAnd(IReadOnlyList<string> parts) => parts.Count switch
    {
        0 => string.Empty,
        1 => parts[0],
        _ => $"{string.Join(", ", parts.Take(parts.Count - 1))} and {parts[^1]}"
    };
}
