using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Features.Activities.Services;
using Wombat.Domain.Activities;
using Wombat.Domain.Epas;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Application.Features.MultiSourceFeedback;

/// <summary>
/// Which EPAs a released multi-source feedback campaign covered, read from the evidence rows its release wrote. (T186)
/// </summary>
/// <remarks>
/// <para>
/// <b>The rule.</b> A campaign covers an EPA exactly when it is released and one of its evidence activities carries that
/// EPA: a finished activity of the evidence type about the campaign's subject, whose <c>DataJson</c> names the campaign
/// under <see cref="CampaignIdField" /> and whose stamped <see cref="Activity.EpaId" /> (T137) is the EPA. A release
/// writes one such row per EPA it could honour (<see cref="ReleaseMsfCampaignCommandHandler" />, T121), in the same
/// commit as the release, so the rows are the fact itself rather than a record of it.
/// </para>
/// <para>
/// <b>Why not <see cref="MsfCampaignEpa.RecordedOn" />.</b> That per-EPA stamp is the release's own note of the same
/// fact, and it can be missing where the evidence is not: campaigns released before the stamp existed have their
/// <c>msf_cpsa</c> rows and a null stamp. Read from the stamp, the committee snapshot said such a campaign's EPAs were
/// "no longer on the trainee's curriculum" and the coverage grid said they were not covered. Every reader of coverage
/// reads it here: the committee snapshot's campaign line, the coverage grid, and the campaign report the coordinator,
/// the trainee and the portfolio PDF print (<see cref="IMsfAggregationService" />), so none can disagree with the
/// evidence a panel sees listed beside them, or with another. Nothing reads the stamp. The declared set
/// (<see cref="MsfCampaign.CoveredEpas" />) still says what the campaign was offered as evidence for; this says which of
/// those, if any, it became.
/// </para>
/// <para>
/// <b>Released only, whatever the caller passes.</b> Before release the trainee has not seen the report and the
/// coordinator may still withdraw it, and a withdrawn campaign was retracted (T138). Release is the only writer of these
/// rows and withdrawal refuses a released campaign, so an unreleased campaign has none; the state is checked here all the
/// same, so that the rule is this class's and not each caller's.
/// </para>
/// <para>
/// <b>The evidence type follows the campaign's kind.</b> A multi-source feedback release writes <c>msf_cpsa</c> and a
/// learner-feedback release writes <c>learner_feedback_cpsa</c> (T164), and <see cref="MsfEvidenceKinds.ActivityTypeKeyFor" />
/// is the one mapping, which the release writes by. A reader that holds campaigns of either kind passes each campaign's
/// kind and is answered from that kind's rows, so a learner-feedback campaign is recorded exactly when a
/// <c>learner_feedback_cpsa</c> row names it and never by an <c>msf_cpsa</c> row, and the other way round. A reader that
/// wants one instrument only (the MSF coverage grid) passes that instrument's key.
/// </para>
/// <para>
/// <b>Finished rows only</b>, by the one definition of finished (<see cref="ActivityCompletion" />, D44): the row's state
/// is a terminal state of the workflow it is pinned to. <c>msf_cpsa</c> rows are written straight into their terminal
/// <c>recorded</c> by the release and nobody may create one by hand (T162), so for MSF this removes nothing. It is here
/// because the evidence type is a parameter: a type whose rows can sit in a draft, or end declined or cancelled, must not
/// have an unfinished row read as coverage.
/// </para>
/// <para>
/// <b>What is read.</b> One query: the subject's rows of the evidence type that carry an EPA, joined to the EPA for its
/// code. The campaign id lives only inside <c>DataJson</c>, which is not portable SQL, so it is matched here, over one
/// subject's rows of one system-written type: a row per EPA per campaign, tens in a programme. Only if a row names a
/// released campaign are the finished states of those rows' pinned workflows read
/// (<see cref="ActivityCompletion.LoadFinishedStatesAsync" />). Nothing leaves but campaign ids and EPAs. No activity
/// id, no <c>DataJson</c> and no rating reaches a caller, which is why this does not
/// go through <c>ActivityReadScope.WhereReadableBy</c>: the facts returned are the campaign's, and each caller has
/// already authorised reading the trainee's campaigns (a panel's access to its review; <c>TraineeScopeResolver</c>;
/// <see cref="MsfCampaignRules.CanReadReportAsync" />; the portfolio export's own authorisation).
/// </para>
/// </remarks>
public static class MsfCampaignCoverage
{
    /// <summary>The field of an evidence row's data that names the campaign whose release wrote it.</summary>
    public const string CampaignIdField = "campaign_id";

    /// <summary>
    /// For each released campaign of <paramref name="campaigns" />, the EPAs its finished evidence rows carry, each
    /// campaign read from the evidence type its own kind's release writes (<see cref="MsfEvidenceKinds.ActivityTypeKeyFor" />,
    /// T164). One read per kind present, so at most two.
    /// </summary>
    /// <param name="subjectUserId">The trainee the campaigns are about. Only their evidence rows are read.</param>
    /// <param name="campaigns">The campaigns to read, each with its state and its template's kind.</param>
    public static async Task<ILookup<int, MsfRecordedEpa>> RecordedEpasAsync(
        IApplicationDbContext dbContext,
        string subjectUserId,
        IEnumerable<(int CampaignId, MsfCampaignState State, MsfTemplateKind Kind)> campaigns,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(campaigns);

        var recorded = new List<(int CampaignId, MsfRecordedEpa Epa)>();
        foreach (var kind in campaigns.GroupBy(campaign => campaign.Kind))
        {
            var lookup = await RecordedEpasAsync(
                dbContext,
                subjectUserId,
                kind.Select(campaign => (campaign.CampaignId, campaign.State)),
                MsfEvidenceKinds.ActivityTypeKeyFor(kind.Key),
                cancellationToken);

            recorded.AddRange(lookup.SelectMany(group => group.Select(epa => (group.Key, epa))));
        }

        // A campaign has one kind, so each campaign's EPAs come from one read, already in EPA code order.
        return recorded.ToLookup(entry => entry.CampaignId, entry => entry.Epa);
    }

    /// <summary>
    /// For each released campaign of <paramref name="campaigns" />, the EPAs its finished evidence rows of
    /// <paramref name="evidenceTypeKey" /> carry, in EPA code order. A campaign with none, and every campaign that is not
    /// released, has no entry, which an <see cref="ILookup{TKey,TElement}" /> answers with an empty sequence.
    /// </summary>
    /// <param name="subjectUserId">The trainee the campaigns are about. Only their evidence rows are read.</param>
    /// <param name="campaigns">The campaigns to read, each with its state. Every one must be about <paramref name="subjectUserId" />.</param>
    /// <param name="evidenceTypeKey">The activity type key the campaigns' evidence is recorded as.</param>
    public static async Task<ILookup<int, MsfRecordedEpa>> RecordedEpasAsync(
        IApplicationDbContext dbContext,
        string subjectUserId,
        IEnumerable<(int CampaignId, MsfCampaignState State)> campaigns,
        string evidenceTypeKey,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentException.ThrowIfNullOrWhiteSpace(subjectUserId);
        ArgumentNullException.ThrowIfNull(campaigns);
        ArgumentException.ThrowIfNullOrWhiteSpace(evidenceTypeKey);

        var released = campaigns
            .Where(campaign => campaign.State == MsfCampaignState.Released)
            .Select(campaign => campaign.CampaignId)
            .ToHashSet();

        if (released.Count == 0)
        {
            return None();
        }

        var rows = await dbContext.Set<Activity>()
            .AsNoTracking()
            .Where(activity => activity.SubjectUserId == subjectUserId &&
                               activity.ActivityType.Key == evidenceTypeKey &&
                               activity.EpaId != null)
            .Join(
                dbContext.Set<Epa>(),
                activity => activity.EpaId,
                epa => (int?)epa.Id,
                (activity, epa) => new
                {
                    EpaId = epa.Id,
                    epa.Code,
                    activity.DataJson,
                    activity.ActivityTypeId,
                    activity.SchemaVersion,
                    activity.CurrentState
                })
            .ToListAsync(cancellationToken);

        var naming = rows
            .Select(row => (CampaignId: ReadCampaignId(row.DataJson), Row: row))
            .Where(entry => entry.CampaignId is int campaignId && released.Contains(campaignId))
            .ToList();

        if (naming.Count == 0)
        {
            return None();
        }

        // Evidence is a finished activity (D44), judged against the workflow each row is pinned to. Read only once a row
        // names a released campaign, so a trainee with no such evidence costs no second read.
        var finishedStates = await ActivityCompletion.LoadFinishedStatesAsync(
            dbContext,
            naming.Select(entry => (entry.Row.ActivityTypeId, entry.Row.SchemaVersion)),
            cancellationToken);

        return naming
            .Where(entry => finishedStates[(entry.Row.ActivityTypeId, entry.Row.SchemaVersion)].Contains(entry.Row.CurrentState))
            .Select(entry => (CampaignId: entry.CampaignId!.Value, Epa: new MsfRecordedEpa(entry.Row.EpaId, entry.Row.Code)))
            .DistinctBy(row => (row.CampaignId, row.Epa.EpaId))
            .OrderBy(row => row.Epa.EpaCode, StringComparer.Ordinal)
            .ThenBy(row => row.Epa.EpaId)
            .ToLookup(row => row.CampaignId, row => row.Epa);
    }

    private static ILookup<int, MsfRecordedEpa> None()
        => Array.Empty<(int CampaignId, MsfRecordedEpa Epa)>().ToLookup(entry => entry.CampaignId, entry => entry.Epa);

    /// <summary>
    /// The campaign an evidence row names, or null when its data is not an object with an integer
    /// <see cref="CampaignIdField" />. The release writes it as a JSON number.
    /// </summary>
    private static int? ReadCampaignId(string dataJson)
    {
        try
        {
            using var document = JsonDocument.Parse(dataJson);
            return document.RootElement.ValueKind == JsonValueKind.Object &&
                   document.RootElement.TryGetProperty(CampaignIdField, out var value) &&
                   value.ValueKind == JsonValueKind.Number &&
                   value.TryGetInt32(out var campaignId)
                ? campaignId
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

/// <summary>An EPA a released campaign recorded evidence for (<see cref="MsfCampaignCoverage" />).</summary>
public sealed record MsfRecordedEpa(int EpaId, string EpaCode);
