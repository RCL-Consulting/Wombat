using FluentAssertions;
using Wombat.Web.Components.Shared;

namespace Wombat.Web.Tests.Shared;

/// <summary>
/// T400 (flow 18, T18, R3-H05, R3-R09; build-contracts § 0): the scheduled jobs' addresses, which Home's "Its runs" builds
/// and Run history reads by the same parameter names.
/// </summary>
public sealed class JobLinksTests
{
    [Fact]
    public void ThePages_AreAtTheirRoutes()
    {
        JobLinks.Jobs.Should().Be("/admin/jobs");
        JobLinks.Runs.Should().Be("/admin/jobs/runs");
        JobLinks.JobParameter.Should().Be("job");
        JobLinks.StatusParameter.Should().Be("status");
    }

    [Fact]
    public void ItsRuns_OpensRunHistoryOnTheJobsFailedRuns()
        => JobLinks.RunsOf("entrustment-decision-expiry", "Failed")
            .Should().Be("/admin/jobs/runs?job=entrustment-decision-expiry&status=Failed");

    [Theory]
    [InlineData("audit-log-retention", null, "/admin/jobs/runs?job=audit-log-retention")]
    [InlineData(null, "Running", "/admin/jobs/runs?status=Running")]
    [InlineData("", "", "/admin/jobs/runs")]
    [InlineData(null, null, "/admin/jobs/runs")]
    public void APartNotGiven_IsLeftOut(string? jobKey, string? status, string expected)
        => JobLinks.RunsOf(jobKey, status).Should().Be(expected);

    [Fact]
    public void EachValue_IsEscaped()
        => JobLinks.RunsOf("a job&key=1", "Failed")
            .Should().Be("/admin/jobs/runs?job=a%20job%26key%3D1&status=Failed");

    [Theory]
    [InlineData("Succeeded")]
    [InlineData("Failed")]
    [InlineData("Running")]
    public void AStatus_IsTheRunStatussName(string status)
    {
        Enum.GetNames<Wombat.Domain.Scheduling.ScheduledJobRunStatus>().Should().Contain(status);
        JobLinks.RunsOf(null, status).Should().EndWith("?status=" + status);
    }
}
