using Wombat.Domain.CommitteeDecisions;
using Wombat.Domain.DataRights;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.DataRights;

/// <summary>
/// A request as a command answers with it (submit, approve, reject, complete). The pages read their own DTOs below
/// (T387): <see cref="MyDataRightsDto" />, <see cref="DataRightsRequestSummaryDto" /> and
/// <see cref="DataRightsRequestDetailDto" />.
/// </summary>
public sealed record DataRightsRequestDto(
    Guid Id,
    string RequesterUserId,
    string RequesterDisplayName,
    DateTime RequestedOn,
    DataRightsRequestType Type,
    DataRightsRequestStatus Status,
    string Reason,
    string? DecisionNote,
    string? DecidedByUserId,
    DateTime? DecidedOn,
    DateTime? CompletedOn);

/// <summary>
/// One row of the queue (Data rights requests; T387, Q10, F1, S1, Q23). What it carries of a decision is unchanged: the
/// status alone.
/// </summary>
/// <param name="RequesterName">
/// The account's first and last name, read live (F1, lane K: <c>GetContactsAsync</c>), so a corrected name shows; once the
/// account is erased, the pseudonym (its erasure record's, which the erasure also wrote over the stored name, F3). The
/// stored <c>RequesterDisplayName</c> ("First Last" since T387) only for an account the directory no longer holds.
/// </param>
public sealed record DataRightsRequestSummaryDto(
    Guid Id,
    string RequesterName,
    DateTime RequestedOn,
    DataRightsRequestType Type,
    DataRightsRequestStatus Status)
{
    /// <summary>The account's address, read live (S1); null once the account is erased, so no address is shown.</summary>
    public string? RequesterEmail { get; init; }

    /// <summary>The South African day the requester was erased (N5, N13: "Erased 2026-10-28"); null for anyone else.</summary>
    public DateOnly? ErasedOn { get; init; }

    /// <summary>
    /// The request's institution by name, for the Administrator's Institution column (Q23); null for a request with no
    /// institution (the page says "None"), and for a scoped reviewer, who sees one institution's requests.
    /// </summary>
    public string? InstitutionName { get; init; }
}

/// <summary>A page of the queue, and whose requests it is (S4, S12: the subtitle, the caption and the empty sentence).</summary>
public sealed record PagedDataRightsResult(
    IReadOnlyList<DataRightsRequestSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    /// <summary>The caller sees every institution's requests: a global Administrator.</summary>
    public bool EveryInstitution { get; init; }

    /// <summary>
    /// The scoped reviewer's institution by name; null for the Administrator (<see cref="EveryInstitution" />) and for a
    /// Coordinator whose account is set to no institution, who sees no request (S4).
    /// </summary>
    public string? InstitutionName { get; init; }
}

/// <summary>My data rights (T387, S3, S10, T319): the requester's own requests, each with its answer, and where they are.</summary>
/// <param name="InstitutionName">
/// The requester's institution by name, for "Staff at ‹institution› decide it"; null for an account set to no institution
/// (M18's other sentence).
/// </param>
public sealed record MyDataRightsDto(
    string? InstitutionName,
    IReadOnlyList<MyDataRightsRequestDto> Requests);

/// <summary>
/// One of the requester's requests with its answer (T319, Q5): the decision, its day and its note, never who decided.
/// </summary>
/// <param name="CorrectedRecords">
/// The records corrected on a Rectification, as <see cref="DataRightsRecords" /> keys, each once, in the order first
/// recorded (one grouped read of the corrections); empty for every other request.
/// </param>
public sealed record MyDataRightsRequestDto(
    Guid Id,
    DateTime RequestedOn,
    DataRightsRequestType Type,
    DataRightsRequestStatus Status,
    DateTime? DecidedOn,
    string? DecisionNote,
    DateTime? CompletedOn,
    IReadOnlyList<string> CorrectedRecords);

/// <summary>
/// A request's page (T387, Q12, S14): everything Request, Decision, Correction and Decide draw. Null from the query for an
/// unknown id and for one the caller may not open alike, so the page draws one Request unavailable for both (Q13, Q17).
/// </summary>
/// <param name="RequesterName">
/// As <see cref="DataRightsRequestSummaryDto.RequesterName" />: the live name (F1), the pseudonym once erased. Every sentence
/// on the page that names the requester uses it.
/// </param>
public sealed record DataRightsRequestDetailDto(
    Guid Id,
    string RequesterUserId,
    string RequesterName,
    DateTime RequestedOn,
    DataRightsRequestType Type,
    DataRightsRequestStatus Status,
    string Reason,
    string? DecisionNote,
    string? DecidedByUserId,
    DateTime? DecidedOn,
    DateTime? CompletedOn)
{
    /// <summary>The account's address, read live (S1); null once erased.</summary>
    public string? RequesterEmail { get; init; }

    /// <summary>The request's institution by name, for the subtitle; null for a request with no institution (S8).</summary>
    public string? InstitutionName { get; init; }

    /// <summary>The South African day the requester was erased (N5); null for anyone else.</summary>
    public DateOnly? ErasedOn { get; init; }

    /// <summary>
    /// Who decided, by name (Decided by: the name over the id); null while undecided; the pseudonym if the decider was
    /// erased; the id itself for an account the directory does not hold.
    /// </summary>
    public string? DecidedByName { get; init; }

    /// <summary>The corrections recorded on a Rectification, in the order recorded (Corrections recorded on this request).</summary>
    public IReadOnlyList<DataRightsCorrectionDto> Corrections { get; init; } = [];

    /// <summary>
    /// The caller may decide it now: <see cref="DataRightsAuthorization.CanReview" /> and the request is Submitted (§ 0).
    /// Decide is drawn only when true.
    /// </summary>
    public bool MayDecide { get; init; }

    /// <summary>
    /// The caller may record a correction and complete it now: <see cref="DataRightsAuthorization.CanRectify" />, a
    /// Rectification, Approved (§ 0). The Correction section's form and Complete request are drawn only when true.
    /// </summary>
    public bool MayRecord { get; init; }

    /// <summary>
    /// The caller may download the request's export (<see cref="DataRightsAuthorization.CanDownload" />); Decide's "You
    /// cannot download it yourself." is shown when false (N2). The request page offers no Download.
    /// </summary>
    public bool MayDownload { get; init; }

    /// <summary>The caller asked for it and would otherwise decide it (Q21, F9): Decide is replaced by the line saying who does.</summary>
    public DataRightsOwnRequest OwnRequest { get; init; }

    /// <summary>
    /// The requester's programme start as a correction's record (S2); null when the requester holds no trainee profile, and
    /// the radio is not offered. Read for a Rectification only, and never for an erased requester.
    /// </summary>
    public ProgrammeStartRecordDto? ProgrammeStart { get; init; }

    /// <summary>
    /// The institution whose sign-in sets the requester's name (F6, S7: <c>GetSignInMethodsAsync</c>, the first
    /// institutional sign-in's display name); null when the person has none, for an erased requester, and for any request
    /// but a Rectification. When set, the two name radios are
    /// drawn disabled with "This name comes from ‹institution›'s sign-in; the institution corrects it." (R3-C11, the
    /// round's inference, PROPOSED), and such a correction records as Other.
    /// </summary>
    public string? NameFromSignIn { get; init; }
}

/// <summary>Whether a request is the caller's own, which they would otherwise decide (Q21, F9).</summary>
public enum DataRightsOwnRequest
{
    /// <summary>Not the caller's own, or the caller would not decide it anyway.</summary>
    No,

    /// <summary>A Coordinator's own: "another Coordinator at ‹institution›, or the Wombat administrator, decides it."</summary>
    AnotherCoordinatorDecides,

    /// <summary>An Administrator's own: "another Administrator decides it."</summary>
    AnotherAdministratorDecides
}

/// <summary>The requester's programme start, as the Correction section names it (S2).</summary>
/// <param name="ProfileId">The preferred profile (<c>TraineeScopeResolver.PreferredProfiles</c>), which "Open ‹name›'s programme" links.</param>
/// <param name="Ended">The programme has ended: its record is archived and cannot be changed (flow 13's <c>TraineeProfileWords.EndedLine</c>).</param>
/// <param name="MayOpen">The caller may open the profile: the Administrator, or an Institutional admin of its institution.</param>
public sealed record ProgrammeStartRecordDto(int ProfileId, bool Ended, bool MayOpen);

/// <summary>
/// One correction recorded on a Rectification (T387, S11): the record, the values as typed (the JSON strings read back),
/// and who recorded it when.
/// </summary>
/// <param name="TargetType">A <see cref="DataRightsRecords" /> key; its label is <see cref="DataRightsRecords.Label" />.</param>
public sealed record DataRightsCorrectionDto(
    Guid Id,
    string TargetType,
    string FromValue,
    string ToValue,
    string AppliedByUserId,
    string AppliedByName,
    DateTime AppliedOn);

/// <summary>What an erasure ended (T387, S6), counted where each was ended: the approval's result sentence reads it.</summary>
public sealed record ErasureOutcome(
    string Pseudonym,
    int ReviewsWithdrawn,
    int CampaignsWithdrawn,
    int RequestsWithdrawn,
    int ProfilesEnded);

/// <summary>An approval's answer: the request, and for an Erasure what the erasure ended (S6).</summary>
public sealed record ApproveDataRightsResult(
    DataRightsRequestDto Request,
    ErasureOutcome? Erasure);

/// <summary>
/// What approving an Erasure would end and keep, read when Approve and erase… is pressed (Q3, Q19, F5, M2, S5): the
/// dialog's counts. Gated as the command: null for a request the caller may not decide, or that is not a submitted
/// Erasure.
/// </summary>
/// <param name="PersonName">The requester's live name (F1).</param>
/// <param name="StaffRoleKeys">The roles the account holds other than Trainee and PendingTrainee, as keys (the page labels them, <c>WombatRoleLabels</c>); empty for none.</param>
/// <param name="OpenReviews">The person's open committee reviews the caller may see by name: every one for the Administrator, the caller's institution's for a Coordinator (F5).</param>
/// <param name="OpenReviewsElsewhere">The open reviews at another institution, counted, never named (F5: "and 1 open review at another institution").</param>
/// <param name="OpenCampaigns">The person's MSF campaigns not yet released, which the erasure withdraws.</param>
/// <param name="OtherOpenRequests">The person's other open data-rights requests, which the erasure withdraws (F3, M2).</param>
/// <param name="ProgrammeName">The active programme's curriculum, "Paediatric EPA Curriculum (11.2)"; null with no active profile.</param>
/// <param name="KeptReviews">The settled reviews kept under the pseudonym, the caller's to name as <see cref="OpenReviews" /> (N8: "the ratified review #5, closed check-in #6").</param>
public sealed record ErasureForecastDto(
    string PersonName,
    IReadOnlyList<string> StaffRoleKeys,
    IReadOnlyList<ErasureForecastReview> OpenReviews,
    int OpenReviewsElsewhere,
    IReadOnlyList<ErasureForecastCampaign> OpenCampaigns,
    IReadOnlyList<ErasureForecastRequest> OtherOpenRequests,
    string? ProgrammeName,
    IReadOnlyList<ErasureForecastReview> KeptReviews);

/// <summary>A review the dialog names: "Formative check-in #8, Paed Annual Review Panel, Scheduled".</summary>
/// <param name="TypeLabel">The review's type as a heading says it (<c>CommitteeDecisionWording</c>): "Formative check-in", "Annual review".</param>
public sealed record ErasureForecastReview(int Id, string TypeLabel, string PanelName, CommitteeReviewState State);

/// <summary>
/// A campaign the dialog names: <paramref name="Name" /> as the MSF pages tell a campaign from the others, without the
/// subject the dialog already names, "Annual MSF (closing 2029-03-21)" (flow 10's <c>MsfCampaignText.Describe</c>'s
/// parenthesis; <c>GetErasureForecastQueryHandler.CampaignName</c>), and its state.
/// </summary>
public sealed record ErasureForecastCampaign(int Id, string Name, MsfCampaignState State);

/// <summary>Another open request of the person: "Access, made 2026-10-28 00:03 SAST".</summary>
public sealed record ErasureForecastRequest(Guid Id, DataRightsRequestType Type, DateTime RequestedOn);

public sealed record DataRightsErasureRecordDto(
    Guid Id,
    Guid RequestId,
    string UserId,
    string Pseudonym,
    DateTime ErasedOn,
    string RetentionReasonsJson);

public sealed record AccessExportResult(
    byte[] ZipBytes,
    string FileName);
