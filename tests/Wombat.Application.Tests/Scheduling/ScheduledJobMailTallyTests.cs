using FluentAssertions;
using Microsoft.Extensions.Logging;
using Wombat.Application.Common.Email;
using Wombat.Application.Scheduling;
using Wombat.Infrastructure.Scheduling;

namespace Wombat.Application.Tests.Scheduling;

/// <summary>
/// T283: a nudge's or a digest's run logs, once the mail worker has reported on every mail it handed over, how many were
/// sent and how many were not delivered.
/// </summary>
/// <remarks>
/// Until T283 a job's one line said whom it wrote to, and a mail the worker gave up on was dropped with a line of its own
/// that named no run. The jobs themselves are held to handing their mail over keyed (the job tests' "…Keyed…" cases).
/// </remarks>
public sealed class ScheduledJobMailTallyTests
{
    private static readonly DateTime StartedAt = new(2029, 3, 5, 7, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ARunsLine_IsLoggedOnceEveryMailIsReported_CountingThoseNotDelivered()
    {
        var tally = new ScheduledJobMailTally();
        var logger = new CapturingLogger();
        var run = tally.Start("ActivityDraftNudgeJob", Context(logger));
        var mails = HandOver(run, 3);
        run.Close();

        await Report(tally, mails[0], sent: true);
        await Report(tally, mails[1], sent: false);
        logger.Entries.Should().BeEmpty("one mail has not been reported yet");

        await Report(tally, mails[2], sent: true);

        var line = logger.Entries.Should().ContainSingle().Which;
        line.Level.Should().Be(LogLevel.Warning, "a mail was not delivered");
        line.Values["JobName"].Should().Be("ActivityDraftNudgeJob");
        line.Values["RunStartedAt"].Should().Be("2029-03-05 07:00:00Z");
        line.Count("SentCount").Should().Be(2);
        line.Count("NotDeliveredCount").Should().Be(1);
    }

    [Fact]
    public async Task ReportsThatArriveBeforeTheRunCloses_AreCounted_AndTheLineComesAtTheClose()
    {
        // A fast mail server answers while the job is still handing mail over.
        var tally = new ScheduledJobMailTally();
        var logger = new CapturingLogger();
        var run = tally.Start("WeeklyCoordinatorDigestJob", Context(logger));
        var mails = HandOver(run, 2);

        await Report(tally, mails[0], sent: true);
        await Report(tally, mails[1], sent: true);
        logger.Entries.Should().BeEmpty("the run may hand over more");

        run.Close();

        var line = logger.Entries.Should().ContainSingle().Which;
        line.Level.Should().Be(LogLevel.Information, "every mail was sent");
        line.Count("SentCount").Should().Be(2);
        line.Count("NotDeliveredCount").Should().Be(0);
    }

    [Fact]
    public async Task ALineIsLoggedOnce_HoweverManyTimesTheRunIsClosed_OrAMailReportedAgain()
    {
        var tally = new ScheduledJobMailTally();
        var logger = new CapturingLogger();
        var run = tally.Start("ActivityDraftNudgeJob", Context(logger));
        var mail = HandOver(run, 1).Single();
        run.Close();
        await Report(tally, mail, sent: false);

        run.Close();
        await Report(tally, mail, sent: false);

        logger.Entries.Should().ContainSingle();
    }

    [Fact]
    public void ARunThatHandedOverNothing_LogsNothingOfItsOwn()
    {
        var tally = new ScheduledJobMailTally();
        var logger = new CapturingLogger();

        tally.Start("AssessorPendingNudgeJob", Context(logger)).Close();
        tally.Dispose();

        logger.Entries.Should().BeEmpty("the job's own line says it sent nothing");
    }

    [Fact]
    public async Task ARunNeverFullyReported_IsLoggedWhenTheSameJobNextRuns_WithHowManyHadNotCome()
    {
        // A host with no mail worker, or one that stopped with a mail in flight, reports nothing more of the run.
        var tally = new ScheduledJobMailTally();
        var logger = new CapturingLogger();
        var first = tally.Start("ActivityDraftNudgeJob", Context(logger));
        var mails = HandOver(first, 3);
        first.Close();
        await Report(tally, mails[0], sent: false);

        var other = tally.Start("AssessorPendingNudgeJob", Context(logger));
        HandOver(other, 1);
        other.Close();
        logger.Entries.Should().BeEmpty("another job's run says nothing of this one");

        tally.Start("ActivityDraftNudgeJob", Context(logger, StartedAt.AddDays(1))).Close();

        var line = logger.Entries.Should().ContainSingle().Which;
        line.Level.Should().Be(LogLevel.Warning);
        line.Values["JobName"].Should().Be("ActivityDraftNudgeJob");
        line.Values["RunStartedAt"].Should().Be("2029-03-05 07:00:00Z");
        line.Count("SentCount").Should().Be(0);
        line.Count("NotDeliveredCount").Should().Be(1);
        line.Count("UnreportedCount").Should().Be(2);
        line.Message.Should().EndWith(": sent 0, not delivered 1, not reported before the job ran again 2.",
            "a mail may still be being retried, so not \"never\" (T283 review)");

        // The earlier run is gone: a late report of it counts nowhere and logs nothing.
        await Report(tally, mails[1], sent: true);
        logger.Entries.Should().ContainSingle();
    }

    [Fact]
    public async Task AsTheHostStops_EveryRunStillWaiting_IsLoggedWithHowManyNeverCame()
    {
        var tally = new ScheduledJobMailTally();
        var logger = new CapturingLogger();
        var run = tally.Start("WeeklyCoordinatorDigestJob", Context(logger));
        var mails = HandOver(run, 2);
        run.Close();
        await Report(tally, mails[0], sent: true);

        tally.Dispose();

        var line = logger.Entries.Should().ContainSingle().Which;
        line.Count("SentCount").Should().Be(1);
        line.Count("NotDeliveredCount").Should().Be(0);
        line.Count("UnreportedCount").Should().Be(1);
        line.Message.Should().EndWith(": sent 1, not delivered 0, never reported 1.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("msf-link:1:AbC-_12xyzAbC-_1")]
    [InlineData("account-invitation:1:AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
    [InlineData("job-mail:no-such-run")]
    public async Task AReportOfAnyOtherMail_CountsNowhere(string? key)
    {
        var tally = new ScheduledJobMailTally();
        var logger = new CapturingLogger();
        var run = tally.Start("ActivityDraftNudgeJob", Context(logger));
        var mail = HandOver(run, 1).Single();
        run.Close();

        await tally.RecordAsync(mail with { DeliveryKey = key }, EmailDeliveryOutcome.Dropped(3, StartedAt), CancellationToken.None);
        logger.Entries.Should().BeEmpty();

        await Report(tally, mail, sent: true);
        logger.Entries.Should().ContainSingle().Which.Count("NotDeliveredCount").Should().Be(0);
    }

    [Fact]
    public void AKeyedMail_KeepsEverythingElseItCarries()
    {
        var tally = new ScheduledJobMailTally();
        var run = tally.Start("ActivityDraftNudgeJob", Context(new CapturingLogger()));
        var message = new EmailMessage("trainee@test.local", "Drafts", "<p/>", "T", Tags: ["draft-nudge"]);

        var keyed = run.Keyed(message);

        keyed.Should().BeEquivalentTo(message, options => options.Excluding(candidate => candidate.DeliveryKey));
        keyed.DeliveryKey.Should().StartWith("job-mail:");
        keyed.Tags.Should().NotContain(tag => tag.Contains(keyed.DeliveryKey!, StringComparison.Ordinal));
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static ScheduledJobContext Context(ILogger logger, DateTime? startedAt = null)
        => new(startedAt ?? StartedAt, logger);

    private static List<EmailMessage> HandOver(ScheduledJobMailRun run, int count)
    {
        var mails = new List<EmailMessage>();
        for (var index = 0; index < count; index++)
        {
            mails.Add(run.Keyed(new EmailMessage($"person-{index}@test.local", "Reminder", "<p/>", "T", Tags: ["nudge"])));
            run.HandedOver();
        }

        return mails;
    }

    private static Task Report(ScheduledJobMailTally tally, EmailMessage mail, bool sent)
        => tally.RecordAsync(
            mail,
            sent ? EmailDeliveryOutcome.Delivered(1, StartedAt) : EmailDeliveryOutcome.Dropped(3, StartedAt),
            CancellationToken.None);
}
