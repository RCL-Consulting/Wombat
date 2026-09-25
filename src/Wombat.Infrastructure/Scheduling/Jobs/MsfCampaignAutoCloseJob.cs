using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Interfaces;
using Wombat.Application.Scheduling;
using Wombat.Domain.MultiSourceFeedback;

namespace Wombat.Infrastructure.Scheduling.Jobs;

/// <summary>
/// Closes, hourly, each open MSF campaign whose window has ended (<see cref="MsfCampaign.ClosesOn" /> before today), and
/// so anonymises its respondents (<see cref="MsfCampaign.Close" />, T184).
/// </summary>
/// <remarks>
/// <para>
/// <b>One campaign at a time</b> (T267). Each campaign is read, closed and saved on its own, on a context of its own. Until
/// T267 the run closed every expired campaign it had found and stored them in one save, checked against each campaign's
/// <c>xmin</c> token (<c>MsfCampaignConfiguration</c>). So a coordinator who closed or withdrew one of them between the
/// run's read and its save refused the whole save, and none of the others closed until the next run, an hour later. The
/// same went for a resend of a campaign's links (<c>ResendMsfLinks</c>), which writes the campaign row, unchanged, to be
/// checked against that token. An hour late is an hour more before its respondents are anonymised and its report is
/// ready to release. <see cref="MsfCampaign.ClosedOn" /> places a campaign by its UTC day: the semester its coverage counts
/// in, the committee window that sees it, and the date its evidence carries. A campaign falls due at midnight UTC, when
/// the first run after it is due, so an hour late moves that day only for a campaign already nearly a day overdue, as
/// after a host that was down; at the end of June or December that day is the semester's.
/// </para>
/// <para>
/// <b>A save the token refuses</b> is followed by one more read of that campaign. Closed or withdrawn elsewhere, it is
/// left as it now stands, its <see cref="MsfCampaign.ClosedOn" /> the one its own close wrote (T246). Still open and
/// expired, as after a resend, it is closed on that read. A second refusal leaves it open for the next run. Each campaign
/// left is logged, with its id and, when it is no longer open, its state.
/// </para>
/// <para>
/// <b>Any other failure</b> of one campaign's read or save does not stop the rest either. The run closes the others, then
/// fails, naming the campaigns it could not close, so the run is recorded as failed and the next run tries them again.
/// Nothing of a failed campaign is stored, since its changes were on its own context. A cancellation stops the run.
/// </para>
/// </remarks>
public sealed class MsfCampaignAutoCloseJob : IScheduledJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MsfCampaignAutoCloseJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public string Key => "msf-campaign-auto-close";
    public string CronExpression => "0 * * * *";
    public string Description => "Closes open MSF campaigns whose window has ended and anonymises their respondents (hourly).";

    public async Task ExecuteAsync(ScheduledJobContext context, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(context.UtcNow);

        List<int> expired;
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            expired = await scope.ServiceProvider.GetRequiredService<IApplicationDbContext>().Set<MsfCampaign>()
                .Where(IsDueToClose(today))
                .OrderBy(campaign => campaign.Id)
                .Select(campaign => campaign.Id)
                .ToListAsync(cancellationToken);
        }

        if (expired.Count == 0)
        {
            context.Logger.LogInformation("MsfCampaignAutoCloseJob: no expired campaigns found.");
            return;
        }

        var closed = 0;
        var skipped = 0;
        var leftOpen = 0;
        var failed = new List<int>();
        Exception? firstFailure = null;

        foreach (var campaignId in expired)
        {
            Attempt attempt;
            try
            {
                attempt = await TryCloseAsync(campaignId, today, context.UtcNow, cancellationToken);
                if (attempt.Outcome == Outcome.Changed)
                {
                    attempt = await TryCloseAsync(campaignId, today, context.UtcNow, cancellationToken);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                context.Logger.LogError(
                    exception,
                    "MsfCampaignAutoCloseJob: campaign {CampaignId} could not be closed; the run goes on with the others.",
                    campaignId);
                failed.Add(campaignId);
                firstFailure ??= exception;
                continue;
            }

            switch (attempt.Outcome)
            {
                case Outcome.Closed:
                    closed++;
                    break;

                case Outcome.NoLongerDue:
                    skipped++;
                    context.Logger.LogInformation(
                        "MsfCampaignAutoCloseJob: campaign {CampaignId} was closed or withdrawn elsewhere while this run was closing it (it is now {State}), so it was skipped.",
                        campaignId,
                        attempt.StateNow?.ToString() ?? "gone");
                    break;

                case Outcome.Changed:
                    // Counted apart from a skip: this campaign is still due, and the next run closes it.
                    leftOpen++;
                    context.Logger.LogWarning(
                        "MsfCampaignAutoCloseJob: campaign {CampaignId} changed twice while this run was closing it, so it was left open for the next run.",
                        campaignId);
                    break;
            }
        }

        context.Logger.LogInformation(
            "MsfCampaignAutoCloseJob: auto-closed {Closed} of {Found} expired campaigns; skipped {Skipped}, left open {LeftOpen}, failed {Failed}.",
            closed,
            expired.Count,
            skipped,
            leftOpen,
            failed.Count);

        if (firstFailure is not null)
        {
            throw new InvalidOperationException(
                $"MsfCampaignAutoCloseJob: {failed.Count} expired campaign(s) could not be closed ({string.Join(", ", failed)}); " +
                $"of the other {closed + skipped + leftOpen}, {closed} were closed, {skipped} skipped and {leftOpen} left open.",
                firstFailure);
        }
    }

    /// <summary>
    /// The one statement of which campaigns this job closes: open, and past their last day. The run's list and each
    /// campaign's own read ask it alike, so a campaign closed or withdrawn after the list was read is not closed again.
    /// </summary>
    private static Expression<Func<MsfCampaign, bool>> IsDueToClose(DateOnly today)
        => campaign => campaign.State == MsfCampaignState.Open && campaign.ClosesOn < today;

    /// <summary>
    /// Reads one campaign, as it is now, on a context of its own, and closes it if it is still due. Nothing it changed
    /// outlives the context, so a refused save leaves nothing for the next campaign's save to carry.
    /// </summary>
    private async Task<Attempt> TryCloseAsync(
        int campaignId,
        DateOnly today,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        // Closing anonymises every respondent (MsfCampaign.Close): the one routine the close command runs too, which is
        // why the invitations are loaded. (T184)
        var campaign = await dbContext.Set<MsfCampaign>()
            .Include(candidate => candidate.Invitations)
            .Where(candidate => candidate.Id == campaignId)
            .Where(IsDueToClose(today))
            .SingleOrDefaultAsync(cancellationToken);

        if (campaign is null)
        {
            var stateNow = await dbContext.Set<MsfCampaign>()
                .Where(candidate => candidate.Id == campaignId)
                .Select(candidate => (MsfCampaignState?)candidate.State)
                .SingleOrDefaultAsync(cancellationToken);

            return new Attempt(Outcome.NoLongerDue, stateNow);
        }

        campaign.Close(utcNow);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // The campaign's xmin token refused the save: its row was written after the read above. By a close or a
            // withdrawal, or by a write that leaves it open, such as a resend of its links; the caller reads it again.
            return new Attempt(Outcome.Changed, null);
        }

        return new Attempt(Outcome.Closed, MsfCampaignState.UnderReview);
    }

    private enum Outcome
    {
        Closed,
        NoLongerDue,
        Changed
    }

    private readonly record struct Attempt(Outcome Outcome, MsfCampaignState? StateNow);
}
