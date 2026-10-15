using System.Text.RegularExpressions;
using AngleSharp.Dom;
using MediatR;
using Wombat.Application.Features.MultiSourceFeedback;
using Wombat.Domain.MultiSourceFeedback;
using Wombat.Web.Services;

namespace Wombat.Web.Tests.MultiSourceFeedback;

/// <summary>
/// The MSF report's cast as round 3 draws it (T369, flow 10, lane R): Lerato Molefe's Default MSF, 2026-10-14 to
/// 2026-10-28, nine invited, minimums 5 / 3 / 2, evidence for PAED-010 and PAED-012; once closed, the Consultant and Nurse
/// groups shown "from 3 responses" and Peer doctor and Allied health professional suppressed.
/// </summary>
internal static class MsfReportFixtures
{
    public const int CampaignId = 5;
    public const string TraineeUserId = "trainee-1";
    public const string TraineeName = "Lerato Molefe";
    public const string CoordinatorUserId = "coordinator-1";

    public static readonly DateOnly OpensOn = new(2026, 10, 14);
    public static readonly DateOnly ClosesOn = new(2026, 10, 28);

    /// <summary>14:34 UTC, 16:34 SAST: the release time 3.46 reads.</summary>
    public static readonly DateTime ReleasedOn = new(2026, 10, 14, 14, 34, 0, DateTimeKind.Utc);

    public static readonly DateTime ClosedOn = new(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);

    public static MsfCoveredEpaDto Epa(int id, string code, string title, bool willBeRecorded = true, bool recorded = false)
        => new(id, code, title, recorded) { WillBeRecorded = willBeRecorded };

    public static readonly MsfCoveredEpaDto Paed010 = Epa(110, "PAED-010", "Leading and operating within a clinical team");

    public static readonly MsfCoveredEpaDto Paed012 =
        Epa(112, "PAED-012", "Communicating with and counselling patients, caregivers and healthcare teams");

    /// <summary>The report as the handler hands it to the coordinator, in <paramref name="state" />.</summary>
    public static MsfCampaignAggregateReportDto Report(
        MsfCampaignState state,
        int totalResponses = 8,
        int surviving = 2,
        bool? ready = null,
        IReadOnlyList<MsfCategoryAggregateDto>? groups = null,
        IReadOnlyList<MsfCoveredEpaDto>? epas = null)
    {
        var closed = state is MsfCampaignState.UnderReview or MsfCampaignState.Released or MsfCampaignState.Closed;
        return new MsfCampaignAggregateReportDto(
            CampaignId, TraineeUserId, "Default MSF", state, 5, 3, totalResponses, null,
            ready ?? (totalResponses >= 5 && surviving >= 2),
            groups ?? (closed ? MolefeGroups() : []),
            2, surviving,
            epas ?? [Paed010, Paed012],
            null,
            state == MsfCampaignState.Released ? ReleasedOn : null)
        {
            SubjectName = TraineeName,
            OpensOn = OpensOn,
            ClosesOn = ClosesOn,
            InvitedTotal = 9,
            ClosedOn = closed ? ClosedOn : null,
            ClosedByUserId = closed ? CoordinatorUserId : null,
            ReleasedOn = state == MsfCampaignState.Released ? ReleasedOn : null,
            SubjectIsCurrent = true
        };
    }

    /// <summary>3.44's groups: Peer doctor 1 and Allied health professional 1 suppressed, Consultant and Nurse 3 each.</summary>
    public static IReadOnlyList<MsfCategoryAggregateDto> MolefeGroups()
        =>
        [
            Suppressed(MsfRespondentCategory.PeerDoctor, 1),
            Shown(MsfRespondentCategory.Consultant, 4.333, "Sound judgement in clinic; teaches the interns well",
                "Ready to run a ward; should document escalation decisions more fully", "Keeps the team calm"),
            Shown(MsfRespondentCategory.Nurse, 4.667, "Calm and kind with parents at night; hands over clearly",
                "Leads neonatal resuscitations and explains her decisions to the nurses", "Always on time"),
            Suppressed(MsfRespondentCategory.Ahp, 1)
        ];

    public const string Question = "What should the trainee keep doing or improve?";

    public static MsfCategoryAggregateDto Shown(MsfRespondentCategory category, double average, params string[] comments)
        => new(category, comments.Length, false,
        [
            new MsfQuestionAggregateDto(1, "Overall, how does the trainee perform?", MsfQuestionType.Scale,
                new MsfScaleAggregateDto(average, comments.Length), []),
            new MsfQuestionAggregateDto(2, Question, MsfQuestionType.LongText, null, comments)
        ]);

    public static MsfCategoryAggregateDto Suppressed(MsfRespondentCategory category, int responses)
        => new(category, responses, true, []);

    public static string Text(IElement element) => Regex.Replace(element.TextContent, @"\s+", " ").Trim();
}

/// <summary>
/// Answers the report as it stands, and closes and releases it as the handlers would: it reads back under review, its
/// groups read (F1), or released. A refusal leaves it in <see cref="StateAfterRefusal" />, as a campaign changed elsewhere.
/// </summary>
internal sealed class MsfReportSender(MsfCampaignAggregateReportDto report) : IScopedSender
{
    private MsfCampaignAggregateReportDto _report = report;
    private TaskCompletionSource? _held;
    private bool _acted;

    /// <summary>Every close and release sent, in order.</summary>
    public List<object> Commands { get; } = [];

    /// <summary>How many times the report was read.</summary>
    public int Reads { get; private set; }

    public Exception? Refusal { get; init; }

    public MsfCampaignState? StateAfterRefusal { get; init; }

    /// <summary>What every read throws once a command has answered, done or refused.</summary>
    public Exception? ReadFailureAfterCommand { get; init; }

    /// <summary>What the first read throws (R14).</summary>
    public Exception? FirstReadFailure { get; set; }

    /// <summary>Holds each close and release until <see cref="Release" />.</summary>
    public bool Hold { get; init; }

    public void Release() => (_held ?? throw new InvalidOperationException("Nothing was held.")).SetResult();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        switch (request)
        {
            case GetCampaignAggregateReportQuery:
                if (FirstReadFailure is { } failure)
                {
                    FirstReadFailure = null;
                    return Task.FromException<TResponse>(failure);
                }

                if (_acted && ReadFailureAfterCommand is not null)
                {
                    return Task.FromException<TResponse>(ReadFailureAfterCommand);
                }

                Reads++;
                return Task.FromResult((TResponse)(object)_report);

            case CloseMsfCampaignCommand close:
                Commands.Add(close);
                if (Refusal is not null)
                {
                    return Refuse<TResponse>();
                }

                _report = _report with
                {
                    State = MsfCampaignState.UnderReview,
                    ClosedOn = MsfReportFixtures.ClosedOn,
                    ClosedByUserId = MsfReportFixtures.CoordinatorUserId,
                    Categories = _report.Categories.Count > 0 ? _report.Categories : MsfReportFixtures.MolefeGroups()
                };
                _acted = true;
                return Held((TResponse)(object)_report);

            default:
                throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }
    }

    public Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        if (request is not ReleaseMsfCampaignCommand release)
        {
            throw new NotSupportedException($"Unhandled request: {request.GetType().Name}");
        }

        Commands.Add(release);
        if (Refusal is not null)
        {
            return Refuse<bool>();
        }

        _report = _report with
        {
            State = MsfCampaignState.Released,
            ReleasedOn = MsfReportFixtures.ReleasedOn,
            EvidenceRecordedOn = MsfReportFixtures.ReleasedOn,
            CoordinatorNarrative = release.Narrative,
            ReviewerEntrustmentLevel = release.EntrustmentLevel,
            CoveredEpas = _report.CoveredEpas.Select(epa => epa with { Recorded = epa.WillBeRecorded }).ToList()
        };
        _acted = true;
        return Held(true);
    }

    private Task<T> Refuse<T>()
    {
        _acted = true;
        if (StateAfterRefusal is { } state)
        {
            _report = _report with { State = state };
        }

        return Task.FromException<T>(Refusal!);
    }

    private Task<T> Held<T>(T result)
    {
        if (!Hold)
        {
            return Task.FromResult(result);
        }

        _held = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        return AfterAsync(_held.Task, result);
    }

    private static async Task<T> AfterAsync<T>(Task held, T result)
    {
        await held;
        return result;
    }
}
